using CargoWise.Application;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class LicenceUpdateNotificationTest : TestCase
	{
		public void TestSend()
		{
			using (ObjectFactory.Substitute<IOutgoingSystemMessage>(new DebugOnlyOutgoingSystemMessage()))
			{
				new LicenceUpdateNotification().Send(null, "not xml, escape this <", "some message with non ascii char Pi (\u03a0) and Sigma (\u03a3).");
				AssertEquals("message created", 1, DebugOnlyOutgoingSystemMessage.CreateCalls);
				string expected = "<ReferenceDataUpdateResponse><Status>some message with non ascii char Pi (\u03a0) and Sigma (\u03a3).</Status><ReferenceDataUpdate>not xml, escape this &lt;</ReferenceDataUpdate></ReferenceDataUpdateResponse>";
				AssertEquals("message text", expected, DebugOnlyOutgoingSystemMessage.XmlMessageBody);
			}

			DebugOnlyOutgoingSystemMessage.Initialize();
		}
	}
}
