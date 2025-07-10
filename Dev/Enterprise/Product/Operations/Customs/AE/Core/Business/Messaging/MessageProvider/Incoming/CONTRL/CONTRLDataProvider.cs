using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D23A;
using CONTRLMessage = Enterprise.Edifact.D23A.Messages.CONTRL.CONTRLMessage;

namespace Enterprise.Customs.AE.Business;

sealed class CONTRLDataProvider : ICONTRLDataProvider
{
	readonly Lazy<CONTRLMessage> contrlMessage;

	public CONTRLDataProvider(ZString messageText)
	{
		Argument.NotNullOrEmpty(messageText, nameof(messageText));

		contrlMessage = new Lazy<CONTRLMessage>(() => (CONTRLMessage)new D23AMessageFactory().GetMessage(AECharacterSet.New(), messageText));
	}

	public ZString OutgoingAccessReference => CachedValueHelper.GetValue(ref outgoingAccessReference, () => InterchangeResponse?.OutgoingReference);
	CachedValue<string> outgoingAccessReference;

	public ICONTRLInterchangeResponseProvider InterchangeResponse => CachedValueHelper.GetValue(ref interchangeResponse, GetInterchangeResponse);
	CachedValue<ICONTRLInterchangeResponseProvider> interchangeResponse;

	public ICONTRLMessageResponseProvider MessageResponse => CachedValueHelper.GetValue(ref messageResponse, GetMessageResponse);
	CachedValue<ICONTRLMessageResponseProvider> messageResponse;

	CONTRLInterchangeResponseProvider GetInterchangeResponse()
	{
		return contrlMessage.Value == null ? null : new CONTRLInterchangeResponseProvider(contrlMessage.Value.UCI[0]);
	}

	CONTRLMessageResponseProvider GetMessageResponse()
	{
		var segmentGroup1 = contrlMessage.Value?.Group1;
		return segmentGroup1 == null || segmentGroup1.Count == 0
				? null
				: new CONTRLMessageResponseProvider(segmentGroup1[0]);
	}
}
