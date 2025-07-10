using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BaseUnderbondMessageProcessor : CMRMessageResponseProcessor
	{
		public BaseUnderbondMessageProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}

		#region Email Groups

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.UnderbondSendAcknowledgementsToGroup;
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.UnderbondSendAcknowledgements;
			}
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.UnderbondSendImpedimentsToGroup;
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.UnderbondSendImpediments;
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.UnderbondSendErrorsToGroup;
			}
		}

		protected override ZString ErrorEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.UnderbondSendErrors;
			}
		}

		#endregion
	}
}
