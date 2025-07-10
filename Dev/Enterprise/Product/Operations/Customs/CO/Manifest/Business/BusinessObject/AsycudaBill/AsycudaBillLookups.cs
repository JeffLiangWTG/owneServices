using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CargoDispositionList => Factory.GetCachedValue<CodeDescriptionPairList>("COBillCargoDispositionList", () => new CargoDispositionCodeList());

		public CodeDescriptionPairList TravelDocumentTypes
		{
			get
			{
				return Factory.GetCachedValue("COBillsTravelDocumentTypes",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(TravelDocumentTypeCodeList.Codes._0, TravelDocumentTypeCodeList.Descriptions._0);
						result.AddPair(TravelDocumentTypeCodeList.Codes._3, TravelDocumentTypeCodeList.Descriptions._3);
						result.AddPair(TravelDocumentTypeCodeList.Codes._6, TravelDocumentTypeCodeList.Descriptions._6);
						result.AddPair(TravelDocumentTypeCodeList.Codes._7, TravelDocumentTypeCodeList.Descriptions._7);
						result.AddPair(TravelDocumentTypeCodeList.Codes._9, TravelDocumentTypeCodeList.Descriptions._9);
						return result;
					}
				);
			}
		}

		public override OrgHeaderCollection GoodsLocationsOrgHeaderCollection => new WarehouseClientCollection(Factory);

		public CodeDescriptionPairList ContainerModeList => Factory.GetCachedValue<CodeDescriptionPairList>("COContainerModeList", () => new ContainerModeCodeList());
	}
}
