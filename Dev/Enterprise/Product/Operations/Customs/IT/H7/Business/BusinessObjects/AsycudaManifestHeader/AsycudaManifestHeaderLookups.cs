using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.H7.Business;
public class AsycudaManifestHeaderLookups : EU.H7.Business.AsycudaManifestHeaderLookups
{
	public AsycudaManifestHeaderLookups(EU.H7.Business.AsycudaManifestHeader parent) : base(parent)
	{
	}

	public CodeDescriptionPairList SubmitTypeList => new CodeDescriptionPairList();

	public new CodeDescriptionPairList MethodOfPaymentList => new CodeDescriptionPairList();
}
