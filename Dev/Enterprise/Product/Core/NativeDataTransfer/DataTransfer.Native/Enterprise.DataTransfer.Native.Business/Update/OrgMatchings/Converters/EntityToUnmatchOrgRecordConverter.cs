using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	/// <summary>
	/// Convert Entity To UnmatchOrgRecord
	/// </summary>
	class EntityToUnmatchOrgRecordConverter : IConverter<IOrgHeaderForMatching, UnmatchOrgRecord>
	{
		public UnmatchOrgRecord Convert(IOrgHeaderForMatching org)
		{
			var unmatchOrg = new UnmatchOrgRecord
			{
				EDICode = org.OH_Code,
				OrganisationName = org.OH_FullName
			};

			foreach (var address in org.Addresses)
			{
				unmatchOrg.AddressLine1 = address.OA_Address1;
				unmatchOrg.AddressLine2 = address.OA_Address2;
				unmatchOrg.PostCode = address.OA_PostCode;
				unmatchOrg.City = address.OA_City;
				unmatchOrg.StateOrProvince = address.OA_State;
				break;
			}
			return unmatchOrg;
		}
	}
}
