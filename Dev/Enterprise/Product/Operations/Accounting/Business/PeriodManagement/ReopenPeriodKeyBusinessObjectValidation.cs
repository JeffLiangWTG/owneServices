namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class ReopenPeriodKeyBusinessObjectValidation : AutoReopenPeriodKeyBusinessObjectValidation
	{
		public ReopenPeriodKeyBusinessObjectValidation(AutoReopenPeriodKeyBusinessObject parent)
			: base(parent) { }

		#region Implementation

		public new ReopenPeriodKeyBusinessObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReopenPeriodKeyBusinessObject)base.Parent; }
		}

		#endregion
	}
}

