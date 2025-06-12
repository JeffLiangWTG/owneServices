namespace eServices.eHubPortal.Constants;

public static class OwnerId
{
	public const string eServices = "ESV";
	public const string Customs = "CUS";
	public const string Air = "AIR";
	public const string Ocean = "OCM";

	public static string LookupOwnerName(string id)
	{
		return id switch
		{
			OwnerId.eServices => nameof(OwnerId.eServices),
			OwnerId.Customs => nameof(OwnerId.Customs),
			OwnerId.Air => nameof(OwnerId.Air),
			OwnerId.Ocean => nameof(OwnerId.Ocean),
			_ => id
		};
	}
}
