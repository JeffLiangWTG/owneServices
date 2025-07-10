using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Forwarding.Business
{
	public abstract class ForwardingConsolCustomsStatusProvider
	{
		protected ForwardingConsolCustomsStatusProvider(ForwardingConsol consol)
		{
			Consol = Argument.NotNull(consol, "consol");
		}

		protected readonly ForwardingConsol Consol;

		static ZBool CurrentCompanyIsUK => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedKingdom;

		public static ForwardingConsolCustomsStatusProvider New(ForwardingConsol consol)
		{
			return consol.Factory.GetCachedValue(
				"ForwardingConsolCustomsStatusProvider:" + consol.PK,
				() =>
				{
					Type statusProviderType;
					if (CurrentCompanyIsUK && consol.IsAir && consol.JK_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.Ordinal))
					{
						statusProviderType = ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICcsukForwardingConsolCustomsStatusProvider>();
					}
					else
					{
						statusProviderType = typeof(EmptyForwardingConsolCustomsStatusProvider);
					}

					return (ForwardingConsolCustomsStatusProvider)Activator.CreateInstance(statusProviderType, consol);
				});
		}

		public abstract ZString CustomsCargoStatus();

		class EmptyForwardingConsolCustomsStatusProvider : ForwardingConsolCustomsStatusProvider
		{
			public EmptyForwardingConsolCustomsStatusProvider(ForwardingConsol consol)
				: base(consol)
			{
			}

			public override ZString CustomsCargoStatus()
			{
				return string.Empty;
			}
		}
	}
}
