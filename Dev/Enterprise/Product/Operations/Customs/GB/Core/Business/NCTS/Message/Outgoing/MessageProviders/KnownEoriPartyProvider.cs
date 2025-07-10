using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class KnownEoriPartyProvider : PartyProvider
	{
		public KnownEoriPartyProvider(JobDocAddress jobDocAddress, ZBool addKnownEoriLogic, bool isInPhase5TransitionPeriod) : base(jobDocAddress, isInPhase5TransitionPeriod)
		{
			addKnownEori = addKnownEoriLogic;
		}

		readonly ZBool addKnownEori;

		public override ZBool IncludeEORI
		{
			get
			{
				if (addKnownEori)
				{
					var id = EuEoriProviderAndValidator.GetEuIdentificationNumber(jobDocAddress);
					if (!string.IsNullOrEmpty(id))
					{
						if (id.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.OrdinalIgnoreCase)
							|| id.StartsWith(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		protected override ZBool SuppressForeignEORIActive => addKnownEori && GBCustomsDataRegistry.Instance.SuppressForeignEORI.Value;
	}
}
