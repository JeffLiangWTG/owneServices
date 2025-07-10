namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public sealed class F17HeaderProvider : SendAndAmendHeader17Provider
{
	public F17HeaderProvider(AsycudaManifestHeader header)
		: base(header)
	{
	}

	protected override bool UsesPlaceHolder => true;
}
