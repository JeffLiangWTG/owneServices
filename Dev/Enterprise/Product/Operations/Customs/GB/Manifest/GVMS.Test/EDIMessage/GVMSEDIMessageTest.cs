using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GVMSEDIMessage))]
	public class GVMSEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<GVMSEDIMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.GbCustomsGVMSManifest, message.EM_ApplicationCode);
			AssertEquals(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, message.EM_MessageType);
		}

		public void TestMessageDataObject()
		{
			var ediMessage = Factory.New<GVMSEDIMessage>();

			ediMessage.EM_MessageText = "We might sometimes get a plain text error that we cannot deserialise";
			var msgDataObj = ediMessage.MessageDataObject;
			AssertNull(msgDataObj);

			ediMessage.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""state"": ""CHECKED IN"",
  ""inspectionRequired"": false,
  ""reportToLocations"": [{
      ""inspectionTypeId"": ""1"",
      ""locationIds"": [
        ""L0029A"", ""L0030A"", ""L0031A""
      ]
    }, {
      ""inspectionTypeId"": ""2"",
      ""locationIds"": [
        ""L0029A"", ""L0031A""
      ]
    }
  ]
}";

			msgDataObj = ediMessage.MessageDataObject;
			AssertNotNull(msgDataObj);
			AssertEquals("GMRO0000F2KW", msgDataObj.gmrId);
		}
	}
}
