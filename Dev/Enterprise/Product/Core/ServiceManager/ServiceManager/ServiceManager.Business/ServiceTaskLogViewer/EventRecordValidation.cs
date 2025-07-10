namespace Enterprise.ServiceManager.Business
{
	public class EventRecordValidation : AutoEventRecordValidation
	{
		public EventRecordValidation(AutoEventRecord parent)
			: base(parent) { }

		#region Implementation

		public new EventRecord Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (EventRecord)base.Parent; }
		}

		#endregion
	}
}

