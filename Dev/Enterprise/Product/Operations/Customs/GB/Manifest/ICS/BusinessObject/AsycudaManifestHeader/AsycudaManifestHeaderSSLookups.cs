using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaManifestHeaderSSLookups : AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderSSLookups(AsycudaManifestHeaderBase parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList TransportModeList => Factory.GetCachedValue<CodeDescriptionPairList>("GBSSTransportTypeList", () => new GBSSTransportTypeList());
	}
}
