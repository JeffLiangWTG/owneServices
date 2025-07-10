using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Module
{
	public class SumAFilterStripBusinessObjectLookups
	{
		public SumAFilterStripBusinessObjectLookups(SumAFilterStripBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}

		public readonly SumAFilterStripBusinessObject filterBizObj;

		BusinessObjectFactory Factory => filterBizObj.Factory;

		public OrganisationsFindBoxCollection OrganisationList => new OrganisationsFindBoxCollection(Factory);

		public GlbBranchCollection BranchList => new GlbBranchCollection(Factory);

		public RefUNLOCOCollection LoadingList => new RefUNLOCOCollection(Factory);

		public CodeDescriptionPairList CustomsOfficeList
		{
			get
			{
				return Factory.GetCachedValue("E4C086D3-7B39-4EC9-9A33-92BA82C44023", () =>
				{
					var list = RefCusCodeListTypes.GetCachedList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					list.SortByDescription();
					return list;
				});
			}
		}

		public CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportTypeGenericList>();

		public CodeDescriptionPairList ApplicationCodeList => Factory.GetCachedValue<TemporaryStorageApplicationCodeList>();

		public CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<TemporaryStorageDeclarationTypeList>();

		public CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<EDIMessageStatusList>();
	}
}
