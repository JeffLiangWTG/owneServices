#if DEBUG
using System;
using System.IO;
using CargoWise.BuildTools;
using Enterprise.DocBuilderTemplateMerge;

namespace Enterprise.DocumentEngine
{
	internal class CustomizedDocumentElementsFileManager
	{
		public string GetFilePath()
		{
			var pendingChanges = SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges();

			if (PendingChangesForTesting != null)
			{
				pendingChanges = PendingChangesForTesting;
			}

			var customizedDocumentElementsFilePath = Array.Find(pendingChanges, pendingChange => IsCustomizedDocumentElement(pendingChange)) ?? Path.Combine(BuildConstants.GetLocalPath(SystemDocumentElementsMerger.DocBuilderDocumentsDir), SystemDocumentElementsMerger.NewCustomizedDocumentElementsFileName());
			return customizedDocumentElementsFilePath;
		}

		bool IsCustomizedDocumentElement(string filePath)
		{
			return filePath.Contains(SystemDocumentElementsMerger.DocBuilderDocumentsDir) && SystemDocumentElementsMerger.CustomizedDocumentElementsFileNameRegex.IsMatch(filePath);
		}

		public string[] PendingChangesForTesting;
	}
}
#endif
