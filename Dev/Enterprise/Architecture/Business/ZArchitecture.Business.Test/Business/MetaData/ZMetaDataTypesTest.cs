using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.ComponentModel.Testing
{
	sealed class ZMetaDataTypesTest : TestCaseWithFactory
	{
		public void TestModuleIdRegistered()
		{
			AssertNotNull(MetaDataType.GetMetaDataType(ZMetaDataTypes.ModuleId));
		}
	}
}
