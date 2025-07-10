using System;
using System.Data;
using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Bordereau;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CHEDIMessage : EDIMessage, Integration.Customs.CH.IEDIMessage, IDocumentSupportable, ICHEDIMessage
{
	public CHEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodes.CHCustomsEdec;
	}

	public override bool UsesPlaceHolders => EM_MessageType == MessageTypeCodeList.Codes.ECM;

	protected override string MessageNumberPlaceHolderOverride => MessageNumberPlaceHolderHtml;

	protected override string GetMessageReferenceNumber()
	{
		return Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EM_ApplicationCode).GetNextFormatted(Factory);
	}

	protected override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<MessageSubTypeCodeList>();

	string messageInterpretation;
	public override ZString EM_MessageInterpretation
	{
		get => messageInterpretation ?? (messageInterpretation = GetMessageInterpretation());
		set
		{
			base.EM_MessageInterpretation = value;
			messageInterpretation = value;
		}
	}

	ZString GetMessageInterpretation() => MessagePrettyFormatterFactory.GetMessageFormatter(this)?.GetFormattedText() ?? base.EM_MessageInterpretation;

	public override ZString EM_MessageText
	{
		get => base.EM_MessageText;
		set
		{
			var oldValue = EM_MessageText;
			base.EM_MessageText = value;

			if (EM_MessageText != oldValue)
			{
				messageAnalyzerLoaded = false;
				universalEventData = null;
				messageInterpretation = null;
			}
		}
	}

	public IMessageAnalyzer MessageAnalyzer
	{
		get
		{
			if (!messageAnalyzerLoaded)
			{
				var messageText = IsUniversalEvent ? (ZString)UniversalEventData.GetResponseMessage() : EM_MessageText;
				var analyzerType = GetMessageAnalyzerType();

				messageAnalyzer = messageText.IsEmpty || analyzerType == null ? null : MessageSchemaDecider.GetSpecificMessageAnalyzer(analyzerType, messageText);
				messageAnalyzerLoaded = true;
			}
			return messageAnalyzer;
		}
	}
	IMessageAnalyzer messageAnalyzer;
	bool messageAnalyzerLoaded;

	public IMessageDetail MessageDetail => MessageAnalyzer?.MessageDetail;

	Type GetMessageAnalyzerType()
	{
		if (!IsTransmitMessage)
		{
			switch (EM_MessageType)
			{
				case MessageTypeCodeList.Codes.Import:
				case MessageTypeCodeList.Codes.Export:
					return typeof(IGoodsDeclarationsResponseAnalyzer);
				case MessageTypeCodeList.Codes.EBD:
					return typeof(IDocumentImportResponseAnalyzer);
				case MessageTypeCodeList.Codes.ECM:
					switch (EM_MessageSubType)
					{
						case MessageSubTypeCodeList.Codes.Request:
							return typeof(IEdecComplaintRequestAnalyzer);
						default:
							return typeof(IEdecComplaintResponseAnalyzer);
					}
				case MessageTypeCodeList.Codes.EVV:
					return typeof(IEvvResponseAnalyzer);
				case MessageTypeCodeList.Codes.BOR:
					return typeof(IEdecBordereauResponseAnalyzer);
				case MessageTypeCodeList.Codes.MSG:
					switch (EM_ApplicationCode)
					{
						case ApplicationCodes.CHCustomsCharteraOutput:
							return typeof(ICharteraResponseAnalyzer);
						default:
							return typeof(IPassarResponseAnalyzer);
					}
			}
		}
		else
		{
			switch (EM_MessageType)
			{
				case MessageTypeCodeList.Codes.ECM:
					return typeof(IEdecComplaintRequestAnalyzer);
				case MessageTypeCodeList.Codes.REQ:
					switch (EM_MessageSubType)
					{
						case MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest:
							return typeof(IDocumentDeliveryRequestAnalyzer);
						default:
							return null;
					}
			}
		}
		return null;
	}

	public bool IsUniversalEvent
	{
		get
		{
			var result = false;
			if (EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive)
			{
				switch (EM_MessageType)
				{
					case MessageTypeCodeList.Codes.PassarNcts:
					case MessageTypeCodeList.Codes.MSL:
					case MessageTypeCodeList.Codes.MSG:
					case MessageTypeCodeList.Codes.REQ:
					case MessageTypeCodeList.Codes.Import when EM_MessageSubType == MessageSubTypeCodeList.Codes.Rejected:
					case MessageTypeCodeList.Codes.EBD when EM_MessageSubType == MessageSubTypeCodeList.Codes.Rejected:
					case MessageTypeCodeList.Codes.ECM when EM_MessageSubType == MessageSubTypeCodeList.Codes.Rejected:
					case MessageTypeCodeList.Codes.Export when EM_MessageSubType == MessageSubTypeCodeList.Codes.Rejected:
						result = true;
						break;
				}
			}
			return result;
		}
	}

	public UniversalEventWrapper UniversalEventData => universalEventData ?? (universalEventData = IsUniversalEvent && !EM_MessageText.IsEmpty ? new UniversalEventWrapper(EM_MessageText) : null);
	UniversalEventWrapper universalEventData;

	public DocumentSupporter DocumentSupporter => ducumentSupporter ?? (ducumentSupporter = new CHEDIMessageDocumentSupporter(this));
	DocumentSupporter ducumentSupporter;
}
