using NUnit.Framework;

namespace Enterprise.Customs.Common.IT.Testing
{
	class ITJobMessageTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestITJobMessageTypeList()
		{
			NUnit.Framework.Assert.That(new ITJobMessageTypeList().CodesAsString, Is.EqualTo("EXP, IMP, MSC, ARN, ULR, DEP, TST"), "ITJobMessageTypeList includes all relevant codes; some are excluded from declaration lookups and used only for Declaration Lock for Edit.");
		}
	}
}

