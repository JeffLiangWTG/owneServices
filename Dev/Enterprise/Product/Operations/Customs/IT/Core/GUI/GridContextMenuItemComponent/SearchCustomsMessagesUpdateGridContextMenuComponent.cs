using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class SearchCustomsMessagesUpdateGridContextMenuComponent : GridContextMenuItemComponent<ITEDIMessage>
{
	public SearchCustomsMessagesUpdateGridContextMenuComponent(ZGrid grid) : base(grid)
	{
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		try
		{
			var currentMessage = DataContext;
			if (currentMessage != null)
			{
				var updatesRequester = new CustomsMessageUpdatesRequester();
				var fileName = updatesRequester.RequestUpdates(currentMessage);
				Globals.Message.Show(Res.GetString("85347EE9-64DC-401C-9C20-EE93EF080C0C", "Customs updates have been requested for '{0}'.", fileName));
			}
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			Globals.Message.ShowError(ex.Message);
		}
	}

	protected override ZMenuItem GetMenuItem() => new ZMenuItem(Res.GetString("69D52578-FF7C-4F41-A7EB-B4D757694B4B", "Search Customs Messages Updates"));

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		var dataContext = DataContext;

		return Grid.SelectedElements.Length == 1
			&& dataContext?.Interchange != null
			&& SADConstants.CustomsInterchangeType.IdocR == dataContext.EM_MessageType;
	}

	#region Implementation

	protected override ITEDIMessage GetDataContext()
	{
		return Grid.GetCurrent() as ITEDIMessage;
	}

	#endregion
}
