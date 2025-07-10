using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxPortConfigObjLookups : ZLookups
	{
		public FaxPortConfigObjLookups(FaxPortConfigObj parent)
			: base(parent) { }

		#region Implementation

		protected new FaxPortConfigObj Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (FaxPortConfigObj)base.Parent; }
		}

		#endregion
	}
}
