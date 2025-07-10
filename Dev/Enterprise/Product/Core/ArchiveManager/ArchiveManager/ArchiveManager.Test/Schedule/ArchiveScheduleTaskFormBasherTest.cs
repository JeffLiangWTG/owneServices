using System.Windows.Forms;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.GUI.Schedule;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Schedule
{
	[TestedType(typeof(ArchiveScheduleTaskForm))]
	class ArchiveScheduleTaskFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var task = Factory.New<ArchiveScheduleTask>();
			return new ArchiveScheduleTaskForm(task);
		}
	}
}
