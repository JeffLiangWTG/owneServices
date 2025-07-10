using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.EPayment
{
	[System.CodeDom.Compiler.GeneratedCode("NonPersistentBusinessObjectGenerator", "1.0")]
	public class AutoEPaymentQuoteSummaryValidation : ZValidation
	{
		public AutoEPaymentQuoteSummaryValidation(EPaymentQuoteSummary parent) : base(parent)
		{
			this.parent = parent;
			this.ZValidationInternals = this;
			this.ParentListInternals = parent;
		}
		public void Add(AutoEPaymentQuoteSummaryValidation validation)
		{
			ZValidationInternals.Add(validation);
		}
		public void Remove(AutoEPaymentQuoteSummaryValidation validation)
		{
			ZValidationInternals.Remove(validation);
		}

		#region ValidateAll
		public override void ValidateAll()
		{
			IDisposable suspender = ParentListInternals.SuspendListChanged();
			try
			{
				ValidateAllCore();
			}
			finally
			{
				suspender.Dispose();
			}
		}
		protected virtual void ValidateAllCore()
		{
			ValidateTotalFeeAmount();
		}
		#endregion
		#region TotalFeeAmount
		public void ValidateTotalFeeAmount()
		{
			ZValidationInternals.Validate(Parent.TotalFeeAmountInfo, new RunValidationInvoker(this.TotalFeeAmountValidationInvoker));
		}
		void TotalFeeAmountValidationInvoker()
		{
			CheckTotalFeeAmount();
		}
		public virtual void CheckTotalFeeAmount()
		{ }
		#endregion

		#region Implementation
		public override Type AutoValidationType
		{
			get
			{
				return typeof(AutoEPaymentQuoteSummaryValidation);
			}
		}
		public EPaymentQuoteSummary Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		EPaymentQuoteSummary parent;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		IValidationInternals ZValidationInternals;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		ISingleElementListInternal ParentListInternals;
		#endregion
	}
}
