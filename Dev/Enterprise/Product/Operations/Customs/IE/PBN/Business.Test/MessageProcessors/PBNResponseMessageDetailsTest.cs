using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

public class PBNResponseMessageDetailsTest : TestCaseWithFactory
{
	public void TestGetResponseDetail()
	{
		var details = new PBNResponseMessageDetails();

		var detailsLPC = details.GetResponseDetail(PBNMessageTypes.Codes.LookupPBN, null);
		CombineAssertions("LPB", () =>
		{
			AssertEquals("XmlObjectType", typeof(LPBDefinition), detailsLPC.XmlObjectType);
			AssertEquals("ProcessorType", typeof(LPBPBNMessageProcessor), detailsLPC.ProcessorType);
		});

		var rosErrorText = JsonSerializer.Serialize(ROSErrorInterpreterTest.NewDataObjectToTest());
		var detailsROSError = details.GetResponseDetail(PBNMessageTypes.Codes.LookupPBN, rosErrorText);
		CombineAssertions("ROS Error", () =>
		{
			AssertEquals("XmlObjectType", typeof(ROSErrorDefinition), detailsROSError.XmlObjectType);
			AssertEquals("ProcessorType", typeof(ROSErrorProcessor), detailsROSError.ProcessorType);
		});

		var detailsRUniversalEventErrorWithValidationErrors = details.GetResponseDetail(PBNMessageTypes.Codes.LookupPBN, PBNMessageTestHelper.universalInterchangeEventText);
		CombineAssertions("Universal Event Error", () =>
		{
			AssertEquals("XmlObjectType", typeof(ROSErrorDefinition), detailsRUniversalEventErrorWithValidationErrors.XmlObjectType);
			AssertEquals("ProcessorType", typeof(ROSErrorProcessor), detailsRUniversalEventErrorWithValidationErrors.ProcessorType);
		});

		var invalidTextWithValidationErrors = details.GetResponseDetail(PBNMessageTypes.Codes.LookupPBN, "<validationErrors<<>>");
		CombineAssertions("Incorrect text", () =>
		{
			AssertEquals("XmlObjectType", typeof(LPBDefinition), invalidTextWithValidationErrors.XmlObjectType);
			AssertEquals("ProcessorType", typeof(LPBPBNMessageProcessor), invalidTextWithValidationErrors.ProcessorType);
		});
	}
}
