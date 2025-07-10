using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIRCRRMessageProcessor : BaseAirCargoMessageProcessor
	{
		public AIRCRRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.AIRCR, "Air Cargo Report Response(AIRCRR)")
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			if (LinkedCusHAWB != null)
			{
				LinkedCusHAWB.CS_IsResponsePending = false;
				if (!statusType.Contains(RejectedString))
				{
					LinkedCusHAWB.CS_IsPrealerted = true;
				}
			}
			return true;
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType.Contains(AcceptedString); }
		}

		#region E-mail Modes

		protected override ZString AcknowledgementEmailMode
		{
			get { return IsHVLV ? Env.Registry.AUCustoms.HVLVAirCargoSendAcknowledgements : Env.Registry.AUCustoms.AirCargoSendAcknowledgements; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return IsHVLV ? Env.Registry.AUCustoms.HVLVAirCargoSendImpediments : Env.Registry.AUCustoms.AirCargoSendImpediments; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return IsHVLV ? Env.Registry.AUCustoms.HVLVAirCargoSendErrors : Env.Registry.AUCustoms.AirCargoSendErrors; }
		}

		CusHAWBBase LinkedCusHAWB => incomingMessage?.EM_LinkedObject as CusHAWBBase;

		bool IsHVLV => LinkedCusHAWB?.CS_IsHVLV ?? false;

		#endregion
	}
}
