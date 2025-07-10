using System;
using System.Collections.Generic;
using System.ComponentModel;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.RemoteDesktopServices;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Support;
using Enterprise.ZArchitecture.ModulePlugIn;
using Enterprise.ZArchitecture.PlugIn;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules
{
	public interface ZControllerInternals
	{
		IBusiness GetNewBusinessEntityInLocalFactory();
		IBusiness GetNewBusinessEntityInFactory(BusinessObjectFactory factory);
		IZForm GetForm(IBusiness businessEntity);
	}

	public abstract class ZController : ZControllerInternals, IController
	{
		public abstract ControllerID ID { get; }

		/// <summary>
		/// Get the ModuleID of this controller. NOTE: This property may return null.
		/// </summary>
		public abstract ModuleIdentifier ModuleID { get; }

		public virtual ModuleIdentifier ModuleIDForDocumentSecurity
		{
			get
			{
				return ModuleID;
			}
		}

		public virtual bool SupportsHyperlinking
		{
			get
			{
				return true;
			}
		}

		public ZModule ParentModule
		{
			get;
			set;
		}

		public abstract Type TypeOfTopLevelBusinessObject { get; }

		protected abstract IZForm GetForm(IBusiness businessEntity);

		IZForm ZControllerInternals.GetForm(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}

		protected ZController()
		{
		}

		public virtual ZGuid LastSavedPK
		{
			get
			{
				var result = ZGuid.Empty;

				var form = LastShownForm as ZForm;
				var businessObject = form == null ? null : form.DataSource as IIdentified;
				if (businessObject != null)
				{
					result = businessObject.Identifier;
				}
				else if (LastShownFormDataSourceIdentifier != ZGuid.Empty)
				{
					result = LastShownFormDataSourceIdentifier;
				}
				else
				{
					ErrorReporter.ReportOnce("ZControllerLastSavedPKIsUnknown", string.Format("LastSavedPK cannot be identified: Controller={0}, Form={1}, Form.DataSource={2}, stack trace={3}",
						GetType().FullName,
						form == null ? "null" : form.GetType().FullName,
						form == null || form.DataSource == null ? "null" : form.DataSource.GetType().FullName,
						form == null || ((IDisposeStackProvider)form).DisposeStack == null ? "null" : ((IDisposeStackProvider)form).DisposeStack.ToString()));
				}

				return result;
			}
		}

		/// <summary>
		/// Create a New BusinessEntity from your ZController's Factory.
		/// </summary>
		/// <returns>The BusinessEntity to give to your New Forms.</returns>
		protected virtual IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New(TypeOfTopLevelBusinessObject);
		}

		/// <summary>
		/// Create a New BusinessEntity from specified Factory.
		/// </summary>
		/// <returns>The BusinessEntity to give to your New Forms.</returns>
		protected virtual IBusiness GetNewBusinessEntityInFactory(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			var currentFactory = Factory;
			IBusiness result = null;
			try
			{
				fFactory = factory;
				result = GetNewBusinessEntityInLocalFactory();
			}
			finally
			{
				fFactory = currentFactory;
			}
			return result;
		}

		internal IBusiness GetNewBusinessEntityInLocalFactoryInternal()
		{
			return GetNewBusinessEntityInLocalFactory();
		}

		IBusiness ZControllerInternals.GetNewBusinessEntityInLocalFactory()
		{
			return GetNewBusinessEntityInLocalFactory();
		}

		IBusiness ZControllerInternals.GetNewBusinessEntityInFactory(BusinessObjectFactory factory)
		{
			return GetNewBusinessEntityInFactory(factory);
		}

		BusinessObject IController.GetFormBusinessObject(IBusiness sourceEntity)
		{
			return GetLoadedBusinessEntityInLocalFactory(sourceEntity) as BusinessObject;
		}

		/// <summary>
		/// Based on the SourceEntity (PK), load the BusinessEntity you want to give to your Forms from your ZController's Factory.
		/// </summary>
		/// <param name="sourceEntity">Typically, this is the BusinessObject selected in your ZFilterGrid.</param>
		/// <returns>The BusinessEntity to give to your Edit/View/Delete Forms.</returns>
		protected virtual IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			if (sourceEntity == null)
			{
				throw new ArgumentException("SourceEntity");
			}
			return LoadBusinessEntity(Factory, sourceEntity.Identifier);
		}

		/// <summary>
		/// Based on the SourceEntity (PK), load the BusinessEntity you want to give to your Forms from your ZController's Factory.
		/// </summary>
		/// <param name="sourceEntityPK">Typically, this is the PK of the BusinessObject selected in your ZFilterGrid.</param>
		/// <returns>The business entity (IBusiness) to give to your Edit/View/Delete Forms.</returns>
		protected internal virtual IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.Load(TypeOfTopLevelBusinessObject, sourceEntityPK);
		}

		/// <summary>
		/// Sets which Collection will be used for setting default values on New Forms, and providing validation on New and Edit Forms.
		/// </summary>
		/// <param name="collection">The collection to be used for defaults and validation.</param>
		public void SetCollectionForDefaultsAndValidation(IBusinessObjectCollection collection)
		{
			this.CollectionForDefaultsAndValidation = collection;
		}

		public void AddAdditionalDomainValidationGroups(ValidationDomainService validationDomainService)
		{
			if (validationDomainService == null)
			{
				throw new ArgumentNullException(nameof(validationDomainService));
			}
			Factory.Validation.AddAllFrom(validationDomainService);
		}

		/// <summary>
		/// Returns a plugin relevant to this Controller.
		/// </summary>
		/// <param name="businessEntity">This "BusinessEntity" is used to determine what specific type of PlugIn to return, if any. 
		/// By default, "BusinessEntity" is the top level business entity of the form we are plugging in to. Usually of type IBusiness.</param>
		/// <returns>A ZPlugIn, or null (based on BusinessEntity).</returns>
		protected virtual ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ErrorReporter.ReportOnce(this.GetType().Name + ".GetPlugIn()", "Please override GetPlugIn() in your ZController (" + this.GetType().Name + ")");
			return null;
		}

		/// <summary>
		/// Returns a module plugin relevant to this Controller.
		/// </summary>
		/// <param name="module">This module can be used to determine what specific type of module plugin to return, if any.</param>
		/// <returns>A ZModulePlugiIn, or null (based on the passed in module).</returns>
		protected virtual ZModulePlugin GetModulePluginCore(ZFilterGridModule module)
		{
			ErrorReporter.ReportOnce(this.GetType().Name + ".GetModulePlugin", string.Format("Please override GetModulePlugin() in your ZController ({0})", this.GetType().Name));
			return null;
		}

		/// <summary>
		/// This flag controls whether we can open Urls from other companies within the module associated with this ZController.
		/// When this flag is set to true, a hyperlink created in the module will include a reference to a specific Company. That link will only be able to be opened by users within that company.
		/// When this flag is set to false, hyperlinks created in the module lack the Company qualifier, and can be opened without considering the user's company.
		/// </summary>
		public abstract bool MakeUrlsOnlyOpenableForCurrentCompany { get; }

		public virtual bool MakeUrlOnlyOpenableForCurrentCompany(IBusiness bizo) => MakeUrlOnlyOpenableForCurrentCompany(bizo.Identifier);

		public virtual bool MakeUrlOnlyOpenableForCurrentCompany(ZGuid identifier) => MakeUrlsOnlyOpenableForCurrentCompany;

		#region Showing Forms

		/// <summary>
		/// Generic method for loading any form when given the business entity and specific display mode
		/// </summary>
		public IZForm ShowFormOfGivenDisplayType(BusinessObject businessEntity, ODisplayMode displayMode)
		{
			switch (displayMode)
			{
				case ODisplayMode.ReadOnly:
					return ShowViewForm(businessEntity);
				case ODisplayMode.NewSaved:
				case ODisplayMode.Browse:
				case ODisplayMode.Edit:
					return ShowEditForm(businessEntity);
				case ODisplayMode.Delete:
					return ShowDeleteForm(businessEntity);
				case ODisplayMode.New:
					return ShowFormForNewEntity(businessEntity);
				default: // DisplayMode == Undefined
					return ShowNewForm();
			}
		}

		/// <summary>
		/// Show the ZController's New Form.
		/// </summary>
		public virtual IZForm ShowNewForm()
		{
			return ShowFormForNewEntityCore(GetNewBusinessEntityInLocalFactory());
		}

		public IZForm ShowFormForNewEntity(IBusiness businessEntity)
		{
			return ShowFormForNewEntityCore(businessEntity);
		}

		protected virtual IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			var businessObject = businessEntity as BusinessObject;
			var checkpoint = GetCheckPointForNew(businessObject);

			if (checkpoint.IsAllowed || AllowCreateNewWithoutSecurityRight)
			{
				InitialiseDefaultsAndValidation(businessEntity);
				IZForm form;
				try
				{
					form = PrepareNewlyCreateForm(businessEntity);
				}
				catch (FormIsAlreadyInCacheException e)
				{
					SwitchToFormFor(e.ExistingForm, businessEntity);
					return LastShownForm;
				}

				if (form != null)
				{
					SetupModuleResultsBusinessObject(businessEntity, form);
					ShowForm(form, LicenceCheckpointForModifyOverride);
				}
			}
			else
			{
				throw new SecurityAccessDeniedException(checkpoint.ErrorMessageForNotAllowed);
			}
			return LastShownForm;
		}

		protected virtual bool AllowCreateNewWithoutSecurityRight
		{
			get { return false; }
		}

		/// <summary>
		/// Show the ZContoller's Edit Form.
		/// </summary>
		/// <param name="sourceEntity">Typically, this is the BusinessObject selected in your ZFilterGrid.</param>
		public virtual IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			ZModule module = null;
			try
			{
				module = ModuleID != null ? ZModuleFactory.Instance.Create(ModuleID) : null;
			}
			catch (ModuleGuiNotSupportedException) { }

			using (module)
			{
				if (GetCheckPointForEdit(sourceEntity).IsAllowed && (module?.AllowEdit ?? true))
				{
					ShowLoadedForm(sourceEntity, FormAction.Edit);
				}
				else
				{
					ShowViewForm(sourceEntity);
				}
			}
			return LastShownForm;
		}

		public virtual IZForm ShowCopyAndReverseForm(BusinessObject inMemorySourceEntity)
		{
			if (inMemorySourceEntity is ITemplateCopyable && inMemorySourceEntity is ITemplateReversible)
			{
				ShowCopyForm(inMemorySourceEntity, DoTemplateCopyAndReverse);
			}
			else
			{
				throw new ModuleTemplateCopyNotSupportedException("ShowReverseForm is only available if your business object implements ITemplateCopyable and ITemplateReversible");
			}
			return LastShownForm;
		}
		/// <summary>
		/// Show the ZContoller's Template Copy Form.
		/// </summary>
		/// <param name="inMemorySourceEntity">Typically, this is the BusinessObject selected in your ZGrid.</param>
		public virtual IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			if (inMemorySourceEntity is ITemplateCopyable)
			{
				ShowCopyForm(inMemorySourceEntity, DoTemplateCopy);
			}
			else
			{
				throw new ModuleTemplateCopyNotSupportedException("ShowTemplateCopyForm is only available if your business object implements ITemplateCopyable");
			}
			return LastShownForm;
		}

		IBusiness DoTemplateCopyAndReverse(IBusiness loadedSourceEntity)
		{
			var result = DoTemplateCopy(loadedSourceEntity);
			((ITemplateReversible)result).Reverse();
			return result;
		}

		IBusiness DoTemplateCopy(IBusiness loadedSourceEntity)
		{
			return ((ITemplateCopyable)loadedSourceEntity).TemplateCopy();
		}

		protected virtual IZForm ShowCopyForm(BusinessObject inMemorySourceEntity, CopyOfBusinessObject returnsNewBusinessEntity)
		{
			var checkpoint = GetCheckPointForCopy(inMemorySourceEntity);

			if (checkpoint.IsAllowed)
			{
				var loadedSourceEntity = GetLoadedBusinessEntityInLocalFactory(inMemorySourceEntity);
				if (loadedSourceEntity == null)
				{
					LastShownForm = null;
					ShowAlreadyDeletedOrIrreversiblyChangedMessage();
					return LastShownForm;
				}
				var copiedBusinessObject = returnsNewBusinessEntity(loadedSourceEntity);
				IZForm form;
				try
				{
					form = PrepareNewlyCreateForm(copiedBusinessObject);
					if (copiedBusinessObject.Identifier != ZGuid.Empty)
					{
						form.IdentifierForPersistingForm = copiedBusinessObject.Identifier.ToGuid();
					}
				}
				catch (FormIsAlreadyInCacheException e)
				{
					SwitchToFormFor(e.ExistingForm, copiedBusinessObject);
					return LastShownForm;
				}
				ShowForm(form, LicenceCheckpointForModifyOverride);
			}
			else
			{
				checkpoint.ShowError();
				LastShownForm = null;
			}

			return LastShownForm;
		}
		protected delegate IBusiness CopyOfBusinessObject(IBusiness inMemorySourceEntity);

		/// <summary>
		/// Show the ZContoller's View Form.
		/// </summary>
		/// <param name="sourceEntity">Typically, this is the BusinessObject selected in your ZFilterGrid.</param>
		public virtual IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			var checkpoint = GetCheckPointForView(sourceEntity);

			if (checkpoint.IsAllowed)
			{
				ShowLoadedForm(sourceEntity, FormAction.View);
			}
			else
			{
				checkpoint.ShowError();
				LastShownForm = null;
			}
			return LastShownForm;
		}

		/// <summary>
		/// Show the ZContoller's Delete Form.
		/// </summary>
		/// <param name="sourceEntity">Typically, this is the BusinessObject selected in your ZFilterGrid.</param>
		public virtual IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			var checkpoint = GetCheckPointForDelete(sourceEntity);

			if (checkpoint.IsAllowed)
			{
				if (sourceEntity.CanDelete)
				{
					ShowDeleteFormAfterSecurityAndCanDeleteCheck(sourceEntity);
				}
				else
				{
					Globals.Message.ShowError(sourceEntity.ReasonForNotAbleToDelete, DeleteFormCaption);
				}
			}
			else
			{
				checkpoint.ShowError();
				LastShownForm = null;
			}
			return LastShownForm;
		}

		protected virtual void ShowDeleteFormAfterSecurityAndCanDeleteCheck(BusinessObject sourceEntity)
		{
			ShowLoadedForm(sourceEntity, FormAction.Delete);
		}

		protected virtual string DeleteFormCaption
		{
			get { return Res.GetString("ddaa8d48-51f1-419d-b71b-16c5424b2f03", "Cannot delete the record"); }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected virtual bool DisallowMultiDeleteBecauseDeleteIsNotWhatIsReallyHappeningInAccounting
		{
			get
			{
				return false;
			}
		}

		bool CanDeleteMultiple(BusinessObject[] selectedBusinessObjects)
		{
			var canDeleteMultiple = false;
			if (!DisallowMultiDeleteBecauseDeleteIsNotWhatIsReallyHappeningInAccounting)
			{
				foreach (var selectedBusinessObject in selectedBusinessObjects)
				{
					var checkpoint = GetCheckPointForDelete(selectedBusinessObject);
					if (!checkpoint.IsAllowed)
					{
						checkpoint.ShowError();
						return false;
					}
				}
				canDeleteMultiple = true;
			}
			return canDeleteMultiple;
		}

		public void DeleteMultipleWithoutListing(BusinessObject[] selectedBusinessObjects)
		{
			if (CanDeleteMultiple(selectedBusinessObjects))
			{
				DeleteMultipleWithoutListingCore(selectedBusinessObjects);
			}
		}

		/// <summary>
		/// Deletes multiple objects in a batch.
		/// </summary>
		/// <param name="selectedBusinessObjects">The selected business objects.</param>
		public void DeleteMultiple(BusinessObject[] selectedBusinessObjects)
		{
			if (CanDeleteMultiple(selectedBusinessObjects))
			{
				DeleteMultipleCore(selectedBusinessObjects);
			}
		}

		BusinessObjectMultipleDeleterAction GetAction(BusinessObject[] selectedBusinessObjects)
		{
			var action = BusinessObjectMultipleDeleterAction.Delete;
			if (selectedBusinessObjects.Length > 0)
			{
				var cancellable = selectedBusinessObjects[0] as ICancellable;
				if (cancellable != null && PreventDeleteAttribute.IsTrue(cancellable.GetType()))
				{
					action = cancellable.IsCancelled ? BusinessObjectMultipleDeleterAction.Activate : BusinessObjectMultipleDeleterAction.Deactivate;
				}
			}
			return action;
		}

		protected virtual void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			var action = GetAction(selectedBusinessObjects);
			new BusinessObjectMultipleDeleterGUI(selectedBusinessObjects).Process(action);
		}

		protected virtual void DeleteMultipleWithoutListingCore(BusinessObject[] selectedBusinessObjects)
		{
			var action = GetAction(selectedBusinessObjects);
			new BusinessObjectMultipleDeleterGUI(selectedBusinessObjects).Process(action, false);
		}

		/// <summary>
		/// When a form is shown, it will be shown modal to ParentForm.
		/// </summary>
		public void SetFormsModalTo(Form parentForm)
		{
			fParentModalForm = parentForm;
		}

		public bool EnablePreviousNextSupport = true;

		/// <summary>
		/// The form that was last shown or switched to (if it was open already).
		/// Will be null if the form could not be shown (eg, due to security restrictions).
		/// </summary>
		public IZForm LastShownForm
		{
			get
			{
				return lastShownForm;
			}
			protected set
			{
				lastShownForm = value;
				LastShownFormDataSourceIdentifier = ZGuid.Empty;

				var form = value as ZForm;
				var dataSource = form != null ? form.DataSource as IIdentified : null;
				if (dataSource != null)
				{
					LastShownFormDataSourceIdentifier = dataSource.Identifier;
				}
			}
		}

		IZForm lastShownForm;

		ZGuid LastShownFormDataSourceIdentifier;

		public IDisposable SetArgsForNewForm(IEnumerable<string> args)
		{
			ArgsForNewForm = args;

			return new DisposableAction(() => ArgsForNewForm = null);
		}

		protected IEnumerable<string> ArgsForNewForm { get; private set; }

		#endregion

		#region Factory

		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = GetNewFactory();
					SetStrategyProvider(Factory);
				}
				return fFactory;
			}
		}

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory() { NameForDebugging = "ZController_GetNewFactory" };
		}

		#endregion

		#region SetStrategyProvider

		protected virtual void SetStrategyProvider(BusinessObjectFactory factory)
		{
		}

		#endregion

		#region Security CheckPoints

		protected abstract SecurityCheckpoint CheckPointForView { get; }
		protected abstract SecurityCheckpoint CheckPointForNew { get; }
		protected abstract SecurityCheckpoint CheckPointForEdit { get; }
		protected abstract SecurityCheckpoint CheckPointForDelete { get; }

		#region TestStuff
#if DEBUG

		public SecurityCheckpoint CheckPointForViewExposedForTest => CheckPointForView;
		public SecurityCheckpoint CheckPointForNewExposedForTest => CheckPointForNew;
		public SecurityCheckpoint CheckPointForEditExposedForTest => CheckPointForEdit;
		public SecurityCheckpoint CheckPointForDeleteExposedForTest => CheckPointForDelete;
		public SecurityCheckpoint CheckPointForCopyExposedForTest => CheckPointForCopy;
		public SecurityCheckpoint GetCheckPointForCopyForTest(BusinessObject inMemorySourceEntity) => GetCheckPointForCopy(inMemorySourceEntity);

#endif
		#endregion

		public virtual SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return ZFilterGridModule.IsTemplateRecord(bizObject) ? TemplateRecordCheckpoint : CheckPointForView;
		}

		public virtual SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject)
		{
			return ZFilterGridModule.IsTemplateRecord(bizObject) ? TemplateRecordAddCheckpoint : CheckPointForNew;
		}

		protected virtual SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity)
		{
			var checkPointForNew = GetCheckPointForNew(inMemorySourceEntity);
			if (checkPointForNew.IsAllowed && CheckPointForCopy != null)
			{
				return CheckPointForCopy;
			}
			return checkPointForNew;
		}

		SecurityCheckpoint CheckPointForCopy
		{
			get
			{
				if (checkPointForCopy == null)
				{
					if (ParentModule is IZFilterGridModule parentFilterGridModule && parentFilterGridModule.AllowAddCopyMenuItem &&
						ParentModule.SecurityCheckpoint != null && !(ParentModule.SecurityCheckpoint is NoneSecurityCheckpoint))
					{
						checkPointForCopy = (SecurityCheckpoint)EnvProxy.Instance.Security.FindOrCreateCopyCheckpoint(ParentModule.SecurityCheckpoint);
					}
				}
				return checkPointForCopy;
			}
		}
		SecurityCheckpoint checkPointForCopy;

		public virtual SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return ZFilterGridModule.IsTemplateRecord(bizObject) ? TemplateRecordEditCheckpoint : CheckPointForEdit;
		}

		public virtual SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return ZFilterGridModule.IsTemplateRecord(bizObject) ? TemplateRecordDeleteCheckpoint : CheckPointForDelete;
		}

		#region TemplateRecords

		internal SecurityCheckpoint TemplateRecordCheckpoint => TemplateRecordCheckpointCore ?? CheckPointForView;

		SecurityCheckpoint TemplateRecordCheckpointCore
		{
			get
			{
				if (templateRecordCheckpointCore == null)
				{
					var parentFilterGridModule = ParentModule as IZFilterGridModule;
					if (parentFilterGridModule != null && parentFilterGridModule.AllowTemplateRecords && ParentModule.SecurityCheckpoint != null && !(ParentModule.SecurityCheckpoint is NoneSecurityCheckpoint))
					{
						templateRecordCheckpointCore = (SecurityCheckpoint)EnvProxy.Instance.Security.FindOrCreateTemplateRecordCheckpoint(ParentModule.SecurityCheckpoint);
					}
				}
				return templateRecordCheckpointCore;
			}
		}
		SecurityCheckpoint templateRecordCheckpointCore;

		internal SecurityCheckpoint TemplateRecordAddCheckpoint
		{
			get
			{
				if (templateRecordAddCheckpoint == null && TemplateRecordCheckpointCore != null)
				{
					templateRecordAddCheckpoint = (SecurityCheckpoint)EnvProxy.Instance.Security.FindOrCreateTemplateRecordAddCheckpoint(TemplateRecordCheckpointCore);
				}
				return templateRecordAddCheckpoint ?? CheckPointForNew;
			}
		}
		SecurityCheckpoint templateRecordAddCheckpoint;

		internal SecurityCheckpoint TemplateRecordEditCheckpoint
		{
			get
			{
				if (templateRecordEditCheckpoint == null && TemplateRecordCheckpointCore != null)
				{
					templateRecordEditCheckpoint = (SecurityCheckpoint)EnvProxy.Instance.Security.FindOrCreateTemplateRecordEditCheckpoint(TemplateRecordCheckpointCore);
				}
				return templateRecordEditCheckpoint ?? CheckPointForEdit;
			}
		}
		SecurityCheckpoint templateRecordEditCheckpoint;

		internal SecurityCheckpoint TemplateRecordDeleteCheckpoint
		{
			get
			{
				if (templateRecordDeleteCheckpoint == null && TemplateRecordCheckpointCore != null)
				{
					templateRecordDeleteCheckpoint = (SecurityCheckpoint)EnvProxy.Instance.Security.FindOrCreateTemplateRecordDeleteCheckpoint(TemplateRecordCheckpointCore);
				}
				return templateRecordDeleteCheckpoint ?? CheckPointForDelete;
			}
		}
		SecurityCheckpoint templateRecordDeleteCheckpoint;

		#endregion

		#endregion

		#region LicenceCheckPointOverride

		public ILicenceCheckpoint LicenceCheckpointForViewOverride { get; set; }
		public ILicenceCheckpoint LicenceCheckpointForModifyOverride { get; set; }

		public void SetLicenceCheckpointsOverrides(ILicenceCheckpoint checkpoint)
		{
			LicenceCheckpointForViewOverride = checkpoint;
			LicenceCheckpointForModifyOverride = checkpoint;
		}

		#endregion

		#region Implementation

		public ZString InitialTabPageNameToSelectWhenAFormIsShown { get; internal set; }

		protected void SetInitialTabPageNameToSelectWhenAFormIsShown(string tabPageName)
		{
			InitialTabPageNameToSelectWhenAFormIsShown = tabPageName;
		}

		// TODO: Update ZWinform and deprecate this
		protected ODisplayMode GetODisplayMode(FormAction action)
		{
			var result = ODisplayMode.New;

			switch (action)
			{
				case FormAction.Edit:
					result = ODisplayMode.Browse;
					break;

				case FormAction.View:
					result = ODisplayMode.ReadOnly;
					break;

				case FormAction.Delete:
					result = ODisplayMode.Delete;
					break;
			}

			return result;
		}

		protected void ShowForm(IZForm formToShow, ILicenceCheckpoint licenceCheckpointOverride = null)
		{
			if (NativeMethods.GetWindowHandlesForCurrentProcess() > NativeMethods.GuiResourcesThreshold)
			{
				formToShow.Dispose();
				Globals.Message.Show(Res.GetString("357c7782-f3f0-45c4-9916-5a1b36b1a38f", "There are too many windows and/or graphical elements open by the application. Please close some unused windows and repeat this operation again."));
				return;
			}

			using (PerformanceStatisticsCollector.StartMonitoring("ZController.ShowForm", GetType().FullName))
			{
				if (!LicenceCheckpointLogin(formToShow, licenceCheckpointOverride))
				{
					return;
				}

				using (new ZWaitCursorChanger())
				{
					SetControllerID(formToShow, ID);

					if (formToShow is ZForm)
					{
						var zForm = (ZForm)formToShow;
						if (!EnablePreviousNextSupport)
						{
							zForm.AutoAddPreviousNextButtons = false;
						}
						zForm.InitialTabPageNameToSelectOnLoaded = InitialTabPageNameToSelectWhenAFormIsShown;
						zForm.LoadPersistedFormArgs(ArgsForNewForm);
					}

					LastShownForm = formToShow;

					if (ShowChildrenAsDialog)
					{
						ZFormModaliser.ShowDialogAndDispose((Form)formToShow);
					}
					else if (fParentModalForm == null || (formToShow is IFormInteractionMode shouldBeModal && !shouldBeModal.IsModal()))
					{
						ShowModelessForm(formToShow);
					}
					else
					{
						ZFormModaliser.Show((Form)formToShow, fParentModalForm);
					}
				}
			}
		}

		public bool ShowChildrenAsDialog { get; set; }

		protected virtual void SetControllerID(IZForm form, ControllerID proposedControllerID)
		{
			form.ControllerID = proposedControllerID;
		}

		protected virtual void ShowModelessForm(IZForm form)
		{
			ShowModelessFormCore(form);
		}

		public static void ShowModelessFormCore(IZForm form)
		{
#if !WINZOR
			int delayMilliseconds;
			if (MenuClickPendingTracker.Count > 0
				&& ObjectFactory.Get<TerminalService>().IsRemoteAppSession
				&& 0 < (delayMilliseconds = DataRegistry.Instance.RemoteAppShowFormViaMenuDelayMilliseconds)
				&& form is ZForm zform)
			{
				zform.ShowWithoutActivationOverride = true;

				// Introduce activation delay to workaround RemoteApp bug (WI00033116).
				// Using a Windows.Forms timer so the tick will be handled on the current, UI thread.
				// Not using Thread.Sleep since it would block the UI thread, causing visible delay
				// in the redraw that removes the popup menu, for example.
				var timer = new Timer();
				timer.Interval = delayMilliseconds;
				timer.Tick += ActivateAfterTimerDelay;
				timer.Tag = form;
				timer.Start();

				ZForm.QuietlyShowForm(form);
			}
			else
#endif
			if (form != null)
			{
				ZForm.QuietlyShowForm(form);
				var winForm = (Form)form;
				if (!winForm.IsDisposed && winForm.IsHandleCreated)
				{
					winForm.BeginInvoke(new MethodInvoker(() => ActivateForm(winForm)));
				}
			}
		}

		protected static void ActivateAfterTimerDelay(object sender, EventArgs eventArgs)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				ActivateAfterTimerDelayClock = ZDateTime.UtcNow;
				ActivateAfterTimerDelayWasCalled = true;
			}
#endif

			var timer = (Timer)sender;
			if (timer.Tag != null)
			{
				timer.Stop();
				var form = (ZForm)timer.Tag;
				timer.Tag = null;
				timer.Dispose();
				if (!form.IsDisposed && form.IsHandleCreated)
				{
					form.BeginInvoke(new MethodInvoker(() => ActivateForm(form)));
				}
			}
		}
#if DEBUG
		public static ZDateTime ActivateAfterTimerDelayClock;
		public static bool ActivateAfterTimerDelayWasCalled;
#endif

		static void ActivateForm(Form form)
		{
			if (form.Visible && form.CanFocus)
			{
				form.Activate();
			}
		}

		protected void SetupModuleResultsBusinessObject(IBusiness sourceEntity, IZForm form)
		{
			if (ModuleID != null)
			{
				var bizO = new ModuleResultsBusinessObject(ModuleResultsPKCollection);
				bizO.CurrentPK = GetCurrentPK(sourceEntity);

				if (form != null)
				{
					form.ModuleResultsBusinessObject = bizO;
				}
			}
		}

		protected virtual ZGuid GetCurrentPK(IBusiness sourceEntity)
		{
			return sourceEntity.Identifier;
		}

		internal protected ZPKCollection ModuleResultsPKCollection
		{
			get { return fModuleResultsPKCollection ?? ZModuleResults.Instance.GetPKCollectionForModule(ModuleID); }
			set { fModuleResultsPKCollection = value; }
		}

		ZPKCollection fModuleResultsPKCollection;

		protected virtual IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("ZController.ShowLoadedForm", GetType().FullName))
			{
				IBusiness businessEntity;
				var cancelled = false;
				var isCancelled = false;
				FormAction forcedAction = action;

				using (PerformanceStatisticsCollector.StartMonitoring("ZController.GetLoadedBusinessEntityInLocalFactory", GetType().FullName))
				{
					businessEntity = GetLoadedBusinessEntityInLocalFactory(sourceEntity);
					forcedAction = SwitchFormAction(businessEntity, action);

					var cancellableEntity = CancellableHelper.GetICancellable(businessEntity);
					if (forcedAction == FormAction.Delete && cancellableEntity != null && PreventDeleteAttribute.IsTrue(businessEntity.GetType()))
					{
						cancellableEntity.IsCancelled = !cancellableEntity.IsCancelled;
						isCancelled = cancellableEntity.IsCancelled;
						cancelled = true;
					}
				}

				if (businessEntity == null)
				{
					LastShownForm = null;
					ShowAlreadyDeletedOrIrreversiblyChangedMessage();
				}
				else if (IsFormShownFor(businessEntity))
				{
					SwitchToFormFor(businessEntity);
					if (LastShownForm != null && forcedAction == FormAction.Delete)
					{
						SwitchToDeleteForm(cancelled, isCancelled);
					}
				}
				else
				{
					IZForm form;
					using (PerformanceStatisticsCollector.StartMonitoring("ZController.PrepareNewlyCreateForm", GetType().FullName))
					{
						try
						{
							form = PrepareNewlyCreateForm(businessEntity);
							if (form != null && sourceEntity.Identifier != ZGuid.Empty)
							{
								if (!(sourceEntity is NonPersistentBusinessObject) && TypeOfTopLevelBusinessObject.IsAssignableFrom(sourceEntity.GetType()))
								{
									form.IdentifierForPersistingForm = sourceEntity.Identifier.ToGuid();
								}
								if (sourceEntity is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord && templateRecordProvider.TemplateRecord is IBusiness templateRecord)
								{
									form.IdentifierForPersistingForm = templateRecord.Identifier.ToGuid();
								}
							}
						}
						catch (FormIsAlreadyInCacheException e)
						{
							SwitchToFormFor(e.ExistingForm, businessEntity);
							return LastShownForm;
						}
					}

					if (form != null)
					{
						form.DisplayMode = GetODisplayMode(forcedAction); // TODO: use Action rather than DisplayMode

						// need to update status of Save button depending on HasChanges of BO again
						// coz - it gets overridden in PrepareNewlyCreateForm(BusinessEntity) call
						if (form is ZForm zform)
						{
							ZFormPostingButtonsStrategy.UpdateSaveButtonsBasedOnHasChanges(zform);
						}

						if (forcedAction == FormAction.Edit)
						{
							InitialiseValidation(businessEntity);
						}
						SetupModuleResultsBusinessObject(sourceEntity, form);
						ShowForm(form, GetLicenceCheckpointOverride(forcedAction));
						if (cancelled)
						{
							OnBusinessObjectIsCancelledChanged(form, isCancelled);
						}
					}
				}
				return LastShownForm;
			}
		}

		protected virtual FormAction SwitchFormAction(IBusiness entity, FormAction originalAction)
		{
			return originalAction;
		}

		void SwitchToDeleteForm(bool deleteOperationCancelled, bool isBusinessObjectCancelled)
		{
			var zFormLastShownForm = LastShownForm as ZForm;
			if (zFormLastShownForm != null)
			{
				var formDataSourceBO = zFormLastShownForm.DataSource as BusinessObject;
				if (LastShownForm.DisplayMode == ODisplayMode.ReadOnly)
				{
					if (!LicenceCheckpointLogin(LastShownForm, GetLicenceCheckpointOverride(FormAction.Delete), true))
					{
						return;
					}
				}
				else if (formDataSourceBO != null)
				{
					formDataSourceBO.SetReadOnlyIncludingChildren(true);
					var cancellableEntity = CancellableHelper.GetICancellable(formDataSourceBO);
					if (cancellableEntity != null && deleteOperationCancelled)
					{
						cancellableEntity.IsCancelled = isBusinessObjectCancelled;
					}
				}

				LastShownForm.DisplayMode = ODisplayMode.Delete;

				if (deleteOperationCancelled)
				{
					OnBusinessObjectIsCancelledChanged(LastShownForm, isBusinessObjectCancelled);
				}

				zFormLastShownForm.RefreshCaption();
				zFormLastShownForm.IsEditToDelete = true;
				zFormLastShownForm.ShowOtherUsersCurrentlyAccessingThisEntity();
			}
		}

		bool LicenceCheckpointLogin(IZForm formToShow, ILicenceCheckpoint licenceCheckpointOverride = null, bool isViewToDelete = false)
		{
			if (ModuleID != null)
			{
				using (var module = ZModuleFactory.Instance.Create(ModuleID))
				{
					var licenceCheckpoint = licenceCheckpointOverride ?? module.LicenceCheckPoint;
					if (licenceCheckpoint.Login(formToShow) == LicenceLoginResponse.Denied)
					{
						if (!isViewToDelete)
						{
							formToShow.Dispose();
						}
						licenceCheckpoint.ShowLastError();
						return false;
					}
				}
			}
			return true;
		}

		void OnBusinessObjectIsCancelledChanged(IZForm formToShow, bool isCancelled)
		{
			var zFormToShow = formToShow as ZForm;
			var formToShowBO = zFormToShow?.DataSource as BusinessObject;
			var formToShowICancellable = formToShowBO as ICancellable;

			if (formToShowICancellable != null)
			{
				formToShowICancellable.IsCancelled = isCancelled;
			}

			if (formToShowBO != null)
			{
				formToShowBO.ReadOnly = isCancelled;
				if (!isCancelled)
				{
					formToShowBO.MarkAsNeedingValidation();
				}
			}

			if (zFormToShow != null && zFormToShow.PlugIns != null)
			{
				zFormToShow.PlugIns.OnBusinessObjectIsCancelledChanged(isCancelled);
			}
		}

		ILicenceCheckpoint GetLicenceCheckpointOverride(FormAction action)
		{
			switch (action)
			{
				case FormAction.View:
					return LicenceCheckpointForViewOverride;

				default:
					return LicenceCheckpointForModifyOverride;
			}
		}

		public string AlreadyDeletedOrIrreversiblyChangedMessage
		{
			get
			{
				return AlreadyDeletedOrIrreversiblyChangedMessageCore;
			}
		}

		protected virtual string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("0184f3b3-c027-4be6-b295-8d043d15026f", "The selected record has been deleted by another user. It cannot be displayed.");
			}
		}

		protected void ShowAlreadyDeletedOrIrreversiblyChangedMessage()
		{
			Globals.Message.ShowError(AlreadyDeletedOrIrreversiblyChangedMessage);
		}

		protected IZForm PrepareNewlyCreateForm(IBusiness businessEntity)
		{
			var form = GetForm(businessEntity);

			if (form != null)
			{
				form.DisplayMode = GetDisplayModeForNew();

				try
				{
					OpenedFormCache.Add(GetIDForBusinessEntity(businessEntity), (Form)form, GetIDForFormCache(businessEntity));
				}
				// Prevent User double click 
				catch (ArgumentException e)
				{
					var existingForm = OpenedFormCache.GetForm(GetIDForBusinessEntity(businessEntity), GetIDForFormCache(businessEntity)) as IZForm;
					if (form != existingForm)
					{
						form.Dispose();
					}
					if (existingForm != null)
					{
						throw new FormIsAlreadyInCacheException("This form's cache is existed", e, existingForm);
					}
					else
					{
						throw;
					}
				}
			}

			return form;
		}

		protected virtual string GetIDForFormCache(IBusiness businessEntity)
		{
			return ID.ToString();
		}

		protected virtual ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.New;
		}

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity, SecurityCheckpoint overrideSecurityCheckpoint)
		{
			var businessObject = businessEntity as BusinessObject;

			var checkPointForEdit = GetCheckPointForEdit(businessObject)
				?? throw new InvalidOperationException("Controller: " + GetType().ToString() + " could not get a valid CheckPointForEdit for the BusinessObject: " + businessObject.GetType().ToString() + " - PK: " + businessObject.PK.ToString());
			var mostRightsCheckPoint = checkPointForEdit.IsAllowed ? checkPointForEdit : GetCheckPointForView(businessObject);
			var checkPoint = overrideSecurityCheckpoint ?? mostRightsCheckPoint;
			var result = GetPlugIn(businessEntity);

			if (result != null)
			{
				if (checkPoint != null)
				{
					result.SetSecurityCheckpoint(checkPoint, checkPointForEdit);
				}
				result.Controller = this;
			}

			return result;
		}

		public ZModulePlugin GetModulePlugin(ZFilterGridModule module)
		{
			return GetModulePluginCore(module);
		}

		public virtual ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.ZArchitecture.GUI.Res.GetData("ZController.PluginTabPageCaption", "You need to override ZController's property '{0}'").Format("PluginTabPageCaption"); }
		}

		public Form ParentModalForm
		{
			get { return fParentModalForm; }
		}

		protected IBusinessObjectCollection CollectionForDefaultsAndValidation;
		BusinessObjectFactory fFactory;
		Form fParentModalForm;

		OpenedFormCache OpenedFormCache
		{
			get { return OpenedFormCache.GetInstance(); }
		}

		protected virtual Guid GetIDForBusinessEntity(IBusiness entity)
		{
			return entity.Identifier.ToGuid();
		}

		public virtual bool IsFormShownFor(IBusiness entity)
		{
			return OpenedFormCache.Contains(GetIDForBusinessEntity(entity), GetIDForFormCache(entity));
		}

#if WINZOR

		public void RemoveOpenedForm(IBusiness entity)
		{
			OpenedFormCache.Remove(GetIDForBusinessEntity(entity), GetIDForFormCache(entity));
		}

#endif

		public Form GetOpenedForm(IBusiness entity)
		{
			var entityID = GetIDForBusinessEntity(entity);
			var controllerID = GetIDForFormCache(entity);
			return OpenedFormCache.GetForm(entityID, controllerID);
		}

		public virtual void SwitchToFormFor(IBusiness entity)
		{
			var entityID = GetIDForBusinessEntity(entity);
			var controllerID = GetIDForFormCache(entity);
			SwitchToCachedForm((IZForm)OpenedFormCache.GetForm(entityID, controllerID), entityID, controllerID);
		}

		void SwitchToFormFor(IZForm iZForm, IBusiness entity)
		{
			SwitchToCachedForm(iZForm, GetIDForBusinessEntity(entity), GetIDForFormCache(entity));
		}

		void SwitchToCachedForm(IZForm iZForm, Guid entityID, string controllerID)
		{
			LastShownForm = iZForm;
			OpenedFormCache.SwitchToCachedForm(entityID, controllerID);
		}

		void InitialiseDefaultsAndValidation(IBusiness businessEntity)
		{
			if (CollectionForDefaultsAndValidation != null && businessEntity is BusinessObject)
			{
				CollectionForDefaultsAndValidation.SetupNewElementButDoNotAddIt((BusinessObject)businessEntity, true);

				if (CollectionForDefaultsAndValidation is IRelationshipAdderForController)
				{
					((IRelationshipAdderForController)CollectionForDefaultsAndValidation).AddRelationshipToNewObject((BusinessObject)businessEntity);
				}
			}

			InitialiseValidation(businessEntity);
		}

		void InitialiseValidation(IBusiness businessEntity)
		{
			if (businessEntity is BusinessObject && CollectionForDefaultsAndValidation is IValidateForController)
			{
				((BusinessObject)businessEntity).SetValidationFromController((IValidateForController)CollectionForDefaultsAndValidation);
			}
		}

		#endregion
	}

	#region FormIsAlreadyInCacheException

	[Serializable]
	public class FormIsAlreadyInCacheException : Exception
	{
		readonly IZForm existingForm;

#if NETFRAMEWORK
		public FormIsAlreadyInCacheException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public FormIsAlreadyInCacheException(string message, Exception inner, IZForm form)
			: base(message, inner)
		{
			existingForm = form;
		}

		public IZForm ExistingForm { get { return existingForm; } }
	}

#endregion
}
