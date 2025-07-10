using NUnit.Framework;

namespace Enterprise.Customs.Common.CH.Testing
{
	class CHJobMessageTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCHJobMessageTypeList()
		{
			NUnit.Framework.Assert.That(new CHJobMessageTypeList().CodesAsString, Is.EqualTo("EXP, IMP, MSC, ARN, ULR, DEP, EDA"), "CHJobMessageTypeList Codes");
		}
	}
}
