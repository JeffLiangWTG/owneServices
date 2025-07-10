using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.US.Testing
{
	class MessageStatusListEITest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCodeDescriptionPairList()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new MessageStatusListEI();
			NUnit.Framework.Assert.That(list.GetCodeDescriptionPairList(), Is.EqualTo(list).Using(CustomComparers.TypeComparison));
		}
	}
}
