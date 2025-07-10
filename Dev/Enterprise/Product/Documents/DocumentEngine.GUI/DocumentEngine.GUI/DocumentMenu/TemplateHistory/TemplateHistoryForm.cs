using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	partial class TemplateHistoryForm : ZChildForm
	{
		public TemplateHistoryForm(StmTemplateBase bo)
			: base(bo)
		{
			InitializeComponent();
			AddSaveAsMenuItemForTemplate();
			ShowTemplateHistory();
		}

		void TemplateHistoryForm_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Escape)
			{
				Close();
			}
		}

		void ShowTemplateHistory()
		{
			var showGrid = ((StmTemplateBase)DataSource).TemplateHistories != null;
			TemplateHistoryDetailGrid.Visible = showGrid;
			EmptyLabel.Visible = !showGrid;
		}

		void AddSaveAsMenuItemForTemplate()
		{
			var menuItem = CreateSaveAsMenuItem(() => { return TemplateHistoryDetailGrid.ListManager.GetCurrent() as IeDocBase; });
			var contextMenu = TemplateHistoryDetailGrid.ContextMenu;
			contextMenu.MenuItems.Add(0, menuItem);
		}

		static MenuItem CreateSaveAsMenuItem(Func<IeDocBase> getHistoryTemplate)
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("8f632093-5374-4bea-bf89-fe36e6704944", "Save As..."), new EventHandler((sender, e) =>
			{
				var template = getHistoryTemplate();
				if (template != null)
				{
					using (var dialog = new ZSaveFileDialog())
					{
						var filename = Path.GetFileName(template.FileName);
						var extension = Path.GetExtension(filename);
						if (string.IsNullOrEmpty(extension))
						{
							//Assume format is XLS if we couldn't retrieve the extension
							extension = AttachmentTypeList.Codes.Xls;
						}
						if (string.IsNullOrEmpty(filename))
						{
							//Use template name if filename is empty
							filename = MakeFilenameSafe.MakeSafe(template.FileName + extension, '_');
						}
						dialog.FileName = filename;
						dialog.Filter = (NoResString)"Excel Templates|*" + extension;
						dialog.OverwritePrompt = true;

						if (dialog.ShowDialog() == DialogResult.OK)
						{
							using (var stream = dialog.OpenFile())
							{
								stream.Write(template.ImageData, 0, template.ImageData.Length);
								Globals.Message.Show(ResString.GetMultilingualString("978b5f76-9806-4638-9396-64f1c3055cb4", "History Template has been saved to {0}.", dialog.UnmappedFileName), ResString.GetMultilingualString("aba66940-49ee-4f0e-98ef-5f06126dbd36", "Save Successful"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
							}
						}
					}
				}
			}));

			return result;
		}
	}
}
