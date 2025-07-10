using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Manifest.Business;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaManifestHeaderCustomsOfficeRequirementHelper : IcsCustomsOfficeRequirementHelper
	{
		public AsycudaManifestHeaderCustomsOfficeRequirementHelper(IEuOfficeCodeProvider officeCodeProvider) : base(officeCodeProvider)
		{
		}

		protected override string Parent => Res.GetString("267DF01C-EF0C-4DC6-AF90-BAFEAE78DDAF", "manifest");

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			yield return new CustomsOfficeRequirement(OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion, false, true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
			};
			yield return new CustomsOfficeRequirement(OfficeCodes_ICS.Codes.OfficeOfFirstEntry, false, true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
			};
			yield return new CustomsOfficeRequirement(OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry, false, true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
			};
		}
	}
}
