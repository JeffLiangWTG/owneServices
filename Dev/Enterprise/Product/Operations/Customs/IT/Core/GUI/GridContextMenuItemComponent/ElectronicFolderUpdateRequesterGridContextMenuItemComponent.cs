using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class ElectronicFolderUpdateRequesterGridContextMenuItemComponent : GridContextMenuItemComponent<ITEDIMessage>
{
	public ElectronicFolderUpdateRequesterGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override ZMenuItem GetMenuItem()
	{
		var resendEFStatusRequestMenuItem = new ZMenuItem(ResString.GetMultilingualString("28FE6077-E58D-4E3D-9016-12337549F5CB", "Resend EF Status Request"));
		resendEFStatusRequestMenuItem.Name = "ResendEfStatusRequest";
		return resendEFStatusRequestMenuItem;
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		if (DataContext?.EM_LinkedObject is ISingleWindowRequestDataProvider dataProvider)
		{
			if (dataProvider.IssueDate.IsEmpty)
			{
				Globals.Message.Show(Res.GetString("23072A52-BE56-45A1-A85E-24C2687DC44A", "This entry is not registered, the request cannot be performed."));
				return;
			}

			ResendEFStatusRequest(dataProvider);
		}
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return base.IsMenuItemVisible(menuItem)
			&& !((DataContext?.EM_LinkedObject as CusEntryHeader)?.Declaration?.IsUCC6 ?? false);
	}

	void ResendEFStatusRequest(ISingleWindowRequestDataProvider dataProvider)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			new ElectronicFolderUpdatesRequester(dataProvider, factory).RequestUpdates();

			factory.Save();
			Globals.Message.Show(Res.GetString("F22878FB-AA05-4DEB-B2EF-E40DF5122266", "EF status update request has been sent to customs."));
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}
}
