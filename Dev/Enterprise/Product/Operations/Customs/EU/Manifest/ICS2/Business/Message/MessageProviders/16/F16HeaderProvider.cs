namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public sealed class F16HeaderProvider : SendAndAmendHeader16Provider
{
	public F16HeaderProvider(AsycudaManifestHeader header)
		: base(header)
	{
	}

	protected override bool UsesPlaceHolder => true;
}
