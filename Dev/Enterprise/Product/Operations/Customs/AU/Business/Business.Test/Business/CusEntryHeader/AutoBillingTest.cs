using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AutoBillingTest : TestCaseWithFactory
	{
		public void TestARAutoPostingForTwoEntries()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 10m);
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 20m);
			Factory.Save();
			AssertNull("No invoicing job should have been created as nothing cleared", new JobHeader.Loader(declaration).Load());

			entry1.EntryNumber = "~1";
			Factory.Save();

			var job = new JobHeader.Loader(declaration).Load();
			AssertNull("No paid or ATD received yet", job);

			entry1.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			Factory.Save();

			job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("Paid now", job);

			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge", 1, charges.Length);
			AssertEquals("Charge amount", 10m, charges[0].JR_LocalCostAmt);
			AssertEquals("cost should have been posted", true, charges[0].IsCostPosted);
			AssertEquals("revenue should NOT have been posted yet as there is another formal entry that is not cleared", false, charges[0].IsRevenuePosted);

			entry2.EntryNumber = "~2";
			entry2.Factory.Save();
			charges = entry2.Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge as the second entry has not been cleared as far as Acc integration is concerned", 1, charges.Length);
			AssertEquals("Charge amount", 10m, charges[0].JR_LocalCostAmt);
			AssertEquals("cost should have been posted", true, charges[0].IsCostPosted);
			AssertEquals("revenue should NOT have been posted yet as there is another formal entry that is not cleared", false, charges[0].IsRevenuePosted);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			entry2.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			entry2.Factory.Save();

			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("two charges", 2, charges.Length);
			AssertEquals("revenue should have been posted", true, charges[0].IsRevenuePosted);
			AssertEquals("revenue should have been posted", true, charges[1].IsRevenuePosted);

			AssertEquals("cost should have been posted", true, charges[0].IsCostPosted);
			AssertEquals("cost should have been posted", true, charges[1].IsCostPosted);

			AssertEquals("revenue should have been posted", 10m, charges[0].ARLine.AL_LineAmount);
			AssertEquals("revenue should have been posted", 20m, charges[1].ARLine.AL_LineAmount);

			AssertEquals("No email should have been sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestARAutoPostingForTwoEntriesWhenPAYRECProcessedInOneFactory()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			declaration.JE_TransportMode = "SEA";
			var importer = testHelper.Importer;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 10m);
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 20m);
			Factory.Save();
			AssertNull("No invoicing job should have been created as nothing cleared", new JobHeader.Loader(declaration).Load());

			entry1.EntryNumber = "~1";
			entry2.EntryNumber = "~2";
			Factory.Save();

			var job = new JobHeader.Loader(declaration).Load();
			AssertNull("No paid or ATD received yet", job);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			entry1.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			entry2.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			Factory.Save();

			job = new JobHeader.Loader(declaration).Load();
			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("two charges", 2, charges.Length);
			AssertEquals("Charge amount", 10m, charges[0].JR_LocalCostAmt);
			AssertEquals("cost should have been posted", true, charges[0].IsCostPosted);
			AssertEquals("revenue should have been posted", true, charges[0].IsRevenuePosted);

			AssertEquals("Charge amount", 20m, charges[1].JR_LocalCostAmt);
			AssertEquals("cost should have been posted", true, charges[1].IsCostPosted);
			AssertEquals("revenue should have been posted when the second entry is processed", true, charges[1].IsRevenuePosted);
		}

		public void TestIntegrateForEXW_TransportModeNotRelevant()
		{
			var newFactory = new BusinessObjectFactory();
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(newFactory);
			var disbursementChargeCode = testHelper.DisbursementChargeCode;
			disbursementChargeCode.AC_AT_GSTRate = ZGuid.Empty;
			disbursementChargeCode.Factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DisbursementCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, disbursementChargeCode.PK.ToGuid());

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			declaration.JE_TransportMode = ZString.Empty;//Not relevant for ExWarehouse
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importer.CompanyData.OB_IsDebtor = true;
			importer.CompanyData.SetARTaxApplicable(false);
			importer.CompanyData.SetAPTaxApplicable(false);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 10m);

			Factory.Save();
			AssertNull("No invoicing job should have been created as nothing cleared", new JobHeader.Loader(declaration).Load());

			entry.EntryNumber = "~1";
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			Factory.Save();

			var job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("No paid or ATD received yet", job);
		}

		public void TestAutoRateAndPostAP()
		{
			var entry = SetUpForAutoRate();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CMRPAYRECMessage responseMessage = factory2.New<CMRPAYRECMessage>();
			responseMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+1D21 CG9E J350:1+11'DTM+138:20100202:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00153055/1/CMT1::1'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ABT:AAACKAPGJ'RFF+RA:AAACKAPJ7'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000050.00'TAX+3'MOA+9:0000000000500.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000600.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000020.00'TAX+3'MOA+206:0000000000000.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000001170.00'UNT+37+000001'";

			var processor = new PAYRECMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(responseMessage);

			factory2.Save();

			JobHeader retrievedJob = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, entry.Declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, entry.Declaration.CompanyPK));
			AssertNotNull("A Job should be referencing Declaration", retrievedJob);
			AssertEquals("Table code", JobDeclarationSchema.Constants.Prefix, retrievedJob.JH_ParentTableCode);

			JobCharge charge1 = factory2.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, retrievedJob.PK));
			AssertNotNull(charge1);
			AssertEquals("AP posted", true, charge1.IsCostPosted);
			AssertEquals("AR is not posted", false, charge1.IsRevenuePosted);
			AssertEquals("Amount", entry.CH_TotalPaid, charge1.JR_OSSellAmt);
		}

		public void TestAutoRateAndPostAPForASP()
		{
			var dsbChargeCode = CreateOrGetChargeCode("~c2~", "Customs Disbursements", Enterprise.Core.Constants.ChargeType.Disbursement);

			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			var entry = SetUpForAutoRate();

			var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var chargeCode2 = collection.AddNew();
			chargeCode2.AC_ChargeCode = dsbChargeCode.PK;
			chargeCode2.ChargeType = Registry.Business.Customs.AU.EntryChargeTypeList.Codes.GSTAmount;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeCodeWithDate() { ChargeCode = testHelper.DisbursementChargeCode.PK.ToGuid(), ActiveTimeUtc = ZDateTime.Today.AddDays(-1) });
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var newEntry = factory2.Load<CusEntryHeader>(entry.PK);
			var responseMessageForASP = factory2.New<CMRPAYRECMessage>();
			responseMessageForASP.EM_MessageNum = "000001";
			responseMessageForASP.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+1D21 CG9E J350:1+11'DTM+138:20100202:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00153055/1/CMT1::1'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ABT:AAACKAPGJ'RFF+RA:AAACKAPJ7'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000050.00'TAX+3'MOA+9:0000000000500.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000600.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000020.00'TAX+3'MOA+206:0000000000030.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000030.00'UNT+37+000001'";
			var processor2 = new PAYRECMessageProcessor(new BatchProcessor.LoggingInformation());
			processor2.ProcessMessage(responseMessageForASP);
			factory2.Save();

			AssertEquals(CMREntryPaymentStatusList.Codes.PayAckPending, newEntry.AddInfo.ZA_PaymentStatus_Hidden);

			var retrievedJob = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, newEntry.Declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, newEntry.Declaration.CompanyPK));
			var charges = factory2.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, retrievedJob.PK));
			AssertEquals(1, charges.Length);
			var charge1 = charges.FirstOrDefault();
			AssertEquals("AP posted", true, charge1.IsCostPosted);
			AssertEquals("AR is not posted", false, charge1.IsRevenuePosted);
			AssertEquals(testHelper.DisbursementChargeCode.PK, charge1.ChargeCode.PK);
			AssertEquals(30m, charge1.JR_OSSellAmt);

			var responseMessageForASPClear = factory2.New<CMRPAYRECMessage>();
			responseMessageForASPClear.EM_MessageNum = "000002";
			responseMessageForASPClear.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+1D21 CG9E J350:1+11'DTM+138:20100202:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00153055/1/CMT1::1'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ABT:AAACKAPGJ'RFF+RA:AAACKAPJ7'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000050.00'TAX+3'MOA+9:0000000000500.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000600.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000020.00'TAX+3'MOA+206:0000000000030.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000031.00'UNT+37+000001'";
			processor2.ProcessMessage(responseMessageForASPClear);
			factory2.Save();

			AssertEquals(CMREntryPaymentStatusList.Codes.Paid, newEntry.AddInfo.ZA_PaymentStatus_Hidden);

			charges = factory2.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, retrievedJob.PK));
			AssertEquals(4, charges.Length);
			var charge2 = charges.Where(x => x.ChargeCode.PK == testHelper.DisbursementChargeCode.PK).Except(charge1).FirstOrDefault();
			AssertEquals("AP posted", true, charge2.IsCostPosted);
			AssertEquals("AR is not posted", false, charge2.IsRevenuePosted);
			AssertEquals(500m, charge2.JR_OSSellAmt);
			var restOfCharges = charges.Except(new JobCharge[] { charge1, charge2 });
			AssertEquals(580m, restOfCharges.Sum(x => x.JR_OSSellAmt));
			AssertEquals(580m, restOfCharges.Sum(x => x.JR_OSCostAmt));
		}

		/// <summary>
		/// In conjunction with the above test but this one is with bad data. The Customs Quarantine Charge Code is from another company.
		/// </summary>
		public void TestAutoRateAndPostAPForASP_BadData_CustomsQuarantineChargeCodeFromAnotherCompany()
		{
			var dsbChargeCode = CreateOrGetChargeCode("~c2~", "Customs Disbursements", Enterprise.Core.Constants.ChargeType.Disbursement);

			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			var entry = SetUpForAutoRate();

			var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var chargeCode2 = collection.AddNew();
			chargeCode2.AC_ChargeCode = dsbChargeCode.PK;
			chargeCode2.ChargeType = Registry.Business.Customs.AU.EntryChargeTypeList.Codes.GSTAmount;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeCodeWithDate() { ChargeCode = testHelper.DisbursementChargeCode.PK.ToGuid(), ActiveTimeUtc = ZDateTime.Today.AddDays(-1) });
			var companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(companyQuery);
			// Make it wrong here with a different company
			testHelper.DisbursementChargeCode.AC_GC = anotherCompany.PK;

			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var newEntry = factory2.Load<CusEntryHeader>(entry.PK);
			var responseMessageForASP = factory2.New<CMRPAYRECMessage>();
			responseMessageForASP.EM_MessageNum = "000001";
			responseMessageForASP.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+1D21 CG9E J350:1+11'DTM+138:20100202:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00153055/1/CMT1::1'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ABT:AAACKAPGJ'RFF+RA:AAACKAPJ7'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000050.00'TAX+3'MOA+9:0000000000500.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000600.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000020.00'TAX+3'MOA+206:0000000000030.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000030.00'UNT+37+000001'";
			var logger = new BatchProcessor.LoggingInformation();
			var processor2 = new PAYRECMessageProcessor(logger);
			processor2.ProcessMessage(responseMessageForASP);
			factory2.Save();

			var retrievedJob = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, newEntry.Declaration.PK));
			// This case may need an enhancement WI as the charge is removed silently and no one would understand what happened.
			// There is no interactor to log the problem down.
			// Before this change, there was an ErrorReport when validating the invalid charge code.
			// It was only for developer and still hard for identifying the issue anyway.
			AssertNull("Job should not be created", retrievedJob);
		}

		AccChargeCode CreateOrGetChargeCode(string code, string description, string chargeType)
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ZZ" + code));
			if (chargeCode == null)
			{
				chargeCode = Factory.New<AccChargeCode>();
				chargeCode.AC_Code = "ZZ" + code;
			}

			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = 0m;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.SetGLAccountDataForTesting();

			var zeroTaxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			zeroTaxRate.SetRateNumerator_ForTestOnly(0);

			chargeCode.AC_AT_GSTRate = zeroTaxRate.PK;

			return chargeCode;
		}

		public void TestAutoRateAndPostAPForASP_DeclarationCreatedBeforeSettingASPRegistry()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			var entry = SetUpForAutoRate();

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeCodeWithDate() { ChargeCode = testHelper.DisbursementChargeCode.PK.ToGuid() });
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var newEntry = factory2.Load<CusEntryHeader>(entry.PK);
			var responseMessageForASP = factory2.New<CMRPAYRECMessage>();
			responseMessageForASP.EM_MessageNum = "000001";
			responseMessageForASP.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+1D21 CG9E J350:1+11'DTM+138:20100202:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00153055/1/CMT1::1'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ABT:AAACKAPGJ'RFF+RA:AAACKAPJ7'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000050.00'TAX+3'MOA+9:0000000000500.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000600.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000020.00'TAX+3'MOA+206:0000000000030.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000030.00'UNT+37+000001'";
			var processor2 = new PAYRECMessageProcessor(new BatchProcessor.LoggingInformation());
			processor2.ProcessMessage(responseMessageForASP);
			factory2.Save();

			AssertEquals(CMREntryPaymentStatusList.Codes.PayAckPending, newEntry.AddInfo.ZA_PaymentStatus_Hidden);

			var retrievedJob = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, newEntry.Declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, newEntry.Declaration.CompanyPK));
			AssertNull(retrievedJob);
		}

		public void TestAutoRateAndPostAPForASP_AutoRatingAfterDisableASPRegistry()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			var entry = SetUpForAutoRate();

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeCodeWithDate() { ChargeCode = testHelper.DisbursementChargeCode.PK.ToGuid(), ActiveTimeUtc = ZDateTime.Today.AddDays(-1) });
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var newEntry = factory2.Load<CusEntryHeader>(entry.PK);
			var responseMessageForASP = factory2.New<CMRPAYRECMessage>();
			responseMessageForASP.EM_MessageNum = "000001";
			responseMessageForASP.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+1D21 CG9E J350:1+11'DTM+138:20100202:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00153055/1/CMT1::1'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ABT:AAACKAPGJ'RFF+RA:AAACKAPJ7'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000050.00'TAX+3'MOA+9:0000000000500.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000600.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000020.00'TAX+3'MOA+206:0000000000030.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000030.00'UNT+37+000001'";
			var processor = new PAYRECMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(responseMessageForASP);
			factory2.Save();

			AssertEquals(CMREntryPaymentStatusList.Codes.PayAckPending, newEntry.AddInfo.ZA_PaymentStatus_Hidden);

			var retrievedJob = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, newEntry.Declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, newEntry.Declaration.CompanyPK));
			var charges = factory2.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, retrievedJob.PK));
			AssertEquals(1, charges.Length);
			var charge1 = charges.FirstOrDefault();
			AssertEquals("AP posted", true, charge1.IsCostPosted);
			AssertEquals("AR is not posted", false, charge1.IsRevenuePosted);
			AssertEquals(testHelper.DisbursementChargeCode.PK, charge1.ChargeCode.PK);
			AssertEquals(30m, charge1.JR_OSSellAmt);

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, (ChargeCodeWithDate)RatingDataRegistry.Instance.CustomsQuarantineChargeCode.DataType.DefaultValue);
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var responseMessageForASPClear = factory3.New<CMRPAYRECMessage>();
			responseMessageForASPClear.EM_MessageNum = "000002";
			responseMessageForASPClear.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+1D21 CG9E J350:1+11'DTM+138:20100202:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00153055/1/CMT1::1'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ABT:AAACKAPGJ'RFF+RA:AAACKAPJ7'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000050.00'TAX+3'MOA+9:0000000000500.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000600.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000020.00'TAX+3'MOA+206:0000000000050.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000051.00'UNT+37+000001'";
			var processor2 = new PAYRECMessageProcessor(new BatchProcessor.LoggingInformation());
			processor2.ProcessMessage(responseMessageForASPClear);
			factory3.Save();

			charges = factory3.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, retrievedJob.PK));
			AssertEquals(2, charges.Length);
			var charge2 = charges.Where(x => x.ChargeCode.PK == testHelper.DisbursementChargeCode.PK && x.PK != charge1.PK).FirstOrDefault();
			AssertEquals(1100m, charge2.JR_OSSellAmt);
		}

		public void TestIsEntryHeld()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals(false, entryHeader.IsEntryHeld);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals(false, entryHeader.IsEntryHeld);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals(true, entryHeader.IsEntryHeld);
		}

		public void TestAgencyBranchID()
		{
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AF36GY";
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B00122382";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";
			AssertEquals("AgencyBranchIdentifier comes from current branch id", "AF36GY", entryHeader.AgencyBranchIdentifier);

			var orgMessage = Factory.New<CMRIMDMessage>();
			orgMessage.EM_MessageText = CMRImportDeclarationTestData.IMD; //IMD+B00122382/1/NAD+VT+AA33HF
			orgMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			orgMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			orgMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-2);
			entryHeader.Messages.Add(orgMessage);
			entryHeader.EntryNumber = "AAAA7GW6R";
			AssertEquals("AgencyBranchIdentifier comes from original message", "AA33HF", entryHeader.AgencyBranchIdentifier);

			var chgMessage = Factory.New<CMRIMDMessage>();
			chgMessage.EM_MessageText = CMRImportDeclarationTestData.IMD.Replace("NAD+VT+AA33HF", "NAD+VT+AF46WC"); //IMD+B00122382/1/NAD+VT+AF46WC
			chgMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			chgMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			chgMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-1);
			entryHeader.Messages.Add(chgMessage);
			AssertEquals("AgencyBranchIdentifier comes from original message", "AA33HF", entryHeader.AgencyBranchIdentifier);

			var orgMessage2 = Factory.New<CMRIMDMessage>();
			orgMessage2.EM_MessageText = CMRImportDeclarationTestData.IMD.Replace("NAD+VT+AA33HF", "NAD+VT+AF46WC"); //IMD+B00122382/1/NAD+VT+AF46WC
			orgMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			orgMessage2.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			orgMessage2.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			entryHeader.Messages.Add(orgMessage2);
			entryHeader.EntryNumber = "AAAA7GW6R";
			AssertEquals("AgencyBranchIdentifier comes from last original message", "AF46WC", entryHeader.AgencyBranchIdentifier);
		}

		OrgHeader CreateOrgHeader(string code, bool creditor, bool debtor)
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			header.OH_Code = "Z" + code;
			header.OH_IsDebtor = debtor;
			header.OH_IsCreditor = creditor;

			if (debtor)
			{
				header.CompanyData.SetARTaxApplicable(true);
				header.MiscServ.OM_ARWHTApplicable = true;
			}

			if (creditor)
			{
				header.CompanyData.SetAPTaxApplicable(true);
				header.MiscServ.OM_APWHTApplicable = true;
			}

			Factory.Save();
			return header;
		}

		CusEntryHeader SetUpForAutoRate()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TEST1234560";
			container.CO_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "6405.10.00 58";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_CountryOfOrigin = "NZ";
			invoiceLine.AddInfo.ZA_PST = "GEN";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_LinkedObject = entry;
			message.EM_MessageText = "UNH+" + EDIMessage.MessageNumberPlaceHolder + "+CUSDEC:D:99B:UN'BGM+929:::IMD+B00153055/1/CMT1:1+9'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+GBSOU::6'LOC+79+AUSYD::6'DTM+178:20100202:102'DTM+260:20100202:102'DTM+252:20100202:102'GIS+EPA:109:95'GIS+Y:153:95'GIS+POR:109:95'GIS+LLB:109:95'GIS+TLB:109:95'FII+COQ+323232+:::242200::215'MEA+AAE+G+KG:32.00000'FTX+DEL+++ABC IMPORTS PTY LTD'RFF+ABQ:TEST'RFF+ADU:B00153055/1'RFF+ANU:B'RFF+APH:FOB'RFF+ADP:0008::Y'RFF+AMG:0001'RFF+ADP:0009::Y'TDT+20+028S+S+++++9252242::11'NAD+AT+12090995252::95'NAD+VT+AA33HF::95'NAD+DP++ALEXANDRIA++156 MAIN RD++:::NSW+2015+AU'NAD+CB+54321::95'MOA+63:5000.00:AUD'MOA+141:5500.00:AUD'MOA+39:5000.00:AUD'MOA+313:500.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+++LCL:67:95'PAC+1+1'PCI+1'RFF+AAQ:TEST1234560'PCI+1'RFF+MB:UIOEWRIOU'CST+1+I::95+N10::95'FTX+AAA+++SUEDE SHOES'LOC+27+NZ::5'MEA+AAA++PR:1500.00000'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00020:00035:N'PCI+1'FTX+RAH+++00025:00040:N'PCI+1'FTX+RAH+++00314:00313:N'MOA+38:5000.00:AUD'RFF+ABD:64051000'RFF+AED:58'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:TEST1234560'RFF+AFV:TV'UNS+S'UNT+64+1'";
			message.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			entry.CH_BGMReference = "B00153055/1";
			Factory.Save();
			Assert("PreCondition", entry.CH_TotalPaid > 0m);
			return entry;
		}

		OrgHeader DisbursementCreditor
		{
			get
			{
				if (fDisbursementCreditor == null)
				{
					fDisbursementCreditor = CreateOrgHeader("~o~", true, false);
				}
				return fDisbursementCreditor;
			}
		}
		OrgHeader fDisbursementCreditor;
	}
}
