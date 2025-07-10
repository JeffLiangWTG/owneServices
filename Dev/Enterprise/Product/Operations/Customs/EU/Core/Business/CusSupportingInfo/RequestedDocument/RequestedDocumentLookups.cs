using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class RequestedDocumentLookups : CusSupportingInfoLookups
	{
		public RequestedDocumentLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<RequestedDocumentStatusList>();
	}
}
