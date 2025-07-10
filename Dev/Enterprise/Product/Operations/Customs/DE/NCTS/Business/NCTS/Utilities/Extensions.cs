using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public static class Extensions
	{
		public static JobDocAddress GetDocAddressOrNullIfInvalid(this JobDocAddress docAddress) => docAddress?.IsValidAddress ?? false ? docAddress : null;
	}
}
