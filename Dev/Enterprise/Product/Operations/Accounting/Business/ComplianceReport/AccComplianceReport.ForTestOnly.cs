#if DEBUG

using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public partial class AccComplianceReport
	{
		public void SetOpeningBalance_ForTestOnly(ZDecimal balance)
		{
			glOpeningBalanceDR = glOpeningBalanceCR = balance;
		}

		public void ClearReportLines_ForTestOnly()
		{
			reportLines = null;
			reportLinesCurrentPeriod = null;
			reportLinesPreviousPeriod = null;
			reportTotals = null;
			reportTotalsPreviousPeriod = null;
			reportTotalsCurrentPeriod = null;
			glOpeningBalanceDetails = null;
			glOpeningBalanceDetailsView = null;
			glMovementDetails = null;
			glMovementDetailsView = null;
			glClosingBalanceDetails = null;
			glClosingBalanceDetailsView = null;
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ACR_ReportType = TestObjectCreator.GetRandomString(3);
			ACR_Description = "Compliance Report " + ACR_ReportType;
			ACR_Periodicity = "RNG";
			ACR_DateFrom = ZDate.Today.AddDays(-7);
			ACR_DateTo = ZDate.Today;
		}
	}
}

#endif
