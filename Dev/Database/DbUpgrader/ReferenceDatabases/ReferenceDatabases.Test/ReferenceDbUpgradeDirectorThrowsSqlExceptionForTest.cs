using System.IO;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.DbUpgrader.Shared;
using Moq;

namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class ReferenceDbUpgradeDirectorThrowsOtherExceptionForTest : ReferenceDbUpgradeDirector
	{
		public ReferenceDbUpgradeDirectorThrowsOtherExceptionForTest(IUpgradeContext upgradeContext, DbConnection connection, IUpgradeTaskWorkflowLogger logger) : base(upgradeContext, connection, logger)
		{
		}

		protected override IRefDataBaseUpgrader SetupUpgrader()
		{
			var upgrader = new Mock<IRefDataBaseUpgrader>();
			upgrader.Setup(x => x.DoUpgrade(null)).Throws(new IOException("Test Exception"));
			return upgrader.Object;
		}
	}
}
