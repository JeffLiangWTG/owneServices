using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business;

public class ManifestStatus : Customs.Business.ManifestStatus
{
	protected ManifestStatus(string status)
		: base((NoResString)status)
	{
	}

	public static readonly ManifestStatus Idle = new ManifestStatus("Idle");
}
