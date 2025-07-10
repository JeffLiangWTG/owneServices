using System;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZFormUtilitiesTest : TestCaseWithDummy
	{
		#region Ensure edited value is committed

		public void TestEnsureEditedZGridCellDataValueIsCommitted()
		{
			var superBizObj = Factory.New<DummyBusinessObject>();
			var rowBizObj = superBizObj.Collection.AddNew();

			using (var testForm = new ZTestGridForm(superBizObj))
			{
				testForm.Show();

				AssertEquals("Z0_Description", "Default", rowBizObj.Z0_Description);
				var columnStyle = (ZGridColumnStyle)testForm.TabGrid.TableStyles[0].GridColumnStyles[0];
				testForm.TabGrid.BeginEdit(columnStyle, 0);
				KeySender.PostKeyDown(testForm.GetFrontMostActiveControl(), Keys.L); //namespace!
				KeySender.PostKeyDown(testForm.GetFrontMostActiveControl(), Keys.O);
				KeySender.PostKeyDown(testForm.GetFrontMostActiveControl(), Keys.L);
				Application.DoEvents();

				typeof(MenuItem).InvokeMember(
				"OnPopup",
					BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					testForm.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName],
					new[] { EventArgs.Empty });
				AssertEquals("Should update Z0_Description on bizobj", "lol", rowBizObj.Z0_Description.ToLower());
			}
		}

		public void TestEnsureSelectedControlValueCommitted()
		{
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();

				testForm.TextBox.Text = "qwertyuiop";
				testForm.TextBox.Focus();

				const int selectionLength = 3;
				testForm.TextBox.SelectionLength = selectionLength;

				const int selectionStart = 4;
				testForm.TextBox.SelectionStart = selectionStart;

				typeof(MenuItem).InvokeMember(
					"OnPopup",
					BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				testForm.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName],
					new[] { EventArgs.Empty });

				AssertEquals("selectionLength", selectionLength, testForm.TextBox.SelectionLength);
				AssertEquals("selectionStart", selectionStart, testForm.TextBox.SelectionStart);
				AssertEquals("text box not active control", testForm.TextBox, testForm.GetFrontMostActiveControl());
				AssertEquals("data was not posted to the business layer", "QWERTYUIOP", Dummy.Z0_Description);
			}
		}

		#endregion

		#region BusinessEntityForPersistingForm

		public void TestGetBusiness_ShouldFallBackToBusinessEntityForPersistingForm()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var form = new ZFormWithBusinessEntityForPersistingForm())
			{
				AssertNull(ZFormUtilities.GetBusiness(form));

				form.BusinessEntityForPersistingForm_Override = dummy;

				AssertEquals(dummy, ZFormUtilities.GetBusiness(form));
			}
		}

		public void TestBusinessEntityShortcutUrl_ShouldFallBackToBusinessEntityForPersistingForm()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var form = new ZFormWithBusinessEntityForPersistingForm())
			{
				form.BusinessEntityForPersistingForm_Override = dummy;

				AssertStartsWith("", string.Format("edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK={0}&VersionNumber={1}&Hash=", dummy.PK, new EnterpriseInformationRetriever().VersionNumber), ZFormUtilities.BusinessEntityShortcutUrl(form, false));
			}
		}

		class ZFormWithBusinessEntityForPersistingForm : ZForm
		{
			internal ZFormWithBusinessEntityForPersistingForm()
			{
				ControllerID = DummyControllerIDs.Dummy;
			}

			internal IBusiness BusinessEntityForPersistingForm_Override { get; set; }

			public override IBusiness BusinessEntityForPersistingForm
			{
				get { return BusinessEntityForPersistingForm_Override; }
			}
		}

		#endregion

		public void TestGetBusiness()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummyBizO))
			{
				var bizO = ZFormUtilities.GetBusiness(testForm);
				AssertNotNull("Business Object", bizO);
				AssertEquals("Same BizO returned", dummyBizO, bizO);
			}
		}

		public void TestBizOShortcut()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummyBizO))
			{
				var bizO = ZFormUtilities.GetBusiness(testForm);
				testForm.ControllerID = DummyControllerIDs.Dummy;
				var shortcut = ZFormUtilities.BusinessEntityShortcutUrl(testForm, false);
				AssertEquals(shortcut, ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, bizO.Identifier));
			}
		}

		public void TestSupportsHyperlinkingController()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummyBizO))
			{
				var bizO = ZFormUtilities.GetBusiness(testForm);
				testForm.ControllerID = DummyControllerIDs.DummyControllerNotSupportsHyperlinking;
				var shortcut = ZFormUtilities.BusinessEntityShortcutUrl(testForm, false);
				AssertEquals(shortcut, string.Empty);
			}
		}

		public void TestBizOShortcut_WithNoControllerID()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummyBizO))
			{
				var bizO = ZFormUtilities.GetBusiness(testForm);
				testForm.ControllerID = null;
				var shortcut = ZFormUtilities.BusinessEntityShortcutUrl(testForm, false);
				AssertEquals("", shortcut);
				AssertNoExceptionThrown(delegate { });

				testForm.ControllerID = DummyControllerIDs.Dummy;
				shortcut = ZFormUtilities.BusinessEntityShortcutUrl(testForm, false);
				AssertNotEquals("", shortcut);
				AssertEquals(shortcut, ShowEditFormUrlHandler.Instance.Create(testForm.ControllerID, bizO.Identifier));
				AssertNoExceptionThrown(delegate { });
			}
		}

		public void TestGetControllerIDsFromURLSafe()
		{
			DataRegistry.Instance.WebHyperlinksEnabled = false;

			var guid = Guid.NewGuid();
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, guid);

			AssertStartsWith("URL should be a edient-based URL", "edient:", url);

			var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(url);
			AssertNotNull(tuple);

			AssertEquals(DummyControllerIDs.Dummy, tuple.Item1);
			AssertEquals(guid, tuple.Item2);
		}

		public void TestGetControllerIDsFromURLSafe_HtmlEncodedUrl()
		{
			DataRegistry.Instance.WebHyperlinksEnabled = false;

			var guid = Guid.NewGuid();
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, guid);
			var encodedUrl = WebUtility.HtmlEncode(url);

			AssertStartsWith("URL should be a edient-based URL", "edient:", encodedUrl);

			var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(encodedUrl);
			AssertNotNull(tuple);

			AssertEquals(DummyControllerIDs.Dummy, tuple.Item1);
			AssertEquals(guid, tuple.Item2);
		}

		public void TestGetControllerIDsFromTrampolineURLSafe()
		{
			DataRegistry.Instance.WebHyperlinksEnabled = true;
			WebDataRegistry.Instance.RootServicesUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://web.services.for.cw1/path/to/services");

			var guid = Guid.NewGuid();
			var url = ShowEditFormUrlHandler.Instance.CreateWebTrampolineUri(DummyControllerIDs.Dummy, guid);

			AssertStartsWith("URL should be a HTTP-based URL", "https://", url);

			var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(url);
			AssertNotNull(tuple);

			AssertEquals(DummyControllerIDs.Dummy, tuple.Item1);
			AssertEquals(guid, tuple.Item2);
		}

		public void TestGetControllerIDsFromURLSafe_InvalidURL()
		{
			var url = "I'm a little teapot, short and stout";
			var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(url);
			AssertNull(tuple);
		}

		public void TestGetControllerIDsFromURLSafe_PartiallyValidUrl()
		{
			var url = EdiUrlPrefix.Value + "Imalittleteapotshortandstout";
			var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(url);
			AssertNull(tuple);
		}

		public void TestGetControllerIDsFromURLSafe_InvalidGuid()
		{
			var guid = Guid.NewGuid();
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, guid).Replace(guid.ToString(), "ImNotAGuid");
			var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(url);
			AssertNull(tuple);
		}

		public void TestGetControllerIDsFromURLSafe_InvalidController()
		{
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Guid.NewGuid()).Replace(DummyControllerIDs.Dummy.Name, "ImSoInvalid");
			var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(url);
			AssertNull(tuple);
		}

		public void TestProcessFavoritesOrRecent()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummyBizO))
			{
				testForm.ControllerID = DummyControllerIDs.Dummy;
				Factory.Save();
				var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(testForm);
				AssertNotNull("Link Wrapper Constructed", linkWrapper);
			}
		}

		public void TestReloadFormMenuItem_ShouldReOpenFormProperly()
		{
			AssertReloadFormMenuItem_WorksWithGivenReloadMethod(GetReloadFormButtonAndClick);
		}

		public void AssertReloadFormMenuItem_WorksWithGivenReloadMethod(Action<ZForm> myReloadTechnique)
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 1;
			Factory.Save();

			using (var formToReload = new ZFormForTestWithAccessibleValue(dummy1))
			using (var formToChangeSource = new ZFormForTestWithAccessibleValue(dummy1))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(formToReload, saveAndCloseButton, null, null);
				formToReload.ControllerID = DummyControllerIDs.Dummy;
				formToReload.Show();

				ZFormPostingButtonsStrategy.SetupPosting(formToChangeSource, saveAndCloseButton, null, null);
				formToChangeSource.ControllerID = DummyControllerIDs.Dummy;
				formToChangeSource.Show();

				AssertEquals("PRE: Our ZDummyForm has a properly set description.", dummy1.Z0_Number, formToReload.AccessibleNumber);
				AssertEquals("PRE: Our ZDummyForm has a properly set description.", dummy1.Z0_Number, formToChangeSource.AccessibleNumber);

				myReloadTechnique(formToReload);

				var firstFormCreatedByReloading = (ZDummyForm)GetFormCreatedByReloading(formToReload);
				AssertEquals("PRE: Reloading a form with no changes should do nothing of consequence, but instead... it did!", dummy1.Z0_Number.ToString(), firstFormCreatedByReloading.CalcEdit.Text);

				formToChangeSource.AccessibleNumber = 11;
				formToChangeSource.FireSaveButton();

				myReloadTechnique(firstFormCreatedByReloading);

				var secondFormCreatedByReloading = (ZDummyForm)GetFormCreatedByReloading(firstFormCreatedByReloading);
				AssertEquals("We should be able to reload the form and have the most up-to-date information from the database, but instead...", formToChangeSource.AccessibleNumber.ToString(), secondFormCreatedByReloading.CalcEdit.Text);

				secondFormCreatedByReloading.Close();
			}
		}

		public void TestReloadFormMenuItem_SavesFocusedControl()
		{
			AssertReloadFormMenuItem_SavesFocusedControl(GetReloadFormButtonAndClick);
		}

		public void AssertReloadFormMenuItem_SavesFocusedControl(Action<ZForm> myReloadTechnique)
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			using (var formToReload = new ZDummyForm())
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(formToReload, saveAndCloseButton, null, null);
				formToReload.ControllerID = DummyControllerIDs.Dummy;
				formToReload.Show();

				formToReload.CalcEdit.Focus();
				myReloadTechnique(formToReload);
				AssertEquals(typeof(ZCalcEdit), formToReload.ActiveControl.GetType());

				formToReload.TextBox.Focus();
				myReloadTechnique(formToReload);
				AssertEquals(typeof(ZTextBox), formToReload.ActiveControl.GetType());

				formToReload.TopLevelTabControl.SelectedTab = formToReload.TabPage1;
				myReloadTechnique(formToReload);
				AssertEquals("Number1", formToReload.TopLevelTabControl.SelectedTab.Name);

				formToReload.TopLevelTabControl.SelectedTab = formToReload.TabPage2;
				myReloadTechnique(formToReload);
				AssertEquals("Number2", formToReload.TopLevelTabControl.SelectedTab.Name);
			}
		}

		public void TestReloadFormMenuItem_ShouldPreserveDisplayMode()
		{
			var modes = Enum.GetValues(typeof(ODisplayMode));

			foreach (ODisplayMode mode in modes)
			{
				if (mode != ODisplayMode.Undefined)
				{
					var dummy = Factory.New<DummyBusinessObject>();
					Factory.RefreshEnabled = false;
					dummy.Z0_Number = 1;
					Factory.Save();

					AssertReloadButtonExistsForParticularFormDisplayType(dummy, mode);
				}
			}
		}

		public void TestReloadFormMenuItem_FailsOnNonPersistedForms()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.RefreshEnabled = false;
			dummy.Z0_Number = 1;

			using (var formToReload = new ZFormForTestWithAccessibleValue(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(formToReload, saveAndCloseButton, null, null);
				formToReload.ControllerID = DummyControllerIDs.Dummy;
				formToReload.Show();

				AssertEquals("PRE: Our form's businessEntity should be non-persisted.", false, ((BusinessObject)formToReload.BusinessEntity).IsInDatabase);

				formToReload.AccessibleNumber = 21; // to trigger form having changes

				var reloadButton = formToReload.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileReloadMenuItemName];
				AssertNotNull("PRE: This button definitely exists, but for prosperity's sake, we check just in-case!", reloadButton);
				reloadButton.PerformClick();
				Application.DoEvents();

				AssertNoAdditionalFormsOpened(formToReload);

				AssertEquals("We expect our message that indicates a form cannot be reloaded due to non-persistence, but instead...", "This form cannot be reloaded as it has never been saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetLinkWrapperForFavoriteOrRecentWithSpecifiedBusiness()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			using (var form = new ZForm(dummy1))
			{
				form.ControllerID = DummyControllerIDs.Dummy;

				var link = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
				AssertEquals(dummy1.PK, link.RecordKey);

				link = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form, dummy2);
				AssertEquals(dummy2.PK, link.RecordKey);

				link = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form, dummy2, false);
				AssertEquals(dummy1.PK, link.RecordKey);

				link = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form, dummy2, true);
				AssertEquals(dummy2.PK, link.RecordKey);
			}
		}

		public void TestGetLinkWrapperForFavoriteOrRecent_ShouldUseOverriddenRecentItemCaptionFormattingLogic()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			AssertEquals("Precondition", "NCODE - Default", dummy.HumanReadableShortcutName);

			using (var form = new ZFormWithOverriddenRecentItemCaptionFormatting(dummy))
			{
				form.ControllerID = DummyControllerIDs.Dummy;

				var link = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
				AssertEquals("Overridden description - NCODE - Default", link.RecordDescription);
			}
		}

		public void TestGetLinkWrapperForFavoriteOrRecent_ShouldUseRelatedRecentItemCaption_WhenBusinessPKNotUsed()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = dummy1.Collection.AddNew();

			Factory.Save();

			using (var form = new DummyForm(dummy1))
			{
				form.ControllerID = DummyControllerIDs.Dummy;

				var link = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form, dummy1, false);
				AssertEquals(dummy2.HumanReadableShortcutName, link.RecordDescription);
			}
		}

		void GetReloadFormButtonAndClick(ZForm form)
		{
			var reloadButton = form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileReloadMenuItemName];

			AssertNotNull("This button should highly exist, and yet...", reloadButton);

			reloadButton.PerformClick();
			Application.DoEvents();
		}

		void AssertReloadButtonExistsForParticularFormDisplayType(DummyBusinessObject dummy, ODisplayMode mode)
		{
			using (var formToReload = new ZFormForTestWithAccessibleValue(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(formToReload, saveAndCloseButton, null, null);
				formToReload.ControllerID = DummyControllerIDs.Dummy;
				formToReload.Show();

				formToReload.DisplayMode = mode;
				AssertEquals("PRE: We should create a form with the given DisplayMode, but instead we did not!", mode, formToReload.DisplayMode);

				var reloadButton = formToReload.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileReloadMenuItemName];
				AssertNotNull("PRE: This button definitely exists, but for prosperity's sake, we check just in-case!", reloadButton);

				reloadButton.PerformClick();
				Application.DoEvents();

				var formCreatedByReloading = GetFormCreatedByReloading(formToReload);

				ZControllerTest.AssertReloadResultsInCorrectDisplayMode(mode, formCreatedByReloading.DisplayMode);

				formCreatedByReloading.Close();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void AssertNoAdditionalFormsOpened(ZForm formThatWasReloaded)
		{
			foreach (var form in Application.OpenForms)
			{
				if (form is ZForm zForm
					&& zForm.GetHashCode() != formThatWasReloaded.GetHashCode())
				{
					Assert(false);
				}
			}

			Assert(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public ZForm GetFormCreatedByReloading(ZForm formThatWasReloaded)
		{
			ZForm formCreatedByReloading = null;
			foreach (var form in Application.OpenForms)
			{
				if (form is ZForm zForm
					&& zForm.GetHashCode() != formThatWasReloaded.GetHashCode())
				{
					formCreatedByReloading = zForm;
				}
			}

			AssertNotNull("We should find our reloadedForm, but for some reason have not.", formCreatedByReloading);

			return formCreatedByReloading;
		}

		#region Test Classes

		public class ZFormForTestWithAccessibleValue : ZForm
		{
			public ZFormForTestWithAccessibleValue(DummyBusinessObject dataSource) : base(dataSource) { }

			public int AccessibleNumber
			{
				get
				{
					var dummyEntity = this.BusinessEntity as DummyBusinessObject;
					return dummyEntity.Z0_Number;
				}
				set
				{
					if (this.BusinessEntity as DummyBusinessObject != null)
					{
						((DummyBusinessObject)BusinessEntity).Z0_Number = value;
					}
				}
			}
		}

		class ZFormWithOverriddenRecentItemCaptionFormatting : ZForm
		{
			public ZFormWithOverriddenRecentItemCaptionFormatting(object dataSource)
				: base(dataSource)
			{ }

			protected override string FormatRecentItemCaption(string caption)
			{
				return $"Overridden description - {caption}";
			}
		}

		class DummyForm : ZForm
		{
			public DummyForm(DummyBusinessObject dataSource)
				: base(dataSource)
			{
				this.IdentifierForPersistingForm = dataSource.Collection[0].PK.ToGuid();
			}
		}
		#endregion

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}
	}
}
