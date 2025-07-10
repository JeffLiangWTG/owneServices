using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.Testing.ComplianceReport.JPKV7M
{
	public abstract class JPKTestBase : TestCaseWithFactory
	{
		#region AR Transaction

		protected void SetupARTransactions()
		{
			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			Creator.CC4.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

			var shipment1 = Creator.CreateShipment("S0001", transportMode: "ROA");
			var job1 = Creator.CreateJob(shipment1, Creator.LocalClient, 0, Creator.Agent, 0);
			var charge1 = Creator.CreateCharge(job1, Creator.CC1, "Desc 1", localCurrency, 50m, null, localCurrency, 100m, Creator.AALSHI);
			charge1.JR_AT_SellGSTRate = LOWPTU.PK;

			var transport = Creator.CreateJobPlugIn(JobInvoicingConsumerTypes.TransportConsignment);
			var job2 = Creator.CreateJob(transport, Creator.DebtorDE, 10, Creator.ZECTRA, 10);
			var charge2 = Creator.CreateCharge(job2, Creator.CC2, "Desc 2", localCurrency, 53m, null, localCurrency, 123m, Creator.DebtorDE);
			charge2.JR_AT_SellGSTRate = MIDPTU.PK;
			var charge3 = Creator.CreateCharge(job2, Creator.CC3, "Desc 3", localCurrency, 283m, null, localCurrency, 321m, Creator.DebtorDE);
			charge3.JR_AT_SellGSTRate = PTU.PK;

			var shipment2 = Creator.CreateShipment("S0002", transportMode: "SEA");
			var job3 = Creator.CreateJob(shipment2, Creator.LocalClient, 0, Creator.Agent, 0);
			var charge4 = Creator.CreateCharge(job3, Creator.CC1, "Desc 4", localCurrency, 51m, null, localCurrency, 101m, Creator.DebtorDE);
			charge4.JR_AT_SellGSTRate = FREEPTU.PK;
			var charge6 = Creator.CreateCharge(job3, Creator.CC2, "Desc 6", localCurrency, 53m, null, localCurrency, 103m, DebtorCreditorAT);
			charge6.JR_AT_SellGSTRate = EXEMPT.PK;
			var charge7 = Creator.CreateCharge(job3, Creator.CC4, "Desc 7", localCurrency, 71m, null, localCurrency, 117m, DebtorCreditorAT);
			charge7.JR_AT_SellGSTRate = EXCLUDE.PK;
			var charge8 = Creator.CreateCharge(job3, Creator.CC3, "Desc 8", localCurrency, 57m, null, localCurrency, 107m, DebtorCreditorAT);
			charge8.JR_AT_SellGSTRate = EXCLUDE.PK;
			var charge9 = Creator.CreateCharge(job3, Creator.CC3, "Desc 9", localCurrency, 59m, null, localCurrency, 109m, DebtorCreditorPL);
			charge9.JR_AT_SellGSTRate = LOWPTU.PK;
			var charge10 = Creator.CreateCharge(job3, Creator.CC3, "Desc 10", localCurrency, 61m, null, localCurrency, 111m, OtherCompanyOrgProxy);
			charge10.JR_AT_SellGSTRate = PTU.PK;

			Factory.Save();

			ARInv1 = (ARInvoice)Creator.CreateInvoice(typeof(ARInvoice), "ARINV01", localCurrency, 1m, Creator.AALSHI);
			ARInv1.AH_Desc = "First AR Invoice";
			ARInv1.Lines.Add(Creator.CreateRevenueLine(charge1, ARInv1.PK));
			ARInv1.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
			ARInv1.AH_JH = job1.PK;

			ARInv2 = (ARInvoice)Creator.CreateInvoice(typeof(ARInvoice), "ARINV02", localCurrency, 1m, Creator.DebtorDE);
			ARInv2.AH_Desc = "Second AR Invoice";
			ARInv2.Lines.Add(Creator.CreateRevenueLine(charge2, ARInv2.PK));
			ARInv2.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-5);
			ARInv2.Lines.Add(Creator.CreateRevenueLine(charge3, ARInv2.PK));
			ARInv2.Lines[1].AL_TaxDate = ZDate.Today.AddDays(1);
			ARInv2.Lines.Add(Creator.CreateRevenueLine(charge4, ARInv2.PK));
			ARInv2.Lines[2].AL_TaxDate = ZDate.Today.AddDays(-1);

			var charge5 = Creator.CreateCharge(job2, Creator.CC1, "Desc 4", localCurrency, -59m, null, localCurrency, -127m, Creator.DebtorDE);
			charge5.JR_AT_SellGSTRate = PTU.PK;

			ARCrd = Creator.CreateARCreditNote("ARCRD", Creator.DebtorDE, localCurrency, charge5.JR_OSSellExRate, "AR Credit Note");
			var crdLine = Creator.CreateARCreditNoteLine(ARCrd, job2, Creator.CC1, 127m, localCurrency, charge5.JR_OSSellExRate, "Desc 5");
			crdLine.AL_AT = PTU.PK;
			crdLine.AL_TaxDate = ZDate.Today.AddDays(-1);
			ARCrd.AH_JH = job2.PK;
			charge5.JR_AL_ARLine = crdLine.PK;

			ARInv3 = (ARInvoice)Creator.CreateInvoice(typeof(ARInvoice), "ARINV03", localCurrency, 1m, DebtorCreditorAT);
			ARInv3.AH_Desc = "Third AR Invoice";
			ARInv3.Lines.Add(Creator.CreateRevenueLine(charge6, ARInv3.PK));
			ARInv3.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-2);
			ARInv3.Lines.Add(Creator.CreateRevenueLine(charge7, ARInv3.PK));
			ARInv3.Lines[1].AL_TaxDate = ZDate.Today;
			ARInv3.Lines.Add(Creator.CreateRevenueLine(charge8, ARInv3.PK));
			ARInv3.Lines[2].AL_TaxDate = ZDate.Today.AddDays(-3);
			ARInv3.AH_JH = job3.PK;

			ARInv4 = (ARInvoice)Creator.CreateInvoice(typeof(ARInvoice), "ARINV04", localCurrency, 1m, DebtorCreditorPL);
			ARInv4.AH_Desc = "Fourth AR Invoice";
			ARInv4.Lines.Add(Creator.CreateRevenueLine(charge9, ARInv4.PK));
			ARInv4.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
			ARInv4.AH_JH = job3.PK;

			ARInv5 = (ARInvoice)Creator.CreateInvoice(typeof(ARInvoice), "ARINV05", localCurrency, 1m, OtherCompanyOrgProxy);
			ARInv5.AH_Desc = "Fifth AR Invoice";
			ARInv5.Lines.Add(Creator.CreateRevenueLine(charge10, ARInv5.PK));
			ARInv5.Lines[0].AL_TaxDate = ARInv5.AH_InvoiceDate.Date;
			ARInv5.AH_JH = job3.PK;

			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(Report, ARInv1.Lines[0], sequence: 1);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv2.Lines[0], sequence: 2);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv2.Lines[1], sequence: 3);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv2.Lines[2], sequence: 4);
			Creator.CreateComplianceReportTransactionPivot(Report, ARCrd.Lines[0], sequence: 5);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv3.Lines[0], sequence: 6);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv3.Lines[1], sequence: 7);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv3.Lines[2], sequence: 8);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv4.Lines[0], sequence: 9);
			Creator.CreateComplianceReportTransactionPivot(Report, ARInv5.Lines[0], sequence: 10);
		}

		protected ARInvoice ARInv1 { get; set; }
		protected ARInvoice ARInv2 { get; set; }
		protected ARInvoice ARInv3 { get; set; }
		protected ARInvoice ARInv4 { get; set; }
		protected ARInvoice ARInv5 { get; set; }
		protected ARCreditNote ARCrd { get; set; }

		#endregion

		#region AP Transactions

		protected void SetupAPTransactions()
		{
			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			Creator.CC4.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			Creator.CC5.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;
			Creator.CC6.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

			Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Australia, "ABN", "61432101235");

			var shipment1 = Creator.CreateShipment("S00001", transportMode: "ROA");
			var job1 = Creator.CreateJob(shipment1, Creator.LocalClient, 0, Creator.Agent, 0);
			var charge1 = Creator.CreateCharge(job1, Creator.CC4, "Desc 1 Goods", localCurrency, 50m, Creator.AALSHI, localCurrency, 100m, null);
			charge1.JR_AT_CostGSTRate = PTUREV.PK;
			var charge2 = Creator.CreateCharge(job1, Creator.CC5, "Desc 2 Service", localCurrency, 41m, Creator.AALSHI, localCurrency, 81m, null);
			charge2.JR_AT_CostGSTRate = PTUREV.PK;

			var transport = Creator.CreateJobPlugIn(JobInvoicingConsumerTypes.TransportConsignment);
			var job2 = Creator.CreateJob(transport, Creator.DebtorDE, 10, Creator.ZECTRA, 10);
			var charge3 = Creator.CreateCharge(job2, Creator.CC5, "Desc 3 Service", localCurrency, 53m, Creator.Creditor1, localCurrency, 123m, null);
			charge3.JR_AT_CostGSTRate = MIDPTUREV.PK;
			var charge4 = Creator.CreateCharge(job2, Creator.CC6, "Desc 4 Goods", localCurrency, 283m, Creator.Creditor1, localCurrency, 321m, null);
			charge4.JR_AT_CostGSTRate = PTUREV.PK;

			var shipment2 = Creator.CreateShipment("S00002", transportMode: "SEA");
			var job3 = Creator.CreateJob(shipment2, Creator.LocalClient, 0, Creator.Agent, 0);
			var charge6 = Creator.CreateCharge(job3, Creator.CC4, "Desc 6 Goods", localCurrency, 51m, DebtorCreditorAT, localCurrency, 101m, null);
			charge6.JR_AT_CostGSTRate = LOWPTUREV.PK;
			var charge7 = Creator.CreateCharge(job3, Creator.CC5, "Desc 7 Service", localCurrency, 61m, DebtorCreditorAT, localCurrency, 103m, null);
			charge7.JR_AT_CostGSTRate = PTUREV.PK;
			var charge8 = Creator.CreateCharge(job3, Creator.CC6, "Desc 8 Goods", localCurrency, 57m, DebtorCreditorAT, localCurrency, 107m, null);
			charge8.JR_AT_CostGSTRate = FREEPTUREV.PK;
			var charge9 = Creator.CreateCharge(job3, Creator.CC5, "Desc 9 Service", localCurrency, 59m, DebtorCreditorPL, localCurrency, 109m, null);
			charge9.JR_AT_CostGSTRate = PTU.PK;
			var charge10 = Creator.CreateCharge(job3, Creator.CC6, "Desc 10 Goods", localCurrency, 67m, OtherCompanyOrgProxy, localCurrency, 111m, null);
			charge10.JR_AT_CostGSTRate = MIDPTUREV.PK;

			Factory.Save();

			APInv1 = (APInvoice)Creator.CreateInvoice(typeof(APInvoice), "APINV01", localCurrency, 1m, Creator.AALSHI);
			APInv1.AH_Desc = "First AP Invoice";
			APInv1.Lines.Add(Creator.CreateCostLine(charge1, APInv1.PK));
			APInv1.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
			APInv1.AH_JH = job1.PK;

			APInv2 = (APInvoice)Creator.CreateInvoice(typeof(APInvoice), "APINV02", localCurrency, 1m, Creator.AALSHI);
			APInv2.AH_Desc = "Second AP Invoice";
			APInv2.Lines.Add(Creator.CreateCostLine(charge2, APInv2.PK));
			APInv2.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
			APInv2.AH_JH = job1.PK;

			APInv3 = (APInvoice)Creator.CreateInvoice(typeof(APInvoice), "APINV03", localCurrency, 1m, Creator.Creditor1);
			APInv3.AH_Desc = "Third AP Invoice";
			APInv3.Lines.Add(Creator.CreateCostLine(charge3, APInv3.PK));
			APInv3.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-5);
			APInv3.Lines.Add(Creator.CreateCostLine(charge4, APInv3.PK));
			APInv3.Lines[1].AL_TaxDate = ZDate.Today.AddDays(1);
			APInv3.AH_JH = job2.PK;

			var charge5 = Creator.CreateCharge(job2, Creator.CC5, "Desc 5 Service", localCurrency, -53m, Creator.Creditor1, localCurrency, -71m, null);
			charge5.JR_AT_CostGSTRate = PTUREV.PK;

			APCrd = Creator.CreateAPCreditNote("APCRD", Creator.Creditor1, localCurrency, charge5.JR_OSCostExRate, "AP Credit Note");
			var crdLine = Creator.CreateAPCreditNoteLine(APCrd, job2, Creator.CC5, localCurrency, charge5.JR_OSCostExRate, "Desc 4", 53m);
			crdLine.AL_AT = PTUREV.PK;
			crdLine.AL_TaxDate = ZDate.Today.AddDays(-1);
			APCrd.AH_JH = job2.PK;
			charge5.JR_AL_APLine = crdLine.PK;

			APInv4 = (APInvoice)Creator.CreateInvoice(typeof(APInvoice), "APINV04", localCurrency, 1m, DebtorCreditorAT);
			APInv4.AH_Desc = "Fourth AP Invoice";
			APInv4.Lines.Add(Creator.CreateCostLine(charge6, APInv4.PK));
			APInv4.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
			APInv4.Lines.Add(Creator.CreateCostLine(charge7, APInv4.PK));
			APInv4.Lines[1].AL_TaxDate = ZDate.Today.AddDays(-2);
			APInv4.Lines.Add(Creator.CreateCostLine(charge8, APInv4.PK));
			APInv4.Lines[2].AL_TaxDate = ZDate.Today.AddDays(-3);
			APInv4.AH_JH = job3.PK;

			APInv5 = (APInvoice)Creator.CreateInvoice(typeof(APInvoice), "APINV05", localCurrency, 1m, DebtorCreditorPL);
			APInv5.AH_Desc = "Fifth AP Invoice";
			APInv5.Lines.Add(Creator.CreateCostLine(charge9, APInv5.PK));
			APInv5.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
			APInv5.AH_JH = job3.PK;

			APInv6 = (APInvoice)Creator.CreateInvoice(typeof(APInvoice), "APINV06", localCurrency, 1m, OtherCompanyOrgProxy);
			APInv6.AH_Desc = "Sixth AP Invoice";
			APInv6.Lines.Add(Creator.CreateCostLine(charge10, APInv6.PK));
			APInv6.Lines[0].AL_TaxDate = APInv6.AH_InvoiceDate.Date;
			APInv6.AH_JH = job3.PK;

			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(Report, APInv1.Lines[0], sequence: 11);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv2.Lines[0], sequence: 12);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv3.Lines[0], sequence: 13);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv3.Lines[1], sequence: 14);
			Creator.CreateComplianceReportTransactionPivot(Report, APCrd.Lines[0], sequence: 15);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv4.Lines[0], sequence: 16);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv4.Lines[1], sequence: 17);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv4.Lines[2], sequence: 18);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv5.Lines[0], sequence: 19);
			Creator.CreateComplianceReportTransactionPivot(Report, APInv6.Lines[0], sequence: 20);
		}

		protected APInvoice APInv1 { get; set; }
		protected APInvoice APInv2 { get; set; }
		protected APInvoice APInv3 { get; set; }
		protected APInvoice APInv4 { get; set; }
		protected APInvoice APInv5 { get; set; }
		protected APInvoice APInv6 { get; set; }
		protected APCreditNote APCrd { get; set; }

		#endregion

		#region Tax Rates

		protected AccTaxRate LOWPTU { get; set; }
		protected AccTaxRate MIDPTU { get; set; }
		protected AccTaxRate PTU { get; set; }

		protected AccTaxRate FREEPTU { get; set; }
		protected AccTaxRate EXEMPT { get; set; }
		protected AccTaxRate EXCLUDE { get; set; }

		protected AccTaxRate LOWPTUREV { get; set; }
		protected AccTaxRate MIDPTUREV { get; set; }
		protected AccTaxRate PTUREV { get; set; }
		protected AccTaxRate FREEPTUREV { get; set; }

		protected void SetupTaxRates()
		{
			LOWPTU = AccTaxRate.FindExistingTaxRate(Factory, "LOWPTU", AccTaxRate.Types.Rated, CountryCodes.Poland);
			LOWPTU.SetRateNumerator_ForTestOnly(5);
			MIDPTU = AccTaxRate.FindExistingTaxRate(Factory, "MIDPTU", AccTaxRate.Types.Rated, CountryCodes.Poland);
			MIDPTU.SetRateNumerator_ForTestOnly(8);
			PTU = AccTaxRate.FindExistingTaxRate(Factory, "PTU", AccTaxRate.Types.Rated, CountryCodes.Poland);
			PTU.SetRateNumerator_ForTestOnly(23);

			FREEPTU = AccTaxRate.FindExistingTaxRate(Factory, "FREEPTU", AccTaxRate.Types.Rated, CountryCodes.Poland);
			EXEMPT = AccTaxRate.FindExistingTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, CountryCodes.Poland);
			EXCLUDE = AccTaxRate.FindExistingTaxRate(Factory, "EXCLUDE", AccTaxRate.Types.ExcludedFromTheTaxBase, CountryCodes.Poland);

			LOWPTUREV = AccTaxRate.FindExistingTaxRate(Factory, "LOWPTUREV", AccTaxRate.Types.ReverseRated, CountryCodes.Poland);
			LOWPTUREV.SetRateNumerator_ForTestOnly(5);
			MIDPTUREV = AccTaxRate.FindExistingTaxRate(Factory, "MIDPTUREV", AccTaxRate.Types.ReverseRated, CountryCodes.Poland);
			MIDPTUREV.SetRateNumerator_ForTestOnly(8);
			PTUREV = AccTaxRate.FindExistingTaxRate(Factory, "PTUREV", AccTaxRate.Types.ReverseRated, CountryCodes.Poland);
			PTUREV.SetRateNumerator_ForTestOnly(23);

			FREEPTUREV = AccTaxRate.FindExistingTaxRate(Factory, "FREEPTUREV", AccTaxRate.Types.ReverseRated, CountryCodes.Poland);
		}

		#endregion

		#region Debtors and Creditors

		protected OrgHeader DebtorCreditorPL { get; set; }
		protected OrgHeader DebtorCreditorAT { get; set; }
		protected OrgHeader OtherCompanyOrgProxy { get; set; }

		protected void SetupDebtorsAndCreditors()
		{
			Creator.DebtorDE.OH_FullName = "Germany Debtor";
			Creator.DebtorDE.OH_RL_NKClosestPort = "DEBER";
			Creator.DebtorDE.MainAddress.OA_RN_NKCountryCode = CountryCodes.Germany;
			Creator.DebtorDE.MainAddress.OA_RL_NKRelatedPortCode = "DEBER";
			Creator.CreateCustomsCodes(Creator.DebtorDE, CountryCodes.Germany, "UST", "0147154321");

			Creator.Creditor1.OH_FullName = "Germany Creditor";
			Creator.Creditor1.OH_RL_NKClosestPort = "DEBER";
			Creator.Creditor1.MainAddress.OA_RN_NKCountryCode = CountryCodes.Germany;
			Creator.Creditor1.MainAddress.OA_RL_NKRelatedPortCode = "DEBER";
			Creator.CreateCustomsCodes(Creator.Creditor1, CountryCodes.Germany, "UST", "0357154321");

			DebtorCreditorPL = Creator.CreateOrgHeader("POLDEBWAR", true, true, true, false, true, false);
			DebtorCreditorPL.OH_FullName = "Poland Debtor & Creditor";
			DebtorCreditorPL.OH_RL_NKClosestPort = "PLWAR";
			DebtorCreditorPL.MainAddress.OA_RN_NKCountryCode = CountryCodes.Poland;
			DebtorCreditorPL.MainAddress.OA_RL_NKRelatedPortCode = "PLWAR";
			Creator.CreateCustomsCodes(DebtorCreditorPL, CountryCodes.Poland, "PTU", "001 471-543.2");
			// Mark as not related
			DebtorCreditorPL.CompanyData.OB_ARConsolidatedAccountingCategory = Constants.AccountsCategory.Unrelated;

			DebtorCreditorAT = Creator.CreateOrgHeader("AUSDEBVIE", true, true, true, false, true, false);
			DebtorCreditorAT.OH_FullName = "Austria Debtor & Creditor";
			DebtorCreditorAT.OH_RL_NKClosestPort = "ATVIE";
			DebtorCreditorAT.MainAddress.OA_RN_NKCountryCode = CountryCodes.Austria;
			DebtorCreditorAT.MainAddress.OA_RL_NKRelatedPortCode = "ATVIE";
			Creator.CreateCustomsCodes(DebtorCreditorAT, CountryCodes.Austria, "UID", "987654321");
			// Make it a related company
			DebtorCreditorAT.CompanyData.OB_ARConsolidatedAccountingCategory = Constants.AccountsCategory.MinorityWithReporting;

			OtherCompanyOrgProxy = Creator.CreateOrgHeader("OTHORG_WW", true, true, true, false, true, false, isOrgProxyForAnotherCompany: true);
			OtherCompanyOrgProxy.OH_FullName = "Other Company Org Proxy";
			OtherCompanyOrgProxy.OH_IsGlobalAccount = true;
			OtherCompanyOrgProxy.OH_RL_NKClosestPort = "DEHAM";
			OtherCompanyOrgProxy.MainAddress.OA_RN_NKCountryCode = CountryCodes.Germany;
			OtherCompanyOrgProxy.MainAddress.OA_RL_NKRelatedPortCode = "DEHAM";
			Creator.CreateCustomsCodes(OtherCompanyOrgProxy, CountryCodes.Germany, "UST", "9237154321");
			Creator.CreateCustomsCodes(OtherCompanyOrgProxy, CountryCodes.Poland, "PTU", "0098765432");

			Factory.Save();
		}

		#endregion

		protected override void SetUp()
		{
			CountrySwitching = GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Poland);

			base.SetUp();
			SetupReport();
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (CountrySwitching != null)
			{
				CountrySwitching.Dispose();
				CountrySwitching = null;
			}
		}

		void SetupReport()
		{
			Report = Factory.NewWithValidTestData<AccComplianceReport>();

			Creator.CreateCustomsCodes(Report.Company.OrgProxy, CountryCodes.Poland, "GCR", "1471");
			Report.Company.GC_BusinessRegNo = "9876 543 210";
			Report.Company.GC_Email = "a@b.com";
			Report.Company.GC_Phone = "02 881 654 321";

			Report.ACR_ReportType = "JPK";
			Report.ACR_DateFrom = new ZDate(2023, 9, 1);
			Report.ACR_DateTo = new ZDate(2023, 9, 30);

			Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Factory.Save();
		}

		protected AccComplianceReport Report { get; private set; }

		protected TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		IDisposable CountrySwitching;
	}
}
