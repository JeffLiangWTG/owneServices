using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ERMMessageProcessor : CMRMessageResponseProcessor
	{
		public ERMMessageProcessor(LoggingInformation logger)
			: base(logger, x => GetMessageType(x), "CCF Error/Reject Message Response (ERM)")
		{
		}

		static string GetMessageType(EDIMessage message)
		{
			var cUSRES = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet());
			return cUSRES != null ? cUSRES.GetRelatedDocumentType().ToString() : Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.ERM;
		}

		#region Implementation

		#region Email groups
		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.CargoStatusSendAcknowledgementsToGroup;
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.CargoStatusSendAcknowledgements;
			}
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.CargoStatusSendImpedimentsToGroup;
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.CargoStatusSendImpediments;
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.CargoStatusSendErrorsToGroup;
			}
		}

		protected override ZString ErrorEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.CargoStatusSendErrors;
			}
		}
		#endregion

		#endregion
	}
}
