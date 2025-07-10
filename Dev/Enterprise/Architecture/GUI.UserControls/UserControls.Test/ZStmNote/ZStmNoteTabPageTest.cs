using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmNoteTabPageTest : ZTabPageControlTest
	{
		#region ReadOnly

		public void TestReadOnly()
		{
			using (var testForm = new WinFormForTest(Dummy))
			{
				testForm.Show();
				Application.DoEvents();

				testForm.ATabPage.ReadOnly = true;
				AssertEquals(true, testForm.ATabPage.ReadOnly);
				testForm.ATabPage.ReadOnly = false;
				AssertEquals(false, testForm.ATabPage.ReadOnly);
			}
		}

		#endregion

		#region Unread Notes

		#region TestUnreadNotePromptEventIsHookedUp

		public void TestUnreadNotePromptEventIsHookedUp()
		{
			using (var testForm = new WinFormForTest(Dummy))
			{
				testForm.Show();
				Application.DoEvents();
				AssertNotNull("ParentFormFound", testForm.ATabPage.ParentFormFound);
			}
		}

		#endregion

		#region TestUserNeedsToReadRelatedNotes

		public void TestUserNeedsToReadRelatedNotes()
		{
			var formBizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithRelatedBizONotes>();

			var currentCode = Env.CurrentCompany.Code;
			var companies = Factory.Load<IGlbCompany>(new ZQuery());
			var nonCurrentCompany = companies.Where(x => x.GC_Code != currentCode).First();
			var currentCompany = companies.Where(x => x.GC_Code == currentCode).First();

			formBizO.RelatedBizO = Dummy;
			using (var testForm = new WinFormForTest(formBizO))
			{
				testForm.Show();
				Application.DoEvents();

				var note1 = Dummy.GetNotes().AddNew();
				note1.ST_GC_RelatedCompany = currentCompany.PK;
				formBizO.GetNotes().ForceReloadRelatedElementsOnNextAccess = true;
				AssertEquals(true, testForm.ATabPage.UserNeedsToReadRelatedNotes);

				note1.ST_GC_RelatedCompany = ZGuid.Empty;
				AssertEquals(true, testForm.ATabPage.UserNeedsToReadRelatedNotes);

				note1.ST_GC_RelatedCompany = nonCurrentCompany.PK;
				AssertEquals(false, testForm.ATabPage.UserNeedsToReadRelatedNotes);

				var note2 = Dummy.GetNotes().AddNew();
				formBizO.GetNotes().ForceReloadRelatedElementsOnNextAccess = true;
				AssertEquals(true, testForm.ATabPage.UserNeedsToReadRelatedNotes);

				note2.ST_GC_RelatedCompany = nonCurrentCompany.PK;
				AssertEquals(false, testForm.ATabPage.UserNeedsToReadRelatedNotes);

				formBizO.GetNotes().ShowNotesForAllCompanies = true;
				AssertEquals(true, testForm.ATabPage.UserNeedsToReadRelatedNotes);
			}
		}
		#endregion

		#region TestUnreadNotePromptEvent_DoesNotBlowUpIfBizOIsDeleted

		public void TestUnreadNotePromptEvent_DoesNotBlowUpIfBizOIsDeleted()
		{
			var dummy = Factory.New<DummyBizOWithRelatedNotesAccessingPropertyOnBizO>();
			Factory.Save();

			using (var testForm = new WinFormForTest(dummy))
			{
				testForm.Show();

				dummy.Delete();
				Factory.Save();

				AssertNoExceptionThrown("Should not blow up if the UserIdleWorker runs while a Factory.Save Error prompt is shown", Application.DoEvents);
			}
		}

		public class DummyEnterpriseBusinessObjectWithRelatedBizONotes : DummyEnterpriseBusinessObject
		{
			public DummyEnterpriseBusinessObjectWithRelatedBizONotes(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override BusinessObject[] BusinessObjectsWithRelatedNotes => new BusinessObject[] { RelatedBizO };

			public BusinessObject RelatedBizO { get; set; }
		}

		public class DummyBizOWithRelatedNotesAccessingPropertyOnBizO : DummyEnterpriseBusinessObject
		{
			public DummyBizOWithRelatedNotesAccessingPropertyOnBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Z0_Guid = Factory.New<DummyEnterpriseBusinessObject>().PK;
			}

			protected override BusinessObject[] BusinessObjectsWithRelatedNotes => new BusinessObject[] { Factory.Load<DummyBizOWithRelatedNotes>(Z0_Guid) };
		}

		#endregion

		#endregion

		#region Text

		public void TestText()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var tabControl = new ZTemplateTabControl())
			using (var notesTabPage = new TestStmNoteTabPage())
			{
				tabControl.TabPages.Add(notesTabPage);
				form.Controls.Add(tabControl);
				form.Show();
				Application.DoEvents();
				AssertEquals("Notes", notesTabPage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateNoteImageOnRelatedChangesWithNullBusinessEntity()
		{
			using (var notesTabPage = new TestStmNoteTabPage())
			{
				notesTabPage.UpdateNoteImageOnRelatedChanges();
			}
		}

		#endregion

		#region Custom Notes

		public void TestCustomNoteTypeList_NotesParent()
		{
			var dummy = Factory.New<DummyBusinessObjectWithOverriddenNotes>();
			using (var form = new DummyFormWithNotes(dummy))
			{
				form.Show();

				AssertNull("Delegate should be hooked up on the parent of the notes, as opposed to the business entity of the form.", dummy.CustomNoteTypesDelegate);
				AssertNotNull("Delegate should be hooked up on the parent of the notes, as opposed to the business entity of the form.", dummy.InnerDummy.CustomNoteTypesDelegate);
			}
		}

		class DummyBusinessObjectWithOverriddenNotes : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObjectWithOverriddenNotes(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyEnterpriseBusinessObject InnerDummy => innerDummy ?? (innerDummy = Factory.New<DummyEnterpriseBusinessObject>());
			DummyEnterpriseBusinessObject innerDummy;

			public override Notes Notes => InnerDummy.Notes;
		}

		public void TestCustomNoteTypeListForModuleAndController()
		{
			// This XML is generated based on the sample data from CustomNoteTypeRegistryItem in Enterprise.Registry.Business. To see its structure, look in that solution.
			var xmlForCustomNoteTypes = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CustomNoteTypes><NoteModuleAndCountryList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CustomNoteModuleAndCountry><ModuleIDName>Dummy</ModuleIDName><CountryCode>ALL</CountryCode><CustomNoteTypesList><CustomNoteTypeItem><IsTextOnly>Y</IsTextOnly><IsAppendingNote>N</IsAppendingNote><IsReadOnlyAfterAdd>N</IsReadOnlyAfterAdd><ForceRead>Y</ForceRead><DefaultVisibility>DOC</DefaultVisibility><NoteName>WillShow</NoteName></CustomNoteTypeItem></CustomNoteTypesList></CustomNoteModuleAndCountry><CustomNoteModuleAndCountry><ModuleIDName>WontShowUpAsModuleForDummy</ModuleIDName><CountryCode>ALL</CountryCode><CustomNoteTypesList><CustomNoteTypeItem><IsTextOnly>Y</IsTextOnly><IsAppendingNote>N</IsAppendingNote><IsReadOnlyAfterAdd>N</IsReadOnlyAfterAdd><ForceRead>Y</ForceRead><DefaultVisibility>DOC</DefaultVisibility><NoteName>WontShow</NoteName></CustomNoteTypeItem></CustomNoteTypesList></CustomNoteModuleAndCountry></NoteModuleAndCountryList></CustomNoteTypes>";
			var result = Encoding.Unicode.GetBytes(xmlForCustomNoteTypes);
			var item = new BinaryRegistryItem("CustomNotes", null, null, null, RegistryStorageFlags.System);

			PredefinedNoteTypesWithCacheClearForTesting.Register();
			var noteTypes = (PredefinedNoteTypesWithCacheClearForTesting)PredefinedNoteTypesWithCacheClearForTesting.Instance;

			try
			{
				noteTypes.ClearCache();
				((IRegistryItemInternals)CustomNotesProvider.Instance.RegistryItem).ClearCache();
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, result);

				AssertNotNull("WillShow is a custom defined note type", SearchForNoteTypeInListOfAll(noteTypes, "WillShow"));
				AssertNotNull("WontShow is a custom defined note type", SearchForNoteTypeInListOfAll(noteTypes, "WontShow"));

				var controller = new DummyControllerWithNotesForm();
				using (var form = (DummyFormWithNotes)controller.ShowNewForm())
				{
					var customNotesForForm = ((IStmNoteParent)form.BusinessEntity).CustomNoteTypesDelegate();
					AssertNotNull(customNotesForForm);
					AssertEquals(1, customNotesForForm.Count);
					AssertEquals("WillShow", ((PredefinedNoteType)((IList)customNotesForForm)[0]).Description);
				}
			}
			finally
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
				((IRegistryItemInternals)CustomNotesProvider.Instance.RegistryItem).ClearCache();
			}
		}

		PredefinedNoteType SearchForNoteTypeInListOfAll(PredefinedNoteTypesWithCacheClearForTesting noteTypes, string itemNameToFind)
			=> Array.Find(noteTypes.All,
				delegate(PredefinedNoteType currentSearchItem)
				{
					return currentSearchItem.Description == itemNameToFind;
				});

		#endregion

		#region Icons

		public void TestNoteIconWhenAddingNote()
		{
			using (var form = new ZStmNoteTestForm(Dummy))
			{
				form.Show();
				AssertEquals("StmNote Tab Image Index", -1, form.TabPage.ImageIndex);

				_ = Dummy.GetNotes().AddNew();
				AssertEquals("StmNote Tab Image Index", form.TabPage.NoteImageIndex, form.TabPage.ImageIndex);
			}
		}

		public void TestNoteIconWithInitialNote()
		{
			using (var form = new ZStmNoteTestForm(Dummy))
			{
				_ = Dummy.GetNotes().AddNew();
				form.Show();
				AssertEquals("StmNote Tab Image Index", form.TabPage.NoteImageIndex, form.TabPage.ImageIndex);
			}
		}

		public void TestNoteIconAfterRemovingNote()
		{
			using (var form = new ZStmNoteTestForm(Dummy))
			{
				_ = Dummy.GetNotes().AddNew();
				form.Show();
				AssertEquals("StmNote Tab Image Index", form.TabPage.NoteImageIndex, form.TabPage.ImageIndex);

				Dummy.GetNotes().RemoveAndDeleteAll();
				AssertEquals("StmNote Tab Image Index", -1, form.TabPage.ImageIndex);
			}
		}

		#endregion

		#region Description Column

		public void TestRemoveDescriptionColumn()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();
			_ = dummy.GetNotes().AddNew();
			_ = dummy.GetNotes().AddNew();
			_ = dummy.GetNotes().AddNew();

			using (var form = new ZForm(dummy))
			{
				var tabControl = new ZTemplateTabControl();
				var noteTab = new ZStmNoteUserControlTest.ZStmNoteTabPageExposed();

				AssertEquals("RemoveDescriptionColumn", false, noteTab.RemoveDescriptionColumn);
				noteTab.RemoveDescriptionColumn = true;
				AssertEquals("RemoveDescriptionColumn", true, noteTab.RemoveDescriptionColumn);

				tabControl.TabPages.Insert(noteTab, 0);
				form.Controls.Add(tabControl);

				form.Show();

				AssertEquals("Grid column count", 15, noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Count);
				AssertEquals("First column name", "Visibility", noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns[0].ColumnStyle.HeaderText);
			}
		}

		#endregion

		#region Create User Full Name column

		public void TestFetchHints_CreateUserFullNamecolumn()
		{
			var factory = new BusinessObjectFactory();
			var staff1 = factory.NewWithValidTestData<GlbStaff>();
			var staff2 = factory.NewWithValidTestData<GlbStaff>();
			var staff3 = factory.NewWithValidTestData<GlbStaff>();
			var staff4 = factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TF1";
			staff2.GS_Code = "TF2";
			staff3.GS_Code = "TF3";
			staff4.GS_Code = "TF4";

			var dummy = factory.New<DummyEnterpriseBusinessObject>();
			var note1 = dummy.GetNotes().AddNew();
			var note2 = dummy.GetNotes().AddNew();
			var note3 = dummy.GetNotes().AddNew();
			var note4 = dummy.GetNotes().AddNew();
			note1.ST_SystemCreateUser = "TF1";
			note2.ST_SystemCreateUser = "TF2";
			note3.ST_SystemCreateUser = "TF3";
			note4.ST_SystemCreateUser = "TF4";
			factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newDummy = newFactory.Load<DummyEnterpriseBusinessObject>(dummy.PK);
			using (var form = new ZForm(newDummy))
			{
				var tabControl = new ZTemplateTabControl();
				var noteTab = new ZStmNoteUserControlTest.ZStmNoteTabPageExposed();

				tabControl.TabPages.Insert(noteTab, 0);
				form.Controls.Add(tabControl);

				form.Show();
				foreach (StmNote note in newDummy.GetNotes().VisibleNotes)
				{
					_ = note.ST_CreatedByUserName;
				}

				AssertEquals("should now have one hit to GlbStaff", 1, newFactory.GetTableHitCount(GlbStaffSchema.Constants.TableName));
			}
		}

		#endregion

		#region Auto Sized

		public void TestIsAutoSized() => AssertEquals(true, TestTabPage.IsAutoSized);

		#endregion

		#region Note Context

		public void TestNoteContextColumnsVisible_WhenProductivityWiseModeOff()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();

			using (var form = new ZForm(dummy))
			{
				var tabControl = new ZTemplateTabControl();
				var noteTab = new ZStmNoteUserControlTest.ZStmNoteTabPageExposed();

				tabControl.TabPages.Insert(noteTab, 0);
				form.Controls.Add(tabControl);

				form.Show();

				AssertEquals("Note Grid columns count", 16, noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Count);
				Assert("Module", noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Contains("ST_NoteContextModuleCaption"));
				Assert("Direction", noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Contains("ST_NoteContextDirectionCaption"));
				Assert("Freight", noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Contains("ST_NoteContextFreightModeCaption"));
			}
		}

		public void TestRemoveNoteContextColumns_WhenProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();

			using (var form = new ZForm(dummy))
			{
				var tabControl = new ZTemplateTabControl();
				var noteTab = new ZStmNoteUserControlTest.ZStmNoteTabPageExposed();

				tabControl.TabPages.Insert(noteTab, 0);
				form.Controls.Add(tabControl);

				form.Show();

				AssertEquals("Note Grid columns count", 13, noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Count);
				Assert("Module", !noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Contains("ST_NoteContextModuleCaption"));
				Assert("Direction", !noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Contains("ST_NoteContextDirectionCaption"));
				Assert("Freight", !noteTab.ZStmNoteUserControlExposed.NoteGrid.Columns.Contains("ST_NoteContextFreightModeCaption"));
			}
		}

		#endregion

		#region Implementation

		protected override Type TypeOfDummy => typeof(DummyEnterpriseBusinessObject);

		new TestStmNoteTabPage TestTabPage => (TestStmNoteTabPage)base.TestTabPage;

		protected override ZTabPage NewTabPage() => new TestStmNoteTabPage();

		protected override ZTabControl NewTabControl() => new ZTemplateTabControl();

		#region Test Classes

		class TestStmNoteTabPage : ZStmNoteTabPage
		{
			public new bool IsAutoSized => base.IsAutoSized;
		}

		class WinFormForTest : ZForm
		{
			public WinFormForTest(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			public ZStmNoteTestTabPage ATabPage;
			public ZTemplateTabControl MainTabControl;
			protected override void InitializeComponent()
			{
				ATabPage = new ZStmNoteTestTabPage();

				MainTabControl = new ZTemplateTabControl();
				MainTabControl.Controls.Add(ATabPage);
				Controls.Add(MainTabControl);

				base.InitializeComponent();
			}
		}

		class DummyControllerWithNotesForm : DummyController
		{
			protected override IZForm GetForm(IBusiness businessEntity) => new DummyFormWithNotes(businessEntity);
		}

		class DummyFormWithNotes : ZDummyForm
		{
			public DummyFormWithNotes(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			public ZStmNoteTabPage notesPage = new ZStmNoteTabPage();
			public ZTemplateTabControl tabControl = new ZTemplateTabControl();

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				tabControl.TabPages.Add(notesPage);
				Controls.Add(tabControl);
			}
		}

		public class PredefinedNoteTypesWithCacheClearForTesting : PredefinedNoteTypes
		{
			public void ClearCache() => base.ClearCacheOfAllNotes();

			public static void Register() => OverrideNewDelegate(() => new PredefinedNoteTypesWithCacheClearForTesting());
		}

		#endregion

		#endregion
	}
}
