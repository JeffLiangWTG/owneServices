using System.Collections.Generic;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskItemProviderValidator : IComplianceRiskProviderValidator
	{
		public ComplianceRiskItemProviderValidator(IComplianceItemRiskStatusProvider complianceItemRiskStatusProvider, List<string> validationErrorList)
		{
			ValidationErrorList = validationErrorList;
			ComplianceItemRiskStatusProvider = complianceItemRiskStatusProvider;
		}

		List<string> ValidationErrorList { get; }

		string ComplianceProviderName => nameof(IComplianceItemRiskStatusProvider);

		IComplianceItemRiskStatusProvider ComplianceItemRiskStatusProvider { get; }

		public void ValidateAll()
		{
			ValidateFactory();
			ValidateParentId();
			ValidateParentTableCode();
		}

		void ValidateFactory()
		{
			if (ComplianceItemRiskStatusProvider.Factory is null)
			{
				ValidationErrorList.Add($"When implementing {ComplianceProviderName}, {nameof(ComplianceItemRiskStatusProvider.Factory)} should not be null");
			}
		}

		void ValidateParentId()
		{
			if (!ComplianceItemRiskStatusProvider.ParentID.IsValid)
			{
				ValidationErrorList.Add($"When implementing {ComplianceProviderName}, {nameof(ComplianceItemRiskStatusProvider.ParentID)} should not be empty or a invalid value");
			}
		}

		void ValidateParentTableCode()
		{
			if (ComplianceItemRiskStatusProvider.ParentTableCode.IsEmpty)
			{
				ValidationErrorList.Add($"When implementing {ComplianceProviderName}, {nameof(ComplianceItemRiskStatusProvider.ParentTableCode)} should not be empty");
			}
		}
	}
}
