using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.AE.Business;

sealed class CONTRLMessageInterpreter : IMessageInterpreter<ICONTRLDataProvider>
{
	public CONTRLMessageInterpreter(EDIMessage message)
	{
		this.message = Argument.NotNull(message, nameof(message));
		tableCreator = new HtmlTableCreator(new string[3]
		{
			Res.GetString("7d3db87a-48cd-4c5b-948d-231e80d6fa38", "No."),
			Res.GetString("fb1ee2fb-7740-408e-86f1-208a47cb54eb", "Interpretation"),
			Res.GetString("42af5c2d-8b1e-4004-b403-33c71a1adbb8", "Segment"),
		});
	}

	readonly EDIMessage message;
	readonly HtmlTableCreator tableCreator;
	int lineNo = 1;

	public ZString GetMessageInterpretation(ICONTRLDataProvider dataProvider)
	{
		var result = new ZStringBuilder();
		InterpreteInterchangeResponse(dataProvider.InterchangeResponse, result);
		InterpreteMessageResponse(dataProvider, result);
		return tableCreator.ToHtml();
	}

	void InterpreteInterchangeResponse(ICONTRLInterchangeResponseProvider interchangeResponse, ZStringBuilder result)
	{
		InterpreteResponse(InterpretationStrings.ResponseTypes.Interchange, interchangeResponse, result);
	}

	void InterpreteMessageResponse(ICONTRLDataProvider dataProvider, ZStringBuilder result)
	{
		var messageResponse = dataProvider.MessageResponse;
		if (messageResponse == null)
		{
			return;
		}

		InterpreteResponse(InterpretationStrings.ResponseTypes.Message, messageResponse, result);
		foreach (var segmentError in messageResponse.SegmentErrors)
		{
			InterpreteSegmentError(segmentError, result);
			var segment = ZString.Empty;
			if (int.TryParse(segmentError.SegmentPosition, out var position))
			{
				segment = GetOutgoingMessageSegment(dataProvider).ElementAtOrDefault(position - 1);
			}
			WriteToTable(result, segment);
		}
	}

	void InterpreteResponse(string responseType, ICONTRLInterchangeResponseProvider response, ZStringBuilder result)
	{
		if (response == null)
		{
			return;
		}
		result.AppendIfNotEmpty(OutgoingReferenceInterpretation(responseType, response.OutgoingReference))
			.AppendIfNotEmpty(ActionCodeInterpretation(responseType, response.ActionCode))
			.AppendIfNotEmpty(SyntaxErrorInterpretation(response.SyntaxErrorCode))
			.AppendIfNotEmpty(ErrorPositionInterpretation(response.ErrorDataElementPosition, response.ErrorDataElementComponentPosition));
		WriteToTable(result, ZString.Empty);
	}

	void InterpreteSegmentError(ISegmentErrorProvider segmentError, ZStringBuilder result)
	{
		result.AppendIfNotEmpty(SegmentPositionInterpretation(segmentError.SegmentPosition))
			.AppendIfNotEmpty(SyntaxErrorInterpretation(segmentError.SegmentSyntaxErrorCode));

		foreach (var dataElementError in segmentError.DataElementErrors)
		{
			result.AppendIfNotEmpty(SyntaxErrorInterpretation(dataElementError.SyntaxErrorCode))
				.AppendIfNotEmpty(ErrorPositionInterpretation(dataElementError.ErrorDataElementPosition, dataElementError.ErrorDataElementComponentPosition));
		}
	}

	void WriteToTable(ZStringBuilder builder, string segment)
	{
		tableCreator.WriteRow(lineNo, builder.ToStringWithNewLineBetweenAppends(), segment);
		lineNo++;
		builder.Clear();
	}

	ZString OutgoingReferenceInterpretation(string responseType, ZString reference)
		=> InterpretationStrings.CONTRL.GetOutgoingReference(responseType, reference);

	ZString ActionCodeInterpretation(string responseType, ZString actionCode)
	{
		return actionCode.ToString() switch
		{
			AEConstants.Messaging.ActionCodedList.ActionCoded4 => InterpretationStrings.CONTRL.GetInvalidHeaderTrailerString(responseType),
			AEConstants.Messaging.ActionCodedList.ActionCoded7 => InterpretationStrings.CONTRL.GetInvalidContentString(responseType),
			AEConstants.Messaging.ActionCodedList.ActionCoded8 => InterpretationStrings.CONTRL.GetSubmissionAcceptedString(responseType),
			_ => ZString.Empty
		};
	}

	ZString SyntaxErrorInterpretation(ZString errorCode)
	{
		if (errorCode.IsEmpty)
		{
			return ZString.Empty;
		}

		var errorDescription = GetErrorDescriptionForCode(errorCode);
		return InterpretationStrings.CONTRL.GetSyntaxErrorString(errorCode, errorDescription);
	}

	ZString ErrorPositionInterpretation(ZString dataElementPosition, ZString dataElementComponentPosition)
	{
		if (dataElementPosition.IsEmpty)
		{
			return ZString.Empty;
		}
		return InterpretationStrings.CONTRL.GetErrorPositionString(dataElementPosition, dataElementComponentPosition);
	}

	ZString SegmentPositionInterpretation(ZString segmentPosition)
	{
		if (segmentPosition.IsEmpty)
		{
			return ZString.Empty;
		}
		return InterpretationStrings.CONTRL.GetErrorSegmentPositionString(segmentPosition);
	}

	ZString GetErrorDescriptionForCode(ZString errorCode)
	{
		return errorCode.IsEmpty
		? ZString.Empty
		: new RefCusCodeListTypesListProvider()
			.GetList(message.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest, RefCusCodeListTypes.ErrorCode, ZDateTime.Today, Array.Empty<ZString>())
			.GetDescriptionFromCode(errorCode);
	}

	ZString[] GetOutgoingMessageSegment(ICONTRLDataProvider dataProvider)
	{
		if (segmentsCache == null)
		{
			var characterSet = AECharacterSet.New();
			var outgoingMessageSegments = Utils.SplitSegment(message.Factory.GetOutboundMessage(dataProvider.OutgoingAccessReference)?.EM_MessageText ?? ZString.Empty, characterSet);
			segmentsCache = outgoingMessageSegments.Select(e => e.Replace(characterSet.SegmentDelimiter, "")).ToArray();
		}
		return segmentsCache;
	}
	ZString[] segmentsCache;
}
