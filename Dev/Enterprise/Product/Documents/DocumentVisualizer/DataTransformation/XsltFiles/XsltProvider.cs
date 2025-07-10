using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	public class XsltProvider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XSLT File Full Name")]
		public string GetXsltFromFile(string file)
		{
			var fileName = string.Format(CultureInfo.InvariantCulture, "Enterprise.DocumentVisualizer.DataTransformation.XsltFiles.{0}.xslt", file);
			using (var stream = GetType().Assembly.GetManifestResourceStream(fileName))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
