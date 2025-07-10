using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ServiceManager.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class FileBasedLogViewerControl : ZUserControl, ILogViewerControl
	{
		public FileBasedLogViewerControl()
		{
			InitializeComponent();

			MenuItem sendToMenuItem = new ZMenuItem(ResString.GetMultilingualString("ca137927-d486-4934-9357-acab39f1cdec", "Send To..."), (sender, e) => SendLog());
			MenuItem refreshMenuItem = new ZMenuItem(ResString.GetMultilingualString("383906c7-8c84-4aca-b492-3d3904001da9", "Refresh"), (sender, e) => RefreshLogs(), Shortcut.F5);

			eventGrid.ContextMenu.MenuItems.Add(sendToMenuItem);
			eventGrid.ContextMenu.MenuItems.Add("-");
			eventGrid.ContextMenu.MenuItems.Add(refreshMenuItem);
		}

		FileBasedLogViewer FileBasedLogViewer
		{
			get { return (FileBasedLogViewer)BindingSource.Current; }
		}

		public void RefreshLogs()
		{
			string currentLogFile = null;
			if (LogFileGrid.ListManager != null && LogFileGrid.CurrentRowIndex >= 0)
			{
				currentLogFile = FileBasedLogViewer.LogFileList[LogFileGrid.CurrentRowIndex].Name;
			}

			FileBasedLogViewer.ReloadLogFileList();

			if (currentLogFile != null)
			{
				for (var i = 0; i < FileBasedLogViewer.LogFileList.Count; i++)
				{
					if (FileBasedLogViewer.LogFileList[i].Name == currentLogFile)
					{
						FileBasedLogViewer.LogFileList[i].ReloadEvents();
						LogFileGrid.CurrentRowIndex = i;
						break;
					}
				}
			}
		}

		void SendLog()
		{
			LogFileRecord currentLog = null;
			if (LogFileGrid.ListManager != null && LogFileGrid.CurrentRowIndex >= 0)
			{
				currentLog = FileBasedLogViewer.LogFileList[LogFileGrid.CurrentRowIndex];
			}

			if (currentLog != null)
			{
				using var sendToForm = new SendToForm();
				if (sendToForm.ShowDialog(this) == DialogResult.OK)
				{
					currentLog.Send(sendToForm.SendToAddress.Recipients.ToArray());
				}
			}
		}

		void eventGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			var eventRecord = (EventRecord)e.ObjectAtRow;
			if (eventRecord.Type == "Error")
			{
				e.Colour = Color.Tomato;
			}
			else if (eventRecord.Type == "Warning")
			{
				e.Colour = Color.Yellow;
			}
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<FileBasedLogViewerControl>().Result;
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			if (FileBasedLogViewer != null)
			{
				taskTypeDropEdit.ReadOnly = FileBasedLogViewer.TaskTypeReadOnly;
			}
		}
	}
}
