using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using TaxTypes = Enterprise.MasterFiles.Business.AccTaxRate.Types;

namespace Enterprise.Accounting.Utility.Testing
{
	public static class LIQSubmissionDataTestHelper
	{
		public static void CreateTaxRates(List<InvoicingLineBase> lines, TestObjectCreator creator)
		{
			lines[0].AL_AT = creator.CreateTaxRate("RAT", "Rated", TaxTypes.Rated, 10, string.Empty, 0, 1).PK;
			lines[1].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[2].AL_AT = creator.CreateTaxRate("EXT", "Exempt", TaxTypes.Exempt, 10, string.Empty, 0, 1).PK;
			lines[3].AL_AT = creator.CreateTaxRate("CAP", "CapitalRated", TaxTypes.CapitalRated, 10, string.Empty, 0, 1).PK;
			lines[4].AL_AT = creator.CreateTaxRate("NOT", "NotReportable", TaxTypes.NotReportable, 10, string.Empty, 0, 1).PK;
			lines[5].AL_AT = creator.CreateTaxRate("SUS", "Suspended", TaxTypes.Suspended, 10, string.Empty, 0, 1).PK;
			lines[6].AL_AT = creator.CreateTaxRate("BST", "ReportableUnderBusinessTax", TaxTypes.ReportableUnderBusinessTax, 10, string.Empty, 0, 1).PK;
			lines[7].AL_AT = creator.CreateTaxRate("EXL", "ExcludedFromTheTaxBase", TaxTypes.ExcludedFromTheTaxBase, 10, string.Empty, 0, 1).PK;
			lines[8].AL_AT = ZGuid.Empty;
			lines[9].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[10].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[11].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[12].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[13].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[14].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
		}

		public static LIQSubmissionDataColumns CreateSubmissionData(BusinessObjectFactory factory, AccComplianceReport report)
		{
			var submissionData = new LIQSubmissionDataColumns(factory, report);
			submissionData.ComputedByCW1.Box1_TotalVatBaseReceivables = 1000M;
			submissionData.ComputedByCW1.Box2_TotalVatReceivables = 500M;
			submissionData.ComputedByCW1.Box3_TotalVatBasePayables = 200M;
			submissionData.ComputedByCW1.Box4_TotalVatPayablesRecoverable = 2500M;
			submissionData.ComputedByCW1.Box5_TotalVatPayablesNotRecoverable = 3500M;
			submissionData.ComputedByCW1.Box7_BalancePreviousPeriod = -300M;

			submissionData.Adjustments.Box1_TotalVatBaseReceivables = 10M;
			submissionData.Adjustments.Box2_TotalVatReceivables = 500M;
			submissionData.Adjustments.Box3_TotalVatBasePayables = 200M;
			submissionData.Adjustments.Box4_TotalVatPayablesRecoverable = 250M;
			submissionData.Adjustments.Box5_TotalVatPayablesNotRecoverable = 35M;
			submissionData.Adjustments.Box7_BalancePreviousPeriod = 70M;

			submissionData.Declaration = true;
			submissionData.ReturnDueDate = ZDateTime.Today.Date;

			submissionData.PageFromAP = 1;
			submissionData.PageFromAR = 2;
			submissionData.PageFromLiquidazione = 3;
			submissionData.PageToAP = 4;
			submissionData.PageToAR = 5;
			submissionData.PageToLiquidazione = 6;

			submissionData.UpdateValuesToSubmit();
			return submissionData;
		}
	}
}
