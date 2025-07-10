using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

class IvistoRequestGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public IvistoRequestGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem) : base(grid, parentMenuItem)
	{
		Argument.NotNull(parentMenuItem, nameof(parentMenuItem));
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("A17949B4-9DF1-460F-8AEC-84CB24FFA07C", "IVISTO Request");
		return new ZMenuItem(menuItemText) { Name = "IvistoRequest" };
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return base.IsMenuItemVisible(menuItem) && IsUcc6ExportDeclaration();
	}

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem)
	{
		return base.IsMenuItemEnabled(menuItem) && EntryHasBothMrnAndReleaseCode();
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		var entryHeader = EntryHeader;
		if (entryHeader == null)
		{
			return;
		}

		if (IsFormPreSaved(entryHeader, menuItem))
		{
			SendIvistoRequest(entryHeader);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	bool IsUcc6ExportDeclaration()
	{
		var declaration = EntryHeader?.Declaration;
		return declaration?.IsUCC6AndIsExport ?? false;
	}

	bool EntryHasBothMrnAndReleaseCode()
	{
		var entryHeader = EntryHeader;
		return entryHeader != null
			&& !entryHeader.MovementReferenceNumber.IsEmpty
			&& entryHeader.EntryNumbersProvider.ReleaseInfo != null;
	}

	void SendIvistoRequest(CusEntryHeader entryHeader)
	{
		try
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var ivistoRequestMessageFactory = GetNewIvistoRequestMessageFactory();
			ivistoRequestMessageFactory.CreateIvistoRequestMessage(entryHeader, factory);
			factory.Save();

			Globals.Message.Show(IvistoRequestSentMessageConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	protected virtual IvistoRequestMessageFactory GetNewIvistoRequestMessageFactory()
	{
		return new IvistoRequestMessageFactory();
	}

	ResourceString IvistoRequestSentMessageConfirmation => ResString.GetMultilingualString("24EE0811-9197-4D0B-BD3B-CF509219E737", "IVISTO Request has been sent to customs.");
}
