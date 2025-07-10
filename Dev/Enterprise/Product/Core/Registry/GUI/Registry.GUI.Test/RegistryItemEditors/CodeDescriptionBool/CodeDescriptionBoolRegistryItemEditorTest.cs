using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRegistryItemEditor))]
	sealed class CodeDescriptionBoolRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestNewBoundWinFormsEditorPane()
		{
			var mock = new Mock<ICondition>();
			var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"ThisIsTheCaption", mock.Object);

			var editor = new CodeDescriptionBoolRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			mock.Setup(m => m.IsMet).Returns(true);

			var flags = BindingFlags.NonPublic | BindingFlags.Instance;

			using (var editorPane = (CodeDescriptionBoolControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.BoolColumnCaption", "ThisIsTheCaption", typeof(CodeDescriptionBoolControl).GetField("BoolColumnCaption", flags).GetValue(editorPane));
				AssertEquals("EditorPane.IsBoolColumnVisible", true, typeof(CodeDescriptionBoolControl).GetField("IsBoolColumnVisible", flags).GetValue(editorPane));
				AssertEquals("EnglishDescription", editorPane.descriptionColumnStyleInfo.ColumnName);
			}

			mock.VerifyAll();
			mock.Setup(m => m.IsMet).Returns(false);

			using (var editorPane = (CodeDescriptionBoolControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.IsBoolColumnVisible", false, typeof(CodeDescriptionBoolControl).GetField("IsBoolColumnVisible", flags).GetValue(editorPane));
			}

			mock.VerifyAll();

			AssertEditorPaneDescriptionColumnTranslatable(true, "Description");
			AssertEditorPaneDescriptionColumnTranslatable(false, "EnglishDescription");
		}

		void AssertEditorPaneDescriptionColumnTranslatable(bool isDescriptionColumnTranslatable, string descriptionColumnName)
		{
			var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"ThisIsTheCaption", true, true, isDescriptionColumnTranslatable);
			var editor = new CodeDescriptionBoolRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			using (var editorPane = (CodeDescriptionBoolControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals(descriptionColumnName, editorPane.descriptionColumnStyleInfo.ColumnName);
			}
		}

		public void TestSetupCodeColumnWithCorrectValue()
		{
			var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"ThisIsTheCaption", null, true, true, true);
			var editor = new CodeDescriptionBoolRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			using (var editorPane = (CodeDescriptionBoolControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("IsCodeColumnVisible is true", true, editorPane.IsCodeColumnVisible);
			}

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"ThisIsTheCaption", null, true, true, false);
			editor = new CodeDescriptionBoolRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			using (var editorPane = (CodeDescriptionBoolControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("IsCodeColumnVisible is false", false, editorPane.IsCodeColumnVisible);
			}
		}

		public void TestSetupCodeColumnWithCorrectValue_CodeColumnCaption()
		{
			var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"ThisIsTheCaption", (NoResString)"CodeCaption", null, true, true, true);
			var editor = new CodeDescriptionBoolRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			using (var editorPane = (CodeDescriptionBoolControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("CodeColumnCaption", "CodeCaption", typeof(CodeDescriptionBoolControl).GetField("CodeColumnCaption", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editorPane));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeDescriptionBoolRegistryItemEditor(RegistryItem.DataType, RegistryItem.EditorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionBoolControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionBoolControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionBoolRegistryItem("", null, null, null, RegistryStorageFlags.System, (NoResString)"");
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CodeDescriptionBoolCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
