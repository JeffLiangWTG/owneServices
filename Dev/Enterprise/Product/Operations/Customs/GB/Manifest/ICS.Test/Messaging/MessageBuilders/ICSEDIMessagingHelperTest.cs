using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315.Testing
{
	public class ICSEDIMessagingHelperTest : TestCaseWithFactory
	{
		public void TestCreateMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var helper = new ICSEDIMessagingHelper(header);
			helper.SendMessageAndSave(GBMessageTypeList.Codes.New);
			var createdMessage = Factory.LoadTop1<IcsNorthernIrelandEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
			AssertNotNull("ICSEDIMessage should have been created.", createdMessage);

			CombineAssertions("ICSEDIMessage values", () =>
			{
				AssertEquals("EM_ApplicationCode", "GIN", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", "TRX", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				AssertEquals("EM_MessageSubType", "NEW", createdMessage.EM_MessageSubType);
			});

			AssertEquals("Message status of AsycudaManifestHeader", header.AMA_MessageStatus, ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting);
		}

		public void TestMessageInterpretationForCC315A()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var helper = new ICSEDIMessagingHelper(header);
			helper.SendMessageAndSave(GBMessageTypeList.Codes.New);
			var createdMessage = Factory.LoadTop1<IcsSsGreatBritainEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
			AssertNotNull("ICSEDIMessage should have been created.", createdMessage);
			var expected = IgnoreBreaksAndIndentations($@"
<p><strong>Message number: </strong>{createdMessage.EM_MessageNum}<br>
<strong>Reference number: </strong>C123456<br><strong>Number of items: </strong>0<br>
<strong>Number of packages: </strong>0<br>
<strong>Gross mass: </strong>0<br>
<strong>Declaration place: </strong>UNIT 3, 480 NUDGEE ROAD<br>
<strong>Commercial reference: </strong>C123456</p>");
			AssertContains(expected, createdMessage.EM_MessageInterpretation);
		}

		string IgnoreBreaksAndIndentations(string input)
		{
			return Regex.Replace(input, @"[\r\n]+\s*", string.Empty);
		}
	}
}
