using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ComplianceRisk.Business
{
	class CompliancePartyProviderValidator : IComplianceRiskProviderValidator
	{
		readonly ICompliancePartyRiskStatusProvider partyRiskStatusProvider;
		readonly List<string> validationErrors;

		public CompliancePartyProviderValidator(ICompliancePartyRiskStatusProvider partyRiskStatusProvider, List<string> validationErrors)
		{
			this.partyRiskStatusProvider = partyRiskStatusProvider;
			this.validationErrors = validationErrors;
		}

		public void ValidateAll()
		{
			ValidateParties();
		}

		void ValidateParties()
		{
			if (partyRiskStatusProvider.Parties is null)
			{
				validationErrors.Add($"When implementing {nameof(ICompliancePartyRiskStatusProvider)}, {nameof(partyRiskStatusProvider.Parties)} should not be null");
			}
			else
			{
				if (partyRiskStatusProvider.Parties.Any(o => o?.Party != null && !(o.Party is OrgHeader or IScreeningPartyForVessel or JobDocAddress or RefVessel)))
				{
					var stringBuilder = new StringBuilder();
					stringBuilder.AppendLine($"When implementing {nameof(ICompliancePartyRiskStatusProvider)}, every element of {nameof(partyRiskStatusProvider.Parties)} should be instance of following supported types:");
					new[] { typeof(OrgHeader), typeof(IScreeningPartyForVessel), typeof(JobDocAddress), typeof(RefVessel) }.ForEach(o => stringBuilder.AppendLine($"{o}"));
					validationErrors.Add(stringBuilder.ToString());
				}
			}
		}
	}
}
