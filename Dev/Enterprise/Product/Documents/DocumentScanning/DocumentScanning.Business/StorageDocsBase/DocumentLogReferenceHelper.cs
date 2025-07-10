using System.Globalization;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	public static class DocumentLogReferenceHelper
	{
		public static string GetReference(StorageDocsBase storageDocs, Event logEvent)
		{
			return string.Format(CultureInfo.InvariantCulture,
				(NoResString)"eDoc '{0}-{1}-{2}-{3}' {4}|{5}",
				GlbCompany.CurrentCompany?.CompanyName,
				GlbBranch.CurrentBranch?.GB_BranchName,
				storageDocs.SC_DocType,
				storageDocs.SC_FileNameWithExtension,
				logEvent.Description,
				storageDocs.PK);
		}
	}
}
