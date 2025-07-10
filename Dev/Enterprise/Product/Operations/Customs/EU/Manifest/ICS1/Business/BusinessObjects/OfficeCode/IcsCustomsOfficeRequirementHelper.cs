using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class IcsCustomsOfficeRequirementHelper : CustomsOfficeRequirementHelper
	{
		public IcsCustomsOfficeRequirementHelper(IEuOfficeCodeProvider officeCodeProvider) : base(officeCodeProvider)
		{
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			return Factory.GetCachedValue("EU.ICSCustomsOfficeRequirementHelper.OtherRequirements", () =>
			{
				return new List<CustomsOfficeRequirement>
				{
					new CustomsOfficeRequirement(OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion, false, false)
					{
						OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
					},
					new CustomsOfficeRequirement(OfficeCodes_ICS.Codes.OfficeOfFirstEntry, true, false)
					{
						OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
					},
					new CustomsOfficeRequirement(OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry, false, false)
					{
						OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
					}
				};
			});
		}

		protected override string Parent => Res.GetString("e63c6120-2ad2-4197-8994-18df1f256a5b", "manifest");
	}
}
