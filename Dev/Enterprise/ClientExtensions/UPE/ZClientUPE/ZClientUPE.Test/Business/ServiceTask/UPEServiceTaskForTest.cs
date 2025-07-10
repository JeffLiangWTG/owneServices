using System.Threading;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Client.UPE.ServiceTask.Testing
{
	public class UPEServiceTaskForTest : UPEServiceTask
	{
		public UPEServiceTaskForTest(ILogger logger) : base(logger)
		{
		}

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			Executed = true;
		}

		public new void TryMoveOrDeleteFile(ZString sourceFile, ZString archiveDirectory)
		{
			base.TryMoveOrDeleteFile(sourceFile, archiveDirectory);
		}

		public new void TryDeleteFile(ZString sourceFile)
		{
			base.TryDeleteFile(sourceFile);
		}

		public bool Executed;
	}
}
