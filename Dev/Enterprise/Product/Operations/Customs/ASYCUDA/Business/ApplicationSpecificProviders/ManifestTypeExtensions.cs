namespace Enterprise.Customs.ASYCUDA.Business
{
	public static class ManifestTypeExtensions
	{
		public static bool IsManifestMessageLevel(this IManifestType manifestType) => manifestType.MessageLevel == MessageLevel.Manifest;
		public static bool IsBillMessageLevel(this IManifestType manifestType) => manifestType.MessageLevel == MessageLevel.Bill;
		public static bool IsPackMessageLevel(this IManifestType manifestType) => manifestType.MessageLevel == MessageLevel.Pack;
	}
}
