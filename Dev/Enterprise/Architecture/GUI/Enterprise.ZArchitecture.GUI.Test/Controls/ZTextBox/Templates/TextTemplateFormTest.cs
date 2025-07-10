using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(TextTemplateForm))]
	sealed class TextTemplateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TextTemplateForm(Factory.New<StmNoteTemplate>(), new BusinessObject[] { Factory.New<DummyBusinessObject>() }, true);
		}

		public void TestMapTreePresentationManagerParentBusinessObjectsShouldNotNull()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var context = new BusinessObject[] { dummy };
			var template = Factory.New<StmNoteTemplate>();
			using (var form = new TextTemplateForm(template, context, true))
			{
				form.Show();
				form.insertMacroButton.PerformClick();

				AssertEquals("MapTreePresentationManager ParentBusinessObjects should not null", context, form.mapTreePresenter.ParentBusinessObjects);
			}
		}

		public void TestInsertMacroShouldEscapeAllSpecialCharacters()
		{
			var context = Factory.New<DummyBusinessObject>();
			var template = Factory.New<StmNoteTemplate>();
			bool shouldEscapeAllSpecialCharacters = false;

			var mock = new Mock<IMapTreePresentationManager>();
			mock.Setup(p => p.ShowPresentationManagerForm(It.IsAny<bool>(), It.IsAny<bool>()))
				.Callback((bool p1, bool p2) =>
				{
					shouldEscapeAllSpecialCharacters = p2;
				});

			using (var form = new TextTemplateForm(template, new BusinessObject[] { context }, true, true))
			{
				form.Show();
				form.mapTreePresenter = mock.Object;
				form.insertMacroButton.PerformClick();

				AssertEquals(true, shouldEscapeAllSpecialCharacters);
			}

			using (var form = new TextTemplateForm(template, new BusinessObject[] { context }, true))
			{
				form.Show();
				form.mapTreePresenter = mock.Object;
				form.insertMacroButton.PerformClick();

				AssertEquals(false, shouldEscapeAllSpecialCharacters);
			}
		}

		public void TestPreviewSpecialCharactersShouldBeUnescaped()
		{
			var context = Factory.New<DummyBusinessObject>();
			context.Z0_Description = @"<\"">";
			var template = Factory.New<StmNoteTemplate>();
			template.S8_TemplateText = @"""<Z0_Description>""==""\<\""\>""";

			using (var form = new TextTemplateForm(template, new BusinessObject[] { context }, true))
			{
				form.Show();
				form.previewButton.PerformClick();
				AssertEquals(@"""<\"">""==""<\"">""", form.previewForm.textBox.Text);
			}

			context.Z0_Description = @"\\abc";
			template.S8_TemplateText = @"""<Z0_Description>"" == ""\\\\abc""";

			using (var form = new TextTemplateForm(template, new BusinessObject[] { context }, true, true))
			{
				form.Show();
				form.previewButton.PerformClick();
				var expectedText = @"""\\abc"" == ""\\abc""";

				AssertEquals(expectedText, form.previewForm.textBox.Text);
			}
		}

		public void TestDeleteButton()
		{
			var template = Factory.New<StmNoteTemplate>();
			template.S8_ContextID = "delete";
			template.S8_Description = "me";
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = new TextTemplateForm(template, new BusinessObject[] { Factory.New<DummyBusinessObject>() }, true))
			{
				form.Show();
				form.deleteButton.PerformClick();
				AssertEquals(false, form.Visible);
				AssertEquals(true, template.IsDeleted);
			}
		}

		public void TestDeleteButton_ConcurrentDelete()
		{
			var template = Factory.New<StmNoteTemplate>();
			Factory.RefreshEnabled = false;
			template.S8_ContextID = "delete";
			template.S8_Description = "me";
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = new TextTemplateForm(template, new BusinessObject[] { Factory.New<DummyBusinessObject>() }, true))
			{
				form.Show();
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var templateInFactory2 = factory2.Load<StmNoteTemplate>(template.PK);
				templateInFactory2.Delete();
				factory2.Save();
				AssertEquals(false, template.IsDeleted);
				form.deleteButton.PerformClick();

				AssertEquals("This record has already been deleted. The form will now be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, template.IsDeleted);
			}
		}

		public void TestPreviewButton()
		{
			var context = Factory.New<DummyBusinessObject>();
			context.Z0_Number = 2;
			var template = Factory.New<StmNoteTemplate>();
			template.S8_TemplateText = "It takes <Z0_Number> to make a thing go right.";
			using (var form = new TextTemplateForm(template, new BusinessObject[] { context }, true))
			{
				form.Show();
				form.previewButton.PerformClick();
				AssertEquals("It takes 2 to make a thing go right.", form.previewForm.textBox.Text);
			}
		}

		public void TestPublicCheckBoxCheckChanging_ShouldNotAffectAllCompaniesEnabledProperty()
		{
			using (var form = new TextTemplateForm(Factory.New<StmNoteTemplate>(), new BusinessObject[] { Factory.New<DummyBusinessObject>() }, true))
			{
				form.Show();
				form.publicCheckbox.Checked = true;
				Assert(form.allCompaniesCheckbox.Enabled);
				form.publicCheckbox.Checked = false;
				Assert(form.allCompaniesCheckbox.Enabled);
			}
		}

		public void TestGetRootTypeFromIRootTypeProvider()
		{
			var template = Factory.New<StmNoteTemplate>();
			using (var form = new TextTemplateForm(template, new BusinessObject[] { Factory.New<DummyBusinessObjectWithIRootTypeProvider>(), Factory.New<DummyBusinessObjectWithIRootTypeProvider>() }, true))
			{
				form.Show();
				form.insertMacroButton.PerformClick();
				var types = form.mapTreePresenter.ParentTypes;
				AssertEquals("Gets 2 parent types. There should be no duplicate", 2, types.Length);
				AssertEquals(typeof(DummyBusinessObjectWithIRootTypeProvider), types[0]);
				AssertEquals(typeof(DummyBusinessObject), types[1]);
			}

			using (var form = new TextTemplateForm(template, new BusinessObject[] { Factory.New<DummyBusinessObject>(), Factory.New<DummyBusinessObject>() }, true))
			{
				form.Show();
				form.insertMacroButton.PerformClick();
				var types = form.mapTreePresenter.ParentTypes;
				AssertEquals("Gets 1 parent type. There should be no duplicate", 1, types.Length);
				AssertEquals(typeof(DummyBusinessObject), types[0]);
			}
		}

		public void TestGetRootType_RootsAreNotStatic()
		{
			var template = Factory.New<StmNoteTemplate>();
			using (var form = new TextTemplateForm(template, new BusinessObject[] { Factory.New<DummyBusinessObjectWithIRootTypeProvider>(), Factory.New<DummyBusinessObjectWithIRootTypeProvider>() }, true))
			{
				form.Show();
				DummyBusinessObjectWithIRootTypeProvider.RootTypes_Override.Value = new[] { typeof(StmNoteTemplate) };
				form.insertMacroButton.PerformClick();
				var types = form.mapTreePresenter.ParentTypes;
				AssertContainsExactElementsInAnyOrder(DummyBusinessObjectWithIRootTypeProvider.RootTypes_Override.Value.Append(typeof(DummyBusinessObjectWithIRootTypeProvider)), form.mapTreePresenter.ParentTypes);

				DummyBusinessObjectWithIRootTypeProvider.RootTypes_Override.Value = new[] { typeof(DummyBusinessObject) };
				form.insertMacroButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(DummyBusinessObjectWithIRootTypeProvider.RootTypes_Override.Value.Append(typeof(DummyBusinessObjectWithIRootTypeProvider)), form.mapTreePresenter.ParentTypes);
			}
		}

		public void TestCheckBoxHeight()
		{
			var template = Factory.New<StmNoteTemplate>();
			using (var form = new TextTemplateForm(template, new BusinessObject[] { Factory.New<DummyBusinessObject>() }, true))
			{
				var text = "TEXT";
				var g = form.publicCheckbox.CreateGraphics();
				Assert("Checkbox height should not be smaller than two lines of text", form.publicCheckbox.Height >= (int)Math.Ceiling(g.MeasureString(text, form.publicCheckbox.Font).Height * 2));
			}
		}

		#region Implementation

		public class DummyBusinessObjectWithIRootTypeProvider : DummyBusinessObject, IRootTypeProvider
		{
			public DummyBusinessObjectWithIRootTypeProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public static readonly Overridable<Type[]> RootTypes_Override = new Overridable<Type[]>(null);

			Type[] IRootTypeProvider.RootTypes
			{
				get => RootTypes_Override.Value ?? new Type[] { typeof(DummyBusinessObjectWithIRootTypeProvider), typeof(DummyBusinessObject) };
			}

			BusinessObject[] IRootTypeProvider.Roots
			{
				get => new BusinessObject[] { this };
			}
		}

		#endregion
	}
}
