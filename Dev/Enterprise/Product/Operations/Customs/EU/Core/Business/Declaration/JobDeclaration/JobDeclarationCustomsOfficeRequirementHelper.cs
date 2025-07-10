using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class JobDeclarationCustomsOfficeRequirementHelper : CustomsOfficeRequirementHelper
	{
		public JobDeclarationCustomsOfficeRequirementHelper(IEuOfficeCodeProvider declaration) : base(declaration)
		{
		}
		protected JobDeclaration Declaration => (JobDeclaration)OfficeCodeProvider;

		protected override string GetCacheKeyCombination()
		{
			return string.Join(",", "JobDeclarationCustomsOfficeRequirementHelper", Declaration.CountryCode, Declaration.JE_MessageType);
		}

		protected override CustomsOfficeRequirement GetMainOffice()
		{
			return Factory.GetCachedValue("EU.JobDeclarationCustomsOfficeRequirementHelper.MainOffice", () => new CustomsOfficeRequirement(ZString.Empty, true, false));
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			return Factory.GetCachedValue("EU.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements" + Declaration.JE_MessageType, () =>
			{
				var result = new List<CustomsOfficeRequirement>();
				if (Declaration.IsExport)
				{
					result.Add(new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, true, false));
				}
				return result;
			});
		}

		protected override IEnumerable<string> ValidateCore()
		{
			var errors = new List<string>();
			if (MainOffice != null && MainOffice.IsMandatory && Declaration.JE_CustomsOffice.IsEmpty)
			{
				errors.Add(Res.GetString("2ba81e45-ff11-46b3-981c-4ad6d094a304", "You have not entered an office of type {0}.", MainOffice.FriendlyName));
			}
			errors.AddRange(base.ValidateCore());
			return errors;
		}

		public ZString GetOfficeCode(string role)
		{
			if (MainOffice != null && MainOffice.OfficeRole == role && !Declaration.JE_CustomsOffice.IsEmpty)
			{
				return Declaration.JE_CustomsOffice;
			}
			else if (OtherRequirements.Any(x => x.OfficeRole == role))
			{
				return Declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_RoleCodes.Contains(role))?.CY_Data ?? ZString.Empty;
			}

			return ZString.Empty;
		}
	}
}
