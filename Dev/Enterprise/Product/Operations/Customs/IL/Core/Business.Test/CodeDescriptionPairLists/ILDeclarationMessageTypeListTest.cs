using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDeclarationMessageTypeListTest : TestCaseWithFactory
	{
		public void TestList()
		{
			AssertCodeDescriptionPairList(new ILDeclarationMessageTypeList(), ("IMP", "Import"), ("EXP", "Export"));
		}
	}
}
