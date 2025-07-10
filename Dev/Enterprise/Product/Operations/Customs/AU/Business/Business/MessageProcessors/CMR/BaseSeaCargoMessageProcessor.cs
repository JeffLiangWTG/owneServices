using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BaseSeaCargoMessageProcessor : CMRMessageResponseProcessor
	{
		public BaseSeaCargoMessageProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}
		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType.Contains(AcceptedString, StringComparison.OrdinalIgnoreCase); }
		}

		#region Email Groups

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.SeaCargoSendAcknowledgementsToGroup;
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.SeaCargoSendAcknowledgements;
			}
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.SeaCargoSendImpedimentsToGroup;
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.SeaCargoSendImpediments;
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.SeaCargoSendErrorsToGroup;
			}
		}

		protected override ZString ErrorEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.SeaCargoSendErrors;
			}
		}

		#endregion
	}
}
