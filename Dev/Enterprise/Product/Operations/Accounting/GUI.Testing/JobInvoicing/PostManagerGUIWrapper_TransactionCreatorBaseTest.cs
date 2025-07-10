using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public abstract class PostManagerGUIWrapperTest : TransactionCreatorBaseTest
	{
		public virtual void TestAllowDeliveryDuringPreviewInvoice()
		{
			DummyShipment shipment;
			Charge charge;
			var testJob = CreateDummyShipmentJob(out shipment, out charge);
			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob);
			Assert(wrapper.AllowDeliveryDuringPreviewInvoice_ForTestOnly);

			wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, testJob);
			Assert(!wrapper.AllowDeliveryDuringPreviewInvoice_ForTestOnly);
		}

		public void TestHandleNegativeCompliancesFailedToCreate()
		{
			AssertHandleNegativeCompliancesFailedToCreate("S0001", true);
			AssertHandleNegativeCompliancesFailedToCreate("S0002", false);
		}

		void AssertHandleNegativeCompliancesFailedToCreate(string shipmentNum, bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				TestObjectCreator.AALSHI.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.ABIGAS.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";
				var twCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TWD"));

				var shipment = TestObjectCreator.CreateShipment(shipmentNum);
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 0m, null, 0m);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", twCurrency, 150m, TestObjectCreator.AALSHI, twCurrency, 100m, TestObjectCreator.ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				charge.JR_LocalSellAmt = 100m;
				charge.JR_OSSellAmt = 100m;
				TestObjectCreator.GST1.AT_PostingGroupId = 1;
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "desc", twCurrency, 70m, TestObjectCreator.AALSHI, twCurrency, -50m, TestObjectCreator.ABIGAS);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge1.JR_AT_SellGSTRate = TestObjectCreator.GST2.PK;
				charge.JR_LocalSellAmt = -50m;
				charge.JR_OSSellAmt = -50m;
				TestObjectCreator.GST2.AT_PostingGroupId = 2;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job);
				using (Form form = new Form())
				{
					wrapper.ParentForm = form;
					wrapper.Post();

					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));

					var invoicePosted = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
					AssertNotNull("Should be one invoice created", invoicePosted);

					var complianceCreated = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
					AssertEquals("No compliance should be created", 0, complianceCreated.Length);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestComplianceDocumentCreatedSucceedWithoutAnyNegativeWarnings()
		{
			AssertComplianceDocumentCreatedSucceedWithoutAnyNegativeWarnings("S0001", true);
			AssertComplianceDocumentCreatedSucceedWithoutAnyNegativeWarnings("S0002", false);
		}

		void AssertComplianceDocumentCreatedSucceedWithoutAnyNegativeWarnings(string shipmentNum, bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				TestObjectCreator.AALSHI.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.ABIGAS.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";
				var twCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TWD"));

				var shipment = TestObjectCreator.CreateShipment(shipmentNum);
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 0m, null, 0m);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", twCurrency, 100m, TestObjectCreator.AALSHI, twCurrency, 100m, TestObjectCreator.ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "desc", twCurrency, 100m, TestObjectCreator.AALSHI, twCurrency, 50m, TestObjectCreator.ABIGAS);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge1.JR_AT_SellGSTRate = TestObjectCreator.GST2.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job);
				using (Form form = new Form())
				{
					wrapper.ParentForm = form;
					wrapper.Post();

					AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));

					var invoicePosted = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
					AssertNotNull("Should be one invoice created", invoicePosted);

					var complianceCreated = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
					AssertEquals("Should be one compliance created", 1, complianceCreated.Length);
					complianceCreated.First().Delete();
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestRaiseComplianceSequenceFailedToAssign_Inv()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff currentuser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentuser.GS_EmailAddress = "david.park@test.com";
			Factory.Save();
			group.Staff.Add(currentuser);
			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());

			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			AddComplianceSubTypeConfig(collection, CountryCodes.Mexico, "TXI", "AR");

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Mexico);
			TestObjectCreator.AALSHI.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			var mxCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "MXN"));
			var taxRate = TestObjectCreator.CreateTaxRate("MXT", "Mexico Test Tax Rate", 10);

			var shipment = TestObjectCreator.CreateShipment("S00001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 0m, null, 0m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", mxCurrency, 100m, TestObjectCreator.AALSHI, mxCurrency, 100m, TestObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_SellGovtChargeCode = "AAA";
			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job);
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				wrapper.Post();

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist."));
				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var invoicePosted = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
				AssertNotNull("Should be one invoice created", invoicePosted);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AccComplianceSequence sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 2, 100, 25);
			sequence.XD_Prefix = "01.02-";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;

			var shipment2 = TestObjectCreator.CreateShipment("S00002");
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.AALSHI, 0m, null, 0m);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "desc", mxCurrency, 100m, TestObjectCreator.AALSHI, mxCurrency, 100m, TestObjectCreator.ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_AT_SellGSTRate = taxRate.PK;
			charge2.JR_SellGovtChargeCode = "AAA";
			Factory.Save();

			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition", job2);

			wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job2);
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				wrapper.Post();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please configure appropriate Compliance Invoice Books for your Login Company, Branch or Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"));
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please configure appropriate Compliance Invoice Books for your Login Company, Transaction Header Branch or Transaction Header Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"));
				AssertEquals("Email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				var invoicePosted = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("Should be one invoice created", invoicePosted);
			}
		}

		public void TestRaiseComplianceSequenceFailedToAssign_Crd()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff currentuser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentuser.GS_EmailAddress = "david.park@test.com";
			Factory.Save();
			group.Staff.Add(currentuser);
			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());

			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			AddComplianceSubTypeConfig(collection, CountryCodes.Mexico, "TCR", "AR", "CRD");

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Mexico);
			TestObjectCreator.AALSHI.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			var mxCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "MXN"));
			var taxRate = TestObjectCreator.CreateTaxRate("MXT", "Mexico Test Tax Rate", 10);

			var shipment = TestObjectCreator.CreateShipment("S00001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 0m, null, 0m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", mxCurrency, 0m, TestObjectCreator.AALSHI, mxCurrency, -100m, TestObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_SellGovtChargeCode = "AAA";
			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job);
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				wrapper.Post();

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist."));
				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var crdPosted = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("Should be one credit note created", crdPosted);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AccComplianceSequence sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TCR", 2, 100, 25);
			sequence.XD_Prefix = "01.02-";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;

			var shipment2 = TestObjectCreator.CreateShipment("S00002");
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.AALSHI, 0m, null, 0m);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "desc", mxCurrency, 100m, TestObjectCreator.AALSHI, mxCurrency, 100m, TestObjectCreator.ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_AT_SellGSTRate = taxRate.PK;
			charge2.JR_SellGovtChargeCode = "AAA";
			Factory.Save();

			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition", job2);

			wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job2);
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				wrapper.Post();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please configure appropriate Compliance Invoice Books for your Login Company, Branch or Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"));
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please configure appropriate Compliance Invoice Books for your Login Company, Transaction Header Branch or Transaction Header Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"));
				AssertEquals("Email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				var crdPosted = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("Should be one credit note created", crdPosted);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestImportUnsavedCharges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYenWithCharge);
				AccountingConfigurationRegistry.Instance.RoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.Registry.FreightChargeCode);
				DummyShipment shipment;
				Charge charge;
				Job testJob = CreateDummyShipmentJob(out shipment, out charge);
				charge.JR_RX_NKSellCurrency = "USD";
				charge.JR_OSSellExRate = 1;
				charge.JR_LocalSellAmt = 13M;
				charge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "USNLG";
				shipment.JS_RL_NKDestination = "JPAMX";
				testJob.ExchangeRates[0].JF_BaseRate = 1;
				Factory.Save();

				var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob);
				using (Form form = new Form())
				{
					wrapper.ParentForm = form;
					wrapper.Post();

					AssertEquals(2, wrapper.OriginalJobs_ForTestOnly.FirstOrDefault().Charges.Count);
					Assert(wrapper.OriginalJobs_ForTestOnly.FirstOrDefault().HasChanges);
				}
			}
		}

		public void TestImportUnsavedCharges_WithAdditionalChargesInMemory()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AccountingConfigurationRegistry.Instance.RoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC1.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.GSTFREE1.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYenWithCharge);

				TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				var testJob = CreateDummyShipmentJob(out DummyShipment shipment, out Charge charge);
				Factory.Save();
				testJob.JH_GE = TestObjectCreator.FIADepartment.PK;
				charge.JR_RX_NKSellCurrency = "USD";
				charge.JR_OSSellExRate = 1;
				charge.JR_LocalSellAmt = 13M;
				charge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "USNLG";
				shipment.JS_RL_NKDestination = "JPAMX";
				testJob.ExchangeRates[0].JF_BaseRate = 1;
				var charge2 = testJob.Charges.AddNew();
				charge2.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(charge);
				charge2.JR_AC = TestObjectCreator.CC2.PK;
				charge2.JR_RX_NKSellCurrency = "USD";
				charge2.JR_LocalSellAmt = 13M;
				charge2.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				Factory.Save();

				var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
				taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypesList.Codes.AccountsPayable);

				var taxProcessorMock = new Mock<ITaxProcessor>();
				ObjectFactory.Substitute(taxProcessorMock.Object);
				var newNotRoundingChargePK = ZGuid.Empty;
				taxProcessorMock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("Error").Callback((ITaxRecordParent parent) =>
				{
					var jobInParentFactory = parent.Factory.Load<Job>(testJob.PK);
					var newNotRoundingCharge = jobInParentFactory.Charges.AddNew();
					newNotRoundingCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(charge);
					newNotRoundingChargePK = newNotRoundingCharge.PK;
				});

				var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob);
				using (Form form = new Form())
				{
					wrapper.ParentForm = form;
					AssertEquals("Precondition: testJob.Charges.Count", 2, testJob.Charges.Count);
					wrapper.Post();

					Assert("Postcondition: isProcessTaxesOnPostingCalled", !newNotRoundingChargePK.IsEmpty);
					var newCharge = testJob.Charges.FirstOrDefault(x => !x.IsInDatabase);
					AssertNotNull(nameof(newCharge), newCharge);
					AssertNotEquals("newCharge is not newNotRoundingCharge", newNotRoundingChargePK, newCharge.PK);
					AssertContainsExactElementsInAnyOrder(new[] { charge, charge2, newCharge }, testJob.Charges);
				}
			}
		}

		public void TestRefreshChargesWhenPosted_WithContext()
		{
			AssertRefreshChargesWhenPosted(true);
		}

		public void TestRefreshChargesWhenPosted_WithoutContext()
		{
			AssertRefreshChargesWhenPosted(false);
		}

		void AssertRefreshChargesWhenPosted(bool addContext)
		{
			var testJob = CreateDummyShipmentJob(out DummyShipment shipment, out Charge charge);
			Factory.Save();

			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypesList.Codes.AccountsPayable);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			var newNotRoundingChargePK = ZGuid.Empty;
			taxProcessorMock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("").Callback((ITaxRecordParent parent) =>
			{
				if (addContext)
				{
					parent.Factory.SetContext(NewChargeLoadActionOnPosting.RefreshChargesWhenPosted);
				}

				var newCharge = parent.Factory.New<Charge>();
				newCharge.JR_JH = testJob.PK;
				newCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(charge);
				newNotRoundingChargePK = newCharge.PK;
			});

			var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob);
			using (Form form = new Form())
			{
				wrapper.TransactionFactory_ForTestOnly.RefreshEnabled = false; //To be sure that charge is not loaded by data refresh bus instead of Job.RefreshCharges
				wrapper.ParentForm = form;
				AssertEquals("Precondition: testJob.Charges.Count", 1, testJob.Charges.Count);
				wrapper.Post();

				Assert("Postcondition: isProcessTaxesOnPostingCalled", !newNotRoundingChargePK.IsEmpty);
				if (addContext)
				{
					AssertContainsExactElementsInAnyOrder(new[] { charge.PK, newNotRoundingChargePK }, testJob.Charges.GetPKs());
				}
				else
				{
					AssertContainsExactElementsInAnyOrder(new[] { charge.PK }, testJob.Charges.GetPKs());
				}
			}
		}

		public void TestPreviewCharges()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			DummyShipment shipment;
			Charge charge;
			var testJob = CreateDummyShipmentJob(out shipment, out charge);
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			AssertNoErrors(charge);
			using (var form = new Form())
			{
				var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, testJob);
				wrapper.ParentForm = form;
				wrapper.Preview();

				using (var previewForm = ZFormModaliser.ActiveForm as InvoicePreviewForm)
				{
					AssertNotNull("Preview form shown", previewForm);
					AssertEquals(1, ((InvoicesPreviewer)previewForm.BusinessEntity).PreviewInvoices.Count);
					AssertNoErrors("Validation not run on previewed invoices", ((InvoicesPreviewer)previewForm.BusinessEntity).PreviewInvoices[0]);
				}
			}
		}

		public void TestPreviewFormUsesAllowedReportDeliveryOption()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			DummyShipment shipment;
			Charge charge;
			var testJob = CreateDummyShipmentJob(out shipment, out charge);
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			using (var form = new Form())
			{
				var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, testJob);
				wrapper.ParentForm = form;
				wrapper.AllowedReportDeliveryOption = AllowedDeliveryOptions.PreviewOnly;
				wrapper.Preview();

				using (var previewForm = ZFormModaliser.ActiveForm as InvoicePreviewForm)
				{
					AssertNotNull("Preview form shown", previewForm);
					AssertEquals(AllowedDeliveryOptions.PreviewOnly, previewForm.DeliveryOptions);
				}
			}

			using (var form = new Form())
			{
				var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, testJob);
				wrapper.ParentForm = form;
				wrapper.AllowedReportDeliveryOption = AllowedDeliveryOptions.All;
				wrapper.Preview();

				using (var previewForm = ZFormModaliser.ActiveForm as InvoicePreviewForm)
				{
					AssertNotNull("Preview form shown", previewForm);
					AssertEquals(AllowedDeliveryOptions.All, previewForm.DeliveryOptions);
				}
			}
		}

		#region PlugInData EditSecurity

		[TestDate(2005, 9, 10)]
		public void TestPlugInDataEditSecurity_Allowed_NotLock()
		{
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			DummyShipment shipment;
			Charge charge;
			InvoicingPostManagerGUIWrapper wrapper;
			var testJob = CreateDummyShipmentJob(out shipment, out charge);
			((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = false;
			ZFormModaliser.LastFormShownDialogForTest = null;
			wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob, null);
			wrapper.DoTestPostTransactions = true;

			wrapper.Post();

			AssertNull("Should NOT Prompt Login Form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have created one invoice", 1, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
		}

		[TestDate(2005, 9, 10)]
		public void TestPlugInDataEditSecurity_Allowed_Lock()
		{
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			DummyShipment shipment;
			Charge charge;
			InvoicingPostManagerGUIWrapper wrapper;
			var testJob = CreateDummyShipmentJob(out shipment, out charge);
			SecurityTestObject.CreateTestUser(true, shipment.InvoicingSupporter.EditSecurityCheckpoint.Code, "TST", "tst", "passwordRight");
			((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = true;
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var loginForm = form as LoginForm;
				if (loginForm != null)
				{
					loginForm.DoLoginForTest("TST", "passwordRight");
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				}
			});
			wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob, null);
			wrapper.DoTestPostTransactions = true;

			wrapper.Post();

			AssertType<LoginForm>("Should Prompt Login Form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have created one invoice", 1, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
		}

		[TestDate(2005, 9, 10)]
		public void TestPlugInDataEditSecurity_NotAllowed_NotLock()
		{
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			DummyShipment shipment;
			Charge charge;
			InvoicingPostManagerGUIWrapper wrapper;
			var testJob = CreateDummyShipmentJob(out shipment, out charge);
			((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = false;
			ZFormModaliser.LastFormShownDialogForTest = null;
			wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob, null);
			wrapper.DoTestPostTransactions = true;

			wrapper.Post();

			AssertNull("Should NOT Prompt Login Form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have created one invoice", 1, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
		}

		[TestDate(2005, 9, 10)]
		public void TestPlugInDataEditSecurity_NotAllowed_Lock()
		{
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			DummyShipment shipment;
			Charge charge;
			InvoicingPostManagerGUIWrapper wrapper;
			var testJob = CreateDummyShipmentJob(out shipment, out charge);
			((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = true;
			ZFormModaliser.LastFormShownDialogForTest = null;
			wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob, null);
			wrapper.DoTestPostTransactions = true;

			wrapper.Post();

			AssertType<LoginForm>("Should Prompt Login Form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should NOT have created one invoice", 0, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
		}

		class DummyShipmentProcessTask : ForwardingShipmentProcessTask
		{
			public DummyShipmentProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override Type ParentType
			{
				get { return typeof(DummyShipment); }
			}

			public new DummyShipment Parent
			{
				get { return (DummyShipment)base.Parent; }
			}
		}

		class DummyShipmentProcessTaskCollection : ForwardingShipmentProcessTaskCollection
		{
			public DummyShipmentProcessTaskCollection(DummyShipment shipment)
				: base(shipment)
			{
			}

			public new DummyShipmentProcessTask this[int index]
			{
				get { return (DummyShipmentProcessTask)Elements[index]; }
			}

			public new DummyShipmentProcessTask AddNew()
			{
				return (DummyShipmentProcessTask)base.AddNew();
			}

			public override ProcessTaskCollection CreateNewCollection()
			{
				return new DummyShipmentProcessTaskCollection(Parent);
			}

			new DummyShipment Parent
			{
				get { return (DummyShipment)base.Parent; }
			}
		}

		class DummyShipment : ForwardingShipment, IJobInvoicingPlugIn
		{
			public DummyShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override Freight.Business.CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
			{
				return new DummyShipmentInvoicingSupporter(this);
			}

			protected override ForwardingShipmentProcessTaskCollection GetNewWorkflowItems()
			{
				return new DummyShipmentProcessTaskCollection(this);
			}

			public class DummyShipmentInvoicingSupporter : ForwardingShipmentInvoicingSupporter
			{
				public DummyShipmentInvoicingSupporter(DummyShipment parent)
					: base(parent)
				{
				}

				protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
				{
					return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.AllowInvAmendmentsDays);
				}

				public bool fEditSecurityLockCore;
				protected override bool EditSecurityLockCore
				{
					get
					{
						return fEditSecurityLockCore;
					}
				}
			}
		}

		Job CreateDummyShipmentJob(out DummyShipment shipment, out Charge charge)
		{
			SetupInvoiceRollup(TestObjectCreator.ABIGAS);
			shipment = Factory.New<DummyShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_INCO = "FOB";
			shipment.JS_UniqueConsignRef = "S00010001";
			shipment.JS_HouseBill = "UVWXYZ";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ActualChargeable = 100M;
			Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			charge = testJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 150m;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			return testJob;
		}

		#endregion

		public void TestPlugInDataSetOnReloadedJob()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			ForwardingShipment testShipment = Factory.New<ForwardingShipment>();
			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = testShipment;
			testJob.JH_GE = department.PK;
			Factory.Save();

			InvoicingPostManagerGUIWrapper wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob, null);

			AssertEquals("1 job exists", 1, wrapper.Jobs_ForTestOnly.Count());
			AssertEquals("PluginData set on reloaded job", testShipment.PK, wrapper.Jobs_ForTestOnly.First().PlugInData.PK);
		}

		#region PaymentRequisitionStatusOverride

		[TestDate(2005, 9, 10)]
		public void TestPaymentRequisitionStatusOverrideWithSecurityNotAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			OverrideRequisitionDetailsSecurity.IsAllowed = false;

			Func<APInvoiceRequisitionForm> getShownDialog;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog);

			SetupJobDataForAPInvoiceTests();
			GUIWrapper.Post();

			AssertEquals("Invoice should be posted.", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable)));
			AssertNull("Requisition should not have shown.", getShownDialog());
		}

		[TestDate(2005, 9, 10)]
		public void TestPaymentRequisitionStatusOverrideWithRegistryNotAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			OverrideRequisitionDetailsSecurity.IsAllowed = true;

			Func<APInvoiceRequisitionForm> getShownDialog;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog);

			SetupJobDataForAPInvoiceTests();
			GUIWrapper.Post();

			AssertEquals("Invoice should be posted.", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable)));
			AssertNull("Requisition should not have shown.", getShownDialog());
		}

		[TestDate(2005, 9, 10)]
		public void TestPaymentRequisitionStatusOverrideWithRegistryAndSecurityAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			OverrideRequisitionDetailsSecurity.IsAllowed = true;

			Func<APInvoiceRequisitionForm> getShownDialog;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog);

			SetupJobDataForAPInvoiceTests();
			GUIWrapper.Post();

			AssertEquals("Invoice should be posted.", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable)));
			AssertNotNull("Requisition should have shown.", getShownDialog());
		}

		#endregion

		[TestDate(2005, 3, 11)]
		public void TestPostedTransactionsShownInInvoiceNumberOrder()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			BackDateInvoicesConfiguration config = BackDateInvoicesConfiguration_BackDateInvoicesTrue;
			config.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var shipment1 = creator.CreateShipment("S1");
			var job1 = creator.CreateJob(shipment1, false);
			job1.LocalChargesPK = creator.ABIGAS.PK;
			var charge1 = creator.CreateCharge(job1, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge2 = creator.CreateCharge(job1, TestObjectCreator.CC2, "desc", TestObjectCreator.AUD, 200m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 200m, TestObjectCreator.Agent);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge3 = creator.CreateCharge(job1, TestObjectCreator.CC3, "desc", TestObjectCreator.AUD, 300m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 300m, TestObjectCreator.Agent2);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, factory, job1);
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			wrapper.Post();

			AssertEquals("Should have created three invoices", 3, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
			AssertEquals("AllPostedInvoicePKs.Count", 3, wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Count);

			string previousInvoiceNumber = string.Empty;
			foreach (var pk in wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs)
			{
				var invoice = Factory.Load<InvoicingBase>(pk);
				AssertNotNull(invoice);
				Assert(string.Format("Must be ordered by InvoiceNumber but were {0}, {1}", previousInvoiceNumber, invoice.InvoiceNumber), invoice.InvoiceNumber.CompareTo(previousInvoiceNumber) >= 0);
				previousInvoiceNumber = invoice.InvoiceNumber;
			}
		}

		#region BackDateARInvoices

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithRegistryDisabled()
		{
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			try
			{
				Func<ChangeTransactionDatesMessageBox> getShownDialog;
				HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog);

				SetupJobDataForBackDateARInvoiceTests();
				GUIWrapper.Post();

				AssertEquals("Should have created one invoice", 1, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
				AssertEquals("Date should be current date", ZDateTime.Now.Date, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].AH_InvoiceDate.Date);
				AssertNull("Should have not shown question about back dating", getShownDialog());
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithTransactionDateSecurityNotAllowed()
		{
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			ModifyTransactionDateSecurity.IsAllowed = false;
			try
			{
				Func<ChangeTransactionDatesMessageBox> getShownDialog;
				HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog);

				SetupJobDataForBackDateARInvoiceTests();
				GUIWrapper.Post();

				AssertEquals("Should have created one invoice", 1, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
				AssertEquals("Date should be current date", ZDateTime.Now.Date, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].AH_InvoiceDate.Date);
				AssertNull("Should have not shown question about back dating", getShownDialog());
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithPostDateSecurityNotAllowed()
		{
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = true });
			ModifyPostDateSecurity.IsAllowed = false;
			try
			{
				Func<ChangeTransactionDatesMessageBox> getShownDialog;
				HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog);

				SetupJobDataForBackDateARInvoiceTests();
				GUIWrapper.Post();

				AssertEquals("Should have created one invoice", 1, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
				AssertEquals("Date should be current date", ZDateTime.Now.Date, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].AH_InvoiceDate.Date);
				AssertNull("Should have not shown question about back dating", getShownDialog());
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithRegistryEnabledAndUserAnsweringYes()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 8, 31), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 8, 31));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithRegistryEnabledAndUserAnsweringYesAndDefaultedPostDate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			BackDateInvoicesConfiguration config = BackDateInvoicesConfiguration_BackDateInvoicesTrue;
			config.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 8, 31), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 8, 31));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithRegistryEnabledAndUserAnsweringNo()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();

				InitializeBackDateARInvoiceWithBackDateDialogResultNo();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 9, 10));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithRegistryEnabledAndUserAnsweringCancel()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesTrue);

			AssertEquals("Prerequisite: there should be no invoices", 0, Factory.GetDatabaseCount(typeof(InvoicingBase)));

			try
			{
				SetupJobDataForBackDateARInvoiceTests();

				InitializeBackDateARInvoice(DialogResult.Cancel);
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 9, 10));
				AssertEquals("No invoices should have been created", 0, Factory.GetDatabaseCount(typeof(InvoicingBase)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesWithRegistryEnabledForOverridePostDate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = true });
			try
			{
				SetupJobDataForBackDateARInvoiceTests();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndSHPTermWhitEmptySHPDate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.FromShipmentDate.Code, 30);
				SetShipmentDate(ZDateTime.Empty);
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 10, 10));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndSHPTermWhitValidPastSHPDate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.FromShipmentDate.Code, 30);
				SetShipmentDate(ZDateTime.Now.AddDays(-20));
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 20));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndSHPTermWhitValidFarPastSHPDate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.FromShipmentDate.Code, 30);
				SetShipmentDate(ZDateTime.Now.AddDays(-100));
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndSHPTermWhitValidFutureSHPDate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.FromShipmentDate.Code, 30);
				SetShipmentDate(ZDateTime.Now.AddDays(20));
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 10, 30));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndCODTerm()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_BackDateInvoicesTrueOverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.CashOnDelivery.Code, 0);
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 8, 31), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 8, 31));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndINVTerm()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_BackDateInvoicesTrueOverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.FromInvoiceDate.Code, 30);
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 8, 31), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 09, 30));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndMTHTerm()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.FromMonthEnd.Code, 30);
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 10, 30));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndPERTerm()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.FromPeriodEnd.Code, 30);
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 9, 10),
					new AccountingPeriodCalculator(Factory).GetLastDayForPeriod(new ZDateTime(2005, 9, 10)).AddDays(30).Date);
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestBackDateARInvoicesWithRegistryEnabledForOverrideTransactionDateAndPIATermDays()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				BackDateInvoicesConfiguration_BackDateInvoicesTrueOverrideTransactionDateTrue);
			try
			{
				SetupJobDataForBackDateARInvoiceTests();
				SetInvoiceTerms(InvoiceTermsList.PaymentInAdvance.Code, 0);
				Factory.Save();

				InitializeBackDateARInvoiceWithBackDateDialogResultYes();
				AssertInvoiceDatesOnPosting(new ZDateTime(2005, 8, 31), new ZDateTime(2005, 9, 10), new ZDateTime(2005, 8, 31), new ZDateTime(2005, 8, 31));
				AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		protected virtual void InitializeBackDateARInvoiceWithBackDateDialogResultYes()
		{
			InitializeBackDateARInvoice(DialogResult.Yes);
		}

		protected virtual void InitializeBackDateARInvoiceWithBackDateDialogResultNo()
		{
			InitializeBackDateARInvoice(DialogResult.No);
		}

		void InitializeBackDateARInvoice(DialogResult backDateDialogResult)
		{
			ZFormModaliser.ResultToReturnFromShowDialog = backDateDialogResult;
		}

		protected virtual void AssertInvoiceDatesOnPosting(ZDateTime expectedInvoiceDate, ZDateTime expectedPostDate, ZDateTime lastMonthDate, ZDateTime expectedDueDate)
		{
			Func<ChangeTransactionDatesMessageBox> getShownDialog;
			Func<IBusiness> getShownDialogBusinessEntity;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog, out getShownDialogBusinessEntity);

			GUIWrapper.Post();

			AssertEquals("Should have created one invoice", 1, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
			InvoicingBase invoice = GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0];
			AssertNotNull("Should have shown question about back dating", getShownDialog());
			ChangeTransactionDatesBusinessObject lastBizo = getShownDialogBusinessEntity() as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastBizo);
			AssertEquals("Should have shown dialog box with correct invoice date", lastMonthDate, lastBizo.InvoiceDate.Date);
			AssertEquals("Should have shown dialog box with correct invoice date", expectedPostDate, lastBizo.PostDate.Date);
			AssertEquals("Should have posted invoice correct invoice date", expectedInvoiceDate, invoice.AH_InvoiceDate.Date);
			AssertEquals("Should have posted invoice correct post date", expectedPostDate, invoice.AH_PostDate.Date);
			AssertEquals("Should have posted invoice correct due date", expectedDueDate, invoice.AH_DueDate.Date);
		}

		protected virtual void SetupJobDataForBackDateARInvoiceTests()
		{
			SetupJobData();
			SetupChargeData(150, TestObjectCreator.CC1);
		}

		protected virtual void SetupJobDataForAPInvoiceTests()
		{
			SetupJobData();
			SetupCostChargeData(150, TestObjectCreator.CC1);
		}

		protected virtual SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.ModifyTransactionDate); }
		}

		protected virtual SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.ModifyPostDate); }
		}

		protected virtual SecurityCheckpoint OverrideRequisitionDetailsSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.OverrideRequisitionDetails); }
		}

		protected virtual void SetShipmentDate(ZDateTime date)
		{
			((ForwardingShipment)Job1.PlugInData).JS_E_ARV = date;
		}

		protected virtual void SetInvoiceTerms(ZString term, ZByte termDays)
		{
			TestObjectCreator.LocalClient.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = term;
			TestObjectCreator.LocalClient.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = termDays;
		}

		protected BackDateInvoicesConfiguration BackDateInvoicesConfiguration_BackDateInvoicesFalse
		{
			get
			{
				return new BackDateInvoicesConfiguration();
			}
		}

		protected BackDateInvoicesConfiguration BackDateInvoicesConfiguration_BackDateInvoicesTrue
		{
			get
			{
				BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
				config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
				config.InvoiceDateConfigurationCollection[0].Today = true;
				return config;
			}
		}

		protected BackDateInvoicesConfiguration BackDateInvoicesConfiguration_BackDateInvoicesTrueOverrideTransactionDateTrue
		{
			get
			{
				BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
				config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
				config.InvoiceDateConfigurationCollection[0].Today = true;
				config.InvoiceDateConfigurationCollection[0].Override = true;
				return config;
			}
		}

		protected BackDateInvoicesConfiguration BackDateInvoicesConfiguration_OverrideTransactionDateTrue
		{
			get
			{
				BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
				config.InvoiceDateConfigurationCollection[0].Today = true;
				config.InvoiceDateConfigurationCollection[0].Override = true;
				return config;
			}
		}

		protected BackDateInvoicesConfiguration BackDateInvoicesConfiguration_OverrideTransactionDateFalse
		{
			get
			{
				return new BackDateInvoicesConfiguration();
			}
		}

		#endregion

		#region TransactionDescriptionDefaulting

		public void TestTransactionDescriptionDefaulting()
		{
			SetupJobDataForBackDateARInvoiceTests();

			if (GUIWrapperPlugIn != null)
			{
				var jobInvoiceDescriptionConfig = AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.Value;
				var item = jobInvoiceDescriptionConfig.AddNew();
				item.JobType = GUIWrapperPlugIn.InvoicingSupporter.ConsumerType.Code;
				if (GUIWrapperPlugIn.InvoicingSupporter.ConsumerType.IsDirectionSupported)
				{
					item.DirectionCode = "ALL";
				}
				if (GUIWrapperPlugIn.InvoicingSupporter.ConsumerType.IsTransportModeSupported)
				{
					item.Mode = "ALL";
				}
				item.InvoiceDescription = "Default Description";
				AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobInvoiceDescriptionConfig);

				GUIWrapper.Post();

				AssertEquals("Should have created one invoice", 1, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
				AssertEquals("Description should be set from default", "Default Description", GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].AH_Desc);
			}
			else
			{
				Assert("Is not Applicable for this class because it cannot provide an IJobInvoicingPlugIn", true);
			}
		}

		#endregion

		[TestDate(2005, 9, 10)]
		public void TestSecurityOverrideInvoicingLevels()
		{
			SetupJobData();
			SetupChargeData(-150m, TestObjectCreator.CC1);

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, Job1);
			wrapper.Post();

			AssertEquals("Should have created one invoice", 1, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);

			Enterprise.Environment.Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;

			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);

			SetupChargeData(-150m, TestObjectCreator.CC1);

			ZFormModaliser.LastFormShownDialogForTest = null;
			wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, Job1);
			wrapper.ShowLoginFormForTest = true;
			wrapper.Post();

			AssertEquals(true, wrapper.PostManager_ForTestOnly.CancelPosting);
			Assert("Should not create any invoices", !wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.IsInDatabaseIncludingChildren);
			AssertEquals("Should prompt login form", true, ZFormModaliser.LastFormShownDialogForTest.GetType().IsSubclassOf(typeof(LoginForm)));

			ZFormModaliser.LastFormShownDialogForTest = null;
			Func<LoginForm> getShownDialog;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog);
			wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, Job1);
			wrapper.ShowLoginFormForTest = false;
			wrapper.Post();

			AssertNull("Should not prompt login form", getShownDialog());
			AssertEquals(false, wrapper.PostManager_ForTestOnly.CancelPosting);
			Assert("Should have created one invoice", wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.IsInDatabaseIncludingChildren);
		}

		public void TestMessageIfInvalidAL_AC_AL_AG()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode.AC_AG_AccrualAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = TestObjectCreator.GLHeader2.PK;

			SetupJobData();
			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = 10m;
			charge.JR_OSCostAmt = 20m;
			charge.JR_OSCostExRate = 1;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			wrapper.Post();
			ZString errormessage = TransactionLine.GetInvalidChargeCodeError("", "");

			Assert("Last message should be that Charge Code must have GL Account entered.", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(errormessage.Left(10)));
		}

		public void TestMessageIfValidAL_AC_AL_AG()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = Constants.ChargeType.Revenue;
			chargeCode.AC_AG_AccrualAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = TestObjectCreator.GLHeader2.PK;

			SetupJobData();
			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = 10m;
			charge.JR_OSCostAmt = 20m;
			charge.JR_OSCostExRate = 1;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_PaymentType = ReceiptTypes.Cash;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			chargeCode.AC_AG_RevenueAccount = TestObjectCreator.GLHeader2.PK;
			chargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			wrapper.Post();
			Assert("No errors expected.", !UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		[TestDate(2005, 1, 1)]
		public void TestMessageIfChequeBookUsesSamePrinterShown()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			DocumentEngine.Scheduler.Business.StmPrintQueue printer = Factory.New<DocumentEngine.Scheduler.Business.StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;
			chequeBook.AK_StartNo = chequeBook.AK_CurrentNo = 1;
			chequeBook.AK_LastNo = 10;

			AccChequeBook book2 = Factory.New<AccChequeBook>();
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;
			book2.AK_StartNo = book2.AK_CurrentNo = 100;
			book2.AK_LastNo = 110;

			SetupJobData();
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 10m;
			charge.JR_OSCostAmt = 20m;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_APInvoiceNum = "1111111";
			charge.JR_APInvoiceDate = new ZDateTime(2005, 1, 1);
			charge.JR_AB = testBank.PK;
			charge.JR_AK = chequeBook.PK;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1).Post();

			AssertEquals("Last message should be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestConfirmIfCreditLimitExceeded()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0100001", TestObjectCreator.AUD, 1m, 50m, 0m, 50m, 0m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;

			TestObjectCreator.ABIGAS.CompanyData.OB_ARCreditLimit = 15m;

			Factory.Save();

			Assert(TestObjectCreator.ABIGAS.CreditChecker.IsCreditLimitExceededClearCacheForTest(LedgerTypes.AccountsReceivable).Value);

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode.AC_AG_AccrualAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = TestObjectCreator.GLHeader2.PK;

			SetupJobData();
			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = 10m;
			charge.JR_OSCostAmt = 20m;
			charge.JR_OSCostExRate = 1;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_PaymentType = ReceiptTypes.Cash;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			chargeCode.AC_AG_RevenueAccount = TestObjectCreator.GLHeader2.PK;
			chargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;

			Factory.Save();

			Assert(TestObjectCreator.ABIGAS.CreditChecker.IsCreditLimitExceededClearCacheForTest(LedgerTypes.AccountsReceivable).Value);

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			AccountingConfigurationRegistry.Instance.ShowCreditLimitWarningOnJobPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			wrapper.Post();

			Assert("Last message should be a Message.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			Assert("Last message should be that it is over credit limit.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("which is over the credit limit."));

			// We cancelled posting and can try to post again
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			wrapper.Post();

			Assert("Last message should be a Message.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			Assert("Last message should be that it is over credit limit.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("which is over the credit limit."));
		}

		[TestDate(2005, 9, 10)]
		public void TestPaymentAutoAllocation()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesTrue);
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				SetupJobData_AutoAllocationTest();

				TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				wrapper.Post();

				AccTransactionHeaderCollection postedTransactions = new AccTransactionHeaderCollection(Factory);
				postedTransactions.Load(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("Should have created 2 payments", 2, postedTransactions.Count);

				AccTransactionHeader payment1 = GetPaymentFromCollection(postedTransactions, "5");
				AssertNotNull("Payment should exist", payment1);
				AccTransactionHeader payment2 = GetPaymentFromCollection(postedTransactions, "3");
				AssertNotNull("Payment should exist", payment2);

				AssertEquals("Charge should have its cheque No set", "5", Charge.JR_ChequeNo);
				AssertEquals("Charge2 should have its cheque No set", "3", Charge2.JR_ChequeNo);

				Assert("Auto printing should be performed", wrapper.Test_Allocator_ForTestOnly.ChequeWasAutoPrinted);
				AssertEquals("Printing called only once", 1, wrapper.Test_Allocator_ForTestOnly.PrintingCalled_Counter);

				AssertEquals("There should be 2 printers passed for printing", 2, wrapper.Test_Allocator_ForTestOnly.PrintersPassedForAutoPrinting.Count);
				Assert("Printer #1", wrapper.Test_Allocator_ForTestOnly.PrintersPassedForAutoPrinting.Contains(AutoAllocateChequeBook.AK_SQ));
				Assert("Printer #2", wrapper.Test_Allocator_ForTestOnly.PrintersPassedForAutoPrinting.Contains(AutoAllocateChequeBook2.AK_SQ));
				AssertEquals("There should be 2 collections of payments passed for autoprinting", 2, wrapper.Test_Allocator_ForTestOnly.CollectionsPassedForPrinting.Count);
				AssertEquals("There should be only 1 payment in collection 1", 1, wrapper.Test_Allocator_ForTestOnly.CollectionsPassedForPrinting[0].Count());
				AssertEquals("There should be only 1 payment in collection 2", 1, wrapper.Test_Allocator_ForTestOnly.CollectionsPassedForPrinting[1].Count());

				ZGuid paymentOfCollection1;
				ZGuid paymentOfCollection2;
				if (wrapper.Test_Allocator_ForTestOnly.CollectionsPassedForPrinting[0].First().AH_ChequeOrReference == "5")
				{
					paymentOfCollection1 = payment1.PK;
					paymentOfCollection2 = payment2.PK;
				}
				else
				{
					paymentOfCollection2 = payment1.PK;
					paymentOfCollection1 = payment2.PK;
				}
				Assert("Collection should contain the payment1", wrapper.Test_Allocator_ForTestOnly.CollectionsPassedForPrinting[0].Any(x => x.PK == paymentOfCollection1));
				Assert("Collection should contain the payment2", wrapper.Test_Allocator_ForTestOnly.CollectionsPassedForPrinting[1].Any(x => x.PK == paymentOfCollection2));

				AutoAllocateChequeBook.Reload();
				AutoAllocateChequeBook2.Reload();
				AssertEquals("CurrentNo should change on AutoAllocateChequeBook", 6m, AutoAllocateChequeBook.AK_CurrentNo);
				Assert("AutoAllocateChequeBook is still active", AutoAllocateChequeBook.AK_IsActive);
				AssertEquals("CurrentNo should change on AutoAllocateChequeBook2", 4m, AutoAllocateChequeBook2.AK_CurrentNo);
				Assert("AutoAllocateChequeBook should become to be inactive", !AutoAllocateChequeBook2.AK_IsActive);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			SetupJobData_AutoAllocationTest("TestAutoAllocationAndPrintCheques");

			var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (new DisposableAction(() => Globals.SetIsUnitTestingProductionFunctionality(true), () => Globals.SetIsUnitTestingProductionFunctionality(false)))
			{
				AssertNoExceptionThrown("does not throw", () => wrapper.Post());
			}
			AssertEquals("Message match", "Auto cheque printing failed. Please check Auto cheque printing settings. Testing Auto Print Cheques failure.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[TestDate(2005, 9, 10)]
		public void TestNonEmptyMenuPathForChequeTemplate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			SetupJobData_AutoAllocationTest("TestNonEmptyMenuPath");
			var bankAccount = Factory.Load<AccBankAccount>(AutoAllocateChequeBook.AK_AB);
			var chequeTemplate = bankAccount.ChequeTemplate.SO_Name;

			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			AssertEquals(true, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate(chequeTemplate, payment));

			var zQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, chequeTemplate);
			var menuItem = Factory.LoadTop1<StmMenuItem>(zQuery);
			menuItem.SU_MenuPath = "ABC";
			Factory.Save();

			AssertEquals(false, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate(chequeTemplate, payment));

			var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Job1);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (new DisposableAction(() => Globals.SetIsUnitTestingProductionFunctionality(true), () => Globals.SetIsUnitTestingProductionFunctionality(false)))
			{
				AssertNoExceptionThrown("does not throw", () => wrapper.Post());
			}
			AssertEquals("Message match",
				"Auto cheque printing failed. Please check Auto cheque printing settings. Please make sure 'Standard' Document menu with Empty menu path value exists for cheque template Standard. If not, please create one through the Document Menu Customization screen",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPaymentCreationErrorMessages()
		{
			var expectedMessage = @"{0} could not be posted due to the following errors:
Account: An AP Bank Account could not be found with currency AUD and payment type DDR for the payee {1}.

Please set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting " + "\"Edit Payment Organization Detail\"" +
@", with the currency AUD and payment type of DDR.";

			TestObjectCreator.AUDBankAccount.AB_AllowAutoDDR = true;

			SetupJobData();
			var charge1 = SetUpAPInvoiceWithDDRPayment(100, "11111", "Test DDR 1", TestObjectCreator.Creditor1);
			var charge2 = SetUpAPInvoiceWithDDRPayment(200, "22222", "Test DDR 2", TestObjectCreator.Creditor1);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Job1);
			wrapper.Post();

			AccTransactionHeaderCollection postedTransactions = new AccTransactionHeaderCollection(Factory);
			postedTransactions.Load(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			var expectedNotification = string.Join(System.Environment.NewLine + System.Environment.NewLine,
					string.Format(expectedMessage, string.Format("Payment {0} DDR {1} (AUD {2})", TestObjectCreator.Creditor1.OH_Code, charge1.JR_ChequeNo, charge1.JR_OSCostAmtWithGSTAmt.ToString(TestObjectCreator.AUD.Decimals)), TestObjectCreator.Creditor1.OH_Code),
					string.Format(expectedMessage, string.Format("Payment {0} DDR {1} (AUD {2})", TestObjectCreator.Creditor1.OH_Code, charge2.JR_ChequeNo, charge2.JR_OSCostAmtWithGSTAmt.ToString(TestObjectCreator.AUD.Decimals)), TestObjectCreator.Creditor1.OH_Code));
			AssertEquals("Should have created no payments", 0, postedTransactions.Count);
			AssertContains("Payment creation error notification", expectedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Should have created one fully approved Payment Approval", 2, new BusinessObjectFactory().Load<AccPaymentApproval>(new ZQuery(AccPaymentApprovalSchema.AV_Status, PaymentApprovalStatus.FullyApproved)).Length);

			TestObjectCreator.AddAPBankAccountDetails(TestObjectCreator.Creditor1, ReceiptTypes.DirectDebit, TestObjectCreator.AUD);

			SetUpAPInvoiceWithDDRPayment(300, "33333", "Test DDR 3", TestObjectCreator.Creditor1);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Job1);
			wrapper.Post();

			postedTransactions = new AccTransactionHeaderCollection(Factory);
			postedTransactions.Load(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertEquals("Should have created 1 payment", 1, postedTransactions.Count);
			AssertEquals("Should have created one posted Payment Approval", 1, new BusinessObjectFactory().Load<AccPaymentApproval>(new ZQuery(AccPaymentApprovalSchema.AV_AH, postedTransactions[0].PK)).Length);
		}

		public void TestInvoiceTermOverriddeMessages()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				var expectedMessage = "Invoice Term overridden for following Debtor(s) --\r\n\r\n[ABIGAS] : Invoice terms are CUS - 4 Days from Customs Clearance date. As there is no CLR event on this job, Invoice Term bases on Shipment date";

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				TestObjectCreator.ABIGAS.OH_IsCreditor = true;
				TestObjectCreator.ABIGAS.OH_IsDebtor = true;
				OrgARTerms term = TestObjectCreator.ABIGAS.CompanyData.ARTerms[0];
				TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = "DEF";
				term.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				term.PY_InvoiceTerm = InvoiceTermsList.FromCustomsClearanceDate.Code;
				term.PY_InvoiceDays = 4;

				var shipment = TestObjectCreator.CreateShipment("S000011", "AUSYD", "AUMEL");
				shipment.JS_E_ARV = new ZDateTime(2015, 07, 25);
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.ABIGAS, 1.0m, TestObjectCreator.Agent, 1.0M);
				SetupCostAndSellChargeData(job, 500, CC1, 1700, TestObjectCreator.ABIGAS, TestObjectCreator.ABIGAS);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job);
				wrapper.DoTestPostTransactions = false;
				wrapper.Post();

				AssertContains("Invoice Term Overridde notification", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevValue;
			}
		}

		Charge SetUpAPInvoiceWithDDRPayment(decimal osAmount, string invoiceNum, string paymentRef, OrgHeader creditor)
		{
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = osAmount;
			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_APInvoiceNum = invoiceNum;
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			SetAPPaymentInfo(charge, ReceiptTypes.DirectDebit, TestObjectCreator.AUDBankAccount, paymentRef);
			return charge;
		}

		[TestDate(2005, 9, 10)]
		public void TestRecognizeRevenueBehaviourForDateBeforeFirstPeriod()
		{
			string expectedMessage = "cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date. A General Ledger Accounting Period cannot be created because it is earlier than the first period currently existing.";
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			SetupJobData();
			SetupChargeData(-150m, TestObjectCreator.CC1);

			ForwardingShipment shipment = Job1.PlugInData as ForwardingShipment;

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, Job1);
			Job testJob = wrapper.Jobs_ForTestOnly.First();

			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.BrettsBirthday;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			Factory.Save();
			wrapper.Preview();
			AssertEquals(false, Contains(expectedMessage, UnitTestUserNotification.Instance.PreviousMessages));
			AssertEquals("Should have not created any invoices", 0, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);

			wrapper.Post();
			AssertEquals(true, Contains(expectedMessage, UnitTestUserNotification.Instance.PreviousMessages));
			AssertEquals("Should have not created any invoices", 0, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Factory.Save();
			wrapper.Post();
			AssertEquals(false, Contains(expectedMessage, UnitTestUserNotification.Instance.PreviousMessages));
			AssertEquals("Should have not created any invoices", 0, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);

			testJob.ResetPreviouseRespose_ShouldUseImmediateRevenueRecognisedDate();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Factory.Save();
			wrapper.Post();
			AssertEquals(true, Contains(expectedMessage, UnitTestUserNotification.Instance.PreviousMessages));
			AssertEquals("Should have created one invoice", 1, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
		}

		#region Concurrency Testing

		#region Action Delegates

		Charge CreateJobWithCharge(BusinessObjectFactory factory)
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(factory);
			testHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(factory);

			AccChargeCode chargeCode = factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode.AC_AG_AccrualAccount = testObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = testObjectCreator.GLHeader2.PK;
			chargeCode.AC_AG_RevenueAccount = testObjectCreator.GLHeader2.PK;
			chargeCode.AC_AG_CostAccount = testObjectCreator.GLHeader1.PK;

			SetupJobData(factory);
			Job1.JH_GE = testObjectCreator.NonCurrentDepartment.PK;
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = 10m;
			charge.JR_OSCostAmt = 20m;
			charge.JR_OSCostExRate = 1;
			charge.JR_Desc = "Testing";
			charge.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_OH_CostAccount = testObjectCreator.ABIGAS.PK;
			factory.Save();
			return charge;
		}

		Charge LoadChargeFromPK(BusinessObjectFactory factory, ZGuid pK)
		{
			Charge result = factory.Load<Charge>(pK);
			AssertNotNull(result);
			return result;
		}

		void UpdateSellAmountAndSave(BusinessObjectFactory factory, Charge charge)
		{
			charge.JR_OSSellAmt = 200;
			factory.Save();
		}

		void PostRevenue(BusinessObjectFactory factory, JobHeader jobHeader)
		{
			Job job = factory.Load<Job>(jobHeader.PK);
			BusinessObjectFactory newIsolatedFactory = new BusinessObjectFactory();
			newIsolatedFactory.RefreshEnabled = false;
			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, factory, job, newIsolatedFactory, new EventHandler(DoStuffBeforeSave));
			wrapper.DoTestPostTransactions = true;
			wrapper.Post();
		}

		void DoStuffBeforeSave(object sender, EventArgs e)
		{
			Session2.PerformAction(UpdateSellAmountAndSave, ChargeInSession2);
		}

		#endregion

		SessionForConcurrencyTesting Session2;
		Charge ChargeInSession2;

		public void TestConcurrencyIssueOnChargesGetHandled()
		{
			SessionForConcurrencyTesting session1 = new SessionForConcurrencyTesting();
			Session2 = new SessionForConcurrencyTesting();

			Charge chargeInSession1 = session1.PerformActionWithReturnValue(CreateJobWithCharge);
			ChargeInSession2 = Session2.PerformActionWithReturnValue(LoadChargeFromPK, chargeInSession1.PK);

			session1.PerformAction(PostRevenue, chargeInSession1.Job);

			string expectedMessage = "Please try to post these charges again. Posting has failed. One or more of the charges you are attempting to post has been modified by another user. The other users changes have been merged with yours.";
			AssertContains("Must not be mergeable.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Charge must be synchronized in different sessions.", ChargeInSession2.JR_OSSellAmt, chargeInSession1.JR_OSSellAmt);
			AssertEquals("Amount must have updated value", 200M, chargeInSession1.JR_OSSellAmt);
		}

		#endregion

		public void TestGetNewPostManagerWithCorrectFactory()
		{
			SetupJobData();
			SetupChargeData(-150m, TestObjectCreator.CC1);
			SetupPosting();
			GUIWrapper.Post();
			AssertEquals(GUIWrapper.TransactionFactory_ForTestOnly, GUIWrapper.GetNewPostManager_ForTestOnly().Factory);
		}

		public virtual void TestGetParentForPostingAction()
		{
			var parent = GUIWrapper.GetParentInfoForPostingAction_ForTestOnly();
			var parentID = parent.Id;
			var parentTableCode = parent.TableCode;

			AssertEquals(GUIWrapper.Jobs_ForTestOnly.First().PK, parentID);
			AssertEquals(JobHeaderSchema.Constants.Prefix, parentTableCode);
		}

		#region TestOverrideTransactionDescription

		[TestDate(2005, 9, 10)]
		public void TestOverrideTransactionDescription_Case1()
		{
			Env.Security.AROverrideTransactionDescription.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.PopupARInvoiceDescriptionOverrideOnPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertOverrideTransactionDescription(true);
		}

		[TestDate(2005, 9, 10)]
		public void TestOverrideTransactionDescription_Case2()
		{
			Env.Security.AROverrideTransactionDescription.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.PopupARInvoiceDescriptionOverrideOnPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertOverrideTransactionDescription(false);
		}

		[TestDate(2005, 9, 10)]
		public void TestOverrideTransactionDescription_Case3()
		{
			Env.Security.AROverrideTransactionDescription.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.PopupARInvoiceDescriptionOverrideOnPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertOverrideTransactionDescription(false);
		}

		[TestDate(2005, 9, 10)]
		public void TestOverrideTransactionDescription_Case4()
		{
			Env.Security.AROverrideTransactionDescription.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.PopupARInvoiceDescriptionOverrideOnPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertOverrideTransactionDescription(false);
		}

		protected virtual void AssertOverrideTransactionDescription(bool shouldOverride)
		{
			SetupJobData();
			SetupChargeData(150, TestObjectCreator.CC1);

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
			{
				OverrideTransactionDescriptionForm form = (OverrideTransactionDescriptionForm)formOrDialog;
				OverrideTransactionDescriptionHelper bizo = (OverrideTransactionDescriptionHelper)form.BusinessEntity;
				form.Shown += new EventHandler((sender, e) =>
				{
					bizo.WrappedObjects[0].AH_Desc = "My test description";
					form.FireSaveButton();
				});
			});
			SetupPosting();
			GUIWrapper.Post();

			AssertEquals("Should have created one invoice", 1, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
			var postedInvoice = Factory.Load<TransactionHeader>(GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].PK);
			if (shouldOverride)
			{
				AssertType("LastFormShownDialogForTest", typeof(OverrideTransactionDescriptionForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should have created one invoice", "My test description", postedInvoice.AH_Desc);
			}
			else
			{
				AssertEquals("LastFormShownDialogForTest should not be shown", null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#endregion

		public virtual void TestReloadChargesIfDifferencesInDb()
		{
			var otherUserFactory = new BusinessObjectFactory() { RefreshEnabled = false }; // Factory for interfering user who will add additional charge(s) just before our main Factory attempts to post

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			SetupJobData();

			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			Job testJobLoadedByOtherUser = otherUserFactory.Load<Job>(Job1.PK);
			CreateCharge(testJobLoadedByOtherUser, CC1, "Charge Code 2", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			otherUserFactory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals("Before reloading we only know of the one charge.", 1, Job1.Charges.Count);
			wrapper.Post();
			AssertEquals("Expect error because another user has made changes.", @"This job cannot be posted because changes have been made by another user since you have saved. These changes have been reloaded.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("After reloading we now know of the additional charge.", 2, Job1.Charges.Count);
		}

		public void TestJobsWhenJobDeactivated()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job);

			AssertEquals("Contain one charge", 1, wrapper.OriginalJobs_ForTestOnly.Count());
			AssertEquals("Contain one charge", 1, wrapper.Jobs_ForTestOnly.Count());

			var newFactory = new BusinessObjectFactory();
			Job newJob = newFactory.Load<Job>(job.PK);
			newJob.MarkAsInactive();
			newFactory.Save();

			AssertEquals("Contain one deactivated Job", 1, wrapper.OriginalJobs_ForTestOnly.Count());
			AssertEquals("Contain one deactivated Job", 1, wrapper.Jobs_ForTestOnly.Count());
		}

		public void TestJobsWhenJobDeactivatedForPostManagerGUIWrapper()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.ActiveOrAllAddresses[0].PK;
			CreateCharge(job, CC1, "Charge Code 2", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var newJob = newFactory.Load<Job>(job.PK);
			newJob.MarkAsInactive();
			newFactory.Save();
			var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wrapper.Post();
			AssertEquals(@"You cannot post because no Job Invoices have been created.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public virtual void TestCustomsDisbursementChargesDuplicateMessageOnlyAppliesToCustomCharges()
		{
			var customsChargeCodePKs = Enterprise.Registry.Business.Customs.EntryChargeTypeList.GetAllChargeCodePKsOf(Env.CurrentCompany.PK, Env.CurrentCompany.Country.Code);
			Assert("CC1 charge code has not been added to the customs disbursement list", !customsChargeCodePKs.Contains(CC1.PK));

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			SetupJobData();

			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wrapper.Post();
			Assert("No errors expected for general duplicate charge code", !UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public virtual void TestSayingYesNoToCustomsDisbursementChargesDuplicateMessage()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			SetupJobData();

			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC1.PK.ToGuid());
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			wrapper.Post();
			Assert("Last message should be a question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			var expectedWarningMessage = @"There are multiple customs disbursement charges with the same charge code: ZZCC1

Continue with posting?";
			AssertEquals("Warning expected due to duplicate customs disbursement charge code.", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			wrapper.Post();
			Assert("Last message should be a question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
		}

		[TestDate(2018, 10, 10)]
		public virtual void TestSayingYesNoToPortugalSAFTMissingGSTMessage()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			SetupJobData();

			var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 1, 100, 1);
			sequence.XD_Prefix = "01.02-";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;

			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "PTLIS";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				wrapper.Post();
				Assert("Last message should be a question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				var expectedWarningMessage = @"The transaction is missing information that is mandatory for your country/region reporting:
Tax Registration Number of the Debtor [ZLOCCLT]

Continue with posting?
";
				AssertMultilineASCIIEquals("Warning expected due to missing SAFT GST.", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				wrapper.Post();
				Assert("Last message should be a question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public virtual void TestWhenLoadJobsInNewFactorySetsParentItAlsoSetsRelatedJobs()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			SetupJobData();
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);

			var shipment = ((ForwardingShipment)Job1.PlugInData);
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			var childShipment = TestObjectCreator.CreateShipment(shipment.JobNumber + "_0");
			var childJob = TestObjectCreator.CreateJob(childShipment, false);
			CreateCharge(childJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);

			shipment.CoLoadShipments.Add(childShipment);

			Factory.Save();

			((IAutoRatingAccountingUtils)Job1).ReloadChargesFromAdditionalJobs();
			AssertEquals("Precondition: Count of AdditionalJobsToShowChargesFor", 1, ((IJobInvoicingPlugInAdditionalJobs)Job1.PlugInData).AdditionalJobsToShowChargesFor.Length);
			AssertEquals("Both charges found in reloaded job", 2, Job1.Charges.Count);

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wrapper.Post();
			AssertNull("No messages (and therefore must be no message about concurrency error due to difference in charges)", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Both charges found in reloaded job", 2, wrapper.Jobs_ForTestOnly.First().Charges.Count);
		}

		public void TestPostingPluginFactoryExclusion()
		{
			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			SetupJobData();
			Job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			wrapper.DoTestPostTransactions = false;
			wrapper.Post();

			var assertionFactory = new BusinessObjectFactory();
			var orgReloaded = assertionFactory.Load<OrgHeader>(org.PK);

			AssertNull("Plugin factory should be excluded, therefore new org not saved", orgReloaded);
		}

		public virtual void TestCreditLimitEmailsAreSentWhenPosting()
		{
			SetupForCreditLimitEmails();
			SetupJobData();
			SetupMultipleInvoices(Job1);

			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			AssertEquals("0 Emails should be sent", previousEmailCount, 0);
			GUIWrapper.DoTestPostTransactions = false;
			GUIWrapper.Post();

			AssertEquals("4 Emails should be sent", previousEmailCount + 4, Env.OutgoingMailManager.EmailsCreated.Count);
			var aPPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, GUIWrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs));
			var aRPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, GUIWrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs));
			AssertEquals("Should have created 6 invoices", 6, aPPostedInvoices.Length + aRPostedInvoices.Length);
			Env.OutgoingMailManager.EmailsCreated.Sort(new Comparison<EmailDef>(CompareEmailDef));

			AssertEquals("Credit Limit Exceeded by AUD $2,646.80 (ABI GAS & TOOLS / ABIGAS)", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("Credit Limit Exceeded by AUD $2,646.80 (Test Company Name / ZDebtor)", Env.OutgoingMailManager.EmailsCreated[1].Subject);
			AssertEquals("Credit Limit Exceeded by AUD $800.00 (Test Company Name / ZDebtor)", Env.OutgoingMailManager.EmailsCreated[2].Subject);
			AssertEquals("Credit Limit Exceeded by AUD $900.00 (ABI GAS & TOOLS / ABIGAS)", Env.OutgoingMailManager.EmailsCreated[3].Subject);
		}

		[TestDate(2015, 1, 1)]
		public void TestBackDateARInvoicesInvoiceDateDefaultingBehaviourMonthEndSuspension()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = true });
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			var configDateTime = ZDateTime.Now.AddMonths(1).ToDateTime();
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configDateTime);

			var currentARInvoiceDate = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDateMenuCaption();
			AssertEquals("Current Invoice Date : 28-Feb-15", currentARInvoiceDate.ToString());

			SetupJobDataForBackDateARInvoiceTests();

			Func<ChangeTransactionDatesMessageBox> getShownDialog;
			Func<IBusiness> getShownDialogBusinessEntity;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog, out getShownDialogBusinessEntity);

			GUIWrapper.Post();

			AssertEquals("Should have created one invoice", 1, GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
			InvoicingBase invoice = GUIWrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0];
			AssertNull("Should not have shown question about back dating", getShownDialog());
			ChangeTransactionDatesBusinessObject lastBizo = getShownDialogBusinessEntity() as ChangeTransactionDatesBusinessObject;
			AssertNull(lastBizo);

			var postDate = new DateTime(2015, 2, 28);
			var invoiceDate = new DateTime(2015, 2, 28);
			var dueDate = new DateTime(2015, 2, 28);

			AssertEquals("Should have posted invoice correct invoice date", invoiceDate, invoice.AH_InvoiceDate.Date);
			AssertEquals("Should have posted invoice correct post date", postDate, invoice.AH_PostDate.Date);
			AssertEquals("Should have posted invoice correct due date", dueDate, invoice.AH_DueDate.Date);

			AssertEquals("Invoices should have been created", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)));
		}

		public void TestRunInvoiceDateNotGreaterThanPostDateValidation()
		{
			using (AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var config = BackDateInvoicesConfiguration_BackDateInvoicesTrue;
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

				var factory = new BusinessObjectFactory();
				var creator = new TestObjectCreator(factory);

				var shipment1 = creator.CreateShipment("S1");
				var job1 = creator.CreateJob(shipment1, false);
				job1.LocalChargesPK = creator.ABIGAS.PK;
				var charge1 = creator.CreateCharge(job1, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge1.JR_APInvoiceNum = "INV001";
				charge1.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);

				factory.Save();

				var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, factory, job1);
				wrapper.Post();

				AssertEquals(@"Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should not create any invoices", !wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
			}
		}

		[TestDate(2020, 10, 06)]
		public void TestRunInvoiceTaxDateHasTaxRateValidation()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var config = BackDateInvoicesConfiguration_BackDateInvoicesTrue;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "ALL";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AR";
			taxDateOption.TaxDateOption = "INV";

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var factory = new BusinessObjectFactory();
				// simulate the case where the tax rate is missing for the tax date
				var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(factory);
				taxRate.SetRate_ForTestOnly(6, 10, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(+2));
				factory.Save();

				var creator = new TestObjectCreator(factory);
				var shipment1 = creator.CreateShipment("S1");
				var job1 = creator.CreateJob(shipment1, false);
				job1.LocalChargesPK = creator.ABIGAS.PK;
				creator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				var charge1 = creator.CreateCharge(job1, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				using (charge1.GetValidationSuspender())
				{
					charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
					charge1.JR_AT_SellGSTRate = taxRate.PK;

					factory.Save();

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					InitializeBackDateARInvoiceWithBackDateDialogResultYes();
					var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, factory, job1);
					wrapper.Post();

					AssertEquals(@"Posting is prevented. No valid Tax Rate found for the selected Tax Date, or Tax Date is empty.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestJobOnHoldMessageIsShownAsWarning()
		{
			var postManager = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
			UnitTestUserNotification.Instance.ClearMessages();
			postManager.JobsOnHold_ForTestOnly(this, new BasePostManager.OnJobOnHoldEventArgs(new[] { Job1 }));
			Assert("Job on hold message should be a warning", UnitTestUserNotification.Instance.LastMessage.WasWarning);
		}

		public void TestNotCheckPostingGroupsWhenComplianceDocumentModuleEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
				vat3.AT_PostingGroupId = 0;

				var vat5 = TestObjectCreator.CreateTaxRate("VAT5", "VAT5", 5);
				vat5.AT_PostingGroupId = 1;

				Factory.Save();

				SetupJobData();

				LocalClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m, 0m); //cleaning up cfx otherwise it will be applied when we change country

				var charge1 = CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
				var charge2 = CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
				charge1.JR_AT_CostGSTRate = vat3.PK;
				charge2.JR_AT_CostGSTRate = vat5.PK;
				charge1.JR_APInvoiceNum = charge2.JR_APInvoiceNum = "APINV321";
				charge1.JR_APInvoiceDate = charge2.JR_APInvoiceDate = ZDateTime.Today;

				Factory.Save();

				TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Job1);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var expectedWarningMessage = @"AP Invoice Number APINV321.
You have prepared charges using a mix of Tax ID Posting Groups. Are you sure you want to post these charges?";

				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
					wrapper.Post();
					AssertNotEquals(expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Last message should not be a question.", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				}
			}
		}

		[TestDate(2015, 1, 1, 10, 1, 1)]
		public virtual void TestSayingYesNoToInvalidPostingGroupsForAP()
		{
			var tax1 = Factory.NewWithValidTestData<AccTaxRate>();
			var tax2 = Factory.NewWithValidTestData<AccTaxRate>();
			tax1.AT_Type = AccTaxRate.Types.Rated;
			tax1.AT_PostingGroupId = 1;
			tax1.SetRate_ForTestOnly(0, 1, ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1));
			tax2.AT_Type = AccTaxRate.Types.Rated;
			tax2.AT_PostingGroupId = 2;
			tax2.SetRate_ForTestOnly(0, 1, ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1));
			Factory.Save();

			SetupJobData();

			LocalClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m, 0m); //cleaning up cfx otherwise it will be applied when we change country

			var charge1 = CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			var charge2 = CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			charge1.JR_AT_CostGSTRate = tax1.PK;
			charge2.JR_AT_CostGSTRate = tax2.PK;
			charge1.JR_APInvoiceNum = charge2.JR_APInvoiceNum = "APINV321";
			charge1.JR_APInvoiceDate = charge2.JR_APInvoiceDate = ZDateTime.Today;

			Factory.Save();

			TestPostManagerGUIWrapper wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Job1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			var expectedWarningMessage = @"AP Invoice Number APINV321.
You have prepared charges using a mix of Tax ID Posting Groups. Are you sure you want to post these charges?";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
				wrapper.Post();
				AssertEquals(expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Last message should be a question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
				wrapper.Post();
				ExceptionReporterTestListener.Instance.Clear();
				AssertNotEquals(expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Last message should not be a question.", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		protected virtual void SetupForCreditLimitEmails()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);
			AccountingConfigurationRegistry.Instance.CreditorCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.DebtorCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CreditLimitWarningThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 80);
			TestObjectCreator.Debtor.OH_IsCreditor = true;
			TestObjectCreator.Debtor.CompanyData.SetAPTaxApplicable(false);
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			TestObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.ABIGAS.CompanyData.OB_AROnCreditHold = ZBool.True;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARCreditLimit = 200m;
			TestObjectCreator.ABIGAS.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromShipmentDate;
			TestObjectCreator.ABIGAS.CompanyData.OB_APCreditLimit = 200m;
			TestObjectCreator.Debtor.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.Debtor.CompanyData.OB_AROnCreditHold = ZBool.True;
			TestObjectCreator.Debtor.CompanyData.OB_ARCreditLimit = 200m;
			TestObjectCreator.Debtor.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromShipmentDate;
			TestObjectCreator.Debtor.CompanyData.OB_APCreditLimit = 200m;
			Factory.Save();
		}

		protected virtual void SetupMultipleInvoices(Job job)
		{
			SetupCostAndSellChargeData(job, 500, CC1, 1700, TestObjectCreator.ABIGAS, TestObjectCreator.ABIGAS);
			SetupCostAndSellChargeData(job, 500, CC1, 888, TestObjectCreator.ABIGAS, TestObjectCreator.ABIGAS);
			SetupCostAndSellChargeData(job, 500, CC1, 1700, TestObjectCreator.Debtor, TestObjectCreator.Debtor);
			SetupCostAndSellChargeData(job, 500, CC1, 888, TestObjectCreator.Debtor, TestObjectCreator.Debtor);
		}

		protected ForwardingShipment SetupShipmentWithMultipleInvoices(ZGuid orgPK)
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = orgPK;
			SetupMultipleInvoices(job);
			return shipment;
		}

		const string ReportCodeHeader = "ABC";
		const string ReportCodeLine = "CBA";

		void SetupInvoiceRollup(OrgHeader organisation)
		{
			var invRollup = organisation.CompanyData.InvoiceRollupOrGroups;
			invRollup.RemoveAndDeleteAll();
			var group = invRollup.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();
		}

		Job SetupTestDataForComplianceBook(ZDateTime invDate)
		{
			var localClient = TestObjectCreator.ABIGAS;
			SetupInvoiceRollup(localClient);

			var shipment = TestObjectCreator.CreateShipment("S00001022");
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient != null ? localClient.PK : ZGuid.Empty;

			var tax = TestObjectCreator.GST1.PK;

			var charge = CreateCharge(job, TestObjectCreator.CC1, "Description1", TestObjectCreator.EUR, 0m, TestObjectCreator.AALSHI, TestObjectCreator.EUR, 150m, localClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);
			charge.JR_APInvoiceNum = "INV131";
			charge.JR_APInvoiceDate = invDate;

			charge.JR_AT_SellGSTRate = tax;
			charge.JR_AT_CostGSTRate = tax;

			var charge2 = CreateCharge(job, TestObjectCreator.CC11, "Description2", TestObjectCreator.EUR, 100m, TestObjectCreator.AALSHI, TestObjectCreator.EUR, 0m, localClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);
			charge2.JR_APInvoiceNum = "INV231";
			charge2.JR_APInvoiceDate = invDate;

			charge2.JR_AT_SellGSTRate = tax;
			charge2.JR_AT_CostGSTRate = tax;

			TestObjectCreator.AALSHI.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);

			Factory.Save();

			return job;
		}

		ComplianceSubTypeAttributionRuleConfiguration AddComplianceSubTypeConfig(ComplianceSubTypeAttributionRuleConfigurationCollection collection, ZString country, ZString subType, ZString ledgerType, string invType = "INV")
		{
			var item = collection.AddNew();
			item.Country = country;
			item.SubType = subType;
			item.LedgerType = ledgerType;
			item.InvoiceType = invType;
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			item.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			item.OrganisationLocation = "";
			return item;
		}

		AccComplianceSequence SetupNewComplianceSequence(string subType)
		{
			var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, subType, 1, 100, 25);
			sequence.XD_Prefix = subType;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_Code = subType;
			sequence.XD_Description = "Test sequence " + subType;
			sequence.XD_ExpiryDate = ZDate.Empty;
			sequence.XD_StartDate = ZDate.Empty;
			sequence.XD_MaximumNumberDigits = 4;
			sequence.XD_StartNumber = 1.0;
			sequence.XD_EndNumber = 9999.0;
			sequence.XD_AllocationLevel = "COM";
			sequence.XD_IsActive = true;
			sequence.XD_MaxChargesPerTransaction = 40;
			sequence.XD_SequenceClass = subType;
			return sequence;
		}

		[TestDate(2020, 3, 11)]
		public void TestSparseBookStoredErrorOnSaving_PST()
		{
			AssertSparseBookStoredErrorOnSaving(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2020, 3, 11)]
		public void TestSparseBookStoredErrorOnSaving_INV()
		{
			AssertSparseBookStoredErrorOnSaving(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertSparseBookStoredErrorOnSaving(string dateOption)
		{
			var today = ZDateTime.Today;
			var jobChargeInvoiceDate = today.AddDays(-1);
			var dateOutsideSequencePeriod = new ZDateTime(2019, 10, 15);
			var dateLabel = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? "Invoice Date" : "Post Date";
			var job1 = SetupTestDataForComplianceBook(jobChargeInvoiceDate);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Mexico))
			{
				var collectionSubTypeAttributionRule = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				const string subTypeRec = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI;
				var item = AddComplianceSubTypeConfig(collectionSubTypeAttributionRule, CountryCodes.Mexico, subTypeRec, LedgerTypes.AccountsReceivable);
				item.SelfBillingRule = "STD";
				item.VATGroupRule = "EVG";
				const string subTypePay = MexicoComplianceInfo.ComplianceSubTypeCodes.OTR;
				item = AddComplianceSubTypeConfig(collectionSubTypeAttributionRule, CountryCodes.Mexico, subTypePay, LedgerTypes.AccountsPayable);
				item.SelfBillingRule = "STD";
				item.VATGroupRule = "EVG";

				#region SetupComplianceRuleRegistryMexico();
				var configCollection = new ComplianceReportConfigurationCollection();

				var taxRegistrationCodes = new OrgCodeLists().CustomsCodes_List(RefCountry.LoadFromCountryCode(Factory, CountryCodes.Mexico));
				var taxRegistration = taxRegistrationCodes.ToArray().FirstOrDefault(x => !string.IsNullOrEmpty(x.Code));
				AssertNotNull("There should be a Tax Registration", taxRegistration);

				var headerReportConfig = TestObjectCreator.CreateConfigurationForComplianceReport(configCollection
					, CountryCodes.Mexico
					, ReportCodeHeader
					, "Header Compliance Report"
					, AccTransactionHeaderSchema.Constants.Prefix
					, reportLineGrouping: ""
					, goodsAndService: ""
					, "RNG"
					, taxRegistrationType: taxRegistration.Code
					, ZGuid.Empty
					, false);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(headerReportConfig
					, LedgerTypes.AccountsReceivable
					, TransactionTypes.AdjustmentNote
					, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAnAmountOfTax
					, disbursementRule: DisbursementRuleCodes.AllTransactions
					, originalRule: OriginalRuleCodes.AllTransactions);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(headerReportConfig
					, LedgerTypes.AccountsPayable
					, TransactionTypes.AdjustmentNote
					, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAnAmountOfTax
					, disbursementRule: DisbursementRuleCodes.AllTransactions
					, originalRule: OriginalRuleCodes.AllTransactions);

				var lineReportConfig = TestObjectCreator.CreateConfigurationForComplianceReport(configCollection
					, CountryCodes.Mexico
					, ReportCodeLine
					, "Line Compliance Report"
					, AccTransactionLinesSchema.Constants.Prefix
					, reportLineGrouping: ""
					, goodsAndService: ""
					, "RNG"
					, taxRegistrationType: taxRegistration.Code
					, ZGuid.Empty
					, false);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(lineReportConfig
					, LedgerTypes.AccountsReceivable
					, TransactionTypes.AdjustmentNote
					, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAnAmountOfTax
					, disbursementRule: DisbursementRuleCodes.AllTransactions
					, originalRule: OriginalRuleCodes.AllTransactions);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(lineReportConfig
					, LedgerTypes.AccountsPayable
					, TransactionTypes.AdjustmentNote
					, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAnAmountOfTax
					, disbursementRule: DisbursementRuleCodes.AllTransactions
					, originalRule: OriginalRuleCodes.AllTransactions);
				#endregion

				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();

				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, collectionSubTypeAttributionRule))
				using (registry.ComplianceReportConfiguration.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, configCollection))
				{
					SetupNewComplianceSequence(subTypeRec);
					SetupNewComplianceSequence(subTypePay);

					Factory.Save();

					var tax1 = TestObjectCreator.GST1.PK;
					var client = TestObjectCreator.LocalClient.PK;

					var invoice5 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "test5", TestObjectCreator.EUR, 1, 100, 10, 100, 10);
					invoice5.AH_ComplianceSubType = subTypeRec;
					invoice5.AH_TransactionReference = "00007";
					SetInvoiceDate(invoice5, dateOption, new ZDateTime(2020, 3, 5), dateOutsideSequencePeriod);
					invoice5.Lines[0].AL_AT = tax1;
					invoice5.AH_OH = client;

					var invoice6 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "test6", TestObjectCreator.EUR, 1, 100, 10, 100, 10);
					invoice6.AH_ComplianceSubType = subTypeRec;
					invoice6.AH_TransactionReference = "";
					SetInvoiceDate(invoice6, dateOption, new ZDateTime(2020, 2, 15), dateOutsideSequencePeriod);
					invoice6.Lines[0].AL_AT = tax1;
					invoice6.AH_OH = client;

					var invoice7 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "test7", TestObjectCreator.EUR, 1, 100, 10, 100, 10);
					invoice7.AH_ComplianceSubType = subTypePay;
					invoice7.AH_TransactionReference = "00021";
					SetInvoiceDate(invoice7, dateOption, new ZDateTime(2020, 2, 25), dateOutsideSequencePeriod);
					invoice7.Lines[0].AL_AT = tax1;
					invoice7.AH_OH = client;

					var invoice8 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "test8", TestObjectCreator.EUR, 1, 100, 10, 100, 10);
					invoice8.AH_ComplianceSubType = subTypePay;
					invoice8.AH_TransactionReference = "";
					SetInvoiceDate(invoice8, dateOption, new ZDateTime(2020, 2, 15), dateOutsideSequencePeriod);
					invoice8.Lines[0].AL_AT = tax1;
					invoice8.AH_OH = client;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
					using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
					{
						var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.LocalClient, Factory, job1);
						using (Form form = new Form())
						{
							wrapper.ParentForm = form;
							wrapper.Post();

							AssertEquals($@"Compliance Numbers cannot be allocated.
 There is some transaction with the same Compliance Sub Type {subTypeRec} in earlier {dateLabel} and Compliance Number empty.
 Please allocate Compliance Number to all transactions with {dateLabel} < {today.ToShortDateString()}.", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}

					if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
					{
						using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
						using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
						{
							var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job1);
							using (Form form = new Form())
							{
								wrapper.ParentForm = form;
								wrapper.Post();
								var date = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? jobChargeInvoiceDate : today;
								AssertEquals($@"Compliance Numbers cannot be allocated.
 There is some transaction with the same Compliance Sub Type {subTypePay} in earlier {dateLabel} and Compliance Number empty.
 Please allocate Compliance Number to all transactions with {dateLabel} < {date.ToShortDateString()}.", UnitTestUserNotification.Instance.LastMessage.Text);
								Assert("Should not create any invoices", !wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
							}
						}
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
					using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
					{
						var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.LocalClient, Factory, job1);
						using (Form form = new Form())
						{
							wrapper.ParentForm = form;
							wrapper.Post();

							Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
							Assert("Should create invoices", wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
						}
					}

					if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
					{
						using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
						using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
						{
							var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job1);
							using (Form form = new Form())
							{
								wrapper.ParentForm = form;
								wrapper.Post();

								Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
							}
						}
					}
				}
			}
		}

		void SetInvoiceDate(TransactionHeader invoice, string complianceNumberAllocationDateOption, ZDateTime allocationDate, ZDateTime otherDate)
		{
			if (complianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice.AH_PostDate = otherDate;
				invoice.AH_InvoiceDate = allocationDate;
			}
			else
			{
				invoice.AH_PostDate = allocationDate;
				invoice.AH_InvoiceDate = otherDate;
			}
		}

		[TestDate(2020, 3, 11)]
		public void TestPostDateEarlierThanLastDateUsedErrorOnSaving_PST()
		{
			AssertPostDateEarlierThanLastDateUsedErrorOnSaving(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2020, 3, 11)]
		public void TestPostDateEarlierThanLastDateUsedErrorOnSaving_INV()
		{
			AssertPostDateEarlierThanLastDateUsedErrorOnSaving(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertPostDateEarlierThanLastDateUsedErrorOnSaving(string dateOption)
		{
			var today = ZDateTime.Today;
			var job1 = SetupTestDataForComplianceBook(today);
			var dateLabel = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? "Invoice" : "Post";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var collectionSubTypeAttributionRule = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				const string subTypeRec = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				var item = AddComplianceSubTypeConfig(collectionSubTypeAttributionRule, CountryCodes.Italy, subTypeRec, LedgerTypes.AccountsReceivable);
				item.SelfBillingRule = "STD";
				item.VATGroupRule = "EVG";
				const string subTypePay = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
				item = AddComplianceSubTypeConfig(collectionSubTypeAttributionRule, CountryCodes.Italy, subTypePay, LedgerTypes.AccountsPayable);
				item.SelfBillingRule = "STD";
				item.VATGroupRule = "EVG";

				Factory.Save();

				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();

				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, collectionSubTypeAttributionRule))
				{
					var tomorrow = today.AddDays(1).Date;
					var complSeq = SetupNewComplianceSequence(subTypeRec);
					var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
					invoice1.AH_PostDate = invoice1.AH_InvoiceDate = tomorrow;
					invoice1.AH_XD_ComplianceBook = complSeq.PK;

					complSeq = SetupNewComplianceSequence(subTypePay);
					var invoice2 = Factory.NewWithValidTestData<APInvoice>();
					invoice2.AH_PostDate = invoice2.AH_InvoiceDate = tomorrow;
					invoice2.AH_XD_ComplianceBook = complSeq.PK;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
					using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
					{
						var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.LocalClient, Factory, job1);
						using (Form form = new Form())
						{
							wrapper.ParentForm = form;
							wrapper.Post();

							AssertEquals($@"Compliance Numbers cannot be allocated.
 Last posted transaction with the same Compliance Sub Type {subTypeRec} has {dateLabel} Date = {tomorrow.ToShortDateString()}, that is greater than the current one(s).", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}

					if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
					{
						using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
						using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
						{
							var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job1);
							using (Form form = new Form())
							{
								wrapper.ParentForm = form;
								wrapper.Post();

								AssertEquals($@"Compliance Numbers cannot be allocated.
 Last posted transaction with the same Compliance Sub Type {subTypePay} has {dateLabel} Date = {tomorrow.ToShortDateString()}, that is greater than the current one(s).", UnitTestUserNotification.Instance.LastMessage.Text);
								Assert("Should not create any invoices", !wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
							}
						}
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
					using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
					{
						var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.LocalClient, Factory, job1);
						using (Form form = new Form())
						{
							wrapper.ParentForm = form;
							wrapper.Post();

							Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
							Assert("Should create invoices", wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
						}
					}

					if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
					{
						using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
						using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
						{
							var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job1);
							using (Form form = new Form())
							{
								wrapper.ParentForm = form;
								wrapper.Post();

								Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
							}
						}
					}
				}
			}
		}

		[TestDate(2020, 3, 11)]
		public void TestComplianceFullOrExpiredErrorOnSaving_PST()
		{
			AssertComplianceFullOrExpiredErrorOnSaving(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2020, 3, 11)]
		public void TestComplianceFullOrExpiredErrorOnSaving_INV()
		{
			AssertComplianceFullOrExpiredErrorOnSaving(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertComplianceFullOrExpiredErrorOnSaving(string dateOption)
		{
			var today = ZDateTime.Today;
			var job1 = SetupTestDataForComplianceBook(today);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var collectionSubTypeAttributionRule = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				const string subTypeRec = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				var item = AddComplianceSubTypeConfig(collectionSubTypeAttributionRule, CountryCodes.Italy, subTypeRec, LedgerTypes.AccountsReceivable);
				item.SelfBillingRule = "STD";
				item.VATGroupRule = "EVG";
				const string subTypePay = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
				item = AddComplianceSubTypeConfig(collectionSubTypeAttributionRule, CountryCodes.Italy, subTypePay, LedgerTypes.AccountsPayable);
				item.SelfBillingRule = "STD";
				item.VATGroupRule = "EVG";

				Factory.Save();

				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();

				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, collectionSubTypeAttributionRule))
				{
					var complSeq = SetupNewComplianceSequence(subTypeRec);
					complSeq.XD_NextNumber = 10000.0;

					complSeq = SetupNewComplianceSequence(subTypePay);
					complSeq.XD_StartDate = today.AddDays(1).Date;
					complSeq = SetupNewComplianceSequence(subTypePay);
					complSeq.XD_ExpiryDate = today.AddDays(-1).Date;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
					using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
					{
						var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.LocalClient, Factory, job1);
						using (Form form = new Form())
						{
							wrapper.ParentForm = form;
							wrapper.Post();

							AssertEquals(@"Please check your Compliance Book Setups.
 Compliance Numbers could not be allocated.
 All suitable Compliance Books for the relevant criteria are fully used or expired (e.g. Sub Type, Allocation Level, Branch, Active Status, Start Date, Expiry Date etc.", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}

					if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
					{
						using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
						using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
						{
							var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job1);
							using (Form form = new Form())
							{
								wrapper.ParentForm = form;
								wrapper.Post();

								AssertEquals(@"Please check your Compliance Invoice Book Setups. 
 A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist.", UnitTestUserNotification.Instance.LastMessage.Text);
								Assert("Should not create any invoices", !wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
							}
						}
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
					using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
					{
						var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.LocalClient, Factory, job1);
						using (Form form = new Form())
						{
							wrapper.ParentForm = form;
							wrapper.Post();

							Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
							Assert("Should create invoices", wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
						}
					}

					if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
					{
						using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
						using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
						{
							var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job1);
							using (Form form = new Form())
							{
								wrapper.ParentForm = form;
								wrapper.Post();

								Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
							}
						}
					}
				}
			}
		}

		#region NothingPostedHandler

		public void TestNothingPostedHandler_WhenAllowZeroValueARInvoices()
		{
			AssertNothingPostedHandler(true);
		}

		public void TestNothingPostedHandler_WhenDisallowZeroValueARInvoices()
		{
			AssertNothingPostedHandler(false);
		}

		void AssertNothingPostedHandler(bool allowZeroValueARInvoices)
		{
			var wrapper = PrepareTestDataForNothingPostedHandler();
			using (AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowZeroValueARInvoices))
			{
				AssertNothingPostedHandlerCore(wrapper);
			}
		}

		protected abstract PostManagerGUIWrapper PrepareTestDataForNothingPostedHandler();

		protected abstract void AssertNothingPostedHandlerCore(PostManagerGUIWrapper wrapper);

		protected virtual string GetExpectedMessageForNothingPosted()
		{
			var expectedZeroValueMessage = AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value
				? "* Amounts are zero"
				: "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices";

			return $@"No appropriate charges were found for posting, due to one of the following reasons:
{expectedZeroValueMessage}
* Posting Costs: Creditor, AP Invoice Number or Date are not entered or invalid
* Posting Revenue: Job status is set to 'Invoice on hold' / 'Work on hold' or Debtor is not entered or invalid
* Job has Ready For Financial Closure status";
		}

		#endregion

		#region TestPrintInvoicesFiltersNullTransactionsAndRaisesDeveloperException

		public void TestPrintInvoicesFiltersNullTransactionsAndRaisesDeveloperException()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var shipment1 = creator.CreateShipment("S1");
			var job1 = creator.CreateJob(shipment1, false);
			job1.LocalChargesPK = creator.ABIGAS.PK;
			var charge1 = creator.CreateCharge(job1, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_APInvoiceNum = "INV001";
			charge1.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);

			var arInvoice = creator.CreateARInvoice<ARInvoice>("1000", creator.AUD, 1m, creator.ABIGAS);
			var apInvoice = creator.CreateAPInvoice<APInvoice>("2000", creator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			var apCreditNote = creator.CreateAPCreditNote("3000", creator.ABIGAS, creator.AUD, 1m, "Credit note");
			factory.Save();
			AssertEquals("Precondition: no exceptions", 0, ExceptionReporterTestListener.Instance.Count);

			var transactionHash = new TransactionCreatorHashtable();
			var wrapper = new PostManagerGUIWrapper_AlterPrintInvoiceWithNull(JobInvoicingPostingOption.All, factory, job: job1);
			wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Add(arInvoice);
			wrapper.SetNullForArInvoice = true;
			ExceptionReporterTestListener.Instance.Clear();
			var transactionsForPrinting = wrapper.GetTransactionsForPrinting_ForTestOnly(transactionHash);
			AssertCollectionNotContains("All null transactions for printing should be removed from list of transactions", null, transactionsForPrinting);
			AssertEquals(1, transactionsForPrinting.Length);
			AssertEquals("Developer Error should be reported", ExceptionReporterTestListener.Instance.Count, 1);
			AssertEquals("Developer Error message should be reported", "Null transactions to be printed was found in PostManagerGUIWrapper 'arInvoices' source collection. You cannot print a null transaction. The null transaction has been filtered from list of transactions to be printed. PostManagerGUIWrapper type: PostManagerGUIWrapper_AlterPrintInvoiceWithNull. Other transactions: AR INV 00001000 (ABIGAS). Job type: ForwardingShipment (S1).", ExceptionReporterTestListener.Instance[0].Message);
			wrapper.SetNullForArInvoice = false;
			wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.RemoveAll();
			transactionHash.Clear();

			transactionHash.AddAPInvoice(apInvoice, "ABIGAS", "12345");
			transactionHash.AddAPCreditNote(apCreditNote, "ABIGAS", "54321");
			wrapper.SetNullForApInvoicesAndCreditNotes = true;
			ExceptionReporterTestListener.Instance.Clear();
			transactionsForPrinting = wrapper.GetTransactionsForPrinting_ForTestOnly(transactionHash);
			AssertCollectionNotContains("All null transactions for printing should be removed from list of transactions", null, transactionsForPrinting);
			AssertEquals(2, transactionsForPrinting.Length);
			AssertEquals("Developer Error should be reported", ExceptionReporterTestListener.Instance.Count, 1);
			AssertEquals("Developer Exception should be reported", "Null transactions to be printed was found in PostManagerGUIWrapper 'apInvoicesAndCreditNotes' source collection. You cannot print a null transaction. The null transaction has been filtered from list of transactions to be printed. PostManagerGUIWrapper type: PostManagerGUIWrapper_AlterPrintInvoiceWithNull. Other transactions: AP INV 2000 (), AP CRD 3000 (ABIGAS). Job type: ForwardingShipment (S1).", ExceptionReporterTestListener.Instance[0].Message);
			wrapper.SetNullForApInvoicesAndCreditNotes = false;
			transactionHash.Clear();

			wrapper.SetNullForApPayments = true;
			ExceptionReporterTestListener.Instance.Clear();
			transactionsForPrinting = wrapper.GetTransactionsForPrinting_ForTestOnly(transactionHash);
			AssertCollectionNotContains("All null transactions for printing should be removed from list of transactions", null, transactionsForPrinting);
			AssertEquals(0, transactionsForPrinting.Length);
			AssertEquals("Developer Error should be reported", ExceptionReporterTestListener.Instance.Count, 1);
			AssertEquals("Developer Exception should be reported", "Null transactions to be printed was found in PostManagerGUIWrapper 'apPayments' source collection. You cannot print a null transaction. The null transaction has been filtered from list of transactions to be printed. PostManagerGUIWrapper type: PostManagerGUIWrapper_AlterPrintInvoiceWithNull. Other transactions: <none>. Job type: ForwardingShipment (S1).", ExceptionReporterTestListener.Instance[0].Message);
			wrapper.SetNullForApPayments = false;

			wrapper.SetNullForApApprovalRequests = true;
			ExceptionReporterTestListener.Instance.Clear();
			transactionsForPrinting = wrapper.GetTransactionsForPrinting_ForTestOnly(transactionHash);
			AssertCollectionNotContains("All null transactions for printing should be removed from list of transactions", null, transactionsForPrinting);
			AssertEquals(0, transactionsForPrinting.Length);
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Developer Error should be reported", ExceptionReporterTestListener.Instance.Count, 1);
			AssertEquals("Developer Exception should be reported", "Null transactions to be printed was found in PostManagerGUIWrapper 'apApprovalRequests' source collection. You cannot print a null transaction. The null transaction has been filtered from list of transactions to be printed. PostManagerGUIWrapper type: PostManagerGUIWrapper_AlterPrintInvoiceWithNull. Other transactions: <none>. Job type: ForwardingShipment (S1).", ExceptionReporterTestListener.Instance[0].Message);
			wrapper.SetNullForApApprovalRequests = false;
			transactionHash.Clear();

			wrapper = new PostManagerGUIWrapper_AlterPrintInvoiceWithNull(JobInvoicingPostingOption.All, factory);
			transactionHash.AddAPInvoice(apInvoice, "ABIGAS", "12345");
			transactionHash.AddAPCreditNote(apCreditNote, "ABIGAS", "54321");
			wrapper.SetNullForApInvoicesAndCreditNotes = true;
			ExceptionReporterTestListener.Instance.Clear();
			transactionsForPrinting = wrapper.GetTransactionsForPrinting_ForTestOnly(transactionHash);
			AssertCollectionNotContains("All null transactions for printing should be removed from list of transactions", null, transactionsForPrinting);
			AssertEquals(2, transactionsForPrinting.Length);
			AssertEquals("Developer Error should be reported", ExceptionReporterTestListener.Instance.Count, 1);
			AssertEquals("Developer Exception should be reported; null job should be indicated as such", "Null transactions to be printed was found in PostManagerGUIWrapper 'apInvoicesAndCreditNotes' source collection. You cannot print a null transaction. The null transaction has been filtered from list of transactions to be printed. PostManagerGUIWrapper type: PostManagerGUIWrapper_AlterPrintInvoiceWithNull. Other transactions: AP INV 2000 (), AP CRD 3000 (ABIGAS). Job type: <null>.", ExceptionReporterTestListener.Instance[0].Message);
			wrapper.SetNullForApInvoicesAndCreditNotes = false;
			transactionHash.Clear();

			ExceptionReporterTestListener.Instance.Clear();
		}

		public class PostManagerGUIWrapper_AlterPrintInvoiceWithNull : PostManagerGUIWrapper
		{
			public PostManagerGUIWrapper_AlterPrintInvoiceWithNull(JobInvoicingPostingOption postingOption, BusinessObjectFactory factory, Job job = null)
				: base(job?.Parent, postingOption, factory, null, null)
			{
				Jobs = job == null ? Enumerable.Empty<Job>() : LoadJobsInNewFactory(job);
			}

			public bool SetNullForArInvoice;
			public bool SetNullForApInvoicesAndCreditNotes;
			public bool SetNullForApPayments;
			public bool SetNullForApApprovalRequests;

			protected override (IEnumerable<InvoicingBase> arInvoices, InvoicingBase[] apInvoicesAndCreditNotes, APPayment[] apPayments, APInvoice[] apApprovalRequests)
				AlterTransactionsForPrinting_ForTestOnly(IEnumerable<InvoicingBase> arInvoices, InvoicingBase[] apInvoicesAndCreditNotes, APPayment[] apPayments, APInvoice[] apApprovalRequests)
			{
				if (SetNullForArInvoice)
				{
					arInvoices = arInvoices.Concat(new InvoicingBase[] { null });
				}
				if (SetNullForApInvoicesAndCreditNotes)
				{
					apInvoicesAndCreditNotes = apInvoicesAndCreditNotes.Concat(new InvoicingBase[] { null }).ToArray();
				}
				if (SetNullForApPayments)
				{
					apPayments = apPayments.Concat(new APPayment[] { null }).ToArray();
				}
				if (SetNullForApApprovalRequests)
				{
					apApprovalRequests = apApprovalRequests.Concat(new APInvoice[] { null }).ToArray();
				}
				return (arInvoices, apInvoicesAndCreditNotes, apPayments, apApprovalRequests);
			}

			protected override string GetJobOnHoldMessage(IEnumerable<Job> jobs)
			{
				return "";
			}

			protected override string GetNothingPostedMessage()
			{
				return "";
			}

			protected override BasePostManager GetNewPostManager()
			{
				return new InvoicingPostManager(Jobs.FirstOrDefault());
			}
		}

		#endregion

		#region After Creation, Pre Post Validation

		[TestDate(2021, 09, 23)]
		public void TestPostAPWithZeroExchangeRate()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 0m, null, 0m);

			var chargeA = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeA",
				TestObjectCreator.USD, 200M, TestObjectCreator.AALSHI, "INV001");
			chargeA.JR_OSCostExRate = 0.000111;
			chargeA.JR_LocalCostAmt = 1801801.80;

			var chargeB = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "chargeB",
				TestObjectCreator.USD, -200M, TestObjectCreator.AALSHI, "INV001");
			chargeB.JR_OSCostExRate = 0.000111;
			chargeB.JR_LocalCostAmt = -1801801.81;
			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job);
			using (var form = new Form())
			{
				wrapper.ParentForm = form;
				wrapper.Post();

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"Invoice was created with zero exchange rate. Exchange rate must be greater than 0.
Invoice currency: USD, debtor / creditor: AALSHI, ledger: AP, post date: 23-Sep-21 00:00:00, invoice date 23-Sep-21 00:00:00."));
			}
		}

		public void TestAfterCreationBeforePostActionsObjectType()
		{
			var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
			AssertType<PostManagerCreatedTransactionActions>(obj);

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 0m, null, 0m);
			var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job);
			AssertType<PostManagerCreatedTransactionActions>(wrapper.AfterCreationBeforePostTransactionActions);
		}

		public void TestAfterCreationBeforePostActionsAreCalled()
		{
			SetupJobData();
			SetupChargeData(-150m, TestObjectCreator.CC1);

			var mockValidator = new Mock<IPostManagerCreatedTransactionActions>();
			mockValidator.Setup(x => x.PerformInvoiceExchangeRateCheck(It.IsNotNull<TransactionCreatorHashtable>(), It.IsNotNull<IPostManagerUserNotifier>())).Returns(true);
			mockValidator.Setup(x => x.PerformSurchargeLineDeptCheck(It.IsNotNull<TransactionCreatorHashtable>(), It.IsNotNull<IPostManagerUserNotifier>())).Returns(true);
			mockValidator.Setup(x => x.PerformCashAdvanceRelatedCheck(It.IsNotNull<TransactionCreatorHashtable>(), It.IsNotNull<IPostManagerUserNotifier>())).Returns(true);
			mockValidator.Setup(x => x.PerformInvPostDateCheck(It.IsNotNull<TransactionCreatorHashtable>(), It.IsNotNull<IPostManagerUserNotifier>())).Returns(true);
			mockValidator.Setup(x => x.PerformInvTaxDateCheck(It.IsNotNull<TransactionCreatorHashtable>(), It.IsNotNull<IPostManagerUserNotifier>())).Returns(true);
			mockValidator.Setup(x => x.PerformInvComplianceCheck(It.IsNotNull<TransactionCreatorHashtable>(), It.IsNotNull<IPostManagerUserNotifier>())).Returns(true);
			mockValidator.Setup(x => x.PerformARCreditNoteLevelAuthorization(It.IsNotNull<BasePostManager>(), It.IsNotNull<IPostingJobTransactionsApprovalGUIProvider>())).Returns(true);
			using (ObjectFactory.Substitute(mockValidator.Object))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (var form = new Form())
				{
					var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1);
					wrapper.ParentForm = form;
					wrapper.Post();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					mockValidator.VerifyAll();
					Assert("Should create an invoice", wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Any());
				}
			}
		}

		#endregion

		#region InvoicePostingExchangeRateOption
		//sub class should check/implement the tests
		public abstract void TestPostWithInvoicePostingExchangeRateOption();
		public abstract void TestBackDatingARAPInvoiceWithInvoicePostingExchangeRateOption();

		protected void SetupForInvoicePostingExchangeRateOption()
		{
			AssertEquals("precondition", new ZDateTime(2015, 5, 10).Date, ZDateTime.Now.Date);

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");

			TestObjectCreator.CreateUSDBuyRate(5.01m, new DateTime(2015, 5, 1));
			TestObjectCreator.CreateUSDBuyRate(5.02m, new DateTime(2015, 5, 2));
			TestObjectCreator.CreateUSDBuyRate(5.03m, new DateTime(2015, 5, 3));
			TestObjectCreator.CreateUSDBuyRate(5.10m, new DateTime(2015, 5, 10));

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
		}

		protected void SetupForBackDatingWithInvoicePostingExchangeRateOption()
		{
			AssertEquals("precondition", new ZDateTime(2015, 5, 1).Date, ZDateTime.Now.Date);

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			TestObjectCreator.CreateUSDBuyRate(5.01m, new DateTime(2015, 5, 1));
			TestObjectCreator.CreateUSDBuyRate(4.30m, new DateTime(2015, 4, 30));

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			//AR back dating
			var config = BackDateInvoicesConfiguration_BackDateInvoicesTrue;
			config.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			//AP backdating
			var regBackDateAPConfig = new BackDateAPInvoicesConfiguration();
			var regBackDateAPConfig1 = regBackDateAPConfig.PostDateConfigurationCollection[0];
			regBackDateAPConfig1.JobType = "ALL";
			regBackDateAPConfig1.DirectionCode = "";
			regBackDateAPConfig1.Mode = "";
			regBackDateAPConfig1.BrokerCode = "";
			regBackDateAPConfig1.SignificantDateCode = "ADD";
			regBackDateAPConfig1.PriorClosedPeriod = "";
			regBackDateAPConfig1.PriorOpenPeriod = "";
			regBackDateAPConfig1.CurrentPeriod = "EPM";
			regBackDateAPConfig1.FuturePeriod = "";
			regBackDateAPConfig1.ReversalRule = "STD";
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regBackDateAPConfig);
		}

		protected ZGuid CreateShipmentForInvoicePostingExchangeRateOption()
		{
			AssertEquals("precondition", new ZDateTime(2015, 5, 10).Date, ZDateTime.Now.Date);

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

			var chargeA = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeA",
				TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "chargeA",
				TestObjectCreator.USD, 100M, TestObjectCreator.ABIGAS);

			var chargeB = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeB",
				TestObjectCreator.USD, 200M, TestObjectCreator.AALSHI, "chargeB",
				TestObjectCreator.USD, 200M, TestObjectCreator.ABIGAS);

			var chargeC = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeB",
				TestObjectCreator.USD, 300M, TestObjectCreator.AALSHI, "chargeB",
				TestObjectCreator.USD, 300M, TestObjectCreator.ABIGAS);

			foreach (Charge charge in job.Charges)
			{
				charge.JR_APInvoiceNum = TestObjectCreator.GetRandomString(8);
				charge.JR_InvoiceType = "CUR";

				AssertEquals("USD", charge.JR_CostCurrency);
				AssertEquals(5.10m, charge.JR_OSCostExRate); //today's rate

				AssertEquals("USD", charge.JR_RX_NKSellCurrency);
				AssertEquals(5.10m, charge.JR_OSSellExRate); //today's rate
			}

			chargeA.JR_APInvoiceDate = new ZDateTime(2015, 5, 2);
			chargeB.JR_APInvoiceDate = new ZDateTime(2015, 5, 3);
			chargeC.JR_APInvoiceDate = new ZDateTime(2015, 5, 1);

			return shipment.PK;
		}

		protected void AssertShipmentAndAPInvoiceForInvoicePostingExchangeRateOption(ZGuid shipmentPK)
		{
			AssertEquals("precondition", new ZDateTime(2015, 5, 10).Date, ZDateTime.Now.Date);

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.Load<ForwardingShipment>(shipmentPK);

			var job = new Job.Loader(shipment).Load();
			AssertEquals(3, job.Charges.Count);

			var chargeA = job.Charges[0];
			var chargeB = job.Charges[1];
			var chargeC = job.Charges[2];

			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, job.Charges.Cast<Charge>().Select(x => x.JR_APInvoiceNum));
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			var invoices = newFactory.Load<InvoicingBase>(query);

			AssertEquals(3, invoices.Length);

			//charges/lines should be updated to the invoice date's rate respectively
			AssertEquals("USD", chargeA.JR_RX_NKCostCurrency);
			AssertEquals(5.02m, chargeA.JR_OSCostExRate);
			AssertEquals(100m, chargeA.JR_OSCostAmt);
			AssertEquals(19.92m, chargeA.JR_LocalCostAmt);

			AssertEquals("USD", chargeB.JR_RX_NKCostCurrency);
			AssertEquals(5.03m, chargeB.JR_OSCostExRate);
			AssertEquals(200m, chargeB.JR_OSCostAmt);
			AssertEquals(39.76m, chargeB.JR_LocalCostAmt);

			AssertEquals("USD", chargeC.JR_RX_NKCostCurrency);
			AssertEquals(5.01m, chargeC.JR_OSCostExRate);
			AssertEquals(300m, chargeC.JR_OSCostAmt);
			AssertEquals(59.88m, chargeC.JR_LocalCostAmt);

			//lines should be updated
			foreach (Charge charge in job.Charges)
			{
				var invoice = invoices.FirstOrDefault(x => x.AH_TransactionNum == charge.JR_APInvoiceNum);
				AssertEquals(charge.JR_RX_NKCostCurrency, invoice.AH_RX_NKTransactionCurrency);
				var expectedRefreshedExRate = invoice.Company.GetExchangeRate().GetRate(charge.JR_Calc_LocalCostAmtWithGST, charge.JR_OSCostAmtWithGSTAmt);
				AssertEquals("AH_PostedToEFT", true, invoice.AH_PostedToEFT);
				AssertEquals($"Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), {charge.JR_OSCostAmtWithGSTAmt} / {charge.JR_Calc_LocalCostAmtWithGST}."
					, expectedRefreshedExRate
					, invoice.AH_ExchangeRate
				);

				AssertEquals(1, invoice.Lines.Count);
				var line = invoice.Lines[0];
				AssertEquals(charge.JR_RX_NKCostCurrency, line.AL_RX_NKTransactionCurrency);
				AssertEquals(charge.JR_OSCostExRate, line.AL_ExchangeRate.Round(2));
				AssertEquals(charge.JR_OSCostAmt, line.AL_OSExTaxAmount);
				AssertEquals(charge.JR_LocalCostAmt, line.AL_LocalExTaxAmount);
			}

			//the job should be updated to the latest invoice date's rate
			var jobsNewRate = job.GetExchangeRate("USD", chargeA.JR_OH_CostAccount, ExchangeRateOrgTypeEnum.Creditor, ExchangeRateType.Buy);
			AssertEquals("should be the latest rate", 5.03m, jobsNewRate);

			// Sell Rate was not updated as we used only Ex Rate for Creditor
			foreach (Charge charge in job.Charges)
			{
				AssertEquals("USD", charge.JR_RX_NKSellCurrency);
				AssertEquals(5.10m, charge.JR_OSSellExRate);
			}
			AssertEquals(100m, chargeA.JR_OSSellAmt);
			AssertEquals(19.61m, chargeA.JR_LocalSellAmt);
			AssertEquals(200m, chargeB.JR_OSSellAmt);
			AssertEquals(39.22m, chargeB.JR_LocalSellAmt);
			AssertEquals(300m, chargeC.JR_OSSellAmt);
			AssertEquals(58.82m, chargeC.JR_LocalSellAmt);
		}

		protected void AssertShipmentAndARInvoiceForInvoicePostingExchangeRateOption(ZGuid shipmentPK)
		{
			AssertEquals("precondition", new ZDateTime(2015, 5, 10).Date, ZDateTime.Now.Date);

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.Load<ForwardingShipment>(shipmentPK);

			var job = new Job.Loader(shipment).Load();
			var arQuery = new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK);
			arQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, "AR");
			arQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			var arInvoice = newFactory.LoadTop1<ARInvoice>(arQuery);

			AssertEquals(3, arInvoice.Lines.Count);
			AssertEquals((new ZDateTime(2015, 5, 10)).Date, arInvoice.AH_InvoiceDate.Date);

			AssertEquals(3, job.Charges.Count);

			var chargeA = job.Charges[0];
			var chargeB = job.Charges[1];
			var chargeC = job.Charges[2];

			foreach (Charge charge in job.Charges)
			{
				AssertEquals("USD", charge.JR_RX_NKSellCurrency);
				AssertEquals(5.10m, charge.JR_OSSellExRate); //invoice date's rate
			}
			AssertEquals(100m, chargeA.JR_OSSellAmt);
			AssertEquals(19.61m, chargeA.JR_LocalSellAmt);
			AssertEquals(200m, chargeB.JR_OSSellAmt);
			AssertEquals(39.22m, chargeB.JR_LocalSellAmt);
			AssertEquals(300m, chargeC.JR_OSSellAmt);
			AssertEquals(58.82m, chargeC.JR_LocalSellAmt);

			AssertEquals("USD", arInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals(5.10m, arInvoice.AH_ExchangeRate);

			//lines should be updated
			foreach (Charge charge in job.Charges)
			{
				var line = arInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.PK == charge.JR_AL_ARLine);
				AssertEquals(charge.JR_RX_NKSellCurrency, line.AL_RX_NKTransactionCurrency);
				AssertEquals(charge.JR_OSSellExRate, line.AL_ExchangeRate.Round(2));
				AssertEquals(charge.JR_OSSellAmt, line.AL_OSAmount);
				AssertEquals(charge.JR_LocalSellAmt, line.AL_LineAmount);
			}

			//the job should be updated to the latest invoice date's rate
			var jobsNewRate = job.GetExchangeRate("USD", chargeA.JR_OH_SellAccount, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateType.Buy);
			AssertEquals("should be the latest rate", 5.10m, jobsNewRate);
		}

		protected ZGuid CreateConsolForInvoicePostingExchangeRateOption()
		{
			AssertEquals("precondition", new ZDateTime(2015, 5, 10).Date, ZDateTime.Now.Date);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = TestObjectCreator.GetRandomString(9);
			var apportionments = new ApportionmentListing(Factory, consol);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = TestObjectCreator.GetRandomString(9);
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = TestObjectCreator.GetRandomString(9);

			apportionments.IsActivated = true;
			apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();

			var cC1Cost = apportionments.CostsCollection.TryAddNew();
			cC1Cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cC1Cost.E6_RX_NKCurrency = "USD";
			cC1Cost.E6_ExchangeRate = 6.1m;
			cC1Cost.E6_OSCostAmount = 100m;
			cC1Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cC1Cost.E6_InvoiceNum = TestObjectCreator.GetRandomString(8);
			cC1Cost.E6_InvoiceDate = new ZDateTime(2015, 5, 2);
			cC1Cost.E6_ApportionmentMethod = "SHP";

			var cC2Cost = apportionments.CostsCollection.TryAddNew();
			cC2Cost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			cC2Cost.E6_RX_NKCurrency = "USD";
			cC2Cost.E6_ExchangeRate = 6.2m;
			cC2Cost.E6_OSCostAmount = 200m;
			cC2Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cC2Cost.E6_InvoiceNum = TestObjectCreator.GetRandomString(8);
			cC2Cost.E6_InvoiceDate = new ZDateTime(2015, 5, 3);
			cC2Cost.E6_ApportionmentMethod = "SHP";

			var cC3Cost = apportionments.CostsCollection.TryAddNew();
			cC3Cost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			cC3Cost.E6_RX_NKCurrency = "USD";
			cC3Cost.E6_ExchangeRate = 6.3m;
			cC3Cost.E6_OSCostAmount = 300m;
			cC3Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cC3Cost.E6_InvoiceNum = TestObjectCreator.GetRandomString(8);
			cC3Cost.E6_InvoiceDate = new ZDateTime(2015, 5, 1);
			cC3Cost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			var jobs = new JobCollection(Factory);
			ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment1.PK);
			jobsQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentID, shipment2.PK);
			jobs.Load(jobsQuery);

			foreach (Job job in jobs)
			{
				job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			}

			return consol.PK;
		}

		protected void AssertConsolAndAPInvoiceForInvoicePostingExchangeRateOption(ZGuid consolPK)
		{
			AssertEquals("precondition", new ZDateTime(2015, 5, 10).Date, ZDateTime.Now.Date);

			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.Load<ForwardingConsol>(consolPK);
			var consolCosts = consol.GetApportionments().CostsCollection;

			var jobs = new JobCollection(newFactory);
			jobs.Load(new ZQuery(JobHeaderSchema.JH_ParentID, consol.Shipments.Select(x => x.PK)));

			AssertEquals(2, jobs.Count);
			AssertEquals(3, consolCosts.Count);

			AssertEquals(5.02m, consolCosts[0].E6_ExchangeRate);
			AssertEquals(100m, consolCosts[0].E6_OSCostAmount);
			AssertEquals(19.92m, consolCosts[0].E6_LocalCostAmount);

			AssertEquals(5.03m, consolCosts[1].E6_ExchangeRate);
			AssertEquals(200m, consolCosts[1].E6_OSCostAmount);
			AssertEquals(39.76m, consolCosts[1].E6_LocalCostAmount);

			AssertEquals(5.01m, consolCosts[2].E6_ExchangeRate);
			AssertEquals(300m, consolCosts[2].E6_OSCostAmount);
			AssertEquals(59.88m, consolCosts[2].E6_LocalCostAmount);

			foreach (Job job in jobs)
			{
				AssertEquals(3, job.Charges.Count);

				foreach (JobConsolCost consolCost in consolCosts)
				{
					var charge = job.Charges.Cast<Charge>().FirstOrDefault(x => x.ParentConsolCost.PK == consolCost.PK);

					AssertEquals(consolCost.E6_RX_NKCurrency, charge.JR_OSCostCurrencyCode);
					AssertEquals(consolCost.E6_ExchangeRate, charge.JR_OSCostExRate);

					AssertEquals(consolCost.E6_OSCostAmount / 2, charge.JR_OSCostAmt);
					AssertEquals(consolCost.E6_LocalCostAmount / 2, charge.JR_LocalCostAmt);
				}
			}

			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, consolCosts.Cast<JobConsolCost>().Select(x => x.E6_InvoiceNum));
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			var invoices = newFactory.Load<InvoicingBase>(query);

			AssertEquals(3, invoices.Length);

			foreach (JobConsolCost consolCost in consolCosts)
			{
				var invoice = invoices.FirstOrDefault(x => x.InvoiceNumber == consolCost.E6_InvoiceNum);
				AssertEquals("USD", invoice.AH_RX_NKTransactionCurrency);
				var expectedRefreshedExRate = invoice.Company.GetExchangeRate().GetRate(consolCost.E6_Calc_LocalTotalAmount, consolCost.E6_Calc_OSTotalAmount);
				AssertEquals("AH_PostedToEFT", true, invoice.AH_PostedToEFT);
				AssertEquals($"Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), {consolCost.E6_Calc_OSTotalAmount} / {consolCost.E6_Calc_LocalTotalAmount}."
					, expectedRefreshedExRate
					, invoice.AH_ExchangeRate
				);

				AssertEquals(2, invoice.Lines.Count);

				foreach (InvoicingLineBase line in invoice.Lines)
				{
					AssertEquals(consolCost.E6_AC_ChargeCode, line.AL_AC);
					AssertEquals(consolCost.E6_RX_NKCurrency, line.AL_RX_NKTransactionCurrency);
					AssertEquals(consolCost.E6_ExchangeRate, line.AL_ExchangeRate.Round(2));

					AssertEquals(consolCost.E6_OSCostAmount / 2, line.AL_OSExTaxAmount);
					AssertEquals(consolCost.E6_LocalCostAmount / 2, line.AL_LocalExTaxAmount);
				}
			}

			//the jobs should have at most one exchange rate for debtor
			if (jobs[0].ExchangeRates.Count != 0)
			{
				AssertEquals(1, jobs[0].ExchangeRates.Count);
				AssertEquals(ExchangeRateValidLedgerEnum.AR, jobs[0].ExchangeRates[0].OrgType.ToLedger());
				AssertEquals(5.10m, jobs[0].ExchangeRates[0].JF_BaseRate);

				AssertEquals(1, jobs[1].ExchangeRates.Count);
				AssertEquals(ExchangeRateValidLedgerEnum.AR, jobs[1].ExchangeRates[0].OrgType.ToLedger());
				AssertEquals(5.10m, jobs[1].ExchangeRates[0].JF_BaseRate);
			}
		}
		#endregion

		#region Implementation

		protected void SetupChargeData(ZDecimal osSellAmount, AccChargeCode chargeCode, OrgHeader sellAccount = null)
		{
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = osSellAmount;
			if (osSellAmount < 0)
			{
				charge.JR_OSCostAmt = 0;
			}
			charge.JR_OH_SellAccount = sellAccount == null ? TestObjectCreator.LocalClient.PK : sellAccount.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();
		}

		protected void SetupCostChargeData(ZDecimal osAmount, AccChargeCode chargeCode, OrgHeader account = null)
		{
			Charge charge = Job1.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = osAmount;
			charge.JR_OH_CostAccount = account == null ? TestObjectCreator.AALSHI.PK : account.PK;
			charge.JR_APInvoiceNum = "APINV321";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			Factory.Save();
		}

		protected void SetupCostAndSellChargeData(Job job, ZDecimal osAmount, AccChargeCode chargeCode, ZDecimal osSellAmount, OrgHeader account = null, OrgHeader sellAccount = null)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = osSellAmount;
			charge.JR_OH_SellAccount = sellAccount == null ? TestObjectCreator.LocalClient.PK : sellAccount.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_OSCostAmt = osAmount;
			charge.JR_OH_CostAccount = account == null ? TestObjectCreator.AALSHI.PK : account.PK;
			charge.JR_APInvoiceNum = TestObjectCreator.GetRandomString(8);
			charge.JR_APInvoiceDate = ZDateTime.Today;
			Factory.Save();
		}

		protected virtual void SetupPosting()
		{
		}

		protected void SetupJobData()
		{
			SetupJobData(Factory);
		}

		protected void SetupJobData(BusinessObjectFactory factory)
		{
			SetupInvoiceRollup(TestObjectCreator.LocalClient);

			ForwardingShipment shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = TestObjectCreator.GetRandomString(8);
			fJob1 = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Job1.PlugInData = shipment;
			Job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job1.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			factory.Save();
		}

		Charge Charge;
		Charge Charge2;

		AccChequeBook AutoAllocateChequeBook;
		AccChequeBook AutoAllocateChequeBook2;

		protected void SetupJobData_AutoAllocationTest(string chequeBookDesc = null)
		{
			SetupInvoiceRollup(TestObjectCreator.LocalClient);

			AutoAllocateChequeBook = GetAutoPrintChequeBook(TestObjectCreator.AUDBankAccount, 1, 6, 5, "Chk1");
			if (!string.IsNullOrEmpty(chequeBookDesc))
			{
				AutoAllocateChequeBook.AK_Desc = chequeBookDesc;
			}

			AutoAllocateChequeBook2 = GetAutoPrintChequeBook(TestObjectCreator.AUDBankAccount, 1, 3, 3, "Chk2");
			Factory.Save();

			Charge = CreateCharge(Job1, TestObjectCreator.CC1, "Charge Code 1", TestObjectCreator.AUD, 150M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 150M, TestObjectCreator.LocalClient);
			Charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			SetAPInvoiceInfo(Charge, "1", ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(20));
			SetAPPaymentInfo(Charge, ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, "");
			Charge.JR_AK = AutoAllocateChequeBook.PK;

			Charge2 = CreateCharge(Job1, TestObjectCreator.CC2, "Charge Code 2", TestObjectCreator.AUD, 150M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 150M, TestObjectCreator.LocalClient);
			Charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			SetAPInvoiceInfo(Charge2, "2", ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(20));
			SetAPPaymentInfo(Charge2, ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, "");
			Charge2.JR_AK = AutoAllocateChequeBook2.PK;

			Factory.Save();
		}

		AccChequeBook GetAutoPrintChequeBook(AccBankAccount bank, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO, ZString aK_Code)
		{
			AccBankAccount bankAccount = bank;
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			chequeBook.AK_Code = aK_Code;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		AccTransactionHeader GetPaymentFromCollection(AccTransactionHeaderCollection collection, ZString chequeNo)
		{
			foreach (AccTransactionHeader header in collection)
			{
				if (header.AH_ChequeOrReference == chequeNo)
				{
					return header;
				}
			}
			return null;
		}

		public class TestPostManagerGUIWrapper : PostManagerGUIWrapper
		{
			/// <summary>
			/// This constructor is used only to test exception thrown in construction when SecurityHelper is null
			/// </summary>
			public TestPostManagerGUIWrapper()
				: base(null, JobInvoicingPostingOption.All, null, null)
			{
				DoTestPostTransactions = true;
			}

			public TestPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory factory, Job job, Action refreshJobCharges = null)
				: base(job.Parent, postingOption, factory, null, refreshJobCharges)
			{
				Jobs = LoadJobsInNewFactory(job);
				DoTestPostTransactions = true;
			}

			public TestPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory factory, Job job, BusinessObjectFactory newIsolatedFactory, EventHandler doStuffBeforeSave)
				: base(job.Parent, postingOption, factory, null)
			{
				this.TransactionFactory = newIsolatedFactory;
				this.DoStuffBeforeSaveForTest += doStuffBeforeSave;
				Jobs = LoadJobsInNewFactory(job);
				DoTestPostTransactions = true;
			}

			protected override void PrintInvoices(TransactionCreatorHashtable transactions)
			{
			}

			protected override string GetJobOnHoldMessage(IEnumerable<Job> jobs)
			{
				return "";
			}

			protected override string GetNothingPostedMessage()
			{
				return "";
			}

			protected override BasePostManager GetNewPostManager()
			{
				return new InvoicingPostManager(Jobs.FirstOrDefault());
			}
		}

		public void TestJobCreationExceptionWhenDebtorPECCodeIsNotDefined_WhenComplianceSubTypeIsEAR() =>
			TestJobCreationExceptionWhenDebtorPECCodeIsNotDefined(@"An email address is required for this Debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the Organization record for the Debtor to include a valid email address (Maintain > Master Data > Organization).

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)");

		public void TestJobCreationExceptionWhenDebtorPECCodeIsNotDefined_WhenComplianceSubTypeIsNotEAR() =>
			TestJobCreationExceptionWhenDebtorPECCodeIsNotDefined(@"A Post Box Alias is required for this debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the organization record for the debtor to include a valid Post Box Alias email address using the registration number type PEC.

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)", true);

		void TestJobCreationExceptionWhenDebtorPECCodeIsNotDefined(string expectedError, bool addVTECusCode = false)
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(GlbBranch.CurrentBranch.PK.ToGuid(), DateTime.Today.AddDays(-30)))
			{
				var vat18 = TestObjectCreator.CreateTaxRate("VAT", "VAT", 18);
				vat18.AT_PostingGroupId = 1;

				var vat8 = TestObjectCreator.CreateTaxRate("VAT8", "VAT8", 8);
				vat8.AT_PostingGroupId = 2;

				Factory.Save();

				SetupJobData();

				LocalClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m, 0m); //cleaning up cfx otherwise it will be applied when we change country

				var charge1 = CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
				var charge2 = CreateCharge(Job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
				charge1.JR_AT_CostGSTRate = vat18.PK;
				charge2.JR_AT_CostGSTRate = vat8.PK;
				charge1.JR_APInvoiceNum = charge2.JR_APInvoiceNum = "APINV321";
				charge1.JR_APInvoiceDate = charge2.JR_APInvoiceDate = ZDateTime.Today;

				Factory.Save();

				var guiWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1, null);

				if (addVTECusCode)
				{
					LocalClient.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VTE, "123456789");
					LocalClient.Factory.Save();
				}
				guiWrapper.Post();

				if (!addVTECusCode)
				{
					AssertEquals("Error message is shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertNotContains(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains(@"AP Invoice Number APINV321.
You have prepared charges using a mix of Tax ID Posting Groups. Are you sure you want to post these charges?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public class TestPostManagerGUIWrapperForTestConcurrency : TestPostManagerGUIWrapper
		{
			readonly ZGuid chargePKForDelete;
			public TestPostManagerGUIWrapperForTestConcurrency(JobInvoicingPostingOption postingOption, BusinessObjectFactory factory, Job job, ZGuid chargePK)
				: base(postingOption, factory, job)
			{
				DoTestPostTransactions = false;
				chargePKForDelete = chargePK;
			}

			protected override bool PostTransactionsCore(TransactionCreatorHashtable transactions)
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var testCharge = newFactory.Load<Charge>(chargePKForDelete);
				testCharge?.Delete();
				newFactory.Save();
				return base.PostTransactionsCore(transactions);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Today.Year);
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Today.Year + 1);
		}

		bool Contains(string text, UnitTestUserNotification.PreviousMessageList messageList)
		{
			bool contains = false;
			foreach (UnitTestUserNotification.PreviousMessage message in messageList)
			{
				if (!string.IsNullOrEmpty(message.Text) && message.Text.Contains(text))
				{
					contains = true;
					break;
				}
			}
			return contains;
		}

		protected abstract PostManagerGUIWrapper GUIWrapper
		{
			get;
		}

		protected virtual IJobInvoicingPlugIn GUIWrapperPlugIn
		{
			get { return GUIWrapper.JobInvoicingPlugIn_ForTestOnly; }
		}

		int CompareEmailDef(EmailDef emailX, EmailDef emailY)
		{
			int result = emailX.Subject.CompareTo(emailY.Subject);

			if (result == 0)
			{
				result = emailX.Body.CompareTo(emailY.Body);
			}

			return result;
		}

		#endregion
	}
}
