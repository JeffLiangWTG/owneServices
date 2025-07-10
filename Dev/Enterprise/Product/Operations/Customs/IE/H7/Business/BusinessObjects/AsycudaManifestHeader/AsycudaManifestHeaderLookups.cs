using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaManifestHeaderLookups : EU.H7.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override CodeDescriptionPairList AgentTypeList => Factory.GetCachedValue("IE.H7.AsycudaManifestHeaderLookups.AgentTypeList", () =>
		{
			var result = new EU.Business.RepresentationTypeList();
			result.RemoveCode(EU.Business.RepresentationTypeList.Codes._1Self);
			return result;
		});

		public new CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<PaymentMethodList>();

		public CodeDescriptionPairList SubmitTypeList => Factory.GetCachedValue<SubmitTypeList>();

		public new CodeDescriptionPairList RegistrationStatusList => Factory.GetCachedValue<AISEntryStatusList>();
	}
}
