using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.IDEA
{
	public class IDEAeDocs
	{
		public IDEAeDocs(AccComplianceReport complianceReport)
		{
			DocManager = complianceReport.DocManagerInfo();
			DocManager.SetupEDocsFactoryToBeSavedWithMainFactory(true);
		}

		public const int MaxEdocSize = 1500 * 1024 * 1024;
		readonly DocManagerInfo DocManager;

		public int GetMaxArchiveNumber()
		{
			int maxArchiveNumber = 0;
			for (var i = 0; i < DocManager.AllEDocs.Count; i++)
			{
				var filename = DocManager.AllEDocs[i].FileName;
				if (filename.StartsWith("IDEA-Export-") && filename.EndsWith(".zip"))
				{
					var delimiterPosition = filename.IndexOf("-part");
					if (delimiterPosition >= 0 && int.TryParse(filename.Substring(delimiterPosition + 5, 3), out var archiveNumber) && archiveNumber > maxArchiveNumber)
					{
						maxArchiveNumber = archiveNumber;
					}
				}
			}

			return maxArchiveNumber;
		}
	}
}
