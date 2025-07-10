#if DEBUG
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DbUpgrader.Data;

namespace Enterprise.DocumentEngine.Build
{
	public class DocumentsSetupController : DataUpgradeSetupController
	{
		/// <summary>
		/// Manage a non client specific document set.
		/// </summary>
		public DocumentsSetupController()
				: base(System.Array.Empty<UpgradeTask>())
		{
			AddDocCompleteTask();
		}

		public DocumentsSetupController(bool createDocCompleteTask)
			: base(System.Array.Empty<UpgradeTask>())
		{
			if (createDocCompleteTask)
			{
				AddDocCompleteTask();
			}
		}

		void AddDocCompleteTask()
		{
			TaskSetupList.Add(new DocumentDataTaskSetup(new DocumentsUpgradeTask(), this, new DocumentsUpgradeTask(new DocumentsCompleteDataFile())));
		}
	}
}
#endif
