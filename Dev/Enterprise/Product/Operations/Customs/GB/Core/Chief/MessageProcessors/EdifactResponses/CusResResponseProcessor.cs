
namespace Enterprise.Customs.GB.Chief.CusRes
{
	using System;
	using CargoWise.BrandManager;
	using CargoWise.Common;
	using Enterprise.BatchProcessor;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.Integration;
	using Enterprise.Messaging.MessageProcessors;
	using Enterprise.ZArchitecture.Core;

	/// <summary>
	/// Will handle interchanges of type CUSRES and CONTRL
	/// </summary>
	public class CusResResponseProcessor : ApplicationTypeMessageProcessor
	{
		public CusResResponseProcessor
			(ILogger serviceLogger, LoggingInformation logger)  // <--- that is daft
			: base(logger)
		{
			this.serviceLogger = serviceLogger;
		}

		readonly ILogger serviceLogger;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override void ProcessMessageCore(EDIMessage message)
		{
			Argument.NotNull(message, "Message");

			CusResHandler_BASE messageHandler;
			if (message.CharacterSet == null)
			{
				throw new Exception("Inbound message's character set could not be determined. " + message.EM_MessageText.SubstringSafe(0, 100));
			}

			Edifact.Auto.SegmentGroup edifactMessage = message.GetAutoEdifactMessageUsingNamedFactory(GbEdiMessageFactory.Factory);
			if (edifactMessage == null)
			{
				if (IsUkCtrl(message))
				{   // UKCTRL is not a standard United Nations edifact message and therefore we have no definition for it.
					messageHandler = new CusResHandler_UKCTRL(serviceLogger);
				}
				else
				{
					// We cannot process this message as we have not set up a factory to understand it.
					serviceLogger.Log(LogType.Warning, delegate
					{ return string.Format("Unexpected message type. Cannot process this message because {0} is not configured for its specific type.  Reprocessing will not fix this.  The message has been set to failed and will be ignored. Please contact {0} support regarding adding this message type to the suite of supported messages. \r\nMessage number: {1}. Message text (partial): {2}. Message PK: {3}.", BrandingFactory.Instance.ProductName, message.EM_MessageNum, message.EM_MessageText.SubstringSafe(0, 100), message.PK.ToString()); });
					message.EM_Status = EDIMessage.Status.Failed;
					return;
				}
			}
			else
			{
				messageHandler = GetMessageHandlerBasedOnType(edifactMessage, message);
			}

			if (messageHandler != null)
			{
				messageHandler.DoAllProcessing(message);
			}
		}

		bool IsUkCtrl(EDIMessage message)
		{
			// UKCTRL is not a standard United Nations message, so we parse it as simple text.  We only need the FTX elements from it.
			return message.EM_MessageText.Contains(@"UKCTRL\1\912\UK") ||
					 message.EM_MessageText.Contains(@"UKCTRL:1:912:UK");
		}

		CusResHandler_BASE GetMessageHandlerBasedOnType(Edifact.Auto.SegmentGroup segmentGroupType, EDIMessage message)
		{
			Argument.NotNull(segmentGroupType, "segmentGroupType");

			var (cusRes, cusresTypeCode) = GetCUSRESMessageAndCode(segmentGroupType, message);
			var (cusDecInbound, cusdecTypeCode) = GetCUSDECMessageAndCode(segmentGroupType, message);

			if (segmentGroupType is Edifact.D04A.Messages.CONTRL.CONTRLMessage) // NB: if we receive a 4:1:UN:Contrl message, it will look like a D:04A:UN:Contrl message, because we added this latter registration to the D04A factory.
			{
				return new CusResHandler_CONTRL(serviceLogger);
			}
			else if (segmentGroupType is EdiFact.UKCINV.UkCinvMessage)
			{
				return new CusResHandler_UKCINV(serviceLogger);
			}
			serviceLogger.Log(LogType.Error, () => "Could not process received EDIFACT message as it was of an unrecognised version.");
			message.EM_Status = EDIMessage.Status.Failed;
			return null;
		}

		(Edifact.D04A.Messages.CUSDEC.CUSDECMessage message, string code) GetCUSDECMessageAndCode(Edifact.Auto.SegmentGroup segmentGroupType, EDIMessage inboundMessage)
		{
			var code = string.Empty;
			var cusDecInbound = segmentGroupType as Edifact.D04A.Messages.CUSDEC.CUSDECMessage;
			if (cusDecInbound != null)
			{
				if (cusDecInbound.BGM.Count > 0)
				{
					code = cusDecInbound.BGM[0].DocumentMessageName.DocumentNameCode.ToString();
				}
				else
				{
					serviceLogger.Log(LogType.Error, $"Message {inboundMessage.EM_MessageNum} lacked a BGM segment, cannot process it");
				}
			}
			return (cusDecInbound, code);
		}

		const string UNKNOWN_ERROR = "UNKNOWN";

		(Edifact.D04A.Messages.CUSRES.CUSRESMessage message, string code) GetCUSRESMessageAndCode(Edifact.Auto.SegmentGroup segmentGroupType, EDIMessage inboundMessage)
		{
			var code = string.Empty;
			var cusRes = segmentGroupType as Edifact.D04A.Messages.CUSRES.CUSRESMessage;
			if (cusRes != null)
			{
				if (cusRes.BGM.Count > 0)
				{
					code = cusRes.BGM[0].DocumentMessageName.DocumentNameCode.ToString();
				}
				else
				{
					serviceLogger.Log(LogType.Error, $"Message {inboundMessage.EM_MessageNum} lacked a BGM segment.  This is known error on CHIEF that HMRC refuse to fix. Processing will be attempted without this essential information but success cannot be guaranteed.  It will be assumed that the response is a CUSRES relating to a CUSDEC.");
					code = UNKNOWN_ERROR;
				}
			}
			return (cusRes, code);
		}

		protected override string ApplicationCodeCore
		{
			get { return ApplicationCodeShared; }
		}

		public static string ApplicationCodeShared
		{
			get { return EDIInterchange.ApplicationCodes.GbEdifactShared; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return ApplicationCodeList.Descriptions.GbEdifactShared; }
		}
	}
}
