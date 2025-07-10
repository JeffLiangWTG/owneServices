using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRBaseMessageProcessor : ApplicationTypeMessageProcessor
	{
		public CMRBaseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore
		{
			get { return messageFriendlyName; }
		}
		string messageFriendlyName = "";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.CMR;

		protected override bool RequiresPreProcessingCore => true;

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			CMRStatusRecalculationSuspender.SuspendStatusRecalculation(message.Factory);
			GetMessageProcessor(message)?.PreProcessMessage(message);
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			CMRStatusRecalculationSuspender.ResumeStatusRecalculation(message.Factory);
			GetMessageProcessor(message)?.ProcessMessage(message);
		}

		CustomsMessageProcessor GetMessageProcessor(EDIMessage message)
		{
			CustomsMessageProcessor processor = null;

			try
			{
				var edifactMessage = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet());
				if (edifactMessage is Edifact.D99B.Messages.CUSRES.CUSRESMessage cusresMessage)
				{
					var docName = cusresMessage.BGM[0].DocumentMessageName.DocumentName;
					processor = GetCUSRESProcessor(docName);
					if (processor == null)
					{
						Logger.ContinueDebugLog("Don't know how to process CUSRES documents of type: " + docName + ".");
						message.EM_Status = EDIMessage.Status.Error;
					}
				}
				else if (edifactMessage is Edifact.D99B.Messages.CONTRL.CONTRLMessage)
				{
					processor = GetCONTROLProcessor();
				}
				else
				{
					Logger.LogWarning("Don't know how to process message; it is not a CUSRES; status set to Error.");
					message.EM_Status = EDIMessage.Status.Error;
				}

				if (processor != null)
				{
					messageFriendlyName = processor.MessageFriendlyName;
				}
			}
			catch (InvalidFormatException)
			{
				message.EM_Status = EDIMessage.Status.Failed;
			}

			return processor;
		}

		protected virtual internal CMRMessageResponseProcessor GetCUSRESProcessor(string docName)
		{
			switch (docName)
			{
				case "EXDR":
					return new EXDRMessageProcessor(Logger);
				case "EXREL":
					return new EXRELMessageProcessor(Logger);
				case "IMDR":
					return new IMDRMessageProcessor(Logger);
				case "ESMR":
					return new ESMRMessageProcessor(Logger);
				case "EMMR":
					return new EMMRMessageProcessor(Logger);
				case "DEPARTR":
					return new DEPARTRMessageProcessor(Logger);
				case "STREQR":
					return new STREQRMessageProcessor(Logger);
				case "DEPRECR":
					return new DEPRECRMessageProcessor(Logger);
				case "DEPRELR":
					return new DEPRELRMessageProcessor(Logger);
				case "WARRELR":
					return new WARRELRMessageProcessor(Logger);
				case "WARRETR":
					return new WARRETRMessageProcessor(Logger);
				case "CTORECR":
					return new CTORECRMessageProcessor(Logger);
				case "CTOREMR":
					return new CTOREMRMessageProcessor(Logger);
				case "IDL":
					return new IDLMessageProcessor(Logger);
				case "CARMOV":
					return new CARMOVMessageProcessor(Logger);
				case "AIRCRR":
					return new AIRCRRMessageProcessor(Logger);
				case "SEACRR":
					return new SEACRRMessageProcessor(Logger);
				case "UBMREQE":
					return new UBMREQEMessageProcessor(Logger);
				case "UBMREQR":
					return new UBMREQRMessageProcessor(Logger);
				case "AIRIARR":
					return new AIRIARRMessageProcessor(Logger);
				case "SEAIARR":
					return new SEAIARRMessageProcessor(Logger);
				case "AIRAARR":
					return new AIRAARRMessageProcessor(Logger);
				case "SEAAARR":
					return new SEAAARRMessageProcessor(Logger);
				case "AIROUTR":
					return new AIROUTRMessageProcessor(Logger);
				case "SEAOUTR":
					return new SEAOUTRMessageProcessor(Logger);
				case "PAYREC":
					return new PAYRECMessageProcessor(Logger);
				case "SACR":
					return new SACRMessageProcessor(Logger);
				case "CARST":
					return new CARSTMessageProcessor(Logger);
				case "PAYINV":
					return new PAYINVMessageProcessor(Logger);
				case "CARLSTR":
					return new CARLSTRMessageProcessor(Logger);
				case "SAM":
					return new SAMMessageProcessor(Logger);
				case "DSA":
					return new DSAMessageProcessor(Logger);
				case "ATD":
					return new ATDMessageProcessor(Logger);
				case "DOCS":
					return new DOCSMessageProcessor(Logger);
				case "REFACC":
					return new REFACCMessageProcessor(Logger);
				case "REFREJ":
					return new REFREJMessageProcessor(Logger);
				case "PAYEXC":
					return new PAYEXCMessageProcessor(Logger);
				case "DRWBCKR":
					return new DRWBCKRMessageProcessor(Logger);
				case "SEI":
					return new SEIMessageProcessor(Logger);
				case "SEQR":
					return new SEQRMessageProcessor(Logger);
				case "CLREGR":
					return new CLREGRMessageProcessor(Logger);
				case "CLNTDUP":
					return new CLNTDUPMessageProcessor(Logger);
				case "ERM":
					return new ERMMessageProcessor(Logger);
			}
			return null;
		}

		protected virtual internal CustomsMessageProcessor GetCONTROLProcessor()
		{
			return new CONTRLMessageProcessor(Logger);
		}

		#region TestCase

#if DEBUG

		public class CMRTestMessageProcessor : CMRBaseMessageProcessor
		{
			public CMRTestMessageProcessor(LoggingInformation logger)
				: base(logger)
			{
			}
		}

#endif

		#endregion
	}
}
