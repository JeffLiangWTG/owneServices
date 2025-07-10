using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GVMS
{
	public static class GVMSMessageDataObjectExtension
	{
		public static AsycudaManifestHeader GetMainfest(this GVMSMessageDataObject response, BusinessObjectFactory factory)
		{
			AsycudaManifestHeader header = null;
			var gmrID = response.gmrId;
			if (!string.IsNullOrEmpty(gmrID))
			{
				ZDBOnlySubQuery entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, gmrID);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms);
				query.AddSubQuery(entryNumberQuery, JoinCondition.And);

				header = factory.LoadTop1<AsycudaManifestHeader>(query);
			}
			return header;
		}
	}
}
