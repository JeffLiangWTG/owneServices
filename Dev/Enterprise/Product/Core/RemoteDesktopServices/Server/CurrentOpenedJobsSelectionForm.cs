using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Enterprise.RemoteDesktopServices.Server
{
	public partial class CurrentOpenedJobsSelectionForm : Form
	{
		public CurrentOpenedJobsSelectionForm(IEnumerable<Form> formsToSelect)
		{
			OpenedJobs = (formsToSelect ?? throw new ArgumentNullException(nameof(formsToSelect))).ToList();
			InitializeComponent();

			Load += CurrentOpenedJobsSelectionForm_Load;
		}

		void CurrentOpenedJobsSelectionForm_Load(object sender, EventArgs e)
		{
			comboBox1.DropDownWidth = Math.Max(comboBox1.Width, GetMaxDropDownWidth(comboBox1));
		}

		static int GetMaxDropDownWidth(ComboBox comboBox)
		{
			if (comboBox.Items.Count == 0)
			{
				return comboBox.Width;
			}

			using (var g = comboBox.CreateGraphics())
			{
				var font = comboBox.Font;
				var vertScrollBarWidth = (comboBox.Items.Count > comboBox.MaxDropDownItems)
					? SystemInformation.VerticalScrollBarWidth : 0;

				return (int)comboBox.Items
					.Cast<object>()
					.Select(x => g.MeasureString(comboBox.GetItemText(x), font).Width)
					.Max() + vertScrollBarWidth;
			}
		}

		public List<Form> OpenedJobs { get; }
		public Form SelectedForm { get; private set; }

		void okButton_Click(object sender, EventArgs e)
		{
			SelectedForm = (Form)comboBox1.SelectedItem;
			Close();
		}

#if DEBUG
		internal ComboBox ComboBox => comboBox1;
#endif
	}
}
