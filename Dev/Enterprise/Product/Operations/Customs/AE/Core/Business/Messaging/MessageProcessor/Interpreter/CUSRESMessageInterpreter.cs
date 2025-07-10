using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.AE.Business;

sealed class CUSRESMessageInterpreter : IMessageInterpreter<ICUSRESDataProvider>
{
	public CUSRESMessageInterpreter(EDIMessage message)
	{
		this.message = Argument.NotNull(message, nameof(message));
	}

	readonly EDIMessage message;

	public ZString GetMessageInterpretation(ICUSRESDataProvider dataProvider)
	{
		var result = new ZStringBuilder();
		result.AppendIfNotEmpty(DocumentIdentifierInterpretation(dataProvider.DocumentIdentifier))
			.AppendIfNotEmpty(EntryStatusInterpretation(dataProvider.EntryStatus));

		foreach (var informationRequest in dataProvider.InformationRequests)
		{
			InterpretInformationRequest(informationRequest, result);
		}
		return result.ToStringWithNewLineBetweenAppends();
	}

	void InterpretInformationRequest(IInformationRequest informationRequest, ZStringBuilder result)
	{
		result.AppendIfNotEmpty(RequestTypeInterpretation(informationRequest.RequestType))
			.AppendIfNotEmpty(ErrorSegmentInterpretation(informationRequest.ErrorSegment));

		foreach (var responseDetail in informationRequest.ResponseDetails)
		{
			result.AppendIfNotEmpty(responseDetail);
		}
	}

	ZString DocumentIdentifierInterpretation(ZString documentIdentifier)
		=> InterpretationStrings.CUSRES.GetDocumentIdentifierString(documentIdentifier);

	ZString EntryStatusInterpretation(ZString entryStatus)
	{
		if (entryStatus.IsEmpty)
		{
			return ZString.Empty;
		}
		var statusDescription = GetEntryStatusDescription(entryStatus);
		return InterpretationStrings.CUSRES.GetEntryStatusString(entryStatus, statusDescription);
	}

	ZString RequestTypeInterpretation(ZString requestType)
	{
		var requestTypeDescription = new InformationRequestTypeList().GetDescriptionFromCode(requestType);
		return requestTypeDescription == null ? ZString.Empty
				: InterpretationStrings.CUSRES.GetInformationRequestString(requestTypeDescription);
	}

	ZString ErrorSegmentInterpretation(ZString errorSegment)
	{
		if (errorSegment.IsEmpty)
		{
			return ZString.Empty;
		}
		return InterpretationStrings.CUSRES.GetErrorSegmentString(errorSegment);
	}

	ZString GetEntryStatusDescription(ZString statusCode)
	{
		return statusCode.IsEmpty
		? ZString.Empty
		: new RefCusCodeListTypesListProvider()
			.GetList(message.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest, RefCusCodeListTypes.CustomsManifestStatus, ZDateTime.Today, Array.Empty<ZString>())
			.GetDescriptionFromCode(statusCode);
	}
}
