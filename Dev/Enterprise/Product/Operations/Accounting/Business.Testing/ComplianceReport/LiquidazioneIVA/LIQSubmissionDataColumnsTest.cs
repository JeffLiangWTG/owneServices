using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.Testing
{
	[TestedType(typeof(LIQSubmissionDataColumns))]
	public partial class LIQSubmissionDataColumnsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new LIQSubmissionDataColumns(Factory, Factory.New<AccComplianceReport>());
		}

		LIQSubmissionDataColumns Columns;
		AccComplianceReport Report;

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.New<AccComplianceReport>();
			Report.FillWithValidTestData();

			Columns = new LIQSubmissionDataColumns(Factory, Report);
			Columns.ComputedByCW1.Box1_TotalVatBaseReceivables = 1500m;
			Columns.ComputedByCW1.Box2_TotalVatReceivables = 100m;
			Columns.ComputedByCW1.Box3_TotalVatBasePayables = 1000m;
			Columns.ComputedByCW1.Box4_TotalVatPayablesRecoverable = 200m;
			Columns.ComputedByCW1.Box5_TotalVatPayablesNotRecoverable = 200m;
			Columns.ComputedByCW1.Box7_BalancePreviousPeriod = -100m;

			Columns.UpdateValuesToSubmit();

			Columns.PageFromAR = 1;
			Columns.PageFromAP = 2;
			Columns.PageFromLiquidazione = 3;
		}

		public void TestAmounts()
		{
			AssertEquals(1500m, Columns.ValuesToSubmit.Box1_TotalVatBaseReceivables);
			AssertEquals(100m, Columns.ValuesToSubmit.Box2_TotalVatReceivables);
			AssertEquals(1000m, Columns.ValuesToSubmit.Box3_TotalVatBasePayables);
			AssertEquals(200m, Columns.ValuesToSubmit.Box4_TotalVatPayablesRecoverable);
			AssertEquals(200m, Columns.ValuesToSubmit.Box5_TotalVatPayablesNotRecoverable);
			AssertEquals(300m, Columns.ValuesToSubmit.Box6_VatBalanceReceivablesAndPayables);
			AssertEquals(-100m, Columns.ValuesToSubmit.Box7_BalancePreviousPeriod);
			AssertEquals(200m, Columns.ValuesToSubmit.Box8_TotalBalance);

			Columns.Adjustments.Box1_TotalVatBaseReceivables = -150m;
			Columns.Adjustments.Box4_TotalVatPayablesRecoverable = 50m;

			AssertEquals(1350m, Columns.ValuesToSubmit.Box1_TotalVatBaseReceivables);
			AssertEquals(100m, Columns.ValuesToSubmit.Box2_TotalVatReceivables);
			AssertEquals(1000m, Columns.ValuesToSubmit.Box3_TotalVatBasePayables);
			AssertEquals(250m, Columns.ValuesToSubmit.Box4_TotalVatPayablesRecoverable);
			AssertEquals(200m, Columns.ValuesToSubmit.Box5_TotalVatPayablesNotRecoverable);
			AssertEquals(350m, Columns.ValuesToSubmit.Box6_VatBalanceReceivablesAndPayables);
			AssertEquals(-100m, Columns.ValuesToSubmit.Box7_BalancePreviousPeriod);
			AssertEquals(250m, Columns.ValuesToSubmit.Box8_TotalBalance);

			Columns.Adjustments.Box2_TotalVatReceivables = 250m;
			Columns.Adjustments.Box4_TotalVatPayablesRecoverable = -50m;
			Columns.Adjustments.Box7_BalancePreviousPeriod = 51m;

			AssertEquals(1350m, Columns.ValuesToSubmit.Box1_TotalVatBaseReceivables);
			AssertEquals(350m, Columns.ValuesToSubmit.Box2_TotalVatReceivables);
			AssertEquals(1000m, Columns.ValuesToSubmit.Box3_TotalVatBasePayables);
			AssertEquals(150m, Columns.ValuesToSubmit.Box4_TotalVatPayablesRecoverable);
			AssertEquals(200m, Columns.ValuesToSubmit.Box5_TotalVatPayablesNotRecoverable);
			AssertEquals(500m, Columns.ValuesToSubmit.Box6_VatBalanceReceivablesAndPayables);
			AssertEquals(-49m, Columns.ValuesToSubmit.Box7_BalancePreviousPeriod);
			AssertEquals(451m, Columns.ValuesToSubmit.Box8_TotalBalance);
		}

		public void TestValidatePageFrom()
		{
			Columns.PageFromAP = 1;
			AssertNoError(Columns.PageFromAPInfo, "Please enter a starting page number, before proceeding with printing and archiving the report, the page number must be greater than 0.");
			AssertNoError(Columns.PageFromARInfo, "Please enter a starting page number, before proceeding with printing and archiving the report, the page number must be greater than 0.");
			AssertNoError(Columns.PageFromLiquidazioneInfo, "Please enter a starting page number, before proceeding with printing and archiving the report, the page number must be greater than 0.");
			AssertNoWarning(Columns.PageFromAPInfo, "The page number should be consecutive to the last printed page of the previous period.");
			AssertNoWarning(Columns.PageFromARInfo, "The page number should be consecutive to the last printed page of the previous period.");
			AssertNoWarning(Columns.PageFromLiquidazioneInfo, "The page number should be consecutive to the last printed page of the previous period.");

			Columns.SetMinPageFrom(2, 2, 4);
			Columns.PageFromAR = 2;
			AssertHasWarning(Columns.PageFromAPInfo, "The page number should be consecutive to the last printed page of the previous period.");
			AssertHasWarning(Columns.PageFromARInfo, "The page number should be consecutive to the last printed page of the previous period.");
			AssertHasWarning(Columns.PageFromLiquidazioneInfo, "The page number should be consecutive to the last printed page of the previous period.");

			Columns.PageFromAP = 0;
			Columns.PageFromAR = 0;
			Columns.PageFromLiquidazione = 0;
			AssertHasError(Columns.PageFromAPInfo, "Please enter a starting page number, before proceeding with printing and archiving the report, the page number must be greater than 0.");
			AssertHasError(Columns.PageFromARInfo, "Please enter a starting page number, before proceeding with printing and archiving the report, the page number must be greater than 0.");
			AssertHasError(Columns.PageFromLiquidazioneInfo, "Please enter a starting page number, before proceeding with printing and archiving the report, the page number must be greater than 0.");
		}
	}
}
