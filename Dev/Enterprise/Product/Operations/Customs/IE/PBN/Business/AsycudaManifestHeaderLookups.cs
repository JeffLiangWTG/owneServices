using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}
		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override CodeDescriptionPairList TransportModeList => Factory.GetCachedValue("IE_PBN_TransportModeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);
			result.AddPair(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road);
			return result;
		});

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<LogicalStatusList>();

		public override CodeDescriptionPairList RegistrationStatusList => Factory.GetCachedValue<PBNCustomsStatusList>();

		public override CodeDescriptionPairList Natures => Factory.GetCachedValue("IE.PBN.AsycudaManifestNatures", () => {
			var result = new ShipmentTypeList();
			result.RemoveCode(ShipmentTypeList.Codes.Transhipment28);
			result.RemoveCode(ShipmentTypeList.Codes.Transit24);
			return result;
		});
	}
}
