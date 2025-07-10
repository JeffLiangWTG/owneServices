using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using TaxTypes = Enterprise.MasterFiles.Business.AccTaxRate.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA
{
	public class LIQSubmissionDataHelper
	{
		public LIQSubmissionDataHelper(AccComplianceReport report)
		{
			Report = report;
		}

		public AccComplianceReport Report { get; }

		public void SetValues(LIQSubmissionData subData)
		{
			VATSummaryData boxes = new VATSummaryData(ZDecimal.Zero);

			foreach (AccComplianceReportLine line in Report.ReportLines.Cast<AccComplianceReportLine>())
			{
				if (line.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					CalculateARLineData(line, ref boxes);
				}
				else if (line.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					CalculateAPLineData(line, ref boxes);
				}
			}

			SetPreviousPeriodData(subData);

			subData.Box1_TotalVatBaseReceivables = boxes.TotalVatBaseReceivables;
			subData.Box2_TotalVatReceivables = boxes.TotalVatReceivables;
			subData.Box3_TotalVatBasePayables = boxes.TotalVatBasePayables;
			subData.Box4_TotalVatPayablesRecoverable = boxes.TotalVatPayablesRecoverable;
			subData.Box5_TotalVatPayablesNotRecoverable = boxes.TotalVatPayablesNotRecoverable;

			subData.IsReadOnly = true;
		}

		void CalculateARLineData(AccComplianceReportLine line, ref VATSummaryData boxes)
		{
			boxes.TotalVatBaseReceivables += line.TotalExTaxAmount;
			boxes.TotalVatReceivables += line.TotalTaxAmount;
		}

		void CalculateAPLineData(AccComplianceReportLine line, ref VATSummaryData boxes)
		{
			if (line.AT_Type == TaxTypes.ReverseRated)
			{
				boxes.TotalVatBaseReceivables -= line.TotalExTaxAmount;
				boxes.TotalVatReceivables -= line.TaxReverseChargeInputAmount;
			}
			boxes.TotalVatBasePayables += line.TotalExTaxAmount;
			boxes.TotalVatPayablesRecoverable += (line.TaxReverseChargeInputAmount + line.TaxRecoverableAmount);
			boxes.TotalVatPayablesNotRecoverable += line.TaxNotRecoverableAmount;
		}

		struct VATSummaryData
		{
			public ZDecimal TotalVatBaseReceivables;
			public ZDecimal TotalVatReceivables;
			public ZDecimal TotalVatBasePayables;
			public ZDecimal TotalVatPayablesRecoverable;
			public ZDecimal TotalVatPayablesNotRecoverable;
			public ZDecimal BalancePreviousPeriod;

			public VATSummaryData(ZDecimal initValue)
			{
				TotalVatBaseReceivables = initValue;
				TotalVatReceivables = initValue;
				TotalVatBasePayables = initValue;
				TotalVatPayablesRecoverable = initValue;
				TotalVatPayablesNotRecoverable = initValue;
				BalancePreviousPeriod = initValue;
			}
		}

		public (bool, int , int, int) SetPreviousPeriodData(LIQSubmissionData subData)
		{
			subData.Box7_BalancePreviousPeriod = ZDecimal.Zero;

			var previousReport = Report.GetPreviousReport();
			if (previousReport != null && previousReport.ACR_DateTo == Report.ACR_DateFrom.AddDays(-1)
				&& (previousReport.ACR_Status == AccComplianceReport.Status.ReportGenerated
				|| previousReport.ACR_Status == AccComplianceReport.Status.ReportFinalised))
			{
				var previousSubmissionData = LIQSubmissionDataColumnsAdapter.LoadLIQSubmissionData(previousReport);
				if (previousSubmissionData != null)
				{
					previousSubmissionData.UpdateValuesToSubmit();

					var balancePreviousPeriod = previousSubmissionData.ValuesToSubmit.Box8_TotalBalance;
					subData.Box7_BalancePreviousPeriod = balancePreviousPeriod <= ZDecimal.Zero
						? balancePreviousPeriod : ZDecimal.Zero;

					return (true, previousSubmissionData.PageToAR, previousSubmissionData.PageToAP, previousSubmissionData.PageToLiquidazione);
				}
			}
			return (false, 0, 0, 0);
		}
	}
}
