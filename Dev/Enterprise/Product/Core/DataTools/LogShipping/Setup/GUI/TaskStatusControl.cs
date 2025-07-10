using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class TaskStatusControl : UserControl
	{
		public TaskStatusControl()
		{
			InitializeComponent();
			Status = TaskStatus.Initial;
		}

		public string TextLabel
		{
			get { return label.Text; }
			set { label.Text = value; }
		}

		#region Status

		public TaskStatus Status
		{
			get
			{
				return status;
			}
			set
			{
				status = value;

				pictureBox.Image = stateImageList.Images[(int)status];

				SetFontStyle(status);
			}
		}

		TaskStatus status;

		#endregion

		void SetFontStyle(TaskStatus taskState)
		{
			switch (taskState)
			{
				case TaskStatus.Initial:
					label.ForeColor = Color.DarkGray;
					break;
				case TaskStatus.Setup:
					label.ForeColor = Color.Black;
					break;
				case TaskStatus.Success:
					label.ForeColor = Color.SeaGreen;
					break;
				case TaskStatus.Skipped:
					label.ForeColor = Color.SlateGray;
					break;
				case TaskStatus.Failure:
					label.ForeColor = Color.Red;
					break;
			}
		}
	}

	public enum TaskStatus
	{
		Initial,
		Success,
		Setup,
		Skipped,
		Failure
	}
}
