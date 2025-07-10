using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	public sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		public void TestGetUniqueNumberForAccountingIntegration() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "28578139";
			entryHeader.ConsolidatedEntryMemberID = 1;
			AssertEquals("UniqueNumber for non-consolidated entry", "28578139", ((IAccInvoiceDataProvider)entryHeader).UniqueNumber);

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(new BusinessObjectFactory(), 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			AssertEquals("Consolidated entry doesn't have EntryNumber", "", ((IAccInvoiceDataProvider)leadDeclarationEntryHeader).UniqueNumber);

			leadDeclarationEntryHeader.EntryNumber = "28578139";
			leadDeclarationEntryHeader.ConsolidatedEntryMemberID = 1;
			AssertEquals("UniqueNumber for consolidated entry", "28578139-1", ((IAccInvoiceDataProvider)leadDeclarationEntryHeader).UniqueNumber);
		});

		public void TestIntegrateIfNecessary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "44051192";
			AssertCostAndRevenuePostedByAccountingIntegration(entry, "44051192");
		}

		public void TestIntegrateIfNecessary_ConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var declaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.EntryHeader;
			entry.EntryNumber = "44051192";
			entry.ConsolidatedEntryMemberID = 1;
			AssertEquals("Declaration is consolidated", true, ConsolidatedDeclaration.IsConsolidated(declaration));
			AssertCostAndRevenuePostedByAccountingIntegration(entry, "44051192-1");
		}

		void AssertCostAndRevenuePostedByAccountingIntegration(CusEntryHeader entry, ZString expectedAPInvoiceNum)
		{
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification { SendGroupPK = @group.PK };
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions
			{
				EnableAccountingIntegration = true,
				APPostDSB = true,
				ARPostDSB = true
			};
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = entry.Declaration;
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			declaration.JE_TransportMode = "SEA";
			var importer = testHelper.Importer;
			declaration.JE_OH_Importer = importer.PK;

			entry.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 10m);
			Factory.Save();
			AssertNull("No invoicing job should have been created as nothing cleared", new JobHeader.Loader(declaration).Load());

			// Trigger Accounting Integration
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			Factory.Save();

			var job = new JobHeader.Loader(declaration).Load();
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			CombineAssertions(() =>
			{
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 10m, charges[0].JR_LocalCostAmt);
				AssertEquals("Invoice No", expectedAPInvoiceNum, charges[0].JR_APInvoiceNum);
				Assert("Cost should have been posted", charges[0].IsCostPosted);
				Assert("Revenue should have been posted", charges[0].IsRevenuePosted);
				AssertEquals(@"Customs Disbursements
Current Amounts
  Quarantine Processing Charge               10.00", charges[0].JR_Desc);
			});
		}

		public void TestScheduledPaymentDate() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var addInfo = entryHeader.AddInfo;
			addInfo.ZA_ScheduledPaymentDate_Hidden = new ZDateTime(2023, 10, 21, 23, 45, 12);
			AssertEquals("Getter", new ZDateTime(2023, 10, 21, 23, 45, 12), entryHeader.ScheduledPaymentDate);

			entryHeader.ScheduledPaymentDate = new ZDateTime(2023, 12, 25, 10, 16, 38);
			AssertEquals("Setter", new ZDateTime(2023, 12, 25, 10, 16, 38), addInfo.ZA_ScheduledPaymentDate_Hidden);
		});

		public void TestCustomsChargeAmountPayableNow() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var addInfo = entryHeader.AddInfo;
			addInfo.ZA_CustomsPayNow_Hidden = 100m;
			AssertEquals("Getter", 100m, entryHeader.CustomsChargeAmountPayableNow);

			entryHeader.CustomsChargeAmountPayableNow = 120m;
			AssertEquals("Setter", 120m, addInfo.ZA_CustomsPayNow_Hidden);
		});

		public void TestAQISServicePaymentAmountPayableNow() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var addInfo = entryHeader.AddInfo;
			addInfo.ZA_AQISPayNow_Hidden = 100m;
			AssertEquals("Getter", 100m, entryHeader.AQISServicePaymentAmountPayableNow);

			entryHeader.AQISServicePaymentAmountPayableNow = 120m;
			AssertEquals("Setter", 120m, addInfo.ZA_AQISPayNow_Hidden);
		});

		public void TestGetTotalChargeValueFor_WhenDutyIsPartiallyDeferred()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMPORG";
			importer.AUIsDutyDeferred = false;
			declaration.JE_OH_Importer = importer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);
			var customsChargeEntry = (ICustomsChargeEntry)entry;

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			Factory.Save();
			var duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("None deferred, 100m to be paid.", 100m, duty);

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entry.Messages.Add(imdrMessage);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 99m);
			Factory.Save();
			duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("99m deferred, 1m left.", 1m, duty);

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			Factory.Save();
			duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("All deferred, 0m to be paid, so it won't appear as a charge.", 0m, duty);

			entryLine.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 3418m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 85m);
			Factory.Save();
			duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("All deferred, 0m to be paid, so it won't appear as a charge.", 3333m, duty);
			var dtd = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyDeferredAmount], "");
			AssertEquals("DTD amount.", 85m, dtd);
		}

		public void TestGetTotalChargeValueFor_DutyPlus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMPORG";
			importer.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = importer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			var customsChargeEntry = (ICustomsChargeEntry)entry;

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 35m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 50m);
			Factory.Save();

			var duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("All deferred, 0m to be paid, so it won't appear as a charge.", 0m, duty);
			AssertEquals("DTY is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));
			var apc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.AQISProcessingCharge], "");
			AssertEquals("APC amount.", 35m, apc);
			AssertEquals("APC is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.AQISProcessingCharge, "", null));
			var dpc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge], "");
			AssertEquals("DPC amount.", 50m, dpc);
			AssertEquals("DPC is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, "", null));
			var dtd = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyDeferredAmount], "");
			AssertEquals("DTD amount.", 200m, dtd);

			importer.AUIsDutyDeferred = false;
			Factory.Save();
			duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("No Duty deferred. Shows full amount.", 200m, duty);
			AssertEquals("DTY is paid by broker.", true, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));
			apc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.AQISProcessingCharge], "");
			AssertEquals("APC amount.", 35m, apc);
			AssertEquals("APC is paid by broker.", true, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.AQISProcessingCharge, "", null));
			dpc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge], "");
			AssertEquals("DPC amount.", 50m, dpc);
			AssertEquals("DPC is paid by broker.", true, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, "", null));
			dtd = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyDeferredAmount], "");
			AssertEquals("DTD amount.", 0m, dtd);

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entry.Messages.Add(imdrMessage);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 284m);
			Factory.Save();
			duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("199m deferred, 1m left. Shows amount to be paid.", 1m, duty);
			AssertEquals("DTY is paid by broker.", true, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));
			apc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.AQISProcessingCharge], "");
			AssertEquals("APC amount.", 35m, apc);
			AssertEquals("APC is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.AQISProcessingCharge, "", null));
			dpc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge], "");
			AssertEquals("DPC amount.", 50m, dpc);
			AssertEquals("DPC is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, "", null));
			dtd = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyDeferredAmount], "");
			AssertEquals("DTD amount.", 199m, dtd);

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 285m);
			Factory.Save();
			duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("All deferred, 0m to be paid, so it won't appear as a charge.", 0m, duty);
			AssertEquals("DTY is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));
			apc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.AQISProcessingCharge], "");
			AssertEquals("APC amount.", 35m, apc);
			AssertEquals("APC is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.AQISProcessingCharge, "", null));
			dpc = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge], "");
			AssertEquals("DPC amount.", 50m, dpc);
			AssertEquals("DPC is deferred. Not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, "", null));
			dtd = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyDeferredAmount], "");
			AssertEquals("DTD amount.", 200m, dtd);

			entryLine.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 3418m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 38m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 50m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			Factory.Save();
			duty = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyAmount], "");
			AssertEquals("Partially deferred, 3406 of 3418 to be paid.", 3406m, duty);
			dtd = customsChargeEntry.GetTotalChargeValueFor(entry.EntryChargeTypeList[CusEntryChargeTypeList.Codes.DutyDeferredAmount], "");
			AssertEquals("DTD amount is 12 dollars (fees are not duty).", 12m, dtd);
		}

		public void TestPayableDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMPORG";
			importer.AUIsDutyDeferred = false;
			declaration.JE_OH_Importer = importer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 35m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 50m);

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			entryLine1.Fees.SetAmount(CusEntryChargeTypeList.Codes.WetAmount, 15m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", false, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 0m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 200m, entry.PayableDuty);

			importer.AUIsDutyDeferred = true;
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 200m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 0m, entry.PayableDuty);

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(imdrMessage);
			imdrMessage.EM_Status = EDIMessage.Status.Failed;
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 200m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 0m, entry.PayableDuty);

			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", false, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 0m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 200m, entry.PayableDuty);

			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 300m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 200m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 0m, entry.PayableDuty);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 300m, entry.DutyAmount);
			AssertEquals("DeferredDuty ($300 deferred - $100 in deferrable charges)", 200m, entry.DeferredDuty);
			AssertEquals("PayableDuty (duty owing after deferrable amounts are subtracted)", 100m, entry.PayableDuty);
		}

		public void TestPayableDuty_WithEntryLineEGGs()
		{
			var tariffClassificationCharacteristic29 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic29.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029129";
			tariffClassificationCharacteristic29.TC_CharacteristicCode = 29;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			var line1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			line1.JI_Tariff = "2402.91.29";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMPORG";
			importer.AUIsDutyDeferred = false;
			declaration.JE_OH_Importer = importer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 35m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 50m);

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.InvoiceLines.AddRange(declaration.InvoiceLines);
			AssertEquals(true, entryLine1.IsExciseEquivalentGoods);
			entryLine1.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			entryLine1.Fees.SetAmount(CusEntryChargeTypeList.Codes.WetAmount, 15m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", false, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 0m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 200m, entry.PayableDuty);

			importer.AUIsDutyDeferred = true;
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 0m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 200m, entry.PayableDuty);

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(imdrMessage);
			imdrMessage.EM_Status = EDIMessage.Status.Failed;
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 0m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 200m, entry.PayableDuty);

			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", false, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 0m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 200m, entry.PayableDuty);

			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 200m, entry.DutyAmount);
			AssertEquals("DeferredDuty", 15m, entry.DeferredDuty);
			AssertEquals("PayableDuty", 185m, entry.PayableDuty);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);
			Factory.Save();

			AssertEquals("IsDutyDeferred", true, entry.IsDutyDeferred);
			AssertEquals("DutyAmount", 300m, entry.DutyAmount);
			AssertEquals("DeferredDuty ($115 deferred - $100 in deferrable charges)", 15m, entry.DeferredDuty);
			AssertEquals("PayableDuty (duty owing after deferrable amounts are subtracted)", 285m, entry.PayableDuty);
		}

		public void TestIsDutyDeferred()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMPORG";
			importer.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = importer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			Factory.Save();

			AssertEquals("IsDutyDeferred from Importer", true, entry.IsDutyDeferred);

			importer.AUIsDutyDeferred = false;
			Factory.Save();
			AssertEquals("IsDutyDeferred from Importer", false, entry.IsDutyDeferred);

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(imdrMessage);
			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			Factory.Save();
			AssertEquals("IsDutyDeferred from DutyDeferredAmount", true, entry.IsDutyDeferred);

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			Factory.Save();
			AssertEquals("IsDutyDeferred from DutyDeferredAmount", false, entry.IsDutyDeferred);

			importer.AUIsDutyDeferred = true;
			Factory.Save();
			AssertEquals("IsDutyDeferred from DutyDeferredAmount when a valid IMDR exists", false, entry.IsDutyDeferred);

			imdrMessage.EM_Status = EDIMessage.Status.Failed;
			Factory.Save();
			AssertEquals("IsDutyDeferred from Importer without a valid IMDR", true, entry.IsDutyDeferred);

			var imdrMessageOrig = Factory.New<CMRIMDRMessage>();
			imdrMessageOrig.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entry.Messages.Add(imdrMessageOrig);
			imdrMessageOrig.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			AssertEquals("IsDutyDeferred from Importer without a valid IMD Response", true, entry.IsDutyDeferred);

			var imdrMessage2 = Factory.New<CMRIMDRMessage>();
			imdrMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(imdrMessage2);
			imdrMessage2.EM_Status = EDIMessage.Status.Received;
			imdrMessage2.EM_SystemCreateTimeUtc = imdrMessage.EM_SystemCreateTimeUtc.AddMinutes(1);
			Factory.Save();
			AssertEquals("IsDutyDeferred from DutyDeferredAmount when a valid IMDR exists", false, entry.IsDutyDeferred);

			entry.IsCalculatingDuty = true;
			AssertEquals("IsDutyDeferred from Importer when calculating duty even when a valid IMDR exists", true, entry.IsDutyDeferred);

			entry.IsCalculatingDuty = false;
			AssertEquals("IsDutyDeferred from DutyDeferredAmount when not calculating duty and a valid IMDR exists", false, entry.IsDutyDeferred);
		}

		[TestDate(2005, 6, 2)]
		public void TestFOBInLocalCurrencyForAU()
		{
			RefCurrency usCurrency = RefCurrency.New(Factory);
			usCurrency.RX_Code = "USD";
			ZDateTime from = new ZDateTime(2005, 6, 1);
			ZDateTime to = new ZDateTime(2005, 6, 5);
			usCurrency.SetCustomsRate(from, to, 0.6707m);

			RefCurrency hkCurrency = RefCurrency.New(Factory);
			hkCurrency.RX_Code = "HKD";
			hkCurrency.SetCustomsRate(from, to, 6.6707m);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.AutoCreateChargesBasedOnIncoTerm = false;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			InvoiceCharge fIFT = invoice.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 200m;
			fIFT.J7_RX_NKCurrency = "HKD";
			fIFT.J7_IsDutiable = false; //not dutiable
			fIFT.J7_IsIncludedInITOT = true;

			InvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = "HKD";
			DoMerge(testDec);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("FOB in local currency", 14879.81m, entryHeader.FOBInLocalCurrency.Amount);
		}

		public void TestJZ_CU_RelatedHouseBill_DeletedRowInformationCannotBeAccessedThroughTheRow()
		{
			var testDec = JobDeclaration.New(Factory);
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entry2 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine11 = entry1.MergedLines.AddNew();
			var entryLine12 = entry1.MergedLines.AddNew();

			var invoice1 = testDec.Invoices.AddNew();
			var invoice2 = testDec.Invoices.AddNew();
			var invoiceLine21 = invoice1.InvoiceLines.AddNew();
			var invoiceLine22 = invoice2.InvoiceLines.AddNew();

			invoiceLine21.JI_CL = entryLine11.PK;
			invoiceLine22.JI_CL = entryLine12.PK;

			AssertEquals("pre-condition, more than one entry", 2, testDec.CustomsEntryHeaders.Count);
			AssertEquals("pre-condition, cache invoiceheaders of one entry", 2, entry1.InvoiceHeaders.Length);

			invoice1.Delete();

			AssertNoExceptionThrown(() =>
			{
				entry1.ResetTotalsAndCachedValues();
			});
		}

		public void TestAllOtherDuties()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 100m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 200m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty, 400m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimCountervailingDuty, 500m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimDumpingDuty, 600m);

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 10m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 20m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty, 40m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimCountervailingDuty, 50m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimDumpingDuty, 60m);

			AssertEquals("All other duty for entry headers", 1980m, entryHeader.AllOtherDuties);
		}

		public void TestHasSecurityTreatment()
		{
			var instrumentCharacteristic1 = Factory.New<CMRInstrumentCharacteristic>();
			instrumentCharacteristic1.IK_InstrumentNumber = "111111";
			instrumentCharacteristic1.IK_CharacteristicCode = 1;
			var instrumentCharacteristic2 = Factory.New<CMRInstrumentCharacteristic>();
			instrumentCharacteristic2.IK_InstrumentNumber = "333333";
			instrumentCharacteristic2.IK_CharacteristicCode = 3;

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			Assert("No treatment codes", !entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "353";
			Assert("Not a secutity treatment code", !entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "351";
			Assert("Is a secutity treatment code", entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "352";
			Assert("Is a secutity treatment code", entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "354";
			Assert("Is a secutity treatment code", entryHeader.HasSecurityTreatment);

			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "391";
			invoiceLine2.AddInfo.TCI_InstrumentNo = "111111";
			Assert("Not an end use security BL", !entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.TCI_InstrumentNo = "333333";
			Assert("Is an end use security BL", entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.TCI_InstrumentNo = ZString.Empty;
			invoiceLine2.AddInfo.TI2_InstrumentNo = "333333";
			Assert("Is an end use security BL", entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.TI2_InstrumentNo = ZString.Empty;
			invoiceLine2.AddInfo.PRI_InstrumentNo = "333333";
			Assert("Is an end use security BL", entryHeader.HasSecurityTreatment);
			invoiceLine2.AddInfo.PRI_InstrumentNo = ZString.Empty;
			invoiceLine2.InstrumentCode = "333333";
			Assert("Is an end use security BL", entryHeader.HasSecurityTreatment);
			invoiceLine2.InstrumentCode = ZString.Empty;
			Assert("Not an end use security BL", !entryHeader.HasSecurityTreatment);
		}

		public void TestDoesTILVExist()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("DoesTILVExist", false, entry.DoesTILVExist);

			invoiceLine.AddInfo.ZA_TILV = "0.00AUD";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("DoesTILVExist", true, entry.DoesTILVExist);

			invoiceLine.AddInfo.ZA_TILV = "";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			declaration.ResumeApportionment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("DoesTILVExist", false, entry.DoesTILVExist);
		}

		public void TestWithdrawnLog()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;

			CusEntryLine entryLine = entry.MergedLines.AddNew();
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("hasBeenWithdrawn", false, entry.HasBeenWithdrawn);
			AssertEquals("IsActive", true, entry.IsActive);

			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("hasBeenWithdrawn", false, entry.HasBeenWithdrawn);
			AssertEquals("IsActive", true, entry.IsActive);

			entry.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			AssertEquals("hasBeenWithdrawn", false, entry.HasBeenWithdrawn);
			AssertEquals("IsActive", true, entry.IsActive);

			entry.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("hasBeenWithdrawn", true, entry.HasBeenWithdrawn);
			AssertEquals("IsActive", true, entry.IsActive);

			entry.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("hasBeenWithdrawn", true, entry.HasBeenWithdrawn);
			AssertEquals("IsActive", true, entry.IsActive);

			entry.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("hasBeenWithdrawn", true, entry.HasBeenWithdrawn);
			AssertEquals("IsActive", true, entry.IsActive);
		}

		public void TestClearLogWhenStatusIsSetToClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			declaration.OutstandingAmendmentLogManger.AddANewOutstandingAmendmentLog("A");
			AssertEquals("HasAnOutstandingAmendmentLog", true, declaration.OutstandingAmendmentLogManger.HasOutstandingAmendments);

			entry.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("HasAnOutstandingAmendmentLog it is not cleared yet", true, declaration.OutstandingAmendmentLogManger.HasOutstandingAmendments);
			Factory.Save();

			entry.CH_Status = CustomsEntryStatus.ClearPayment.Code;
			AssertEquals("HasAnOutstandingAmendmentLog Payment clearance does not matter", true, declaration.OutstandingAmendmentLogManger.HasOutstandingAmendments);
			Factory.Save();

			entry.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("HasAnOutstandingAmendmentLog it is not cleared yet", false, declaration.OutstandingAmendmentLogManger.HasOutstandingAmendments);
		}

		public void TestClearConsolidatedEntryChangedLogWhenStatusIsSetToClear()
		{
			var consolidatedEntry = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			consolidatedEntry.Factory.Save();
			var leadDec = (JobDeclaration)consolidatedEntry.LeadDeclaration;
			leadDec.EntryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			leadDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var memberDec = (JobDeclaration)consolidatedEntry.JobDeclarations[1];
			memberDec.EntryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			memberDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			AssertNotEquals(memberDec.PK, leadDec.PK);
			Factory.Save();

			var leadDecLog = leadDec.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);
			var memberDecLog = memberDec.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);
			var conDecLog = consolidatedEntry.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);

			AssertEquals("consolidatedEntry Has Consolidated Entry Changes", true, consolidatedEntry.HasConsolidatedEntryChanges);
			AssertEquals("leadDec Has Consolidated Entry Changes", true, leadDec.HasConsolidatedEntryChanges);
			AssertEquals("memberDec Has Consolidated Entry Changes", true, memberDec.HasConsolidatedEntryChanges);

			memberDec.EntryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("Clear Member Status clears memberDec log", true, memberDecLog.IsCancelled);
			AssertEquals("Clear Member Status doesn't clear consolidatedEntry logs", true, consolidatedEntry.HasConsolidatedEntryChanges);

			leadDec.EntryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("Clear Lead Status clears leadDec log", true, leadDecLog.IsCancelled);
			AssertEquals("Clear Lead Status clears consolidatedEntry logs", false, consolidatedEntry.HasConsolidatedEntryChanges);
		}

		public void TestClearLogWhenAmendmentSent()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			declaration.OutstandingAmendmentLogManger.AddANewOutstandingAmendmentLog("A");
			AssertEquals("HasAnOutstandingAmendmentLog", true, declaration.OutstandingAmendmentLogManger.HasOutstandingAmendments);

			entry.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("Sending an amendment now clears the logs", false, declaration.OutstandingAmendmentLogManger.HasOutstandingAmendments);
			Factory.Save();

			entry.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("HasOutstandingFailedAmendments is set by failed amendment", true, declaration.OutstandingAmendmentLogManger.HasOutstandingFailedAmendments);
			Factory.Save();

			entry.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("Sending another amendment does not clear HasOutstandingFailedAmendments", true, declaration.OutstandingAmendmentLogManger.HasOutstandingFailedAmendments);
			Factory.Save();

			entry.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("An accepted amednment will clear HasOutstandingFailedAmendments", false, declaration.OutstandingAmendmentLogManger.HasOutstandingFailedAmendments);
		}

		public void TestClearConsolidatedEntryChangedLogWhenAmendmentSent()
		{
			var consolidatedEntry = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 0);
			consolidatedEntry.Factory.Save();
			var declaration = (JobDeclaration)consolidatedEntry.LeadDeclaration;

			var entry = declaration.EntryHeader;
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			var decLog = declaration.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);
			var cecLog = consolidatedEntry.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);

			AssertEquals("Has Consolidated Entry Changes", true, declaration.HasConsolidatedEntryChanges);
			AssertEquals("Has Consolidated Entry Changes", true, consolidatedEntry.HasConsolidatedEntryChanges);

			entry.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("Sending an amendment clears the logs", false, declaration.HasConsolidatedEntryChanges);
			AssertEquals("Sending an amendment clears the logs", false, consolidatedEntry.HasConsolidatedEntryChanges);
		}

		public void TestAllEntryLines()
		{
			CusEntryHeader entryHeader = (CusEntryHeader)GetNewBusinessObject();
			AssertEquals(typeof(AllCusEntryLineCollection<CusEntryLine>), entryHeader.AllEntryLines.GetType());
		}

		public void TestBillCollectionForEntry()
		{
			CusEntryHeader entryHeader = (CusEntryHeader)GetNewBusinessObject();
			AssertEquals(typeof(CMRBillCollectionForEntry), entryHeader.Bills.GetType());
		}

		public void TestAllEntryFeesExcludingAQISServiceFee()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 10.1m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 20.2m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 30.3m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 40.4m);

			AssertEquals("All entry fees", 101m, entryHeader.AllEntryFees);
			AssertEquals("All Entry fees excluding AQIS service fee", 70.7m, entryHeader.AllEntryFeesExcludingAQISServiceFee);
		}

		public void TestIsWithdrawn()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Withdrawn", false, entryHeader.IsWithdrawn);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("Withdrawn", true, entryHeader.IsWithdrawn);
		}

		public void TestSubjectToRedLineProcessing()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Subject To Red Line Processing", true, entryHeader.SubjectToRedLineProcessing.IsEmpty);

			entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
			AssertEquals("Subject To Red Line Processing", false, entryHeader.SubjectToRedLineProcessing.IsEmpty);
			AssertEquals("Subject To Red Line Processing", "Subject to Red Line", entryHeader.SubjectToRedLineProcessing);
		}

		public void TestNormalisedInvoiceTotalCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 29);
			testDec.JE_MergeBy = "TRF";

			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.5826m, helper.EURCurrency);
			GroupInvoiceCharge oNS = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			oNS.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 80.74m;
			oNS.J7_RX_NKCurrency = helper.EURCurrency.RX_Code;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1249.80m;
			invoice1.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			InvoiceCharge oFT1 = invoice1.Charges.AddNew();
			oFT1.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT1.J7_Amount = 85m;
			oFT1.J7_IsIncludedInITOT = true;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 1249.80m;
			line1.JI_Tariff = "9506.99.90 32";

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 31599.16m;
			invoice2.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			InvoiceCharge oFT2 = invoice2.Charges.AddNew();
			oFT2.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT2.J7_Amount = 469m;
			oFT2.J7_IsIncludedInITOT = true;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 31599.16m;
			line2.JI_Tariff = "7326.90.90 58";

			testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals(true, entryHeader.ShouldEntryBeNormalised);

			AssertEquals(helper.EURCurrency, entryHeader.NormalisedInvoiceTotalCurrency);
			AssertEquals("Customs Factor", 1.71644353m, entryHeader.CustomsFactor);
		}

		public void TestNormalisedInvoiceTotalCurrencyWhenONSInDiffCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 29);
			testDec.JE_MergeBy = "TRF";

			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.5826m, helper.USDCurrency);
			GroupInvoiceCharge oNS = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			oNS.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 80.74m;
			oNS.J7_RX_NKCurrency = helper.USDCurrency.RX_Code;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1249.80m;
			invoice1.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			InvoiceCharge oFT1 = invoice1.Charges.AddNew();
			oFT1.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT1.J7_Amount = 85m;
			oFT1.J7_IsIncludedInITOT = false;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 1164.80m;
			line1.JI_Tariff = "9506.99.90 32";

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 31599.16m;
			invoice2.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			InvoiceCharge oFT2 = invoice2.Charges.AddNew();
			oFT2.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT2.J7_Amount = 469m;
			oFT2.J7_IsIncludedInITOT = false;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 31130.16m;
			line2.JI_Tariff = "7326.90.90 58";

			testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals(true, entryHeader.ShouldEntryBeNormalised);
			AssertEquals(helper.AUDCurrency, entryHeader.NormalisedInvoiceTotalCurrency);
			AssertEquals("Customs Factor", 1m, entryHeader.CustomsFactor);
		}

		public void TestAuthorityToDealMessage()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertNull("ATDMessage", entryHeader.AuthorityToDealMessage);

			EDIMessage pAYRECMessage = entryHeader.Messages.AddNew(typeof(CMRPAYRECMessage));
			AssertNull("ATDMessage", entryHeader.AuthorityToDealMessage);

			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = ATDMessageText;
			AssertNotNull("ATDMessage", entryHeader.AuthorityToDealMessage);
			AssertEquals("Security", "AAAANNPTY", entryHeader.AuthorityToDealMessage.SecurityCode);
		}

		public void TestMultipleAuthorityToDealMessages()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertNull("ATDMessage", entryHeader.AuthorityToDealMessage);

			EDIMessage pAYRECMessage = entryHeader.Messages.AddNew(typeof(CMRPAYRECMessage));
			AssertNull("ATDMessage", entryHeader.AuthorityToDealMessage);

			EDIMessage aTDMessage1 = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage1.EM_MessageText = ATDMessageText;

			EDIMessage aTDMessage2 = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'" +
				"TDT+20++6'LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'" +
				"NAD+IM++EAGLE DATAMATION NB'RFF+ABO:B00122382/1/1::1'RFF+ABT:AAAANNPR6::1'RFF+ABQ:SIMPLE MAIL'RFF+AIA:TTTTTTTT'RFF+AAE:N10'" +
				"DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'CNT+5:1'CNT+2:1'CNT+3:0'" +
				"UNT+30+000001'UNZ+1+00000000273283'";

			AssertNotNull("ATDMessage", entryHeader.AuthorityToDealMessage);
			AssertEquals("Security", "TTTTTTTT", entryHeader.AuthorityToDealMessage.SecurityCode);
		}

		public void TestCMRPAYRECMessages()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "S12345";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S12345/1";
			Factory.Save();

			CMRREFACCMessage message1 = Factory.New<CMRREFACCMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.REFACC;
			entryHeader.Messages.Add(message1);

			CMRPAYRECMessage message2 = Factory.New<CMRPAYRECMessage>();
			message2.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			entryHeader.Messages.Add(message2);

			CMRPAYINVMessage message3 = Factory.New<CMRPAYINVMessage>();
			message3.EM_MessageText = CMRImportDeclarationTestData.PAYINV;
			entryHeader.Messages.Add(message3);

			AssertEquals("Found 1 CMRPAYRECMessage for Entry Header", 1, entryHeader.CMRPAYRECMessages.Length);
		}

		public void TestCMRPAYRECandREFACCMessages()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "S12345";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S12345/1";
			Factory.Save();

			CMRREFACCMessage message1 = Factory.New<CMRREFACCMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.REFACC;
			entryHeader.Messages.Add(message1);

			CMRPAYRECMessage message2 = Factory.New<CMRPAYRECMessage>();
			message2.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			entryHeader.Messages.Add(message2);

			CMRPAYINVMessage message3 = Factory.New<CMRPAYINVMessage>();
			message3.EM_MessageText = CMRImportDeclarationTestData.PAYINV;
			entryHeader.Messages.Add(message3);

			AssertEquals("Found 2 CMRPAYRECandREFACCMessages for Entry Header", 2, entryHeader.CMRPAYRECandREFACCMessages.Length);
		}

		public void TestWarehouseNumberOfPacks()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.WarehouseNumberOfPacks = 150;
			AssertEquals("Warehouse number of packs", 150, entryHeader.AddInfo.ZA_WarehouseNumberOfPacks_Hidden);
		}

		public void TestRefundReasonCode()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.RefundReasonCode = "126A";
			AssertEquals("Refund Reason Code", "126A", entryHeader.AddInfo.ZA_RRC_Hidden);
		}

		public void TestCH_StatusSavingNumbers()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			PackingGroup pack4 = declaration.PackingGroups.AddNew();
			pack4.CR_HouseContainerNumber = 4;
			pack4.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			PackingGroup pack2 = declaration.PackingGroups.AddNew();
			pack2.CR_HouseContainerNumber = 2;
			pack2.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			PackingGroup pack15 = declaration.PackingGroups.AddNew();
			pack15.CR_HouseContainerNumber = 15;
			pack15.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLineNumber5 = entryHeader.MergedLines.AddNew();
			entryLineNumber5.CL_LineNumber = 5;
			CusEntryLine entryLineNumber2 = entryHeader.MergedLines.AddNew();
			entryLineNumber2.CL_LineNumber = 2;
			CusEntryLine entryLineNumber10 = entryHeader.MergedLines.AddNew();
			entryLineNumber10.CL_LineNumber = 10;
			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;

			AssertEquals("Highest line number not saved", (ZShort)0, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 0, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("Highest line number not saved", (ZShort)0, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 0, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Highest line number saved", (ZShort)10, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number saved", 15, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			CusEntryLine entryLineNumber25 = entryHeader.MergedLines.AddNew();
			entryLineNumber25.CL_LineNumber = 25;
			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("Highest line number not saved", (ZShort)10, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 15, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("Highest line number not saved", (ZShort)10, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 15, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("Highest line number saved", (ZShort)25, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number saved", 15, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);
		}

		public void TestCH_StatusSavingNumbersForSAC()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SAC";
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			PackingGroup pack4 = declaration.PackingGroups.AddNew();
			pack4.CR_HouseContainerNumber = 4;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLineNumber5 = entryHeader.MergedLines.AddNew();
			entryLineNumber5.CL_LineNumber = 5;
			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;

			AssertEquals("Highest line number not saved", (ZShort)0, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 0, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingSAC.Code;
			AssertEquals("Highest line number not saved", (ZShort)0, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 0, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			AssertEquals("Highest line number not saved", (ZShort)0, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 0, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);

			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			AssertEquals("Highest line number not saved", (ZShort)0, entryHeader.CH_HighestLineNumber);
			AssertEquals("Highed packing number not saved", 0, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);
		}

		public void TestAllEntryFees()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 100m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 110m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 120m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.TotalPayableAdmin, 130m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 140m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 150m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 160m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 170m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 180m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 190m);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("All entry fees for CMR", 750m, entryHeader.AllEntryFees);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("All entry fees for Edifice", 820m, entryHeader.AllEntryFees);
		}

		public void TestOtherCMRCharges()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 100m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 110m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 120m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.TotalPayableAdmin, 130m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 140m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 150m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 160m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 170m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 180m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 190m);

			AssertEquals("Other CMR Charges", 1140M, entryHeader.OtherCMRCharges);
		}

		public void TestIsWaitingForResponse()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("AwaitingAmendment iswaiting for response", true, entryHeader.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingSAC.Code;
			AssertEquals("AwaitingSAC iswaiting for response", true, entryHeader.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("AwaitingFormalLodge iswaiting for response", true, entryHeader.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPayment.Code;
			AssertEquals("AwaitingPayment iswaiting for response", true, entryHeader.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			AssertEquals("AwaitingPreLodge iswaiting for response", true, entryHeader.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			AssertEquals("AwaitingWithdrawal iswaiting for response", true, entryHeader.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("ClearWithdrawal iswaiting for response", false, entryHeader.IsWaitingForResponse);
		}

		public void TestTotalPayableAdvisedInLastClearanceMessage()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = TestMessages.IMDMessageText;
			iMDMessage.EM_LinkedObject = entryHeader;
			iMDMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;

			Factory.Save();

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals("Total payable", 191.90m, new OutstandingAmountRetriever(iMDRMessage).OutstandingAmount);

			Factory.Save();

			CMRSACRMessage sACRMessageRejected = Factory.New<CMRSACRMessage>();
			sACRMessageRejected.EM_MessageText = TestMessages.SACRMessageTextNegative;
			sACRMessageRejected.EM_LinkedObject = entryHeader;
			sACRMessageRejected.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			sACRMessageRejected.EM_MessageSubType = EDIMessage.Status.Rejected;
			Factory.Save();

			AssertEquals("Rejected", true, sACRMessageRejected.IsRejected);
			AssertEquals("Total payable", -16m, new OutstandingAmountRetriever(sACRMessageRejected).OutstandingAmount);
			AssertEquals("TotalPayableAdvised in last successful message", 191.90m, entryHeader.TotalPayableDueAdvisedInLastClearanceMessage);
		}

		public void TestTransportAndInsuranceForheader()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 7000m;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3000m;

			invoiceLine.AddInfo.ZA_TILV = "150AUD";
			invoiceLine2.AddInfo.ZA_TILV = "250AUD";

			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryHeader.ResetTotalsAndCachedValues();
			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("Transport and insurance for header", 400m, entryHeader.TransportAndInsurance.Amount);

			invoiceLine.AddInfo.ZA_TILV = "";
			invoiceLine2.AddInfo.ZA_TILV = "";

			invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");
			invoiceLine2.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 350m, "AUD");
			entryHeader.ResetTotalsAndCachedValues();
			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("Transport and insurance for header", 450m, entryHeader.TransportAndInsurance.Amount);
		}

		public void TestTotalNumberOfPackages()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_TotalNoOfPacks = 4;
			CusContainer container = testDec.CusContainers.AddNew();
			testDec.JE_HouseBill = "HB1";
			Bill houseBill = testDec.PrimaryHouseBill;

			PackingGroup multiPack = entryHeader.PackingGroups.AddNew();
			Package package = multiPack.Packages.AddNew();
			package.CW_PackQty = 2;
			package.CW_InBondPackQty = 3;
			multiPack.CR_CO_Container = container.PK;
			multiPack.CR_CU_HouseBill = houseBill.PK;
			AssertEquals("Total number of packages", 6, entryHeader.TotalNumberOfPackages);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Total number of packages", 4, entryHeader.TotalNumberOfPackages);

			entryHeader.WarehouseNumberOfPacks = 5;
			AssertEquals("Total number of packages", 5, entryHeader.TotalNumberOfPackages);
		}

		public void TestTotalNumberOfPackagesForMultipleEntryiesForNature30()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "EXW";
			testDec.JE_TotalNoOfPacks = 4;
			CusEntryHeader entryHeader1 = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Total number of packages", 4, entryHeader1.TotalNumberOfPackages);

			CusEntryHeader entryHeader2 = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Total number of packages", 0, entryHeader1.TotalNumberOfPackages);
			AssertEquals("Total number of packages", 0, entryHeader2.TotalNumberOfPackages);

			entryHeader1.WarehouseNumberOfPacks = 5;
			AssertEquals("Total number of packages", 5, entryHeader1.TotalNumberOfPackages);
			AssertEquals("Total number of packages", 0, entryHeader2.TotalNumberOfPackages);

			entryHeader2.WarehouseNumberOfPacks = 6;
			AssertEquals("Total number of packages", 5, entryHeader1.TotalNumberOfPackages);
			AssertEquals("Total number of packages", 6, entryHeader2.TotalNumberOfPackages);
		}

		public void TestWarehouseNumberOfPacksForMessage()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "EXW";
			testDec.JE_TotalNoOfPacks = 4;
			CusEntryHeader entryHeader1 = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Total number of packages", 4, entryHeader1.WarehouseNumberOfPacksForMessage);

			CusEntryHeader entryHeader2 = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Total number of packages", 0, entryHeader1.WarehouseNumberOfPacksForMessage);
			AssertEquals("Total number of packages", 0, entryHeader2.WarehouseNumberOfPacksForMessage);

			entryHeader1.WarehouseNumberOfPacks = 5;
			AssertEquals("Total number of packages", 5, entryHeader1.WarehouseNumberOfPacksForMessage);
			AssertEquals("Total number of packages", 0, entryHeader2.WarehouseNumberOfPacksForMessage);

			entryHeader2.WarehouseNumberOfPacks = 6;
			AssertEquals("Total number of packages", 5, entryHeader1.WarehouseNumberOfPacksForMessage);
			AssertEquals("Total number of packages", 6, entryHeader2.WarehouseNumberOfPacksForMessage);
		}

		public void TestDutyDeferred()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			entryHeader.Charges[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge].C1_ChargeAmount = 33m;
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);

			Assert("Duty not deferred.", !entryHeader.IsDutyDeferred);
			AssertEquals("Duty not deferred.", 100m, entryLine.DutyAmount);
			AssertEquals("Duty not deferred.", 100m, entryHeader.DutyAmount);
			AssertEquals("Duty not deferred.", 100m, entryHeader.DutyAmountIncludingWHEstimate);
			AssertEquals("Duty not deferred.", 100m, entryHeader.PayableDuty);
			AssertEquals("Duty not deferred.", 133m, entryHeader.TotalAmountPayable);
			AssertEquals("Duty not deferred.", 0m, entryHeader.DeferredDuty);

			var org = Factory.New<OrgHeader>();
			dec.JE_OH_Importer = org.PK;
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = false;
			Factory.Save();

			Assert("Consignee indicates that Duty is not deferred", !entryHeader.IsDutyDeferred);
			AssertEquals("Duty not deferred.", 100m, entryHeader.DutyAmount);
			AssertEquals("Duty not deferred.", 133m, entryHeader.TotalAmountPayable);
			AssertEquals("Duty not deferred.", 0m, entryHeader.DeferredDuty);

			org.AUIsDutyDeferred = true;
			Factory.Save();

			Assert("Consignee indicates that Duty is deferred.", entryHeader.IsDutyDeferred);
			AssertEquals("Line level DutyAmount", 100m, entryLine.DutyAmount);
			AssertEquals("Header DutyAmount", 100m, entryHeader.DutyAmount);
			AssertEquals("Header DutyAmountIncludingWHEstimate", 100m, entryHeader.DutyAmountIncludingWHEstimate);
			AssertEquals("TotalDeferredDutyFromCustoms", 0m, entryHeader.TotalDeferredDutyFromCustoms);
			AssertEquals("Header PayableDuty - Duty is deferred.", 0m, entryHeader.PayableDuty);
			AssertEquals("Duty is deferred.", 0m, entryHeader.TotalAmountPayable);
			AssertEquals("Duty is deferred.", 100m, entryHeader.DeferredDuty);

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			imdrMessage.EM_Status = EDIMessage.Status.Received;
			imdrMessage.EM_MessageText = TestMessages.IMDRMessageText;
			imdrMessage.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Reload(true);
			entryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 0;
			Factory.Save();

			Assert("Customs Response indicates that Duty is not deferred.", !entryHeader.IsDutyDeferred);
			AssertEquals("Duty not deferred.", 100m, entryHeader.DutyAmount);
			AssertEquals("Duty not deferred.", 100m, entryHeader.PayableDuty);
			AssertEquals("Duty not deferred.", 133m, entryHeader.TotalAmountPayable);
			AssertEquals("Duty not deferred.", 0m, entryHeader.DeferredDuty);

			entryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 133;
			Factory.Save();

			Assert("Customs Response indicates that Duty is deferred.", entryHeader.IsDutyDeferred);
			AssertEquals("Duty deferred.", 100m, entryHeader.DutyAmount);
			AssertEquals("TotalDeferredDutyFromCustoms", 133m, entryHeader.TotalDeferredDutyFromCustoms);
			AssertEquals("Header PayableDuty - Duty is deferred.", 0m, entryHeader.PayableDuty);
			AssertEquals("Duty is deferred.", 0m, entryHeader.TotalAmountPayable);
			AssertEquals("Duty is deferred.", 100m, entryHeader.DeferredDuty);

			org.AUIsDutyDeferred = false;
			Factory.Save();
			Assert("Duty is deferred. Consignee setting is ignored.", entryHeader.IsDutyDeferred);
			AssertEquals("TotalDeferredDutyFromCustoms", 133m, entryHeader.TotalDeferredDutyFromCustoms);
			AssertEquals("Duty deferred.", 0m, entryHeader.PayableDuty);
			AssertEquals("Duty is deferred.", 0m, entryHeader.TotalAmountPayable);
			AssertEquals("Duty is deferred.", 100m, entryHeader.DeferredDuty);
		}

		public void TestDutyDeferred_ConsolidatedEntry()
		{
			var consolidatedEntry = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			consolidatedEntry.Factory.Save();
			var leadDec = (JobDeclaration)consolidatedEntry.LeadDeclaration;
			leadDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			leadDec.EntryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			leadDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			var leadDecEntryHeader = leadDec.CustomsEntryHeaders.AddNew();
			var entryLine = leadDecEntryHeader.MergedLines.AddNew();

			leadDecEntryHeader.Charges[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge].C1_ChargeAmount = 33m;
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);

			CombineAssertions(() =>
			{
				Assert("Duty not deferred.", !leadDecEntryHeader.IsDutyDeferred);
				AssertEquals("Duty not deferred. entryLine DutyAmount", 100m, entryLine.DutyAmount);
				AssertEquals("Duty not deferred. DutyAmount", 100m, leadDecEntryHeader.DutyAmount);
				AssertEquals("Duty not deferred. DutyAmountIncludingWHEstimate", 100m, leadDecEntryHeader.DutyAmountIncludingWHEstimate);
				AssertEquals("Duty not deferred. PayableDuty", 100m, leadDecEntryHeader.PayableDuty);
				AssertEquals("Duty not deferred. DeferredDuty", 0m, leadDecEntryHeader.DeferredDuty);
				AssertEquals("Duty not deferred. TotalAmountPayable", 133m, leadDecEntryHeader.TotalAmountPayable);
			});

			var org = Factory.New<OrgHeader>();
			leadDec.JE_OH_Importer = org.PK;
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = false;
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Consignee indicates that Duty is not deferred", !leadDecEntryHeader.IsDutyDeferred);
				AssertEquals("Duty not deferred DutyAmount.", 100m, leadDecEntryHeader.DutyAmount);
				AssertEquals("Duty not deferred DeferredDuty.", 0m, leadDecEntryHeader.DeferredDuty);
				AssertEquals("Duty not deferred TotalAmountPayable.", 133m, leadDecEntryHeader.TotalAmountPayable);
			});

			org.AUIsDutyDeferred = true;
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Consignee indicates that Duty is deferred.", leadDecEntryHeader.IsDutyDeferred);
				AssertEquals("Line level DutyAmount", 100m, entryLine.DutyAmount);
				AssertEquals("Header DutyAmount", 100m, leadDecEntryHeader.DutyAmount);
				AssertEquals("Header DutyAmountIncludingWHEstimate", 100m, leadDecEntryHeader.DutyAmountIncludingWHEstimate);
				AssertEquals("TotalDeferredDutyFromCustoms", 0m, leadDecEntryHeader.TotalDeferredDutyFromCustoms);
				AssertEquals("Header PayableDuty - Duty is deferred.", 0m, leadDecEntryHeader.PayableDuty);
				AssertEquals("Duty is deferred. DeferredDuty", 100m, leadDecEntryHeader.DeferredDuty);
				AssertEquals("Duty is deferred. TotalAmountPayable", 0m, leadDecEntryHeader.TotalAmountPayable);
			});

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			imdrMessage.EM_Status = EDIMessage.Status.Received;
			imdrMessage.EM_MessageText = TestMessages.IMDRMessageText;
			imdrMessage.EM_LinkedObject = consolidatedEntry;
			consolidatedEntry.Messages.Reload(true);
			leadDecEntryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 0;
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Customs Response indicates that Duty is not deferred.", !leadDecEntryHeader.IsDutyDeferred);
				AssertEquals("Duty not deferred. DutyAmount", 100m, leadDecEntryHeader.DutyAmount);
				AssertEquals("Duty not deferred. PayableDuty", 100m, leadDecEntryHeader.PayableDuty);
				AssertEquals("Duty not deferred. DeferredDuty", 0m, leadDecEntryHeader.DeferredDuty);
				AssertEquals("Duty not deferred. TotalAmountPayable", 133m, leadDecEntryHeader.TotalAmountPayable);
			});

			leadDecEntryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 133;
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Customs Response indicates that Duty is deferred.", leadDecEntryHeader.IsDutyDeferred);
				AssertEquals("Duty deferred. DutyAmount", 100m, leadDecEntryHeader.DutyAmount);
				AssertEquals("TotalDeferredDutyFromCustoms", 133m, leadDecEntryHeader.TotalDeferredDutyFromCustoms);
				AssertEquals("Header PayableDuty - Duty is deferred.", 0m, leadDecEntryHeader.PayableDuty);
				AssertEquals("Duty is deferred. DeferredDuty", 100m, leadDecEntryHeader.DeferredDuty);
				AssertEquals("Duty is deferred. TotalAmountPayable", 0m, leadDecEntryHeader.TotalAmountPayable);

				org.AUIsDutyDeferred = false;
				Factory.Save();
				Assert("Duty is deferred. Consignee setting is ignored.", leadDecEntryHeader.IsDutyDeferred);
				AssertEquals("TotalDeferredDutyFromCustoms", 133m, leadDecEntryHeader.TotalDeferredDutyFromCustoms);
				AssertEquals("Duty deferred. PayableDuty", 0m, leadDecEntryHeader.PayableDuty);
				AssertEquals("Duty is deferred. DeferredDuty", 100m, leadDecEntryHeader.DeferredDuty);
				AssertEquals("Duty is deferred. TotalAmountPayable", 0m, leadDecEntryHeader.TotalAmountPayable);
			});
		}

		public void TestDeferrableCharges()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			importer.AUIsDutyDeferred = false;

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges[CusEntryChargeTypeList.Codes.AQISProcessingCharge].C1_ChargeAmount = 1m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.AQISServicePaymentAmount].C1_ChargeAmount = 2m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge].C1_ChargeAmount = 3m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.TotalPayableAdmin].C1_ChargeAmount = 4m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.AQISContainerCharges].C1_ChargeAmount = 5m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.Woodlevy].C1_ChargeAmount = 6m;

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 11m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 12m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 13m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 14m);

			CombineAssertions("Not Deferred", () =>
			{
				AssertEquals("AQISProcessingCharge", 1m, entryHeader.AQISProcessingCharge);
				AssertEquals("AQISServicePaymentAmount", 2m, entryHeader.AQISServicePaymentAmount);
				AssertEquals("DeclarationProcessingCharge", 3m, entryHeader.DeclarationProcessingCharge);
				AssertEquals("EntryFee", 3m, entryHeader.EntryFee);
				AssertEquals("TotalPayableAdmin", 4m, entryHeader.TotalPayableAdmin);
				AssertEquals("AQISContainerCharges", 5m, entryHeader.AQISContainerCharges);
				AssertEquals("WoodLevy", 6m, entryHeader.WoodLevy);
				AssertEquals("WoodLevyIncludingWHEstimate", 6m, entryHeader.WoodLevyIncludingWHEstimate);

				AssertEquals("CountervailingDuty", 11m, entryHeader.CountervailingDuty);
				AssertEquals("DumpingDuty", 12m, entryHeader.DumpingDuty);
				AssertEquals("LCTAmount", 13m, entryHeader.LCTAmount);
				AssertEquals("LCTAmountIncludingWHEstimate", 13m, entryHeader.LCTAmountIncludingWHEstimate);
				AssertEquals("PayableLCT", 13m, entryHeader.PayableLCT);
				AssertEquals("WETAmount", 14m, entryHeader.WETAmount);
				AssertEquals("WETAmountIncludingWHEstimate", 14m, entryHeader.WETAmountIncludingWHEstimate);
				AssertEquals("PayableWET", 14m, entryHeader.PayableWET);

				AssertEquals("PayableAQISCharges", 8m, entryHeader.PayableAQISCharges);
				AssertEquals("PayableOtherCMRCharges", 19m, entryHeader.PayableOtherCMRCharges);
				AssertEquals("TotalAmountPayable", 71m, entryHeader.TotalAmountPayable);
				AssertEquals("TotalDeferrableDutyAndCharges", 60m, entryHeader.EstimatedDeferredDutyAndCharges);
			});

			importer.AUIsDutyDeferred = true;
			Factory.Save();

			CombineAssertions("Deferred", () =>
			{
				AssertEquals("AQISProcessingCharge", 1m, entryHeader.AQISProcessingCharge);
				AssertEquals("AQISServicePaymentAmount", 2m, entryHeader.AQISServicePaymentAmount);
				AssertEquals("DeclarationProcessingCharge", 3m, entryHeader.DeclarationProcessingCharge);
				AssertEquals("EntryFee", 3m, entryHeader.EntryFee);
				AssertEquals("TotalPayableAdmin", 4m, entryHeader.TotalPayableAdmin);
				AssertEquals("AQISContainerCharges", 5m, entryHeader.AQISContainerCharges);
				AssertEquals("WoodLevy", 6m, entryHeader.WoodLevy);
				AssertEquals("WoodLevyIncludingWHEstimate", 6m, entryHeader.WoodLevyIncludingWHEstimate);

				AssertEquals("CountervailingDuty", 11m, entryHeader.CountervailingDuty);
				AssertEquals("DumpingDuty", 12m, entryHeader.DumpingDuty);
				AssertEquals("LCTAmount", 13m, entryHeader.LCTAmount);
				AssertEquals("LCTAmountIncludingWHEstimate", 13m, entryHeader.LCTAmountIncludingWHEstimate);
				AssertEquals("PayableLCT", 0m, entryHeader.PayableLCT);
				AssertEquals("WETAmount", 14m, entryHeader.WETAmount);
				AssertEquals("WETAmountIncludingWHEstimate", 14m, entryHeader.WETAmountIncludingWHEstimate);
				AssertEquals("PayableWET", 0m, entryHeader.PayableWET);

				AssertEquals("PayableAQISCharges", 7m, entryHeader.PayableAQISCharges);
				AssertEquals("PayableOtherCMRCharges", 9m, entryHeader.PayableOtherCMRCharges);
				AssertEquals("TotalAmountPayable", 11m, entryHeader.TotalAmountPayable);
				AssertEquals("TotalDeferrableDutyAndCharges", 60m, entryHeader.EstimatedDeferredDutyAndCharges);
			});
		}

		public void TestAllAQISChargesForEntryHeader()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 100m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 140m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 150m);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("All quarantine charges", 390m, entryHeader.AllAQISCharges);
		}

		public void TestNatureFlagsForCMR()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			AssertEquals("IsNature10", true, entryHeader.IsNature10);
			AssertEquals("IsNature20", false, entryHeader.IsNature20);
			AssertEquals("IsNature30", false, entryHeader.IsNature30);
			AssertEquals("IsNature1020", false, entryHeader.IsNature1020);

			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("IsNature10", false, entryHeader.IsNature10);
			AssertEquals("IsNature20", false, entryHeader.IsNature20);
			AssertEquals("IsNature30", false, entryHeader.IsNature30);
			AssertEquals("IsNature1020", true, entryHeader.IsNature1020);

			invoiceLine2.JI_IsPackToBondForLine = true;
			AssertEquals("IsNature10", false, entryHeader.IsNature10);
			AssertEquals("IsNature20", true, entryHeader.IsNature20);
			AssertEquals("IsNature30", false, entryHeader.IsNature30);
			AssertEquals("IsNature1020", false, entryHeader.IsNature1020);

			invoiceLine.JI_IsPackToBondForLine = false;
			invoiceLine2.JI_IsPackToBondForLine = false;
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("IsNature10", false, entryHeader.IsNature10);
			AssertEquals("IsNature20", false, entryHeader.IsNature20);
			AssertEquals("IsNature30", true, entryHeader.IsNature30);
			AssertEquals("IsNature1020", false, entryHeader.IsNature1020);
		}

		public void TestNatureFlagsForEdifice()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("IsNature10", true, entryHeader.IsNature10);
			AssertEquals("IsNature20", false, entryHeader.IsNature20);
			AssertEquals("IsNature30", false, entryHeader.IsNature30);
			AssertEquals("IsNature1020", false, entryHeader.IsNature1020);

			invoiceLine.JI_IsPackToBondForLine = true;

			AssertEquals("IsNature10", false, entryHeader.IsNature10);
			AssertEquals("IsNature20", true, entryHeader.IsNature20);
			AssertEquals("IsNature30", false, entryHeader.IsNature30);
			AssertEquals("IsNature1020", false, entryHeader.IsNature1020);

			invoiceLine.JI_IsPackToBondForLine = false;
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("IsNature10", false, entryHeader.IsNature10);
			AssertEquals("IsNature20", false, entryHeader.IsNature20);
			AssertEquals("IsNature30", true, entryHeader.IsNature30);
			AssertEquals("IsNature1020", false, entryHeader.IsNature1020);
		}

		public void TestIsPostLodgeStatus()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals("FailFormalLodge - IsPostLodgeStatus", false, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Clear Formal Lodge - IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("Not sent - IsPostLodgeStatus", false, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPayment.Code;
			AssertEquals("Clear payment -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals("ClearPreLodge - IsPostLodgeStatus", false, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			AssertEquals("ClearSAC -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			AssertEquals("AwaitingPreLodge - IsPostLodgeStatus", false, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("ClearAmendment -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.FailPreLodge.Code;
			AssertEquals("FailPreLodge - IsPostLodgeStatus", false, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("FailAmendment -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			AssertEquals("FailSAC - IsPostLodgeStatus", false, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.FailPayment.Code;
			AssertEquals("FailPayment -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPayment.Code;
			AssertEquals("AwaitingPayment -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			AssertEquals("AwaitingWithdrawal -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("ClearWithdrawal -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);

			entryHeader.CH_Status = CustomsEntryStatus.FailWithdrawal.Code;
			AssertEquals("FailWithdrawal -IsPostLodgeStatus", true, ((ICPQAHeaderAttachee)entryHeader).IsStatusPostLodge);
		}

		public void TestIDeclarationChargeProvider()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;//N30

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100m;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 50m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			IDeclarationChargeProvider entryHeaderAsChargeProvider = entryHeader;

			AssertEquals("N10 value", ZDecimal.Zero, entryHeaderAsChargeProvider.N10CustomsValue);
			AssertEquals("N20 value", ZDecimal.Zero, entryHeaderAsChargeProvider.N20CustomsValue);
			AssertEquals("N30 value", 150m, entryHeaderAsChargeProvider.N30CustomsValue);
			AssertEquals("IsS162ATemporaryImport", false, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			entryHeader.ResetTotalsAndCachedValues();

			AssertEquals("N10 value", 150m, entryHeaderAsChargeProvider.N10CustomsValue);
			AssertEquals("N20 value", ZDecimal.Zero, entryHeaderAsChargeProvider.N20CustomsValue);
			AssertEquals("N30 value", ZDecimal.Zero, entryHeaderAsChargeProvider.N30CustomsValue);
			AssertEquals("IsS162ATemporaryImport", false, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			invoiceLine.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("N10 value", 50m, entryHeaderAsChargeProvider.N10CustomsValue);
			AssertEquals("N20 value", 100m, entryHeaderAsChargeProvider.N20CustomsValue);
			AssertEquals("N30 value", ZDecimal.Zero, entryHeaderAsChargeProvider.N30CustomsValue);
			AssertEquals("IsS162ATemporaryImport", false, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			invoiceLine2.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("N10 value", ZDecimal.Zero, entryHeaderAsChargeProvider.N10CustomsValue);
			AssertEquals("N20 value", 150m, entryHeaderAsChargeProvider.N20CustomsValue);
			AssertEquals("N30 value", ZDecimal.Zero, entryHeaderAsChargeProvider.N30CustomsValue);
			AssertEquals("IsS162ATemporaryImport", false, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			invoiceLine.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			invoiceLine2.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			entryHeader.ResetTotalsAndCachedValues();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport mode", TransportModeEnum.Sea, entryHeaderAsChargeProvider.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport mode", TransportModeEnum.Air, entryHeaderAsChargeProvider.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("Transport mode", TransportModeEnum.Post, entryHeaderAsChargeProvider.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Transport mode", TransportModeEnum.Other, entryHeaderAsChargeProvider.TransportMode);

			declaration.JE_TransportMode = "";
			AssertEquals("Transport mode", TransportModeEnum.Undefined, entryHeaderAsChargeProvider.TransportMode);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "354";
			invoiceLine.AddInfo.ZA_SCN = "123";
			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "354";
			invoiceLine2.AddInfo.ZA_SCN = "123";
			AssertEquals("IsS162ATemporaryImport", true, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "351";
			AssertEquals("IsS162ATemporaryImport", false, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "354";
			invoiceLine2.AddInfo.ZA_SCN = ZString.Empty;
			AssertEquals("IsS162ATemporaryImport", true, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = ZString.Empty;
			AssertEquals("IsS162ATemporaryImport", false, entryHeaderAsChargeProvider.IsS162ATemporaryImport);

			Assert("Not ExemptedFromCustomsAndQuarantineFees", !entryHeaderAsChargeProvider.IsExemptedFromCustomsAndQuarantineFees);

			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			Assert("IsExemptedFromCustomsAndQuarantineFees - No - must be both a diplomat and have Unaccompanied Personal Effects", !entryHeaderAsChargeProvider.IsExemptedFromCustomsAndQuarantineFees);

			declaration.AddInfo.ZA_UPEIndicator_Hidden = false;
			var importer = Factory.New<OrgHeader>();
			var cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.Diplomat;
			declaration.JE_OH_Importer = importer.PK;
			Assert("IsExemptedFromCustomsAndQuarantineFees - No - must be both a diplomat and have Unaccompanied Personal Effects", !entryHeaderAsChargeProvider.IsExemptedFromCustomsAndQuarantineFees);

			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			Assert("IsExemptedFromCustomsAndQuarantineFees - Yes", entryHeaderAsChargeProvider.IsExemptedFromCustomsAndQuarantineFees);
		}

		public void TestPaymentResponseLeadsToClearedCustomsLog()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);

			factory.Save();

			AssertEquals("Not cleared", ZDateTime.Empty, entryHeader.ClearanceDate);

			CMRIMDRMessage message = factory.New<CMRIMDRMessage>();
			message.EM_MessageText = TestMessages.IMDRMessageText;
			message.EM_LinkedObject = entryHeader;
			message.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(message);
			factory.Save();

			AssertEquals("Not cleared", ZDateTime.Empty, entryHeader.ClearanceDate);

			var mock = factory.NewMoq<CMRPAYRECMessage>();
			mock.Setup(m => m.EM_DateTimeInterchangeSent).Returns(ZDateTime.Now);
			var pAYRECMessage = mock.Object;
			pAYRECMessage.EM_MessageText = TestMessages.PAYRECMessageText;
			pAYRECMessage.EM_LinkedObject = entryHeader;
			pAYRECMessage.EM_ReceiveTransmit = "RCV";

			entryHeader.Messages.Add(pAYRECMessage);
			factory.Save();
			Assert("Cleared", entryHeader.ClearanceDate != ZDateTime.Empty);
		}

		public void TestDeclarationDateInAU()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			ZDateTime date1 = ZDateTime.Today.AddDays(-1);
			Assert("pre-condition", entryHeader.DeclarationDate.IsEmpty);
			testDec.ManualClearanceDate = date1;
			AssertEquals("manual clearance date returned", date1, entryHeader.DeclarationDate);
		}

		public void TestAgentReferenceForCMRForFAR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B99999999/1";
			AssertEquals("Agent Reference", "B99999999/1", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "B99999999/1 Agent Re", entryHeader.AgentReference);
		}

		public void TestAgentReferenceForLegacyFAR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B99999999/1";
			AssertEquals("Agent Reference", "B99999999/1", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "B99999999/1 Age", entryHeader.AgentReference);
		}

		public void TestAgentReferenceForCMRForNSR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B99999999/1";
			AssertEquals("Agent Reference", ZString.Empty, entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "Agent Reference", entryHeader.AgentReference);
		}

		public void TestAgentReferenceForLegacyNSR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B99999999/1";
			AssertEquals("Agent Reference", ZString.Empty, entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "Agent Reference", entryHeader.AgentReference);
		}

		public void TestAgentReferenceForCMRForPAR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00001000/1";

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			AssertEquals("Agent Reference", declaration.JE_DeclarationReference, entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "Agent Reference", entryHeader.AgentReference);
		}

		public void TestAgentReferenceForLegacyPAR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B99999999/1";
			Factory.Save();
			AssertEquals("Agent Reference", declaration.JE_DeclarationReference, entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "Agent Reference", entryHeader.AgentReference);
		}

		public void TestAgentReferenceForCMRForDEF()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.DEF;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B99999999/1";
			AssertEquals("Agent Reference", "B99999999/1", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "Agent Reference", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Re";
			AssertEquals("Agent Reference", "B99999999/1 Agent Re", entryHeader.AgentReference);
		}

		public void TestBugInAgentReferenceForDEF()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.DEF;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_DeclarationReference = "B99999999";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Agent Reference", "B99999999/1", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "Agent Reference", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Re";
			AssertEquals("Agent Reference", "B99999999/1 Agent Re", entryHeader.AgentReference);
		}

		public void TestAgentReferenceForLegacyDEF()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.DEF;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B99999999/1";
			AssertEquals("Agent Reference", "B99999999/1", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Agent Reference";
			AssertEquals("Agent Reference", "Agent Reference", entryHeader.AgentReference);

			declaration.JE_AgentsReference = "Age";
			AssertEquals("Agent Reference", "B99999999/1 Age", entryHeader.AgentReference);
		}

		public void TestAddInfoRegisteredAsEditibleChild()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("AddInfo is editible child", true, entryHeader.IsRegisteredEditableChildObject(entryHeader.AddInfo));
		}

		public void TestAmendedLines()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JobComInvoiceLines.AddNew();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDec = factory2.Load<JobDeclaration>(declaration.PK);
			reloadedDec.MessageInitiator = sender;
			reloadedDec.DoMerge();

			AssertEquals("Count of Amended Line", 1, reloadedDec.CustomsEntryHeaders[0].AmendedLines.Length);
		}

		public void TestAmendedLinesDeletedLinesRefreshedWhenMergeIsDone()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = sender;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();

			invoice1.JobComInvoiceLines.AddNew();
			invoice2.JobComInvoiceLines.AddNew();

			testDec.DoMerge();
			Factory.Save();

			AssertEquals("There should be two entry lines created", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Amended Lines", 2, testDec.CustomsEntryHeaders[0].AmendedLines.Length);
			AssertEquals("All Lines", 2, testDec.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("No delete pending lines", 0, testDec.CustomsEntryHeaders[0].PendingDeletedLinesForAmendment.Length);
			AssertEquals("No deleted lines with refund reason code", 0, testDec.CustomsEntryHeaders[0].DeletedLinesWithRefundReasonCode.Length);

			testDec.CustomsEntryHeaders[0].CH_HighestLineNumber = 2;
			testDec.CustomsEntryHeaders[0].EntryNumber = "AAA";
			testDec.CustomsEntryHeaders[0].AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.DoMerge();
			AssertEquals("There should be one entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Amended Lines", 2, testDec.CustomsEntryHeaders[0].AmendedLines.Length);
			AssertEquals("All Lines", 2, testDec.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("One delete pending line", 1, testDec.CustomsEntryHeaders[0].PendingDeletedLinesForAmendment.Length);
			AssertEquals("No deleted lines with refund reason code", 0, testDec.CustomsEntryHeaders[0].DeletedLinesWithRefundReasonCode.Length);

			var entryLine2 = testDec.CustomsEntryHeaders[0].AllEntryLines[1];
			AssertEquals("Refund reason code on line 2", "126A", entryLine2.RefundReasonCode);
			entryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;

			testDec.DoMerge();
			AssertEquals("There should be one entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Amended Lines", 2, testDec.CustomsEntryHeaders[0].AmendedLines.Length);
			AssertEquals("All Lines", 2, testDec.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("No delete pending line", 0, testDec.CustomsEntryHeaders[0].PendingDeletedLinesForAmendment.Length);
			AssertEquals("One deleted line with refund reason code", 1, testDec.CustomsEntryHeaders[0].DeletedLinesWithRefundReasonCode.Length);

			entryLine2.RefundReasonCode = ZString.Empty;
			testDec.DoMerge();
			AssertEquals("There should be one entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Amended Lines", 1, testDec.CustomsEntryHeaders[0].AmendedLines.Length);
			AssertEquals("All Lines", 2, testDec.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("No delete pending line", 0, testDec.CustomsEntryHeaders[0].PendingDeletedLinesForAmendment.Length);
			AssertEquals("No deleted line with refund reason code", 0, testDec.CustomsEntryHeaders[0].DeletedLinesWithRefundReasonCode.Length);
		}

		public void TestAmendedHouseBillContainerPacks()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;

			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JobComInvoiceLines.AddNew();

			declaration.JE_HouseBill = "1";
			Bill houseBill1 = declaration.Bills[0];
			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "2";

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDec = factory2.Load<JobDeclaration>(declaration.PK);
			reloadedDec.MessageInitiator = sender;
			reloadedDec.DoMerge();

			AssertEquals("Count of Amended Line", 2, reloadedDec.CustomsEntryHeaders[0].AmendedHouseBillContainerPacks.Length);
		}

		public void TestHouseBillContainersForEntry()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JobComInvoiceLines.AddNew();

			declaration.JE_HouseBill = "1";
			var houseBill1 = declaration.Bills[0];

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "2";

			declaration.DoMerge();
			AssertEquals("Count on entry headers", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Count for HouseBillContainer", 2, declaration.CustomsEntryHeaders[0].PackingGroups.Count);

			invoiceHeader1.AddInfo.ZA_EFD = "050505";
			invoiceHeader1.JZ_CU_RelatedHouseBill = houseBill1.PK;
			invoiceHeader2.JZ_CU_RelatedHouseBill = houseBill2.PK;
			declaration.DoMerge();
			AssertEquals("Count on entry headers", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Count for HouseBillContainer for entry one", 1, declaration.CustomsEntryHeaders[0].PackingGroups.Count);
			AssertEquals("Count for HouseBillContainer for entry two", 1, declaration.CustomsEntryHeaders[1].PackingGroups.Count);
		}

		public void TestCH_StatusForEntryLineNumber()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLineNumber5 = entryHeader.MergedLines.AddNew();
			entryLineNumber5.CL_LineNumber = 5;
			CusEntryLine entryLineNumber2 = entryHeader.MergedLines.AddNew();
			entryLineNumber2.CL_LineNumber = 2;
			CusEntryLine entryLineNumber10 = entryHeader.MergedLines.AddNew();
			entryLineNumber10.CL_LineNumber = 10;
			entryHeader.CH_Status = "TTT";

			AssertEquals("Declaration status is empty", true, declaration.JE_EntryStatus.IsEmpty);
			AssertEquals("Highest line number is empty", true, entryHeader.CH_HighestLineNumber.IsEmpty);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			entryHeader.CH_Status = "TT2";
			AssertEquals("Highest line number is empty", true, entryHeader.CH_HighestLineNumber.IsEmpty);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Highest line number is not empty", (ZShort)10, entryHeader.CH_HighestLineNumber);
		}

		public void TestCH_StatusForHouseBillContainerLineNumber()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			PackingGroup pack4 = declaration.PackingGroups.AddNew();
			pack4.CR_HouseContainerNumber = 4;
			pack4.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			PackingGroup pack2 = declaration.PackingGroups.AddNew();
			pack2.CR_HouseContainerNumber = 2;
			pack2.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			PackingGroup pack15 = declaration.PackingGroups.AddNew();
			pack15.CR_HouseContainerNumber = 15;
			pack15.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			entryHeader.CH_Status = "TTT";

			AssertEquals("Highest line number is empty", true, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden.IsEmpty);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			entryHeader.CH_Status = "TT2";
			AssertEquals("Highest line number is empty", true, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden.IsEmpty);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Highest line number is empty", (ZShort)15, entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden);
		}

		public void TestAQISServicePaymentAmount()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("AQIS service payment amount", 0m, entryHeader.AQISServicePaymentAmount);
			AssertEquals("No charge is created", 0, entryHeader.Charges.Count);
			AssertEquals("Entry header has no changes", false, entryHeader.HasChanges);

			entryHeader.AQISServicePaymentAmount = 100m;
			AssertEquals("One charge is created", 1, entryHeader.Charges.Count);
			AssertEquals("AQIS service amount from charge collection", 100m, entryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount));
			AssertEquals("AQIS service amount", 100m, entryHeader.AQISServicePaymentAmount);
			AssertEquals("Entry header has changes", true, entryHeader.HasChanges);
		}

		[TestDate(2005, 1, 1, 1, 1, 1)]
		public void TestClearanceDateEndToEnd()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			creator.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			creator.Entry1.CH_Status = "LDG";
			creator.Declaration.DoMerge();
			Factory.Save();
			AssertEquals(creator.Entry1.ClearanceDate, ZDateTime.Empty);
			creator.Entry1.CH_Status = "PAY";
			Factory.Save();
			AssertEquals(creator.Entry1.ClearanceDate, new ZDateTime(2005, 1, 1, 1, 1, 1));
			AssertEquals(((IBondedWarehouseTransactionLineProvider)creator.EntryLine1).TransactionLine.EntryDate, new ZDateTime(2005, 1, 1, 1, 1, 1));
		}

		public void TestIsNextMessageOriginalForCMR()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("IsNextMessageOriginalForCMR", true, entryHeader.IsNextMessageOriginalForCMR);

			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("IsNextMessageOriginalForCMR", true, entryHeader.IsNextMessageOriginalForCMR);

			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals("IsNextMessageOriginalForCMR", true, entryHeader.IsNextMessageOriginalForCMR);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			AssertEquals("IsNextMessageOriginalForCMR", true, entryHeader.IsNextMessageOriginalForCMR);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals("IsNextMessageOriginalForCMR", true, entryHeader.IsNextMessageOriginalForCMR);

			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			AssertEquals("IsNextMessageOriginalForCMR", false, entryHeader.IsNextMessageOriginalForCMR);
		}

		[TestDate(2005, 1, 1)]
		public void TestICPQAHeaderAttachee()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CL = entryLine.PK;

			AssertEquals("FKColumnInCusEntryCPDecTable", CusEntryCPDecSchema.ON_CH, ((ICPQAAttachee)entryHeader).FKColumnInCusEntryCPDecTable);
			AssertEquals("Selection date", new ZDateTime(2005, 1, 1), ((ICPQAAttachee)entryHeader).SelectionDate);

			invoice.AddInfo.ZA_EFD = "010105";
			AssertEquals("Selection date", new ZDateTime(2005, 1, 1), ((ICPQAAttachee)entryHeader).SelectionDate);
		}

		public void TestLodgementQuestionKeyWETQOrLCTQ()
		{
			JobDeclaration testDec = SetInvoiceAndEntry();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			JobComInvoiceLine line = testDec.FilteredInvoiceLines[0];

			line.AddInfo.ZA_WETQ = "Y";
			LodgementQuestionKeys key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsABNQuotedForLCTAndWET", true, key.IsABNQuotedForLCTAndWET);

			entryHeader.ResetTotalsAndCachedValues();
			line.AddInfo.ZA_WETQ = "N";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsABNQuotedForLCTAndWET", false, key.IsABNQuotedForLCTAndWET);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			entryHeader.ResetTotalsAndCachedValues();
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaid", true, key.IsPaid);
		}

		public void TestLodgementQuestionKeyContainers()
		{
			JobDeclaration testDec = SetInvoiceAndEntry();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			JobComInvoiceLine line = testDec.FilteredInvoiceLines[0];

			CusContainer container = testDec.CusContainers.AddNew();

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			LodgementQuestionKeys key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("HasFCL", true, key.HasFCLOrFCXLines);
			AssertEquals("HasLCL", false, key.HasLCLLines);

			entryHeader.ResetTotalsAndCachedValues();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("HasFCL", false, key.HasFCLOrFCXLines);
			AssertEquals("HasLCL", true, key.HasLCLLines);

			entryHeader.ResetTotalsAndCachedValues();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("HasFCL", true, key.HasFCLOrFCXLines);
			AssertEquals("HasLCL", false, key.HasLCLLines);
		}

		public void TestLodgementQuestionKeyIsPaid()
		{
			JobDeclaration testDec = SetInvoiceAndEntry();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			JobComInvoiceLine line = testDec.FilteredInvoiceLines[0];

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			LodgementQuestionKeys key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaid", true, key.IsPaid);

			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaid", false, key.IsPaid);

			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaid", true, key.IsPaid);

			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = false;
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaid", true, key.IsPaid);
		}

		public void TestLodgementQuestionKeyWhenPaidUnderProtest()
		{
			JobDeclaration testDec = SetInvoiceAndEntry();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			JobComInvoiceLine line = testDec.FilteredInvoiceLines[0];

			LodgementQuestionKeys key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaidUnderProtest", false, key.IsPaidUnderProtest);

			testDec.JE_PaidUnderProtestStatement = "Statement";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaidUnderProtest", false, key.IsPaidUnderProtest);

			entryHeader.ResetTotalsAndCachedValues();
			line.AddInfo.ZA_PUP = "Y";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsPaidUnderProtest", true, key.IsPaidUnderProtest);
		}

		public void TestLodgementQuestionKeyIsNature30()
		{
			JobDeclaration testDec = SetInvoiceAndEntry();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			JobComInvoiceLine line = testDec.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = testDec.FilteredInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;

			LodgementQuestionKeys key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsNature30", false, key.IsNature30);

			line.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsNature30", false, key.IsNature30);
			AssertEquals("IsNature20 as line 2 is not N20", false, key.IsNature20);

			entryHeader.ResetTotalsAndCachedValues();
			line2.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsNature20", true, key.IsNature20);

			entryHeader.ResetTotalsAndCachedValues();
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsNature30", true, key.IsNature30);

			entryHeader.ResetTotalsAndCachedValues();
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			testDec.JE_OH_Importer = importer.PK;
			AssertEquals("IsGST deferred", true, importer.MiscServ.IsGSTVATDeferred);
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsGSTDeferred", true, key.IsGSTDeferred);

			entryHeader.ResetTotalsAndCachedValues();
			importer.MiscServ.OM_IMIsGSTDeferred = false;
			AssertEquals("IsGST deferred", false, importer.MiscServ.IsGSTVATDeferred);
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsGSTDeferred", false, key.IsGSTDeferred);
		}

		public void TestLodgementQuestionKeyTheRest()
		{
			JobDeclaration testDec = SetInvoiceAndEntry();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			JobComInvoiceLine line = testDec.FilteredInvoiceLines[0];

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			LodgementQuestionKeys key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsSACWithoutLines", true, key.IsSAC);

			entryHeader.ResetTotalsAndCachedValues();
			testDec.JE_MessageSubType = "FRM";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsSACWithoutLines", false, key.IsSAC);

			entryHeader.ResetTotalsAndCachedValues();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsSea", true, key.IsSea);

			entryHeader.ResetTotalsAndCachedValues();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsSea", false, key.IsSea);

			JobComInvoiceLine invoiceLine2 = testDec.FilteredInvoiceLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_CustomsValue = 300m;
			line.CusEntryLine.CL_CustomsValue = 249m;
			entryHeader.ResetTotalsAndCachedValues();

			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("TotalCustomsValue", 549m, key.TotalCustomsValue);

			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.EntryNumber = "EntryNum";
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.RefundReasonCode = "AA";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsRefundAmendment", true, key.IsRefundAmendment);

			entryHeader.ResetTotalsAndCachedValues();
			entryLine.RefundReasonCode = "";
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsRefundAmendment", false, key.IsRefundAmendment);

			entryHeader.ResetTotalsAndCachedValues();
			testDec.AddInfo.ZA_UPEIndicator_Hidden = true;
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("IsRefundAmendment", true, key.IsUPEDeclaration);
		}

		public void TestFlatDutyPortion()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.FlatDutyPortion, 100m);

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.FlatDutyPortion, 200m);

			AssertEquals("Flat duty portion", 300m, entryHeader.FlatDutyPortion);
		}

		public void TestIStatusNeedsRecalculationProvider()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImport CMR", true, testDec.IsImportCMR);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Does not need recalculation yet", false, ((IStatusNeedsRecalculationProvider)entryHeader).StatusNeedsRecalculation);

			entryHeader.Messages.AddNew().EM_MessageText = "A";
			AssertEquals("Need recalculation", true, ((IStatusNeedsRecalculationProvider)entryHeader).StatusNeedsRecalculation);

			AssertEquals("Messages collection", entryHeader.Messages, ((IStatusNeedsRecalculationProvider)entryHeader).Messages);
		}

		public void TestStatusNeedsRecalculation_MessagesShouldNotBeLoadedJustForThisPurpose()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImport CMR", true, testDec.IsImportCMR);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Pre-condition, should not be loaded", false, entryHeader.MessagesAreLoaded);
			AssertEquals("Should not require calculation as the messages have not been loaded into the collection", false, ((IStatusNeedsRecalculationProvider)entryHeader).StatusNeedsRecalculation);
			AssertEquals("Should not be loaded", false, entryHeader.MessagesAreLoaded);
		}

		public void TestCountervailingDuty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 100m);

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 200m);

			AssertEquals("Countervailing Duty", 300m, entryHeader.CountervailingDuty);
		}

		public void TestDumpingDuty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 100m);

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 200m);

			AssertEquals("Dumping Duty", 300m, entryHeader.DumpingDuty);
		}

		public void TestSecurityAmounts()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityConcession, 100m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityLiability, 10m);
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityConcession, 200m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityLiability, 20m);
			AssertEquals("Securiy Concession", 300m, entryHeader.TotalSecurityConcession);
			AssertEquals("Securiy Liability", 30m, entryHeader.TotalSecurityLiability);
		}

		public void TestRecycleOfEntryHeaderResetsPackageGroupLineNumbers()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddDays(1);
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;

			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "2";
			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();

			declaration.JE_HouseBill = "1";
			Bill houseBill1 = declaration.Bills[0];

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "2";

			invoiceHeader1.JZ_ValuationDateOverride = ZDateTime.Today;
			invoiceHeader1.JZ_CU_RelatedHouseBill = houseBill1.PK;
			invoiceHeader2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			invoiceHeader2.JZ_CU_RelatedHouseBill = houseBill2.PK;
			declaration.DoMerge();
			houseBill1.PackingGroups[0].CR_HouseContainerNumber = 1;
			houseBill2.PackingGroups[0].CR_HouseContainerNumber = 1;
			AssertEquals("Count on entry headers", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Count for HouseBillContainer for entry one", 1, declaration.CustomsEntryHeaders[0].PackingGroups.Count);
			AssertEquals("Count for HouseBillContainer for entry two", 1, declaration.CustomsEntryHeaders[1].PackingGroups.Count);
			AssertEquals("HouseBill1 has 1 packing group", 1, houseBill1.PackingGroups.Count);
			AssertEquals("HouseBill1 packing group has line number 1", (ZShort)1, houseBill1.PackingGroups[0].CR_HouseContainerNumber);
			AssertEquals("HouseBill2 has 1 packing group", 1, houseBill2.PackingGroups.Count);
			AssertEquals("HouseBill2 packing group has line number 1", (ZShort)1, houseBill2.PackingGroups[0].CR_HouseContainerNumber);

			declaration.CustomsEntryHeaders[0].CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.CustomsEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			declaration.CustomsEntryHeaders[0].EntryNumber = "1";
			declaration.CustomsEntryHeaders[1].CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.CustomsEntryHeaders[1].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			declaration.CustomsEntryHeaders[1].EntryNumber = "2";
			Factory.Save();

			declaration.CustomsEntryHeaders[1].CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			declaration.CustomsEntryHeaders[1].CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("Withdrawn", true, declaration.CustomsEntryHeaders[1].HasBeenWithdrawn);

			invoiceHeader2.JZ_ValuationDateOverride = ZDateTime.Today;
			declaration.DoMerge();

			AssertEquals("Count for HouseBillContainer for entry one", 2, declaration.CustomsEntryHeaders[0].PackingGroups.Count);
			AssertEquals("HouseBill1 has line number 1", (ZShort)1, houseBill1.PackingGroups[0].CR_HouseContainerNumber);
			AssertEquals("HouseBill2 has line number 0", (ZShort)0, houseBill2.PackingGroups[0].CR_HouseContainerNumber);

			AssertEquals("Error Reported", "CR_HouseContainerNumber(s) (1) set to 0 when existing value > 0 and entry recycled, but entry not deleted. Job: B00001000, Entry: 2", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestEffectiveDutyDateForEdifice()
		{
			JobDeclaration declaration = SetupImportDec();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();

			declaration.DoMerge();
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Effective Duty Date", ZDateTime.Today, customsHeader.EffectiveDutyDate);

			header.AddInfo.ZA_EFD = "050505";
			AssertEquals("Effective Duty Date", header.EffectiveDutyDate, customsHeader.EffectiveDutyDate);
		}

		/// <summary>
		/// OtherCharge2 is non-dutiable charges
		/// </summary>
		public void TestOtherCharge2ComesWithMinusForMessageWrapper()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = "LEG";
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 100, audCurrency.RX_Code);
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = false;
			charge.J7_IsIncludedInITOT = true;

			header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 500m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 500m;

			declaration.DoMerge();
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("PreCondition: The InvoiceHeader should be balanced", 0m, header.JZ_Calc_Balance);
			AssertEquals("Other charge2 should be expressed with minus", -100m, customsHeader.OtherCharges2.Amount);
		}

		public void TestMergedChargesInSameCurrency()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			BaseJobComInvHeaderCharge lCH = header.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100, audCurrency.RX_Code);
			lCH.J7_IsIncludedInITOT = true;

			header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 500m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 500m;

			declaration.DoMerge();

			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];

			ZDecimal expected = (400 + 500) / (1000 - 100) * 100m;

			AssertEquals("Merged Charges", expected, customsHeader.LandingCharges.Amount);
		}

		public void TestMergedChargesInDifferentCurrency()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			BaseJobComInvHeaderCharge lCH = header.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 70);
			lCH.J7_IsIncludedInITOT = true;
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 500m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 500m;

			declaration.DoMerge();
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("Merged Charges", 70m, customsHeader.LandingCharges.Amount);
			AssertEquals("Merged Charges Currency", audCurrency, customsHeader.LandingCharges.Currency);
		}

		public void TestIsPrimeEntry()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("IsPrimeEntry", false, customsHeader.IsPrimeEntry);
			CusEntryHeader customsHeader2 = declaration.CustomsEntryHeaders.AddNew();
			customsHeader2.CH_CH_PrimeEntry = customsHeader.PK;
			AssertEquals("IsPrimeEntry", true, customsHeader.IsPrimeEntry);
			customsHeader2.CH_CH_PrimeEntry = ZGuid.Empty;
			AssertEquals("IsPrimeEntry", false, customsHeader.IsPrimeEntry);
		}

		public void TestIsEnclosureEntry()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("IsEnclosureEntry", false, customsHeader.IsEnclosureEntry);
			CusEntryHeader customsHeader2 = declaration.CustomsEntryHeaders.AddNew();
			customsHeader.CH_CH_PrimeEntry = customsHeader2.PK;
			AssertEquals("IsEnclosureEntry", true, customsHeader.IsEnclosureEntry);
			customsHeader.CH_CH_PrimeEntry = ZGuid.Empty;
			AssertEquals("IsEnclosureEntry", false, customsHeader.IsEnclosureEntry);
		}

		public void TestIsNormalEntry()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("IsNormalEntry", true, customsHeader.IsNormalEntry);
			CusEntryHeader customsHeader2 = declaration.CustomsEntryHeaders.AddNew();
			customsHeader.CH_CH_PrimeEntry = customsHeader2.PK;
			AssertEquals("IsNormalEntry", false, customsHeader.IsNormalEntry);
			customsHeader.CH_CH_PrimeEntry = ZGuid.Empty;
			AssertEquals("IsNormalEntry", true, customsHeader.IsNormalEntry);
			customsHeader2.CH_CH_PrimeEntry = customsHeader.PK;
			AssertEquals("IsNormalEntry", false, customsHeader.IsNormalEntry);
			customsHeader2.CH_CH_PrimeEntry = ZGuid.Empty;
			AssertEquals("IsNormalEntry", true, customsHeader.IsNormalEntry);
		}

		public void TestEntryFee_ForLegacy()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 140m);
			AssertEquals("EntryFee for CMR", 140m, entryHeader.EntryFee);
		}

		public void TestEntryFee_ForCMR()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 140m);
			AssertEquals("EntryFee for CMR", 140m, entryHeader.EntryFee);
		}

		public void TestEntryFee_ForWeeklySettlementsEntry()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_SettlementPeriodType = "SW";

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals("No EntryFee applies for a Weekly Settlement Entry. Fee returned should be zero.", 0m, entryHeader.EntryFee);
		}

		public void TestExceedingMaximumOfThreeCurrenciesStopsEdificeMessage()
		{
			JobDeclaration declaration = SetupImportDec();
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			header.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100, audCurrency.RX_Code);
			header.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 10, nzdCurrency.RX_Code);
			header.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10, usdCurrency.RX_Code);
			header.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50, hkdCurrency.RX_Code);

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 400m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 500m;

			declaration.DoMerge();

			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("CustomsHeader.UsedCurrencies.Length", 4, customsHeader.UsedCurrencies.Length);
			customsHeader.RunPreSaveValidation();
			AssertEquals("Expected To many currencies error", true, declaration.HasMessageErrors);
		}

		public void TestUsedCurrenciesOneCurrency()
		{
			JobDeclaration declaration = SetupImportDec();
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			header.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100);

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 400m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 500m;

			declaration.DoMerge();
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("CustomsHeader.UsedCurrencies.Length", 1, customsHeader.UsedCurrencies.Length);
			AssertEquals("CustomsHeader.UsedCurrencies[0]", audCurrency, customsHeader.UsedCurrencies[0]);
		}

		public void TestUsedCurrenciesTwoCurrency()
		{
			JobDeclaration declaration = SetupImportDec();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100, usdCurrency.RX_Code);

			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 400m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 500m;

			declaration.DoMerge();
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("CustomsHeader.UsedCurrencies.Length", 2, customsHeader.UsedCurrencies.Length);
			AssertEquals("CustomsHeader.UsedCurrencies[0]", audCurrency, customsHeader.UsedCurrencies[0]);
			AssertEquals("CustomsHeader.UsedCurrencies[1]", usdCurrency, customsHeader.UsedCurrencies[1]);
		}

		public void TestUsedCurrenciesIncludesAdjustmentCurrencies()
		{
			JobDeclaration declaration = SetupImportDec();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100, usdCurrency.RX_Code);

			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 400m;
			line1.JI_AddInfo = "ADJ=10HKD";

			declaration.DoMerge();
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("CustomsHeader.UsedCurrencies.Length", 3, customsHeader.UsedCurrencies.Length);
			AssertEquals("CustomsHeader.UsedCurrencies[0]", audCurrency.RX_Code, customsHeader.UsedCurrencies[0].Code);
			AssertEquals("CustomsHeader.UsedCurrencies[1]", usdCurrency.RX_Code, customsHeader.UsedCurrencies[1].Code);
			AssertEquals("CustomsHeader.UsedCurrencies[2]", hkdCurrency.RX_Code, customsHeader.UsedCurrencies[2].Code);
		}

		public void TestUsedCurrenciesIncludesDumpingExportPriceCurrencies()
		{
			JobDeclaration declaration = SetupImportDec();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100, usdCurrency.RX_Code);

			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 400m;
			line1.JI_AddInfo = "DXP=10HKD";

			declaration.DoMerge();
			CusEntryHeader customsHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("CustomsHeader.UsedCurrencies.Length", 3, customsHeader.UsedCurrencies.Length);
			AssertEquals("CustomsHeader.UsedCurrencies[0]", audCurrency.RX_Code, customsHeader.UsedCurrencies[0].Code);
			AssertEquals("CustomsHeader.UsedCurrencies[1]", usdCurrency.RX_Code, customsHeader.UsedCurrencies[1].Code);
			AssertEquals("CustomsHeader.UsedCurrencies[2]", hkdCurrency.RX_Code, customsHeader.UsedCurrencies[2].Code);
		}

		public void TestCustomsFactor()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 3813.48m, audCurrency.RX_Code);
			oFT.J7_IsIncludedInITOT = true;
			JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_OH_Supplier = GetValidSupplier();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 17970.18m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;

			JobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 17970.18m;

			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("Invoice Header 1 Customs Factor", 0.787788m, decimal.Round(entryHeader.CustomsFactor, 6));
		}

		public void TestCustomsFactorWithNature10And20()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (declaration.GetValidationSuspender())
			{
				JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, audCurrency.RX_Code);
				oFT.J7_IsIncludedInITOT = true;
				JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader1.JZ_OH_Supplier = GetValidSupplier();
				invoiceHeader1.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
				invoiceHeader1.JZ_InvoiceAmount = 1000m;
				invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;
				invoiceHeader1.JZ_Nature10PackCount = 10;
				invoiceHeader1.JZ_BondPackCount = 20;

				JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 200m;
				invoiceLine1.JI_IsPackToBondForLine = true;

				JobComInvoiceLine invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 800m;
				invoiceLine2.JI_IsPackToBondForLine = false;

				declaration.DoMerge();
				AssertEquals("Two customs entry headers", 2, declaration.CustomsEntryHeaders.Count);
			}

			ZDecimal line1FOB = 200m * 0.9m;
			ZDecimal line2FOB = 800m * 0.9m;
			ZDecimal line1Factor = line1FOB / 200;
			ZDecimal line2Factor = line2FOB / 800;

			AssertEquals("Cus Header 1 Customs Factor", line1Factor, declaration.CustomsEntryHeaders[0].CustomsFactor);
		}

		public void TestCustomsFactorSimpleTest()
		{
			var rate = Factory.LoadTop1<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, usdCurrency.RX_Code));
			using (rate.GetValidationSuspender())
			{
				rate.RE_StartDate = new ZDateTime(2003, 11, 13);
				rate.RE_ExpiryDate = new ZDateTime(2003, 11, 14);
				rate.RE_SellRate = 0.7010m;
				rate.RE_GC = GlbCompany.CurrentCompany.PK;
				rate.RE_ExRateType = "CUS";
				rate.RE_OH_Client = ZGuid.Empty;
			}
			Factory.Save();

			JobDeclaration declaration = SetupImportDec();
			using (declaration.GetValidationSuspender())
			{
				declaration.JE_ExportDate = new ZDateTime(2003, 11, 14);
				JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader1.JZ_OH_Supplier = GetValidSupplier();
				invoiceHeader1.JZ_RX_NKInvoice_Currency = usdCurrency.RX_Code;
				invoiceHeader1.JZ_InvoiceAmount = 1000m;
				invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				JobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;
			}
			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Customs Factor", 1.42653352m, entryHeader.CustomsFactor);
		}

		public void TestNature10Packages()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, audCurrency.RX_Code);

			JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_OH_Supplier = GetValidSupplier();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoiceHeader1.JZ_Nature10PackCount = 10;
			invoiceHeader1.JZ_BondPackCount = 20;

			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine1.JI_IsPackToBondForLine = true;

			JobComInvoiceLine invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 800m;
			invoiceLine2.JI_IsPackToBondForLine = false;

			declaration.DoMerge();

			AssertEquals("Cus Header 1 N10 Packs", 0, declaration.CustomsEntryHeaders[0].Nature10Packages);
			AssertEquals("Cus Header 2 N10 Packs", 10, declaration.CustomsEntryHeaders[1].Nature10Packages);
		}

		public void TestPackageCountForInvoiceHeaders()
		{
			JobDeclaration declaration = SetupImportDec();

			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_Nature10PackCount = 10;

			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_Nature10PackCount = 25;

			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();

			declaration.DoMerge();

			AssertEquals("Total Packs", 35, declaration.CustomsEntryHeaders[0].PackageCountForInvoiceHeaders);
		}

		public void TestPackagesForEdifice()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);

			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_BondPackCount = 20;
			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_IsPackToBondForLine = true;
			JobComInvoiceLine invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_IsPackToBondForLine = false;

			declaration.DoMerge();
			AssertEquals("Packages", 20, declaration.CustomsEntryHeaders[0].PackagesCount);
		}

		public void TestPacakgesForCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();

			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "Container1";
			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "Container2";

			declaration.JE_HouseBill = "HouseBill";

			Package pack1 = declaration.Packages.AddNew();
			pack1.CW_PackQty = 120;
			pack1.CW_HouseBill = declaration.Bills[0].CU_BillUniqueCode;
			pack1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			Package pack2 = declaration.Packages.AddNew();
			pack2.CW_HouseBill = declaration.Bills[0].CU_BillUniqueCode;
			pack2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			pack2.CW_PackQty = 220;

			Factory.Save();
			declaration.DoMerge();
			AssertEquals("Packages", 340, declaration.CustomsEntryHeaders[0].PackagesCount);
		}

		public void TestNature20Packages()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, audCurrency.RX_Code);

			JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_OH_Supplier = GetValidSupplier();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoiceHeader1.JZ_Nature10PackCount = 10;
			invoiceHeader1.JZ_BondPackCount = 20;

			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine1.JI_IsPackToBondForLine = true;

			JobComInvoiceLine invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 800m;
			invoiceLine2.JI_IsPackToBondForLine = false;

			declaration.DoMerge();

			AssertEquals("Cus Header 1 N20 Packs", 20, declaration.CustomsEntryHeaders[0].Nature20Packages);
			AssertEquals("Cus Header 2 N20 Packs", 0, declaration.CustomsEntryHeaders[1].Nature20Packages);
		}

		public void TestWeightForSplitNature()
		{
			JobDeclaration declaration = SetupImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (declaration.GetValidationSuspender())
			{
				JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, audCurrency.RX_Code);

				JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader1.JZ_OH_Supplier = GetValidSupplier();
				invoiceHeader1.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
				invoiceHeader1.JZ_InvoiceAmount = 1000m;
				invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
				invoiceHeader1.JZ_Nature10PackCount = 10;
				invoiceHeader1.JZ_BondPackCount = 20;
				invoiceHeader1.JZ_Weight = 90;
				invoiceHeader1.JZ_WeightUQ = "KG";
				JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 200m;
				invoiceLine1.JI_IsPackToBondForLine = true;

				JobComInvoiceLine invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 800m;
				invoiceLine2.JI_IsPackToBondForLine = false;
			}

			declaration.DoMerge();

			AssertEquals("Cus Header 1 Weight", 60, (int)ZArchitecture.Core.Utilities.Round((decimal)declaration.CustomsEntryHeaders[0].GrossWeight.Amount, 0));
			AssertEquals("Cus Header 1 WeightUQ", "KG", declaration.CustomsEntryHeaders[0].GrossWeight.Unit);
			AssertEquals("Cus Header 2 Weight", 30, (int)ZArchitecture.Core.Utilities.Round((decimal)declaration.CustomsEntryHeaders[1].GrossWeight.Amount, 0));
			AssertEquals("Cus Header 2 WeightUQ", "KG", declaration.CustomsEntryHeaders[1].GrossWeight.Unit);
		}

		public void TestWeightForMergedInvoices()
		{
			JobDeclaration declaration = SetupImportDec();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, audCurrency.RX_Code);

			JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_OH_Supplier = GetValidSupplier();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoiceHeader1.JZ_Nature10PackCount = 10;
			invoiceHeader1.JZ_Weight = 90;
			invoiceHeader1.JZ_WeightUQ = "KG";
			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine1.JI_IsPackToBondForLine = false;

			JobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_OH_Supplier = GetValidSupplier();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			invoiceHeader2.JZ_InvoiceAmount = 1000m;
			invoiceHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoiceHeader2.JZ_Nature10PackCount = 10;
			invoiceHeader2.JZ_Weight = 1.5m;
			invoiceHeader2.JZ_WeightUQ = "T";
			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 800m;
			invoiceLine2.JI_IsPackToBondForLine = false;

			declaration.DoMerge();

			AssertEquals("Cus Header 1 Weight", 1590, (int)ZArchitecture.Core.Utilities.Round((decimal)declaration.CustomsEntryHeaders[0].GrossWeight.Amount, 0));
			AssertEquals("Cus Header 1 WeightUQ", "KG", declaration.CustomsEntryHeaders[0].GrossWeight.Unit);
		}

		public void TestTotalAmountPayable()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			testDec.CustomsEntryHeaders.Add(header);
			CusEntryLine line = Factory.New<CusEntryLine>();
			header.MergedLines.Add(line);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 33.89m);

			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 1201.76m);

			AssertEquals("TotalPayableAmount", 1235.65m, header.TotalAmountPayable);
		}

		public void TestHasBeenMergedWithMessageErrors()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.Validation.ValidateAll();

			CusEntryHeader header1 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header3 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("HasBeenMergedWithMessageErrors", true, header1.HasBeenMergedWithMessageErrors);
			AssertEquals("HasBeenMergedWithMessageErrors", true, header2.HasBeenMergedWithMessageErrors);
			AssertEquals("HasBeenMergedWithMessageErrors", true, header3.HasBeenMergedWithMessageErrors);
		}

		public void TestPopulateCH_BGMReferenceIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			declaration.JE_DeclarationReference = "B00000001";
			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			var header3 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("CH_BGMReference", "", header1.CH_BGMReference);
			header1.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/1", header1.CH_BGMReference);

			header3.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/3", header3.CH_BGMReference);

			header2.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/2", header2.CH_BGMReference);
		}

		public void TestPopulateCH_BGMReferenceIfNeededForDecWithMessageErrors()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.Validation.ValidateAll();

			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			var header3 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("CH_BGMReference", "", header1.CH_BGMReference);
			header1.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/1MESSAGEERRORS", header1.CH_BGMReference);

			header3.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/3MESSAGEERRORS", header3.CH_BGMReference);

			header2.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/2MESSAGEERRORS", header2.CH_BGMReference);
		}

		public void TestPopulateCH_BGMReferenceIfNeededForExportOrImportDecWithMessageErrors()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.Validation.ValidateAll();

			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			var header3 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("CH_BGMReference", "", header1.CH_BGMReference);
			header1.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/1", header1.CH_BGMReference);

			header3.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/3", header3.CH_BGMReference);

			header2.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/2", header2.CH_BGMReference);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000002";
			header1.CH_BGMReference = header2.CH_BGMReference = header3.CH_BGMReference = ZString.Empty;
			declaration.Validation.ValidateAll();
			header1.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000002/1", header1.CH_BGMReference);

			header3.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000002/3", header3.CH_BGMReference);

			header2.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000002/2", header2.CH_BGMReference);
		}

		public void TestCH_BGMReferencePopulatedOnSave()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader header1 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header3 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("BMReference1", declaration.JE_DeclarationReference + "/1", header1.CH_BGMReference);
			AssertEquals("BMReference2", declaration.JE_DeclarationReference + "/2", header2.CH_BGMReference);
			AssertEquals("BMReference3", declaration.JE_DeclarationReference + "/3", header3.CH_BGMReference);
		}

		public void TestCH_BGMReferencePopulatedOKwhenEntryReset()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			AssertContains("Correct BGM Reference Number", "/1", entryHeader.CH_BGMReference);

			CMRIMDMessage discardedMessage1 = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			discardedMessage1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			discardedMessage1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SendersReferencePlaceHolder;
			CMRIMDMessage discardedMessage2 = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			discardedMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			discardedMessage2.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SendersReferencePlaceHolder;
			declaration.SetMessageParentToJobDeclaration();
			entryHeader.CH_BGMReference = "";
			Factory.Save();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			AssertContains("Correct BGM Reference Number", "/3", entryHeader.CH_BGMReference);
		}

		[ExpectException(typeof(Exception))]
		public void TestCH_BGMReferenceBlankedIfNotSavedProperly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader header1 = declaration.CustomsEntryHeaders.AddNew(typeof(TestHelperEntryHeaderThatThrowsExceptionWhileSaving));
			try
			{
				Factory.Save();
			}
			finally
			{
				AssertEquals("BGMReference", "", header1.CH_BGMReference);
			}
		}

		public void TestSettingEntryNumberCreatesCusEntryNumberRecord()
		{
			const string TestEntryNumber = "1M192730109";
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertEquals("Header Entry Number", null, header.CusEntryNumber);
			header.EntryNumber = TestEntryNumber;
			AssertEquals("Entry Number", TestEntryNumber, header.CusEntryNumber.CE_EntryNum);

			var entryNumFromFactory = Factory.LoadTop1<AUCusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_EntryNum, TestEntryNumber));
			AssertEquals("Entry Number from Factory", TestEntryNumber, entryNumFromFactory.CE_EntryNum);
		}

		public void TestMinimumLine()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invHeader1.JZ_InvoiceNumber = "BBB";
			JobComInvoiceHeader invHeader2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invHeader2.JZ_InvoiceNumber = "AAA";

			CusEntryHeader header = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = header.MergedLines.AddNew();
			CusEntryLine entryLine2 = header.MergedLines.AddNew();

			JobComInvoiceLine invLine1 = invHeader1.JobComInvoiceLines.AddNew();
			invLine1.JI_LineNo = 1;
			JobComInvoiceLine invLine2 = invHeader1.JobComInvoiceLines.AddNew();
			invLine2.JI_LineNo = 2;
			JobComInvoiceLine invLine3 = invHeader2.JobComInvoiceLines.AddNew();
			invLine3.JI_LineNo = 4;
			JobComInvoiceLine invLine4 = invHeader2.JobComInvoiceLines.AddNew();
			invLine3.JI_LineNo = 3;

			invLine1.JI_CL = entryLine1.PK;
			invLine2.JI_CL = entryLine1.PK;
			invLine3.JI_CL = entryLine2.PK;
			invLine4.JI_CL = entryLine2.PK;

			AssertEquals("MinimumLine", invLine4, header.MinimumLine);
		}

		public void TestGetImpendiments()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.LogImpediment("Declaration red-lined");
			header.LogImpediment("Declaration amber-lined");
			AssertEquals("Impendments", "Declaration red-lined\r\nDeclaration amber-lined\r\n", header.GetImpediments());
		}

		public void TestLogImpediment()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.LogImpediment("Declaration red-lined");
			AssertEquals("ImpedimentCount", 1, header.ImpedimentLogs.Count);
		}

		public void TestClearImpediments()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.LogImpediment("Declaration red-lined");
			AssertEquals("ImpedimentCount", 1, header.ImpedimentLogs.Count);
			AssertEquals("Impendments", "Declaration red-lined\r\n", header.GetImpediments());
			header.ImpedimentLogs.CancelAll();
			AssertEquals("ImpedimentCount", 0, header.ImpedimentLogs.Count);
			AssertEquals("Impendments", "", header.GetImpediments());
		}

		public void TestACSBarrierStatus()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertEquals("BarrierStatus", CusEntryHeader.ACSBarrierStatus.None, header.BarrierStatus);
			header.BarrierStatus = CusEntryHeader.ACSBarrierStatus.PaperClearanceApplies;
			AssertEquals("BarrierStatus", CusEntryHeader.ACSBarrierStatus.PaperClearanceApplies, header.BarrierStatus);
			header.BarrierStatus = CusEntryHeader.ACSBarrierStatus.Get("Y");
			AssertEquals("BarrierStatus", CusEntryHeader.ACSBarrierStatus.Clear, header.BarrierStatus);
		}

		public void TestACSCommercialStatus()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertEquals("CommercialStatus", CusEntryHeader.ACSCommercialStatusList.None, header.ACSCommercialStatus);
			header.ACSCommercialStatus = CusEntryHeader.ACSCommercialStatusList.ClearButNotPaid;
			AssertEquals("CommercialStatus", CusEntryHeader.ACSCommercialStatusList.ClearButNotPaid, header.ACSCommercialStatus);
			header.ACSCommercialStatus = CusEntryHeader.ACSCommercialStatusList.Get("Y");
			AssertEquals("CommercialStatus", CusEntryHeader.ACSCommercialStatusList.Clear, header.ACSCommercialStatus);
		}

		public void TestAQISBarrierStatus()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertEquals("BarrierStatus", CusEntryHeader.AQISBarrierStatusList.None, header.AQISBarrierStatus);
			header.AQISBarrierStatus = CusEntryHeader.AQISBarrierStatusList.AwaitingReporting;
			AssertEquals("BarrierStatus", CusEntryHeader.AQISBarrierStatusList.AwaitingReporting, header.AQISBarrierStatus);
			header.AQISBarrierStatus = CusEntryHeader.AQISBarrierStatusList.Get("Y");
			AssertEquals("BarrierStatus", CusEntryHeader.AQISBarrierStatusList.Clear, header.AQISBarrierStatus);
		}

		public void TestAQISCommercialStatus()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertEquals("CommercialStatus", CusEntryHeader.AQISCommercialStatusList.None, header.AQISCommercialStatus);
			header.AQISCommercialStatus = CusEntryHeader.AQISCommercialStatusList.NotClear;
			AssertEquals("CommercialStatus", CusEntryHeader.AQISCommercialStatusList.NotClear, header.AQISCommercialStatus);
			header.AQISCommercialStatus = CusEntryHeader.AQISCommercialStatusList.Get("Y");
			AssertEquals("CommercialStatus", CusEntryHeader.AQISCommercialStatusList.Clear, header.AQISCommercialStatus);
		}

		public void TestEntryStatusTransmit()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertEquals("EntryStatusTransmit", CusEntryHeader.EntryStatusTransmitList.None, header.EntryStatusTransmit);
			header.EntryStatusTransmit = CusEntryHeader.EntryStatusTransmitList.None;
			AssertEquals("EntryStatusTransmit", CusEntryHeader.EntryStatusTransmitList.None, header.EntryStatusTransmit);
			header.EntryStatusTransmit = CusEntryHeader.EntryStatusTransmitList.Get("Y");
			AssertEquals("EntryStatusTransmit", CusEntryHeader.EntryStatusTransmitList.Transmitted, header.EntryStatusTransmit);
		}

		public void TestEntryStatusConditions()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertEquals("EntryStatusConditions", CusEntryHeader.EntryStatusConditionsList.None, header.EntryStatusConditions);
			header.EntryStatusConditions = CusEntryHeader.EntryStatusConditionsList.Get("IM");
			AssertEquals("EntryStatusConditions", "Subject to Imported Food Inspection Service (IFIP); Subject to Motor Vehicle Standards Act (MVSA)", header.EntryStatusConditions.Description);
		}

		public void TestCH_CargoStatus()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.ACSCommercialStatus = CusEntryHeader.ACSCommercialStatusList.ClearButNotPaid;
			header.AQISBarrierStatus = CusEntryHeader.AQISBarrierStatusList.AwaitingReporting;
			header.AQISCommercialStatus = CusEntryHeader.AQISCommercialStatusList.NotClear;
			header.EntryStatusConditions = CusEntryHeader.EntryStatusConditionsList.Get("IM");
			AssertEquals("CH_CargoStatus", "ACS Status: Commercial=P, Quarantine Status: Barrier=X, Commercial=N, Entry: Conditions=IM", header.CH_CargoStatus);
		}

		public void TestCH_CargoStatusLongDescription()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.BarrierStatus = CusEntryHeader.ACSBarrierStatus.PaperClearanceApplies;
			header.ACSCommercialStatus = CusEntryHeader.ACSCommercialStatusList.ClearButNotPaid;
			header.AQISBarrierStatus = CusEntryHeader.AQISBarrierStatusList.SubjectToQuarantine;
			header.EntryStatusTransmit = CusEntryHeader.EntryStatusTransmitList.NotTransmitted;
			header.EntryStatusConditions = CusEntryHeader.EntryStatusConditionsList.Get("IM");
			header.EntryStatusTransmit = CusEntryHeader.EntryStatusTransmitList.NotTransmitted;
			AssertEquals("CH_CargoStatusLongDescription", "ACS Barrier Status: 'P' - Paper clearance applies\r\nACS Commercial Status: 'P' - ACS Commercial clear but NOT paid\r\nQuarantine Barrier Status: 'Q' - Subject to Quarantine\r\nEntry Status Conditions: 'IM' - Subject to Imported Food Inspection Service (IFIP); Subject to Motor Vehicle Standards Act (MVSA)\r\nEntry Status Transmit: '-' - Status has not been transmitted\r\n", header.CH_CargoStatusLongDescription);
		}

		public void TestOverseasInsuranceRoundsCorrectly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();

			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders[0].MergedLines.AddNew();
			declaration.CustomsEntryHeaders[0].MergedLines.AddNew();
			declaration.CustomsEntryHeaders[0].MergedLines.AddNew();

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CL = declaration.CustomsEntryHeaders[0].MergedLines[0].PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1].JI_CL = declaration.CustomsEntryHeaders[0].MergedLines[1].PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[2].JI_CL = declaration.CustomsEntryHeaders[0].MergedLines[2].PK;

			JobComInvoiceHeader invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			invoice.JZ_InvoiceAmount = 3000;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			BaseJobComInvHeaderCharge insuranceCharge = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, audCurrency.RX_Code);
			insuranceCharge.J7_IsIncludedInITOT = true;

			invoice.JobComInvoiceLines[0].JI_LinePrice = 1000;
			invoice.JobComInvoiceLines[1].JI_LinePrice = 1000;
			invoice.JobComInvoiceLines[2].JI_LinePrice = 1000;
			declaration.ResumeApportionment();
			AssertEquals(100m, declaration.CustomsEntryHeaders[0].OverseasInsurance.Amount);
		}

		public void TestAssignedBGMReferenceForResetCMRImportDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			declaration.CustomsEntryHeaders.AddNew();

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			AssertEquals("Correct BGM Reference Number", true, declaration.CustomsEntryHeaders[0].CH_BGMReference.Contains("/1"));

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.CustomsEntryHeaders.AddNew();

			CMRIMDMessage discardedMessage1 = (CMRIMDMessage)declaration.Messages.AddNew(typeof(CMRIMDMessage));
			discardedMessage1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			discardedMessage1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SendersReferencePlaceHolder;
			CMRIMDMessage discardedMessage2 = (CMRIMDMessage)declaration.Messages.AddNew(typeof(CMRIMDMessage));
			discardedMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			discardedMessage2.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SendersReferencePlaceHolder;

			Factory.Save();
			AssertEquals("Correct BGM Reference Number", true, declaration.CustomsEntryHeaders[0].CH_BGMReference.Contains("/2"));
		}

		public void TestDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.CustomsEntryHeaders.AddNew().CH_BGMReference = "B00001001/1";
			AssertEquals("Details", "Reference Number: B00001001/1\r\nDeclaration Reference: B00001001\r\n", declaration.CustomsEntryHeaders[0].Details);
		}

		public void TestShortDescription()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("ShortDescription", "Declaration Reference: B00001001", declaration.CustomsEntryHeaders[0].ShortDescription);
		}

		public void TestSupplierCodeAndName()
		{
			JobDeclaration declaration = SetupImportDec();

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("Supplier Code is empty", ZString.Empty, declaration.CustomsEntryHeaders[0].JZ_SupplierCode);
			AssertEquals("Supplier Name is empty", ZString.Empty, declaration.CustomsEntryHeaders[0].JZ_SupplierName);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			invoiceHeader.JZ_OH_Supplier = header.PK;
			declaration.DoMerge();
			Factory.Save();
			Assert("Supplier Code is not empty", !declaration.CustomsEntryHeaders[0].JZ_SupplierCode.IsEmpty);
			Assert("Supplier Name is not empty", !declaration.CustomsEntryHeaders[0].JZ_SupplierName.IsEmpty);
		}

		[ExpectNoExceptions]
		public void TestNoExpectionsThrownWhenInvoiceHeaderDeleted()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			ZString supplierName = entryHeader.JZ_SupplierName;
			ZString supplierCode = entryHeader.JZ_SupplierCode;
		}

		public void TestWeGetTheOverseasFreightCurrencyInUsedCurrencyList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			header.JZ_InvoiceAmount = 10000m;

			line.JI_LinePrice = 10000m;
			line.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 10m, "USD");

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("PreCondition - OverseasFreight.Currency.Code", "USD", entryHeader.OverseasFreight.Currency.Code);
			AssertEquals(2, entryHeader.UsedCurrencies.Length);
			AssertEquals("AUD", entryHeader.UsedCurrencies[0].Code);
			AssertEquals("USD", entryHeader.UsedCurrencies[1].Code);
		}

		public void TestIAddInfo()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			//These members are implemented temporarily to get AddInfo from dbo.CusEntryHeader
			//Needs to be revisted later. Work item Created : W00036252
			invoice.AddInfo.ZA_ORG = "NZ";
			invoice.AddInfo.ZA_PRF = "X";
			invoice.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 1);

			AssertEquals("AggregatedZA_ORG", "NZ", ((IAddInfo)entryHeader).AggregatedZA_ORG);
			AssertEquals("AggregatedZA_PRF", "X", ((IAddInfo)entryHeader).AggregatedZA_PRF);
			AssertEquals("Date of valuation", new ZDateTime(2005, 1, 1), ((IAddInfo)entryHeader).DateOfValuation);
		}

		public void TestDocumentSupporter()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			AssertEquals("Document support is AU type", typeof(CusEntryHeaderDocumentSupporter), entryHeader.DocumentSupporter.GetType());
		}

		public void TestIsAllowedToPrintATD()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("No ATD Message, not allowed to print ATD", ZBool.False, entryHeader.IsAllowedToPrintATD);

			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = ATDMessageText;
			AssertEquals("Contains ATD message and security code, allowed to print", ZBool.True, entryHeader.IsAllowedToPrintATD);
		}

		[ExpectNoExceptions]
		public void TestLoadingInvoiceHeadersInDifferentCountryDoesNotFail()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = invoiceLine.PK;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			AssertEquals("Should not give exception", 1, entryHeader.InvoiceHeaders.Length);
		}

		public void TestHasRemissionOnBunkerFuelsFlag()
		{
			var testDec = SetInvoiceAndEntry();
			var entryHeader = testDec.CustomsEntryHeaders[0];
			var line = testDec.FilteredInvoiceLines[0];
			var merger = new LineMerger(testDec);
			merger.DoMerge();
			Assert("Expected HasRemissionOnBunkerFuels to be false when no line has treatment code 142.", !entryHeader.HasRemissionOnBunkerFuels);

			line.AddInfo.ZA_TreatmentCode_Hidden = "142";
			merger.DoMerge();
			Assert("Expected HasRemissionOnBunkerFuels to be true when a line has treatment code 142.", entryHeader.HasRemissionOnBunkerFuels);
		}

		public void TestLodgementQuestionKeyHasRemissionOnBunkerFuels()
		{
			var testDec = SetInvoiceAndEntry();
			var entryHeader = testDec.CustomsEntryHeaders[0];
			var line = testDec.FilteredInvoiceLines[0];

			line.AddInfo.ZA_TreatmentCode_Hidden = "141";
			var merger = new LineMerger(testDec);
			merger.DoMerge();
			var key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("Expected Key HasRemissionOnBunkerFuels to be false when no line has treatment code 142.", false, key.HasRemissionOnBunkerFuels);

			line.AddInfo.ZA_TreatmentCode_Hidden = "142";
			merger.DoMerge();
			key = ((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey;
			AssertEquals("Expected Key HasRemissionOnBunkerFuels to be true when a line has treatment code 142.", true, key.HasRemissionOnBunkerFuels);
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => (CusEntryHeader)GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			return entryHeader;
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		RefCurrency audCurrency;
		RefCurrency usdCurrency;
		RefCurrency nzdCurrency;
		RefCurrency hkdCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			audCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			nzdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			hkdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "HKD");
			SetValidExchangeRateForUSD();
			TaxOrFeeTestHelper.SetUp();
		}

		void SetValidExchangeRateForUSD()
		{
			ZQuery filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, usdCurrency.RX_Code);
			var uSDRate = Factory.LoadTop1<RefExchangeRate>(filter);
			uSDRate.RE_StartDate = new ZDateTime(2003, 11, 15);
			uSDRate.RE_ExpiryDate = new ZDateTime(2003, 11, 15);
		}

		ZGuid GetValidSupplier()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			var supplier = Factory.LoadTop1<OrgHeader>(filter);
			return supplier.PK;
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		}

		JobDeclaration SetupImportDec()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (declaration.GetValidationSuspender())
			{
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = ZDateTime.Today;
			}
			return declaration;
		}

		JobDeclaration SetInvoiceAndEntry()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoice = testDec.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CL = entryLine.PK;
			return testDec;
		}

		public const string ATDMessageText =
			"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'" +
			"TDT+20++6'LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'" +
			"NAD+IM++EAGLE DATAMATION NB'RFF+ABO:B00122382/1/1::1'RFF+ABT:AAAANNPR6::1'RFF+ABQ:SIMPLE MAIL'RFF+AIA:AAAANNPTY'RFF+AAE:N10'" +
			"DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'CNT+5:1'CNT+2:1'CNT+3:0'" +
			"UNT+30+000001'UNZ+1+00000000273283'";

		sealed class TestHelperEntryHeaderThatThrowsExceptionWhileSaving : CusEntryHeader
		{
			public TestHelperEntryHeaderThatThrowsExceptionWhileSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new Exception();
			}
		}
	}
}
