using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A Module ZGuidFindBox that allows users to hook to the 'Selected' event.
	/// The 'Selected' event is called when the object is selected.
	/// The argurment of the 'Selected' event contains the array of selected BusinessObjects.
	/// </summary>
	[SuppressFormDesignerAnalysis]
	public class ZGuidFindBoxWithSelectedEvent : ZGuidFindBox
	{
		public ZGuidFindBoxWithSelectedEvent()
			: this(null, null)
		{
		}

		public ZGuidFindBoxWithSelectedEvent(Func<ZCodeFindBox, IBusinessObjectCollection> popupCollectionDelegate, ZCodeFindBox linkedCodeFindBox)
			: base()
		{
			PopupCollectionDelegate = popupCollectionDelegate;
			LinkedCodeFindBox = linkedCodeFindBox;
		}
		readonly Func<ZCodeFindBox, IBusinessObjectCollection> PopupCollectionDelegate;
		readonly ZCodeFindBox LinkedCodeFindBox;

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

#if DEBUG
		public ZFilterModule ModuleForTest;
#endif
		protected virtual IFindBoxPopup GetNewPopupFormCore()
		{
			IFindBoxPopup popup = null;
			var module = NewModuleFromModuleID();
#if DEBUG
			ModuleForTest = module;
#endif
			if (module != null)
			{
				module.OverrideModuleDecisionProvider(new CustomPopupModuleDecisionProvider(this, PopupCollectionDelegate, LinkedCodeFindBox));
				popup = CreateEmbeddedPopup(module);
			}
			else
			{
				ErrorReporter.ReportOnce("InvalidModuleIDForControl:" + Name, "A valid ModuleID could not be found for : " + Name + " (List.GetType() is " + (List == null ? "null" : List.GetType().FullName) + ")");
				popup = new ZCodeFindBoxPopup(PopupCaption);
			}
			return popup;
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

		#region CustomPopupModuleDecisionProvider
		class CustomPopupModuleDecisionProvider : PopupModuleDecisionProvider
		{
			public CustomPopupModuleDecisionProvider(IFindBox findBox, Func<ZCodeFindBox, IBusinessObjectCollection> popupCollectionDelegate, ZCodeFindBox linkedCodeFindBox)
				: base(findBox)
			{
				PopupCollectionDelegate = popupCollectionDelegate;
				LinkedCodeFindBox = linkedCodeFindBox;
			}
			readonly Func<ZCodeFindBox, IBusinessObjectCollection> PopupCollectionDelegate;
			readonly ZCodeFindBox LinkedCodeFindBox;

			public override IBusinessObjectCollection List
			{
				get
				{
					if (PopupCollectionDelegate == null)
					{
						return base.List;
					}
					else
					{
						return PopupCollectionDelegate.Invoke(LinkedCodeFindBox);
					}
				}
			}
		}
		#endregion
	}
}
