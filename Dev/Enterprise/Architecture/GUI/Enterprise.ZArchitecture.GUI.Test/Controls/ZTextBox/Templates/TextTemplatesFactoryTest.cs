using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TextTemplatesFactoryTest : TestCaseWithFactory
	{
		public void TestFallbackContextIdShouldNotBeUpgraded()
		{
			var dummy = Factory.NewWithValidTestData<DummyBizOWithRelatedNotes>();
			var stmNote1 = dummy.GetNotes().AddNew();
			stmNote1.ST_Description = "Test desc 1";
			stmNote1.ST_IsCustomDescription = false;
			var stmNote2 = dummy.GetNotes().AddNew();
			stmNote2.ST_Description = "Test desc 2";
			stmNote2.ST_IsCustomDescription = false;
			Factory.Save();

			using (var form = new StmNoteFormTest(stmNote1, "ST_NoteData"))
			{
				var template = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
				template.S8_TemplateText = "test tempalte";
				template.Factory.Save();

				var templates = form.TextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals(1, templates.Length);
				AssertEquals("context ID should contains note description", "DummyBizo.StmNote.Test desc 1", templates[0].S8_ContextID);

				form.TextBox.contextMenuManager.TextTemplatesFactory.ResetContext();
				stmNote1.ST_IsCustomDescription = true;
				templates = form.TextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals("as the description is not changed, template should be found as fallback.", 1, templates.Length);
			}

			using (var form = new StmNoteFormTest(stmNote2, "ST_NoteData"))
			{
				var templates = form.TextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals("No template for Test desc 2", 0, templates.Length);
			}

			using (var form = new StmNoteFormTest(stmNote1, "ST_NoteData"))
			{
				var templates = form.TextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals("template for Test desc 1", 1, templates.Length);
				AssertEquals("context ID should contains note description", "DummyBizo.StmNote.Test desc 1", templates[0].S8_ContextID);
			}
		}

		public void TestTopBoIsAddedToContextBusinessObjectsForGridControl()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1 = dummy.Collection.AddNew();

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid() { Location = new Point(0, 0), Size = new Size(200, 200) })
			using (var tabControl = new ZTabControl())
			using (var tabPageWithGrid = new ZTabPage())
			{
				grid.Columns.AddTextColumn("Z0_VarCharMax", 100, true, true, false);
				tabPageWithGrid.Controls.Add(grid);
				tabControl.TabPages.Add(tabPageWithGrid);
				form.Controls.Add(tabControl);
				grid.SetDataBinding(dummy, "Collection");

				form.Show();

				var columnStyle = grid.Columns[0].ColumnStyle as ZTextBoxColumnStyle;
				grid.BeginEdit(columnStyle, 0);
				var newRowView = (DummyChildBusinessObject)grid.List.AddNew();
				newRowView.Z0_VarCharMax = "meh meh";

				var contextBusinessObjects = columnStyle.contextMenuManager.TextTemplatesFactory.ContextBusinessObjects;
				AssertEquals(2, contextBusinessObjects.Length);
				AssertCollectionContains(dummy, contextBusinessObjects);
				AssertCollectionContains(dummy1, contextBusinessObjects);
			}
		}

		public void TestAlternateContextIDs()
		{
			var dummyParent = Factory.NewWithValidTestData<DummyBizOWithRelatedNotesAlternateContext>();

			using (var form = new StmNoteFormTest(dummyParent, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				AssertNotNull(form.TextBox.contextMenuManager.TextTemplatesFactory.New());

				var contextBusinessObjects = form.TextBox.contextMenuManager.TextTemplatesFactory.ContextBusinessObjects;
				AssertEquals(1, contextBusinessObjects.Length);
				Assert(contextBusinessObjects.Contains(dummyParent));
				AssertEquals(2, form.TextBox.contextMenuManager.TextTemplatesFactory.AlternateContextIds_Exposed.Count);

				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				form.SetDataBinding(dummy, "");

				contextBusinessObjects = form.TextBox.contextMenuManager.TextTemplatesFactory.ContextBusinessObjects;
				AssertEquals(1, contextBusinessObjects.Length);
				Assert(contextBusinessObjects.Contains(dummy));
				AssertNull(form.TextBox.contextMenuManager.TextTemplatesFactory.AlternateContextIds_Exposed);
			}
		}

		public void TestGetAllTemplatesForControl()
		{
			var dummy = Factory.NewWithValidTestData<DummyBizOWithRelatedNotes>();
			var stmNote = dummy.GetNotes().AddNew();
			stmNote.ST_Description = "Test desc 1";
			Factory.Save();

			using (var form = new StmNoteFormTest(stmNote, "ST_NoteData"))
			{
				var template = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
				template.S8_TemplateText = "test tempalte";
				template.Factory.Save();

				var templates = form.TextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals(1, templates.Length);
				AssertEquals("context ID should contains note description", "DummyBizo.StmNote.Test desc 1", templates[0].S8_ContextID);

				stmNote.ST_Description = "test desc 2";
				templates = form.TextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals("as the description is changed, template could not be found.", 0, templates.Length);
			}
		}

		class StmNoteFormTest : ZForm
		{
			public StmNoteFormTest(BusinessObject bzo, string bindingMember)
				: base(bzo)
			{
				TextBox = new ZTextBox();
				BindingSource.SetBindingMember(TextBox, bindingMember);
				Controls.Add(TextBox);
				Show();
			}

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				base.SetDataBinding(dataSource, dataMember);

				TextBox?.contextMenuManager?.TextTemplatesFactory?.ResetContext();
			}

			public readonly ZTextBox TextBox;
		}

		class DummyBizOWithRelatedNotesAlternateContext : DummyBizOWithRelatedNotes, ICustomTextTemplateAlternateContexts, ICustomTextTemplateContext
		{
			public DummyBizOWithRelatedNotesAlternateContext(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IReadOnlyCollection<string> GetAlternateTextTemplateContextIDs(object dataSource, KBindingMemberInfo bindingMemberInfo)
			{
				return new string[] { "AlternateId1", "AlternateId2" };
			}

			public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo)
			{
				return new BusinessObject[] { this };
			}

			public string GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
			{
				return "TestContextID";
			}
		}
	}
}
