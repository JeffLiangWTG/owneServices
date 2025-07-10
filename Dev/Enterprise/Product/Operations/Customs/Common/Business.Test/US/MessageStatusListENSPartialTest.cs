using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.US.Testing
{
	class MessageStatusListENSTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCodeDescriptionPairList()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new MessageStatusListENS();
			NUnit.Framework.Assert.That(list.GetCodeDescriptionPairList(), Is.EqualTo(list).Using(CustomComparers.TypeComparison));
		}
	}
}
