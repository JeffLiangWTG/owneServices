using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class AmendmentGridContextMenuItemComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public AmendmentGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		var entryHeader = DataContext;
		if (entryHeader is null)
		{
			return;
		}

		if (entryHeader.HasMrn)
		{
			var result = PromptUserHelper.ShowEntryAmendmentConfirmation();
			if (result == ZDialogResult.OK)
			{
				SetAsAmendingAndPrompt();
			}
			return;
		}

		var handler = new EntryAmendmentHandler(entryHeader.Factory);
		using (var form = new EntryAmendmentForm(handler))
		{
			if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
			{
				SetAsAmendingAndPrompt(handler.MovementReferenceNumber, handler.TotalEntryLines);
			}
		}

		void SetAsAmendingAndPrompt(ZString? movementReferenceNumber = null, ZInt? totalEntryLines = null)
		{
			entryHeader.SetAsAmending(movementReferenceNumber, totalEntryLines);
			Globals.Message.Show(PromptUserHelper.EntryAmendmentCompleteMessage);
		}
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return DataContext?.IsInAmendableStatus ?? false;
	}

	protected override ZMenuItem GetMenuItem() => new ZMenuItem(Res.GetString("83E74FFC-0053-4D6D-81E7-2C070EC626B5", "Set Entry as Amendment"));
}
