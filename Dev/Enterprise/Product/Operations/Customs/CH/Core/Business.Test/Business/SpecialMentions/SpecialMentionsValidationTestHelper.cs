using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

class SpecialMentionsValidationTestHelper
{
	internal static void TestCheckSpecialMentions(ZPropertyInfo specialMentionsInfo)
	{
		const string message = "Special mentions may have a maximum of 99 lines.";

		specialMentionsInfo.Value = Replicate("line\r\n", 100);
		TestCaseWithFactory.AssertHasMessageError("More than 99 lines", specialMentionsInfo, message);

		specialMentionsInfo.Value = Replicate("123456789 ", 7 * 99 + 1);
		TestCaseWithFactory.AssertHasMessageError("Too many characters", specialMentionsInfo, message);

		specialMentionsInfo.Value = Replicate("line\r\n", 99);
		TestCaseWithFactory.AssertNoMessageError("Not more than 99 lines", specialMentionsInfo, message);
	}

	static ZString Replicate(ZString str, int count)
	{
		var result = new StringBuilder();
		while (count-- > 0)
		{
			result.Append(str);
		}
		return result.ToString();
	}
}
