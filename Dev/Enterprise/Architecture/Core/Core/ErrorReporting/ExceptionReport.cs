using System.IO;
using System.Xml;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class ExceptionReport
	{
		public ExceptionReport(string xml)
		{
			Xml = xml;
		}

		public readonly string Xml;

		string fTextFileName;
		public string TextFileName
		{
			get
			{
				if (fTextFileName == null)
				{
					fTextFileName = EnvProxy.Instance.GetTempFileName();
				}
				return fTextFileName;
			}
		}

		public string HtmlFileName
		{
			get { return EnvProxy.Instance.TempPath + "Exception.xml"; }
		}

		public void CreateTextFile()
		{
			using (StreamWriter stream = File.CreateText(TextFileName))
			{
				stream.Write(Xml);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Extensible Markup Language")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Feature // SuppressCodeSmell Reason = Extensible Markup Language")]
		public void CreateHtmlFile()
		{
			using (StreamWriter writer = File.CreateText(HtmlFileName))
			{
				XmlTextWriter xWriter = new XmlTextWriter(writer.BaseStream, System.Text.Encoding.UTF8);
				xWriter.WriteRaw(@"<?xml-stylesheet type='text/xsl' href='C:\Dev\exception.xslt'?>" + "\n");
				xWriter.WriteRaw(Xml);
				xWriter.Flush();
			}
		}
	}
}
