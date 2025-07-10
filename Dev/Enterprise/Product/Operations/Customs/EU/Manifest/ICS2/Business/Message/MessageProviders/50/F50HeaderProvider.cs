namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public sealed class F50HeaderProvider : SendAndAmendHeader50Provider
{
	public F50HeaderProvider(AsycudaManifestHeader header)
		: base(header)
	{
	}

	protected override bool UsesPlaceHolder => true;
}
