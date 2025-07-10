using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UserDefinedEntryStatus, ZDateTime.Today);
		}
	}
}
