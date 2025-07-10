using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

[TestsSubclassesOf(typeof(TemporaryStoragePackedItemConfiguration))]
public abstract class TemporaryStoragePackedItemConfigurationAbstractTest<T> : TestCaseWithFactory where T : TemporaryStoragePackedItemConfiguration, new()
{
	public abstract void TestGetValidationDecider();
	protected override void SetUp()
	{
		base.SetUp();
		configuration = new T();
	}
	protected T configuration;
}

[TestedType(typeof(TemporaryStoragePackedItemConfiguration))]
sealed class TemporaryStoragePackedItemConfigurationTest : TemporaryStoragePackedItemConfigurationAbstractTest<TemporaryStoragePackedItemConfiguration>
{
	public override void TestGetValidationDecider()
	{
		AssertType<TemporaryStoragePackedItemValidationDecider>(configuration.GetValidationDecider());
	}
}
