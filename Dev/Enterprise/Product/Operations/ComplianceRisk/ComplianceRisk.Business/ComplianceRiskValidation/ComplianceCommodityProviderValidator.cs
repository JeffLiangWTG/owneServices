using System.Collections.Generic;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityProviderValidator : IComplianceRiskProviderValidator
	{
		public ComplianceCommodityProviderValidator(IComplianceCommodityRiskStatusProvider commodityRiskStatusProvider, List<string> validationErrorList)
		{
			CommodityRiskStatusProvider = commodityRiskStatusProvider;
			ValidationErrorList = validationErrorList;
		}

		IComplianceCommodityRiskStatusProvider CommodityRiskStatusProvider { get; }

		string ComplianceProviderName => nameof(IComplianceCommodityRiskStatusProvider);

		List<string> ValidationErrorList { get; }

		public void ValidateAll()
		{
			ValidateEffectiveDate();
			ValidateAssessmentPointPairInfo();
			ValidateCommodities();
		}

		void ValidateEffectiveDate()
		{
			if (!CommodityRiskStatusProvider.EffectiveDate.IsValid)
			{
				ValidationErrorList.Add($"When implementing {ComplianceProviderName}, {nameof(CommodityRiskStatusProvider.EffectiveDate)} should be valid");
			}
		}

		void ValidateAssessmentPointPairInfo()
		{
			if (CommodityRiskStatusProvider.AssessmentPointPairInfo is null)
			{
				ValidationErrorList.Add($"When implementing {ComplianceProviderName}, {nameof(CommodityRiskStatusProvider.AssessmentPointPairInfo)} should not be null");
			}
		}

		void ValidateCommodities()
		{
			if (CommodityRiskStatusProvider.Commodities is null)
			{
				ValidationErrorList.Add($"When implementing {ComplianceProviderName}, {nameof(CommodityRiskStatusProvider.Commodities)} should not be null");
			}
		}
	}
}
