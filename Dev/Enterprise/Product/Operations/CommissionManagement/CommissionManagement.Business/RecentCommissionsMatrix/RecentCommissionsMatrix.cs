using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public class RecentCommissionsMatrix : NonPersistentBusinessObjectCollection<RecentCommissionsRow>
	{
		public RecentCommissionsMatrix(ZDateTime referenceDate, ViewCommissionLineCollection lineCollection)
		{
			this.ReferenceDate = referenceDate;
			this.LineCollection = lineCollection;
			this.LineCollection.CountChanged += lineCollection_CountChanged;
		}

		public readonly ZDateTime ReferenceDate;
		readonly ViewCommissionLineCollection LineCollection;

		#region Allowed Actions

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion Allowed Actions

		#region Refresh

		void lineCollection_CountChanged(object sender, EventArgs e)
		{
			Refresh();
		}

		void Refresh()
		{
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();
				Add(GetRow(LineCollection, AccCommissionLineCommissionStatusList.Codes.Pending));
				Add(GetRow(LineCollection, AccCommissionLineCommissionStatusList.Codes.Approved));
				Add(GetRow(LineCollection, AccCommissionLineCommissionStatusList.Codes.Paid));
				Add(GetRow(LineCollection, ZString.Empty));
			}
		}

		RecentCommissionsRow GetRow(ViewCommissionLineCollection lineCollection, ZString statusCode)
		{
			var firstDayOfCurrentMonth = new ZDateTime(ReferenceDate.Year, ReferenceDate.Month, 1);
			var linesMatchingStatus = lineCollection.Where(x => !x.RecognitionDate.IsEmpty && (statusCode.IsEmpty || x.CommissionStatusCode == statusCode));

			return new RecentCommissionsRow()
			{
				StatusCode = statusCode,
				Total = linesMatchingStatus.Sum(x => x.VCL_EntityCommissionAmountInLocalCurrency),
				TotalCurrentMonth = linesMatchingStatus.Where(x => x.RecognitionDate.Month == firstDayOfCurrentMonth.Month).Sum(x => x.VCL_EntityCommissionAmountInLocalCurrency),
				TotalPreviousMonth = linesMatchingStatus.Where(x => x.RecognitionDate.Month == firstDayOfCurrentMonth.AddMonths(-1).Month).Sum(x => x.VCL_EntityCommissionAmountInLocalCurrency),
				Total2MonthsAgo = linesMatchingStatus.Where(x => x.RecognitionDate.Month == firstDayOfCurrentMonth.AddMonths(-2).Month).Sum(x => x.VCL_EntityCommissionAmountInLocalCurrency),
				Total3MonthsAgo = linesMatchingStatus.Where(x => x.RecognitionDate.Month == firstDayOfCurrentMonth.AddMonths(-3).Month).Sum(x => x.VCL_EntityCommissionAmountInLocalCurrency),
				TotalOver3MonthsAgo = linesMatchingStatus.Where(x => x.RecognitionDate < firstDayOfCurrentMonth.AddMonths(-3)).Sum(x => x.VCL_EntityCommissionAmountInLocalCurrency),
				DecimalPlaces = lineCollection.Any() ? lineCollection.First().LocalCurrencyDecimalPlaces : 2
			};
		}

		#endregion Refresh

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RecentCommissionsRow();
		}

		#endregion Overrides
	}
}
