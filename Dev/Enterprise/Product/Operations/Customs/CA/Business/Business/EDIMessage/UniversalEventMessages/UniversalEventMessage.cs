using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Edifact.D13A.Messages.GOVCBR;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class UniversalEventMessage : EDIMessage, IDocumentSupportable
	{
		public UniversalEventMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			EM_MessageType = EDIMessageTypeList.Codes.XDC;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
		}

		UniversalEvent UniversalEvent
		{
			get
			{
				if (universalEvent == null)
				{
					universalEvent = new CachedProperty<UniversalEvent>(Factory, () => { return GetEM_MessageTextReader()?.Parse<UniversalEvent>(); });
				}
				return universalEvent.Value;
			}
		}
		CachedProperty<UniversalEvent> universalEvent;

		#region Overrides

		public override ZString XMLCustomsMessageType
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.XMLCustomsMessageType); }
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageSubType
		{
			get
			{
				var customsMessageType = XMLCustomsMessageType;
				return customsMessageType.IsEmpty ? base.EM_MessageSubType : customsMessageType;
			}
		}

		public override ZString EM_MessageInterpretation
		{
			get
			{
				var result = MessageInterpretationNoteManager.Value;
				if (result.IsEmpty && !EM_MessageText.IsEmpty)
				{
					result = InterpretationGenerator.GetInterpretatedHTML();
				}
				return result.IsEmpty ? EM_MessageText : result;
			}
			set
			{
				MessageInterpretationNoteManager.Value = value;
				EM_MessageInterpretationInfo.RefreshBinding();
			}
		}

		protected override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<UniversalEventMessageTypes>();

		public override ZString ReferenceNumber
		{
			get
			{
				var result = ZString.Empty;
				var uxmlEvent = UniversalEvent;
				if (uxmlEvent != null)
				{
					result = uxmlEvent.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.OrganizationReference);
					if (result.IsEmpty)
					{
						result = UniversalEventMessageProcessorHelper.GetUniqueIDValueFromDataTarget(uxmlEvent);
					}
					if (result.IsEmpty)
					{
						ZStringBuilder builder = new ZStringBuilder();
						foreach (D4MessageInterpretationGenerator.RelatedDocument relatedDocument in UniversalEventMessageProcessorHelper.GetRelatedDocument(uxmlEvent))
						{
							builder.AppendFormat("{0}{1}", relatedDocument.DocumentNumber, relatedDocument.DocumentType.IsEmpty ? string.Empty : "/" + relatedDocument.DocumentType);
						}
						result = builder.ToStringWithDelimiterBetweenAppends(",");
					}
				}
				return result;
			}
		}

		public override ZDateTime RNSProcessingDate => UniversalEvent.GetEventTime().ToZDateTime();

		public override ZString StatusDescription
		{
			get
			{
				var result = ZString.Empty;
				var umxlEvent = UniversalEvent;
				if (umxlEvent != null)
				{
					var statusCode = umxlEvent.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.Status.Name);
					if (!statusCode.IsEmpty)
					{
						result = ZString.Format("{0} - {1}", statusCode, CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, statusCode));
					}
				}
				return result;
			}
		}

		public override ZInt EDIFACTInterchangeNumber
		{
			get
			{
				var result = ZInt.Zero;
				var umxlEvent = UniversalEvent;
				if (umxlEvent != null)
				{
					var number = umxlEvent.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.InterchangeNumber);
					if (!number.IsEmpty)
					{
						result = ZInt.ParseSafe(number, ZInt.Zero);
					}
				}
				return result;
			}
		}

		public override ZInt EDIFACTMessageNumber
		{
			get
			{
				var result = ZInt.Zero;
				var umxlEvent = UniversalEvent;
				if (umxlEvent != null)
				{
					var number = umxlEvent.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.MessageNumber);
					if (!number.IsEmpty)
					{
						result = ZInt.ParseSafe(number, ZInt.Zero);
					}
				}
				return result;
			}
		}

		public override ZString RawMessageInterpretation
		{
			get
			{
				if (rawMessageInterpretation == null)
				{
					var sentHtml = InterpretationGenerator.SentRawMessage.IsEmpty ? string.Empty : InterpretationGenerator.ToHtml(InterpretationGenerator.SentRawMessage);
					rawMessageInterpretation = string.IsNullOrEmpty(sentHtml) ? "" : "Send:\r\n" + sentHtml;
					var responseHtml = InterpretationGenerator.ResponseRawMessage.IsEmpty ? string.Empty : InterpretationGenerator.ToHtml(InterpretationGenerator.ResponseRawMessage);
					rawMessageInterpretation += string.IsNullOrEmpty(responseHtml) ? "" : "Results:\r\n" + responseHtml;
				}
				return rawMessageInterpretation;
			}
		}
		string rawMessageInterpretation;

		public override ZString RawMessage
		{
			get
			{
				if (rawMessage == null)
				{
					rawMessage = InterpretationGenerator.SentRawMessage.IsEmpty ? string.Empty : "Send:\r\n" + ReplaceForEdifactMessage(InterpretationGenerator.SentRawMessage) + "\r\n";
					rawMessage += InterpretationGenerator.ResponseRawMessage.IsEmpty ? string.Empty : "Results:\r\n" + ReplaceForEdifactMessage(InterpretationGenerator.ResponseRawMessage) + "\r\n";
				}
				return rawMessage;
			}
		}
		string rawMessage;

		ZString ReplaceForEdifactMessage(ZString input)
		{
			return input.Replace("\r", ZString.Empty).Replace("\n", ZString.Empty).Replace("\t", "").Replace("\'", "\r\n");
		}

		UniversalEventMessageInterpretationGenerator InterpretationGenerator => interpretationGenerator ?? (interpretationGenerator = new UniversalEventMessageInterpretationGenerator(Factory, this));

		UniversalEventMessageInterpretationGenerator interpretationGenerator;

		#region Properties

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public IEnumerable<ZString> StatusCodes
		{
			get
			{
				var statusCodes = UniversalEventMessageProcessorHelper.GetContextValuesByType(UniversalEvent?.ContextCollection, UniversalEventMessageProcessorConstants.ContextType.Status.Name);
				return statusCodes ?? new List<ZString>();
			}
		}

		public ZString StatusCodesCombined => ZString.Join("|", StatusCodes.ToArray());

		DocumentSupporter IDocumentSupportable.DocumentSupporter => DocumentSupporter;

		UniversalEventMessageDocumentSupporter DocumentSupporter => new UniversalEventMessageDocumentSupporter(this);

		#endregion

		#endregion

		#region New Properties

		public ZString EventType
		{
			get { return UniversalEvent?.EventType ?? ZString.Empty; }
		}

		public GOVCBRMessage GOVCBR
		{
			get { return govcbr ?? (govcbr = InterpretationGenerator.GetGOVCBRMessageWithCharSet(InterpretationGenerator.RawMessage).EDIFactMessage); }
		}
		GOVCBRMessage govcbr;

		#endregion
	}
}
