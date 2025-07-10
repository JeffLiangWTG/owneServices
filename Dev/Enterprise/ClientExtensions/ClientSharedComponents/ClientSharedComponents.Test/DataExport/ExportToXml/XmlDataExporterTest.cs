using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.DataExport.Testing
{
	public abstract class XmlDataExporterTest : TestCaseWithFactory
	{
		[TestDate(2006, 11, 10, 12, 0, 0)]
		public void TestExport()
		{
			BusinessObject bizOToExport = GetBizOToExport();
			try
			{
				Exporter.Export(bizOToExport, Notify);
				Assert("Notify should not have errors", !Notify.HasErrors);
				Assert("File should have been created", File.Exists(ExpectedFileName));
			}
			finally
			{
				DeleteIfExists(ExpectedFileName);
			}
		}

		protected XmlDataExporter Exporter
		{
			get { return GetNewXmlDataExporter(); }
		}

		protected NotificationBuffer Notify
		{
			get
			{
				if (fNotify == null)
				{
					fNotify = new NotificationBuffer();
				}
				return fNotify;
			}
		}
		NotificationBuffer fNotify;

		protected abstract XmlDataExporter GetNewXmlDataExporter();
		protected abstract BusinessObject GetBizOToExport();
		protected abstract ZString ExpectedFileName { get; }
	}
}
