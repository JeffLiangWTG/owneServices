using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class ResetCancelledEntryGridContextMenuItemComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public ResetCancelledEntryGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		var entryHeader = DataContext;
		if (entryHeader is null)
		{
			return;
		}

		var message = Res.GetString("99139AB7-BCEA-4117-8504-30DA7D1F8205", "Are you sure you want to Reset this Canceled Entry?\r\nAll registration data will also be deleted");
		var caption = Res.GetString("E6533AA2-424E-4096-BB83-9CF29DCA3768", "Reset Entry");

		var result = Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Information);
		if (result == ZDialogResult.Yes)
		{
			entryHeader.ResetCancelledEntry();
			Globals.Message.Show(Res.GetString("D65077EB-4442-41B5-98ED-BD87F0E50558", "Entry has been reset"));
		}
	}

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem)
	{
		var entryHeader = DataContext;
		if (entryHeader != null)
		{
			return entryHeader.Declaration.IsUCC6 && entryHeader.IsCancellationAcceptedBySystem;
		}
		return true;
	}

	protected override ZMenuItem GetMenuItem() => new ZMenuItem(Res.GetString("43AFE61D-7B1D-4EBC-8A14-00D78AB655C7", "Reset Canceled Entry"));
}
