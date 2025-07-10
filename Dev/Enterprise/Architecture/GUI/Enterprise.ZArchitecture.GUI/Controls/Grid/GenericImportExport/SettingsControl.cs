using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	[ToolboxItem(false)]
	public partial class SettingsControl : ZUserControl
	{
		public SettingsControl()
		{
			InitializeComponent();
		}

		ImportExportWizard Wizard
		{
			get { return (ImportExportWizard)BindingSource.Current; }
		}

		void AddSettingButton_Click(object sender, EventArgs e)
		{
			Wizard.Setting = ZString.Empty;
			SettingsDropEdit.Focus();
		}

		void RemoveSettingButton_Click(object sender, EventArgs e)
		{
			if (!Wizard.Setting.IsEmpty)
			{
				if (Wizard.IsSettingSystemSetting)
				{
					Globals.Message.ShowInformation(Res.GetString("6a4d7f96-5df5-4d6d-a4b1-4407c4a55d81", "System settings cannot be removed."));
				}
				else
				{
					var message = Res.GetString("629c4537-43e7-44ba-9f24-f7309c22ec23", "Are you sure you want to remove the settings permanently?");

					var result = Globals.Message.Show(message, FindForm().Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						Wizard.RemoveSettings();
					}
				}
			}
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			var dialog = new ZOpenFileDialog();
			dialog.Filter = SettingsFileFilter;
			dialog.CheckFileExists = true;

			var dr = dialog.ShowDialog(this);
			if (dr == DialogResult.OK)
			{
				try
				{
					using (var stream = dialog.OpenFile())
					{
						ImportExportWizardSettings wizardSettings = null;
						try
						{
							using (var reader = new StreamReader(stream))
							{
								wizardSettings = (ImportExportWizardSettings)ImportExportWizardSettings.FromXml(reader.ReadToEnd(), Wizard.SettingsType, true);
							}
						}
						catch (XmlException ex)
						{
							Globals.Message.ShowError(ex.Message);
						}
						catch (IOException ex)
						{
							Globals.Message.ShowError(ex.Message);
						}
						catch (Exception ex)
						{
							if (ex.IsCriticalException()) { throw; }
							else
							{
								Globals.Message.ShowDeveloperException("Data Import Wizard Settings Import", ex);
							}
						}
						if (wizardSettings != null)
						{
							Wizard.SetSettings(wizardSettings);
						}
					}
				}
				catch (IOException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void ExportButton_Click(object sender, EventArgs e)
		{
			var dialog = new ZSaveFileDialog();
			dialog.Filter = SettingsFileFilter;
			dialog.RestoreDirectory = true;

			var dr = dialog.ShowDialog(this);
			if (dr == DialogResult.OK)
			{
				using (var writer = new StreamWriter(dialog.OpenFile()))
				{
					writer.Write(Wizard.GetSettings().AsXml(false));
				}
			}
		}

		static string SettingsFileFilter
		{
			get { return Res.GetString("171e7bd3-d3c6-408b-bd3c-3fc21829ad0c", "Settings Files (*.settings)|*.settings|All files (*.*)|*.*"); }
		}
	}
}
