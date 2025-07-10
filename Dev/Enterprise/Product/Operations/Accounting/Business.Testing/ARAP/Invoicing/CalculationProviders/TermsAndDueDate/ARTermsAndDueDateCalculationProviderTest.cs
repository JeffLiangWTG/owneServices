using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class ARTermsAndDueDateCalculationProviderTest : TermsAndDueDateCalculationProviderTest
	{
		protected override TermsAndDueDateCalculationProvider GetCalculationProvider(Invoice invoice)
		{
			return new ARTermsAndDueDateCalculationProvider(invoice);
		}

		protected override Invoice GetInvoice()
		{
			return Factory.New<ARInvoice>();
		}

		protected override void SetupOrganisation()
		{
			base.SetupOrganisation();
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Enterprise.Core.Constants.InvoiceTerms.CashOnDelivery;
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 0;
		}

		public override void TestSetInvoiceTerms()
		{
			CalculationProvider.SetInvoiceTermsAndDays();
			AssertEquals("Invoice Terms", Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm, Invoice.AH_InvoiceTerm);
			AssertEquals("Invoice Term Days", Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays, Invoice.AH_InvoiceTermDays);
		}

		public void TestSetDisbursementInvoiceTerms()
		{
			ZDateTime invoiceDate = ZDateTime.Now.AddDays(5);
			Invoice.AH_InvoiceDate = invoiceDate;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			OrgARTerms dsbTerm = Organisation.CompanyData.CreateOrLoadDisbursementARTerm();
			dsbTerm.PY_InvoiceTerm = Enterprise.Core.Constants.InvoiceTerms.FromInvoiceDate;
			dsbTerm.PY_InvoiceDays = 7;
			CalculationProvider.SetInvoiceTermsAndDays();
			AssertEquals("Disbursement Invoice Terms", Core.Constants.InvoiceTerms.FromInvoiceDate, Invoice.AH_InvoiceTerm);
			AssertEquals("Disbursement Invoice Term Days", (short)7, Invoice.AH_InvoiceTermDays);
		}

		public void TestSetInvoiceTermsForAnotherInvoiceType()
		{
			ZDateTime invoiceDate = ZDateTime.Now.AddDays(5);
			Invoice.AH_InvoiceDate = invoiceDate;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			OrgARTerms dsbTerm = Organisation.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching);
			dsbTerm.PY_InvoiceTerm = Enterprise.Core.Constants.InvoiceTerms.FromInvoiceDate;
			dsbTerm.PY_InvoiceDays = 7;
			CalculationProvider.SetInvoiceTermsAndDays();
			AssertEquals("Disbursement Invoice Terms", Core.Constants.InvoiceTerms.FromInvoiceDate, Invoice.AH_InvoiceTerm);
			AssertEquals("Disbursement Invoice Term Days", (short)7, Invoice.AH_InvoiceTermDays);
		}

		[TestDate(2010, 11, 10)]
		public void TestCalculateDueDateForMonthInvoiceCycleInvoiceTerm()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "ORG";

			Invoice.AH_OH = organisation.PK;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Invoice.AH_InvoiceDate = new ZDateTime(2010, 11, 5);
			Invoice.AH_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			Invoice.AH_InvoiceTermDays = 1;
			CalculationProvider.CalculateDueDate();
			AssertEquals("Due Date", ZDateTime.Now, Invoice.AH_DueDate.Date);

			organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 3;
			organisation.CompanyData.LoadARTermForAllInvoiceTypes().ARTermsCycles.AddNew().P5_PaymentDay = 14;

			organisation.CompanyData.CreateOrLoadDisbursementARTerm();
			CalculationProvider.CalculateDueDate();
			AssertEquals("Due Date", new ZDateTime(2011, 01, 14), Invoice.AH_DueDate.Date);
		}

		[TestDate(2010, 11, 10)]
		public void TestCalculateDueDateForTermDaysAndDebtorPaymentCycleInvoiceTerm()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "ORG";

			Invoice.AH_OH = organisation.PK;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Invoice.AH_InvoiceDate = new ZDateTime(2010, 11, 5);
			Invoice.AH_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			Invoice.AH_InvoiceTermDays = 1;
			CalculationProvider.CalculateDueDate();
			AssertEquals("Due Date", ZDateTime.Now, Invoice.AH_DueDate.Date);

			organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 3;
			organisation.CompanyData.LoadARTermForAllInvoiceTypes().ARTermsCycles.AddNew().P5_PaymentDay = 14;

			organisation.CompanyData.CreateOrLoadDisbursementARTerm();
			Invoice.AH_InvoiceTermDays = 3;
			CalculationProvider.CalculateDueDate();
			AssertEquals("Due Date", new ZDateTime(2010, 11, 14), Invoice.AH_DueDate.Date);
		}

		[TestDate(2010, 11, 27)]
		public void TestCalculateDueDateForTermDaysAndDebtorPaymentCycleInvoiceTerm25()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "ORG";

			var term = organisation.CompanyData.LoadARTermForAllInvoiceTypes();
			term.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			term.PY_InvoiceDays = 30;
			organisation.CompanyData.LoadARTermForAllInvoiceTypes().ARTermsCycles.AddNew().P5_PaymentDay = 25;

			Invoice.AH_OH = organisation.PK;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Invoice.AH_InvoiceDate = new ZDateTime(2010, 11, 27);
			Invoice.AH_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			Invoice.AH_InvoiceTermDays = 30;

			organisation.CompanyData.CreateOrLoadDisbursementARTerm();
			CalculationProvider.CalculateDueDate();
			AssertEquals("Due Date", new ZDateTime(2011, 1, 25), Invoice.AH_DueDate.Date);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_NoApprovalRequestCreated_NoOrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(false, false, true);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_NoApprovalRequestCreated_OrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(false, false, false);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_PendingApproval_NoOrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, false, true);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_PendingApproval_OrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, false, false);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_CreditRequestApproved_NoOrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, true, true);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_CreditRequestApproved_OrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, true, false);
		}

		void SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(bool createApprovalRequest, bool isApproved, bool overrideOrgExemptionForCreditCheck)
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromInvoiceDate.Code;
			onHoldTerms.TermDays = 7;

			var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();

			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
			{
				if (overrideOrgExemptionForCreditCheck)
				{
					AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);
				}

				var chargeCode = TestObjectCreator.FRT;
				var debtor = TestObjectCreator.Debtor;
				var shipment = TestObjectCreator.CreateShipment("S001001", false);
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge = TestObjectCreator.CreateCharge(job, chargeCode, "freight", TestObjectCreator.AUD, 100M, TestObjectCreator.LocalClient, TestObjectCreator.AUD, 120M, debtor);

				if (createApprovalRequest)
				{
					var factory = new BusinessObjectFactory();
					var approvalRequest = factory.New<CreditControlledDocumentsApproval>();
					approvalRequest.Initialize(shipment, ZGuid.Empty, new int[] { 3 });
					if (isApproved)
					{
						approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
					}
					factory.Save();
				}

				var invoiceDate = ZDateTime.Today;
				var postDate = ZDateTime.Today;
				var dueDate = ZDateTime.Today.AddDays(30);

				Invoice invoice = (Invoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.AUD, 1M, 120M, 0M, 120M, 0M, debtor, chargeCode.PK, postDate, dueDate, invoiceDate, true);
				invoice.AH_JH = job.PK;
				charge.JR_AL_ARLine = invoice.Lines[0].PK;

				Factory.Save();

				var calculationProvider = GetCalculationProvider(invoice);
				calculationProvider.SetInvoiceTermsAndDays();

				CombineAssertions("Should get a ARTerm from the default configuration when Use ARInvoice Terms And Term Days When Credit Is OnHold is false.", () =>
				{
					AssertEquals("Default Use ARInvoice Terms And Term Days When Credit Is OnHold", false, OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.Value);
					AssertEquals("Default Invoice terms", ARInvoiceTermsList.CashOnDelivery.Code, invoice.AH_InvoiceTerm);
					AssertEquals("Default Term days", (ZByte)0, invoice.AH_InvoiceTermDays);
					AssertEquals("Due date", invoiceDate, invoice.AH_DueDate);
				});
			}
		}

		public override void TestCanInvoiceTermBeSelectedError()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "ORG";

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Invoice.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;

			organisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			organisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 3;
			organisation.CompanyData.CreateOrLoadDisbursementARTerm().ARTermsCycles.AddNew().P5_PaymentDay = 14;

			Invoice.AH_OH = ZGuid.Empty;
			AssertEquals("", CalculationProvider.CanInvoiceTermBeSelectedError);

			Invoice.AH_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertEquals(TermsAndDueDateCalculationProvider.MonthsFromInvoiceCycleDateError, CalculationProvider.CanInvoiceTermBeSelectedError);

			Invoice.AH_OH = organisation.PK;
			AssertEquals("", CalculationProvider.CanInvoiceTermBeSelectedError);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Invoice.AH_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertEquals(TermsAndDueDateCalculationProvider.MonthsFromInvoiceCycleDateError, CalculationProvider.CanInvoiceTermBeSelectedError);
		}

		public void TestCallTimesforCalculateDueDateWhenSetInvoiceTermsAndDays()
		{
			var arInvoice_ForTestOnly = Factory.New<ARInvoice_ForTestOnly>();
			arInvoice_ForTestOnly.AH_OH = TestObjectCreator.AALSHI.PK;
			arInvoice_ForTestOnly.Counter = 0;
			AssertEquals("Pre-condition: the counter should be reset to 0.", 0, arInvoice_ForTestOnly.Counter);
			arInvoice_ForTestOnly.TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();
			AssertEquals("The counter should be 1 as we suspend for set AH_DueDate in method SetInvoiceTermsAndDays. Please check 'CalculateDueDateSuspender' in file 'TermsAndDueDateCalculationProvider'.", 1, arInvoice_ForTestOnly.Counter);
		}

		class ARInvoice_ForTestOnly : ARInvoice
		{
			public ARInvoice_ForTestOnly(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public int Counter { get; set; }

			public override ZDateTime AH_DueDate
			{
				get { return base.AH_DueDate; }
				set
				{
					base.AH_DueDate = value;
					Counter++;
				}
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
