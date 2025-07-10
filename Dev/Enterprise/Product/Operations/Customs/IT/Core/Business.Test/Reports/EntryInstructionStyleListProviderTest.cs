using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Reports.Testing;

sealed class EntryInstructionStyleListProviderTest : TestCase
{
	public void TestGetCodeDescriptionPairList()
	{
		var provider = new EntryInstructionStyleListProvider();
		AssertType<SADDeclarationTypeList>(provider.GetCodeDescriptionPairList());
	}
}
