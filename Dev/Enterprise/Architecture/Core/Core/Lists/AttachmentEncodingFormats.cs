namespace Enterprise.ZArchitecture.Core.Lists
{
	public class AttachmentEncodingFormats : CodeDescriptionPairList
	{
		public const string RFC2047 = "RFC2047";
		public const string RFC2231 = "RFC2231";

		public AttachmentEncodingFormats()
		{
			AddPair(RFC2047, SourceGenerated.ResString.GetMultilingualString("7AF89A95-58AD-4932-A9FC-AB99FC46F884", "RFC2047"));
			AddPair(RFC2231, SourceGenerated.ResString.GetMultilingualString("7DCCF12C-9625-4FA5-A139-FCC3A4FB9B25", "RFC2231"));

			DefaultCode = RFC2047;
		}
	}
}
