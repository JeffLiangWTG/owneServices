
namespace Enterprise.Customs.AE.Business;

public static class ManifestConstants
{
	public static class CargoCode
	{
		public static string GetManifestCode(string containerMode)
		{
			switch (containerMode)
			{
				case Core.Constants.ContainerModes.FCL:
					return "F";
				case Core.Constants.ContainerModes.LCL:
					return "L";
				case Core.Constants.ContainerModes.Empty:
					return "M";
				case Core.Constants.ContainerModes.Bulk:
					return "B";
				case Core.Constants.ContainerModes.Liquid:
					return "Q";
				case Core.Constants.ContainerModes.RollOnRollOff:
					return "R";
				case Core.Constants.ContainerModes.BreakBulk:
					return "G";
				default:
					return "G";
			}
		}
	}

	public const string MessageType = "MFI";
	public const string ManifestFileExtention = "MFT";
}
