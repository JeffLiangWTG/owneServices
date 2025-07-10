using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestMessageSendingObjectLookups : ZLookups
	{
		public AsycudaManifestMessageSendingObjectLookups(BusinessObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList MessageSubTypes => Factory.GetCachedValue("IL.AsycudaManifestMessageSendingObjectLookups.MessageSubTypes", delegate
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest, ILEDIMessageSubTypeList.Descriptions.ForwarderManifestRequest);
			return result;
		});
	}
}
