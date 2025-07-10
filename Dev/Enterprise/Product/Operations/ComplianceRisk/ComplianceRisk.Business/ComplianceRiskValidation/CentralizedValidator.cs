using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ComplianceRisk.Business
{
	static class CentralizedValidator
	{
		public static void ValidateAll(IComplianceItemRiskStatusProvider complianceItemRiskStatusProvider)
		{
#if DEBUG
			var validationErrors = new List<string>();
			var specificValidators = new List<IComplianceRiskProviderValidator>();

			if (complianceItemRiskStatusProvider is ICompliancePartyRiskStatusProvider partyRiskStatusProvider)
			{
				specificValidators.Add(new CompliancePartyProviderValidator(partyRiskStatusProvider, validationErrors));
			}

			if (complianceItemRiskStatusProvider is IComplianceLocationRiskStatusProvider locationRiskStatusProvider)
			{
				specificValidators.Add(new ComplianceLocationProviderValidator(locationRiskStatusProvider, validationErrors));
			}

			if (complianceItemRiskStatusProvider is IComplianceCommodityRiskStatusProvider commodityRiskStatusProvider)
			{
				specificValidators.Add(new ComplianceCommodityProviderValidator(commodityRiskStatusProvider, validationErrors));
			}

			if (specificValidators.Count == 0)
			{
				validationErrors.Add((NoResString)"The interface IComplianceItemRiskStatusProvider should not be implemented directly.");
			}
			else
			{
				var defaultValidator = new ComplianceRiskItemProviderValidator(complianceItemRiskStatusProvider, validationErrors);
				defaultValidator.ValidateAll();

				specificValidators.ForEach(u => u.ValidateAll());
			}

			if (validationErrors.Count > 0)
			{
				ExceptionReporter.Instance.ReportDeveloperException((NoResString)"The interface IComplianceItemRiskStatusProvider is implemented in the wrong way", new ValidationException(string.Join(System.Environment.NewLine, validationErrors)));
			}
#endif
		}
	}
}
