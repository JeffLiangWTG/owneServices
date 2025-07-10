using CargoWise.Common;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses
{
	class PostUpdateProcessManager
	{
		internal PostUpdateProcessManager(IEntityContext context)
		{
			this.context = Argument.NotNull(context, "IEntityContext context");
		}

		readonly IEntityContext context;

		internal void Process(IEntity rootEntity)
		{
			new OrgPatternMatchRegen.PostUpdateProcess(context, rootEntity).Update();

			new ScreeningStatus.RefVesselScreeningStatusPostUpdateProcess(context, rootEntity).Update();

			new ScreeningStatus.OrgScreeningStatusPostUpdateProcess(context, rootEntity).Update();
		}
	}
}
