namespace WTG.TestHelpers.SpecTesting
{
	public class SpecFile
	{
		public string FileName { get; set; }
		public string Contents { get; set; }

		public SpecFile(string fileName, string contents)
		{
			FileName = fileName;
			Contents = contents;
		}

		public string NormalizedContents()
		{
			return SpecText.NormalizeSpecText(Contents);
		}
	}
}
