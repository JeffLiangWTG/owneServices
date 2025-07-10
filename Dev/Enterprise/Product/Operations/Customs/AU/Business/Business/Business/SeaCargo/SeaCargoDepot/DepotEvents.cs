
namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class DepotEvents
	{
		public const string ImpendingCargo = "IMPENDING ARRIVAL";
		public const string ImpendingCargoCancelled = "CANCEL IMPENDING ARRIVAL";
		public const string CargoArrived = "CARGO ARRIVED";
		public const string CargoArrivedError = "ERROR CARGO ARRIVED";
		public const string CargoArrivedAcknowledged = "ACK CARGO ARRIVED";
		public const string CargoUnpacked = "CARGO UNPACKED";
		public const string CargoUnpackedError = "ERROR CARGO UNPACKED";
		public const string CargoUnpackedAcknowledged = "ACK CARGO UNPACKED";
		public const string CargoDelivered = "CARGO DELIVERED";
		public const string CargoDeliveredError = "ERROR CARGO DELIVERED";
		public const string CargoDeliveredAcknowledged = "ACK CARGO DELIVERED";
		public const string CargoStatusAdviceClear = "CLEAR";
		public const string CargoStatusAdviceConditionalClear = "CONDCLEAR";
		public const string CargoStatusAdviceDetailed = "DETAINED";
		public const string CargoStatusAdviceACSSEIZED = "ACSSEIZED";
		public const string CargoStatusAdviceAQISSEIZED = "AQISSEIZED";
		public const string CargoStatusAdviceClearHRM = "CLEARHRM";
		public const string CargoStatusAdviceHELD = "HELD";
		public const string CargoStatusAdviceSUBUBMOV = "SUBUBMOV";
		public const string CargoStatusAdviceTRANSHIP = "TRANSHIP";
		public const string CargoStatusAdviceTRANSHPHRM = "TRANSHPHRM";
		public const string CargoStatusAdviceTRANSIT = "TRANSIT";
		public const string CargoStatusAdviceManual = "MAN";
	}
}
