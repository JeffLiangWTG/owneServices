using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

class SpecialMentionsTestHelper
{
	internal static void TestSpecialMentions(ZPropertyInfo specialMentionsInfo)
	{
		specialMentionsInfo.Value = ZString.Replicate('X', 100);
		var lines = ((ZString)specialMentionsInfo.Value).Split("\r\n");
		Assertion.AssertEquals(ZString.Replicate('X', 70), lines[0]);
		Assertion.AssertEquals(ZString.Replicate('X', 30), lines[1]);
	}
}
