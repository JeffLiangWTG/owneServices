using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Testing;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(Callout))]
	sealed class CalloutBizoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var lookupsProperty = typeof(Callout).GetProperty("Lookups", typeof(CalloutLookups));
			AssertNotNull("base.Lookups has to be hidden and modified to have the return type CalloutLookups", lookupsProperty);
			AssertNotNull("Lookups object should not be null", lookupsProperty.GetValue(callout, null));
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			var declaration = Factory.New<JobDeclaration>();
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			declaration.JE_DeclarationReference = "DECREF";
			var billTo = Factory.NewWithValidTestData<OrgHeader>();
			var uPSAccountNumber = billTo.CustomsCodes.AddNew();
			uPSAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			uPSAccountNumber.OK_CustomsRegNo = "TEST";
			callout.BillToAccountNumber = "TEST";
			AssertEquals(2, callout.BusinessObjectsWithRelatedNotes.Length);
			AssertEquals("DECREF", ((JobDeclaration)callout.BusinessObjectsWithRelatedNotes[0]).JE_DeclarationReference);
			AssertEquals(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, ((OrgHeader)callout.BusinessObjectsWithRelatedNotes[1]).CustomsCodes[0].OK_CodeType);
		}

		public void TestGetInvoicingPreferencesNote()
		{
			OrgHeader billTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode uPSAccountNumber = billTo.CustomsCodes.AddNew();
			uPSAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			uPSAccountNumber.OK_CustomsRegNo = "TEST";
			callout.BillToAccountNumber = "TEST";
			callout.BillTo.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoicingPreferences.Description, "INVOICING NOTE");
			AssertEquals("INVOICING NOTE", callout.GetInvoicingPreferencesNote().ST_NoteDataAsText);
		}

		public void TestNoteTypeCollection()
		{
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.FinanceNote, callout.NoteTypes);
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.PartPaymentNote, callout.NoteTypes);
		}

		public void TestBillToPK()
		{
			AssertEquals("Should be ZGuid.Empty when BillTo organisation does not exist", ZGuid.Empty, callout.BillToPK);
			var org = AssignBillToOrganisation();
			AssertEquals(org.PK, callout.BillToPK);
		}

		public void TestBillTo()
		{
			AssertNull("Should be null when BillTo organisation does not exist", callout.BillTo);
			var org = AssignBillToOrganisation();
			AssertEquals(org.PK, callout.BillTo.PK);
			Assert("Should be read-only", callout.BillTo.ReadOnly);
		}

		public void TestBillTo_ShouldNotBeMatchedIfAccountNumberIsEmpty()
		{
			var org = Factory.New<UPEOrgHeader>();
			org.AccountNumber = "";
			callout.BillToAccountNumber = "";
			AssertNull("Should not be matching BillTo by Account number if account number is empty", callout.BillTo);
		}

		public void TestBillToAccountNumber_ShouldTruncateLongValues()
		{
			callout.BillToAccountNumber = new string('a', callout.BillToAccountNumberInfo.MaxLength + 5);
			AssertEquals("Value should be truncated to max lenth", new string('a', callout.BillToAccountNumberInfo.MaxLength), callout.BillToAccountNumber);
		}

		public void TestAssignOwnerCodeToImporterIfRequired_AccountNumberCanBeMatched()
		{
			CreateDeclarationAndImporterOrg();
			AssignBillToOrganisation();
			AssertEquals("Should still be empty, Bill To can be matched", "", callout.CusDecImporter.AccountNumber);
		}

		public void TestAssignOwnerCodeToImporterIfRequired_AccountNumberCannotBeMatchedButImporterAccountNumberIsNotEmpty()
		{
			CreateDeclarationAndImporterOrg();
			callout.BillToAccountNumber = "23948234";
			callout.CusDecImporter.AccountNumber = "999";
			AssertEquals("Bill To cannot be matched but CusDecImporter already has an account number", "999", callout.CusDecImporter.AccountNumber);
		}

		public void TestAssignOwnerCodeToImporterIfRequired_AccountNumberCannotBeMatched()
		{
			CreateDeclarationAndImporterOrg();
			callout.BillToAccountNumber = "23948234";
			AssertEquals("Should be assigned to the CusDecImporter", "23948234", callout.CusDecImporter.AccountNumber);
		}

		public void TestJobHeader()
		{
			AssertNull("Should not be created in the getter", callout.JobHeader);
			callout.EnsureJobHeaderExists();
			AssertNotNull("Should be created by EnsureJobHeaderExists method", callout.JobHeader);
			AssertEquals(typeof(CalloutJobHeader), callout.JobHeader.GetType());
		}

		public void TestAlertsList()
		{
			AssertEquals("PreCondition: should be no alerts", 0, callout.AlertsList.Count);
			callout.ResetAlertsList();
			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			importer.MiscServ.OM_IMEFTBankAccount = "1018096435";
			importer.MiscServ.OM_IMEFTBankBSB = "1296";
			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			declaration.JE_OH_Importer = importer.PK;
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals("1 alert", 1, callout.AlertsList.Count);
			Assert("Alert Message is incorrect", callout.AlertsList.Contains("Direct Debit is enabled for the Declaration on this Shipment."));
		}

		public void TestIsConsigneeDetailsReadOnlyUnlessUnmatched()
		{
			Assert(callout.CS_ConsigneeIdentifierInfo.ReadOnly);
			Assert(callout.CS_ConsigneeBusinessNumberInfo.ReadOnly);
		}

		public void TestIsConsignorDetailsReadOnlyUnlessUnmatched()
		{
			Assert(callout.CS_ConsignorIdentifierInfo.ReadOnly);
			Assert(callout.CS_VendorIdentifierInfo.ReadOnly);
		}

		public void TestNotReadOnlyWhenResponsePending()
		{
			callout.CS_IsResponsePending = false;
			AssertReadOnlyOnLoad("Not ReadOnly on load initially for the test", false);
			callout.CS_IsResponsePending = true;
			AssertReadOnlyOnLoad("Not ReadOnly on load even when CS_IsResponsePending=true, properties used in customs messages in the Callout sub-class are read-only anyways", false);
		}

		public void TestTotalAmountDue()
		{
			AssertEquals(ZDecimal.Zero, callout.TotalAmountDue);
			callout.CurrentQueue.P4_CustomDecimal4 = 14m;
			AssertEquals(14m, callout.TotalAmountDue);
			AssertEquals("InnerInfo should be " + callout.CurrentQueue.P4_CustomDecimal4Info.Name, callout.CurrentQueue.P4_CustomDecimal4Info, ((ZWrappedPropertyInfo)callout.TotalAmountDueInfo).InnerInfo);
		}

		public void TestCalculateTotalAmountDue()
		{
			callout.EnsureJobHeaderExists();
			var charge1 = callout.JobHeader.Charges.AddNew();
			charge1.NettAmount = 30m;
			charge1.TaxableAmount = 100m;
			var charge2 = callout.JobHeader.Charges.AddNew();
			charge2.NettAmount = 20m;
			charge2.TaxableAmount = 50m;
			var charge3 = callout.JobHeader.Charges.AddNew();
			charge3.NettAmount = 15m;
			var charge4 = callout.JobHeader.Charges.AddNew();
			charge4.TaxableAmount = 100m;
			AssertEquals("Should not be calculated in the getter", 0m, callout.TotalAmountDue);
			var taxFilter = new ZQuery(AccTaxRateSchema.AT_Code, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.Country.Code);
			var gSTTaxRate = Factory.LoadTop1<AccTaxRate>(taxFilter);

			var taxRate = gSTTaxRate.GetRate_ForTestOnly() / 100m;
			callout.CalculateTotalAmountDue();
			AssertEquals("Incorrect total amount due", 65m + (taxRate * 250m), callout.TotalAmountDue);
		}

		[ExpectNoExceptions]
		public void TestCalculateTotalAmountDue_ShouldNotBlowUpIfJobHeaderDoesNotExist()
		{
			AssertNull("Sanity check", callout.JobHeader);
			callout.CalculateTotalAmountDue();
		}

		public void TestPaymentCollectionType()
		{
			AssertEquals("", callout.PaymentCollectionType);
			callout.CurrentQueue.P4_CustomAttrib7 = "123";
			AssertEquals("123", callout.PaymentCollectionType);
			callout.PaymentCollectionType = "234";
			AssertEquals("234", callout.PaymentCollectionType);
			AssertEquals("234", callout.CurrentQueue.P4_CustomAttrib7);
			AssertEquals("InnerInfo should be " + callout.CurrentQueue.P4_CustomAttrib7Info.Name, callout.CurrentQueue.P4_CustomAttrib7Info, ((ZWrappedPropertyInfo)callout.PaymentCollectionTypeInfo).InnerInfo);
			Assert("Should be read-only", callout.PaymentCollectionTypeInfo.ReadOnly);
		}

		public void TestPaymentDate()
		{
			AssertEquals("PaymentDate should be empty initially for the test", true, callout.CurrentQueue.P4_CustomDate5.IsEmpty);
			callout.PaymentDate = ZDateTime.Now;
			AssertEquals("PaymentDate should go into CustomDate5", false, callout.CurrentQueue.P4_CustomDate5.IsEmpty);
		}

		public void TestCalloutPayment()
		{
			AssertNotNull("Should not be null", callout.Payment);
		}

		public void TestFinanceFreightChargeIncludingGST()
		{
			var taxFilter = new ZQuery(AccTaxRateSchema.AT_Code, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.Country.Code);
			var gstTaxRate = Factory.LoadTop1<AccTaxRate>(taxFilter);
			gstTaxRate.SetRateNumerator_ForTestOnly(10);
			AssertEquals("When there is no freight charge", 0m, callout.FinanceFreightChargeIncludingGST);
			callout.EnsureJobHeaderExists();
			var freightCharge = callout.JobHeader.Charges.AddNew();
			freightCharge.JR_Desc = ShipmentChargeDescription.Freight;
			AssertEquals("When there is no freight charge", 0m, callout.FinanceFreightChargeIncludingGST);
			freightCharge.TaxableAmount = 12m;
			AssertEquals("Freight charge + GST", 13.2m, callout.FinanceFreightChargeIncludingGST);
		}

		public void TestFinanceSecurityFeeIncludingGST()
		{
			var taxFilter = new ZQuery(AccTaxRateSchema.AT_Code, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.Country.Code);
			var gstTaxRate = Factory.LoadTop1<AccTaxRate>(taxFilter);
			gstTaxRate.SetRateNumerator_ForTestOnly(10);
			AssertEquals("When there is no security fee charge", 0m, callout.FinanceSecurityFeeIncludingGST);
			callout.EnsureJobHeaderExists();
			var securityFeeCharge = callout.JobHeader.Charges.AddNew();
			securityFeeCharge.JR_Desc = ShipmentChargeDescription.SecurityFee;
			AssertEquals("When there is no security fee charge", 0m, callout.FinanceSecurityFeeIncludingGST);
			securityFeeCharge.TaxableAmount = 10m;
			AssertEquals("Security fee + GST", 11m, callout.FinanceSecurityFeeIncludingGST);
		}

		public void TestFinanceTerminalFeeAmountIncludingGST()
		{
			var taxFilter = new ZQuery(AccTaxRateSchema.AT_Code, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.Country.Code);
			var gstTaxRate = Factory.LoadTop1<AccTaxRate>(taxFilter);
			gstTaxRate.SetRateNumerator_ForTestOnly(10);
			AssertEquals("When there is no ITF charges", 0m, callout.FinanceTerminalFeeAmountIncludingGST);
			callout.EnsureJobHeaderExists();
			var iTFCharges = callout.JobHeader.Charges.AddNew();
			iTFCharges.JR_Desc = ShipmentChargeDescription.ITFCharges;
			AssertEquals("When there is no ITF charges", 0m, callout.FinanceTerminalFeeAmountIncludingGST);
			iTFCharges.TaxableAmount = 10m;
			AssertEquals("ITF Charge + GST", 11m, callout.FinanceTerminalFeeAmountIncludingGST);
		}

		public void TestConsigneeConsignorDetailsReadOnly()
		{
			var billTo = BillTo;
			callout.BillToAccountNumber = "BillToAccountNum";
			AssertEquals("BillTo should match for the test", billTo.PK, callout.BillTo.PK);
			AssertEquals("BillTo details should be readonly", true, callout.BillToPKInfo.ReadOnly);
			AssertEquals("BillTo details should be readonly", true, callout.BillTo.ReadOnly);
			AssertEquals("BillTo details should be readonly", true, callout.BillTo.MainAddress.OA_PhoneInfo.ReadOnly);
		}

		public void TestCusDecImporterReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertNotNull("Declaration should be linked to the callout for the test", callout.Declaration);
			AssertEquals("Consignee details should be readonly", true, callout.CusDecImporter.ReadOnly);
		}

		public void TestBillToDetailsReadOnly()
		{
			var cleanFactoryToForceOnLoaded = new BusinessObjectFactory();
			callout = cleanFactoryToForceOnLoaded.Load<CalloutForTest>(callout.PK);
			AssertEquals("Consignee details should be readonly", true, callout.CS_OH_ConsigneeInfo.ReadOnly);
		}

		public void TestDeliveryAddressRedirectionReadOnly()
		{
			var cleanFactoryToForceOnLoaded = new BusinessObjectFactory();
			callout = cleanFactoryToForceOnLoaded.Load<CalloutForTest>(callout.PK);
			AssertEquals("Callout should not be redirected initially for the test", false, callout.IsRedirected);
			AssertEquals("IsRedirected property must not be read only", false, callout.IsRedirectedInfo.ReadOnly);
			callout.IsRedirected = true;
			AssertEquals("IsRedirected property must not be read only", false, callout.IsRedirectedInfo.ReadOnly);
			AssertEquals("DeliveryAddressOverride must not be read only when redirected", false, callout.DeliveryAddressOverride.ReadOnly);
		}

		public void TestPartShipsReadOnly()
		{
			AssertEquals("Part Shipments should be read only", true, callout.PartShips.ReadOnly);
		}

		public void TestChildRelatedWayBillsReadOnly()
		{
			AssertEquals("Part Shipments should be read only", true, callout.ChildRelatedWayBills.ReadOnly);
		}

		public void TestBisiDownloadDate()
		{
			AssertEquals(ZDateTime.Empty, callout.BisiDownloadDate);
			var expectedDate = new ZDateTime(2005, 12, 12);
			callout.CurrentQueue.P4_CustomDate2 = expectedDate;
			AssertEquals(expectedDate, callout.BisiDownloadDate);
			expectedDate = new ZDateTime(2004, 10, 10);
			callout.BisiDownloadDate = expectedDate;
			AssertEquals(expectedDate, callout.BisiDownloadDate);
			AssertEquals(expectedDate, callout.CurrentQueue.P4_CustomDate2);
			AssertEquals("InnerInfo should be " + callout.CurrentQueue.P4_CustomDate2Info.Name, callout.CurrentQueue.P4_CustomDate2Info, ((ZWrappedPropertyInfo)callout.BisiDownloadDateInfo).InnerInfo);
			Assert("Should be read-only", callout.BisiDownloadDateInfo.ReadOnly);
		}

		[TestDate(2008, 12, 12, 12, 12, 12)]
		public void TestMoveToChaseQueueOnBISIDownload()
		{
			var timeValue = new ChaseQueueValidationCollection();
			var validation = timeValue.AddNew();
			validation.DayOfTheWeek = Enum.GetName(typeof(DayOfWeek), ZDateTime.Now.DayOfWeek).ToUpper();
			validation.TimeFrom = ZDateTime.Now.AddMinutes(-10);
			UPEDataRegistry.Instance.ChaseQueueValidationRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, timeValue);
			var serviceLevelValue = new ServiceLevelRegistryBusinessObjectCollection();
			var serviceLevel = serviceLevelValue.AddNew();
			serviceLevel.ServiceLevel = "DEF";
			UPEDataRegistry.Instance.ChaseQueueValidationServiceLevelRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceLevelValue);
			var calloutWithChargesData = Factory.NewWithValidTestData<CalloutForTestWithChargesData>();
			calloutWithChargesData.CS_RS_NK_ServiceLevel = "DEF";
			calloutWithChargesData.SetTotalLocalCharges(800);
			((IBisiDownload)calloutWithChargesData).OnAfterBisiDownload();
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Chase, calloutWithChargesData.CurrentQueue.P4_QueueName);
		}

		public void TestIsExcludedFromBISIWarning()
		{
			AssertEquals("Field should not be readonly", false, callout.IsExcludedFromBISIWarningInfo.ReadOnly);
			AssertEquals("IsExcludedFromBISIWarning should be false by default", false, callout.IsExcludedFromBISIWarning);
			callout.IsExcludedFromBISIWarning = true;
			AssertEquals("Setting IsExcludedFromBISIWarning = true", true, callout.IsExcludedFromBISIWarning);
			callout.IsExcludedFromBISIWarning = false;
			AssertEquals("Setting IsExcludedFromBISIWarning = false", false, callout.IsExcludedFromBISIWarning);
		}

		public void TestBISIDownloadNotRequiredOrForcedCaption_NotRequired()
		{
			IShipmentData shipmentData = callout;
			callout.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			AssertEquals("Should be downloaded for the test", true, shipmentData.ShouldBeDownloaded);
			AssertEquals("Caption should be empty initially", "", callout.BISIDownloadNotRequiredOrForcedCaption);
			callout.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			AssertEquals("Should not be downloaded for the test", false, shipmentData.ShouldBeDownloaded);
			AssertEquals("Should not be required for free domicile", "NOT REQUIRED", callout.BISIDownloadNotRequiredOrForcedCaption);
			AssertEquals("The caption should be read-only", true, callout.BISIDownloadNotRequiredOrForcedCaptionInfo.ReadOnly);
		}

		public void TestBISIDownloadNotRequiredOrForcedCaption_ForcedToFinance()
		{
			IShipmentData shipmentData = callout;
			callout.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			AssertEquals("Should be downloaded for the test", true, shipmentData.ShouldBeDownloaded);
			callout.CurrentQueue.P4_Reason = CalloutForTest.ForcedToFinanceQueueRemarks;
			Factory.Save();
			// reload to uncache the property value
			callout = new BusinessObjectFactory().Load<CalloutForTest>(callout.PK);
			AssertEquals("Forced when a 'forced to finance' reason is given", "FORCED", callout.BISIDownloadNotRequiredOrForcedCaption);
		}

		public void TestMoveCommercialQueueOnBISIDownload_MoveToAlternateBrokerHold()
		{
			callout.CurrentQueue.P4_QueueName = ZString.Empty;
			callout.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			callout.Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			callout.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("Callout should not be in a queue initially for the test", "", callout.CurrentQueue.P4_QueueName);
			((IBisiDownload)callout).OnAfterBisiDownload();
			AssertEquals("Callout should be moved into the alternate broker hold after BISI download", CommercialQueueCodeDescriptionPairList.Codes.AlternateBroker, callout.CurrentQueue.P4_QueueName);
		}

		public void TestMoveCommercialQueueOnBISIDownload_MoveToCompletedIfAlternateBrokerAndCargoReportCompleted()
		{
			callout.CurrentQueue.P4_QueueName = ZString.Empty;
			callout.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			callout.Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			callout.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("Callout should not be in a queue initially for the test", "", callout.CurrentQueue.P4_QueueName);
			callout.CurrentQueue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Completed;
			((IBisiDownload)callout).OnAfterBisiDownload();
			AssertEquals("Callout should be completed after BISI download", CustomsQueueCodeDescriptionPairList.Codes.Completed, callout.CurrentQueue.P4_QueueName);
		}

		public void TestEditLogReferenceWhenIsExcludedFromBISIWarningChanges_One()
		{
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event not raised initially", false);
			callout.OnSaving();
			Factory.Save();
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event not raised initially after first save and no changes", false);
		}

		public void TestEditLogReferenceWhenIsExcludedFromBISIWarningChanges_Two()
		{
			callout.IsExcludedFromBISIWarning = false;
			Factory.Save();
			callout.IsExcludedFromBISIWarning = true;
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event not raised until save", false);
			callout.OnSaving();
			Factory.Save();
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event raised for false->true after save", true);
		}

		public void TestEditLogReferenceWhenIsExcludedFromBISIWarningChanges_Three()
		{
			callout.IsExcludedFromBISIWarning = true;
			Factory.Save();
			callout.IsExcludedFromBISIWarning = true;
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event not raised until save", false);
			callout.OnSaving();
			Factory.Save();
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event not raised for true->true after save", false);
		}

		public void TestEditLogReferenceWhenIsExcludedFromBISIWarningChanges_Four()
		{
			callout.IsExcludedFromBISIWarning = true;
			Factory.Save();
			callout.IsExcludedFromBISIWarning = false;
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event not raised until save", false);
			callout.OnSaving();
			Factory.Save();
			AssertEditLogReferenceForIsExcludedFromBISIWarning("Event raised for true->false after save", true);
		}

		public void TestTaxInvoiceAndCommercialInvoice()
		{
			var org = Factory.NewWithValidTestData<UPEOrgHeader>();
			org.ShouldReceiveCommercialInvoice = true;
			org.AccountNumber = "A23546";
			callout.BillToAccountNumber = org.AccountNumber;
			AssertEquals("Commercial invoice is available, should send commercial invoice", typeof(UPETaxInvoiceAndCommercialInvoiceAutoDelivery), callout.GetTaxInvoiceAutoDelivery().GetType());
			var jobDec = Factory.New<UPEJobDeclaration>();
			callout.CS_JE_CustomsFormalEntry = jobDec.PK;
			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var parent = docFactory.New<StorageMain>();
			parent.SM_ParentFK = jobDec.PK;
			var document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
			jobDec.DocManagerInfo.Documents.Add(document);
			jobDec.DocManagerInfo.Save();

			org = Factory.NewWithValidTestData<UPEOrgHeader>();
			org.ShouldReceiveCommercialInvoice = false;
			callout.BillToAccountNumber = org.AccountNumber;
			AssertEquals("Should Receive Commercial Invoice flag is unticked, commercial invoice should be excluded", typeof(UPETaxInvoiceAutoDelivery), callout.GetTaxInvoiceAutoDelivery().GetType());
			org.AccountNumber = "54F6D4F64D";
			callout.BillToAccountNumber = org.AccountNumber;
			org.ShouldReceiveCommercialInvoice = true;
			AssertEquals("account number does not start with A or E", typeof(UPETaxInvoiceAutoDelivery), callout.GetTaxInvoiceAutoDelivery().GetType());
			org.AccountNumber = "A4F6D4F64D";
			callout.BillToAccountNumber = org.AccountNumber;
			AssertEquals("account number starts with A, Should Receive Commercial Invoice flag is ticked and Registry is true", typeof(UPETaxInvoiceAndCommercialInvoiceAutoDelivery), callout.GetTaxInvoiceAutoDelivery().GetType());
			org.AccountNumber = "E4F6D4F64D";
			callout.BillToAccountNumber = org.AccountNumber;
			AssertEquals("account number starts with E", typeof(UPETaxInvoiceAndCommercialInvoiceAutoDelivery), callout.GetTaxInvoiceAutoDelivery().GetType());
		}

		public void TestTaxInvoiceAutoDeliveredOnBISIDownload()
		{
			var callout = Factory.NewWithValidTestData<TestCalloutForDocumentAutoDelivery>();
			AssertEquals("Invoice should not be auto-delivered yet because BISI download hasn't happened", false, callout.TaxInvoiceAutoDelivery.WasDelivered);
			Factory.Save();
			AssertEquals("Invoice should not be auto-delivered yet because BISI download hasn't happened", false, callout.TaxInvoiceAutoDelivery.WasDelivered);
			callout.BisiDownloadDate = ZDateTime.Now;
			AssertEquals("Invoice should not be auto-delivered yet", false, callout.TaxInvoiceAutoDelivery.WasDelivered);
			Factory.Save();
			AssertEquals("Invoice should have been auto-delivered on save", true, callout.TaxInvoiceAutoDelivery.WasDelivered);
		}

		public void TestTaxInvoiceNotAutoDeliveredOnBISIDownload_WhenZeroChargesAndCertainThirdPartyIndicators1()
		{
			var callout = Factory.NewWithValidTestData<TestCalloutForDocumentAutoDelivery>();
			callout.Level1Record = new Level1Record();
			callout.Level1Record.AddRecordLine("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           D 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN1NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Factory.Save();
			callout.BisiDownloadDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Invoice should not have been auto-delivered because third party indicator invalid", false, callout.TaxInvoiceAutoDelivery.WasDelivered);
		}

		public void TestTaxInvoiceNotAutoDeliveredOnBISIDownload_WhenZeroChargesAndCertainThirdPartyIndicators2()
		{
			var callout = Factory.NewWithValidTestData<TestCalloutForDocumentAutoDelivery>();
			callout.Level1Record = new Level1Record();
			callout.Level1Record.AddRecordLine("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           D 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN2NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Factory.Save();
			callout.BisiDownloadDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Invoice should not have been auto-delivered because third party indicator invalid", false, callout.TaxInvoiceAutoDelivery.WasDelivered);
		}

		public void TestTaxInvoiceNotAutoDeliveredOnBISIDownload_WhenZeroChargesAndCertainThirdPartyIndicators4()
		{
			var callout = Factory.NewWithValidTestData<TestCalloutForDocumentAutoDelivery>();
			callout.Level1Record = new Level1Record();
			callout.Level1Record.AddRecordLine("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           D 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN4NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Factory.Save();
			callout.BisiDownloadDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Invoice should not have been auto-delivered because third party indicator invalid", false, callout.TaxInvoiceAutoDelivery.WasDelivered);
		}

		public void TestTaxInvoiceNotAutoDeliveredOnBISIDownload_WhenZeroChargesAndCertainThirdPartyIndicators7()
		{
			var callout = Factory.NewWithValidTestData<TestCalloutForDocumentAutoDelivery>();
			callout.Level1Record = new Level1Record();
			callout.Level1Record.AddRecordLine("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           D 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN7NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Factory.Save();
			callout.BisiDownloadDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Invoice should not have been auto-delivered because third party indicator invalid", false, callout.TaxInvoiceAutoDelivery.WasDelivered);
		}

		public void TestTaxInvoiceAutoDeliveredOnBISIDownload_WhenNonZeroChargesAndCertainThirdPartyIndicators7()
		{
			var callout = Factory.NewWithValidTestData<TestCalloutForDocumentAutoDelivery>();
			callout.CurrentQueue.P4_CustomDecimal4 = 100m;
			callout.Level1Record = new Level1Record();
			callout.Level1Record.AddRecordLine("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           D 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN7NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Factory.Save();
			callout.BisiDownloadDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Invoice should have been auto-delivered because it has charges", true, callout.TaxInvoiceAutoDelivery.WasDelivered);
		}

		public void TestAlternateBrokerDocumentPackAutoDelivered()
		{
			TestAlternateBrokerDocumentPackAutoDelivered(WithAlternateBroker, !WithITFChargable, !WithFreightCharges, !ExpectDocumentDelivery);
			TestAlternateBrokerDocumentPackAutoDelivered(WithAlternateBroker, WithITFChargable, !WithFreightCharges, ExpectDocumentDelivery);
			TestAlternateBrokerDocumentPackAutoDelivered(WithAlternateBroker, !WithITFChargable, WithFreightCharges, ExpectDocumentDelivery);
			TestAlternateBrokerDocumentPackAutoDelivered(WithAlternateBroker, WithITFChargable, WithFreightCharges, ExpectDocumentDelivery);
			TestAlternateBrokerDocumentPackAutoDelivered(!WithAlternateBroker, WithITFChargable, WithFreightCharges, !ExpectDocumentDelivery);
		}

		[TestDate(2006, 1, 1)]
		public void TestCalloutPartPayment_BRK()
		{
			CreateStaffWithEmailGroupAndSetRegistry();
			SetPartPaymentOrRefundControlNumberFountainValue(Factory, 22);
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData<UPEJobDeclaration>().PK;
			callout.CalloutPartPayment.AmountToCollect = 22.44m;
			AssertNotNull(callout.CalloutPartPayment);
			AssertEquals(false, callout.CalloutPartPayment.MustAddNote);
			Factory.Save();
			AssertEquals(0, callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description).Length);
			AssertEquals(22, UPENumberFountains.Instance.PartPaymentControlNumberFountain.PeekPreliminary(Factory));
			AssertEquals(0m, callout.PartPaymentAmountToCollect);
			AssertEquals(false, callout.PartPaymentUsed);
			AssertEquals(false, callout.Declaration.IsRefundEnquiry);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			callout.CalloutPartPayment.MustAddNote = true;
			callout.CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.BRK;
			callout.CalloutPartPayment.AmountToCollect = 22.44m;
			callout.CalloutPartPayment.Remarks = "TEST";
			var expectedNoteText = "Control Number    : 060022\r\n" + callout.CalloutPartPayment.NoteText;
			Factory.Save();
			AssertEquals(1, callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description).Length);
			AssertEquals(22.44m, callout.PartPaymentAmountToCollect);
			AssertEquals(expectedNoteText, callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description)[0].ST_NoteText);
			AssertEquals(23, UPENumberFountains.Instance.PartPaymentControlNumberFountain.PeekPreliminary(Factory));
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestCalloutPartPayment_CODAndPartPaymentUsed()
		{
			CreateStaffWithEmailGroupAndSetRegistry();
			SetPartPaymentOrRefundControlNumberFountainValue(Factory, 22);
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData<UPEJobDeclaration>().PK;
			callout.CalloutPartPayment.MustAddNote = true;
			callout.CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.COD;
			callout.CalloutPartPayment.AmountToCollect = 0m;
			callout.CalloutPartPayment.Remarks = "TEST";
			var expectedNoteText = callout.CalloutPartPayment.NoteText;
			Factory.Save();
			AssertEquals(1, callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description).Length);
			AssertEquals(expectedNoteText, callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description)[0].ST_NoteText);
			AssertEquals(0m, callout.PartPaymentAmountToCollect);
			AssertEquals(true, callout.PartPaymentUsed);
			AssertEquals(22, UPENumberFountains.Instance.PartPaymentControlNumberFountain.PeekPreliminary(Factory));
			AssertEquals(false, callout.Declaration.IsRefundEnquiry);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestCalloutPartPayment_IfSaveHappensTwice_OnlyOneNoteCreated()
		{
			CreateStaffWithEmailGroupAndSetRegistry();
			SetPartPaymentOrRefundControlNumberFountainValue(Factory, 22);
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData<UPEJobDeclaration>().PK;
			callout.CalloutPartPayment.MustAddNote = true;
			callout.CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.COD;
			callout.CalloutPartPayment.AmountToCollect = 0m;
			callout.CalloutPartPayment.Remarks = "TEST";
			Factory.Save();
			callout.CS_HAWB = "TEST";
			Factory.Save();
			AssertEquals(1, callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description).Length);
		}

		public void TestRefund()
		{
			AssertNull("Callout doesn't have a Refund", callout.Refund);
			AssertNull("RelatedOwner", callout.RelatedOwner);
			callout.Notes.RemoveAndDeleteAll();
			UPEJobDeclaration declaration = AddDeclaration(callout);
			IRefundEnquiry owner = callout;
			owner.RefundManager.CreateClientRefund();
			AssertNotNull("Callout refer to the Declaration Refund", owner.Refund);
			AssertEquals("OwnerColumn", ClientRefundSchema.T10_CS, owner.OwnerColumn);
			AssertNotNull("RelatedOwner", owner.RelatedOwner);
			Assert("RelatedOwner is Declaration", ReferenceEquals(declaration, owner.RelatedOwner));
			Assert("Doesn't have Notes", !callout.Notes.HasNotes);
			owner.AddRefundNote();
			Assert("Has Notes", callout.Notes.HasNotes);
			AssertEquals("Has 1 FinanceNote", 1, callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.FinanceNote.Description).Length);
			callout.ResetRefundMissingTest();
			callout.EnableMissingRefundLink = true;
			AssertNotNull("ControlNumber found via notes", owner.Refund);
			callout.EnableMissingRefundLink = false;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			owner.SendNotificationEmail();
			AssertEquals("Doesn't send emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert("owner.RelatedOwner.IsRefundEnquiry", !owner.RelatedOwner.IsRefundEnquiry);
			Assert("owner.RelatedOwner.IsRefundProcessed", !owner.RelatedOwner.IsRefundProcessed);
			Assert("owner.IsRefundEnquiry", !owner.IsRefundEnquiry);
			Assert("owner.IsRefundProcessed", !owner.IsRefundProcessed);
			owner.RelatedOwner.IsRefundEnquiry = true;
			owner.RelatedOwner.IsRefundProcessed = true;
			Assert("owner.RelatedOwner.IsRefundEnquiry", owner.RelatedOwner.IsRefundEnquiry);
			Assert("owner.RelatedOwner.IsRefundProcessed", owner.RelatedOwner.IsRefundProcessed);
			AssertEquals(owner.IsRefundEnquiry, owner.RelatedOwner.IsRefundEnquiry);
			AssertEquals(owner.IsRefundProcessed, owner.RelatedOwner.IsRefundProcessed);
		}

		public void TestPartPaymentAmountToCollect()
		{
			callout.PartPaymentAmountToCollect = 12.43m;
			AssertEquals(12.43m, callout.PartPaymentAmountToCollect);
		}

		public void TestBusinessObjectType()
		{
			AssertEquals("UPECallout", callout.BusinessObjectType);
		}

		public void TestIsInReBillQueue()
		{
			AssertEquals(false, callout.IsInReBillQueue);
			callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			AssertEquals(true, callout.IsInReBillQueue);
			callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertEquals(false, callout.IsInReBillQueue);
		}

		public void TestMultipleBizOsAroundOneRowSet()
		{
			ErrorReporter.Clear();
			var factory2 = new BusinessObjectFactory();
			factory2.AllowMultipleBusinessObjectsAroundOneRow = false;
			var calloutOnFactory2 = factory2.Load<Callout>(callout.PK);
			var child = calloutOnFactory2.MAWB.ChildBills[0];
			AssertEquals(child.GetType().Name, "Callout");
			ErrorReporter.Clear();
		}

		public void TestMissingControlNumer()
		{
			var owner = Factory.NewWithValidTestData<Callout>();
			JobDeclaration jobDec = AddDeclaration(owner);
			owner.RefundManager.CreateClientRefund();
			TestHelperForRefund.PopulateRefund(owner.Refund);
			Assert("Control Number exists.", !owner.RefundManager.Refund.T10_ControlNumber.IsEmpty);
			var noteText = ZString.Format(refundNoteFormat, "0223573", GlbStaff.CurrentUser.GS_FullName, ZDateTime.UtcNow.ToLongTimeString(), "TICKED");
			jobDec.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.RefundNote.Description, noteText);
			var refund = Factory.LoadTop1<ClientRefund>(new ZQuery(ClientRefundSchema.PK, owner.Refund.PK));
			refund.T10_ControlNumber = ZString.Empty;
			Factory.Save();
			Assert("Simulate missing Control Number", refund.T10_ControlNumber.IsEmpty);
			Assert("And now it magically reappears...", !owner.RefundManager.Refund.T10_ControlNumber.IsEmpty);
		}

		CalloutForTest callout;
		protected override BusinessObject GetNewBusinessObject()
		{
			var patternMatchAddress = Factory.New<OrgPatternMatchAddress>();
			patternMatchAddress.P3_ParentID = callout.PK;
			patternMatchAddress.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			return callout;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			var mAWB = Factory.New<CusMAWB>();
			callout = (CalloutForTest)mAWB.ChildBills.AddNew(typeof(CalloutForTest));
			callout.CurrentQueue.P4_QueueName = "AMP";
			Factory.Save();
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.UPECusHAWB);

		UPEJobDeclarationForTest AddDeclaration(Callout callout)
		{
			var declaration = Factory.NewWithValidTestData<UPEJobDeclarationForTest>();
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			Factory.Save();
			return declaration;
		}

		OrgHeader BillTo
		{
			get
			{
				if (fBillTo == null)
				{
					fBillTo = Factory.NewWithValidTestData<OrgHeader>();
					OrgCusCode accountNumber = fBillTo.CustomsCodes.AddNew();
					accountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
					accountNumber.OK_CustomsRegNo = "BillToAccountNum";
				}

				return fBillTo;
			}
		}

		OrgHeader fBillTo;
		void SetPartPaymentOrRefundControlNumberFountainValue(BusinessObjectFactory factory, int value)
		{
			Db.Connection.BeginTransaction();
			try
			{
				UPENumberFountains.Instance.PartPaymentControlNumberFountain.SetNext(factory, value);
			}
			finally
			{
				Db.Connection.CommitTransaction();
			}
		}

		void CreateDeclarationAndImporterOrg()
		{
			callout.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			callout.Declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
		}

		OrgHeader AssignBillToOrganisation()
		{
			var billTo = Factory.New<OrgHeader>();
			var cusCode = billTo.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "X123";
			cusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			callout.BillToAccountNumber = "X123";
			return billTo;
		}

		void AssertReadOnlyOnLoad(string message, bool expectedReadOnly)
		{
			Factory.Save();
			var loadedCallout = new BusinessObjectFactory().Load<Callout>(callout.PK);
			AssertEquals(message, expectedReadOnly, loadedCallout.ReadOnly);
		}

		void AssertEditLogReferenceForIsExcludedFromBISIWarning(string message, bool expectEvent)
		{
			var lastEditLog = callout.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			AssertEquals(message, expectEvent, lastEditLog != null && lastEditLog.SL_Reference.IndexOf("BISI Warning") != -1);
		}

		void TestAlternateBrokerDocumentPackAutoDelivered(bool withAlternateBroker, bool withITFChargable, bool withFreightCharges, bool expectDocumentDelivery)
		{
			var callout = TestCalloutForDocumentAutoDelivery.NewWithDeclarationAndImporter(Factory);
			AssertEquals("Documents not delivered initially", false, callout.AlternateBrokerAutoDelivery.WasDelivered);
			if (withAlternateBroker)
			{
				callout.Declaration.Importer.SetRelatedParty(Factory.NewWithValidTestData<UPEOrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			}

			callout.Declaration.Importer.IsITFChargableForThisImporter = withITFChargable;
			Factory.Save();
			AssertEquals("Documents not delivered until download complete and no ITF/freight charges", false, callout.AlternateBrokerAutoDelivery.WasDelivered);
			if (withFreightCharges)
			{
				callout.SetupFreightCharge();
			}

			Factory.Save();
			AssertEquals("Documents not delivered until download complete and no ITF/freight charges", false, callout.AlternateBrokerAutoDelivery.WasDelivered);
			callout.BisiDownloadDate = ZDateTime.Now;
			Factory.Save();
			if (expectDocumentDelivery)
			{
				AssertEquals("Documents delivered after download and ITF/freight charges exists", true, callout.AlternateBrokerAutoDelivery.WasDelivered);
			}
			else
			{
				AssertEquals("Documents not delivered after download if no ITF/freight charges exist", false, callout.AlternateBrokerAutoDelivery.WasDelivered);
			}
		}

		void CreateStaffWithEmailGroupAndSetRegistry()
		{
			var glbGroup = Factory.New<GlbGroup>();
			glbGroup.GG_Code = "XXX";
			UPEDataRegistry.Instance.PartPaymentNotificationGroup = glbGroup.PK.ToGuid();
			var glbStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			glbStaff.Groups.Add(glbGroup);
			glbStaff.GS_EmailAddress = "test@edi.com.au";
			Factory.Save();
		}

		const bool WithAlternateBroker = true;
		const bool WithITFChargable = true;
		const bool WithFreightCharges = true;
		const bool ExpectDocumentDelivery = true;
		const string refundNoteFormat = "CONTROL NUMBER        : {0}\r\nUSER NAME             : {1}\r\nDATE                  : {2}\r\nREMARKS               : {3}";

		sealed class CalloutForTest : Callout, IShipmentData
		{
			public CalloutForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal void ResetRefundMissingTest()
			{
				base.refund = null;
			}

			// Simulate refund link missing...
			internal bool EnableMissingRefundLink { get; set; }

			public override UPEJobDeclaration Declaration
			{
				get
				{
					return EnableMissingRefundLink ? (UPEJobDeclarationForTest)base.Declaration : base.Declaration;
				}
			}
		}

		sealed class CalloutForTestWithChargesData : Callout, IShipmentData
		{
			public CalloutForTestWithChargesData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void SetTotalLocalCharges(ZDecimal totalLocalCharges)
			{
				chargesData = new ShipmentChargeData[] { new ShipmentChargeData(ShipmentChargeTypeCode.GST, totalLocalCharges, Core.Constants.CurrencyCodes.Australia) };
			}

			IReadOnlyList<ShipmentChargeData> IShipmentData.ChargesData => chargesData;

			ShipmentChargeData[] chargesData;
		}

		sealed class UPEJobDeclarationForTest : UPEJobDeclaration
		{
			public UPEJobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ClientRefund Refund => null;
		}

		sealed class TestCalloutForDocumentAutoDelivery : Callout
		{
			public TestCalloutForDocumentAutoDelivery(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal static TestCalloutForDocumentAutoDelivery NewWithDeclarationAndImporter(BusinessObjectFactory factory)
			{
				var result = factory.NewWithValidTestData<TestCalloutForDocumentAutoDelivery>();
				result.CS_JE_CustomsFormalEntry = factory.NewWithValidTestData(typeof(UPEJobDeclaration)).PK;
				result.Declaration.JE_OH_Importer = factory.NewWithValidTestData(typeof(UPEOrgHeader)).PK;
				return result;
			}

			internal void SetupFreightCharge()
			{
				EnsureJobHeaderExists();
				var freightCharge = JobHeader.Charges.AddNew();
				freightCharge.JR_Desc = ShipmentChargeDescription.Freight;
				freightCharge.TaxableAmount = 9m;
			}

			TestUPETaxInvoiceAutoDelivery taxInvoiceAutoDelivery;
			internal TestUPETaxInvoiceAutoDelivery TaxInvoiceAutoDelivery => taxInvoiceAutoDelivery ?? (taxInvoiceAutoDelivery = new TestUPETaxInvoiceAutoDelivery(this));

			internal override UPETaxInvoiceAutoDelivery GetTaxInvoiceAutoDelivery() => TaxInvoiceAutoDelivery;

			TestUPEAlternateBrokerDocumentPackAutoDelivery alternateBrokerAutoDelivery;
			internal TestUPEAlternateBrokerDocumentPackAutoDelivery AlternateBrokerAutoDelivery => alternateBrokerAutoDelivery ?? (alternateBrokerAutoDelivery = new TestUPEAlternateBrokerDocumentPackAutoDelivery(Declaration));
			
			protected override UPEAlternateBrokerDocumentPackAutoDelivery GetAlternateBrokerDocumentPackAutoDelivery(UPEJobDeclaration declaration)
			{
				return AlternateBrokerAutoDelivery;
			}
		}

		sealed class TestUPETaxInvoiceAutoDelivery : UPETaxInvoiceAutoDelivery
		{
			public TestUPETaxInvoiceAutoDelivery(Callout callout) : base(callout)
			{
			}

			internal bool WasDelivered;
			public override void Deliver()
			{
				WasDelivered = true;
			}
		}

		sealed class TestUPEAlternateBrokerDocumentPackAutoDelivery : UPEAlternateBrokerDocumentPackAutoDelivery
		{
			public TestUPEAlternateBrokerDocumentPackAutoDelivery(UPEJobDeclaration declaration) : base(declaration)
			{
			}

			internal bool WasDelivered;
			public override void Deliver()
			{
				WasDelivered = true;
			}
		}
	}
}
