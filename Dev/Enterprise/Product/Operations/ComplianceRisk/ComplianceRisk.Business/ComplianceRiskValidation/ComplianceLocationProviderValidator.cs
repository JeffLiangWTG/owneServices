using System.Collections.Generic;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ComplianceRisk.Business
{
	class ComplianceLocationProviderValidator : IComplianceRiskProviderValidator
	{
		public ComplianceLocationProviderValidator(IComplianceLocationRiskStatusProvider locationRiskStatusProvider, List<string> validationErrorList)
		{
			LocationRiskStatusProvider = locationRiskStatusProvider;
			ValidationErrorList = validationErrorList;
		}

		IComplianceLocationRiskStatusProvider LocationRiskStatusProvider { get; }

		string ComplianceProviderName => nameof(IComplianceLocationRiskStatusProvider);

		List<string> ValidationErrorList { get; }

		public void ValidateAll()
		{
			ValidateLocations();
		}

		void ValidateLocations()
		{
			var locations = LocationRiskStatusProvider.Locations;
			if (locations is null)
			{
				ValidationErrorList.Add($"When implementing {ComplianceProviderName}, {nameof(LocationRiskStatusProvider.Locations)} should not be null");
			}
			else
			{
				foreach (var location in locations)
				{
					if (location is null)
					{
						ValidationErrorList.Add($"When implementing {ComplianceProviderName}, location should not be null");
					}
					else if (location.Country is not RefCountry)
					{
						ValidationErrorList.Add($"When implementing {ComplianceProviderName}, location.Country should not be null");
					}
					else
					{
						if (location.Code.IsEmpty)
						{
							ValidationErrorList.Add($"When implementing {ComplianceProviderName}, location.Code should not be empty");
						}

						if (location.LocationDescription.IsEmpty)
						{
							ValidationErrorList.Add($"When implementing {ComplianceProviderName}, location.LocationDescription should not be empty");
						}
					}
				}
			}
		}
	}
}
