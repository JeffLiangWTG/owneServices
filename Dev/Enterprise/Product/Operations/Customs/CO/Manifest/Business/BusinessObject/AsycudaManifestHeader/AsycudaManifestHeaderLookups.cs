using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CargoDispositionList => Factory.GetCachedValue<CargoDispositionCodeList>();

		public CodeDescriptionPairList TravelDocumentTypes
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("COHeaderTravelDocumentTypes",
					delegate
					{
						var result = new TravelDocumentTypeCodeList();
						result.RemoveCode(TravelDocumentTypeCodeList.Codes._3);
						result.RemoveCode(TravelDocumentTypeCodeList.Codes._6);
						result.RemoveCode(TravelDocumentTypeCodeList.Codes._7);
						result.RemoveCode(TravelDocumentTypeCodeList.Codes._9);
						return result;
					}
				);
			}
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("MessageStatusCodeList",
					delegate
					{
						var result = new MessageStatusCodeList();
						result.RemoveCode(MessageStatusCodeList.Codes.Registered);
						result.RemoveCode(MessageStatusCodeList.Codes.NotSent);
						result.RemoveCode(MessageStatusCodeList.Codes.Updated);
						result.RemoveCode(MessageStatusCodeList.Codes.Unknown);
						result.RemoveCode(MessageStatusCodeList.Codes.Awaiting);
						result.RemoveCode(MessageStatusCodeList.Codes.Warning);
						return result;
					}
				);
			}
		}

		public CodeDescriptionPairList DeliveryModeList => Factory.GetCachedValue<CODeliveryModeList>();
	}
}
