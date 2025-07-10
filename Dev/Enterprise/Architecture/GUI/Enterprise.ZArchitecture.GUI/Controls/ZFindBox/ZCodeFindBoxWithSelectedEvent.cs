using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A Module CodeFindBox that allows users to hook to the 'Selected' event.
	/// The 'Selected' event is called when the object is selected.
	/// The argurment of the 'Selected' event contains the array of selected BusinessObjects.
	/// </summary>
	[SuppressFormDesignerAnalysis]
	public class ZCodeFindBoxWithSelectedEvent : ZCodeFindBox
	{
		public ZCodeFindBoxWithSelectedEvent()
			: base()
		{
		}

		/// <summary>
		/// The 'Selected' event is called when the object is selected.
		/// The argurment of the 'Selected' event contains the array of selected BusinessObjects.
		/// </summary>
		public event EmbeddedModulePopup.SelectedEventHandler Selected;

		#region Implementation

		protected sealed override IFindBoxPopup GetNewPopupForm()
		{
			var result = GetNewPopupFormCore();
			HookupSelectedEvent(result);
			return result;
		}

		protected virtual IFindBoxPopup GetNewPopupFormCore()
		{
			return base.GetNewPopupForm();
		}

		void HookupSelectedEvent(IFindBoxPopup popup)
		{
			var embeddedPopup = popup as EmbeddedModulePopup;
			if (embeddedPopup != null)
			{
				embeddedPopup.Selected += Selected;
			}
		}

		protected sealed override void OnPopupFormClosed(IFindBoxPopup popupForm)
		{
			var embeddedPopup = popupForm as EmbeddedModulePopup;
			if (embeddedPopup != null)
			{
				embeddedPopup.Selected -= Selected;
			}
			OnPopupFormClosedCore(popupForm);
		}

		protected virtual void OnPopupFormClosedCore(IFindBoxPopup popupForm)
		{
			base.OnPopupFormClosed(popupForm);
		}

		#endregion
	}
}
