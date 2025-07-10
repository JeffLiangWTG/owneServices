namespace Enterprise.Client.EDI.MasterFiles.ProcessManagement
{
	using Enterprise.MasterFiles.Module;
	using Enterprise.ZArchitecture.Business;

	public class EDIProcessTasksModule : ProcessTasksModule
	{
		#region Standard Module overrides

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIProcessTaskFilterBusinessObject();
		}

		#endregion
	}
}
