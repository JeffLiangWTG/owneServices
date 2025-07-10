using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	[TestedType(typeof(PeriodicInvoiceChargePoster))]
	public class PeriodicInvoiceChargePosterTest : ChargePosterTest
	{
		public override void TestPostInvoiceForChargesWithMultipleForeignCurrencies()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PeriodicInvoiceChargePoster(new PeriodicInvoice(Factory));
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_internal ?? (TestObjectCreator_internal = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_internal;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestSetOSAndLocalValuesCorrectlyWhenPostingForeignCurrency()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsReciprocal = !GlbCompany.CurrentCompany.GC_IsReciprocal;
			Factory.Save();

			var currentContext = Env.CurrentUserContext;
			Env.ClearUserContext();
			Env.SetUserContext(currentContext);
			GlbCompany.CurrentCompany.GC_IsReciprocal = Env.CurrentCompany.IsReciprocal;

			Job job1 = TestObjectCreator.CreateJob("Z00001003", TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10);
			Job job2 = TestObjectCreator.CreateJob("Z00001003", TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10);

			ExchangeRate job1Rate = job1.ExchangeRates.AddNew();
			job1Rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			job1Rate.JF_BaseRate = 1.5821;

			ExchangeRate job2Rate = job2.ExchangeRates.AddNew();
			job2Rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			job2Rate.JF_BaseRate = 1.6;

			Charge charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.USD, 172M, TestObjectCreator.ABIGAS);
			Charge charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.USD, 175M, TestObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = "CUD";
			charge2.JR_InvoiceType = "CUD";

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
			periodicInvoice.InvoiceType = "CUD";
			periodicInvoice.Charges.Add(charge1);
			periodicInvoice.Charges.Add(charge2);

			IReceivablesPostingChargeCollection chargesCollection = new IReceivablesPostingChargeCollection();
			chargesCollection.Add(charge1);
			chargesCollection.Add(charge2);
			PostingChargeKey key = new PostingChargeKey(TestObjectCreator.ABIGAS.PK, "CUD", ZGuid.Empty, ZGuid.Empty, 0);
			chargesCollection.Key = key;

			PeriodicInvoiceChargePoster poster = new PeriodicInvoiceChargePoster(periodicInvoice);
			poster.Post(chargesCollection);
			Assert(poster.PostedInvoices.Count > 0);
			InvoicingBase invoice = poster.PostedInvoices[0];
			AssertNotNull(invoice);
			Assert(invoice is ARInvoice);
			AssertEquals(347m, invoice.AH_OSExTaxAmount);
		}

		public void TestPostedInvoiceType()
		{
			Job job = TestObjectCreator.CreateJob("Z00001003", TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			job.JH_UniqueJobInvoiceNumber = (ZShort)6;

			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.AUD, -300M, TestObjectCreator.ABIGAS);

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OSExTaxAmount = 200M;
			periodicInvoice.MiscInvoices.Add(invoice);
			invoice = Factory.New<ARInvoice>();
			invoice.AH_OSExTaxAmount = 200M;
			periodicInvoice.MiscInvoices.Add(invoice);

			AssertEquals(typeof(ARInvoice), new PeriodicInvoiceChargePoster(periodicInvoice).Post(charge).GetType());

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.AUD, 300M, TestObjectCreator.ABIGAS);
			periodicInvoice = new PeriodicInvoice(Factory);
			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_OSExTaxAmount = 200M;
			periodicInvoice.MiscInvoices.Add(creditNote);
			creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_OSExTaxAmount = 200M;
			periodicInvoice.MiscInvoices.Add(creditNote);

			AssertEquals(typeof(ARCreditNote), new PeriodicInvoiceChargePoster(periodicInvoice).Post(charge).GetType());
		}

		public void TestPostedInvoiceTypeCountOnOtherTaxes()
		{
			Job job = TestObjectCreator.CreateJob("Z00001003", TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			job.JH_UniqueJobInvoiceNumber = (ZShort)6;

			AssertEquals(typeof(ARCreditNote), GetPostedInvoiceType(0));
			AssertEquals(typeof(ARInvoice), GetPostedInvoiceType(100));

			Type GetPostedInvoiceType(ZDecimal localTaxOtherTaxes)
			{
				Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.AUD, -300M, TestObjectCreator.ABIGAS);

				PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
				ARInvoice invoice = Factory.New<ARInvoice>();
				invoice.AH_OSExTaxAmount = 250;
				invoice.AH_LocalTaxAmountOtherTaxes = localTaxOtherTaxes;
				AssertEquals("Precondition: AH_LocalTotal", 250m + localTaxOtherTaxes, invoice.AH_LocalTotal);
				periodicInvoice.MiscInvoices.Add(invoice);

				return new PeriodicInvoiceChargePoster(periodicInvoice).Post(charge).GetType();
			}
		}

		[TestDate(2020, 10, 23)]
		public void TestPostedInvoiceLineTaxDate_UseInvoiceDate()
		{
			AssertPostedInvoiceLineTaxDate(TaxDateDefaultingOption.Code.InvoiceDate);
		}

		[TestDate(2020, 10, 23)]
		public void TestPostedInvoiceLineTaxDate_UseTodayDate()
		{
			AssertPostedInvoiceLineTaxDate(TaxDateDefaultingOption.Code.Today);
		}

		[TestDate(2020, 10, 23)]
		public void TestPostedInvoiceLineTaxDate_UseJobDate()
		{
			AssertPostedInvoiceLineTaxDate(TaxDateDefaultingOption.Code.EstimatedArrivalDate);
		}

		public void AssertPostedInvoiceLineTaxDate(string taxDateDefaultingOption)
		{
			var shipment = TestObjectCreator.CreateShipment("S00001003");
			shipment.JS_E_ARV = new ZDateTime(2020, 10, 15);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			job.JH_UniqueJobInvoiceNumber = 6;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.AUD, 300M, TestObjectCreator.ABIGAS);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.InvoiceDate = new ZDateTime(2020, 09, 30);
			periodicInvoice.Charges.Add(charge);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AR";
			taxDateOption.TaxDateOption = taxDateDefaultingOption;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var arInvoice = new PeriodicInvoiceChargePoster(periodicInvoice).Post(charge);
				AssertEquals(typeof(ARInvoice), arInvoice.GetType());
				switch (taxDateDefaultingOption)
				{
					case TaxDateDefaultingOption.Code.InvoiceDate:
						AssertEquals("Should use invoice date", new ZDate(2020, 09, 30), arInvoice.Lines[0].AL_TaxDate);
						break;
					case TaxDateDefaultingOption.Code.Today:
						AssertEquals("Should use today's date", new ZDate(2020, 10, 23), arInvoice.Lines[0].AL_TaxDate);
						break;
					case TaxDateDefaultingOption.Code.EstimatedArrivalDate:
						AssertEquals("Should use job's operational date", new ZDate(2020, 10, 15), arInvoice.Lines[0].AL_TaxDate);
						break;
					default:
						Assert("Invalid tax date default option", false);
						break;
				}
			}
		}

		public void TestPostedInvoiceLineAmountSigns()
		{
			Job job = TestObjectCreator.CreateJob("Z00001003", TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = (ZShort)6;

			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.AUD, -300M, TestObjectCreator.ABIGAS);

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.Charges.Add(charge);

			var arCreditNote = new PeriodicInvoiceChargePoster(periodicInvoice).Post(charge);
			AssertEquals(typeof(ARCreditNote), arCreditNote.GetType());
			AssertEquals(-300M, arCreditNote.Lines[0].AL_LineAmount);
			AssertEquals(300M, arCreditNote.Lines[0].AL_LocalExTaxAmount);

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.AUD, 300M, TestObjectCreator.ABIGAS);
			periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.Charges.Add(charge);

			var arInvoice = new PeriodicInvoiceChargePoster(periodicInvoice).Post(charge);
			AssertEquals(typeof(ARInvoice), arInvoice.GetType());
			AssertEquals(300M, arInvoice.Lines[0].AL_LineAmount);
			AssertEquals(300M, arInvoice.Lines[0].AL_LocalExTaxAmount);
		}

		public void TestConsolidatedInvoiceRefIsEmpty()
		{
			Job job = TestObjectCreator.CreateJob("Z00001003", TestObjectCreator.ABIGAS, 10, TestObjectCreator.ZECTRA, 10M);
			job.JH_UniqueJobInvoiceNumber = 6;

			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "Test Charge", null, 0, null, TestObjectCreator.AUD, -300M, TestObjectCreator.ABIGAS);

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OSExTaxAmount = 200M;
			periodicInvoice.MiscInvoices.Add(invoice);
			invoice = Factory.New<ARInvoice>();
			invoice.AH_OSExTaxAmount = 200M;
			periodicInvoice.MiscInvoices.Add(invoice);

			Assert(new PeriodicInvoiceChargePoster(periodicInvoice).Post(charge).AH_ConsolidatedInvoiceRef.IsEmpty);
		}

		public void TestSetInvoiceHeaderDetails()
		{
			GlbBranch otherBranch = TestObjectCreator.CreateBranch("XYZ", GlbCompany.CurrentCompany);

			Job job1 = TestObjectCreator.CreateJob("Z00001003", TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			job1.JH_UniqueJobInvoiceNumber = (ZShort)6;
			job1.JH_GB = otherBranch.PK;
			job1.JH_GE = TestDepartment.PK;

			Job job2 = TestObjectCreator.CreateJob("Z00001004", TestObjectCreator.ABIGAS, 20M, TestObjectCreator.ZECTRA, 20M);
			job2.JH_UniqueJobInvoiceNumber = (ZShort)6;
			job2.JH_GB = otherBranch.PK;
			job2.JH_GE = TestDepartment.PK;

			Charge charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.MRG100, "Test Charge 1", null, 0, null, TestObjectCreator.AUD, -100M, TestObjectCreator.ABIGAS);
			Charge charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.MRG100, "Test Charge 2", null, 0, null, TestObjectCreator.AUD, -200M, TestObjectCreator.ABIGAS);

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = otherBranch.PK;
			invoice.AH_GE = TestDepartment.PK;
			invoice.AH_OSExTaxAmount = 200M;
			periodicInvoice.MiscInvoices.Add(invoice);

			PeriodicInvoiceChargePoster poster = new PeriodicInvoiceChargePoster(periodicInvoice);
			InvoicingBase postedInvoice = poster.Post(charge1);

			AssertEquals("Should be CurrentBranch", GlbBranch.CurrentBranch.PK, postedInvoice.AH_GB);
			AssertEquals("Should be CurrentDepartment", GlbDepartment.CurrentDepartment.PK, postedInvoice.AH_GE);
			AssertEquals("AH_JH should be set to a job PK", job1.PK, postedInvoice.AH_JH);
			AssertEquals("Description should be as expected", "AR PERIODIC INVOICE", postedInvoice.AH_Desc);

			((IReversing)postedInvoice).GenerateReverseTransaction(true);
			charge1.ClearRevenueLink();
			poster = new PeriodicInvoiceChargePoster(periodicInvoice);
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(TestObjectCreator.ABIGAS.PK, TransactionTypes.Invoice, "Z00001003", ZGuid.Empty, ZGuid.Empty, 0);
			charges.Add(charge1);
			charges.Add(charge2);
			InvoicingBase postedCRNote = poster.Post(charges);

			AssertEquals("Should be CurrentBranch", GlbBranch.CurrentBranch.PK, postedCRNote.AH_GB);
			AssertEquals("Should be CurrentDepartment", GlbDepartment.CurrentDepartment.PK, postedCRNote.AH_GE);
			Assert("AH_JH should be empty", postedCRNote.AH_JH.IsEmpty);
			AssertEquals("Description should be as expected", "AR PERIODIC CREDIT NOTE", postedCRNote.AH_Desc);
		}

		public void TestPeriodicInvoice_PostBranchLevelPosting()
		{
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			OrgInvoiceType invoiceType = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "ALL";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;
			invoiceType.PI_RS_NKServiceLevel = "STD";

			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("S00001003");
			Job job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			job1.JH_UniqueJobInvoiceNumber = (ZShort)6;
			job1.JH_GB = branch1.PK;
			job1.JH_GE = TestDepartment.PK;

			var shipment2 = TestObjectCreator.CreateShipment("S00001004");
			Job job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.ABIGAS, 20M, TestObjectCreator.ZECTRA, 20M);
			job2.JH_UniqueJobInvoiceNumber = (ZShort)6;
			job2.JH_GB = branch2.PK;
			job2.JH_GE = TestDepartment.PK;

			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.MRG100, "Test Charge 1", null, 0, null, TestObjectCreator.AUD, -100M, TestObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.MRG100, "Test Charge 2", null, 0, null, TestObjectCreator.AUD, -200M, TestObjectCreator.ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;

			periodicInvoice.Charges.Add(charge1);
			periodicInvoice.Charges.Add(charge2);

			periodicInvoice.CreateTransactions();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoices = newFactory.Load<InvoicingBase>(new ZQuery());

			AssertEquals("2 Invoices should be created, one for each branch", 2, invoices.Length);
			var branch1Invoice = invoices.FirstOrDefault(x => x.AH_GB == branch1.PK);
			AssertNotNull(branch1Invoice);
			AssertEquals(branch1Invoice.AH_GB, branch1Invoice.Lines[0].AL_GB);

			var branch2Invoice = invoices.FirstOrDefault(x => x.AH_GB == branch2.PK);
			AssertNotNull(branch2Invoice);
			AssertEquals(branch2Invoice.AH_GB, branch2Invoice.Lines[0].AL_GB);
		}

		public void TestPeriodicInvoice_PostBranchLevelPosting_WithGroupedBranches()
		{
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			OrgInvoiceType invoiceType = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "ALL";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;
			invoiceType.PI_RS_NKServiceLevel = "STD";

			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("S00001003");
			Job job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			job1.JH_UniqueJobInvoiceNumber = (ZShort)6;
			job1.JH_GB = branch1.PK;
			job1.JH_GE = TestDepartment.PK;

			var shipment2 = TestObjectCreator.CreateShipment("S00001004");
			Job job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.ABIGAS, 20M, TestObjectCreator.ZECTRA, 20M);
			job2.JH_UniqueJobInvoiceNumber = (ZShort)6;
			job2.JH_GB = branch2.PK;
			job2.JH_GE = TestDepartment.PK;

			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.MRG100, "Test Charge 1", null, 0, null, TestObjectCreator.AUD, -100M, TestObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.MRG100, "Test Charge 2", null, 0, null, TestObjectCreator.AUD, -200M, TestObjectCreator.ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;

			periodicInvoice.Charges.Add(charge1);
			periodicInvoice.Charges.Add(charge2);

			periodicInvoice.CreateTransactions();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoices = newFactory.Load<InvoicingBase>(new ZQuery());

			AssertEquals("1 Invoices should be created as both the branches are in the same posting group", 1, invoices.Length);
			var branchInvoice = invoices.FirstOrDefault(x => x.AH_GB == branch2.PK); //as branch2 is set as the parent branch for the gourp
			AssertNotNull(branchInvoice);
			AssertEquals(2, branchInvoice.Lines.Count);
			AssertNotNull(branchInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_GB == branch1.PK));
			AssertNotNull(branchInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_GB == branch2.PK));
		}

		public void TestMiscPeriodicInvoice_PostBranchLevelPosting()
		{
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			OrgInvoiceType invoiceType = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "ALL";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;

			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.AUD, 1M, 200M, 0M, 200M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK);
			invoice1.AH_GB = branch1.PK;
			invoice1.AH_GE = TestDepartment.PK;
			invoice1.Lines[0].AL_GB = invoice1.AH_GB;

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001002", TestObjectCreator.AUD, 1M, 300M, 0M, 300M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK);
			invoice2.AH_GB = branch1.PK;
			invoice2.AH_GE = TestDepartment.PK;
			invoice2.Lines[0].AL_GB = invoice2.AH_GB;

			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001003", TestObjectCreator.AUD, 1M, 300M, 0M, 300M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK);
			invoice3.AH_GB = branch2.PK;
			invoice3.AH_GE = TestDepartment.PK;
			invoice3.Lines[0].AL_GB = invoice3.AH_GB;

			((IMatching)invoice1).CurrentMatchGroup.AddNew().AP_AH = invoice1.PK;
			((IMatching)invoice2).CurrentMatchGroup.AddNew().AP_AH = invoice2.PK;
			((IMatching)invoice3).CurrentMatchGroup.AddNew().AP_AH = invoice3.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(invoice1);
			TestObjectCreator.SetupMatchLinkMatchDate(invoice2);
			TestObjectCreator.SetupMatchLinkMatchDate(invoice3);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceTerm = periodicInvoice.InvoiceTerms_List[0].Code;
			periodicInvoice.InvoiceType = periodicInvoice.InvoiceTypeList[0].Code;
			periodicInvoice.DueDate = ZDateTime.UtcNow;

			periodicInvoice.MiscInvoices.Add(invoice1);
			periodicInvoice.MiscInvoices.Add(invoice2);
			periodicInvoice.MiscInvoices.Add(invoice3);

			periodicInvoice.CreateTransactions();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var invoices = newFactory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_IsCancelled, false)
							.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, InvoiceTypesList.Codes.FinalInvoice_Batching)
							.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertEquals("Count", 2, invoices.Length);
			var branch1Invoice = invoices.FirstOrDefault(x => x.AH_GB == branch1.PK);
			AssertNotNull(branch1Invoice);
			AssertEquals("2 lines in Branch 1 FID invoice", 2, branch1Invoice.Lines.Count);
			AssertEquals("Both lines have same branch", branch1Invoice.Lines[0].AL_GB, branch1Invoice.Lines[1].AL_GB);
			AssertEquals("Header branch matches with Line branch", branch1Invoice.AH_GB, branch1Invoice.Lines[0].AL_GB);

			var branch2Invoice = invoices.FirstOrDefault(x => x.AH_GB == branch2.PK);
			AssertNotNull(branch2Invoice);
			AssertEquals("Header branch matches with Line branch", branch2Invoice.AH_GB, branch2Invoice.Lines[0].AL_GB);
		}

		public void TestMiscPeriodicInvoice_PostBranchLevelPosting_WithGroupedBranches()
		{
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			OrgInvoiceType invoiceType = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "ALL";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;

			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.AUD, 1M, 200M, 0M, 200M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK);
			invoice1.AH_GB = branch1.PK;
			invoice1.AH_GE = TestDepartment.PK;
			invoice1.Lines[0].AL_GB = invoice1.AH_GB;

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001002", TestObjectCreator.AUD, 1M, 300M, 0M, 300M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK);
			invoice2.AH_GB = branch1.PK;
			invoice2.AH_GE = TestDepartment.PK;
			invoice2.Lines[0].AL_GB = invoice2.AH_GB;

			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001003", TestObjectCreator.AUD, 1M, 400M, 0M, 400M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK);
			invoice3.AH_GB = branch2.PK;
			invoice3.AH_GE = TestDepartment.PK;
			invoice3.Lines[0].AL_GB = invoice3.AH_GB;

			((IMatching)invoice1).CurrentMatchGroup.AddNew().AP_AH = invoice1.PK;
			((IMatching)invoice2).CurrentMatchGroup.AddNew().AP_AH = invoice2.PK;
			((IMatching)invoice3).CurrentMatchGroup.AddNew().AP_AH = invoice3.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(invoice1);
			TestObjectCreator.SetupMatchLinkMatchDate(invoice2);
			TestObjectCreator.SetupMatchLinkMatchDate(invoice3);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceTerm = periodicInvoice.InvoiceTerms_List[0].Code;
			periodicInvoice.InvoiceType = periodicInvoice.InvoiceTypeList[0].Code;
			periodicInvoice.DueDate = ZDateTime.UtcNow;

			periodicInvoice.MiscInvoices.Add(invoice1);
			periodicInvoice.MiscInvoices.Add(invoice2);
			periodicInvoice.MiscInvoices.Add(invoice3);

			periodicInvoice.CreateTransactions();
			AssertNoExceptionThrown(() => Factory.Save());

			var newFactory = new BusinessObjectFactory();

			var invoices = newFactory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_IsCancelled, false)
							.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, InvoiceTypesList.Codes.FinalInvoice_Batching)
							.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertEquals("Only one invoice should be posted as both line branches belong to the same group", 1, invoices.Length);
			var branchInvoice = invoices.FirstOrDefault(x => x.AH_GB == branch2.PK); //as branch2 is set as the parent branch for the gourp
			AssertNotNull(branchInvoice);
			AssertEquals("3 lines in Branch 1 FID invoice", 3, branchInvoice.Lines.Count);
		}

		protected override string ExpectedARInvoiceDescription
		{
			get { return "AR PERIODIC INVOICE"; }
		}

		protected override string ExpectedARCreditNoteDescription
		{
			get { return "AR PERIODIC CREDIT NOTE"; }
		}

		protected override ZGuid ExpectedBranchPK
		{
			get { return GlbBranch.CurrentBranch.PK; }
		}

		protected override ZGuid ExpectedDepartmentPK
		{
			get { return GlbDepartment.CurrentDepartment.PK; }
		}

		protected override void AssertBackDatePostingWithZeroLocalSellTaxAmountAndTaxRateChanged(string invoiceType, Decimal oSExTaxAmount, Decimal oSTaxAmount, Decimal whtTaxAmount, Decimal exChangeRate, bool isBillInLocalCurrency)
		{
			Assert("Backdating is not used in Periodic invoice", true);
		}
	}
}
