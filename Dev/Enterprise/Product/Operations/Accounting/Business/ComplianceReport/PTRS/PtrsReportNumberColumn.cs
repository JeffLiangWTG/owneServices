using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportNumberColumn : PtrsReportColumnWithCommentBase, IHandleValueChanged
	{
		public PtrsReportNumberColumn(AccTaxReturnColumn column) : base(column)
		{
		}

		public ZInt Value
		{
			get => column.ATC_Amount.ToZInt();
			set
			{
				column.ATC_Amount = new ZDecimal(value);
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
