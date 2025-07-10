using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(FileBasedLogViewerControl))]
	class FileBasedLogViewerControlTest : TestCase
	{
		[RequiresSTA]
		public void TestTaskTypeDropEditIsReadOnly() => AssertTaskTypeReadOnly(true, true);
		[RequiresSTA]
		public void TestTaskTypeDropEditIsNotReadOnly() => AssertTaskTypeReadOnly(false, false);

		void AssertTaskTypeReadOnly(bool isReadOnly, bool expectedReadOnly)
		{
			var bindingSource = new FileBasedLogViewer();
			bindingSource.TaskTypeReadOnly = isReadOnly;

			using var view = new FileBasedLogViewerControl();
			view.SetDataBinding(bindingSource, "");
			var taskTypeDropEdit = view.FindSingleOrDefault<ZDropEdit>("taskTypeDropEdit");
			AssertNotNull("taskTypeDropEdit", taskTypeDropEdit);
			AssertEquals(nameof(taskTypeDropEdit.ReadOnly), expectedReadOnly, taskTypeDropEdit.ReadOnly);
		}
	}
}
