using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.DbUpgrader.Shared;
using Moq;

#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#else
using System.Runtime.Serialization;
#endif

namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class ReferenceDbUpgradeDirectorThrowsSqlExceptionForTest : ReferenceDbUpgradeDirector
	{
		public ReferenceDbUpgradeDirectorThrowsSqlExceptionForTest(IUpgradeContext upgradeContext, DbConnection connection, IUpgradeTaskWorkflowLogger logger) : base(upgradeContext, connection, logger)
		{
		}

		protected override IRefDataBaseUpgrader SetupUpgrader()
		{
			var upgrader = new Mock<IRefDataBaseUpgrader>();
			// SYSLIB0050 Resolved
#if NET5_0_OR_GREATER
			var sqlException = (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));
#else
			var sqlException = (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));
#endif
			upgrader.Setup(x => x.DoUpgrade(null)).Throws(sqlException);
			return upgrader.Object;
		}
	}
}
