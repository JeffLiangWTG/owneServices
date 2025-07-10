using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Customs.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class MessagesTabUserControl : BaseMessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			SetupMessageColumns();
			InterpretedMessageTextBox.TextChanged += new EventHandler(InterpretedMessageTextBox_TextChanged);
			if (!DesignModeFinder.IsDesigning && !Globals.IsTest)
			{
				UserIdleWorker.QueueWorkItem(this, new MethodInvoker(RefreshDocumentText), null); // updates the web browser control's HTML with the text in the hidden text box, but only when the computer first becomes idle.
			}
			InterpretedMessageTextWebBrowser.AllowOverlap(InterpretedMessageTextBox);
		}

		void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshDocumentText();
		}

		protected virtual void RefreshDocumentText()
		{
			try
			{
				var documentText = InterpretedMessageTextBox.Text;
				if (!string.IsNullOrEmpty(documentText.Trim()))
				{
					InterpretedMessageTextWebBrowser.DocumentText = documentText;
				}
				else
				{
					InterpretedMessageTextWebBrowser.DocumentText = "<html/>";
				}
			}
			catch (ArgumentException ex)
			{
				ErrorReporter.ReportOnce("EU-GUI-MessageTextRefresh", "Could not refresh web browser text.  Text was: " + InterpretedMessageTextBox.Text, ex);
			}
		}

		void SaveMessageToDisk(object sender, EventArgs ev)
		{
			using (var dialog = new ZFolderBrowserDialog())
			{
				dialog.RequireMappablePath = true;
				dialog.Description = Res.GetString("2df76cba-c720-488b-81c1-e33e70c6fd17", "Select the destination folder");
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					int generatedCount = 0;
					foreach (EDIMessage message in MessagesGrid.SelectedElements)
					{
						try
						{
							var filename = FormattableString.Invariant($"EdiMessage_{message.EM_ApplicationCode}_{message.EM_MessageNum}.txt");
							var filePath = Path.Combine(dialog.UnmappedSelectedPath, filename);
							using (var reader = message.GetEM_MessageTextReader())
							using (var writer = new StreamWriter(ZSaveFileDialog.OpenFile(filePath)))
							{
								reader.CopyTo(writer);
							}
							generatedCount++;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Globals.Message.Show(Res.GetString("6cd1e00a-8445-44a5-9a91-3d52d420a636", "Could not save. {0}", ex.Message));
						}
					}
					if (generatedCount == 1)
					{
						Globals.Message.Show(Res.GetString("f866a614-e0cb-4512-a17c-75015ce3921e", "One message was exported"));
					}
					else
					{
						Globals.Message.Show(Res.GetString("bd8178c7-7f06-4908-b1e3-824fd4ee7348", "{0} messages were exported", generatedCount.ToString(Culture.Current)));
					}
				}
			}
		}

		void SetupMessageColumns()
		{
			MessagesGrid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("B4C2319C-6183-48E9-A321-2D10C13D7A24", "Training Flag"),
				ColumnName = EDIMessage.Schema.EM_IsTestMessage,
				IsReadOnly = true
			});

			MessagesGrid.ContextMenu.MenuItems.Add(Res.GetString("2ad8c302-d8a6-4baa-ba97-f7380ce36d0a", "Save Message to Disk"), SaveMessageToDisk);
		}
	}
}
