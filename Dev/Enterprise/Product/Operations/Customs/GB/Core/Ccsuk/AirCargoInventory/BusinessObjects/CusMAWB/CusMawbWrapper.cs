using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[CodeProperty("MawbCode")]
	[DescriptionProperty("Description")]
	public class CusMawbWrapper : NonPersistentBusinessObject
	{
		public CusMawbWrapper(CusMAWB cusMawb)
		{
			MAWB = cusMawb;
		}

		public CusMAWB MAWB { get; set; }

		public ZString MawbCode
		{
			get
			{
				ZString friendly = MAWB?.Lookups?.ShedsList?.GetDescriptionFromCode(MAWB.CargoTerminalOperatorAirportAndShed);
				return string.IsNullOrEmpty(friendly) ? (MAWB?.ReferenceNumberWithShed ?? ZString.Empty) : friendly;
			}
		}

		public ZString Description => MAWB == null ? string.Empty : FormattableString.Invariant($"{MAWB.CargoTerminalOperatorAirportAndShed}-{MAWB.CM_MAWB} agent '{MAWB.AgentBadge}', customs status '{MAWB.CustomsActionCode}', presence '{MAWB.PresenceOnNetworkStatus}', created {MAWB.LocalCreationDate.ToShortDateString()} {MAWB.LocalCreationDate.ToShortTimeString()}");
	}
}
