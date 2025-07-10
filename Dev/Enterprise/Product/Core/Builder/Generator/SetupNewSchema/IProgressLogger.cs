namespace Enterprise.Builder.Generator
{
	public interface IProgressLogger
	{
		void AddProgressText(string text);
		void ShowStatusLine(string text);
		void ReportSkippedFile(string text);
	}
}
