namespace Enterprise.Customs.AE.Manifest.Business;

class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
{
	public AsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
	{
	}

	protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;
	
	protected override void CheckAMA_OA_Carrier()
	{
		base.CheckAMA_OA_Carrier();
		ValidationHelper.CheckOrgContactInfo(Parent.Carrier, Parent.AMA_OA_CarrierInfo);
		ValidationHelper.CheckAnyMPCICode(Parent.Carrier, Parent.AMA_OA_CarrierInfo);
	}

	protected override bool IsVoyageMandatory => false;
}
