using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderCustomsOfficeRequirementHelper : CustomsOfficeRequirementHelper
	{
		public TemporaryStorageHeaderCustomsOfficeRequirementHelper(IEuOfficeCodeProvider officeCodeProvider) : base(officeCodeProvider)
		{
		}

		protected override string GetCacheKeyCombination() => $"{CacheKeyConst}.{OfficeCodeProvider.CountryCode}";

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			return Factory.GetCachedValue($"{GetCacheKeyCombination()}.{nameof(base.OtherRequirements)}", CreateOtherRequirements);
		}

		protected virtual IEnumerable<CustomsOfficeRequirement> CreateOtherRequirements()
		{
			yield return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry, EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage }
			};
		}

		const string CacheKeyConst = "TemporaryStorageHeaderCustomsOfficeRequirementHelper";
	}
}
