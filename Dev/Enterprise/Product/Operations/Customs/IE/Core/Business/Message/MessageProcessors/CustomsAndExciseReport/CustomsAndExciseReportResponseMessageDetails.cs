using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business;

public static class CustomsAndExciseReportResponseMessageDetails
{
	public static ResponseDetail GetResponseDetail(string messageType, string messageText)
	{
		if (IsXML(messageText))
		{
			return new ResponseDetail(typeof(MessageAcknowledgement), typeof(CustomsAndExciseReportErrorProcessor));
		}
		return ResponseDetails.TryGetValue(messageType, out var types) ? types : ResponseDetail.Empty;
	}

	public static ImmutableDictionary<string, ResponseDetail> ResponseDetails => responseDetails ??= ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail>
	{
		{ CustomsAndExciseReportTypeList.Codes.PSR, new ResponseDetail(typeof(PSRMessage), typeof(PSRProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.PCI, new ResponseDetail(typeof(PCIMessage), typeof(PCIProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.PCT, new ResponseDetail(typeof(PCTMessage), typeof(PCTProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.PTT, new ResponseDetail(typeof(PTTMessage), typeof(PTTProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.DSR, new ResponseDetail(typeof(DSRMessage), typeof(DSRProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.DCT, new ResponseDetail(typeof(DCTMessage), typeof(DCTProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.DTT, new ResponseDetail(typeof(DTTMessage), typeof(DTTProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.UDR, new ResponseDetail(typeof(UDRMessage), typeof(UDRProcessor)) },
		{ CustomsAndExciseReportTypeList.Codes.BAL, new ResponseDetail(typeof(BALMessage), typeof(BALProcessor)) },
	});

	[ThreadStatic]
	static ImmutableDictionary<string, ResponseDetail> responseDetails;

	static bool IsXML(string text)
	{
		if (text != null)
		{
			text = text.Trim();
			return text.StartsWith("<") && text.EndsWith(">");
		}
		return false;
	}
}
