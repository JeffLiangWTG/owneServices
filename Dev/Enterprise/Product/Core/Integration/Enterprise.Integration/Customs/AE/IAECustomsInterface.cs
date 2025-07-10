namespace Enterprise.Integration;

public static partial class Customs
{
	public static class AEManifest
	{
		public interface IManifestController
		{
			object InterchangeSegmentProvider { get; }

			object MessageAttacheeProvider { get; }
		}
	}
}
