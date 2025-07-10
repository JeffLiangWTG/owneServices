using Enterprise.BatchProcessor;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(AEInterchangeUnpacker))]
sealed class AEInterchangeUnpackerTest : InterchangeUnpackerTest<AEInterchangeUnpacker>
{
	public void TestUnknownInterchangeType()
	{
		var interchange = CreateTestInterchange("XXX", "XXXXX");
		var logger = new LoggingInformation();

		var unpacker = new AEInterchangeUnpacker();
		var unpackResult = unpacker.Unpack(interchange, null, null, logger);

		AssertEquals("Unknown interchange type", "Unknown interchange type XXX.", unpackResult.ErrorReason);
	}

	public void TestCONTROLInterchange()
	{
		var interchangeBodyText = $"UNB+UNOB:4::2:4'UNH+123H456+{MessageTypeList.SyntaxAndServiceReportMessage}'UNZ+1'";
		AssertUnpackForKnownType(AEConstants.Messaging.MessageTypes.CONTRL, interchangeBodyText);
	}

	public void TestCUSRESInterchange()
	{
		var interchangeBodyText = $"UNB+UNOB:4::2:4'UNH+123H456+{MessageTypeList.CustomsResponseMessage}'UNZ+1'";
		AssertUnpackForKnownType(AEConstants.Messaging.MessageTypes.CUSRES, interchangeBodyText);
	}

	public void TestXTERRInterchange()
	{
		var interchangeBodyText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<Body>
  <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  </UniversalEvent>
</Body>
</UniversalInterchange>
";
		AssertUnpackForKnownType(AEConstants.Messaging.MessageTypes.XTTERR, interchangeBodyText);
	}

	public void TestDOCSUCInterchange()
	{
		AssertUnpackForDOCType(AEConstants.Messaging.MessageTypes.DOCSUC);
	}

	public void TestDOCERRInterchange()
	{
		AssertUnpackForDOCType(AEConstants.Messaging.MessageTypes.DOCERR);
	}

	void AssertUnpackForKnownType(string interchangeType, string interchangebodyText)
	{
		var interchange = CreateTestInterchange(interchangeType, interchangebodyText);
		var logger = new LoggingInformation();

		var unpacker = new AEInterchangeUnpacker();
		var unpackResult = unpacker.Unpack(interchange, null, null, logger);
		Assert($"Success for known type {interchangeType}", unpackResult.IsSuccess);
		AssertEquals(1, unpackResult.EdiMessages.Count);
	}

	void AssertUnpackForDOCType(string interchangeType)
	{
		var interchange = CreateTestInterchange(interchangeType, "");
		var outgoingMessage = Factory.New<EDIMessage>();
		var logger = new LoggingInformation();

		var unpacker = new AEInterchangeUnpacker();
		var unpackResult = unpacker.Unpack(interchange, null, outgoingMessage, logger);
		Assert($"Success for known type {interchangeType}", unpackResult.IsSuccess);
		AssertEquals(1, unpackResult.EdiMessages.Count);
	}

	protected override string[] ApplicationCodes => new[] { ApplicationCodeList.Codes.UAECustoms };

	EDIInterchange CreateTestInterchange(string interchangeType, string interchangeBodyText)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		interchange.EI_InterchangeNum = "INT001";
		interchange.EI_BodyText = interchangeBodyText;
		interchange.EI_InterchangeType = interchangeType;
		return interchange;
	}
}
