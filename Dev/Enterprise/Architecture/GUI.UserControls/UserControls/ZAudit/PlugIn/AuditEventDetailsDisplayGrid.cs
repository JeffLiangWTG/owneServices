using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn;

[ToolboxItem(false)]
public class AuditEventDetailsDisplayGrid : ZDisplayGrid
{
	protected override void SetupContextMenu()
	{
		base.SetupContextMenu();
		AddSaveBinaryMenuItem();
	}

	void AddSaveBinaryMenuItem()
	{
		ContextMenu.MenuItems.Add(SaveBinaryMenuItem);
	}

	internal ZMenuItem SaveBinaryMenuItem
	{
		get
		{
			if (saveBinanryMenuItem == null)
			{
				saveBinanryMenuItem = new ZMenuItem(ResString.GetMultilingualString("45FA6948-C1E4-45EE-88CD-43DFB9348E4C", "Save as..."), SaveBinaryMenuItem_Click);
				saveBinanryMenuItem.Visible = false;
			}

			return saveBinanryMenuItem;
		}
	}

	ZMenuItem saveBinanryMenuItem;

	void SaveBinaryMenuItem_Click(object sender, EventArgs e)
	{
		DoSave();
	}

	void DoSave()
	{
		var auditChange = SaveBinaryMenuItem.Tag as AuditChange.AuditChangeWithBinaryValue;
		if (auditChange?.BinaryValue == null)
		{
			return;
		}

		try
		{
			var columnName = auditChange.RealColumnName;
			byte[] binValue = auditChange?.BinaryValue;
			var fileExtensionFilter = dataVersionLogValueFormatter?.GetFileExtensionFilter(columnName, binValue);

			using (var dialog = new ZSaveFileDialog())
			{
				dialog.Filter = fileExtensionFilter;
				dialog.CheckPathExists = true;
				dialog.AddExtension = true;
				dialog.OverwritePrompt = true;
				dialog.RestoreDirectory = true;
				dialog.Title = Res.GetString("915D8C58-B110-4154-A583-B8AD1CBE28DC", "Save as...");

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				using (var fileStream = dialog.OpenFile())
				{
					fileStream.Write(binValue, 0, binValue.Length);
				}
			}
		}
		catch (DataVersionLogValueFormatterException ex)
		{
			NotificationHandler.Instance.ReportError(ex.Message, "");
		}
	}

	protected override void HookContextMenu()
	{
		base.HookContextMenu();
		ContextMenu.Popup += ContextMenu_Popup;
	}

	protected override void UnHookContextMenu()
	{
		base.UnHookContextMenu();
		ContextMenu.Popup -= ContextMenu_Popup;
	}

	void ContextMenu_Popup(object sender, EventArgs e)
	{
		var row = GetFirstSelectedRow();
		if (row == null)
		{
			return;
		}

		var auditChange = ((AuditChange)row);

		if (dataVersionLogValueFormatter != null && dataVersionLogValueFormatter.CanSaveBinaryValue(auditChange.RealColumnName) && (auditChange as AuditChange.AuditChangeWithBinaryValue)?.BinaryValue != null)
		{
			SaveBinaryMenuItem.Caption = ResString.GetMultilingualString("2C3A3154-3D6E-4A5C-8C5E-E74C764AFDC0", "Save {0} as...", auditChange.ColumnName);
			SaveBinaryMenuItem.Visible = true;
			SaveBinaryMenuItem.Tag = row;
		}
		else
		{
			SaveBinaryMenuItem.Visible = false;
			SaveBinaryMenuItem.Tag = null;
		}
	}

#if DEBUG
	protected virtual
#endif
		DataVersionLogValueFormatter dataVersionLogValueFormatter => (FindForm() as ZAuditLogsForm)?.AuditParentBizObj?.DataVersionLogValueFormatter;
}
