using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BaseAirCargoMessageProcessor : CMRMessageResponseProcessor
	{
		public BaseAirCargoMessageProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}

		#region E-mail Groups

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return Env.Registry.AUCustoms.AirCargoSendAcknowledgementsToGroup; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return Env.Registry.AUCustoms.AirCargoSendAcknowledgements; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return Env.Registry.AUCustoms.AirCargoSendImpedimentsToGroup; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return Env.Registry.AUCustoms.AirCargoSendImpediments; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return Env.Registry.AUCustoms.AirCargoSendErrorsToGroup; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return Env.Registry.AUCustoms.AirCargoSendErrors; }
		}

		#endregion
	}
}
