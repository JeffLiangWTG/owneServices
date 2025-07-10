using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.RemoteDesktopServices;
using Enterprise.Security;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class ZControllerTest : TestCaseWithDummy
	{
		#region LicenceCheckPointOverrides

		public void TestLicenceCheckpointForViewOverride()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var controller = new DummyController();
			controller.LicenceCheckpointForViewOverride = EnvProxy.Instance.Licence.DocManager;
			Factory.Save();

			using (var form = (ZForm)controller.ShowViewForm(dummy))
			{
				Assert(form.LicensedComponentManager.ContainsCheckpoint(EnvProxy.Instance.Licence.DocManager));
			}
		}

		public void TestLicenceCheckpointForModifyOverride_ShowNewForm()
		{
			var controller = new DummyController();
			controller.LicenceCheckpointForModifyOverride = EnvProxy.Instance.Licence.DocManager;

			using (var form = (ZForm)controller.ShowNewForm())
			{
				Assert(form.LicensedComponentManager.ContainsCheckpoint(EnvProxy.Instance.Licence.DocManager));
			}
		}

		public void TestLicenceCheckpointForModifyOverride_ShowEditForm()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var controller = new DummyController();
			controller.LicenceCheckpointForModifyOverride = EnvProxy.Instance.Licence.DocManager;
			Factory.Save();

			using (var form = (ZForm)controller.ShowEditForm(dummy))
			{
				Assert(form.LicensedComponentManager.ContainsCheckpoint(EnvProxy.Instance.Licence.DocManager));
			}
		}

		[ExpectNoExceptions]
		public void TestShowFormForNewEntityCore_Issue00916704()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Code = "BIG";
			var controller = new DummyController();
			var form = controller.ShowFormForNewEntity(dummyBO);
			var newForm = controller.ShowFormForNewEntity(dummyBO);

			AssertEquals(form, newForm);
			newForm.Dispose();
		}

		public void TestShowForm_WithNullForm()
		{
			var bizO = Factory.New<DummyBusinessObject>();

			var controller = new Mock<DummyControllerWithCustomisableSecurity>();
			controller.Setup(c => c.GetFormCore(bizO)).Returns((IZForm)null);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("GIVEN edit permission WHEN execute ShowForm with null form SHOULD not throw exception.", () => controller.Object.ShowEditForm(bizO));

				controller.Object.SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(false));
				AssertNoExceptionThrown("GIVEN no edit permission WHEN execute ShowForm with null form SHOULD not throw exception.", () => controller.Object.ShowEditForm(bizO));
			});
		}

		public void TestLicenceCheckpointForModifyOverride_ShowDeleteForm()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var controller = new DummyController();
			controller.LicenceCheckpointForModifyOverride = EnvProxy.Instance.Licence.DocManager;
			Factory.Save();

			using (var form = (ZForm)controller.ShowDeleteForm(dummy))
			{
				Assert(form.LicensedComponentManager.ContainsCheckpoint(EnvProxy.Instance.Licence.DocManager));
			}
		}

		#endregion

		#region TestMultipleDelete

		public void TestDeleteMultiple()
		{
			var controller = new MockController();

			var dummyB1 = Factory.New<DummyBusinessObject>();
			dummyB1.Z0_Code = "BIG";
			var dummyB2 = Factory.New<DummyBusinessObject>();
			dummyB2.Z0_Code = "SML";

			var selectedBusinessObjects = new BusinessObject[] { dummyB1, dummyB2 };
			UnitTestUserNotification.Instance.AddOKAnswer();
			controller.DeleteMultiple(selectedBusinessObjects);

			Assert(controller.BatchDeleteWasCalled);
			Assert(controller.SelectedBusinessObjects == selectedBusinessObjects);
		}

		public void TestDeleteMultipleWithoutSecurity()
		{
			var controller = new MockController();

			var dummyB1 = Factory.New<DummyBusinessObject>();
			dummyB1.Z0_Code = "BIG";
			var dummyB2 = Factory.New<DummyBusinessObject>();
			dummyB2.Z0_Code = "DEL";

			var selectedBusinessObjects = new BusinessObject[] { dummyB1, dummyB2 };
			UnitTestUserNotification.Instance.AddOKAnswer();
			controller.DeleteMultiple(selectedBusinessObjects);

			Assert(!controller.BatchDeleteWasCalled);
			AssertEquals("MockSecurityCheckpoint", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class MockController : DummyController
		{
			public bool BatchDeleteWasCalled;
			public BusinessObject[] SelectedBusinessObjects;

			protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
			{
				BatchDeleteWasCalled = true;
				SelectedBusinessObjects = selectedBusinessObjects;
			}

			public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject sourceEntity)
			{
				return ((DummyBusinessObject)sourceEntity).Z0_Code != "DEL"
					? base.GetCheckPointForDelete(sourceEntity)
					: new MockSecurityCheckpoint("MockSecurityCheckpoint");
			}

			protected override void ShowModelessForm(IZForm form)
			{
				ShowModelessFormClock = ZDateTime.UtcNow;
				base.ShowModelessForm(form);
			}
			public ZDateTime ShowModelessFormClock;
		}

		class MockSecurityCheckpoint : SecurityCheckpoint
		{
			public MockSecurityCheckpoint(string displayText) : base("", (NoResString)displayText, null, null, false) { }

			public override void ShowError()
			{
				Globals.Message.Show(DisplayText);
			}

			public override bool IsAllowed
			{
				get { return false; }
				set { base.IsAllowed = value; }
			}
		}

		#endregion

		public void TestShowFormOfGivenDisplayType()
		{
			var modes = Enum.GetValues(typeof(ODisplayMode));

			foreach (ODisplayMode mode in modes)
			{
				var dummyEntity = Factory.New<DummyBusinessObject>();
				var controller = new DummyController();

				Factory.Save();

				using (var form = controller.ShowFormOfGivenDisplayType(dummyEntity, mode))
				{
					AssertReloadResultsInCorrectDisplayMode(mode, form.DisplayMode);
				}
			}
		}

		/// <summary>
		/// Due to the function of ShowFormOfGivenDisplay, when we reload some DisplayModes, they do not result in the same DisplayModes after reload. These specific cases are recored below for testing ease.
		/// </summary>
		public static void AssertReloadResultsInCorrectDisplayMode(ODisplayMode originalMode, ODisplayMode preReloadMode)
		{
			if (originalMode == ODisplayMode.Browse || originalMode == ODisplayMode.Edit || originalMode == ODisplayMode.NewSaved)
			{
				AssertEquals("Our resulting form, that was being edited, should have the Browse display type, and yet...", ODisplayMode.Browse, preReloadMode);
			}
			else if (originalMode == ODisplayMode.New || originalMode == ODisplayMode.Undefined)
			{
				AssertEquals("Our resulting form, if originally a New or Undefined form, should end up with the New display type, but instead...", ODisplayMode.New, preReloadMode);
			}
			else
			{
				AssertEquals("Our resulting form should have a display type of " + originalMode.ToString() + " as requested, and yet...", originalMode, preReloadMode);
			}
		}

		[ExpectNoExceptions]
		public void TestShowChildrenAsDialog()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Code = "BIG";
			Factory.Save();
			var controller = new DummyController();
			controller.ShowChildrenAsDialog = true;

			var form = (ZForm)controller.ShowFormForNewEntity(dummyBO);
			AssertEquals(true, form.IsDisposed);

			form = (ZForm)controller.ShowDeleteForm(dummyBO);
			AssertEquals(true, form.IsDisposed);

			form = (ZForm)controller.ShowEditForm(dummyBO);
			AssertEquals(true, form.IsDisposed);

			form = (ZForm)controller.ShowViewForm(dummyBO);
			AssertEquals(true, form.IsDisposed);

			controller.ShowChildrenAsDialog = false;

			using (form = (ZForm)controller.ShowFormForNewEntity(dummyBO))
			{
				AssertEquals(false, form.IsDisposed);
			}
			AssertEquals(true, form.IsDisposed);
		}

		public void TestGetOpenedForm()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Code = "BIG";
			var controller = new DummyController();
			using (var form = controller.ShowFormForNewEntity(dummyBO))
			{
				AssertEquals("Should return form that is in the opened forms cache.", form, controller.GetOpenedForm(dummyBO));
			}
		}

		public void TestSupportsHyperlinking()
		{
			var controller = new DummyController();
			AssertEquals("Should support Hyperlinking", true, controller.SupportsHyperlinking);
		}

		public void TestShowFormForNewEntity()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Code = "BIG";
			var controller = new DummyController();
			using (var form = (ZForm)controller.ShowFormForNewEntity(dummyBO))
			{
				AssertNotNull("controller.ShowFormForNewEntity(dummyBO)", form);
				AssertEquals("lastFormShown.DisplayMode", ODisplayMode.New, form.DisplayMode);
				AssertEquals("lastFormShown.Text", "New ZDummyForm", form.Text);
				AssertEquals("lastFormShown.BusinessEntity", dummyBO, form.BusinessEntity);
				AssertEquals("LastShownForm set", form, controller.LastShownForm);
			}
		}

		public void TestLastSavedPKWorksInEventOfDisposedForm()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Code = "BIG";
			var controller = new DummyController();
			var form = (ZForm)controller.ShowFormForNewEntity(dummyBO);
			AssertNotEquals(ZGuid.Empty, controller.LastSavedPK);
			form.Dispose();
			AssertEquals(null, form.DataSource);
			AssertNotEquals(ZGuid.Empty, controller.LastSavedPK);
		}

		public void TestGetModulePluginDefaultBehaviour()
		{
			using (var module = new DummyFilterGridModule())
			{
				var controller = new DummyController();
				var plugin = controller.GetModulePlugin(module);

				AssertEquals("should have returned null", null, plugin);
				AssertEquals("should have reported a developer error", "Please override GetModulePlugin() in your ZController (DummyController)", ErrorReporter.LastMessageReported);
			}
			ErrorReporter.Clear();
		}

		public void TestSetInitialTabPageToShow()
		{
			var testController = new DummyController();
			AssertEquals("", testController.InitialTabPageNameToSelectWhenAFormIsShown);

			testController.SetInitialTabPageNameForForm_Exposed("HALLABALOOZA");
			AssertEquals("HALLABALOOZA", testController.InitialTabPageNameToSelectWhenAFormIsShown);
		}

		public void TestOverridableFormCacheID()
		{
			var iD = "blahblahblah";
			var testController = new DummyController();
			testController.IDForFormCache = iD;
			try
			{
				testController.ShowEditForm(Dummy);
				AssertEquals("Should be rego-ed under overriden ID", testController.LastShownForm, OpenedFormCache.GetInstance().GetForm(Dummy.PK.ToGuid(), iD));
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestDisableOfForwardBackButtons()
		{
			var testController = new DummyController();
			Factory.Save();
			try
			{
				testController.ShowEditForm(Dummy);
				Assert("Add prev and next", ((ZDummyForm)testController.LastShownForm).AutoAddPreviousNextButtons);
				testController.LastShownForm.Dispose();

				testController.EnablePreviousNextSupport = false;
				testController.ShowEditForm(Dummy);
				Assert("No prev and next", !((ZDummyForm)testController.LastShownForm).AutoAddPreviousNextButtons);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestModuleResultsBizoSetOnEdit()
		{
			var testController = new DummyController();
			Factory.Save();
			try
			{
				testController.ShowEditForm(Dummy);
				AssertNotNull("ResultBizO set", ((ZForm)testController.LastShownForm).ModuleResultsBusinessObject);
				AssertEquals("Current set", Dummy.PK, ((ZForm)testController.LastShownForm).ModuleResultsBusinessObject.CurrentPK);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestModuleResultsBizoSetOnNew()
		{
			var testController = new DummyController();
			try
			{
				testController.ShowNewForm();
				AssertNotNull("ResultBizO set", ((ZForm)testController.LastShownForm).ModuleResultsBusinessObject);
				AssertEquals("Current set", testController.LastCreatedBizObject.PK, ((ZForm)testController.LastShownForm).ModuleResultsBusinessObject.CurrentPK);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowNonModal()
		{
			using (var parentForm = new Form())
			{
				parentForm.Text = "ParentForm";
				var testController = new DummyController();
				try
				{
					parentForm.Show();
					testController.ShowNewForm();
					Application.DoEvents();
					parentForm.BringToFront();
					Application.DoEvents();
					AssertEquals("Should not be modal - parent form should have focus", true, parentForm.ContainsFocus);
				}
				finally
				{
					DisposeControllerForm(testController);
				}
			}
		}

		public void TestLastShownFormIsNullToStartWith()
		{
			var testController = new DummyController();
			AssertNull(testController.LastShownForm);
		}

		public void TestLastShownFormSetOnNew()
		{
			var testController = new DummyController();
			try
			{
				var shownForm = testController.ShowNewForm();
				AssertNotNull("Form shown returned", shownForm);
				AssertNotNull("LastShownForm set", testController.LastShownForm);
				Assert("Is Form", testController.LastShownForm.GetType().IsSubclassOf(typeof(Form)));
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestLastShownFormSetOnEdit()
		{
			var testController = new DummyController();
			Factory.Save();
			try
			{
				var shownForm = testController.ShowEditForm(Dummy);
				AssertNotNull("Form shown returned", shownForm);
				AssertNotNull("LastShownForm set", testController.LastShownForm);
				Assert("Is Form", testController.LastShownForm.GetType().IsSubclassOf(typeof(Form)));
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestLastShownFormSetOnView()
		{
			Factory.Save();
			var testController = new DummyController();
			try
			{
				var shownForm = testController.ShowViewForm(Dummy);
				AssertNotNull("Form shown returned", shownForm);
				AssertNotNull("LastShownForm set", testController.LastShownForm);
				Assert("Is Form", testController.LastShownForm.GetType().IsSubclassOf(typeof(Form)));
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestLastShownFormSetOnDelete()
		{
			var testController = new DummyController();
			Factory.Save();
			try
			{
				var shownForm = testController.ShowDeleteForm(Dummy);
				AssertNotNull("Form shown returned", shownForm);
				AssertNotNull("LastShownForm set", testController.LastShownForm);
				Assert("Is Form", testController.LastShownForm.GetType().IsSubclassOf(typeof(Form)));
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowNewFormWithSetDefaults()
		{
			var testController = new DummyController();
			var collection = new DummyBusinessObjectCollection(Factory);
			testController.SetCollectionForDefaultsAndValidation(collection);

			try
			{
				testController.ShowNewForm();
				AssertEquals("Form Text", "New ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.New, testController.LastFormCreated.DisplayMode);
				AssertEquals("ID", DummyControllerIDs.Dummy, testController.ID);
				AssertEquals("Defaults set by Collection", "NowSet", ((DummyBusinessObject)testController.LastFormCreated.BusinessEntity).Z0_Description);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowNewFormWithPassedOnDomainValidationRegistrations()
		{
			var testController = new DummyController();
			Factory.Validation.MainGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			try
			{
				testController.ShowNewForm();
				var formBizO = (DummyBusinessObject)testController.LastFormCreated.BusinessEntity;
				Assert("Pre-condition", !formBizO.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

				testController.AddAdditionalDomainValidationGroups(Factory.Validation);
				Assert("Should contain domain validation, passed on from the Collection's factory", formBizO.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAddAdditionalDomainValidationGroups_NullParam()
		{
			var testController = new DummyController();
			try
			{
				testController.AddAdditionalDomainValidationGroups(null);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowNewForm()
		{
			var testController = new DummyController();

			try
			{
				testController.ShowNewForm();
				AssertEquals("Form Text", "New ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.New, testController.LastFormCreated.DisplayMode);
				AssertEquals("ID", DummyControllerIDs.Dummy, testController.ID);
				AssertEquals("Defaults not set by Collection", "Default", ((DummyBusinessObject)testController.LastFormCreated.BusinessEntity).Z0_Description);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowEditForm()
		{
			Dummy.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyController();

			try
			{
				testController.ShowEditForm(Dummy);
				AssertEquals("Form Text", "Edit ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.Browse, testController.LastFormCreated.DisplayMode);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowEditFormWhenViewAndEditDenied()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var deniedCheckPoint = new DummyCheckPointWithSecuritySet(false);

			Dummy.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyControllerWithCustomisableSecurity();

			testController.SetCheckPointForView(deniedCheckPoint);
			testController.SetCheckPointForEdit(deniedCheckPoint);

			try
			{
				testController.ShowEditForm(Dummy);
				AssertEquals("Form", null, testController.LastFormCreated);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowEditFormWhenViewAllowedEditDenied()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var deniedCheckPoint = new DummyCheckPointWithSecuritySet(false);
			var allowedCheckPoint = new DummyCheckPointWithSecuritySet(true);

			Dummy.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyControllerWithCustomisableSecurity();

			testController.SetCheckPointForView(allowedCheckPoint);
			testController.SetCheckPointForEdit(deniedCheckPoint);

			try
			{
				testController.ShowEditForm(Dummy);
				AssertEquals("Form Text", "View ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.ReadOnly, testController.LastFormCreated.DisplayMode);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowTemplateCopyForm()
		{
			var testController = new DummyWithTemplateCopyingController();
			var dummyWithTemplateCopying = Factory.New<DummyBusinessObjectWithTemplateCopying>();
			Factory.Save();

			try
			{
				testController.ShowTemplateCopyForm(dummyWithTemplateCopying);
				var form = testController.LastFormCreated;
				AssertNotEquals("Form", null, form);
				AssertEquals(form.BusinessEntity.Identifier, form.IdentifierForPersistingForm);
				AssertNotEquals(dummyWithTemplateCopying.PK, form.BusinessEntity.Identifier);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowTemplateCopyFormWhenBusinessObjectNoLongerInDatabase()
		{
			var testController = new DummyWithTemplateCopyingController();
			var dummyWithTemplateCopying = Factory.New<DummyBusinessObjectWithTemplateCopying>();

			try
			{
				testController.ShowTemplateCopyForm(dummyWithTemplateCopying);
				AssertEquals("Form", null, testController.LastFormCreated);
				AssertEquals(testController.AlreadyDeletedOrIrreversiblyChangedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		#region DummyBusinessObjectWithTemplateCopying

		class DummyBusinessObjectWithTemplateCopying : DummyBusinessObject, ITemplateCopyable
		{
			public DummyBusinessObjectWithTemplateCopying(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public IBusiness TemplateCopy()
			{
				return Clone();
			}
		}

		class DummyWithTemplateCopyingController : DummyController
		{
			public override Type TypeOfTopLevelBusinessObject
			{
				get { return typeof(DummyBusinessObjectWithTemplateCopying); }
			}

			protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			{
				BusinessObject result = factory.Load<DummyBusinessObjectWithTemplateCopying>(sourceEntityPK);
				return result;
			}
		}

		class DummyControllerWithTemplateRecord : DummyController
		{
			public override Type TypeOfTopLevelBusinessObject
			{
				get { return typeof(DummyBusinessObjectWithTemplateCopying); }
			}

			protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			{
				var templateRecordProvider = factory.Load<DummyTemplateRecordProvider>(sourceEntityPK);
				return templateRecordProvider;
			}
		}

		#endregion

		public void TestShowEditFormWhenBusinessObjectNoLongerInDatabase()
		{
			var testController = new DummyController();
			try
			{
				testController.ShowEditForm(Dummy);
				AssertEquals("Form", null, testController.LastFormCreated);
				AssertEquals(testController.AlreadyDeletedOrIrreversiblyChangedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowViewForm()
		{
			Dummy.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyController();

			try
			{
				testController.ShowViewForm(Dummy);
				AssertEquals("Form Text", "View ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.ReadOnly, testController.LastFormCreated.DisplayMode);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowDeleteForm()
		{
			Dummy.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyController();

			try
			{
				testController.ShowDeleteForm(Dummy);
				AssertEquals("Form Text", "Delete ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.Delete, testController.LastFormCreated.DisplayMode);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowDeleteFormWithCancellable()
		{
			var dummyCancellable = Factory.New<DummyCancellable>();
			dummyCancellable.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyControllerWithCancellableBizO();

			try
			{
				AssertEquals("Precondition: IsCancelled should be false", false, dummyCancellable.IsCancelled);
				testController.ShowDeleteForm(dummyCancellable);
				AssertEquals("Form Text", "Deactivate ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.Delete, testController.LastFormCreated.DisplayMode);

				AssertEquals(true, ((testController.LastShownForm as ZForm).BusinessEntity as ICancellable).IsCancelled);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowDeleteFormWithCancellableNotDeletable()
		{
			var dummyCancellable = Factory.New<DummyCancellableNotDeletable>();
			dummyCancellable.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyControllerWhichAllowsDelete();

			try
			{
				AssertEquals("Precondition: IsCancelled should be false", false, dummyCancellable.IsCancelled);
				testController.ShowDeleteForm(dummyCancellable);
				AssertNull(testController.LastShownForm);

				AssertEquals("Just because", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowDeleteFormInReadWriteModeForReactivating()
		{
			var dummyCancellable = Factory.New<DummyCancellable>();
			dummyCancellable.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyControllerWithCancellableBizO();
			dummyCancellable.IsCancelled = true;

			try
			{
				AssertEquals("Precondition: IsCancelled should be false", true, dummyCancellable.IsCancelled);
				testController.ShowDeleteForm(dummyCancellable);
				AssertEquals("Form Text", "Activate ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.Delete, testController.LastFormCreated.DisplayMode);

				AssertEquals(false, ((testController.LastShownForm as ZForm).BusinessEntity as ICancellable).IsCancelled);
				AssertEquals(false, ((testController.LastShownForm as ZForm).BusinessEntity as BusinessObject).ReadOnly);
				AssertEquals(false, ((testController.LastShownForm as ZForm).BusinessEntity as BusinessObject).LightValidationIsValid);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowDeleteForm_UsesTemplateRecordAsCancellable()
		{
			var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			templateRecordProvider.Z0_Code = "ttt";

			var templateRecord = Factory.New<DummyTemplateRecord>();
			templateRecord.IsCancelled = true;
			templateRecordProvider.TemplateRecord = templateRecord;

			Dummy.Factory.Save();
			var testController = new DummyControllerWithTemplateRecord();

			try
			{
				Assert("Precondition", templateRecord.IsCancelled);
				testController.ShowDeleteForm(templateRecordProvider);

				AssertEquals("Form Text", "Activate ZDummyForm", testController.LastFormCreated.Text);
				AssertEquals("DisplayMode", ODisplayMode.Delete, testController.LastFormCreated.DisplayMode);

				var lastShownForm = (testController.LastShownForm as ZForm);
				var businessEntity = lastShownForm.BusinessEntity;
				var cancellableEntity = lastShownForm.GetICancellable(businessEntity);

				CombineAssertions(() =>
				{
					AssertEquals(false, cancellableEntity.IsCancelled);
					AssertEquals(false, (businessEntity as BusinessObject).ReadOnly);
					AssertEquals(false, (businessEntity as BusinessObject).LightValidationIsValid);
				});
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowDeleteFormCallsOnBusinessObjectIsCancelledChanged()
		{
			var dummyCancellable = Factory.New<DummyCancellable>();
			dummyCancellable.Z0_Code = "zzz";
			Dummy.Factory.Save();
			var testController = new DummyControllerWithDummyPlugin();
			Factory.Save();
			try
			{
				var shownForm = testController.ShowDeleteForm(dummyCancellable);
				var plugin = (shownForm as ZForm).PlugIns.GetPlugIn(DummyControllerIDs.Dummy1) as DummyPlugIn1;
				AssertNotNull(plugin);
				Assert(plugin.IsCancelledCurrentValue);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

#if !WINZOR

		[ExpectNoExceptions]
		public void TestShowDeleteFormWhenPointerAutomaticallyMovesToDefaultButton()
		{
			var zero = 0;
			var snapToDefaultButtonValue = 0;
			Assert(SystemParametersInfo(0x005F /*SPI_GETSNAPTODEFBUTTON*/, 0, ref snapToDefaultButtonValue, 0));
			try
			{
				Assert(SystemParametersInfo(0x0060 /*SPI_SETSNAPTODEFBUTTON*/, 1, ref zero, 2 /*SPIF_SENDCHANGE*/));

				Dummy.Z0_Code = "ttt";
				Dummy.Factory.Save();
				var testController = new DummyController();
				try
				{
					testController.ShowDeleteForm(Dummy);
				}
				finally
				{
					DisposeControllerForm(testController);
				}
			}
			finally
			{
				Assert(SystemParametersInfo(0x0060 /*SPI_SETSNAPTODEFBUTTON*/, snapToDefaultButtonValue, ref zero, 2 /*SPIF_SENDCHANGE*/));
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool SystemParametersInfo(int nAction, int nParam, ref int value, int nUpdate);

#endif

		class DummyControllerWithDummyPlugin : DummyController
		{
			protected override IZForm GetForm(IBusiness businessEntity)
			{
				var form = base.GetForm(businessEntity);
				(form as ZForm).PlugIns.Add(DummyControllerIDs.Dummy1);
				return form;
			}

			protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			{
				BusinessObject result = factory.Load<DummyCancellable>(sourceEntityPK);
				return result ?? base.LoadBusinessEntity(factory, sourceEntityPK);
			}
		}

		class DummyCanBeSavedControllerWithDummyPlugin : DummyController
		{
			protected override IZForm GetForm(IBusiness businessEntity)
			{
				var form = base.GetForm(businessEntity);
				(form as ZForm).PlugIns.Add(DummyControllerIDs.Dummy1);
				return form;
			}

			protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			{
				BusinessObject result = factory.Load<DummyCancellableCanBeSaved>(sourceEntityPK);
				return result ?? base.LoadBusinessEntity(factory, sourceEntityPK);
			}
		}

		public void TestFormCachingHookupForExistingForm()
		{
			Dummy.Z0_Code = "ttt";
			Dummy.Factory.Save();

			var testController1 = new DummyController();
			var testController2 = new DummyController();
			var testController3 = new DummyController();
			var testController4 = new DummyController();

			try
			{
				testController1.ShowEditForm(Dummy); // this should add it to the FormCache
				AssertNotNull("First Form is shown", testController1.LastFormCreated);

				testController2.ShowEditForm(Dummy);
				AssertNull("Second Form is not created", testController2.LastFormCreated);

				testController3.ShowDeleteForm(Dummy);
				AssertNull("Another Delete Form is not created", testController3.LastFormCreated);

				testController4.ShowViewForm(Dummy);
				AssertNull("Another view form is not created", testController4.LastFormCreated);
			}
			finally
			{
				DisposeControllerForm(testController1);
			}
		}

		public void TestFormCachingHookupForNewForm()
		{
			var testController1 = new DummyController();
			var testController2 = new DummyController();
			var testController3 = new DummyController();
			var testController4 = new DummyController();

			try
			{
				testController1.ShowNewForm(); // this should add it to the FormCache
				AssertNotNull("First Form is shown", testController1.LastFormCreated);

				testController2.ShowEditForm(testController1.LastCreatedBizObject);
				AssertNull("Second Form is not created", testController2.LastFormCreated);

				testController3.ShowDeleteForm(testController1.LastCreatedBizObject);
				AssertNull("Another Delete Form is not created", testController3.LastFormCreated);

				testController4.ShowViewForm(testController1.LastCreatedBizObject);
				AssertNull("Another view form is not created", testController4.LastFormCreated);
			}
			finally
			{
				DisposeControllerForm(testController1);
			}
		}

		public void TestGetPlugInInternalDefaultSecurityForView()
		{
			var controller = new DummyControllerWithCustomisableSecurity();
			SecurityCheckpoint viewCheckPoint = new DummyCheckPointWithSecuritySet(true);
			SecurityCheckpoint editCheckPoint = new DummyCheckPointWithSecuritySet(false);
			controller.SetCheckPointForView(viewCheckPoint);
			controller.SetCheckPointForEdit(editCheckPoint);

			using (var plugIn = (DummyPlugIn2)controller.GetPlugInInternal(null, null))
			{
				AssertEquals("PlugIn.SecurityCheckpoint", viewCheckPoint, plugIn.SecurityCheckpoint);
			}
		}

		public void TestGetPlugInInternalDefaultSecurityForViewAndEdit()
		{
			var controller = new DummyControllerWithCustomisableSecurity();
			SecurityCheckpoint viewCheckPoint = new DummyCheckPointWithSecuritySet(true);
			SecurityCheckpoint editCheckPoint = new DummyCheckPointWithSecuritySet(true);
			controller.SetCheckPointForView(viewCheckPoint);
			controller.SetCheckPointForEdit(editCheckPoint);

			using (var plugIn = controller.GetPlugInInternal(null, null))
			{
				AssertEquals("PlugIn.SecurityCheckpoint", editCheckPoint, plugIn.SecurityCheckpoint);
			}
		}

		public void TestGetPlugInInternal()
		{
			var controller = new DummyController1();
			SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);

			using (var plugIn = controller.GetPlugInInternal(null, checkPoint))
			{
				AssertEquals("PlugIn.SecurityCheckpoint", checkPoint, plugIn.SecurityCheckpoint);
			}
		}

		public void TestGetPlugInInternal_SetsControllerID()
		{
			var controller = new DummyController1();
			using (var plugIn = controller.GetPlugInInternal(null, null))
			{
				AssertEquals(DummyControllerIDs.Dummy1, plugIn.ControllerID);
			}
		}

		[ExpectNoExceptions]
		public void TestGetPlugInInternalWithNullPlugIn()
		{
			var controller = new DummyControllerWithNullPlugIn();
			using (var plugIn = controller.GetPlugInInternal(null, null))
			{
			}
		}

		public void TestShowNewFormSetsStrategyProvider()
		{
			var testController = new DummyControllerWithStrategyProvider();

			try
			{
				testController.ShowNewForm();
				AssertEquals("SetStrategyProver was passed", testController.Factory, testController.StrategyFactory);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestShowEditFormSetsStrategyProvider()
		{
			Dummy.Z0_Code = "ttt";
			Dummy.Factory.Save();
			var testController = new DummyControllerWithStrategyProvider();

			try
			{
				testController.ShowEditForm(Dummy);
				AssertEquals("SetStrategyProver was passed", testController.Factory, testController.StrategyFactory);
			}
			finally
			{
				DisposeControllerForm(testController);
			}
		}

		public void TestIRelationshipAdderForController()
		{
			var dummyChild = Factory.New<DummyChildBusinessObject>();

			var dummyCollection = new DummyBusinessObjectCollectionWithIRelationshipAdderForController(Factory, dummyChild);
			var dummyController = (DummyController)ZControllerFactory.Create(DummyControllerIDs.Dummy);
			dummyController.SetCollectionForDefaultsAndValidation(dummyCollection);

			dummyController.ShowNewForm();
			try
			{
				var newFormsBizO = (DummyBusinessObject)((ZForm)dummyController.LastShownForm).BusinessEntity;
				AssertEquals("Should have DummyChild added to the collection", 1, newFormsBizO.Collection.Count);
				AssertEquals("Should have DummyChild added to the collection", dummyChild.PK, newFormsBizO.Collection[0].PK);
			}
			finally
			{
				if (dummyController.LastFormCreated != null)
				{
					dummyController.LastFormCreated.Close();
				}
			}
		}

		public void TestForIValidateForController()
		{
			var dummyCollection = new DummyBusinessObjectCollectionWithValidateForController(Factory);
			var dummyController = (DummyController)ZControllerFactory.Create(DummyControllerIDs.Dummy);
			dummyController.SetCollectionForDefaultsAndValidation(dummyCollection);

			dummyController.ShowNewForm();
			try
			{
				var dummy = (DummyBusinessObject)dummyController.LastFormCreated.BusinessEntity;
				dummy.Z0_AnotherNumber = 6;

				dummy.RunPreSaveValidation();
				AssertEquals("Z0_AnotherNumber has errors", true, dummy.Z0_AnotherNumberInfo.HasErrors());

				dummy.Z0_AnotherNumber = 4;
				dummy.RunPreSaveValidation();
				AssertEquals("Z0_AnotherNumber has no errors", false, dummy.Z0_AnotherNumberInfo.HasErrors());
			}
			finally
			{
				if (dummyController.LastFormCreated != null)
				{
					dummyController.LastFormCreated.Close();
				}
			}
		}

		void DisposeControllerForm(DummyController controller)
		{
			if (controller.LastFormCreated != null)
			{
				controller.LastFormCreated.Close();
				controller.LastFormCreated.Dispose();
			}
		}

		public class DummyBizOValidationForTest : AutoDummyBizoValidation
		{
			public DummyBizOValidationForTest(DummyBusinessObject parent)
				: base(parent)
			{
			}
		}

		class DummyBusinessObjectCollectionWithIRelationshipAdderForController : DummyBusinessObjectCollection, IRelationshipAdderForController
		{
			public DummyBusinessObjectCollectionWithIRelationshipAdderForController(BusinessObjectFactory factory, DummyChildBusinessObject letsHaveARelationship)
				: base(factory)
			{
				this.LetsHaveARelationship = letsHaveARelationship;
			}

			public void AddRelationshipToNewObject(BusinessObject newBusinessObject)
			{
				((DummyBusinessObject)newBusinessObject).Collection.Add(LetsHaveARelationship);
			}

			readonly DummyChildBusinessObject LetsHaveARelationship;
		}

		public class DummyBusinessObjectCollectionWithValidateForController : DummyBusinessObjectCollection, IValidateForController
		{
			public DummyBusinessObjectCollectionWithValidateForController(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public void ValidateEntityOnSaving(IBusiness entity)
			{
				var dummy = (DummyBusinessObject)entity;
				using (var token = dummy.SuspendValidationTesting())
				{
					dummy.Z0_AnotherNumberInfo.ClearAllNotifications();
					if (dummy.Z0_AnotherNumber > 5)
					{
						dummy.Z0_AnotherNumberInfo.AddError("Error! Error! Please leave the building! Alert Alert!");
					}
				}
			}
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			var controller = new DummyController();
			Assert("EnsureUrlIsCompanySpecific by default should be true", controller.MakeUrlOnlyOpenableForCurrentCompany(ZGuid.Empty));
			Assert("We can do the magic", !controller.MakeUrlOnlyOpenableForCurrentCompany(ZGuid.BrettsGuid));
		}

		#region Security Checkpoints

		public void TestGetCheckPointForView()
		{
			var controller = new DummyController();

			AssertEquals((SecurityCheckpoint)EnvProxy.Instance.Security.None, controller.GetCheckPointForView(null));
		}

		public void TestGetCheckPointForNew()
		{
			var controller = new DummyController();

			AssertEquals((SecurityCheckpoint)EnvProxy.Instance.Security.None, controller.GetCheckPointForNew(null));
		}

		public void TestGetCheckPointForEdit()
		{
			var controller = new DummyController();

			AssertEquals((SecurityCheckpoint)EnvProxy.Instance.Security.None, controller.GetCheckPointForEdit(null));
		}

		public void TestGetCheckPointForDelete()
		{
			var controller = new DummyController();

			AssertEquals((SecurityCheckpoint)EnvProxy.Instance.Security.None, controller.GetCheckPointForDelete(null));
		}

		public void TestGetCheckPointForCopy()
		{
			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				var controller = new DummyController { ParentModule = module };
				var dummy = Factory.New<DummyTemplateRecordProvider>();

				module.SupportTemplateRecordsForTest = true;
				module.SetCanBeCopied(true);
				Assert(module.AllowAddCopyMenuItem);

				var newCheckpoint = controller.GetCheckPointForNew(dummy);
				newCheckpoint.IsAllowed = false;

				var copyCheckpoint = controller.GetCheckPointForCopyForTest(dummy);
				AssertSame(copyCheckpoint, newCheckpoint);

				newCheckpoint.IsAllowed = true;
				copyCheckpoint = controller.GetCheckPointForCopyForTest(dummy);
				AssertNotEquals(copyCheckpoint, newCheckpoint);
				AssertEquals("DummyCopyFunction", copyCheckpoint.Code);
			}
		}

		[ExpectNoExceptions]
		public void TestCheckPointForCopyWhenModuleWithNoneSecurityCheckpoint()
		{
			using (var module = new DummyFilterGridModuleWithNoneSecurityCheckpoint())
			{
				var controller = new DummyController { ParentModule = module };
				var dummy = Factory.New<DummyTemplateRecordProvider>();

				module.SupportTemplateRecordsForTest = true;
				module.SetCanBeCopied(true);
				Assert(module.AllowAddCopyMenuItem);

				var newCheckpoint = controller.GetCheckPointForNew(dummy);
				newCheckpoint.IsAllowed = true;

				var copyCheckpoint = controller.CheckPointForCopyExposedForTest;
				AssertNull(copyCheckpoint);
			}
		}

		class DummyFilterGridModuleWithNoneSecurityCheckpoint : DummyFilterGridModuleWithTemplates
		{
			public override SecurityCheckpoint SecurityCheckpoint => EnvProxy.Instance.Security.None as NoneSecurityCheckpoint;
		}

		#endregion

		#region Template Record Security Checkpoints

		public void TestGetCheckPointForViewForTemplateRecord()
		{
			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				var controller = new DummyController { ParentModule = module };

				AssertCheckPointForTemplateRecord(module, () => controller.CheckPointForViewExposedForTest, () => controller.TemplateRecordCheckpoint, bizo => controller.GetCheckPointForView(bizo));
			}
		}

		public void TestGetCheckPointForNewForTemplateRecord()
		{
			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				var controller = new DummyController { ParentModule = module };

				AssertCheckPointForTemplateRecord(module, () => controller.CheckPointForNewExposedForTest, () => controller.TemplateRecordAddCheckpoint, bizo => controller.GetCheckPointForNew(bizo));
			}
		}

		public void TestGetCheckPointForEditForTemplateRecord()
		{
			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				var controller = new DummyController { ParentModule = module };

				AssertCheckPointForTemplateRecord(module, () => controller.CheckPointForEditExposedForTest, () => controller.TemplateRecordEditCheckpoint, bizo => controller.GetCheckPointForEdit(bizo));
			}
		}

		public void TestGetCheckPointForDeleteForTemplateRecord()
		{
			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				var controller = new DummyController { ParentModule = module };

				AssertCheckPointForTemplateRecord(module, () => controller.CheckPointForDeleteExposedForTest, () => controller.TemplateRecordDeleteCheckpoint, bizo => controller.GetCheckPointForDelete(bizo));
			}
		}

		void AssertCheckPointForTemplateRecord(DummyFilterGridModuleWithTemplates module,
			Func<SecurityCheckpoint> baseCheckpoint, Func<SecurityCheckpoint> templateCheckpoint, Func<BusinessObject, SecurityCheckpoint> checkpointGetter)
		{
			module.SupportTemplateRecordsForTest = false;
			AssertSame(baseCheckpoint(), templateCheckpoint());

			module.SupportTemplateRecordsForTest = true;
			AssertNotEquals("Should have separate checkpoint for template records", baseCheckpoint(), templateCheckpoint());

			var dummy = Factory.New<DummyTemplateRecordProvider>();

			dummy.IsTemplateRecord = false;
			AssertSame(baseCheckpoint(), checkpointGetter(dummy));

			dummy.IsTemplateRecord = true;
			AssertSame(templateCheckpoint(), checkpointGetter(dummy));
		}

		#endregion

#if !WINZOR
		public void TestShowFromPopupMenuAsRemoteApp()
		{
			var controller = new MockController();
			try
			{
				using (new MenuClickPendingTracker())
				{
					DataRegistry.Instance.RemoteAppShowFormViaMenuDelayMilliseconds = 1000;
					ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
					ZController.ActivateAfterTimerDelayWasCalled = false;
					var form = (ZForm)controller.ShowNewForm();
					AssertEquals("must be shown immediately", true, form.Visible);
					AssertEquals("must not be active", false, form.Focused);

					for (var i = 0; i < 40; ++i)
					{
						if (ZController.ActivateAfterTimerDelayWasCalled)
						{
							break;
						}

						Application.DoEvents();
						System.Threading.Thread.Sleep(50);
					}
					var start = controller.ShowModelessFormClock;
					var end = ZController.ActivateAfterTimerDelayClock;

					Assert("ActivateAfterTimerDelayWasCalled", ZController.ActivateAfterTimerDelayWasCalled);
					AssertEquals(form, controller.LastFormCreated);
					AssertEquals("DisplayMode", ODisplayMode.New, controller.LastFormCreated.DisplayMode);
					// allow 50ms timer granularity
					Assert((end - start).TotalMilliseconds >= 1000 - 50);
				}
			}
			finally
			{
				DisposeControllerForm(controller);
			}
		}
#endif

		public void TestSuccessfullyDelete_ShowDeleteFormAfterShowEditForm()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var controller = new DummyController();
			try
			{
				UnitTestUserNotification.Instance.ClearMessages();

				var form = (ZDummyForm)controller.ShowEditForm(dummyBO);
				CombineAssertions(() =>
				{
					Assert(form.Text.StartsWith("Edit"));
					Assert(!((BusinessObject)form.DataSource).ReadOnly);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				});

				controller.ShowDeleteForm(dummyBO);
				CombineAssertions(() =>
				{
					Assert(form.Text.StartsWith("Delete"));
					Assert(((BusinessObject)form.DataSource).ReadOnly);
				});

				form.HandleApplyPostingButtonClickUnsafe(true);
				var dummyBONow = Factory.Load<DummyBusinessObject>(dummyBO.PK);
				AssertNull(dummyBONow);
			}
			finally
			{
				DisposeControllerForm(controller);
			}
		}

		public void TestSuccessfullyDelete_ShowDeleteFormAfterShowViewForm()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var controller = new DummyController();
			controller.LicenceCheckpointForViewOverride = EnvProxy.Instance.Licence.DocManager;
			controller.LicenceCheckpointForModifyOverride = EnvProxy.Instance.Licence.ContainerManager;
			try
			{
				var form = (ZDummyForm)controller.ShowViewForm(dummyBO);
				Assert(form.LicensedComponentManager.ContainsCheckpoint(EnvProxy.Instance.Licence.DocManager));

				controller.ShowDeleteForm(dummyBO);
				Assert(form.LicensedComponentManager.ContainsCheckpoint(EnvProxy.Instance.Licence.ContainerManager));

				form.HandleApplyPostingButtonClickUnsafe(true);
				var dummyBONow = Factory.Load<DummyBusinessObject>(dummyBO.PK);
				AssertNull(dummyBONow);
			}
			finally
			{
				DisposeControllerForm(controller);
			}
		}

		public void TestBusinessObjectIsCancelledChanged_ShowDeleteFormAfterViewForm()
		{
			var dummyBO = Factory.New<DummyCancellableCanBeSaved>();
			Factory.Save();
			Assert(!dummyBO.IsCancelled);

			var controller = new DummyCanBeSavedControllerWithDummyPlugin();
			try
			{
				var form = (ZDummyForm)controller.ShowViewForm(dummyBO);
				controller.ShowDeleteForm(dummyBO);
				var plugin = form.PlugIns.GetPlugIn(DummyControllerIDs.Dummy1) as DummyPlugIn1;
				CombineAssertions(() =>
				{
					AssertNotNull(plugin);
					Assert(plugin.IsCancelledCurrentValue);
				});

				form.HandleApplyPostingButtonClickUnsafe(true);
				var dummyBONow = Factory.Load<DummyCancellableCanBeSaved>(dummyBO.PK);
				CombineAssertions(() =>
				{
					AssertNotNull(dummyBONow);
					Assert(dummyBONow.IsCancelled);
				});
			}
			finally
			{
				DisposeControllerForm(controller);
			}
		}

		public void TestShowOtherUsersCurrentlyAccessingThisEntity_ShowDeleteFormAfterShowViewOrEditForm()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			Factory.Save();

			using (var provider = new SemaphoreProviderWithMockUserIdForTesting(Guid.NewGuid()))
			using (((ISemaphoreProvider)provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(dummyBO.PK.ToString())))
			{
				var controller = new DummyController();
				try
				{
					using ((ZDummyForm)controller.ShowViewForm(dummyBO))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						Application.DoEvents();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

						controller.ShowDeleteForm(dummyBO);
						CombineAssertions(() =>
						{
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("These users are currently modifying ZDummyForm or one of its dependent objects:\r\nZDummyForm"));
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.Trim().EndsWith("Your modifications may not be able to be saved if the other user saves first (times shown in your local time zone)."));
						});
					}
					using ((ZDummyForm)controller.ShowEditForm(dummyBO))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						Application.DoEvents();
						CombineAssertions(() =>
						{
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("These users are currently modifying ZDummyForm or one of its dependent objects:\r\nZDummyForm"));
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.Trim().EndsWith("Your modifications may not be able to be saved if the other user saves first (times shown in your local time zone)."));
						});

						UnitTestUserNotification.Instance.ClearMessages();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

						controller.ShowDeleteForm(dummyBO);
						CombineAssertions(() =>
						{
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("These users are currently modifying ZDummyForm or one of its dependent objects:\r\nZDummyForm"));
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.Trim().EndsWith("Your modifications may not be able to be saved if the other user saves first (times shown in your local time zone)."));
						});
					}
				}
				finally
				{
					DisposeControllerForm(controller);
				}
			}
		}

		public void TestBusinessObjectIsCancelledShouldBeFalse_ShowDeleteFormAfterViewForm()
		{
			var dummy = Factory.New<DummyCancellable>();
			dummy.Z0_Code = "ttt";
			dummy.IsCancelled = true;
			Factory.Save();
			var controller = new DummyControllerWithCancellableBizO();
			var controller2 = new DummyControllerWithCancellableBizO();
			try
			{
				controller.ShowEditForm(dummy);
				AssertEquals("Precondition: controller.BusinessEntity.IsCancelled should be true", true, ((controller.LastShownForm as ZForm).BusinessEntity as ICancellable).IsCancelled);
				var showDeleteForm = controller2.ShowDeleteForm(dummy);
				AssertEquals("Precondition: controller2.BusinessEntity.IsCancelled should be false", false, ((controller2.LastShownForm as ZForm).BusinessEntity as ICancellable).IsCancelled);
				var postingButtonsProvider = showDeleteForm as IPostingButtonsProvider;

				AssertEquals("Activate", postingButtonsProvider.CommandButtonPost.Text);
			}
			finally
			{
				DisposeControllerForm(controller);
				DisposeControllerForm(controller2);
			}
		}

		public void TestBusinessObjectIsCancelledShouldNotBeChanged_PreventDeleteAttributeIsTrue()
		{
			var dummy = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			dummy.Z0_Code = "ttt";
			dummy.IsCancelled = true;
			Factory.Save();
			var controller = new DummyControllerwithCancellableWhichCanNotBeDeleted();
			var controller2 = new DummyControllerwithCancellableWhichCanNotBeDeleted();
			try
			{
				controller.ShowDeleteForm(dummy);
				AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(DummyCancellableNotDeletable)));
				AssertEquals("Precondition: controller.BusinessEntity.IsCancelled should be true", true, ((controller.LastShownForm as ZForm).BusinessEntity as ICancellable).IsCancelled);
				controller2.ShowDeleteForm(dummy);
				AssertEquals("Precondition: controller2.BusinessEntity.IsCancelled should be true", true, ((controller2.LastShownForm as ZForm).BusinessEntity as ICancellable).IsCancelled);
			}
			finally
			{
				DisposeControllerForm(controller);
				DisposeControllerForm(controller2);
			}
		}
	}
}
