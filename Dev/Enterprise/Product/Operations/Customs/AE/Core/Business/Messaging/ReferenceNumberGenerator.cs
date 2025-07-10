using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Registry;
using Enterprise.Environment;

namespace Enterprise.Customs.AE.Business;

public sealed class ReferenceNumberGenerator
{
	public ReferenceNumberGenerator(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly BusinessObjectFactory factory;

	public string GenerateMessageReferenceNumber(ZString applicationCode, ZString messageType) => GenerateReferenceNumber(MessageIdentifier, applicationCode, messageType, MessageStaticCharacter);

	public string GenerateInterchangeReferenceNumber(ZString applicationCode, ZString messageType) => GenerateReferenceNumber(InterchangeIdentifier, applicationCode, messageType, InterchangeStaticCharacter);

	public string GenerateDocumentReferenceNumber()
	{
		var messageFilerMPCICode = AECustomsRegistry.Instance.NAICServiceProviderCode.Value;
		var fountainKey = $"AECustomsDocumentNumber{messageFilerMPCICode}";
		var sequenceNumber = Env.NumberFountains.GetAECustomsNumberFountain(fountainKey, DocumentNumberSequenceLength).GetNextFormatted(factory);
		return FormatMPCICode(messageFilerMPCICode) + sequenceNumber;

		string FormatMPCICode(string code)
		{
			const int expectedMPCICodeLength = 7;
			if (code.Length > expectedMPCICodeLength)
			{
				return code.Substring(0, expectedMPCICodeLength);
			}
			return code.PadLeft(expectedMPCICodeLength, '0');
		}
	}

	string GenerateReferenceNumber(ZString messageOrInterchange, ZString applicationCode, ZString messageOrInterchangeType, ZString staticCharacter)
	{
		Argument.NotNullOrEmpty(applicationCode, nameof(applicationCode));
		Argument.NotNullOrEmpty(messageOrInterchangeType, nameof(messageOrInterchangeType));

		var fountainKey = $"AECustomsReferenceNumber{messageOrInterchange}{applicationCode}{messageOrInterchangeType}";

		var timePrefix = ZDateTime.UtcNow.ToString("HHmmss");
		var fountain = Env.NumberFountains.GetAECustomsNumberFountain(fountainKey, ReferenceNumberSequenceLength);
		var numberSequenceSuffix = fountain.GetNextFormatted(factory);
		return timePrefix + staticCharacter + numberSequenceSuffix;
	}

	const string MessageIdentifier = "M";
	const string MessageStaticCharacter = "H";
	const string InterchangeIdentifier = "I";
	const string InterchangeStaticCharacter = "B";
	const int ReferenceNumberSequenceLength = 7;
	const int DocumentNumberSequenceLength = 13;
}
