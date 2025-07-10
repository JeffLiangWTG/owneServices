using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules.Internal
{
	public class DirectToFormModuleDecisionProvider : ModuleDecisionProvider
	{
		public DirectToFormModuleDecisionProvider(IFindBox findBox)
			: base(findBox, new DirectToFormControllerLink(findBox))
		{
		}

		public override bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}
	}

	public class PopupModuleDecisionProvider : ModuleDecisionProvider
	{
		public PopupModuleDecisionProvider(IFindBox findBox)
			: base(findBox, new PopupControllerLink(findBox))
		{
		}

		public override bool ShouldIgnoreAdditionalFilter
		{
			get { return true; }
		}

		public override bool AllowExcelExport
		{
			get { return false; }
		}

		public override bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}

		public override bool AllowMultiSelect
		{
			get { return false; }
		}
	}

	public class FilterSelectionPopupModuleDecisionProvider : PopupModuleDecisionProvider
	{
		public FilterSelectionPopupModuleDecisionProvider(IFindBox findBox)
			: base(findBox)
		{
		}

		protected override void HandleFindBoxOKButtonCore(FilterStripBusinessObject selectedFilters)
		{
			base.HandleFindBoxOKButtonCore(selectedFilters);

			var modulePopup = FindBox?.PopupForm as EmbeddedModulePopup;
			modulePopup?.HandleFilterSelection(selectedFilters);
		}

		protected override void HandleDefaultActionCore(BusinessObject[] selectedBusinessObjects)
		{
		}
	}

	public class PopupModuleDecisionProviderWithMultipleSelect : PopupModuleDecisionProvider
	{
		public PopupModuleDecisionProviderWithMultipleSelect(IFindBox findBox)
			: base(findBox)
		{
		}

		public override bool AllowMultiSelect
		{
			get { return true; }
		}
	}

	public abstract class ModuleDecisionProvider : IModuleDecisionProvider
	{
		protected ModuleDecisionProvider(IFindBox findBox, ZFindBoxControllerLink controllerLink)
		{
			if (findBox == null)
			{
				throw new ArgumentException("FindBox is null");
			}

			this.FindBox = findBox;
			this.ControllerLink = controllerLink;
		}

		protected readonly IFindBox FindBox;
		protected readonly ZFindBoxControllerLink ControllerLink;

		#region IModuleDecisionProvider Members

		public virtual bool ShouldDisplayNotifications
		{
			get { return false; }
		}

		public virtual bool EnablePreviousNextSupport
		{
			get { return true; }
		}

		public virtual bool ShouldLoadFilterBizObj
		{
			get { return false; }
		}

		public virtual bool ShouldSaveFilterBizObj
		{
			get { return false; }
		}

		public virtual bool ShouldIgnoreAdditionalFilter
		{
			get { return false; }
		}

		public virtual bool AllowExcelExport
		{
			get { return true; }
		}

		public virtual IBusinessObjectCollection List
		{
			get
			{
				if (FindBox.ListProvider == null)
				{
					throw new NullReferenceException(string.Format("FindBox.ListProvider is null, ModuleDecisionProvider type = {0}, FindBox type = {1}", this.GetType().FullName, FindBox.GetType().Name));
				}

				return FindBox.ListProvider.List;
			}
		}

		internal Type FindBoxListProviderType => FindBox.ListProvider?.GetType();

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
		{
			HandleDefaultActionCore(selectedBusinessObjects);
		}

		protected virtual void HandleDefaultActionCore(BusinessObject[] selectedBusinessObjects)
		{
			HandleFindBoxOKButton(selectedBusinessObjects);
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			selectedBusinessObjects = PreHandleSelectBusinessObjects(selectedBusinessObjects);
			if (AllowMultiSelect || selectedBusinessObjects.Length == 1)
			{
				if (selectedBusinessObjects != null &&
					FindBox?.PopupForm is EmbeddedModulePopup modulePopup &&
					modulePopup.Module.CheckValidSelectionForFindBox(selectedBusinessObjects) &&
					ValidateSelection(selectedBusinessObjects))
				{
					if (selectedBusinessObjects.Length == 1)
					{
						SetFindBoxCodeDescription(selectedBusinessObjects[0]);
					}

					modulePopup.HandleSelection(selectedBusinessObjects);
				}
			}
			else
			{
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowError(Res.GetString("902fe40b-cbc9-4d37-8384-2d85c3505cb2", "Please select one item."));
			}
		}

		public virtual BusinessObject[] PreHandleSelectBusinessObjects(BusinessObject[] selectedBusinessObjects)
		{
			return selectedBusinessObjects;
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
			HandleFindBoxOKButtonCore(selectedFilters);
		}

		protected virtual void HandleFindBoxOKButtonCore(FilterStripBusinessObject selectedFilters)
		{
		}

		public virtual void SetFindBoxCodeDescription(BusinessObject bizo)
		{
			FindBox.SetCodeDescription(bizo);
		}

		public void InitialiseFindBoxControllerLink(ZController controller)
		{
			if (ControllerLink != null && controller != null && controller.LastShownForm != null)
			{
				ControllerLink.HookController(controller);
			}
		}

		protected virtual bool ValidateSelection(IEnumerable<BusinessObject> selectedObjects)
		{
			return true;
		}

		public virtual bool AllowMultiSelect { get { return true; } }

		#endregion
	}
}
