using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.GUI.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class ConsolInvoicingPostManagerGUIWrapperTest : PostManagerGUIWrapperTest
	{
		public void TestAgentInvoicesCreationWhenJobStatusIsWHL()
		{
			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();
				var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C1");
				consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
				consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
				var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "USLAX", consol);
				Factory.Save();

				var apps = new ApportionmentListing(Factory, consol);
				var freightCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.USD, 0.775m, 250m, TestObjectCreator.Agent, AllocationMethod.Shipment, apps);
				freightCost.E6_InvoiceNum = "abcxyz";
				freightCost.E6_InvoiceDate = ZDateTime.Now;
				freightCost.E6_PaymentDate = ZDateTime.Now;
				Factory.Save();

				var shipmentJob = (Job)shipment.Job;
				shipmentJob.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				Factory.Save();

				var wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, new[] { shipmentJob }, consol, FormForTest, apps);
				wrapper.Post();

				var expectedNothingToPostMessage = @"No appropriate charges were found for posting. This may be because:
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).";

				AssertContains(expectedNothingToPostMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPostWithoutCVError()
		{
			Factory.Save();
			var consol = TestObjectCreator.CreateConsol();
			for (int i = 0; i < 13; i++)
			{
				var shipment = TestObjectCreator.CreateShipment($"S000{i}", consol);
				var job = TestObjectCreator.CreateJob(shipment, false, false, false, TestObjectCreator.LocalClient);
				job.RunPreSaveValidation();
				AssertNoErrors(job);
			}
			var taxRate = TestObjectCreator.CreateTaxRate("VAT5", "tax", 5);

			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 300, TestObjectCreator.Creditor1, AllocationMethod.Manual);
			consolCost.E6_AT_TaxRate = taxRate.PK;
			consolCost.E6_InvoiceNum = "INT1";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;

			var amounts = new[] { 0.01m, 0.03m, 0.03m, 0.04m, 0.05m, 0.06m, 0.06m, 0.08m, 0.08m, 0.09m, 0.10m, 0.12m, 299.25m };
			for (int i = 0; i < 13; i++)
			{
				consolCost.ApportionmentCharges[i].JR_OSCostAmt = amounts[i];
			}

			consolCost.RunPreSaveValidation();
			AssertNoErrors(consolCost);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, consolCost.ApportionmentCharges.Cast<BaseCharge>().Select(x => x.InvoicingJob), consol, FormForTest, consol.GetApportionments());
			wrapper.Post();
			Assert(consolCost.IsPosted);
		}

		public override void TestAllowDeliveryDuringPreviewInvoice()
		{
			Assert(!WrapperForTest.AllowDeliveryDuringPreviewInvoice_ForTestOnly);
		}

		public void TestProfitShareConfirmationSetupDocWrapperContextManagerProperly()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader orgHeader = factory.NewWithValidTestData<OrgHeader>();
			ProfitShareDetail profitShare = new ProfitShareDetail(orgHeader, factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareDetailCollection profitShares = new ProfitShareDetailCollection();
			profitShares.Add(profitShare);
			ProfitShareConfirmationEventArgs args = new ProfitShareConfirmationEventArgs(profitShares);
			WrapperForTest.ConsolInvoicingPostManagerGUIWrapper_ProfitShareConfirmation(null, args);
			AssertEquals("CalculatedProfitShares.Count", 1, args.CalculatedProfitShares.Count);
			DocWrapperContextManager manager = args.CalculatedProfitShares[0].Factory.GetDocWrapperContextManager();
			AssertEquals("DocumentContactTypeCode should be A/R", "A/R", ((IDocWrapperContext)manager).DocumentContactTypeCode);
			AssertEquals("DocumentContactTypeCode should be Profit Share Calculation", "Profit Share Calculation", ((IDocWrapperContext)manager).MenuTitle);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "You cannot access the DocWrapperContext properties until they have been setup by the Report. If you are accessing this property from the Constructor of your DocumentWrapper, try converting to a lazy loading pattern so that it gets accessed after it's been setup.")]
		public void TestProfitSharePrintTaskWillNotBeInitializedIfSupressSetToTrue()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader orgHeader = factory.NewWithValidTestData<OrgHeader>();
			ProfitShareDetail profitShare = new ProfitShareDetail(orgHeader, factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareDetailCollection profitShares = new ProfitShareDetailCollection();
			profitShares.Add(profitShare);
			ProfitShareConfirmationEventArgs args = new ProfitShareConfirmationEventArgs(profitShares);
			AssertEquals(false, WrapperForTest.SupressProfitSharePrintTask);
			WrapperForTest.SupressProfitSharePrintTask = true;
			AssertEquals(true, WrapperForTest.SupressProfitSharePrintTask);
			WrapperForTest.ConsolInvoicingPostManagerGUIWrapper_ProfitShareConfirmation(null, args);
			DocWrapperContextManager manager = args.CalculatedProfitShares[0].Factory.GetDocWrapperContextManager();
			var code = ((IDocWrapperContext)manager).DocumentContactTypeCode;
		}

		public void TestShowExportPostingPopup()
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(TestObjectCreator.AALSHI.PK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);
			Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector optionSelector = new Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector(Factory, Consol, charges);
			ExportAgentPostingEventArgs args = new ExportAgentPostingEventArgs(optionSelector);
			try
			{
				WrapperForTest.ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting_ForTestOnly(null, args);
				AssertEquals("Should show the agent posting currency selection form", typeof(AgentPostingOptionSelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
			finally
			{
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestGatewayPostingOptionAddsExtraHintToNoChargesToPostMessage()
		{
			var items = TestObjectCreator.CreateGatewayConsolsAndShipments();
			var consol = items.gC0001;
			TestObjectCreator.CreateJob(items.s0002);
			Factory.Save();
			var wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Gateway, consol.Factory, consol.Shipments.Cast<ForwardingShipment>().Select(x => x.Job as Job).Where(x => x != null), consol, FormForTest, new ApportionmentListing(Factory, consol));
			wrapper.Post();
			var expectedMessage = @"No appropriate charges were found for posting. This may be because:
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
* There are no charges where debtor is a gateway agent.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).";

			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage?.Text);
		}

		public void TestCantPostInvoicesWhereOrgNotARorAP()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();
			OrgHeader nonAROrg = Factory.NewWithValidTestData<OrgHeader>();
			nonAROrg.OH_IsDebtor = false;
			nonAROrg.OH_IsCreditor = true;

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";
			Consol.SetDefaultReceivingForwarderAddress(nonAROrg);
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			Apportionments.IsActivated = true;
			Apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();

			JobConsolCost cC1Cost = Apportionments.CostsCollection.TryAddNew();
			cC1Cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cC1Cost.E6_OSCostAmount = 100m;
			cC1Cost.E6_OH_Creditor = nonAROrg.PK;
			cC1Cost.E6_InvoiceNum = "IMRAANTEST";
			cC1Cost.E6_InvoiceDate = ZDateTime.Now;
			cC1Cost.E6_IsForCollectInvoice = true;

			Factory.Save();

			ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment1.PK);
			jobsQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentID, shipment2.PK);
			jobsQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			Jobs.Load(jobsQuery);

			foreach (Job job in Jobs)
			{
				foreach (Charge charge in job.Charges)
				{
					charge.JR_OH_SellAccount = ZGuid.Empty;
				}
			}

			Factory.Save();

			WrapperForTest.Post();

			ZString expectedErrorText = "No Transactions have been posted.\r\n" +
										"The following organizations must be marked as 'Receivables':\r\n\r\n" +
										"\t" + nonAROrg.OH_Code;
			AssertNotNull("Should have shown error message", UnitTestUserNotification.Instance.LastMessage);
			AssertEquals("error message caption", expectedErrorText, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("shouldn't have created any invoices", 0, WrapperForTest.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
		}

		public void TestDontValidateNonApplicableApportionedCharges()
		{
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			Apportionments.IsActivated = true;
			Apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();

			JobConsolCost cC1Cost = Apportionments.CostsCollection.TryAddNew();
			cC1Cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cC1Cost.E6_OSCostAmount = 100m;
			cC1Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cC1Cost.E6_InvoiceNum = "IMRAANTEST";
			cC1Cost.E6_InvoiceDate = ZDateTime.Now;

			JobConsolCost cC2Cost = Apportionments.CostsCollection.TryAddNew();
			cC2Cost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			cC2Cost.E6_OSCostAmount = 300m;
			cC2Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cC2Cost.E6_InvoiceNum = "IMRAANTEST";
			cC2Cost.E6_InvoiceDate = ZDateTime.Now;
			cC2Cost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment1.PK);
			jobsQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentID, shipment2.PK);
			jobsQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			Jobs.Load(jobsQuery);

			foreach (Job job in Jobs)
			{
				foreach (Charge charge in job.Charges)
				{
					charge.JR_OH_SellAccount = ZGuid.Empty;
				}
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			Jobs = new JobCollection(newFactory);
			Jobs.Load(jobsQuery);

			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, newFactory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
			wrapper.Post();

			AssertNull("Should be no validation error messagebox", UnitTestUserNotification.Instance.LastMessage.Text);

			APInvoice inv = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "IMRAANTEST").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull(inv);
		}

		public void TestProfitShareDocContactType()
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(TestObjectCreator.AALSHI.PK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);
			Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector optionSelector = new Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector(Factory, Consol, charges);
			ExportAgentPostingEventArgs args = new ExportAgentPostingEventArgs(optionSelector);
			AssertEquals("Should show the agent posting currency selection form", ContactType.Receivables.ToString(), WrapperForTest.ProfitShareMenuItem_ForTestOnly.SU_ContactType);
		}

		public void TestProfitShareMenuItem()
		{
			Assert("DocumentCommand not ReportCommand", WrapperForTest.ProfitShareMenuItem_ForTestOnly is DocumentCommand);
			AssertEquals("SU_MenuName", "Profit Share Calculation", WrapperForTest.ProfitShareMenuItem_ForTestOnly.SU_MenuName);
			AssertEquals("SU_ContactType", ContactType.Receivables.Code, WrapperForTest.ProfitShareMenuItem_ForTestOnly.SU_ContactType);
			Assert("SU_PreventAutoDelivery should be false (to allow delivery details to be found)", !WrapperForTest.ProfitShareMenuItem_ForTestOnly.SU_PreventAutoDelivery);
			Assert("SU_IsSystemDefined should be true", WrapperForTest.ProfitShareMenuItem_ForTestOnly.SU_IsSystemDefined);
			Assert("SU_SupportsVisualisation should be false", !WrapperForTest.ProfitShareMenuItem_ForTestOnly.SU_SupportsVisualisation);
			Assert("SU_IsModifiable should be false", !WrapperForTest.ProfitShareMenuItem_ForTestOnly.SU_IsModifiable);
		}

		public void TestShowInformationWhenIncorrectRegistrySetup()
		{
			WrapperForTest.ConsolInvoicingPostManagerGUIWrapper_IncorrectRegistrySetup_ForTestOnly(null, new IncorrectRegistrySetupEventArgs("Test Registry Location"));
			string expectedMessage = "Incorrect a Registry Item value." +
				System.Environment.NewLine +
				"Please set up a correct value of the Registry Item: 'Test Registry Location'.";
			AssertEquals("Should show error",
				expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Should be a warning", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestPostingWithCreditCardPaymentViaENett()
		{
			var tempUserContext = new TemporaryUserContext();
			tempUserContext.DepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK.ToGuid();
			using (tempUserContext.Set())
			{
				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				TestObjectCreator objectCreator = new TestObjectCreator(Factory);
				AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_IsDebtor = true;
				orgHeader.OH_IsCreditor = true;
				OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
				cusCode.OK_CustomsRegNo = "123456";
				cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				AssertEquals("eNettRegistrationNumber", "123456", orgHeader.ENettRegistrationNumber);

				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_Code = "CONS";
				consignor.OH_IsConsignor = true;

				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "USLAX";
				Consol.SetDefaultReceivingForwarderAddress(orgHeader);
				ForwardingShipment shipment1 = Consol.Shipments.AddNew();
				shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipment1.ConsignorPK = consignor.PK;

				ForwardingShipment shipment2 = Consol.Shipments.AddNew();
				shipment2.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipment2.ConsignorPK = consignor.PK;
				AssertEquals("There should be shipments on the Consol", 2, Consol.Shipments.Count);

				Apportionments.IsActivated = true;
				Apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();

				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZZAUDAcc";
				var bankAccount = objectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", objectCreator.AUD, "123456", "12345678", header);
				bankAccount.AB_DebitCreditCardExpiry = "0699";
				bankAccount.AB_DebitCreditCardName = "MR JOHN SMITH";
				var encoder = new TwoWayEncoder(bankAccount.PK.ToGuid());
				bankAccount.AB_DebitCreditCardNumber = encoder.Encrypt("1234567812345678");
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
				bankAccount.AB_AccountNum = "**** **** ***4 5678";

				cusCode = TestObjectCreator.AALSHI.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
				cusCode.OK_CustomsRegNo = "123456";
				cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				AssertEquals("eNettRegistrationNumber", "123456", TestObjectCreator.AALSHI.ENettRegistrationNumber);

				JobConsolCost cC1Cost = Apportionments.CostsCollection.TryAddNew();
				cC1Cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cC1Cost.E6_OSCostAmount = 100m;
				cC1Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cC1Cost.E6_InvoiceNum = "TEST1234";
				cC1Cost.E6_InvoiceDate = ZDateTime.Now;
				cC1Cost.E6_ApportionmentMethod = "SHP";
				cC1Cost.E6_PaymentType = ReceiptTypes.eNettCreditCard;
				cC1Cost.E6_AB_BankAccount = bankAccount.PK;
				cC1Cost.E6_ChequeOrReference = "111";

				JobConsolCost cC2Cost = Apportionments.CostsCollection.TryAddNew();
				cC2Cost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
				cC2Cost.E6_OSCostAmount = 300m;
				cC2Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cC2Cost.E6_InvoiceNum = "TEST1234";
				cC2Cost.E6_InvoiceDate = ZDateTime.Now;
				cC2Cost.E6_ApportionmentMethod = "SHP";
				cC2Cost.E6_PaymentType = ReceiptTypes.eNettCreditCard;
				cC2Cost.E6_AB_BankAccount = bankAccount.PK;
				cC2Cost.E6_ChequeOrReference = "111";

				Factory.Save();

				ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment1.PK);
				jobsQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentID, shipment2.PK);
				jobsQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				Jobs.Load(jobsQuery);

				ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
				query.AddToFilter(GlbDepartmentSchema.GE_Misc, false);
				GlbDepartment department = Factory.LoadTop1<GlbDepartment>(query);

				foreach (Job job in Jobs)
				{
					foreach (JobCharge charge in job.Charges)
					{
						charge.JR_OH_SellAccount = orgHeader.PK;
					}
				}

				Factory.Save();

				AssertEquals("Shipment1 should not have errors. Errors: " + shipment1.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment1.HasErrors);
				AssertEquals("Shipment2 should not have errors. Errors: " + shipment2.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment2.HasErrors);

				foreach (Job job in Jobs)
				{
					job.RunPreSaveValidation();
					AssertEquals(string.Format("Job {0} should not have errors. Errors: {1}", job.JH_JobNum, job.NotificationsIncludingChildren.ToUniqueMessageListString()), false, job.HasErrors);
				}

				int initialInvokedCount = MockENettWebService.Instance.CountProcessCreditCardWasInvoked;
				eNettWebServiceWrapper.UseRealWebService_ForTesting = false;
				MockENettWebService.Instance.SetupForTesting("CARGOWISE");

				ConsolInvoicingPostManagerGUIWrapper postManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);

				bool isCreditCardSecurityCodeFormShown = false;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form.GetType() == typeof(CreditCardSecurityCodeForm))
					{
						var securityCodeBizo = (PaymentCreditCardSecurityCode)((ZForm)form).BusinessEntity;
						securityCodeBizo.CardSecurityCode = "333";
						securityCodeBizo.Continue = true;

						isCreditCardSecurityCodeFormShown = true;
					}
				});
				postManagerWrapper.PostManager_ForTestOnly.OnNothingPosted += new EventHandler(PostManager_OnNothingPosted);
				postManagerWrapper.Post();
				Assert(nameof(isCreditCardSecurityCodeFormShown), isCreditCardSecurityCodeFormShown);

				APInvoice invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST1234").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("APInvoice", invoice);

				query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, string.Format("AP Payment {0}", Consol.JK_UniqueConsignRef));
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.eNettCreditCard);
				query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, bankAccount.AB_GC);

				APPayment payment = Factory.LoadTop1<APPayment>(query);
				AssertNotNull("APPayment", payment);
				AssertEquals("APPayment.AH_InvoiceAmount", 440.00m, payment.AH_InvoiceAmount);
			}
		}

		[TestDate(2011, 04, 10)]
		public void TestPostingCostsDefaultingPostDate()
		{
			bool originalComPayEnabled = AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value;
			var tempUserContext = new TemporaryUserContext();
			tempUserContext.DepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK.ToGuid();
			using (tempUserContext.Set())
			{
				setupRegistryForPostDateDefaulting();

				TestObjectCreator objectCreator = new TestObjectCreator(Factory);
				AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_IsDebtor = true;
				orgHeader.OH_IsCreditor = true;

				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_Code = "CONS";
				consignor.OH_IsConsignor = true;

				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "USLAX";
				Consol.SetDefaultReceivingForwarderAddress(orgHeader);
				ForwardingShipment shipment1 = Consol.Shipments.AddNew();
				shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipment1.ConsignorPK = consignor.PK;

				ForwardingShipment shipment2 = Consol.Shipments.AddNew();
				shipment2.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipment2.ConsignorPK = consignor.PK;
				AssertEquals("There should be shipments on the Consol", 2, Consol.Shipments.Count);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				ZDateTime departureDate = new ZDateTime(2011, 04, 05);
				Consol.Transports.DepartureTransport.JW_ATD = departureDate;

				Apportionments.IsActivated = true;
				Apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();

				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZZAUDAcc";
				var bankAccount = objectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", objectCreator.AUD, "123456", "12345678", header);

				JobConsolCost cC1Cost = Apportionments.CostsCollection.TryAddNew();
				cC1Cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cC1Cost.E6_OSCostAmount = 100m;
				cC1Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cC1Cost.E6_InvoiceNum = "TEST1234";
				cC1Cost.E6_InvoiceDate = ZDateTime.Now;
				cC1Cost.E6_ApportionmentMethod = "SHP";
				cC1Cost.E6_PaymentType = ReceiptTypes.Cash;
				cC1Cost.E6_AB_BankAccount = bankAccount.PK;
				cC1Cost.E6_ChequeOrReference = "111";

				JobConsolCost cC2Cost = Apportionments.CostsCollection.TryAddNew();
				cC2Cost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
				cC2Cost.E6_OSCostAmount = 300m;
				cC2Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cC2Cost.E6_InvoiceNum = "TEST1234";
				cC2Cost.E6_InvoiceDate = ZDateTime.Now;
				cC2Cost.E6_ApportionmentMethod = "SHP";
				cC2Cost.E6_PaymentType = ReceiptTypes.Cash;
				cC2Cost.E6_AB_BankAccount = bankAccount.PK;
				cC2Cost.E6_ChequeOrReference = "111";

				Factory.Save();

				ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment1.PK);
				jobsQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentID, shipment2.PK);
				jobsQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				Jobs.Load(jobsQuery);

				ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
				query.AddToFilter(GlbDepartmentSchema.GE_Misc, false);
				GlbDepartment department = Factory.LoadTop1<GlbDepartment>(query);

				foreach (Job job in Jobs)
				{
					foreach (JobCharge charge in job.Charges)
					{
						charge.JR_OH_SellAccount = orgHeader.PK;
					}
				}

				Factory.Save();

				AssertEquals("Shipment1 should not have errors. Errors: " + shipment1.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment1.HasErrors);
				AssertEquals("Shipment2 should not have errors. Errors: " + shipment2.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment2.HasErrors);

				foreach (Job job in Jobs)
				{
					job.RunPreSaveValidation();
					AssertEquals(string.Format("Job {0} should not have errors. Errors: {1}", job.JH_JobNum, job.NotificationsIncludingChildren.ToUniqueMessageListString()), false, job.HasErrors);
				}

				ConsolInvoicingPostManagerGUIWrapper postManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);

				postManagerWrapper.PostManager_ForTestOnly.OnNothingPosted += new EventHandler(PostManager_OnNothingPosted);
				postManagerWrapper.Post();

				APInvoice invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST1234").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("APInvoice", invoice);
				AssertEquals("APInvoice.AH_PostDate", departureDate, invoice.AH_PostDate);

				query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, string.Format("AP Payment {0}", Consol.JK_UniqueConsignRef));
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.Cash);
				query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				APPayment payment = Factory.LoadTop1<APPayment>(query);
				AssertNotNull("APPayment", payment);
				AssertEquals("APPayment.AH_InvoiceAmount", 440.00m, payment.AH_InvoiceAmount);
				AssertEquals("APPayment.AH_PostDate", departureDate, payment.AH_PostDate);
			}
		}

		void PostManager_OnNothingPosted(object sender, EventArgs e)
		{
			Assert("Nothing Posted", false);
		}

#region Post Date Defaulting

		void setupRegistryForPostDateDefaulting()
		{
			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();
			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();
			addPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "ALL", "ALL", "", "ARV", "EPM", "SGN", "SGN", "ADD");
			addPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "ALL", "ALL", "", "DEP", "EPM", "SGN", "SGN", "ADD");
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
		}

		void addPostDateConfiguration(BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration, string jobType, string direction, string mode, string broker,
									string significantDateCode, string priorClosedPeriod, string priorOpenPeriod, string currentPeriod, string futurePeriod)
		{
			PostDateConfiguration config = backDateAPInvoicesConfiguration.PostDateConfigurationCollection.AddNew();
			config.JobType = jobType;
			config.DirectionCode = direction;
			config.Mode = mode;
			config.BrokerCode = broker;
			config.SignificantDateCode = significantDateCode;
			config.PriorClosedPeriod = priorClosedPeriod;
			config.PriorOpenPeriod = priorOpenPeriod;
			config.CurrentPeriod = currentPeriod;
			config.FuturePeriod = futurePeriod;
		}

#endregion

#region BackDateARInvoices

		[TestDate(2005, 3, 11)]
		public void TestRevenueRecognitionDatesWhenBackDateARInvoicesWithRegistryEnabled()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			BackDateInvoicesConfiguration config = BackDateInvoicesConfiguration_BackDateInvoicesTrue;
			config.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			for (int i = 0; i < 100; i++)
			{
				var shipmentNumber = "S" + i;
				var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
				Consol.Shipments.Add(shipment);
				var job = TestObjectCreator.CreateJob(shipment, false);
				job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
				job.AgentCollectPK = TestObjectCreator.Agent.PK;
				Jobs.Add(job);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			}

			Factory.Save();

			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);

			InitializeBackDateARInvoiceWithBackDateDialogResultYes();

			Func<ChangeTransactionDatesMessageBox> getShownDialog;
			Func<IBusiness> getShownDialogBusinessEntity;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog, out getShownDialogBusinessEntity);

			wrapper.Post();

			AssertEquals("Should have created one invoice", 1, wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
			InvoicingBase invoice = wrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0];
			AssertNotNull("Should have shown question about back dating", getShownDialog());
			ChangeTransactionDatesBusinessObject lastBizo = getShownDialogBusinessEntity() as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastBizo);

			AssertEquals("Should have shown dialog box with correct invoice date", "Too Many Recognition Dates to Display - See Job Profit Document", lastBizo.RevenueRecognitionDates);
		}

		protected override void SetupJobDataForBackDateARInvoiceTests()
		{
			base.SetupJobDataForBackDateARInvoiceTests();

			Jobs.Add(Job1);

			GUIWrapper_inner = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
		}

		protected override void SetupJobDataForAPInvoiceTests()
		{
			base.SetupJobDataForAPInvoiceTests();

			Jobs.Add(Job1);

			GUIWrapper_inner = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
		}

		protected override SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ModifyTransactionDate); }
		}

		protected override SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ModifyPostDate); }
		}

		protected override SecurityCheckpoint OverrideRequisitionDetailsSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.OverrideRequisitionDetails); }
		}

#endregion

		public override void TestGetParentForPostingAction()
		{
			var parent = WrapperForTest.GetParentInfoForPostingAction_ForTestOnly();
			var parentID = parent.Id;
			var parentTableCode = parent.TableCode;
			AssertEquals(WrapperForTest.Consol_ForTestOnly.CostSupporter.PK, parentID);
			AssertEquals(JobConsolSchema.Constants.Prefix, parentTableCode);
		}

		public void TestConsolHasNoChangesExceptLogAfterPosting()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "JPAAM";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_UniqueConsignRef = "C1";

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1";
			consol.Shipments.Add(shipment);
			Factory.Save();

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job.Parent = shipment;

			CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			CreateCharge(job, CC1, "Charge Code 1", TestObjectCreator.AUD, 100M, Creditor1, TestObjectCreator.AUD, 150M, LocalClient);
			job.Charges[0].JR_InvoiceType = "FIN";

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.AddRange(new Job[] { job });

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, jobs.Cast<Job>(), consol, FormForTest, apps);
			Factory.Save();
			Assert(!consol.HasChanges);
			wrapper.Post();
			Assert("No changes should occur when posting a consol", !consol.HasChanges);

			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, consol.PK);
			StmALog[] retrievedLogs = Factory.Load<StmALog>(filter);

			bool isFound = false;
			ZString logReference = "FIN INV C1";
			foreach (StmALog log in retrievedLogs)
			{
				if (Events.ServiceInvoicePosted.Code == log.SL_SE_NKEvent)
				{
					isFound = true;
					AssertEquals(log.SL_Reference, logReference);
					break;
				}
			}
			Assert("LogReference should occur when posting", isFound);
		}

		public override void TestCreditLimitEmailsAreSentWhenPosting()
		{
			SetupForCreditLimitEmails();
			Job job1 = TestObjectCreator.Job1;
			Job job2 = TestObjectCreator.Job2;
			SetupMultipleInvoices(job1);
			SetupMultipleInvoices(job2);

			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			AssertEquals("0 Emails should be sent", previousEmailCount, 0);
			Jobs.Add(job1);
			Jobs.Add(job2);

			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.ToArray<Job>(), Consol, FormForTest, Apportionments);
			wrapper.DoTestPostTransactions = false;
			wrapper.Post();

			AssertEquals("4 Emails should be sent", previousEmailCount + 4, Env.OutgoingMailManager.EmailsCreated.Count);
			var aPPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs));
			var aRPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs));
			AssertEquals("Should have created 12 invoices", 12, aPPostedInvoices.Length + aRPostedInvoices.Length);
		}

#region Payment Approval Security

		[TestDate(2012, 10, 15)]
		public void TestCheckNonPostingIfNoPaymentApprovalSecurity_WithPaymentAuthorisationSettings_Denied()
		{
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample());
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = false;
			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			TestCheckNonPostingIfNoPaymentApprovalSecurity(
				new string[] { ReceiptTypes.Cheque, ReceiptTypes.Cash },
				@"You are posting one or more payment transactions as part of this action.
You do not have the security rights to post one or more of these payment transactions due to security on the 'Payment Type'.

The payment types are as follows:

Check - for which you require access to security function: Manage -> Payables -> Payment Processing -> New -> Cheque

If you click 'Yes', these payments will be created in the 'Payment Processing' module as 'Approved, but not Posted' payments.
If you click 'No', this will cancel your action and no transactions will be posted.

Continue with posting?"
				);
		}

		[TestDate(2012, 10, 15)]
		public void TestCheckNonPostingIfNoPaymentApprovalSecurity_WithPaymentAuthorisationSettings_Allowed()
		{
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample());
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = true;
			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			TestCheckNonPostingIfNoPaymentApprovalSecurity(
				new string[] { ReceiptTypes.Cheque, ReceiptTypes.Cash },
				null);
		}

		[TestDate(2012, 10, 15)]
		public void TestCheckNonPostingIfNoPaymentApprovalSecurity_WithoutPaymentAuthorisationSettings_Denied()
		{
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty());
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = true;
			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			TestCheckNonPostingIfNoPaymentApprovalSecurity(
				new string[] { ReceiptTypes.Cheque, ReceiptTypes.Cash },
				@"You are posting one or more payment transactions as part of this action.
You do not have the security rights to post one or more of these payment transactions due to security on the 'Payment Type'.

The payment types are as follows:

Check - for which you require access to security function: Manage -> Payables -> Payables Transactions -> New Transactions -> Payment -> Cheque

If you click 'Yes', these payments will be created in the 'Payment Processing' module as 'Approved, but not Posted' payments.
If you click 'No', this will cancel your action and no transactions will be posted.

Continue with posting?"
				);
		}

		[TestDate(2012, 10, 15)]
		public void TestCheckNonPostingIfNoPaymentApprovalSecurity_WithoutPaymentAuthorisationSettings_Allowed()
		{
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty());
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = false;
			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			TestCheckNonPostingIfNoPaymentApprovalSecurity(
				new string[] { ReceiptTypes.Cheque, ReceiptTypes.Cash },
				null);
		}

		[TestDate(2012, 10, 15)]
		public void TestCheckNonPostingIfNoPaymentApprovalSecurity_WithoutPaymentAuthorisationSettings_Denied_Dual()
		{
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty());
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = true;
			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			Env.Security.APPaymentProcessingNewCash.IsAllowed = true;
			Env.Security.NewPayablesPaymentCash.IsAllowed = false;
			TestCheckNonPostingIfNoPaymentApprovalSecurity(
				new string[] { ReceiptTypes.Cheque, ReceiptTypes.Cash },
				@"You are posting one or more payment transactions as part of this action.
You do not have the security rights to post one or more of these payment transactions due to security on the 'Payment Type'.

The payment types are as follows:

Cash - for which you require access to security function: Manage -> Payables -> Payables Transactions -> New Transactions -> Payment -> Cash
Check - for which you require access to security function: Manage -> Payables -> Payables Transactions -> New Transactions -> Payment -> Cheque

If you click 'Yes', these payments will be created in the 'Payment Processing' module as 'Approved, but not Posted' payments.
If you click 'No', this will cancel your action and no transactions will be posted.

Continue with posting?"
				);
		}

		protected void TestCheckNonPostingIfNoPaymentApprovalSecurity(
			string[] receiptTypes,
			string expectedMessage)
		{
			TestObjectCreator.SetupAutoPrintChequeBook(TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook, Factory);
			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("1001", "AUSYD", "AUMEL", Consol);
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			var job = TestObjectCreator.CreateJob(shipment1, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			job.AgentCollectPK = TestObjectCreator.ZECTRA.PK;
			Factory.Save();

			int i = 0;
			foreach (var receiptType in receiptTypes)
			{
				var creditor = TestObjectCreator.CreateOrgHeader(i++.ToString(), true, false);
				var debtor = TestObjectCreator.CreateOrgHeader(i++.ToString(), false, true);
				var chargeNeedingSecurity = TestObjectCreator.CreateCharge((Job)shipment1.Job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, 100,
					creditor, TestObjectCreator.AUD, 100, debtor);

				chargeNeedingSecurity.JR_APInvoiceDate = ZDateTime.Now;
				chargeNeedingSecurity.JR_APInvoiceNum = "123";
				chargeNeedingSecurity.JR_PaymentDate = ZDateTime.Now;
				chargeNeedingSecurity.JR_PaymentType = receiptType;
				chargeNeedingSecurity.JR_AB = TestObjectCreator.AUDBankAccount.PK;

				if (receiptType == ReceiptTypes.Cheque)
				{
					chargeNeedingSecurity.JR_AK = TestObjectCreator.AUDChequeBook.PK;
				}
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			Jobs = new JobCollection(newFactory);
			Jobs.Load(new ZQuery(JobHeaderSchema.PK, shipment1.Job.PK));

			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, newFactory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
			wrapper.Post();

			if (expectedMessage == null)
			{
				AssertNull("No warning expected. User has relevant payment authorization security", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertEquals("Warning expected due to lack of required payment authorization security", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolsPostingWithSubShipments()
		{
			var consol = TestObjectCreator.CreateConsol("AUMEL", "NZAKL", "C001001");
			consol.JK_ConsolMode = "BCN";
			Factory.Save();

			var shipment1 = TestObjectCreator.CreateMasterShipment("S001001", consol);
			Job masterShipmentJob = TestObjectCreator.CreateJob(shipment1, false, true);
			masterShipmentJob.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.MainAddress.PK;
			TestObjectCreator.CreateCharge(masterShipmentJob, TestObjectCreator.CC1, 0M, 50M);

			var shipment2 = TestObjectCreator.CreateShipmentWithCoLoadMaster("S001002", consol, shipment1);
			Job subShipmentJob = TestObjectCreator.CreateJob(shipment2, false, true);
			subShipmentJob.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.MainAddress.PK;
			TestObjectCreator.CreateCharge(subShipmentJob, TestObjectCreator.CC1, 0M, 100M);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			Job.Loader loader = new Job.Loader(newFactory, shipment1);

			Job job = loader.Load(true, false); //While loading job parent should be set but job should not be defaulted

			Jobs = new JobCollection(newFactory);
			Jobs.Add(job);
			Jobs.Add(shipment2.Job);

			JobConsolCost jobConsolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 100M, ZBool.True);

			PrepairApportionSplitChargForTest(jobConsolCost, shipment1, 100m, ZBool.True);
			PrepairApportionSplitChargForTest(jobConsolCost, shipment2, 0m, ZBool.False);

			jobConsolCost.E6_InvoiceNum = "123";
			jobConsolCost.E6_InvoiceDate = DateTime.Now;
			Factory.Save();

			var apportionments = new ApportionmentListing(new BusinessObjectFactory(), consol);
			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, newFactory, Jobs.ToArray<Job>(), consol, FormForTest, apportionments);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wrapper.Post();
			AssertNull("There should now be no error and posting should succeed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

#endregion

#region InvoicePostingExchangeRateOption
		[TestDate(2015, 5, 10)]
		[DisableZeroExchangeRateOverriding]
		public override void TestPostWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupForInvoicePostingExchangeRateOption();
			var consolPK = CreateConsolForInvoicePostingExchangeRateOption();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.Load<ForwardingConsol>(consolPK);

			var jobs = new JobCollection(newFactory);
			jobs.Load(new ZQuery(JobHeaderSchema.JH_ParentID, consol.Shipments.Select(x => x.PK)));

			var postManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, newFactory, jobs.Cast<Job>()
				, consol, FormForTest, consol.GetApportionments());

			postManagerWrapper.Post();

			AssertConsolAndAPInvoiceForInvoicePostingExchangeRateOption(consol.PK);
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 1)]
		public override void TestBackDatingARAPInvoiceWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupForBackDatingWithInvoicePostingExchangeRateOption();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = TestObjectCreator.GetRandomString(9);
			var apportionments = new ApportionmentListing(Factory, consol);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = TestObjectCreator.GetRandomString(9);

			apportionments.IsActivated = true;
			apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();

			var cC1Cost = apportionments.CostsCollection.TryAddNew();
			cC1Cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cC1Cost.E6_RX_NKCurrency = "USD";
			cC1Cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cC1Cost.E6_ExchangeRate = 6.1m;
			cC1Cost.E6_OSCostAmount = 100m;
			cC1Cost.E6_InvoiceNum = TestObjectCreator.GetRandomString(8);
			cC1Cost.E6_InvoiceDate = new ZDateTime(2015, 5, 1);
			cC1Cost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			var jobs = new JobCollection(Factory);
			ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment1.PK);
			jobs.Load(jobsQuery);

			AssertEquals(1, jobs[0].Charges.Count);
			jobs[0].LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			var charge = jobs[0].Charges[0];
			AssertEquals(charge.JR_E6, cC1Cost.PK);
			//here we need to manually create an exchange Rate for debtor becuase Consol costing creates a generic exchange rate
			//this code can be removed after the consol costing is fixed to create specific exchange rate
			var exRate = jobs[0].ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = "USD";
			exRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			exRate.JF_OH_Org = TestObjectCreator.LocalClient.PK;
			AssertEquals(5.01m, exRate.JF_BaseRate);
			charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_InvoiceType = "CUR";

			AssertEquals(6.1m, charge.JR_OSCostExRate);
			AssertEquals(5.01m, charge.JR_OSSellExRate);
			AssertEquals(100m, charge.JR_OSCostAmt);
			charge.JR_OSSellAmt = 150m;
			AssertEquals(16.39M, charge.JR_LocalCostAmt);
			AssertEquals(29.94M, charge.JR_LocalSellAmt);
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Func<ChangeTransactionDatesMessageBox> getShownDialog;
			Func<IBusiness> getShownDialogBusinessEntity;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog, out getShownDialogBusinessEntity);

			var invoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals(0, invoices.Length);

			var postManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, jobs.Cast<Job>()
				, consol, FormForTest, consol.GetApportionments());
			postManagerWrapper.Post();
			Factory.Save();

			invoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals(2, invoices.Length);

			var apInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AP"));
			var arInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AR"));

			AssertNotNull(apInvoice);
			AssertNotNull(arInvoice);

			AssertNotNull("Should have shown question about back dating", getShownDialog());
			var lastBizo = getShownDialogBusinessEntity() as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastBizo);

			var dateTimeExpected = new DateTime(2015, 4, 30);
			AssertEquals("Should have shown dialog box with correct InvoiceDate", dateTimeExpected, lastBizo.InvoiceDate.Date);
			AssertEquals("Should have shown dialog box with correct PostDate", dateTimeExpected, lastBizo.PostDate.Date);

			AssertEquals(dateTimeExpected, apInvoice.AH_PostDate.Date);

			AssertEquals(dateTimeExpected, arInvoice.AH_InvoiceDate.Date);
			AssertEquals(dateTimeExpected, arInvoice.AH_PostDate.Date);
			AssertEquals(dateTimeExpected, arInvoice.AH_DueDate.Date);

			//charges and lines should be updated to new rate and new amount.
			AssertEquals("USD", charge.JR_RX_NKSellCurrency);
			AssertEquals(4.30m, charge.JR_OSSellExRate);
			AssertEquals(150m, charge.JR_OSSellAmt);
			AssertEquals(34.88M, charge.JR_LocalSellAmt);

			AssertEquals("USD", charge.JR_RX_NKCostCurrency);
			AssertEquals(4.30m, charge.JR_OSCostExRate);
			AssertEquals(100m, charge.JR_OSCostAmt);
			AssertEquals(23.26M, charge.JR_LocalCostAmt);

			AssertEquals("USD", apInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals(true, apInvoice.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 25.59.", 4.298554m, apInvoice.AH_ExchangeRate);
			var line1 = apInvoice.Lines[0];
			AssertEquals("USD", line1.AL_RX_NKTransactionCurrency);
			AssertEquals(4.30M, line1.AL_ExchangeRate.Round(2));
			AssertEquals(100m, line1.AL_OSExTaxAmount);
			AssertEquals(23.26M, line1.AL_LocalExTaxAmount);

			AssertEquals("USD", arInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals(4.30M, arInvoice.AH_ExchangeRate);
			var line2 = arInvoice.Lines[0];
			AssertEquals("USD", line2.AL_RX_NKTransactionCurrency);
			AssertEquals(4.30M, line2.AL_ExchangeRate.Round(2));
			AssertEquals(150m, line2.AL_OSExTaxAmount);
			AssertEquals(34.88M, line2.AL_LocalExTaxAmount);
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2020, 10, 10)]
		[ExpectNoExceptions]
		public void TestExchangeRateZeroCriticalValidationNotThrown_ConsolLevelPosting()
		{
			var today = ZDateTime.Today;
			var backDate = new ZDateTime(2020, 09, 30);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.5M, today, today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.6M, backDate, backDate);

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.DefaultPostDateFromInvoiceDate = true;
			config.OverridePostDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var localClient = TestObjectCreator.LocalClient;
			var agent = CreateOrgHeader("TESTAGENT", true, true);
			var agentAddress = TestObjectCreator.CreateAddress(agent);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.JK_OA_ReceivingForwarderAddress = agentAddress.PK;

			var shipment = TestObjectCreator.CreateShipment("S001", consol: consol);
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);

			Factory.Save();

			var costs = new ApportionmentListing(Factory, consol);

			try
			{
				var consolCost = costs.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
				consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				consolCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
				consolCost.E6_OSCostAmount = 300M;
				consolCost.E6_ApportionmentMethod = "SHP";
				consolCost.E6_OH_Creditor = agent.PK;
				consolCost.E6_InvoiceNum = "1001";
				consolCost.E6_InvoiceDate = ZDateTime.Today;

				Factory.Save();

				AssertEquals("Precondition", 1, job.Charges.Count);

				job.Charges[0].JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;

				Factory.Save();

				var jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, FormForTest);
				jobPostManagerWrapper.Post();

				var arInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)
					.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice));

				AssertNotNull(arInvoice);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

				var consolPostManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, new List<Job> { job }, consol, FormForTest, consol.GetApportionments());
				AssertNoExceptionThrown("No CV with 'This transaction exchange rate is less than or equal to 0.' should occur", () => consolPostManagerWrapper.Post());

				var expectedErrorMessage = @"AR Credit Note number 
The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 30-Sep-20. Please check your data and try again.";

				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				var sellExchangeRate_USD = 0.75M;
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", sellExchangeRate_USD, backDate, backDate);
				Factory.Save();

				consolPostManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, new List<Job> { job }, consol, FormForTest, consol.GetApportionments());
				consolPostManagerWrapper.Post();

				var arCreditNote = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)
					.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote));

				AssertNotNull(arCreditNote);

				AssertEquals("AR Credit Note post date", backDate, arCreditNote.AH_PostDate);
				AssertEquals("AR Credit Note exchange rate", sellExchangeRate_USD, arCreditNote.AH_ExchangeRate);
			}
			finally
			{
				costs.ReleaseMutexes();
			}
		}
#endregion

#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			FormForTest = new ZForm();
			Consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			Consol.JK_UniqueConsignRef = "C00001001";
			Apportionments = new ApportionmentListing(Factory, Consol);
			Jobs = new JobCollection(Factory);
			MockENettWebService.ClearInstance();
		}

		protected override void TearDown()
		{
			base.TearDown();

			FormForTest.Dispose();
			MockENettWebService.ClearInstance();
		}

		protected ZForm FormForTest;
		protected ForwardingConsol Consol;
		protected ApportionmentListing Apportionments;
		protected JobCollection Jobs;

		ConsolInvoicingPostManagerGUIWrapper WrapperForTest
		{
			get { return GUIWrapper as ConsolInvoicingPostManagerGUIWrapper; }
		}

		protected override PostManagerGUIWrapper GUIWrapper
		{
			get
			{
				if (GUIWrapper_inner == null)
				{
					GUIWrapper_inner = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
					GUIWrapper_inner.DoTestPostTransactions = true;
				}
				return GUIWrapper_inner;
			}
		}
		protected ConsolInvoicingPostManagerGUIWrapper GUIWrapper_inner;

		protected override void SetupPosting()
		{
			Jobs.Add(Job1);
			GUIWrapper_inner = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
		}

#endregion

		[ExpectNoExceptions]
		public void TestNoCriticalValidationOnPostingConsolWithMasterDetailShipments()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001234");
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ABIGAS.MainAddress.PK;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARBuyersConsolInvoicingStyle = "MAB";

			var shipment1 = CreateSampleShipment(consol, null);
			var shipment2 = CreateSampleShipment(consol, shipment1);
			var shipment3 = CreateSampleShipment(consol, shipment1);

			//Simulate job without mutex
			Job fakeJobForShipment1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			fakeJobForShipment1.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.Addresses[0].PK;
			TestObjectCreator.CreateCharge(fakeJobForShipment1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 100m, null);

			Factory.Save();

			JobConsolCost jobConsolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 2000M, ZBool.True);

			//Simulate job with mutex
			PrepairApportionSplitChargForTest(jobConsolCost, shipment1, 0m, ZBool.False);
			PrepairApportionSplitChargForTest(jobConsolCost, shipment2, 1000m, ZBool.True);
			PrepairApportionSplitChargForTest(jobConsolCost, shipment3, 1000m, ZBool.True);

			Factory.Save();

			ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobsQuery.AddToFilter(JobHeaderSchema.JH_ParentID, consol.CostSupporter.ShipmentsListPKs);
			Jobs.Load(jobsQuery);

			var apportionments = new ApportionmentListing(new BusinessObjectFactory(), consol);
			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, Jobs.Cast<Job>(), consol, FormForTest, apportionments);
			wrapper.Post();
		}

		public void TestPostUnsavedAndSavedConsol()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "JPAAM";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_UniqueConsignRef = "F1";

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "F1";
			consol.Shipments.Add(shipment);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			ConsolInvoicingPostManagerGUIWrapper wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.Cast<Job>(), consol, FormForTest, apps);

			Assert(consol.HasChanges);
			wrapper.Post();
			AssertEquals("Please save before posting", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			((UnitTestUserNotification)Globals.Message).ClearMessages();

			Factory.Save();
			ConsolInvoicingPostManagerGUIWrapper newWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.Cast<Job>(), consol, FormForTest, apps);
			Assert(!consol.HasChanges);
			newWrapper.Post();
			AssertNotEquals("Please save before posting", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
		}

		[TestDate(2015, 5, 1)]
		public void TestPostingCosts_InvoiceAmountAndPaymentAmountShouldBeConsistent()
		{
			var isReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				var tempUserContext = new TemporaryUserContext();
				tempUserContext.DepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK.ToGuid();
				using (tempUserContext.Set())
				{
					GlbCompany.CurrentCompany.GC_IsReciprocal = true;
					AssertEquals(true, GlbCompany.CurrentCompany.GC_IsReciprocal);

					var objectCreator = new TestObjectCreator(Factory);
					var helper = new AccountingPeriodTestHelper(Factory);
					helper.SetupPeriods();
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					orgHeader.OH_IsDebtor = true;
					orgHeader.OH_IsCreditor = true;

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_Code = "CONS";
					consignor.OH_IsConsignor = true;

					Consol.JK_RL_NKLoadPort = "AUSYD";
					Consol.JK_RL_NKDischargePort = "USLAX";
					Consol.SetDefaultReceivingForwarderAddress(orgHeader);

					for (int idx = 0; idx < 8; idx++)
					{
						var shipment1 = Consol.Shipments.AddNew();
						shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
						shipment1.ConsignorPK = consignor.PK;
					}

					AssertEquals("There should be shipments on the Consol", 8, Consol.Shipments.Count);

					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZZAUDAcc";
					var bankAccount = objectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", objectCreator.AUD, "123456", "12345678", header);

					var cost = Apportionments.CostsCollection.TryAddNew();
					cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
					cost.E6_OSCostAmount = 15m;
					cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
					cost.E6_InvoiceNum = "TEST1234";
					cost.E6_InvoiceDate = ZDateTime.Now;
					cost.E6_ApportionmentMethod = "SHP";
					cost.E6_PaymentType = ReceiptTypes.Cash;
					cost.E6_AB_BankAccount = bankAccount.PK;
					cost.E6_ChequeOrReference = "111";

					Factory.Save();

					foreach (var shipment in Consol.Shipments)
					{
						AssertEquals("shipment should not have errors. Errors: " + shipment.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment.HasErrors);
					}

					var shipmentPKs = Consol.Shipments.Select(shipment => shipment.PK).ToArray();
					Jobs.Load(new ZQuery(JobHeaderSchema.JH_ParentID, shipmentPKs).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
					AssertEquals(8, Jobs.Count);

					foreach (Job job in Jobs)
					{
						job.RunPreSaveValidation();
						AssertEquals(string.Format("Job {0} should not have errors. Errors: {1}", job.JH_JobNum, job.NotificationsIncludingChildren.ToUniqueMessageListString()), false, job.HasErrors);
					}

					ConsolInvoicingPostManagerGUIWrapper postManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);

					postManagerWrapper.PostManager_ForTestOnly.OnNothingPosted += new EventHandler(PostManager_OnNothingPosted);
					postManagerWrapper.Post();

					var invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST1234").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

					var query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, string.Format("AP Payment {0}", Consol.JK_UniqueConsignRef));
					query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.Cash);
					query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
					query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

					var payment = Factory.LoadTop1<APPayment>(query);
					var paymentApprovalBases = Factory.Load<PaymentApprovalBase>(new ZQuery());

					AssertEquals("payment amount should be the same as invoice amount", invoice.AH_OSTotalAmount, payment.AH_InvoiceAmount);
					AssertEquals(1, paymentApprovalBases.Length);
					AssertEquals("payment amount should be the same as invoice amount", invoice.AH_OSTotalAmount, paymentApprovalBases[0].AV_Amount);
					AssertEquals(16.50m, invoice.AH_OSTotalAmount);
					AssertEquals("PST", paymentApprovalBases[0].AV_Status);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;
			}
		}

		[TestDate(2015, 5, 1)]
		public void TestPostingCostsPaymentApprovalStatusWithRegistryOnSecurityOff()
		{
			var regSettings = new PaymentAuthorisationSettingsCollection();
			var regSetting = regSettings.AddNew();
			regSetting.Amount = 0m;
			regSetting.AuthorisationRequirement = "1st Level Only";
			regSetting.Range = "Above";
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regSettings);

			Env.Security.APPaymentProcessingFirstApproval.IsAllowed = false;

			AssertPostingCostsPaymentApprovalStatus("AWA");
		}

		[TestDate(2015, 5, 1)]
		public void TestPostingCostsPaymentApprovalStatusWithRegistryOffSecurityOff()
		{
			var regSettings = new PaymentAuthorisationSettingsCollection();
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regSettings);

			Env.Security.APPaymentProcessingFirstApproval.IsAllowed = false;

			AssertPostingCostsPaymentApprovalStatus("PST");
		}

		[TestDate(2015, 5, 1)]
		public void TestPostingCostsPaymentApprovalStatusWithRegistryOnSecurityOn()
		{
			var regSettings = new PaymentAuthorisationSettingsCollection();
			var regSetting = regSettings.AddNew();
			regSetting.Amount = 0m;
			regSetting.AuthorisationRequirement = "1st Level Only";
			regSetting.Range = "Above";
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regSettings);

			Env.Security.APPaymentProcessingFirstApproval.IsAllowed = true;

			AssertPostingCostsPaymentApprovalStatus("PST");
		}

		[TestDate(2015, 5, 1)]
		public void TestPostingCostsPaymentApprovalStatusWithRegistryOffSecurityOn()
		{
			var regSettings = new PaymentAuthorisationSettingsCollection();
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regSettings);

			Env.Security.APPaymentProcessingFirstApproval.IsAllowed = true;

			AssertPostingCostsPaymentApprovalStatus("PST");
		}

		void AssertPostingCostsPaymentApprovalStatus(ZString approvalStatus)
		{
			var tempUserContext = new TemporaryUserContext();
			tempUserContext.DepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK.ToGuid();
			using (tempUserContext.Set())
			{
				var objectCreator = new TestObjectCreator(Factory);
				var helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_IsDebtor = true;
				orgHeader.OH_IsCreditor = true;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_Code = "CONS";
				consignor.OH_IsConsignor = true;

				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "USLAX";
				Consol.SetDefaultReceivingForwarderAddress(orgHeader);

				var shipment1 = Consol.Shipments.AddNew();
				shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipment1.ConsignorPK = consignor.PK;

				AssertEquals("There should be shipments on the Consol", 1, Consol.Shipments.Count);

				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZZAUDAcc";
				var bankAccount = objectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", objectCreator.AUD, "123456", "12345678", header);

				var cost = Apportionments.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 15m;
				cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "TEST1234";
				cost.E6_InvoiceDate = ZDateTime.Now;
				cost.E6_ApportionmentMethod = "SHP";
				cost.E6_PaymentType = ReceiptTypes.Cash;
				cost.E6_AB_BankAccount = bankAccount.PK;
				cost.E6_ChequeOrReference = "111";

				Factory.Save();

				var shipmentPKs = Consol.Shipments.Select(shipment => shipment.PK).ToArray();
				Jobs.Load(new ZQuery(JobHeaderSchema.JH_ParentID, shipmentPKs).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, Jobs.Count);

				foreach (Job job in Jobs)
				{
					job.RunPreSaveValidation();
					AssertEquals(string.Format("Job {0} should not have errors. Errors: {1}", job.JH_JobNum, job.NotificationsIncludingChildren.ToUniqueMessageListString()), false, job.HasErrors);
				}

				ConsolInvoicingPostManagerGUIWrapper postManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);

				postManagerWrapper.PostManager_ForTestOnly.OnNothingPosted += new EventHandler(PostManager_OnNothingPosted);
				postManagerWrapper.Post();

				var invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST1234").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				var query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, string.Format("AP Payment {0}", Consol.JK_UniqueConsignRef));
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.Cash);
				query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				var payment = Factory.LoadTop1<APPayment>(query);
				var paymentApprovalBases = Factory.Load<PaymentApprovalBase>(new ZQuery());

				AssertEquals(1, paymentApprovalBases.Length);
				AssertEquals(approvalStatus, paymentApprovalBases[0].AV_Status);
			}
		}

		ForwardingShipment CreateSampleShipment(ForwardingConsol consol, ForwardingShipment parent)
		{
			var shipment = TestObjectCreator.CreateShipment("S" + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8), "AUMEL", "NZAKL", consol);
			shipment.JS_PackingMode = "BCN";
			shipment.JS_ShipmentType = "ASM";
			if (parent != null)
			{
				shipment.JS_JS_ColoadMasterShipment = parent.PK;
			}
			return shipment;
		}

		void PrepairApportionSplitChargForTest(JobConsolCost jobConsolCost, ForwardingShipment shipment, decimal oSCostAmt, ZBool isUsedForApportionment)
		{
			var chargeShipment = jobConsolCost.ApportionmentCharges.FindChargeForJob(shipment);
			chargeShipment.JR_GB = GlbBranch.CurrentBranch.PK;
			chargeShipment.JR_GE = GlbDepartment.CurrentDepartment.PK;
			chargeShipment.JR_OSCostAmt = oSCostAmt;
			chargeShipment.JR_IsUsedForApportionment = isUsedForApportionment;
			chargeShipment.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
		}

		public void TestConsolPreviewInvoices()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsDebtor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			TestObjectCreator.AALSHI.OH_IsForwarder = true;
			var port1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port1.O5_PortOrCountry = TestObjectCreator.AALSHI.OH_RL_NKClosestPort;
			port1.O5_IsHandlesAirAgent = ZBool.True;
			port1.O5_AgentDirection = "BTH";
			TestObjectCreator.AALSHI.AppointedAgentPorts.Add(port1);

			TestObjectCreator.ZECTRA.OH_IsForwarder = true;
			var port2 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port2.O5_PortOrCountry = TestObjectCreator.ZECTRA.OH_RL_NKClosestPort;
			port2.O5_IsHandlesAirAgent = ZBool.True;
			port2.O5_AgentDirection = "BTH";
			TestObjectCreator.ZECTRA.AppointedAgentPorts.Add(port2);

			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ZECTRA.MainAddress.PK;

			var shipment = TestObjectCreator.CreateShipment("S000001", "", "", consol);
			shipment.JS_ReleaseType = "SWB";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK; // This is necessary to avoid loading Charge instances by Validation in newFactory
			job.JH_OA_AgentCollectAddr = TestObjectCreator.ZECTRA.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var apportionmentListing = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.ZECTRA, apportionmentListing);
			cost.E6_IsForCollectInvoice = true;
			cost.E6_InvoiceNum = "Test";
			cost.E6_OSCostAmount = 100m;
			cost.E6_LocalCostAmount = 100m;
			cost.E6_PPDCLT = "ALL";
			cost.E6_ApportionToRelatedShipments = true;
			cost.E6_InvoiceDate = DateTime.Today;
			cost.E6_PaymentDate = DateTime.Today;

			Factory.Save();

			job.Charges[0].JR_LocalSellAmt = 80m;
			job.Charges[0].JR_OSSellAmt = 80m;
			job.Charges[0].JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;

			AssertNoErrors(job);
			AssertNoErrors(shipment);
			AssertNoErrors(consol);

			Factory.Save();

			var jobsQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
			Jobs.Load(jobsQuery);

			var wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.Cast<Job>(), consol, FormForTest, apportionmentListing);
			wrapper.SupressProfitSharePrintTask = true;
			wrapper.AllowedReportDeliveryOption = AllowedDeliveryOptions.PreviewOnly;
			wrapper.Preview();

			using (var previewForm = ZFormModaliser.ActiveForm as InvoicePreviewForm)
			{
				AssertNotNull("Preview form shown", previewForm);
				var invoices = ((InvoicesPreviewer)previewForm.BusinessEntity).PreviewInvoices;
				AssertEquals("Should have 2 invoices.", 2, invoices.Count);
				AssertType<ARCreditNote>("Should be AR type.", invoices[0]);
				AssertEquals(TransactionTypes.CreditNote, invoices[0].AH_TransactionType);
				AssertEquals("Should not have any error", false, invoices[0].HasErrors);
				AssertEquals("Should be 80 - 100 = -20.", -20m, invoices[0].AH_InvoiceAmount);
				AssertType<APInvoice>("Should be AP type.", invoices[1]);
				AssertEquals("Should be 0.", 0m, invoices[1].AH_InvoiceAmount);
			}
		}

		public void TestInvoiceCurrencyIsLocalWhenConsolCostsHaveMixedCurrencies()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;
			var exchngRate = TestObjectCreator.USD.ExchangeRates.AddNew();
			exchngRate.RE_StartDate = DateTime.Today.AddDays(-1);
			exchngRate.RE_ExpiryDate = DateTime.Today.AddDays(30);
			exchngRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			exchngRate.RE_SellRate = 3.0M;

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			var shipment1 = TestObjectCreator.CreateShipment("S000001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.AALSHI, 1.0M, TestObjectCreator.ZECTRA, 1.0M);

			var shipment2 = TestObjectCreator.CreateShipment("S000002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.AALSHI, 1.0M, TestObjectCreator.ZECTRA, 1.0M);

			var apportionmentListing = new ApportionmentListing(Factory, consol);

			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.ZECTRA, apportionmentListing);
			cost.E6_IsForCollectInvoice = true;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Manual;
			cost.E6_InvoiceNum = "Test";
			cost.E6_RX_NKCurrency = "USD";
			cost.E6_OSCostAmount = 300m;
			cost.E6_LocalCostAmount = 100m;
			cost.E6_PPDCLT = "ALL";
			cost.E6_ApportionToRelatedShipments = true;
			cost.E6_InvoiceDate = DateTime.Today;
			cost.E6_PaymentDate = DateTime.Today;

			var apportionedCharges = cost.ApportionmentCharges.Cast<ApportionSplitCharge>();
			apportionedCharges.First(x => x.JR_JH == job1.PK).JR_OSCostAmt = cost.E6_OSCostAmount * 0.5M;
			apportionedCharges.First(x => x.JR_JH == job2.PK).JR_OSCostAmt = cost.E6_OSCostAmount * 0.5M;
			cost.SetIsUsedForApportionment();

			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.ZECTRA, apportionmentListing);
			cost2.E6_IsForCollectInvoice = true;
			cost2.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Manual;
			cost2.E6_InvoiceNum = "Test";
			cost2.E6_OSCostAmount = 200m;
			cost2.E6_LocalCostAmount = 200m;
			cost2.E6_PPDCLT = "ALL";
			cost2.E6_ApportionToRelatedShipments = true;
			cost2.E6_InvoiceDate = DateTime.Today;
			cost2.E6_PaymentDate = DateTime.Today;
			cost2.E6_RX_NKCurrency = "AUD";

			apportionedCharges = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>();
			apportionedCharges.First(x => x.JR_JH == job1.PK).JR_OSCostAmt = cost2.E6_OSCostAmount;
			apportionedCharges.First(x => x.JR_JH == job2.PK).JR_OSCostAmt = 0M;
			cost2.SetIsUsedForApportionment();

			Factory.Save();

			//job order is important to reproduce the issue.
			var wrapper = new ConsolInvoicingPostManagerGUIWrapper_WithConstantJobOrder_ForTest(JobInvoicingPostingOption.All, Factory, new Job[] { job1, job2 }, consol, FormForTest, apportionmentListing);
			wrapper.Post();

			AssertEquals("Charge Count: Job1", 2, job1.Charges.Count);
			AssertEquals("All Charges should be Apportioned: Job1", true, job1.Charges.Cast<JobCharge>().All(x => x.JR_IsApportioned));

			AssertEquals("Charge Count: Job1", 1, job2.Charges.Count);
			AssertEquals("All Charges should be Apportioned: Job1", true, job2.Charges.Cast<JobCharge>().All(x => x.JR_IsApportioned));

			var invoices = Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "Test").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Invoice Count", 1, invoices.Length);

			var invoice = invoices[0];
			var lines = invoices[0].Lines.Cast<APInvoiceLine>();

			AssertEquals("Invoice Count", "AUD", invoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Invoice Count", true, lines.Where(x => x.AL_AC == TestObjectCreator.CC1.PK).All(x => x.AL_RX_NKTransactionCurrency == "USD"));
			AssertEquals("Invoice Count", true, lines.Where(x => x.AL_AC == TestObjectCreator.CC2.PK).All(x => x.AL_RX_NKTransactionCurrency == "AUD"));
		}

		public void TestJobOnHoldMessage()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol();
			var listing = new ApportionmentListing(Factory, consol);
			var shipment1 = objectCreator.CreateShipment("S00001", consol);
			var shipment2 = objectCreator.CreateShipment("S00002", consol);
			var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, 300m, objectCreator.Creditor1, AllocationMethod.Shipment, listing);
			Factory.Save();

			var job1 = (Job)shipment1.Job;
			var job2 = (Job)shipment2.Job;

			AssertNotNull(job1);
			AssertNotNull(job2);
			job1.JH_OA_AgentCollectAddr = job2.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			job1.JH_OA_LocalChargesAddr = job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job1.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			Factory.Save();

			AssertEquals("Job 1 is on hold", JobHeaderStatus.WorkOnHold.Code, shipment1.Job.JH_Status);
			AssertEquals("Job 2 is not on hold", JobHeaderStatus.Working.Code, shipment2.Job.JH_Status);

			var postManager = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, new[] { job1, job2 }, consol, FormForTest, listing);
			var expectedMessageForJobOnHold = string.Format(@"The following jobs are on hold and their associated job charges will not be posted. 
Further, any consol costs with apportionment to these jobs will not be posted.

To post these jobs later, change the job status from 'WHL'.
 {0}
", job1.JH_JobNum);

			UnitTestUserNotification.Instance.ClearMessages();

			postManager.Post();

			AssertEquals("AR Invoice should be posted", string.Format("Do you want to print invoice {0}?", job2.JH_JobNum), UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Job on hold warning message should be shown", UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.WasWarning && x.Text == expectedMessageForJobOnHold));
		}

		public void TestShouldNotCreateDuplicateChargeWhenPostingOverseaAgentChargesFromConsol()
		 => AssertShouldNotCreateDuplicateCharge(JobInvoicingPostingOption.Agent);

		public void TestShouldNotCreateDuplicateChargeWhenPostingWholeConsol()
		 => AssertShouldNotCreateDuplicateCharge(JobInvoicingPostingOption.All);

		void AssertShouldNotCreateDuplicateCharge(JobInvoicingPostingOption option)
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRate_ForTestOnly(0, 100, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			rate.Factory.Save();

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.Agent.PK;
			var profitShare = agentRelationship.ProfitShareDetails.AddNew();
			profitShare.O4_FreightMode = "AIR";
			profitShare.O4_StartDate = ZDateTime.Today.AddMonths(-1);
			profitShare.O4_EndDate = ZDateTime.Today.AddMonths(1);
			profitShare.O4_SendingPortOrCountry = "AUSYD";
			profitShare.O4_ReceivingPortOrCountry = "USLAX";
			var sendParty = profitShare.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 50m;
			var rcvParty = profitShare.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 50m;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.Agent.PK;
			consol.JK_UniqueConsignRef = "CTest";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Factory.Save();

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = TestObjectCreator.FEADepartment.PK;
			job.PlugInData = shipment;
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_LocalSellAmt = 600m;
			charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			charge.JR_LocalCostAmt = 300m;
			charge.JR_AT_SellGSTRate = rate.PK;
			Factory.Save();

			var jobs = new[] { job };
			var apps = new ApportionmentListing(Factory, consol);

			var postManagerWrapper = new ConsolInvoicingPostManagerGUIWrapper_WithEmptySetupEvents_ForTest(option, Factory, jobs, consol, FormForTest, apps);
			var postManager = postManagerWrapper.PostManager_ForTestOnly as ConsolInvoicingPostManager;
			postManager.ExportAgentPosting += new ExportAgentPostingEventHandler((sender, e) =>
			{
				e.OptionSelector.Currency = TestObjectCreator.USD.RX_Code;
				e.OptionSelector.ExchangeRate = 1.5m;
				e.OptionSelector.UpdateAllChargesPostingStyleAccordingToSelectedCurrency();
			});

			postManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler((sender, e) => { return true; });
			postManagerWrapper.Post();
			Factory.Save();

			var jobReload = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.PK, job.PK));
			var profitShareCharges = jobReload.Charges.Where(x => x.ChargeCode.PK.ToGuid() == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			AssertEquals(1, profitShareCharges.Count());
			var profitShareCharge = profitShareCharges.First();
			AssertEquals(-150m, profitShareCharge.JR_LocalSellAmt);
			AssertEquals(charge.JR_OH_SellAccount, profitShareCharge.JR_OH_SellAccount);
			AssertEquals("USD", profitShareCharge.JR_RX_NKSellCurrency);
		}

		#region NothingPostedHandler

		protected override PostManagerGUIWrapper PrepareTestDataForNothingPostedHandler()
		{
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			var apps = new ApportionmentListing(Factory, consol);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 100m, TestObjectCreator.Agent);

			Factory.Save();

			var shipmentJob = (Job)shipment.Job;
			var apportionedCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault();
			apportionedCharge.JR_LocalSellAmt = 0m;
			apportionedCharge.JR_OSSellAmt = 0m;
			Factory.Save();

			var wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, new[] { shipmentJob }, consol, FormForTest, apps);
			wrapper.DoTestPostTransactions = true;

			return wrapper;
		}

		protected override void AssertNothingPostedHandlerCore(PostManagerGUIWrapper wrapper)
		{
			wrapper.Post();

			var expectedNothingToPostMessage = GetExpectedMessageForNothingPosted();
			AssertContains(expectedNothingToPostMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override string GetExpectedMessageForNothingPosted()
		{
			var expectedZeroValueMessage = AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value
				? string.Empty
				: System.Environment.NewLine + "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices.";

			return $@"No appropriate charges were found for posting. This may be because:{expectedZeroValueMessage}
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).";
		}

		#endregion

		class ConsolInvoicingPostManagerGUIWrapper_WithConstantJobOrder_ForTest : ConsolInvoicingPostManagerGUIWrapper
		{
			public ConsolInvoicingPostManagerGUIWrapper_WithConstantJobOrder_ForTest(JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, Form parentForm, ApportionmentListing consolCostListing)
						: base(postingOption, plugInFactory, jobs, consol, parentForm, consolCostListing)
			{
				//This order by is done to reproduce the scenario reliably which is tested in 'TestInvoiceCurrencyIsLocalWhenConsolCostsHaveMixedCurrencies'.
				var result = this.Jobs.OrderBy(x => x.JH_JobNum);
				this.Jobs = result;
			}
		}

		class ConsolInvoicingPostManagerGUIWrapper_WithEmptySetupEvents_ForTest : ConsolInvoicingPostManagerGUIWrapper
		{
			public ConsolInvoicingPostManagerGUIWrapper_WithEmptySetupEvents_ForTest(JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, Form parentForm, ApportionmentListing consolCostListing)
						: base(postingOption, plugInFactory, jobs, consol, parentForm, consolCostListing)
			{
			}

			protected override void SetUpEvents()
			{
			}
		}
	}
}
