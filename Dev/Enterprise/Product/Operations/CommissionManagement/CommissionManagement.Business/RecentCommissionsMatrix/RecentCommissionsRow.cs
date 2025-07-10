using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public class RecentCommissionsRow : AutoRecentCommissionRow
	{
		#region Properties

		public string StatusDescription
		{
			get
			{
				return StatusCode.IsEmpty ?
					Res.GetString("21b908c3-dc58-4335-8ca0-d900eb9182fc", "Total") :
					new AccCommissionLineCommissionStatusList().GetDescriptionFromCode(StatusCode);
			}
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal Total
		{
			get { return base.Total; }
			set { base.Total = value; }
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal TotalCurrentMonth
		{
			get { return base.TotalCurrentMonth; }
			set { base.TotalCurrentMonth = value; }
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal TotalPreviousMonth
		{
			get { return base.TotalPreviousMonth; }
			set { base.TotalPreviousMonth = value; }
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal Total2MonthsAgo
		{
			get { return base.Total2MonthsAgo; }
			set { base.Total2MonthsAgo = value; }
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal Total3MonthsAgo
		{
			get { return base.Total3MonthsAgo; }
			set { base.Total3MonthsAgo = value; }
		}

		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal TotalOver3MonthsAgo
		{
			get { return base.TotalOver3MonthsAgo; }
			set { base.TotalOver3MonthsAgo = value; }
		}

		public int DecimalPlaces
		{
			get;
			set;
		}

		#endregion Properties
	}
}
