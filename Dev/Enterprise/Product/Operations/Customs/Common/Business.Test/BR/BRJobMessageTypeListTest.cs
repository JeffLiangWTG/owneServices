using NUnit.Framework;

namespace Enterprise.Customs.Common.BR.Testing
{
	class BRJobMessageTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestBRJobMessageTypeList()
		{
			NUnit.Framework.Assert.That(new BRJobMessageTypeList().ContainsOnly("EXP", "IMP", "DRW", "EXW", "WEA", "REF", "MSC", "LPC", "LIC", "ISW"), "Contains EXP, IMP, DRW, EXW, WEA, REF, MSC, LPC, LIC, ISW");
		}
	}
}
