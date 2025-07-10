using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLEDIMessageInterpreterHelperTest : TestCase
{
	public void TestAppendTableRow()
	{
		var stringBuilder = new ZStringBuilder();
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, "MRN", "MRN123", "padding-left:2em;");
		AssertEquals("<tr><td style='padding-left:2em;'><b>MRN:</b></td><td><i>MRN123</i></td></tr>", stringBuilder.ToString());
	}
}
