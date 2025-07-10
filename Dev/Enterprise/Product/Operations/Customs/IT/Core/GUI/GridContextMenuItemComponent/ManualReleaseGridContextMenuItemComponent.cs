using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class ManualReleaseGridContextMenuItemComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public ManualReleaseGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		var entryHeader = DataContext;
		if (entryHeader == null)
		{
			return;
		}

		var handler = new EntryManualReleaseHandler(entryHeader);
		var result = handler.CanDoManualRelease();

		if (result.CanDoManualRelease)
		{
			using (var form = new EntryManualReleaseForm(handler))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					entryHeader.DoManualReleaseAndSetAsChanged(handler);
				}
			}
		}
		else
		{
			Globals.Message.ShowError(result.ErrorMessage);
		}
	}

	protected override ZMenuItem GetMenuItem() => new ZMenuItem(Res.GetString("2C6C076C-EB31-41CA-AFFB-F4465115AC4D", "Manual Release"));
}
