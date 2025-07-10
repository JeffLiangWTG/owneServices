using System.IO;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Types that own disposable fields should be disposable
	/// </summary>
	internal class CA1001
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used for testing CA1001")]
		readonly FileStream fileStream;

		public CA1001(string filePath)
		{
			// CA1001: Types that own disposable fields should be disposable
			fileStream = new FileStream(filePath, FileMode.Open);
		}
	}
}
