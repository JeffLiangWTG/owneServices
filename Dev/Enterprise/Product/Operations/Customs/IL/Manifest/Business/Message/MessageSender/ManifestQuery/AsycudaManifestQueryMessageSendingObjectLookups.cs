using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestQueryMessageSendingObjectLookups : ZLookups
	{
		public AsycudaManifestQueryMessageSendingObjectLookups(BusinessObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList MessageSubTypes => Factory.GetCachedValue("IL.AsycudaManifestQueryMessageSendingObjectLookups.MessageSubTypes", delegate
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ILEDIMessageSubTypeList.Codes.ManifestQueryRequest, ILEDIMessageSubTypeList.Descriptions.ManifestQueryRequest);
			return result;
		});
	}
}
