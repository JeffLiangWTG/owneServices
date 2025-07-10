using Moq;
using Moq.Language.Flow;

namespace CargoWise.Data.Testing;

public class DbRecoveryModelManagerMock : Mock<IDbRecoveryModelManagerInternals>
{
	public ISetup<IDbRecoveryModelManagerInternals, DbRecoveryModel?> SetupGetDesired()
	{
		return Setup(c => c.GetDesired(It.IsAny<DbConnection>(), It.IsAny<string>(), It.IsAny<string>()));
	}

	public ISetup<IDbRecoveryModelManagerInternals, DbRecoveryModel> SetupGetActual()
	{
		return Setup(c => c.GetActual(It.IsAny<DbConnection>(), It.IsAny<string>()));
	}

	public new IDbRecoveryModelManager Object => base.Object;

	public static IDbRecoveryModelManager WithActual(DbRecoveryModel recoveryModel)
	{
		var mock = new DbRecoveryModelManagerMock();
		_ = mock.SetupGetActual().Returns(recoveryModel);
		return mock.Object;
	}
}
