using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(MXMessage))]
	public class MXMessageStreamFormatterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEM_FormattedMessageText()
		{
			MXMessage message = (MXMessage)GetNewBusinessObject();
			AssertEquals("FormattedMessageText", "", message.EM_FormattedMessageText);
			message.EM_MessageText = "New Message";
			AssertEquals("FormattedMessageText", message.EM_MessageText, message.EM_FormattedMessageText);
		}
	}
}
