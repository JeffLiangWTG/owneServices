using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEACRRMessageProcessor : BaseSeaCargoMessageProcessor
	{
		public SEACRRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.SEACR, "Sea Cargo Report Response(SEACRR)")
		{
		}

		#region E-mail Modes

		protected override ZString AcknowledgementEmailMode
		{
			get { return IsHVLV ? Env.Registry.AUCustoms.HVLVSeaCargoSendAcknowledgements : Env.Registry.AUCustoms.SeaCargoSendAcknowledgements; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return IsHVLV ? Env.Registry.AUCustoms.HVLVSeaCargoSendImpediments : Env.Registry.AUCustoms.SeaCargoSendImpediments; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return IsHVLV ? Env.Registry.AUCustoms.HVLVSeaCargoSendErrors : Env.Registry.AUCustoms.SeaCargoSendErrors; }
		}

		CusSCAHouse LinkedCusSCAH => incomingMessage?.EM_LinkedObject as CusSCAHouse;

		bool IsHVLV => LinkedCusSCAH?.CA_IsHVLV ?? false;

		#endregion
	}
}
