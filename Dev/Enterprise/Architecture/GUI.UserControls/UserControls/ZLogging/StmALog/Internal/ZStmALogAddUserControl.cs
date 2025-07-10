using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public partial class ZStmALogAddUserControl : ZUserControl
	{
		public ZStmALogAddUserControl()
		{
			InitializeComponent();
		}

		#region Extra Info Button Click

		void ExtraInfoButton_Click(object sender, EventArgs e)
		{
			var log = BindingSource.Current as BaseStmALog;

			if (log != null)
			{
				var eventReference = new EventReference(log.SL_SE_NKEvent, log.SL_Reference);

				using (var form = new EventReferenceForm(eventReference))
				{
					if (Globals.IsTest)
					{
						form.Show();
					}
					else if (form.ShowDialog() == DialogResult.OK)
					{
						log.SL_Reference = form.ReferenceText;
					}
				}
			}
		}

		#endregion

		internal void SetEventAndReferenceToReadOnly(bool isEventAndReferenceReadOnly)
		{
			EventsDropEdit.ReadOnly = isEventAndReferenceReadOnly;
			DescriptionTextBox.ReadOnly = isEventAndReferenceReadOnly;
			ExtraInfoButton.ReadOnly = isEventAndReferenceReadOnly;
		}
	}
}
