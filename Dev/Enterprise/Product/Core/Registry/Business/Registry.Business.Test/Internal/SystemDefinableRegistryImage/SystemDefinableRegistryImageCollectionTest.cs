using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing;

[TestedType(typeof(SystemDefinableRegistryImageCollection))]
sealed class SystemDefinableRegistryImageCollectionTest : RegistryImageCollectionTest<SystemDefinableRegistryImageCollection>
{
	#region Implementation

	protected override SystemDefinableRegistryImageCollection GetCollectionToTest()
	{
		return new SystemDefinableRegistryImageCollection();
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new SystemDefinableRegistryImage()
		{
			ImagePkForTest = ZGuid.NewZGuid(),
			Code = nextCode++.ToString()
		};
	}

	int nextCode;

	#endregion
}
