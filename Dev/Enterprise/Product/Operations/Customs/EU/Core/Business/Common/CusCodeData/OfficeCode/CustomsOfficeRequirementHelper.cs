using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public abstract class CustomsOfficeRequirementHelper
	{
		protected CustomsOfficeRequirementHelper(IEuOfficeCodeProvider officeCodeProvider)
		{
			OfficeCodeProvider = officeCodeProvider;
		}

		public string CacheKeyCombination => GetCacheKeyCombination();

		protected virtual string GetCacheKeyCombination() => "CustomsOfficeRequirementHelper";

		protected BusinessObjectFactory Factory => ((BusinessObject)OfficeCodeProvider).Factory;

		protected IEuOfficeCodeProvider OfficeCodeProvider { get; }

		public CustomsOfficeRequirement MainOffice => GetMainOffice();

		protected virtual CustomsOfficeRequirement GetMainOffice() => null;

		public IEnumerable<CustomsOfficeRequirement> OtherRequirements => GetOtherRequirements();

		protected virtual IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => Enumerable.Empty<CustomsOfficeRequirement>();

		public CustomsOfficeRequirement GetOtherRequirementByRole(string role) => OtherRequirements.FirstOrDefault(o => o.OfficeRole == role);

		public IEnumerable<string> Validate()
		{
			return ValidateCore();
		}

		protected virtual IEnumerable<string> ValidateCore()
		{
			var mandatoryRequirements = OtherRequirements.Where(x => x.IsMandatory);
			var customsOffices = OfficeCodeProvider.CustomsOffices;
			foreach (var requirement in mandatoryRequirements)
			{
				var office = customsOffices.FirstOrDefault(x => x.CY_Code == requirement.OfficeRole);
				if (office == null || office.CY_Data.IsEmpty)
				{
					yield return requirement.ValidationMessage.IsEmpty
						? Res.GetString("e611775b-7b6f-435a-b436-092317930b39", "The {0} requires an office of type {1} with purpose {2}.", Parent, requirement.FriendlyName, requirement.OfficeRole)
						: (string)requirement.ValidationMessage;
				}
			}
		}

		protected virtual string Parent => Res.GetString("4538664f-ec80-44b2-86dc-b388a63f8b76", "declaration");
	}
}
