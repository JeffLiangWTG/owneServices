using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CARSTMessageProcessor : CMRMessageResponseProcessor
	{
		public CARSTMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.CARST, "Cargo Status Advice - (CARST)")
		{
		}

		protected override bool IsUnsolicitedMessage
		{
			get { return true; }
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return true; }
		}

		#region Implmentation
		#region Email Groups

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

		protected override GlbStaff GetUserToNotify(IBusiness parent)
		{
			var pivot = parent as CusSCAPivot;
			if (pivot != null && pivot.HouseBill != null)
			{
				return base.GetUserToNotify(pivot.HouseBill);
			}

			return base.GetUserToNotify(parent);
		}

		#endregion
		#endregion
	}
}
