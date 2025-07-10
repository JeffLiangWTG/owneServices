using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AutoBillingTest : TestCaseWithFactory
	{
		public void TestARAutoPosting()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("98765"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "98765");

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
				var importer = testHelper.Importer;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.TransactionNumber.Validation.ValidateAll(); //Expose TransactionNumber;
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				entry1.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var entryLine = entry2.MergedLines.AddNew();
				entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.TotalDutyAmount, 200m);
				Factory.Save();
				AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());
				entry1.CH_EntryStatus = EntryStatusList.Codes.Clear;
				Factory.Save();
				AssertNull("No invoicing job should have been created as release is clear but accounting not lodged yet", new JobHeader.Loader(declaration).Load());

				entry2.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				Factory.Save();

				var job = new JobHeader.Loader(declaration).Load();
				AssertNotNull("Account job created", job);
				JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 200m, charges[0].JR_LocalCostAmt);
				Assert("cost should have been posted", charges[0].IsCostPosted);
				Assert("revenue should have been posted", charges[0].IsRevenuePosted);
				AssertEquals("No email should have been sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		public void TestARAutoPostingNotDoneForLVSTotalConsolidation()
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DisbursementCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DisbursementChargeCode.PK.ToGuid());

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

			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.TransactionNumber.Validation.ValidateAll(); //Expose TransactionNumber;
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				entry1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var entryLine = entry1.MergedLines.AddNew();
				entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.TotalDutyAmount, 200m);
				Factory.Save();
				AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());
				entry1.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				Factory.Save();
				AssertNull("Still No invoicing job should have been created as is LVS", new JobHeader.Loader(declaration).Load());
				AssertEquals("No email should have been sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		public void TestARAutoPostingDoneForLVSByImporter()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("98765"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "98765");

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
				var importer = testHelper.Importer;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
				declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.TransactionNumber.Validation.ValidateAll(); //Expose TransactionNumber;
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				entry1.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var entryLine = entry2.MergedLines.AddNew();
				entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.TotalDutyAmount, 200m);
				Factory.Save();
				AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());
				entry1.CH_EntryStatus = EntryStatusList.Codes.Clear;
				Factory.Save();
				AssertNull("No invoicing job should have been created as release is clear but accounting not lodged yet", new JobHeader.Loader(declaration).Load());

				entry2.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				Factory.Save();

				var job = new JobHeader.Loader(declaration).Load();
				AssertNotNull("Account job created", job);
				JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 200m, charges[0].JR_LocalCostAmt);
				Assert("cost should have been posted", charges[0].IsCostPosted);
				Assert("revenue should have been posted", charges[0].IsRevenuePosted);
				AssertEquals("No email should have been sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		public void TestARAutoPostingNotDoneForExport()
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DisbursementCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DisbursementChargeCode.PK.ToGuid());

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
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importer.CompanyData.OB_IsDebtor = true;
			importer.CompanyData.SetARTaxApplicable(false);
			importer.CompanyData.SetAPTaxApplicable(false);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = MessageTypeList.Codes.G7Export;
			var entryLine = entry1.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.TotalDutyAmount, 200m);
			Factory.Save();
			AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());
			entry1.CH_EntryStatus = EntryStatusList.Codes.Clear;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Factory.Save();
			AssertNull("Still No invoicing job should have been created as is Export", new JobHeader.Loader(declaration).Load());
			AssertEquals("No email should have been sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestAlignLineWrappersAndContent()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.G7Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var cusEntryHeader = dec.CustomsEntryHeaders.AddNew();
			var mergedLine = cusEntryHeader.MergedLines.AddNew();

			for (var idx = 0; idx < 26; idx++)
			{
				var container = dec.CusContainers.AddNew();
				container.CO_ContainerNumber = "APLU876543" + idx.ToString("D2");
			}

			AssertEquals(9, cusEntryHeader.MergedLinesForB13A.Count);
			Factory.Save();
			var newEntryHeader = new BusinessObjectFactory().LoadTop1<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.PK, cusEntryHeader.PK));
			AssertEquals(1, newEntryHeader.MergedLines.Count);

			for (var idx = 20; idx < 53; idx++)
			{
				var container = dec.CusContainers.AddNew();
				container.CO_ContainerNumber = "APLU876543" + idx.ToString("D2");
			}

			AssertEquals(41, cusEntryHeader.MergedLinesForB13A.Count);
			Factory.Save();
			newEntryHeader = new BusinessObjectFactory().LoadTop1<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.PK, cusEntryHeader.PK));
			AssertEquals(1, newEntryHeader.MergedLines.Count);

			for (var idx = 0; idx < 80; idx++)
			{
				var invoice = dec.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV12345678" + idx.ToString("D2");
			}

			AssertEquals(73, cusEntryHeader.MergedLinesForB13A.Count);
			Factory.Save();
			newEntryHeader = new BusinessObjectFactory().LoadTop1<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.PK, cusEntryHeader.PK));
			AssertEquals(1, newEntryHeader.MergedLines.Count);

			dec.Invoices.RemoveAll();
			dec.CusContainers.RemoveAll();
			AssertEquals(1, cusEntryHeader.MergedLinesForB13A.Count);
		}

		AccChargeCode DisbursementChargeCode
		{
			get
			{
				if (fDisbursementChargeCode == null)
				{
					fDisbursementChargeCode = CreateChargeCode("~c~", "Customs Disbursements", Core.Constants.ChargeType.Disbursement, 0m, null, null);
				}
				return fDisbursementChargeCode;
			}
		}
		AccChargeCode fDisbursementChargeCode;

		AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "ZZ" + code;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			if (gST != null)
			{
				chargeCode.AC_AT_GSTRate = gST.PK;
			}

			if (wHT != null)
			{
				chargeCode.AC_AW_WithholdingTaxRate = wHT.PK;
			}

			return chargeCode;
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

		OrgHeader CreateOrgHeader(string code, bool creditor, bool debtor)
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Brampton";
			header.MainAddress.OA_State = "ON";
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
	}
}
