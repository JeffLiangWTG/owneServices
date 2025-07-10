using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public abstract class PtrsReportColumnWrapperBase : NonPersistentBusinessObject, IHaveValueValidation
	{
		protected PtrsReportColumnWrapperBase(AccTaxReturnColumn column) : base(column?.Factory)
		{
			Argument.NotNull(column, nameof(column));
			this.column = column;
			ValueInfo.AdditionalValidation += ValueInfo_AdditionalValidation;
		}

		protected AccTaxReturnColumn column { get; }

		public ZString Code => column.ATC_ColumnName;

		public ZPropertyInfo ValueInfo => GetValueInfo();

		protected abstract ZPropertyInfo GetValueInfo();

		protected virtual void ValueInfo_AdditionalValidation() => ValidateValue();

		#region IHaveValueValidation

		void IHaveValueValidation.SetValueCheck(Action<ZPropertyInfo> valueCheck) => ValueCheck = valueCheck;

		bool IHaveValueValidation.HasValueCheck => HasValueCheck;

		void ValidateValue() => ValueCheck?.Invoke(ValueInfo);

		bool HasValueCheck => ValueCheck != null;

		Action<ZPropertyInfo> ValueCheck { get; set; }

		#endregion
	}
}
