using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NCTSEndorsementProvider : INCTSEndorsement
	{
		readonly CusInBondEvent inBondEvent;

		public static NCTSEndorsementProvider NewOrNull(CusInBondEvent inBondEvent) => inBondEvent == null || inBondEvent.BN_EndorsementDate.IsEmpty || inBondEvent.BN_EndorsementPlace.IsEmpty
			|| inBondEvent.BN_EndorsementAuthority.IsEmpty || inBondEvent.BN_EndorsementCountryCode.IsEmpty
			? null : new NCTSEndorsementProvider(inBondEvent);

		NCTSEndorsementProvider(CusInBondEvent inBondEvent)
		{
			this.inBondEvent = Argument.NotNull(inBondEvent, nameof(inBondEvent));
		}

		public string Place => inBondEvent.BN_EndorsementPlace;

		public string Country => inBondEvent.BN_EndorsementCountryCode;

		public DateTime Date => inBondEvent.BN_EndorsementDate.Date.ToDateTime();

		public string Authority => inBondEvent.BN_EndorsementAuthority;
	}
}
