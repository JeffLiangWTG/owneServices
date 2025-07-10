using System;
using System.IO;
using System.Text;

namespace Enterprise.ExcelComparator.Testing
{
	sealed class MockComparisonTool : IComparisonTool
	{
		public MockComparisonTool()
		{
			isInstalled = true;
		}

		bool isInstalled;

		public void SetInstalled(bool value)
		{
			isInstalled = value;
		}

		public string LastComparisonFilePath1
		{
			get;
			private set;
		}

		public string LastComparisonFilePath2
		{
			get;
			private set;
		}

		public string Differences
		{
			get;
			private set;
		}

		#region IComparisonTool Members

		public string Path
		{
			get { return @"C:\Program Files\Mock Comparison Tool\Compare.exe"; }
		}

		public bool IsInstalled()
		{
			return isInstalled;
		}

		public void RunComparison(string filePath1, string filePath2)
		{
			LastComparisonFilePath1 = filePath1;
			LastComparisonFilePath2 = filePath2;

			if (File.Exists(filePath1) && File.Exists(filePath2))
			{
				var differences = new StringBuilder();
				var fileContents1 = File.ReadAllText(LastComparisonFilePath1).Split(new string[] { "\r\n" }, StringSplitOptions.None);
				var fileContents2 = File.ReadAllText(LastComparisonFilePath2).Split(new string[] { "\r\n" }, StringSplitOptions.None);

				var mineLines = Math.Min(fileContents1.Length, fileContents2.Length);
				for (int line = 0; line < mineLines; line++)
				{
					if (!fileContents1[line].Equals(fileContents2[line]))
					{
						differences.AppendLine(String.Format("line:[{0}]   file1:[{1}]   file2:[{2}]", line, fileContents1[line], fileContents2[line]));
					}
				}

				var maxLines = Math.Max(fileContents1.Length, fileContents2.Length);
				for (int line = mineLines; line < maxLines; line++)
				{
					differences.AppendLine(String.Format("line:[{0}]   file1:[{1}]   file2:[{2}]"
						, line
						, (line < fileContents1.Length ? fileContents1[line] : "")
						, (line < fileContents2.Length ? fileContents2[line] : "")));
				}

				Differences = differences.ToString().Trim();
			}
		}

		#endregion
	}
}
