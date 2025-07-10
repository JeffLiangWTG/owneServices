using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.Upgrades;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class ClientDocumentsBaseUpgraderTest : BaseUpgraderTestCase
	{
		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			ClientDocumentsUpgraderTest.CreateTestPackage(out var packagePk, out var packageVersion);

			var clientDocumentsUpgraderMock = new Mock<ClientDocumentsUpgrader>(dummyUpgradeManager, Db.Connection, new VersionLabel(1, 0), new UpgradeInfo(packagePk, packageVersion));
			clientDocumentsUpgraderMock.Setup(c => c.ReportError(It.IsAny<string>()));
			return clientDocumentsUpgraderMock.Object;
		}
	}
}
