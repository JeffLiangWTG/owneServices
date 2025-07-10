using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class RegistrationNumberCollectionExtensions
	{
		public static ZString? GetValue(this List<RegistrationNumber> registrations, ZString registrationNumberTypeCode, ZString countryOfIssueCode)
		{
			ZString? result = null;
			if (registrations != null)
			{
				var registration = registrations.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == registrationNumberTypeCode && x.CountryOfIssue.GetCodeAsUpperCase() == countryOfIssueCode);
				if (registration != null)
				{
					result = registration.Value;
				}
			}
			return result;
		}
	}
}