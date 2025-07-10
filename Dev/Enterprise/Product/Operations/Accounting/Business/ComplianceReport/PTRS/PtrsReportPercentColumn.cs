using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportPercentColumn : PtrsReportColumnWrapperBase, IHaveDecimalPlaces
	{
		public PtrsReportPercentColumn(AccTaxReturnColumn column) : base(column)
		{
			DecimalPlaces = 1;
		}

		int IHaveDecimalPlaces.DecimalPlaces { get => DecimalPlaces; set => DecimalPlaces = value; }

		protected int DecimalPlaces { get; set; }

		[ReadOnly(true)]
		[DecimalPlaces(nameof(DecimalPlaces))]
		public ZDecimal Value { get => column.ATC_Amount; set => column.ATC_Amount = value.Round(DecimalPlaces); }

		protected override ZPropertyInfo GetValueInfo() => GetWrappedZPropertyInfo(nameof(Value), (_) => column.ATC_AmountInfo);
	}
}
