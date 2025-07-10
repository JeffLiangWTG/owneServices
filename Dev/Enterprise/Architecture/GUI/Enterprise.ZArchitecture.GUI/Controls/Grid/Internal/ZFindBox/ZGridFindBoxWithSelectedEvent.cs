namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZGridFindBoxWithSelectedEvent : ZGridFindBox
	{
		public ZGridFindBoxWithSelectedEvent() : base()
		{
		}

		/// <summary>
		/// The 'Selected' event is called when the object is selected.
		/// The argurment of the 'Selected' event contains the array of selected BusinessObjects.
		/// </summary>
		public event EmbeddedModulePopup.SelectedEventHandler Selected;

		protected sealed override IFindBoxPopup GetNewPopupForm()
		{
			var result = base.GetNewPopupForm();
			HookupSelectedEvent(result);
			return result;
		}

		protected sealed override void OnPopupFormClosed(IFindBoxPopup popupForm)
		{
			if (popupForm is EmbeddedModulePopup embeddedPopup)
			{
				embeddedPopup.Selected -= Selected;
			}
			base.OnPopupFormClosed(popupForm);
		}

		void HookupSelectedEvent(IFindBoxPopup popup)
		{
			if (popup is EmbeddedModulePopup embeddedPopup)
			{
				embeddedPopup.Selected += Selected;
			}
		}
	}
}
