#if DEBUG
using System;
using System.Collections.Generic;
using CargoWise.BuildTools;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DbUpgrader.Data;

namespace Enterprise.DocumentEngine.Build
{
	public class DocumentDataTaskSetup : DataTaskSetup
	{
		public DocumentDataTaskSetup(UpgradeTask task, ISetupController controller, UpgradeTask taskForReading)
			: base(task, controller, taskForReading)
		{
		}

		protected override void PerformPreApplyTasks()
		{
			RebuildDocumentsComplete();
		}

		void RebuildDocumentsComplete()
		{
			List<LogMessage> messages;
			string documentsXmlRelativePath = fTask.ResourceFile.FileFullPath.Replace(BuildConstants.LocalEnterprisePath, "");
			string documentsCompleteXmlRelativePath = fTaskForReading.ResourceFile.FileFullPath.Replace(BuildConstants.LocalEnterprisePath, "");

			if (!TemplateSerializationHelper.Execute(BuildConstants.LocalEnterprisePath, documentsXmlRelativePath, documentsCompleteXmlRelativePath, out messages))
			{
				throw new ApplicationException("Error rebuilding DocumentsComplete.xml.");
			}
		}
	}
}
#endif