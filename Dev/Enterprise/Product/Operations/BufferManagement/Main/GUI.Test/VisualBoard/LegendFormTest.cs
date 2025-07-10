using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(LegendForm))]
	class LegendFormTest : ZFormBasherTest
	{
		public void TestLegendComponents()
		{
			using (var form = new LegendForm())
			{
				var statuses = form.TaskStatusPanel.Controls.OfType<ZPanel>().ToArray();
				AssertEquals(8, statuses.Length);
				AssertGraphicNotNullAndDescriptionPresent(statuses[0], "Task is Working");
				AssertGraphicNotNullAndDescriptionPresent(statuses[1], "Task is Suspended");
				AssertGraphicNotNullAndDescriptionPresent(statuses[2], "For task cards, task is startable (task which can be worked on). For workflow cards, a workflow that has a startable task.");
				AssertGraphicNotNullAndDescriptionPresent(statuses[3], "Task has not been assigned to a resource");
				AssertGraphicNotNullAndDescriptionPresent(statuses[4], "This task precedes another task on the same workflow that is assigned to a CCR.");
				AssertGraphicNotNullAndDescriptionPresent(statuses[5], "This task follows another task on the same workflow that is assigned to a CCR.");
				AssertGraphicNotNullAndDescriptionPresent(statuses[6], "This task precedes another task on the same workflow that is assigned to a CCR and is in Zone 1/0 of the relevant buffer.");
				AssertGraphicNotNullAndDescriptionPresent(statuses[7], "This task follows another task on the same workflow that is assigned to a CCR and is in Zone 1/0 of the relevant buffer.");
			}
		}

		void AssertGraphicNotNullAndDescriptionPresent(ZPanel panel, string description)
		{
			var pictureBox = panel.Controls.OfType<PictureBox>().First();
			AssertNotNull(pictureBox.Image);

			var label = panel.Controls.OfType<ZLabel>().First();
			AssertEquals(description, label.Text);
		}

		protected override Form GetFormToBashCore()
		{
			return new LegendForm();
		}
	}
}
