using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitDetailLookups : EU.Business.CusExitDetailLookups
	{
		public CusExitDetailLookups(CusExitDetail parent) : base(parent)
		{
		}

		public OrgHeaderCollection OrgHeaderCollection => new OrgHeaderCollection(Factory);

		public override CodeDescriptionPairList StatusList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDateTime.Today);
	}
}
