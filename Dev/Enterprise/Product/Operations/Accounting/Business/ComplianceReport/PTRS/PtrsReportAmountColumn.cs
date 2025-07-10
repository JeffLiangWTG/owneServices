using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportAmountColumn : PtrsReportColumnWithCommentBase, IHandleValueChanged
	{
		public PtrsReportAmountColumn(AccTaxReturnColumn column) : base(column)
		{
		}

		[DecimalPlaces(2)]
		public ZDecimal Value
		{
			get => column.ATC_Amount;
			set
			{
				column.ATC_Amount = value;
				ValidateComment();
			}
		}

		protected override ZPropertyInfo GetValueInfo() => GetWrappedZPropertyInfo(nameof(Value), _ => column.ATC_AmountInfo);

		void IHandleValueChanged.AttachValueChangedHandlerOnlyOnce(EventHandler valueChanged)
		{
			if (!IsValueChangedHandlerAttached)
			{
				ValueInfo.ValueChanged += valueChanged;
				IsValueChangedHandlerAttached = true;
			}
		}

		public bool IsValueChangedHandlerAttached { get; private set; }
	}
}
