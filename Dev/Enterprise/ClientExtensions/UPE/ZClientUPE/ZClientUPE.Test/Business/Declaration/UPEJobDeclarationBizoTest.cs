using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.BISI.Testing;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Declaration.Testing
{
	[TestedType(typeof(UPEJobDeclaration))]
	sealed class UPEJobDeclarationBizoTest : BaseJobDeclarationAbstractTest
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPEJobDeclaration>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestLetterOfAuthorityProcessing()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();

			var declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			var importer = Factory.NewWithValidTestData<UPEOrgHeader>();
			((UPEOrgMiscServ)importer.MiscServ).LOAReceivedAuthorisingUPStoClearGoods = false;
			importer.DateLOAReceivedAuthorisingUPStoClearGoods = ZDateTime.Now;
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();
			AssertEquals("Letter Of Authority should be printed", 1, LOAPrinted(declaration));
			declaration.JE_HouseBill = "HouseBill";
			Factory.Save();
			AssertEquals("Letter Of Authority should be printed only once", 1, LOAPrinted(declaration));

			declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			importer = Factory.NewWithValidTestData<UPEOrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			((UPEOrgMiscServ)importer.MiscServ).LOAReceivedAuthorisingUPStoClearGoods = true;
			Factory.Save();
			AssertEquals("Letter Of Authority should not be printed", 0, LOAPrinted(declaration));
			((UPEOrgMiscServ)importer.MiscServ).LOAReceivedAuthorisingUPStoClearGoods = false;
			importer.DateLOAReceivedAuthorisingUPStoClearGoods = ZDateTime.Now.AddDays(-8);
			declaration.JE_HouseBill = "HouseBill1";
			Factory.Save();
			AssertEquals("Letter Of Authority should not be printed", 0, LOAPrinted(declaration));
			importer.DateLOAReceivedAuthorisingUPStoClearGoods = ZDateTime.Empty;
			declaration.JE_HouseBill = "HouseBill2";
			Factory.Save();
			AssertEquals("Letter Of Authority should be printed", 1, LOAPrinted(declaration));

			JobDeclaration.JE_OH_Importer = importer.PK;
			Factory.Save();
			Assert(JobDeclaration.AutoSendingLetterOfAuthorityErrorMessage.StartsWith("Unable to Autosend the Lettter of Authority to the printer"));
		}

		public void TestDelete_CascadeDeletesOrgRematch()
		{
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			UPEOrgRematch.LogRematch(JobDeclaration, JobDeclaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			Factory.Save();
			UPEOrgRematch rematch = Factory.LoadTop1<UPEOrgRematch>(new ZQuery(ClientOrgRematchSchema.T5_JE, JobDeclaration.PK));

			AssertEquals("UPEOrgRematch not deleted initially for the test", false, rematch.IsDeleted);
			JobDeclaration.Delete();
			AssertEquals("UPEOrgRematch is cascade deleted when OrgMatchApproval is deleted", true, rematch.IsDeleted);
		}

		public void TestDelete_CascadeDeletesRefundProcessing()
		{
			ClientRefund refund = JobDeclaration.RefundManager.CreateClientRefund();
			Factory.Save();
			AssertEquals("ClientRefund should be saved for the test", true, JobDeclaration.Refund.IsInDatabase);

			refund = JobDeclaration.Refund;
			AssertNotNull("JobDeclaration.Refund", refund);
			JobDeclaration.Delete();
			Factory.Save();
			AssertEquals("ClientRefund record should be cascade deleted", true, refund.IsDeleted);
		}

		#region Related Business Objects

		public void TestAlternateBroker()
		{
			AssertNull(JobDeclaration.AlternateBroker);
			JobDeclaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			AssertNull(JobDeclaration.AlternateBroker);
			JobDeclaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertNotNull(JobDeclaration.AlternateBroker);
		}

		public void TestImporter()
		{
			JobDeclaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			AssertNotNull(JobDeclaration.Importer);
		}

		public void TestCallout()
		{
			JobDeclaration.JE_HouseBill = "123";

			var callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_HAWB = "123";
			callout.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			var calloutSubsequentSplit = Factory.NewWithValidTestData<Callout>();
			calloutSubsequentSplit.CS_HAWB = "123";
			calloutSubsequentSplit.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			calloutSubsequentSplit.CurrentQueue.P4_CustomsReason = ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			Factory.Save();

			JobDeclaration.ResetRelatedCusHAWBs();
			AssertEquals("Should return the correct Callout, not the a subsequent split if split shipment", callout, JobDeclaration.Callout);
		}

		public void TestCallout_SameHAWBForCalloutAndUPECusHAWB()
		{
			var cusMAWB = Factory.New<UPECusMAWB>();
			var cusHAWB1 = Factory.New<UPECusHAWB>();
			cusHAWB1.CS_CM = cusMAWB.PK;
			cusHAWB1.CS_HAWB = "999";
			cusHAWB1.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			cusHAWB1.CurrentQueue.P4_QueueName = ZString.Empty;

			var cusHAWB2 = Factory.NewWithValidTestData<Callout>();
			cusHAWB2.CS_CM = cusMAWB.PK;
			cusHAWB2.CS_HAWB = "999";
			cusHAWB2.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			cusHAWB2.CurrentQueue.P4_QueueName = "AAA";

			Factory.Save();
			AssertEquals(2, JobDeclaration.RelatedCusHAWBs.Count);
			AssertEquals("No exception thrown- returns correct callout.", cusHAWB2.PK, JobDeclaration.Callout.PK);
		}

		public void TestCallout_MultipleCalloutWithSameHAWBExists()
		{
			var cusMAWB = Factory.New<UPECusMAWB>();
			var cusHAWB1 = Factory.New<UPECusHAWB>();
			cusHAWB1.CS_CM = cusMAWB.PK;
			cusHAWB1.CS_HAWB = "999";
			cusHAWB1.CS_GoodsDescription = "Frist";
			cusHAWB1.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			var queue1 = Factory.NewWithValidTestData<ProcessQueue>();
			queue1.P4_ParentID = cusHAWB1.PK;
			queue1.P4_QueueName = ZString.Empty;

			var cusHAWB2 = Factory.NewWithValidTestData<Callout>();
			cusHAWB2.CS_CM = cusMAWB.PK;
			cusHAWB2.CS_HAWB = "999";
			cusHAWB2.CS_GoodsDescription = "Second";
			cusHAWB2.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			cusHAWB2.BisiDownloadDate = ZDateTime.Empty;
			var queue2 = Factory.NewWithValidTestData<ProcessQueue>();
			queue2.P4_ParentID = cusHAWB2.PK;
			queue2.P4_QueueName = ZString.Empty;

			var cusHAWB3 = Factory.NewWithValidTestData<Callout>();
			cusHAWB3.CS_CM = cusMAWB.PK;
			cusHAWB3.CS_HAWB = "999";
			cusHAWB3.CS_GoodsDescription = "Third";
			cusHAWB3.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			cusHAWB3.BisiDownloadDate = ZDateTime.Today;
			var queue3 = Factory.NewWithValidTestData<ProcessQueue>();
			queue3.P4_ParentID = cusHAWB3.PK;
			queue3.P4_QueueName = ZString.Empty;

			Factory.Save();

			AssertEquals(3, JobDeclaration.RelatedCusHAWBs.Count);
			AssertEquals("First callout BO with charges is returned.", cusHAWB3.PK, JobDeclaration.Callout.PK);
		}

		public void TestCallout_LoadDifferentBO_WhenMultipleCalloutWithSameHAWBExists()
		{
			var cusMAWB = Factory.New<UPECusMAWB>();
			var cusHAWB1 = Factory.New<UPECusHAWB>();
			cusHAWB1.CS_CM = cusMAWB.PK;
			cusHAWB1.CS_HAWB = "999";
			cusHAWB1.CS_GoodsDescription = "Frist";
			cusHAWB1.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			var cusHAWB2 = Factory.NewWithValidTestData<Callout>();
			cusHAWB2.CS_CM = cusMAWB.PK;
			cusHAWB2.CS_HAWB = "999";
			cusHAWB2.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			cusHAWB2.CS_GoodsDescription = "Second";
			cusHAWB2.BisiDownloadDate = ZDateTime.Now.AddHours(-1);

			var cusHAWB3 = Factory.NewWithValidTestData<Callout>();
			cusHAWB3.CS_CM = cusMAWB.PK;
			cusHAWB3.CS_HAWB = "999";
			cusHAWB3.CS_GoodsDescription = "Third";
			cusHAWB3.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			cusHAWB3.BisiDownloadDate = ZDateTime.Now;

			var cusHAWB4 = Factory.New<UPECusHAWB>();
			cusHAWB4.CS_CM = cusMAWB.PK;
			cusHAWB4.CS_GoodsDescription = "Fourth";
			cusHAWB4.CS_HAWB = "888";
			cusHAWB4.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			var queue1 = Factory.NewWithValidTestData<ProcessQueue>();
			queue1.P4_ParentID = cusHAWB1.PK;
			queue1.P4_QueueName = "AAA";
			queue1.P4_ParentTableCode = "CS";
			queue1.P4_Reason = "123";

			var queue2 = Factory.NewWithValidTestData<ProcessQueue>();
			queue2.P4_ParentID = cusHAWB2.PK;
			queue2.P4_ParentTableCode = "CS";
			queue2.P4_QueueName = "AAA";
			queue2.P4_Reason = "123";

			var queue3 = Factory.NewWithValidTestData<ProcessQueue>();
			queue3.P4_ParentID = cusHAWB3.PK;
			queue3.P4_ParentTableCode = "CS";
			queue3.P4_QueueName = "AAA";
			queue3.P4_Reason = "123";

			Factory.Save();

			AssertEquals(4, JobDeclaration.RelatedCusHAWBs.Count);
			AssertEquals("First callout BO with charges is returned.", cusHAWB3.PK, JobDeclaration.Callout.PK);
		}

		#endregion

		#region CommoditiesData

		public void TestCommoditiesData()
		{
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			JobComInvoiceHeader invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "1111";
			invHeader.ZA_ORG = Core.Constants.CountryCodes.UnitedStates;

			JobComInvoiceLine invLine1 = invHeader.JobComInvoiceLines.AddNew();
			invLine1.JI_Description = "Goods1";
			invLine1.JI_Tariff = "1";
			invLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invLine1.JI_LinePrice = 100.10;

			JobComInvoiceLine invLine2 = invHeader.JobComInvoiceLines.AddNew();
			invLine2.JI_Description = "Goods2";
			invLine2.JI_Tariff = "2";
			invLine2.JI_CountryOfOrigin = "";
			invLine2.JI_LinePrice = 200.20;

			var commoditiesData = declaration.CommoditiesData;
			AssertEquals("CommoditiesData.Length", 2, commoditiesData.Count);
			AssertCommodityDetailData(commoditiesData[0], "Goods1", "1", Core.Constants.CountryCodes.Australia, 100.10m);
			AssertCommodityDetailData(commoditiesData[1], "Goods2", "2", Core.Constants.CountryCodes.UnitedStates, 200.20m);
		}

		void AssertCommodityDetailData(CommodityDetailData commoditiesData, ZString expectedDescription, ZString expectedTariff, ZString expectedCountry, ZDecimal expectedItemPrice)
		{
			AssertEquals(expectedDescription, commoditiesData.GoodsDescription);
			AssertEquals(expectedTariff, commoditiesData.TariffNumber);
			AssertEquals(expectedCountry, commoditiesData.CountryOfOrigin);
			AssertEquals(expectedItemPrice, commoditiesData.ItemPrice);
		}

		#endregion

		#region Charges Data

		public void TestChargesData()
		{
			UPEJobDeclarationWithDummyCharges declaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.QuarantineFee = 100m;
			declaration.QuarantineProcessingFee = 54m;

			var chargesData = ShipmentChargeDataComparer.SortChargesByTypeCode(declaration.ChargesData);
			AssertEquals("Sanity check", false, declaration.HasAlternateBroker);
			AssertEquals(6, chargesData.Count);
			AssertShipmentChargeData(chargesData[0], "201", 10m);
			AssertShipmentChargeData(chargesData[1], "206", 20m);
			AssertShipmentChargeData(chargesData[2], "216", 60m);
			AssertShipmentChargeData(chargesData[3], "224", 100m);
			AssertShipmentChargeData(chargesData[4], "231", 36.60m);
			AssertShipmentChargeData(chargesData[5], "309", 54m);
		}

		public void TestChargesData_EntryNotCompleted()
		{
			UPEJobDeclarationWithDummyCharges declaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			declaration.QuarantineFee = 100m;
			declaration.QuarantineProcessingFee = 54m;

			var chargesData = ShipmentChargeDataComparer.SortChargesByTypeCode(declaration.ChargesData);
			AssertEquals("Sanity check", false, declaration.HasAlternateBroker);
			AssertEquals(2, chargesData.Count);
			AssertShipmentChargeData(chargesData[0], "224", 100m);
			AssertShipmentChargeData(chargesData[1], "309", 54m);
		}

		public void TestChargesData_HandledByAlternateBroker()
		{
			UPEJobDeclarationWithDummyCharges declaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
			UPEDataRegistry.Instance.TerminalFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 45m);
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			declaration.QuarantineFee = 100m;
			declaration.QuarantineProcessingFee = 54m;
			declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);

			var chargesData = ShipmentChargeDataComparer.SortChargesByTypeCode(declaration.ChargesData);
			AssertEquals("Sanity check", true, declaration.HasAlternateBroker);
			AssertEquals(7, chargesData.Count);
			AssertShipmentChargeData(chargesData[0], "201", 10m);
			AssertShipmentChargeData(chargesData[1], "206", 20m);
			AssertShipmentChargeData(chargesData[2], "216", 60m);
			AssertShipmentChargeData(chargesData[3], "224", 100m);
			AssertShipmentChargeData(chargesData[4], "231", 36.60m);
			AssertShipmentChargeData(chargesData[5], "309", 54m);
			AssertShipmentChargeData(chargesData[6], "436", 45m);
		}

		public void TestChargesData_HandledByAlternateBroker_WithoutITFChargable()
		{
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			UPEDataRegistry.Instance.TerminalFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 45m);
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			declaration.QuarantineFee = 100m;
			declaration.QuarantineProcessingFee = 54m;
			declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			declaration.Importer.IsITFChargableForThisImporter = false;
			declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);

			AssertEquals("Sanity check", true, declaration.HasAlternateBroker);
			foreach (ShipmentChargeData charge in declaration.ChargesData)
			{
				Assert("ITF charges should not be applied when Importer.IsITFChargableForThisImporter=false", charge.TypeCodeEnum != ShipmentChargeTypeCode.Terminal);
			}
		}

		public void TestChargesData_AllKeyEnteredAndRegistryChargesAreEmpty()
		{
			UPEJobDeclarationWithDummyCharges declaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
			UPEDataRegistry.Instance.TerminalFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m);
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			declaration.QuarantineFee = 0m;
			declaration.QuarantineProcessingFee = 0m;
			declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);

			var chargesData = ShipmentChargeDataComparer.SortChargesByTypeCode(declaration.ChargesData);
			AssertEquals("Sanity check", true, declaration.HasAlternateBroker);
			AssertEquals(4, chargesData.Count);
			AssertShipmentChargeData(chargesData[0], "201", 10m);
			AssertShipmentChargeData(chargesData[1], "206", 20m);
			AssertShipmentChargeData(chargesData[2], "216", 60m);
			AssertShipmentChargeData(chargesData[3], "231", 36.60m);
		}

		public void TestChargesData_ImporterHasEFTSetupWithCustoms()
		{
			UPEJobDeclarationWithDummyCharges declaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_IsConsignee = true;
			declaration.JE_OH_Importer = importer.PK;
			importer.MiscServ.OM_IMEFTBankBSB = "TEST";

			AssertEquals("PreCondition", false, declaration.HasAlternateBroker);

			declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.QuarantineFee = 100m;
			declaration.QuarantineProcessingFee = 54m;
			AssertEquals(6, declaration.ChargesData.Count);

			importer.MiscServ.OM_IMEFTBankAccount = "TEST";
			AssertEquals(6, declaration.ChargesData.Count);

			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			var chargesData = ShipmentChargeDataComparer.SortChargesByTypeCode(declaration.ChargesData);
			AssertEquals(2, chargesData.Count);
			AssertShipmentChargeData(chargesData[0], "224", 100m);
			AssertShipmentChargeData(chargesData[1], "309", 54m);

			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.Prepaid;
			uPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			declaration.RelatedCusHAWBs.Load();

			chargesData = ShipmentChargeDataComparer.SortChargesByTypeCode(declaration.ChargesData);
			AssertEquals(2, chargesData.Count);
			AssertShipmentChargeData(chargesData[0], "224", 100m);
			AssertShipmentChargeData(chargesData[1], "309", 54m);

			uPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			AssertEquals(6, declaration.ChargesData.Count);
		}

		public void TestUploadCustomsCharges()
		{
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			consignee.MiscServ.OM_IMEFTBankBSB = "2146";
			consignee.MiscServ.OM_IMEFTBankAccount = "101814435";
			consignee.MiscServ.OM_IMEftCustomsFromImport = true;
			JobDeclaration.JE_OH_Importer = consignee.PK;

			CusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			CusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeBorder;

			Assert("Is DirectDebit and Not FreeDomicile", !JobDeclaration.UploadCustomsCharges);

			consignee.MiscServ.OM_IMEFTBankBSB = ZString.Empty;
			Assert("Not Direct Debit, upload charges should be true", JobDeclaration.UploadCustomsCharges);

			consignee.MiscServ.OM_IMEFTBankBSB = "2156";
			CusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			Assert("Is FreeDomicile, upload charges should be true", JobDeclaration.UploadCustomsCharges);
		}

		public void TestIsCustomsEFTActive()
		{
			AssertNull("PreCondition: import is null", JobDeclaration.Importer);
			Assert("No Importer, should be false", !JobDeclaration.IsCustomsEFTActive);

			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			consignee.MiscServ.OM_IMEFTBankBSB = ZString.Empty;
			consignee.MiscServ.OM_IMEFTBankAccount = ZString.Empty;
			consignee.MiscServ.OM_IMEftCustomsFromImport = false;
			JobDeclaration.JE_OH_Importer = consignee.PK;

			consignee.MiscServ.OM_IMEFTBankBSB = "2146";
			Assert(!JobDeclaration.IsCustomsEFTActive);

			consignee.MiscServ.OM_IMEFTBankAccount = "101814435";
			Assert(!JobDeclaration.IsCustomsEFTActive);

			consignee.MiscServ.OM_IMEftCustomsFromImport = true;
			Assert(JobDeclaration.IsCustomsEFTActive);
		}

		void AssertShipmentChargeData(ShipmentChargeData chargeData, ZString expectedTypeCode, ZDecimal expectedAmount)
		{
			AssertEquals(expectedTypeCode, chargeData.TypeCode);
			AssertEquals(expectedAmount, chargeData.GrossAmount);
		}

		#endregion

		#region New Properties

		public void TestBisiUploadDate()
		{
			AssertEquals("Pre-condition", ZDateTime.Empty, JobDeclaration.BisiUploadDate);

			ZDateTime expectedDate = new ZDateTime(2005, 10, 3);
			JobDeclaration.CurrentQueue.P4_CustomDate1 = expectedDate;
			AssertEquals("Value should change", expectedDate, JobDeclaration.BisiUploadDate);

			expectedDate = new ZDateTime(2005, 11, 4);
			JobDeclaration.BisiUploadDate = expectedDate;
			AssertEquals("Value should change", expectedDate, JobDeclaration.BisiUploadDate);
			AssertEquals("Value should change", expectedDate, JobDeclaration.CurrentQueue.P4_CustomDate1);
			AssertEquals("InnerInfo should be " + JobDeclaration.CurrentQueue.P4_CustomDate1Info.Name, JobDeclaration.CurrentQueue.P4_CustomDate1Info, ((ZWrappedPropertyInfo)JobDeclaration.BisiUploadDateInfo).InnerInfo);
		}

		public void TestCustomsEntryDate()
		{
			AssertEquals("Pre-condition", ZDateTime.Empty, JobDeclaration.CustomsEntryDate);

			ZDateTime expectedDate = new ZDateTime(2005, 10, 3);
			JobDeclaration.CurrentQueue.P4_CustomDate2 = expectedDate;
			AssertEquals("Value should change", expectedDate, JobDeclaration.CustomsEntryDate);

			expectedDate = new ZDateTime(2005, 11, 4);
			JobDeclaration.CustomsEntryDate = expectedDate;
			AssertEquals("Value should change", expectedDate, JobDeclaration.CustomsEntryDate);
			AssertEquals("Value should change", expectedDate, JobDeclaration.CurrentQueue.P4_CustomDate2);
			AssertEquals("InnerInfo should be " + JobDeclaration.CurrentQueue.P4_CustomDate2Info.Name, JobDeclaration.CurrentQueue.P4_CustomDate2Info, ((ZWrappedPropertyInfo)JobDeclaration.CustomsEntryDateInfo).InnerInfo);
		}

		public void TestQuarantineFee()
		{
			AssertEquals("Pre-condition", 0m, JobDeclaration.QuarantineFee);

			JobDeclaration.CurrentQueue.P4_CustomDecimal1 = 243.2m;
			AssertEquals("Value should change", 243.2m, JobDeclaration.QuarantineFee);

			JobDeclaration.QuarantineFee = 45.4m;
			AssertEquals("Value should change", 45.4m, JobDeclaration.QuarantineFee);
			AssertEquals("Value should change", 45.4m, JobDeclaration.CurrentQueue.P4_CustomDecimal1);
			AssertEquals("InnerInfo should be " + JobDeclaration.CurrentQueue.P4_CustomDecimal1Info.Name, JobDeclaration.CurrentQueue.P4_CustomDecimal1Info, ((ZWrappedPropertyInfo)JobDeclaration.QuarantineFeeInfo).InnerInfo);
		}

		public void TestLOALogReferenceText()
		{
			AssertEquals("Letter of Authority was printed", JobDeclaration.GetLOALogReferenceText());
		}

		public void TestQuarantineProcessingFee()
		{
			AssertEquals("Pre-condition", 0m, JobDeclaration.QuarantineProcessingFee);

			JobDeclaration.CurrentQueue.P4_CustomDecimal2 = 243.2m;
			AssertEquals("Value should change", 243.2m, JobDeclaration.QuarantineProcessingFee);

			JobDeclaration.QuarantineProcessingFee = 45.4m;
			AssertEquals("Value should change", 45.4m, JobDeclaration.QuarantineProcessingFee);
			AssertEquals("Value should change", 45.4m, JobDeclaration.CurrentQueue.P4_CustomDecimal2);
			AssertEquals("InnerInfo should be " + JobDeclaration.CurrentQueue.P4_CustomDecimal2Info.Name, JobDeclaration.CurrentQueue.P4_CustomDecimal2Info, ((ZWrappedPropertyInfo)JobDeclaration.QuarantineProcessingFeeInfo).InnerInfo);
		}

		public void TestTotalQuarantineCharges()
		{
			JobDeclaration.QuarantineFee = 0m;
			JobDeclaration.QuarantineProcessingFee = 0m;
			AssertEquals("TotalQuarantineCharges is the sum of QuarantineFee and QuarantineProcessingFee", 0m, JobDeclaration.TotalQuarantineCharges);

			JobDeclaration.QuarantineFee = 418m;
			JobDeclaration.QuarantineProcessingFee = 54m;
			AssertEquals("TotalQuarantineCharges is the sum of QuarantineFee and QuarantineProcessingFee", 472m, JobDeclaration.TotalQuarantineCharges);
		}

		public void TestAlternateBrokerStorageFeeStartDate()
		{
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2006, 1, 5); // Thursday

			GlbHoliday holiday = GlbBranch.CurrentBranch.GlbHolidays.AddNew();
			holiday.GH_Date = new ZDateTime(2006, 1, 9); // Monday

			JobDeclaration.JE_RS_NKServiceLevel = "1";
			AssertEquals("Express shipments date 2 after arrival, excluding weekends and public holidays (10th is Tuesday)", new ZDateTime(2006, 1, 10), JobDeclaration.AlternateBrokerStorageFeeStartDate);

			JobDeclaration.JE_RS_NKServiceLevel = "5";
			AssertEquals("Expedited shipments date 3 after arrival, excluding weekends and public holidays (11th is Wednesday)", new ZDateTime(2006, 1, 11), JobDeclaration.AlternateBrokerStorageFeeStartDate);
		}

		[TestDate(2006, 3, 7)]
		public void TestAlternateBrokerStorageFeeIncludingGST()
		{
			JobDeclaration.JE_RS_NKServiceLevel = "1";
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2006, 3, 1);
			AssertEquals(new ZDateTime(2006, 3, 3), JobDeclaration.AlternateBrokerStorageFeeStartDate);
			AssertEquals("StorageFee for 2 working days of storage (Friday-Tuesday)", 77m, JobDeclaration.AlternateBrokerStorageFeeIncludingGST);
		}

		public void TestIsEntryPrintToBISIPending()
		{
			AssertEquals("Should be false by default", false, JobDeclaration.IsEntryPrintToBISIPending);
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			AssertEquals("When AuthorityToDeal entry status is received", true, JobDeclaration.IsEntryPrintToBISIPending);

			JobDeclaration.IsEntryPrintToBISIPending = false;
			AssertEquals("When IsEntryPrintToBISIPending is set to false", false, JobDeclaration.IsEntryPrintToBISIPending);

			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("When Clear entry status is received", true, JobDeclaration.IsEntryPrintToBISIPending);
		}

		public void TestIsAudit()
		{
			AssertEquals(false, JobDeclaration.IsAudit);
			AssertEquals(false, JobDeclaration.CurrentQueue.P4_CustomFlag2);
			JobDeclaration.IsAudit = true;
			AssertEquals(true, JobDeclaration.IsAudit);
			AssertEquals(true, JobDeclaration.CurrentQueue.P4_CustomFlag2);
		}

		public void TestCommercialInvoiceImage_WithInvalidImage()
		{
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			StorageMain parent = factory.New<StorageMain>();
			parent.SM_ParentFK = JobDeclaration.PK;

			StorageDocs document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = new byte[] { 1, 2, 3 };
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;

			JobDeclaration.DocManagerInfo.Documents.Add(document);
			JobDeclaration.DocManagerInfo.Save();

			AssertEquals("No errors before the image is accessed", 0, ExceptionReporterTestListener.Instance.Count);
			Image notUsed = JobDeclaration.CommercialInvoiceImage;
			AssertEquals("Should be a silenced exception raise due to invalid image bytes", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestIsShipperMatchError()
		{
			Assert(!JobDeclaration.CurrentQueue.P4_CustomFlag3);
			Assert("Should not have ShipperMatchingError in QueueLogs", !JobDeclaration.CurrentQueue.CustomsQueueLogs.ContainsQueue("", ReasonCodeDescriptionPairList.Codes.EN_ShipperMatchingError, ""));
			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, "EN Ticked");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			StmALog[] logs = (StmALog[])JobDeclaration.Logs.GetAllLogs().Find(query);
			AssertEquals(0, logs.Length);

			JobDeclaration.JE_GB = PerBranch.PK;
			ZDateTime melbourneTime;

			using (DisposableEnvironment.ForBranch(MelBranch.GB_Code))
			{
				melbourneTime = ZDateTime.Now;
				JobDeclaration.ShipperMatchingError();
				Assert(JobDeclaration.CurrentQueue.P4_CustomFlag3);
				Assert("Should have ShipperMatchingError in QueueLogs", JobDeclaration.CurrentQueue.CustomsQueueLogs.ContainsQueue("", ReasonCodeDescriptionPairList.Codes.EN_ShipperMatchingError, ""));
				var customsQueueLogsCount = JobDeclaration.CurrentQueue.CustomsQueueLogs.Count;
				AssertGreaterThan("Queue log 's event time should be based on declaration's branch", melbourneTime, JobDeclaration.CurrentQueue.CustomsQueueLogs[0].SL_EventTime);
				logs = (StmALog[])JobDeclaration.Logs.GetAllLogs().Find(query);
				AssertEquals(1, logs.Length);

				JobDeclaration.ShipperMatchingError();
				Assert(JobDeclaration.CurrentQueue.P4_CustomFlag3);
				Assert("Should have ShipperMatchingError in QueueLogs", JobDeclaration.CurrentQueue.CustomsQueueLogs.ContainsQueue("", ReasonCodeDescriptionPairList.Codes.EN_ShipperMatchingError, ""));
				AssertEquals("second log should not be added to the queue", customsQueueLogsCount, JobDeclaration.CurrentQueue.CustomsQueueLogs.Count);
				logs = (StmALog[])JobDeclaration.Logs.GetAllLogs().Find(query);
				AssertEquals(1, logs.Length);
			}
		}

		#endregion

		#region Split Shipment Testing

		public void TestAddMasterBillForSplitShipment()
		{
			UPEJobDeclaration jobDec = Factory.New<UPEJobDeclaration>();
			AssertEquals(false, jobDec.TryAddMasterBillForSplitShipment(""));

			AssertEquals(true, jobDec.TryAddMasterBillForSplitShipment("TEST1"));
			AssertEquals("TEST1", jobDec.Bills.FindByBillNumberAndType("TEST1", BillTypeList.Codes.MasterBill).CU_MasterBill);
			AssertEquals("Masterbill Added = TEST1", jobDec.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
			AssertEquals(1, jobDec.Bills.Count);

			jobDec.Bills[0].CU_MasterBill = "";

			AssertEquals(true, jobDec.TryAddMasterBillForSplitShipment("TEST1"));
			AssertEquals("TEST1", jobDec.Bills.FindByBillNumberAndType("TEST1", BillTypeList.Codes.MasterBill).CU_MasterBill);
			AssertEquals(1, jobDec.Bills.Count);
			AssertEquals("Masterbill Added = TEST1", jobDec.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			AssertEquals(false, jobDec.TryAddMasterBillForSplitShipment("TEST1"));
			AssertEquals("TEST1", jobDec.Bills.FindByBillNumberAndType("TEST1", BillTypeList.Codes.MasterBill).CU_MasterBill);
			AssertEquals(1, jobDec.Bills.Count);
			AssertEquals("Masterbill Added = TEST1", jobDec.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestEnsureSplitShipmentWithAltBrokerIsNotMoved()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23111111111";

			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "TestHAWB";
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			JobDeclaration.JE_MasterBill = "23111111111";
			JobDeclaration.JE_HouseBill = "TestHAWB";
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;

			Factory.Save();

			uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "43311111111";

			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "TestHAWB";
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.BCA;
			JobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker;

			JobDeclaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			JobDeclaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);

			JobDeclaration.JE_MessageStatus = "";

			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.BCA, JobDeclaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker, JobDeclaration.CurrentQueue.P4_CustomsStatus);

			JobDeclaration.TryMoveSplitShipmentToLodgmentQueue();

			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.BCA, JobDeclaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker, JobDeclaration.CurrentQueue.P4_CustomsStatus);
		}

		public void TestIsHoldForCollectionSetToTrue_WithAlternateBrokerThatDoesntHandleDelivery()
		{
			TestIsHoldForCollectionSetToTrue_WithAlternateBrokerThatDoesntHandleDelivery(false);
			TestIsHoldForCollectionSetToTrue_WithAlternateBrokerThatDoesntHandleDelivery(true);
		}

		public void TestMoveQueue()
		{
			JobDeclaration.JE_MessageStatus = "";
			JobDeclaration.JE_EntryStatus = "";
			JobDeclaration.CurrentQueue.P4_CustomsQueue = "";

			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Unknown, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Completed, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Pending, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Submitted, JobDeclaration.CurrentQueue.P4_CustomsQueue);
		}

		public void TestSplitShipmentIdentified()
		{
			UPECusMAWB cusMAWB = Factory.New<UPECusMAWB>();
			cusMAWB.CM_MAWB = "23111111111";

			UPECusHAWB cusHAWB = Factory.New<UPECusHAWB>();
			cusHAWB.CS_CM = cusMAWB.PK;
			cusHAWB.CS_HAWB = "TestHAWB";
			cusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			JobDeclaration.JE_MasterBill = "23111111111";
			JobDeclaration.JE_HouseBill = "TestHAWB";
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Submitted;

			Factory.Save();

			cusMAWB = Factory.New<UPECusMAWB>();
			cusMAWB.CM_MAWB = "43311111111";

			cusHAWB = Factory.New<UPECusHAWB>();
			cusHAWB.CS_CM = cusMAWB.PK;
			cusHAWB.CS_HAWB = "TestHAWB";
			cusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			JobDeclaration.JE_MessageStatus = "";

			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			JobDeclaration.JE_MessageStatus = "";
			Factory.Save();
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Submitted, JobDeclaration.CurrentQueue.P4_CustomsQueue);
		}

		void TestIsHoldForCollectionSetToTrue_WithAlternateBrokerThatDoesntHandleDelivery(bool isDeliveryHandledByAlternateBroker)
		{
			var callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			Factory.Save();

			UPEOrgHeader alternateBroker = (UPEOrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			alternateBroker.IsDeliveryHandledByUPSForThisAlternateBroker = isDeliveryHandledByAlternateBroker;

			UPEOrgHeader importer = Factory.NewWithValidTestData<UPEOrgHeader>();
			importer.SetRelatedParty(alternateBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("Starts at false before moved to alternate broker queue", false, callout.IsHoldForCollection);

			callout.Declaration.JE_OH_Importer = importer.PK;
			if (isDeliveryHandledByAlternateBroker)
			{
				AssertEquals("Defaults be false if there is an alternate broker that handles delivery", false, callout.IsHoldForCollection);
			}
			else
			{
				AssertEquals("Must be true if there is an alternate broker that doesn't handle delivery", true, callout.IsHoldForCollection);
			}
		}

		public void TestMoveQueueWhenStatusInCertainOrder()
		{
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			JobDeclaration.CurrentQueue.P4_CustomsQueue = "";
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Unknown, JobDeclaration.CurrentQueue.P4_CustomsQueue);
		}

		public void TestMoveQueueWhenStatusInCertainOrder1()
		{
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			JobDeclaration.CurrentQueue.P4_CustomsQueue = "";
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.MultiStatus.Code;
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Unknown, JobDeclaration.CurrentQueue.P4_CustomsQueue);
		}

		public void TestMoveQueueWhenStatusInCertainOrder2()
		{
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			JobDeclaration.CurrentQueue.P4_CustomsQueue = "";
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Rejected.Code;
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Unknown, JobDeclaration.CurrentQueue.P4_CustomsQueue);
		}

		public void TestMoveQueueWhenStatusInCertainOrder3()
		{
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			JobDeclaration.CurrentQueue.P4_CustomsQueue = "";
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			JobDeclaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Unknown, JobDeclaration.CurrentQueue.P4_CustomsQueue);
		}

		#endregion

		#region Manual Bill Notification

		public void TestManualBillNotifications()
		{
			BillingNotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.ManualbillNotificationGroup = BillingNotificationGroup.PK.ToGuid();

			JobDeclaration.ManualBillNotification.SetShouldDoManualBillActivitiesOnSave(true);
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("ShouldSendEmail should be true for the test", true, JobDeclaration.ManualBillNotification.ShouldDoManualBillActivitiesOnSave);

			JobDeclaration.JE_HouseBill = "H1234";
			Factory.Save();
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should send the Manual Bill email when required", "Manual Bill - H1234", email.Subject);
		}

		#endregion

		public void TestTotalJobsImportedForImporter()
		{
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;

			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();

			Factory.Save();
			AssertEquals(0, uPEJobDeclaration.TotalJobsImportedForImporter);

			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			uPEJobDeclaration.JE_OH_Importer = orgHeader.PK;
			Factory.Save();
			AssertEquals(1, uPEJobDeclaration.TotalJobsImportedForImporter);

			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.JE_OH_Importer = orgHeader.PK;
			Factory.Save();
			AssertEquals(2, uPEJobDeclaration.TotalJobsImportedForImporter);
		}

		public void TestCurrentQueueInitialisedInConstructor()
		{
			AssertEquals("Should be unallocated initially", DeclarationQueueCodeDescriptionPairList.Codes.Compiling, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			Assert("HasChanges should be set to false", !JobDeclaration.CurrentQueue.HasChanges);
			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.EIR;
			Factory.Save();

			UPEJobDeclaration newDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(JobDeclaration.PK);
			AssertEquals("Should not be re-set once the Queue is persisted in DB", DeclarationQueueCodeDescriptionPairList.Codes.EIR, newDeclaration.CurrentQueue.P4_CustomsQueue);
		}

		public void TestHasAlternateBroker()
		{
			JobDeclaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("No importer, should be false", false, JobDeclaration.HasAlternateBroker);

			JobDeclaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("Import Air Customs Broker is not set in the OrgHeader, should be false", false, JobDeclaration.HasAlternateBroker);

			JobDeclaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("Has Import Air Customs Broker, should be true", true, JobDeclaration.HasAlternateBroker);
		}

		public void TestMoveToAlternateBrokerHold()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			JobDeclaration.JE_OH_Importer = importer.PK;
			AssertEquals("No Move Expected", DeclarationQueueCodeDescriptionPairList.Codes.Compiling, JobDeclaration.CurrentQueue.P4_CustomsQueue);

			JobDeclaration.JE_OH_Importer = ZGuid.Empty;
			JobDeclaration.JE_GB = PerBranch.PK;
			AssertEquals("No logs related to 'Alternate Broker'", 0, JobDeclaration.Logs.Find(e => e.SL_SE_NKEvent == Events.EditedARecord.Code && e.SL_Reference.Contains("Moved to BCA: Has Alternate Broker")).Count());

			ZDateTime melbourneTime = ZDateTime.Empty;

			using (DisposableEnvironment.ForBranch(MelBranch.GB_Code))
			{
				melbourneTime = ZDateTime.Now;
				OrgHeader alternateBroker = Factory.New<OrgHeader>();
				importer.SetRelatedParty(alternateBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
				JobDeclaration.JE_OH_Importer = importer.PK;
			}

			var log = JobDeclaration.Logs.Find(e => e.SL_SE_NKEvent == Events.EditedARecord.Code && e.SL_Reference.Contains("Moved to BCA: Has Alternate Broker")).FirstOrDefault();
			AssertNotNull("Log related to Alternate broker should be found", log);
			AssertEquals("Event 's Branch", perBranch.GB_Code, log.SL_GB_NKBranch);
			AssertGreaterThan("Event Time should be on Perth 's timezone", melbourneTime, log.SL_EventTime);
			AssertEquals("Hold", DeclarationQueueCodeDescriptionPairList.Codes.BCA, JobDeclaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Alternate broker reason", ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker, JobDeclaration.CurrentQueue.P4_CustomsStatus);
			AssertEquals("Classifier should not assigned to the job", true, JobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo.IsEmpty);
		}

		public void TestAllocateClassifierFromImporter()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();

			OrgStaffAssignments orgStaffAssignments = importer.StaffAssignments.AddNew();
			orgStaffAssignments.O8_GS_NKPersonResponsible = Classifier2.GS_Code;
			orgStaffAssignments.O8_Role = UPEStaffRoles.Codes.Classifier;

			orgStaffAssignments = importer.StaffAssignments.AddNew();
			orgStaffAssignments.O8_GS_NKPersonResponsible = Classifier1.GS_Code;
			orgStaffAssignments.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			JobDeclaration.JE_OH_Importer = importer.PK;
			AssertEquals(Classifier2.GS_Code, JobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo);
		}

		public void TestAllocateSupplierRVRoleClassifier()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			JobDeclaration.JE_OH_Importer = importer.PK;
			AssertEquals("Pre-Condition", "", JobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo);

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();

			OrgStaffAssignments orgStaffAssignments = supplier.StaffAssignments.AddNew();
			orgStaffAssignments.O8_GS_NKPersonResponsible = Classifier1.GS_Code;
			orgStaffAssignments.O8_Role = UPEStaffRoles.Codes.RV;

			JobDeclaration.JE_OH_Supplier = supplier.PK;
			AssertEquals(Classifier1.GS_Code, JobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo);
			ZString classifierRoleNK = importer.StaffAssignments.GetStaffAssignment(UPEStaffRoles.Codes.Classifier, OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Air);
			AssertEquals(Classifier1.GS_Code, classifierRoleNK);
		}

		public void TestSupplierChangedChangesSupplierOnInvoiceHeader()
		{
			JobDeclaration.Invoices.AddNew();
			AssertEquals(true, JobDeclaration.Invoices[0].JZ_OH_Supplier.IsEmpty);
			JobDeclaration.JE_OH_Supplier = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			AssertEquals(JobDeclaration.JE_OH_Supplier, JobDeclaration.Invoices[0].JZ_OH_Supplier);
		}

		public void TestClassifierIsNotAllocatedWhenSupplierIsEmpty()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			JobDeclaration.JE_OH_Importer = importer.PK;
			AssertEquals("No Assignement Expected", "", JobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo);
		}

		public void TestClassifierAssignedToImporterWithNoClassifier()
		{
			OrgHeader importer1 = CreateImporterWithClassifierAssignment(Classifier1);
			OrgHeader importer2 = CreateImporterWithClassifierAssignment(Classifier2);
			OrgHeader importer3 = CreateImporterWithClassifierAssignment(Classifier3);

			CreateDeclarationWithImporter(importer1);
			CreateDeclarationWithImporter(importer1);
			CreateDeclarationWithImporter(importer1);

			CreateDeclarationWithImporter(importer2);
			CreateDeclarationWithImporter(importer2);
			JobDeclaration decoyDec1 = CreateDeclarationWithImporter(importer2);
			decoyDec1.JE_IsCancelled = true;
			JobDeclaration decoyDec2 = CreateDeclarationWithImporter(importer2);
			decoyDec2.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			Factory.Save();
			JobDeclaration decoyDec3 = CreateDeclarationWithImporter(importer2);
			decoyDec3.JE_GB = branch.PK;

			CreateDeclarationWithImporter(importer3);
			CreateDeclarationWithImporter(importer3);
			CreateDeclarationWithImporter(importer3);

			Factory.Save();

			OrgHeader newImporter = Factory.New<OrgHeader>();
			JobDeclaration newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_OH_Supplier = (Factory.NewWithValidTestData<OrgHeader>()).PK;
			newDeclaration.JE_OH_Importer = newImporter.PK;

			AssertEquals("A staff assignment should be created because the importer has no classifier", 1, newImporter.StaffAssignments.Count);
			AssertEquals("New staff assignment should be a classifier", UPEStaffRoles.Codes.Classifier, newImporter.StaffAssignments[0].O8_Role);
			AssertEquals("New staff assignment assigned to the classifier assigned with the least total jobs", Classifier2.GS_Code, newImporter.StaffAssignments[0].O8_GS_NKPersonResponsible);

			AssertEquals("Job should be in the unallocated queue", DeclarationQueueCodeDescriptionPairList.Codes.Compiling, newDeclaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Classifier should be assigned to the queue", Classifier2.GS_Code, newDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo);
		}

		public void TestClassifierAssignment_EnsureThatNonClassifierStaffAreNotIncludededInTheCalculationOfStaffWhoHaveTheLeastNumberOfAllocatedJobs()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			UPEJobDeclaration dec = CreateDeclarationWithImporter(org);
			dec.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "XY";

			dec = CreateDeclarationWithImporter(org);
			dec.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = Classifier1.GS_Code;

			dec = CreateDeclarationWithImporter(org);
			dec.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = Classifier1.GS_Code;

			Factory.Save();

			JobDeclaration newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_OH_Supplier = org.PK;
			newDeclaration.JE_OH_Importer = org.PK;

			AssertEquals("Classifier should be assigned to the queue, not XY", Classifier1.GS_Code, newDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo);
		}

		OrgHeader CreateImporterWithClassifierAssignment(GlbStaff classifier)
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgStaffAssignments assignment = importer.StaffAssignments.AddNew();

			assignment.O8_GS_NKPersonResponsible = classifier.GS_Code;
			assignment.O8_Role = UPEStaffRoles.Codes.Classifier;

			return importer;
		}

		UPEJobDeclaration CreateDeclarationWithImporter(OrgHeader importer)
		{
			UPEJobDeclaration result = Factory.New<UPEJobDeclaration>();
			result.JE_OH_Importer = importer.PK;
			return result;
		}

		[TestDate(2005, 1, 1)]
		public void TestCustomsEntryDateIsSetWhenDeclarationIsCleared()
		{
			AssertEquals(ZDateTime.Empty, JobDeclaration.CustomsEntryDate);

			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			ZDateTime expectedDate = new DateTime(2005, 1, 1);
			AssertEquals(expectedDate, JobDeclaration.CustomsEntryDate);

			TestDateAttribute.Date = new DateTime(2006, 10, 2);
			Factory.Save();
			AssertEquals("Should not be re-assigned once it has been cleared", expectedDate, JobDeclaration.CustomsEntryDate);
		}

		public void TestJE_MasterBillInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_MasterBillInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_MasterBillInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_MasterBillInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_MasterBillInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_FolioInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_FolioInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_FolioInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_FolioInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_FolioInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_RL_NKPortOfLoadingInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_RL_NKFinalDestinationInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_RL_NKFinalDestinationInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_RL_NKFinalDestinationInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_RL_NKFinalDestinationInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_RL_NKFinalDestinationInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_RL_NKPortOfArrivalInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_DateOfArrivalInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_DateOfArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_DateOfArrivalInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_DateOfArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_DateOfArrivalInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_DateOfFirstArrivalInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_DateOfFirstArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_DateOfFirstArrivalInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_DateOfFirstArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_DateOfFirstArrivalInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_HouseBill()
		{
			JobDeclaration.JE_HouseBill = "TEST";
			AssertEquals("TEST", JobDeclaration.JE_HouseBill);
			AssertEquals("TEST", JobDeclaration.JE_AgentsReference);
		}

		public void TestJE_HouseBillInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_HouseBillInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_HouseBillInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_HouseBillInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_HouseBillInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_RL_NKOriginInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_RL_NKOriginInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_RL_NKOriginInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_RL_NKOriginInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_RL_NKOriginInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_RL_NKPortOfFirstArrivalInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfFirstArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfFirstArrivalInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_RL_NKPortOfFirstArrivalInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_RL_NKPortOfFirstArrivalInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_DateAtOriginInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_DateAtOriginInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_DateAtOriginInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_DateAtOriginInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_DateAtOriginInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_DateAtFinalDestinationInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_DateAtFinalDestinationInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_DateAtFinalDestinationInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_DateAtFinalDestinationInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_DateAtFinalDestinationInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_GoodsDescriptionInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_GoodsDescriptionInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_GoodsDescriptionInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_GoodsDescriptionInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_GoodsDescriptionInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_OwnerRefInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_OwnerRefInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_OwnerRefInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_OwnerRefInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_OwnerRefInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_TotalWeightInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_TotalWeightInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_TotalWeightInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_TotalWeightInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_TotalWeightInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_TotalWeightUnitInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_TotalWeightUnitInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_TotalWeightUnitInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_TotalWeightUnitInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_TotalWeightUnitInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_TotalNoOfPacksInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_TotalNoOfPacksInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_TotalNoOfPacksInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_TotalNoOfPacksInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_TotalNoOfPacksInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_TotalNoOfPacksPackTypeInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_ShipmentIncoTermInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_ShipmentIncoTermInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_ShipmentIncoTermInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_ShipmentIncoTermInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_ShipmentIncoTermInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestJE_AgentsReferenceInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, JobDeclaration.JE_AgentsReferenceInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, JobDeclaration.JE_AgentsReferenceInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, JobDeclaration.JE_AgentsReferenceInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, JobDeclaration.JE_AgentsReferenceInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();

			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_HAWB = "999";
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			uPECusHAWB = Factory.New<UPECusHAWB>();

			AssertEquals("999", ((UPECusHAWB)JobDeclaration.BusinessObjectsWithRelatedNotes[0]).CS_HAWB);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();

			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_HAWB = "999";
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			uPECusHAWB = Factory.New<UPECusHAWB>();

			AssertEquals("999", ((UPECusHAWB)JobDeclaration.BusinessObjectsWithRelatedEvents[0]).CS_HAWB);
		}

		public void TestOnlyChecksReadOnlyIfNotBatchProcessorUser()
		{
			JobDeclaration.ReadOnly = true;
			AssertEquals("Sanity check", false, Env.CurrentUser.IsBatchProcessor);
			AssertEquals("Sanity check", true, JobDeclaration.ReadOnly);

			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				AssertEquals("Sanity check", true, Env.CurrentUser.IsBatchProcessor);
				AssertEquals("Should not be read-only and should not be calling base", false, JobDeclaration.ReadOnly);
			}
		}

		public void TestChangingImporterDoesNotChangePortOfDestination()
		{
			JobDeclaration.JE_RL_NKFinalDestination = "ZACPT";
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			JobDeclaration.JE_OH_Importer = org.PK;
			AssertEquals("ZACPT", JobDeclaration.JE_RL_NKFinalDestination);
		}

		public void TestChangingSupplierDoesNotChangePortOfOrigin()
		{
			JobDeclaration.JE_RL_NKOrigin = "ZACPT";
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			JobDeclaration.JE_OH_Supplier = org.PK;
			AssertEquals("ZACPT", JobDeclaration.JE_RL_NKOrigin);
		}

		public void TestShimentRefIsShortTrackingNumber()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.WayBillShort = "12345678901";
			UPEDeclarationFromAirCargoCreator creator = new UPEDeclarationFromAirCargoCreator(uPECusHAWB);
			creator.Create(JobDeclaration, new NotificationBufferForTesting());
			AssertEquals("12345678901", ((ILineKey)JobDeclaration).ShipmentRef);
		}

		public void TestImportDateIsDeclarationDateOfArrival()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_ArrivalDate = new ZDateTime(2006, 1, 1);
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			UPEDeclarationFromAirCargoCreator creator = new UPEDeclarationFromAirCargoCreator(uPECusHAWB);
			creator.Create(JobDeclaration, new NotificationBufferForTesting());
			AssertEquals(new ZDateTime(2006, 1, 1), ((ILineKey)JobDeclaration).ImportDate);
		}

		public void TestConsigneePostCode()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_ArrivalDate = new ZDateTime(2006, 1, 1);
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_ConsigneePostcode = "CSPC";

			UPEDeclarationFromAirCargoCreator creator = new UPEDeclarationFromAirCargoCreator(uPECusHAWB);
			creator.Create(JobDeclaration, new NotificationBufferForTesting());
			AssertEquals("CSPC", ((ILineKey)JobDeclaration).ConsigneePostCode);

			uPECusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			uPECusHAWB.Consignee.MainAddress.OA_PostCode = "NWPC";
			AssertEquals("NWPC", ((ILineKey)JobDeclaration).ConsigneePostCode);
		}

		#region RelatedCusHAWBs

		public void TestRelatedCusHAWBs()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();

			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;

			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "999";
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;

			AssertEquals("999", JobDeclaration.RelatedCusHAWBs[0].CS_HAWB);

			Factory.Save();
			JobDeclaration.RelatedCusHAWBs[0].CS_HAWB = "888";
			AssertEquals(true, JobDeclaration.HasChanges);
		}

		public void TestResetRelatedCusHAWBs()
		{
			AssertEquals("PreCondition: 0", 0, JobDeclaration.RelatedCusHAWBs.Count);
			UPECusMAWB mAWB = Factory.New<UPECusMAWB>();
			UPECusHAWB hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew();
			hAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			AssertEquals("Should still be 0 because collection is cached", 0, JobDeclaration.RelatedCusHAWBs.Count);
			JobDeclaration.ResetRelatedCusHAWBs();
			AssertEquals("Should now be 1", 1, JobDeclaration.RelatedCusHAWBs.Count);
		}

		public void TestGetAssociatedCusHAWBs()
		{
			UPEJobDeclaration declaration = (UPEJobDeclaration)UPEJobDeclaration.New(Factory);
			UPECusHAWB cusHAWB1 = CreateCusHAWB(declaration);
			UPECusHAWB cusHAWB2 = CreateCusHAWB(declaration);
			UPECusHAWB cusHAWB3 = CreateCusHAWB(declaration);
			UPECusHAWB cusHAWB4 = CreateCusHAWB(declaration);
			UPECusHAWB[] cusHAWBs = declaration.GetAssociatedHAWBs();
			AssertEquals(4, cusHAWBs.Length);
			AssertEquals(4, declaration.RelatedCusHAWBs.Count);

			UPECusHAWB cusHAWB5 = CreateCusHAWB(declaration);
			cusHAWBs = declaration.GetAssociatedHAWBs();
			AssertEquals("Should not be cached", 5, cusHAWBs.Length);
			AssertEquals("Should be cached", 4, declaration.RelatedCusHAWBs.Count);
		}

		public void TestFirstCusHAWB_RelatedCusHAWBsLoaded()
		{
			UPEJobDeclaration declaration = (UPEJobDeclaration)UPEJobDeclaration.New(Factory);
			UPECusHAWB cusHAWB1 = CreateCusHAWB(declaration);
			object lazyLoadRelatedCusHAWBs = declaration.RelatedCusHAWBs;
			int loadCount = Factory.DatabaseLoadCount;
			AssertEquals(cusHAWB1, declaration.FirstCusHAWB);
			AssertEquals("Should not be retrieving it from factory directly if RelatedCusHAWBs has been loaded", loadCount, Factory.DatabaseLoadCount);
		}

		[ExpectNoExceptions]
		public void TestProcessGSSiMessagingWhenFirstCusHAWBIsNull()
		{
			var dec = Factory.New<UPEJobDeclaration>();
			Factory.Save();

			//P4_CustomsStatusInfo HasChanges only is true if it's saved to DB first.
			dec.CurrentQueue.P4_CustomsStatus = (ZString)ReasonCodeDescriptionPairList.Codes.EN_ShipperMatchingError;
			Factory.Save();
		}

		public void TestFirstCusHAWB_ReturnsNullAfterJobDecDeleted()
		{
			UPEJobDeclaration declaration = (UPEJobDeclaration)UPEJobDeclaration.New(Factory);
			UPECusHAWB cusHAWB = CreateCusHAWB(declaration);

			AssertEquals(cusHAWB, declaration.FirstCusHAWB);
			cusHAWB.Delete();
			AssertEquals(null, declaration.FirstCusHAWB);
		}

		public void TestFirstCusHAWB_RelatedCusHAWBsNotLoaded()
		{
			UPEJobDeclaration declaration = (UPEJobDeclaration)UPEJobDeclaration.New(Factory);
			UPECusHAWB cusHAWB1 = CreateCusHAWB(declaration);
			int loadCount = Factory.DatabaseLoadCount;
			AssertEquals(cusHAWB1, declaration.FirstCusHAWB);
			Assert("Should be retrieving it directly from factory", loadCount < Factory.DatabaseLoadCount);
		}

		UPECusHAWB CreateCusHAWB(UPEJobDeclaration declaration)
		{
			UPECusHAWB result = Factory.New<UPECusHAWB>();
			result.CS_JE_CustomsFormalEntry = declaration.PK;
			return result;
		}

		[ExpectNoExceptions]
		public void TestServiceLevelRefreshesRelatedCusHAWBS_Issue30485()
		{
			var uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			AssertNull(uPEJobDeclaration.FirstCusHAWB);
			var uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			var hAWBs = uPEJobDeclaration.RelatedCusHAWBs;
			Factory.Save();
			uPECusHAWB.CS_JE_CustomsFormalEntry = uPEJobDeclaration.PK;
			var serviceLevel = uPEJobDeclaration.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client);
		}

		#endregion

		#region Queue Moving

		public void TestIsInCustomsBondingQueue()
		{
			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.EIR;
			AssertEquals(false, JobDeclaration.IsInCustomsBondingQueue);

			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;
			AssertEquals(true, JobDeclaration.IsInCustomsBondingQueue);
		}

		public void TestIsInQuarantineHold()
		{
			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;
			JobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			AssertEquals(false, JobDeclaration.IsInQuarantineHoldQueue);

			JobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.BCA;
			JobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			AssertEquals(false, JobDeclaration.IsInQuarantineHoldQueue);

			JobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			AssertEquals(true, JobDeclaration.IsInQuarantineHoldQueue);
		}

		public void TestQueueIsMovedWhenCMREntryStatusIsSet()
		{
			AssertQueueMoving(CMRImportEntryAdvice.Processing.Code, DeclarationQueueCodeDescriptionPairList.Codes.Pending);
			AssertQueueMoving(CMRImportEntryAdvice.Finalised.Code, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving(CMRImportEntryAdvice.Clear.Code, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving(CMRImportEntryAdvice.ATDReceived.Code, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving(CMRImportEntryAdvice.Held.Code, DeclarationQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
			AssertQueueMoving(CMRImportEntryAdvice.MultiStatus.Code, DeclarationQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
			AssertQueueMoving(CMRImportEntryAdvice.Rejected.Code, DeclarationQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
			AssertQueueMoving(CMRImportEntryAdvice.Withdrawn.Code, DeclarationQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
		}

		public void TestQueueIsMovedWhenMessageStatusIsSet()
		{
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.AwaitingFormalLodge.Code, DeclarationQueueCodeDescriptionPairList.Codes.Submitted, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, StatusCodeDescriptionPairList.EmptyStatus);
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.AwaitingPayment.Code, DeclarationQueueCodeDescriptionPairList.Codes.Submitted, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, StatusCodeDescriptionPairList.EmptyStatus);
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.AwaitingSAC.Code, DeclarationQueueCodeDescriptionPairList.Codes.Submitted, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, StatusCodeDescriptionPairList.EmptyStatus);
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.AwaitingAmendment.Code, DeclarationQueueCodeDescriptionPairList.Codes.Submitted, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, StatusCodeDescriptionPairList.EmptyStatus);
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.FailAmendment.Code, DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "");
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.FailFormalLodge.Code, DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "");
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.FailPayment.Code, DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "");
			AssertQueueMoving_MessageStatus(CustomsEntryStatus.FailSAC.Code, DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "");
		}

		void AssertQueueMoving(ZString entryStatus, ZString expectedQueueName)
		{
			AssertQueueMoving(entryStatus, expectedQueueName, "", "");
		}

		void AssertQueueMoving(ZString entryStatus, ZString expectedQueueName, ZString expectedReasonCode)
		{
			AssertQueueMoving(entryStatus, expectedQueueName, expectedReasonCode, "");
		}

		void AssertQueueMoving(ZString entryStatus, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			JobDeclaration.JE_EntryStatus = entryStatus;
			AssertEquals("CurrentQueue has been moved to the wrong Queue", expectedQueueName, JobDeclaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals("CurrentQueue has been moved with the wrong reason code", expectedReasonCode, JobDeclaration.CurrentQueue.P4_CustomsStatus);
			AssertEquals("CurrentQueue has been moved with the wrong status code", expectedStatusCode, JobDeclaration.CurrentQueue.P4_CustomsSubStatus);
		}

		void AssertQueueMoving_MessageStatus(ZString messageStatus, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			JobDeclaration.JE_MessageStatus = messageStatus;
			AssertEquals("CurrentQueue has been moved to the wrong Queue", expectedQueueName, JobDeclaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals("CurrentQueue has been moved with the wrong reason code", expectedReasonCode, JobDeclaration.CurrentQueue.P4_CustomsStatus);
			AssertEquals("CurrentQueue has been moved with the wrong status code", expectedStatusCode, JobDeclaration.CurrentQueue.P4_CustomsSubStatus);
		}

		#endregion

		#region Organisation Rematching

		public void TestLogOrganisationRematch()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			JobDeclaration.JE_OH_Importer = organisation1.PK;
			JobDeclaration.JE_OH_Supplier = organisation2.PK;
			Factory.Save();

			JobDeclaration.JE_OH_Importer = organisation2.PK;
			JobDeclaration.JE_OH_Supplier = organisation1.PK;
			Factory.Save();

			AssertRematch(UPEOrgRematch.OrgTypes.Importer, organisation1.PK, organisation2.PK);
			AssertRematch(UPEOrgRematch.OrgTypes.Supplier, organisation2.PK, organisation1.PK);
		}

		void AssertRematch(ZString organisationType, ZGuid rematchedFromOrg, ZGuid rematchedToOrg)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ClientOrgRematchSchema.T5_JE, SQLComparisonOperator.Equal, JobDeclaration.PK);
			query.AddToFilter(ClientOrgRematchSchema.T5_OrganisationType, SQLComparisonOperator.Equal, organisationType);
			UPEOrgRematch rematch = Factory.LoadTop1<UPEOrgRematch>(query);

			AssertEquals("T5_OrganisationType", organisationType, rematch.T5_OrganisationType);
			AssertEquals("T5_OH_RematchedFromOrg", rematchedFromOrg, rematch.T5_OH_RematchedFromOrg);
			AssertEquals("T5_OH_RematchedToOrg", rematchedToOrg, rematch.T5_OH_RematchedToOrg);
		}

		#endregion

		#region Refund Processing

		public void TestClientRefund()
		{
			AssertNull("JobDeclaration.Refund", JobDeclaration.Refund);
			AssertNotNull("JobDeclaration.Refund", JobDeclaration.RefundManager);
			AssertNotNull("JobDeclaration.Refund", JobDeclaration.RefundWrapper);
			ClientRefund newRefund = JobDeclaration.RefundManager.CreateClientRefund();
			Factory.Save();

			UPEJobDeclaration loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(JobDeclaration.PK);
			ClientRefund loadedRefund = loadedDeclaration.Refund;
			AssertEquals("Should return the correct RefundProcessing object", newRefund.PK, loadedRefund.PK);
		}

		public void TestClientRefund_DefaultUserNameToAssignedUser()
		{
			UPEJobDeclaration declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			declaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "CV";
			ClientRefund refund = declaration.RefundManager.CreateClientRefund();
			AssertEquals("Should default the refund user name to the assigned to user", "CV", refund.T10_GS_NKCreatedUser);
		}

		public void TestClientRefund_DefaultUserNameToCurrentUserIfNoAssignedUser()
		{
			UPEJobDeclaration declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			JobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "";
			ClientRefund refund = declaration.RefundManager.CreateClientRefund();
			AssertEquals("Should default the refund user name to current user is no assigned to", GlbStaff.CurrentUser.GS_Code, refund.T10_GS_NKCreatedUser);
		}

		public void TestRefundEnquiry()
		{
			AssertEquals(false, JobDeclaration.IsRefundEnquiry);
			AssertEquals(false, JobDeclaration.CurrentQueue.P4_CustomFlag4);
			JobDeclaration.IsRefundEnquiry = true;
			AssertEquals(true, JobDeclaration.IsRefundEnquiry);
			AssertEquals(true, JobDeclaration.CurrentQueue.P4_CustomFlag4);
		}

		public void TestIsRefundProcessed()
		{
			AssertEquals(false, JobDeclaration.IsRefundProcessed);
			AssertEquals(false, JobDeclaration.CurrentQueue.P4_CustomFlag5);
			JobDeclaration.IsRefundProcessed = true;
			AssertEquals(true, JobDeclaration.IsRefundProcessed);
			AssertEquals(true, JobDeclaration.CurrentQueue.P4_CustomFlag5);
		}

		[TestDate(2007, 7, 7)]
		public void TestTickUntickNotesForRefundEnquiry()
		{
			AddRelatedCalloutToDeclaration();
			Factory.Save();
			JobDeclaration.RefundManager.CreateClientRefund();

			JobDeclaration.Refund.T10_ControlNumber = ZString.Empty;
			Assert("T10_ControlNumber", JobDeclaration.Refund.T10_ControlNumber.IsEmpty);
			PopulateClientRefundWithData(JobDeclaration.Refund);
			JobDeclaration.Refund.PostRefund = true;

			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();
			Assert("Control number must be generated for refund enquiry", !JobDeclaration.Refund.T10_ControlNumber.IsEmpty);
			AssertEquals("Refund Enquiry Date Opend", new ZDateTime(2007, 07, 07), JobDeclaration.Refund.T10_DateCreated);

			Assert("Declaration is marked as Refund Enquiry", JobDeclaration.IsRefundEnquiry);
			AssertEquals("FinanceNote should be generated", 1, JobDeclaration.Callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.FinanceNote.Description).Length);
			AssertEquals("RefundEnquiry TICKED", 1, JobDeclaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description).Length);
			Assert("RefundEnquiry Message TICKED", JobDeclaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description)[0].ST_NoteText.Contains("TICKED"));
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			JobDeclaration.Refund.ProcessRefund = true;
			ZString controlNumber = JobDeclaration.Refund.T10_ControlNumber;
			JobDeclaration.Refund.T10_IsRefundRejected = true;
			CreateStaffWithEmailGroupAndSetRegistry();
			Factory.Save();
			AssertEquals("Control number can't be changed", controlNumber, JobDeclaration.Refund.T10_ControlNumber);
			AssertEquals("FinanceNote should be generated only once", 1, JobDeclaration.Callout.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.FinanceNote.Description).Length);
			AssertUntickNote();
			AssertEquals("2 email should be send. To the CLS and RefundEnquiryNotification Group", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		void AssertUntickNote()
		{
			AssertEquals("RefundEnquiry TICKED/UNTICKED/Refund", 3, JobDeclaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description).Length);
			ZBool unticked = false;
			foreach (StmNote note in JobDeclaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description))
			{
				unticked = note.ST_NoteText.Contains("UN-TICKED");
				if (unticked)
				{
					break;
				}
			}
			Assert("RefundEnquiry UNTICKED", unticked);
		}

		void CreateStaffWithEmailGroupAndSetRegistry()
		{
			GlbGroup glbGroup = Factory.New<GlbGroup>();
			glbGroup.GG_Code = "XXX";
			UPEDataRegistry.Instance.RefundNotificationGroup = glbGroup.PK.ToGuid();
			GlbStaff glbStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			glbStaff.Groups.Add(glbGroup);
			glbStaff.GS_EmailAddress = "test@edi.com.au";
			Factory.Save();
		}

		public void TestIsRefundProcessedInfo()
		{
			AssertEquals(true, JobDeclaration.IsRefundProcessedInfo.ReadOnly);
		}

		#region IRefundEnquiry Test

		public void TestRefund()
		{
			var declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			Callout callout = AddCallout(declaration);
			IRefundEnquiry owner = declaration;
			declaration.Notes.RemoveAndDeleteAll();

			declaration.RefundManager.CreateClientRefund();
			AssertNotNull("Declaration does have a Refund", declaration.Refund);
			AssertNotNull("RelatedOwner", declaration.RelatedOwner);
			Assert("RelatedOwner is Callout", object.ReferenceEquals(callout, owner.RelatedOwner));
			AssertEquals("OwnerColumn", ClientRefundSchema.T10_JE, owner.OwnerColumn);

			Assert("Doesn't have Notes", !declaration.Notes.HasNotes);
			owner.AddRefundNote();
			Assert("Does have Notes", declaration.Notes.HasNotes);
			AssertEquals("Has 1 FinanceNote", 1, declaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description).Length);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			owner.SendNotificationEmail();
			AssertEquals("Doesn't send emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			declaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = staff.GS_Code;
			staff.GS_EmailAddress = "test@test.com";

			owner.SendNotificationEmail();
			AssertEquals("Doesn't send emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("1 Recipient", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("Recipient", staff.GS_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0]);
		}

		Callout AddCallout(UPEJobDeclaration declaration)
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			declaration.ResetRelatedCusHAWBs();
			Factory.Save();
			return callout;
		}

		#endregion

		#endregion

		#region Alerts

		public void TestHasAlerts()
		{
			AssertEquals(false, JobDeclaration.HasAlerts);
			StringCollection alertsList = JobDeclaration.AlertsList;
			AssertEquals(false, JobDeclaration.HasAlerts);
			JobDeclaration.AlertsList.Add("Test");
			AssertEquals(true, JobDeclaration.HasAlerts);
		}

		public void TestAlertsList_SplitShipment()
		{
			AssertEquals("Pre-Condition", 0, JobDeclaration.AlertsList.Count);

			JobDeclaration.JE_EntryStatus = CustomsEntryStatus.ClearLodge.Code;
			JobDeclaration.ResetAlertsList();
			AssertEquals(0, JobDeclaration.AlertsList.Count);

			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			JobDeclaration.ResetAlertsList();
			AssertEquals(0, JobDeclaration.AlertsList.Count);

			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			JobDeclaration.ResetAlertsList();
			AssertEquals(0, JobDeclaration.AlertsList.Count);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			JobDeclaration.Logs.AddNew(Events.EditedARecord, "Masterbill Added =");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			JobDeclaration.ResetAlertsList();
			AssertEquals(1, JobDeclaration.AlertsList.Count);
			AssertEquals("Split Shipment", JobDeclaration.AlertsList[0]);
		}

		public void TestAlertsList_FreeDomicile()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			uPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			JobDeclaration.ResetAlertsList();
			AssertEquals("Free Domicile", JobDeclaration.AlertsList[0]);
		}

		public void TestAlertsList_ForUnreadRelatedDocuments()
		{
			AssertEquals("No alerts initially for the test", 0, JobDeclaration.AlertsList.Count);

			DocTypeWithForceUserToRead.Factory.Save();
			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
			{
				JobDeclaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "Test.pdf", DocTypeWithForceUserToRead.RT_DocType);
				JobDeclaration.DocManagerInfo.Save();
			}

			JobDeclaration.ResetAlertsList();
			AssertEquals("There are unread eDocs attached to this job", JobDeclaration.AlertsList[0]);
		}

		//todo Bob
		//
		//		public void TestAlertsList_LocalCharges()
		//		{
		//			UPEJobDeclarationWithDummyCharges Declaration = (UPEJobDeclarationWithDummyCharges) Factory.New(typeof(UPEJobDeclarationWithDummyCharges));
		//		
		//			UPECusHAWB UPECusHAWB = (UPECusHAWB) Factory.New(typeof(UPECusHAWB));
		//			UPECusHAWB.CS_JE_CustomsFormalEntry = Declaration.PK;
		//			UPEDataRegistry.Instance.SecurityFeeAmount = 3000m;
		//			Declaration.ResetAlertsList();
		//			AssertEquals("Local Charges are Above $3000", Declaration.AlertsList[0]);
		//		}

		public void TestAlertsList_PreferredCustomer()
		{
			AssertEquals("Pre-Condition", 0, JobDeclaration.AlertsList.Count);

			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			JobDeclaration.Importer.LetterOfAuthorityExpirationDate = ZDateTime.Now.AddDays(100);

			OrgDebtorGroup debtorGroup = Factory.New<OrgDebtorGroup>();
			debtorGroup.OJ_Code = "2";
			JobDeclaration.Importer.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			JobDeclaration.ResetAlertsList();
			AssertEquals(1, JobDeclaration.AlertsList.Count);

			AssertEquals("Importer is a Preferred Customer", JobDeclaration.AlertsList[0]);
		}

		public void TestAlertsList_HighClaimer()
		{
			AssertEquals("Pre-Condition", 0, JobDeclaration.AlertsList.Count);

			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			JobDeclaration.Importer.LetterOfAuthorityExpirationDate = ZDateTime.Now.AddDays(100);
			JobDeclaration.Importer.IsHighClaimer = true;

			JobDeclaration.ResetAlertsList();
			AssertEquals(1, JobDeclaration.AlertsList.Count);

			AssertEquals("Importer is a High Claimer", JobDeclaration.AlertsList[0]);
		}

		public void TestAlertsList_IsPreReleaseNotification()
		{
			var importer = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			JobDeclaration.JE_OH_Importer = importer.PK;
			importer.IsPreReleaseContactFeeApplicable = true;
			JobDeclaration.ResetAlertsList();
			AssertEquals("Importer has Pre-Release Notification Flag Activated", true, JobDeclaration.AlertsList.Contains("Pre-Release Notification"));
		}

		public void TestAlertsList_LetterOfAuthorityExpiration()
		{
			AssertEquals("Pre-Condition", 0, JobDeclaration.AlertsList.Count);

			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			JobDeclaration.Importer.LetterOfAuthorityExpirationDate = ZDateTime.Empty;
			JobDeclaration.ResetAlertsList();
			AssertEquals("There should be an alert if the expiry date isn't set", 1, JobDeclaration.AlertsList.Count);
			AssertEquals("There should be an alert if the expiry date isn't set", "No Letter of Authority on file", JobDeclaration.AlertsList[0]);

			JobDeclaration.Importer.LetterOfAuthorityExpirationDate = ZDateTime.Now.AddDays(31);
			JobDeclaration.ResetAlertsList();
			AssertEquals("There shouldn't be an alert if it will be over 30 days before it expires", 0, JobDeclaration.AlertsList.Count);

			JobDeclaration.Importer.LetterOfAuthorityExpirationDate = ZDateTime.Now.AddDays(25);
			JobDeclaration.ResetAlertsList();
			AssertEquals("There should be an alert if it expires within 30 days", 1, JobDeclaration.AlertsList.Count);
			AssertEquals("There should be an alert if it expires within 30 days", "Letter of Authority will expire in 25 days", JobDeclaration.AlertsList[0]);

			JobDeclaration.Importer.LetterOfAuthorityExpirationDate = ZDateTime.Now;
			JobDeclaration.ResetAlertsList();
			AssertEquals("There should be an alert if it has expired today", 1, JobDeclaration.AlertsList.Count);
			AssertEquals("There should be an alert if it has expired today", "Letter of Authority expired today", JobDeclaration.AlertsList[0]);

			JobDeclaration.Importer.LetterOfAuthorityExpirationDate = ZDateTime.Now.AddDays(-2);
			JobDeclaration.ResetAlertsList();
			AssertEquals("There should be an alert if it has expired", 1, JobDeclaration.AlertsList.Count);
			AssertEquals("There should be an alert if it has expired", "Letter of Authority expired 2 days ago", JobDeclaration.AlertsList[0]);
		}

		public void TestAlertsList_NoAlert()
		{
			AssertEquals("Pre-Condition", 0, JobDeclaration.AlertsList.Count);

			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			JobDeclaration.ResetAlertsList();
			AssertEquals("no alert", 0, JobDeclaration.AlertsList.Count);
		}

		#region PreReleaseCharge

		public void TestPreReleaseCharge_BaseCharge()
		{
			SetupPreReleaseNotificationRegistries();

			var importer = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			JobDeclaration.JE_OH_Importer = importer.PK;
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Rejected.Code;

			var entry = JobDeclaration.CustomsEntryHeaders.AddNew();
			entry.MergedLines.AddNew();

			Assert("Pre-Release Flag is false", !importer.IsPreReleaseContactFeeApplicable);

			var contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			var lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNull("Shipment should not contain Contact Fee", contactFee);
			AssertNull("Shipment should not contain Line Charge Fee", lineCharge);

			importer.IsPreReleaseContactFeeApplicable = true;

			contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNull("Shipment should not contain Contact Fee as declaration is not completed", contactFee);
			AssertNull("Shipment should not contain Line Charge Fee as declaration is not completed", lineCharge);

			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;

			contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			AssertNotNull("Shipment contains Contact Fee", contactFee);
			AssertEquals("Contact Fee Amount", contactFee.GrossAmount, 50m);

			lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNotNull("Shipment contains Line Charge Fee", lineCharge);
			AssertEquals("Charge Per Line Fee Amount", lineCharge.GrossAmount, 80m);

			UPEDataRegistry.Instance.EntryLineChargeBaseAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m);
			contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			AssertNotNull("Shipment contains Contact Fee", contactFee);

			lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNull("Line Charge Fee", lineCharge);
		}

		public void TestPreReleaseCharge_ByNumOfEntryLines()
		{
			SetupPreReleaseNotificationRegistries();
			var importer = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			importer.IsPreReleaseContactFeeApplicable = true;

			JobDeclaration.JE_OH_Importer = importer.PK;
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			var entry = JobDeclaration.CustomsEntryHeaders.AddNew();
			for (int i = 1; i <= 20; i++)
			{ entry.MergedLines.AddNew(); }

			var contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			var lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNotNull("Shipment contains Contact Fee", contactFee);
			AssertNotNull("Shipment contains Line Charge Fee", lineCharge);

			AssertEquals("Contact Fee Amount", contactFee.GrossAmount, 50m);
			AssertEquals("Charge Per Line Fee Amount: 80 + 10 * 4", 120m, lineCharge.GrossAmount);

			UPEDataRegistry.Instance.EntryLineChargeBaseAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m);
			contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);

			AssertNotNull("Shipment contains Contact Fee", contactFee);
			AssertNotNull("Shipment contains Line Charge Fee", lineCharge);
			AssertEquals("Contact Fee Amount", contactFee.GrossAmount, 50m);
			AssertEquals("Charge Per Line Fee Amount: 10 * 4", 40m, lineCharge.GrossAmount);

			importer.IsPreReleaseContactFeeApplicable = false;

			contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNull("Shipment should not contain Contact Fee", contactFee);
			AssertEquals("Charge Per Line Fee Amount: 10 * 4", 40m, lineCharge.GrossAmount);
		}

		public void TestPreReleaseCharge_CappedCharge()
		{
			SetupPreReleaseNotificationRegistries();
			var importer = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			importer.IsPreReleaseContactFeeApplicable = true;

			JobDeclaration.JE_OH_Importer = importer.PK;
			JobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			var entry = JobDeclaration.CustomsEntryHeaders.AddNew();
			for (int i = 1; i <= 200; i++)
			{ entry.MergedLines.AddNew(); }

			var contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			var lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNotNull("Shipment contains Contact Fee", contactFee);
			AssertNotNull("Shipment contains Line Charge Fee", contactFee);

			AssertEquals("Contact Fee Amount", 50m, contactFee.GrossAmount);
			AssertEquals("Charge Per Line Fee Amount- Capped amount should be charged instead", 400m, lineCharge.GrossAmount);

			importer.IsPreReleaseContactFeeApplicable = false;

			contactFee = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ContactFeeChargeType);
			lineCharge = JobDeclaration.ChargesData.FirstOrDefault(x => x.TypeCode == ChargePerLineChargeType);
			AssertNull("Shipment should not contain Contact Fee", contactFee);
			AssertNotNull("Shipment contains Line Charge Fee", lineCharge);

			AssertEquals("Charge Per Line Fee Amount- Capped amount should be charged instead", 400m, lineCharge.GrossAmount);
		}

		static string ContactFeeChargeType
		{
			get { return ((int)ShipmentChargeTypeCode.ContactFee).ToString(); }
		}

		static string ChargePerLineChargeType
		{
			get { return ((int)ShipmentChargeTypeCode.ChargePerLine).ToString(); }
		}
		#endregion

		RefDocType DocTypeWithForceUserToRead
		{
			get
			{
				if (fDocTypeWithForceUserToRead == null)
				{
					fDocTypeWithForceUserToRead = Factory.New<RefDocType>();
					fDocTypeWithForceUserToRead.RT_ForceUserToRead = true;
					fDocTypeWithForceUserToRead.RT_ReferenceType = DocumentImaging.DocManagerReferenceTypes.All;
					fDocTypeWithForceUserToRead.RT_DocType = "FUR";
				}
				return fDocTypeWithForceUserToRead;
			}
		}
		RefDocType fDocTypeWithForceUserToRead;

		#endregion

		#region Notes

		public void TestNoteTypeCollection()
		{
			AssertCollectionContains(PredefinedNoteTypes.Instance.DeliveryInstructionsNote, JobDeclaration.NoteTypes);
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.DeclarationNote, JobDeclaration.NoteTypes);
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.RefundNote, JobDeclaration.NoteTypes);
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.ManualBillNote, JobDeclaration.NoteTypes);
		}

		#endregion

		#region IUPEDocumentSupportable

		public void TestDocumentSupporter()
		{
			AssertEquals("DocumentSupporter of correct type", typeof(UPEJobDeclarationDocumentSupporter), JobDeclaration.DocumentSupporter.GetType());
		}

		public void TestOnPrintBatchItemQueued()
		{
			UPECusHAWB cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;

			IUPEDocumentSupportable documentSupportable = JobDeclaration;
			JobDeclaration.FirstCusHAWB.PrintBatchItemQueued += new PrintBatchItemQueuedEventHandler(OnFirstCusHAWB_PrintBatchItemQueued);

			documentSupportable.OnPrintBatchItemQueued(new PrintBatchItemQueuedEventArgs(null, false));
			AssertEquals("PrintBatchItemQueued should be fired", true, OnFirstCusHAWB_PrintBatchItemQueuedCalled);
			OnFirstCusHAWB_PrintBatchItemQueuedCalled = false;

			JobDeclaration.FirstCusHAWB.Delete();
			documentSupportable.OnPrintBatchItemQueued(new PrintBatchItemQueuedEventArgs(null, false));
			AssertEquals("PrintBatchItemQueued should not be fired, and no exception raised if there is no attached CusHAWB", false, OnFirstCusHAWB_PrintBatchItemQueuedCalled);
		}

		bool OnFirstCusHAWB_PrintBatchItemQueuedCalled;
		void OnFirstCusHAWB_PrintBatchItemQueued(object sender, PrintBatchItemQueuedEventArgs e)
		{
			OnFirstCusHAWB_PrintBatchItemQueuedCalled = true;
		}

		#region EIR Raised

		[TestDate(2006, 5, 5)]
		public void TestOpenInEIRQueueAndSaveInEIRQueue_EIRRaisedSelected()
		{
			JobDeclaration.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			JobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.S1_ShipperConsigneeDetailsInsufficient;
			JobDeclaration.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();

			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();

			UPEJobDeclaration jobDeclarationReLoaded = factoryForTest.Load<UPEJobDeclaration>(JobDeclaration.PK);

			EIRBeenRaisedAnswer = true;
			try
			{
				jobDeclarationReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				jobDeclarationReLoaded.CurrentQueue.P4_Reason = "Test";
				jobDeclarationReLoaded.JE_GoodsDescription = "TEST";
				factoryForTest.Save();

				AssertEquals(Events.EditedARecord.Code, jobDeclarationReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals("EIR Raised", jobDeclarationReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(new ZDateTime(2006, 5, 5), jobDeclarationReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				jobDeclarationReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
			}
		}

		void OnAskHasEIRBeenRaised(object sender, CancelEventArgs eventArgs)
		{
			eventArgs.Cancel = !EIRBeenRaisedAnswer;
		}
		bool EIRBeenRaisedAnswer;

		#endregion

		#endregion

		protected override Type ExpectedMetadataType => typeof(Metadata.UPEJobDeclaration);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>();

		protected override void SetUp()
		{
			base.SetUp();

			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			Classifier1 = ClassifierStaffGroup.Staff.AddNew();
			Classifier1.GS_Code = "c1";
			Classifier1.GS_LoginName = "c1";
			Classifier2 = ClassifierStaffGroup.Staff.AddNew();
			Classifier2.GS_Code = "c2";
			Classifier2.GS_LoginName = "c2";
			Classifier3 = ClassifierStaffGroup.Staff.AddNew();
			Classifier3.GS_Code = "c3";
			Classifier3.GS_LoginName = "c3";

			SetupPreReleaseFlag();
			Factory.Save();
		}

		void AddRelatedCalloutToDeclaration()
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			callout.CurrentQueue.P4_CustomsReason = ReasonCodeDescriptionPairList.Codes._R0_Rebill;
			callout.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			callout.RunPreSaveValidation();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "boberman@bob.com";
			JobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = staff.GS_Code;
			JobDeclaration.ResetRelatedCusHAWBs();

			Factory.Save();
		}

		void PopulateClientRefundWithData(ClientRefund refund)
		{
			refund.T10_EnquiryContact = "Contact";
			refund.T10_EnquiryPhoneNumber = "123";
			refund.T10_EnquiryDetails = "T10_EnquiryDetails";
			refund.T10_EnquiryRaisedBy = refund.RaisedByList[0].Code;
		}

		GlbStaff Classifier1;
		GlbStaff Classifier2;
		GlbStaff Classifier3;

		GlbGroup ClassifierStaffGroup
		{
			get
			{
				if (fClassifierStaffGroup == null)
				{
					fClassifierStaffGroup = Factory.New<GlbGroup>();
					fClassifierStaffGroup.GG_Code = "ABC";
					UPEDataRegistry.Instance.ClassifierStaffGroupCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
				}
				return fClassifierStaffGroup;
			}
		}
		GlbGroup fClassifierStaffGroup;

		GlbGroup BillingNotificationGroup
		{
			get
			{
				if (fBillingNotificationGroup == null)
				{
					fBillingNotificationGroup = Factory.New<GlbGroup>();
					fBillingNotificationGroup.GG_Code = "UPW";

					GlbStaff staff = fBillingNotificationGroup.Staff.AddNew();
					staff.GS_EmailAddress = "clinty@edi.com.au";
					staff.GS_Code = "ZAC";
				}
				return fBillingNotificationGroup;
			}
		}
		GlbGroup fBillingNotificationGroup;

		GlbBranch MelBranch
		{
			get
			{
				if (melBranch == null)
				{
					melBranch = Factory.New<GlbBranch>();
					melBranch.GB_GC = GlbCompany.CurrentCompany.PK;
					melBranch.GB_Code = "TML";
					melBranch.GB_RL_NKHomePort = "AUMEL";
					Factory.Save();
				}

				return melBranch;
			}
		}
		GlbBranch melBranch;

		GlbBranch PerBranch
		{
			get
			{
				if (perBranch == null)
				{
					perBranch = Factory.New<GlbBranch>();
					perBranch.GB_GC = GlbCompany.CurrentCompany.PK;
					perBranch.GB_Code = "TPR";
					perBranch.GB_RL_NKHomePort = "AUPER";
					Factory.Save();
				}

				return perBranch;
			}
		}
		GlbBranch perBranch;

		UPEJobDeclarationWithDummyCharges JobDeclaration
		{
			get
			{
				if (fJobDeclaration == null)
				{
					fJobDeclaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
					fJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				}
				return fJobDeclaration;
			}
		}
		UPEJobDeclarationWithDummyCharges fJobDeclaration;

		UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.New<UPECusHAWB>();
					fCusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
				}
				return fCusHAWB;
			}
		}
		UPECusHAWB fCusHAWB;

		void SetupPreReleaseFlag()
		{
			var testTemplate = Factory.New<ProcessTaskTemplate>();
			testTemplate.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			testTemplate.P0_Name = "test template";

			var preReleaseFlag = testTemplate.GenCustomColumnDefinitions.AddNew();
			preReleaseFlag.XC_Name = UPEOrgHeader.PreReleaseNotificationFieldName;
			preReleaseFlag.XC_Type = AddOnColumnDataType.Codes.Boolean;
		}

		void SetupPreReleaseNotificationRegistries()
		{
			UPEDataRegistry.Instance.PreReleaseChargeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			UPEDataRegistry.Instance.EntryLineChargeCappedAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 400m);
			UPEDataRegistry.Instance.PerLineChargeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4m);
			UPEDataRegistry.Instance.LinesExemptedFromLineCharge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			UPEDataRegistry.Instance.EntryLineChargeBaseAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 80);
			UPEDataRegistry.Instance.ContactFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
		}

		ZInt LOAPrinted(UPEJobDeclaration declaration)
		{
			var query = new ZDBOnlyQuery(typeof(UPEPrintBatch));
			query.AddToFilter(JoinCondition.And, ClientPrintBatchSchema.T7_BatchType, SQLComparisonOperator.Equal, UPEPrintBatchTypes.Codes.UPSLetterOfAuthority);
			var relatedUPEPrintBatchs = new ZDBOnlySubQuery(typeof(UPEPrintBatchItem), ClientPrintBatchItemSchema.T6_T7);
			relatedUPEPrintBatchs.AddToFilter(JoinCondition.And, ClientPrintBatchItemSchema.T6_SU, new UPEDocumentMenuItemLoader(Factory).LoadLetterOfAuthority().PK);
			relatedUPEPrintBatchs.AddToFilter(JoinCondition.And, ClientPrintBatchItemSchema.T6_ParentID, declaration.Importer.PK);
			query.AddSubQuery(relatedUPEPrintBatchs, JoinCondition.And);
			return Factory.GetDatabaseCount(typeof(UPEPrintBatch), query);
		}
	}
}
