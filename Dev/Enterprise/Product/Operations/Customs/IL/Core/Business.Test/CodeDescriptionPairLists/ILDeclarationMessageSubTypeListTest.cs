using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDeclarationMessageSubTypeListTest : TestCaseWithFactory
	{
		public void TestList()
		{
			AssertCodeDescriptionPairList(new ILDeclarationMessageSubTypeList(), ("IM", "Import"), ("EX", "Export"));
		}
	}
}
