using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.US.ISF.Testing
{
	class DispositionCodeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestICodeDescriptionPairListProviderMembers()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new DispositionCodeList();
			NUnit.Framework.Assert.That(list.GetCodeDescriptionPairList(), Is.EqualTo(list).Using(CustomComparers.TypeComparison));
		}
	}
}
