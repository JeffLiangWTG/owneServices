using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	public class MenuItemDescriptor<T>
		where T : BusinessObject
	{
		public MenuItemDescriptor(T payload)
		{
			Payload = payload;
		}

		public string Text { get; set; }
		public EventHandler<MenuItemClickHandlerEventArgs> ClickHandler { get; set; }

		public T Payload { get; set; }

		public bool ShowFormForPayloadAfterExecute { get; set; }
		public ControllerID ControllerID { get; set; }
	}

	public class MenuItemClickHandlerEventArgs : EventArgs
	{
		public MenuItemClickHandlerEventArgs(bool showPayloadFormModally)
		{
			ShowPayloadFormModally = showPayloadFormModally;
		}

		public bool ShowPayloadFormModally { get; private set; }
	}
}
