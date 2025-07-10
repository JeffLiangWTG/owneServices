using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CommissionPeriodListRegistryEditorInfoTest : TestCase
	{
		public void TestBaseDataTypeToBeEdited()
		{
			var editorInfo = new CommissionPeriodListRegistryEditorInfo();
			AssertEquals(typeof(CommissionPeriodCollection), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
