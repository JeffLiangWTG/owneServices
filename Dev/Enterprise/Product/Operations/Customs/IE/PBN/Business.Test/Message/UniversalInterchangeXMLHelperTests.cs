using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

class UniversalInterchangeXMLHelperTest : TestCase
{
	public void TestTryGetReasonNodeFromUniversalInterchangeTypeXML_ValidXML()
	{
		const string jsonInUniversalInterchangeXML = @"{""status"":""REJECTED"",""validationErrors"":[{""code"":""RORO-0005"",""path"":""declarations[0].declarationId"",""description"":""Invalid Format MRN 21IEDUB1E8AC14A6R9""}]}";
		AssertEquals("Expected reason node value when valid xml", jsonInUniversalInterchangeXML, UniversalInterchangeXMLHelper.TryGetReasonNodeFromUniversalInterchangeTypeXML(PBNMessageTestHelper.universalInterchangeEventText));
	}

	public void TestTryGetReasonNodeFromUniversalInterchangeTypeXML_InvalidXML()
	{
		var invalidXML = @"<asdf.<<>";
		AssertNull("Expected reason node value when invalid xml", UniversalInterchangeXMLHelper.TryGetReasonNodeFromUniversalInterchangeTypeXML(invalidXML));
	}
}
