using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business;

public class PBNResponseMessageDetails : IPBNResponseMessageDetails
{
	public ImmutableDictionary<string, ResponseDetail> ResponseDetails => responseDetails ??= ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail>
	{
		{ PBNMessageTypes.Codes.CreatePBN, new ResponseDetail(typeof(CreateAndUpdatePBNMessageDefinition), typeof(CreateAndUpdatePBNMessageProcessor)) },
		{ PBNMessageTypes.Codes.UpdatePBN, new ResponseDetail(typeof(CreateAndUpdatePBNMessageDefinition), typeof(CreateAndUpdatePBNMessageProcessor)) },
		{ PBNMessageTypes.Codes.UpdatePBNDeclarations, new ResponseDetail(typeof(CreateAndUpdatePBNMessageDefinition), typeof(CreateAndUpdatePBNMessageProcessor)) },
		{ PBNMessageTypes.Codes.LookupPBN, new ResponseDetail(typeof(LPBDefinition), typeof(LPBPBNMessageProcessor)) },
		{ PBNMessageTypes.Codes.LookupPBNChannel, new ResponseDetail(typeof(LPCDefinition), typeof(LPCPBNMessageProcessor)) },
	});

	public ResponseDetail GetResponseDetail(string messageType, string messageText)
	{
		if(ROSErrorProvider.TryConvert(messageText, out _))
		{
			return new ResponseDetail(typeof(ROSErrorDefinition), typeof(ROSErrorProcessor));
		}

		return ResponseDetails.TryGetValue(messageType, out var types) ? types : ResponseDetail.Empty;
	}

	ImmutableDictionary<string, ResponseDetail> responseDetails;
}
