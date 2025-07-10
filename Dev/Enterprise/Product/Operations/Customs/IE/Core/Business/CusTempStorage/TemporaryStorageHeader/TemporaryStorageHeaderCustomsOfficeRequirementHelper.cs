using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageHeaderCustomsOfficeRequirementHelper : EU.Business.CusTempStorage.TemporaryStorageHeaderCustomsOfficeRequirementHelper
	{
		public TemporaryStorageHeaderCustomsOfficeRequirementHelper(IEuOfficeCodeProvider officeCodeProvider) : base(officeCodeProvider)
		{
		}

		protected override IEnumerable<CustomsOfficeRequirement> CreateOtherRequirements()
		{
			yield return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry, EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage }
			};
			yield return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfLodgement, isMandatory: false, isLocalCountryOnly: true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry, EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage }
			};
		}
	}
}
