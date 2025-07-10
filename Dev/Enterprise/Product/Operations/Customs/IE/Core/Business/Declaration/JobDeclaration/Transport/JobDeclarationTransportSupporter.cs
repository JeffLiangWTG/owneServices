using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclarationTransportSupporter : EU.Business.Declaration.JobDeclarationTransportSupporter
	{
		public JobDeclarationTransportSupporter(JobDeclaration parent) : base(parent)
		{
		}

		JobDeclaration Declaration => (JobDeclaration)Parent;

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyDischargeChangedCore(transport, previousValue);
			if (Declaration is JobDeclaration declaration && declaration.IsUCC6)
			{
				var discPortCountryCode = transport.JW_RL_NKDiscPort.SubstringSafe(0, 2);
				if (!discPortCountryCode.IsEmpty
					&& discPortCountryCode != declaration.JE_RL_NKPortOfLoading.SubstringSafe(0, 2)
					&& discPortCountryCode != declaration.JE_RL_NKPortOfArrival.SubstringSafe(0, 2)
					&& !declaration.ItineraryCountries.ContainsCode(discPortCountryCode))
				{
					declaration.ItineraryCountries.AddNew(discPortCountryCode);
				}
			}
		}

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyLoadChangedCore(transport, previousValue);
			if (Declaration is JobDeclaration declaration && declaration.IsUCC6)
			{
				var loadPortCountryCode = transport.JW_RL_NKLoadPort.SubstringSafe(0, 2);
				if (!loadPortCountryCode.IsEmpty
					&& loadPortCountryCode != declaration.JE_RL_NKPortOfLoading.SubstringSafe(0, 2)
					&& loadPortCountryCode != declaration.JE_RL_NKPortOfArrival.SubstringSafe(0, 2)
					&& !declaration.ItineraryCountries.ContainsCode(loadPortCountryCode))
				{
					declaration.ItineraryCountries.AddNew(loadPortCountryCode);
				}
			}
		}
	}
}
