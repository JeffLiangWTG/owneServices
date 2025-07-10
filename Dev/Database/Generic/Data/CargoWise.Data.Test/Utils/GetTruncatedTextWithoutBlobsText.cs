using CargoWise.Data.Testing;

namespace CargoWise.Data.Utils.Testing
{
	public class GetTruncatedTextWithoutBlobsText : BlobRemovalAndTruncateTest
	{
		protected override string GetTextWithoutBlobs(string text)
		{
			return CommandTextTrimmer.GetTruncatedTextWithoutBlobs(text);
		}

		protected override string GetTruncatedText(string text)
		{
			return CommandTextTrimmer.GetTruncatedTextWithoutBlobs(text);
		}
	}
}
