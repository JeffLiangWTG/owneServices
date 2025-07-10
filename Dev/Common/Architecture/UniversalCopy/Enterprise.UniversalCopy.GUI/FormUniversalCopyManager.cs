using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	public class FormUniversalCopyManager : UniversalCopyManager, IFormUniversalCopyManager
	{
		public FormUniversalCopyManager(ZForm form)
			: base(GetElementType(form), GetModuleId(form))
		{
			this.form = form;
			this.form.Disposed += FormDisposed;
		}

		readonly ZForm form;

		static Type GetElementType(ZForm form)
		{
			return form.BusinessEntity != null ? form.BusinessEntity.GetType() : form.DataSourceType;
		}

		static ModuleIdentifier GetModuleId(ZForm form)
		{
			if (form.ControllerID == null)
			{
				return null;
			}

			var controller = ZControllerFactory.Create(form.ControllerID);
			return controller != null ? controller.ModuleID : null;
		}

		public override bool AllowsUniversalCopy
		{
			get
			{
				var result = base.AllowsUniversalCopy;

				if (result && LocalModule != null)
				{
					result = LocalModule.AllowNew && LocalModule.AllowUniversalCopy;
				}

				return result;
			}
		}

		#region Menus

		#region Populate menu

		public void AddMenuItems()
		{
			if (AllowsUniversalCopy)
			{
				var menuItemsProvider = form as IFileMenuItemsProvider;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				if (actionsMenuItem != null)
				{
					var universalCopyMenuItem = new ZMenuItem(ResString.GetMultilingualString("d525592d-2ca0-46fe-98b0-64612cfb0791", "Universal Copy"))
					{
						Name = "UniversalCopy"
					};
					actionsMenuItem.MenuItems.Add(universalCopyMenuItem);
					AddMenuItems(universalCopyMenuItem, includeEditMenuItems: true, includeCopySchedulesItem: true);
				}
			}
		}

		protected override bool ShouldIncludeFilteredCopyMenuItems
		{
			get { return false; }
		}

		#endregion

		#region Copy

		protected override bool CopyMenuClicked_TryGetCopyTargets(out IEnumerable<BusinessObject> copyTargets)
		{
			if (form.BusinessEntity != null)
			{
				copyTargets = new[] { form.BusinessEntity as BusinessObject };
				return true;
			}

			copyTargets = null;
			return false;
		}

		protected override bool CopyMenuClicked_CheckCanCopy()
		{
			if (GetNewController() == null)
			{
				Globals.Message.ShowError(Res.GetString("b3298073-7d0d-4676-91f4-49b601e73e6b", "Cannot copy elements because system cannot determine type of module for edit form to use for copied elements."));
				return false;
			}

			return true;
		}

		protected override BusinessObject CopyMenuClicked_GetSourceElement(BusinessObject selectedElement)
		{
			var controller = GetNewController();
			var instanceTypeAttribute = selectedElement.GetType().GetCustomAttribute<UniversalCopyInstanceTypeAttribute>();
			var elementType = instanceTypeAttribute?.InstanceType ?? controller.TypeOfTopLevelBusinessObject;
			var newFactory = controller.Factory;

			if (selectedElement is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord)
			{
				return GetInstantiateFromTemplateRecord(templateRecordProvider, elementType, newFactory);
			}

			var getSourceMethod = instanceTypeAttribute?.GetSourceMethod;
			if (!string.IsNullOrEmpty(getSourceMethod))
			{
				return selectedElement.GetType().InvokeMember(getSourceMethod,
					BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
					selectedElement, null, CultureInfo.InvariantCulture) as BusinessObject;
			}

			if (selectedElement is NonPersistentBusinessObject)
			{
				return newFactory.Load(elementType, selectedElement.PK);
			}

			return newFactory.ImportFromAnotherFactory(selectedElement, elementType);
		}

		protected override void CopyMenuClicked_OnNewElement(BusinessObject newElement)
		{
			if (newElement == null)
			{
				ShowElementWasNotCopiedInformation();
			}
			else
			{
				var formShowingArgs = new FormShowingForElementArgs(newElement);
				OnFormShowingForNewElement(formShowingArgs);
				if (!formShowingArgs.Cancelled)
				{
					GetNewController().ShowFormForNewEntity(newElement);
				}
			}
		}

		ZController GetNewController()
		{
			return form.ControllerID != null ? ZControllerFactory.Create(form.ControllerID) : null;
		}

		#endregion

		#region Schedule

		protected override bool SchedulesMenuSelect_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			var bizObj = form.BusinessEntity as BusinessObject;
			if (bizObj != null)
			{
				scheduleTarget = bizObj;
				return true;
			}

			scheduleTarget = null;
			return false;
		}

		protected override bool CreateScheduleMenuClicked_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			var bizObj = form.BusinessEntity as BusinessObject;
			if (bizObj != null)
			{
				if (!bizObj.IsInDatabase)
				{
					Globals.Message.ShowInformation(Res.GetString("72165d5d-14ed-45e6-8c8d-5688f3e2a7f2", "Please save the form before creating a copy schedule"));
				}
				else
				{
					scheduleTarget = bizObj;
					return true;
				}
			}

			scheduleTarget = null;
			return false;
		}

		#endregion

		#endregion

		#region Disposing

		void FormDisposed(object sender, EventArgs e)
		{
			Dispose();
			form.Disposed -= FormDisposed;
		}

		#endregion
	}
}
