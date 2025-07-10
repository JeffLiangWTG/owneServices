namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxPortConfigObjValidation : AutoFaxPortConfigObjValidation
	{
		public FaxPortConfigObjValidation(AutoFaxPortConfigObj parent)
			: base(parent) { }

		#region Implementation

		public new FaxPortConfigObj Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (FaxPortConfigObj)base.Parent; }
		}

		#endregion
	}
}
