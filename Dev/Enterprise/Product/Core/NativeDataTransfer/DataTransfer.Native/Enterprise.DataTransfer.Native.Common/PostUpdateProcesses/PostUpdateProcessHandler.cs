using CargoWise.Common;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses
{
	abstract class PostUpdateProcessHandler
	{
		protected PostUpdateProcessHandler(IEntityContext context, IEntity rootEntity)
		{
			Context = context;
			RootEntity = Argument.NotNull(rootEntity, "IEntity rootEntity");
		}

		protected IEntityContext Context { get; }

		protected IEntity RootEntity { get; }

		protected abstract bool ShouldRun { get; }

		protected abstract void UpdateCore();

		public void Update()
		{
			if (ShouldRun)
			{
				UpdateCore();
			}
		}
	}
}
