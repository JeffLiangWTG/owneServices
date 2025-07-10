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
	[TestedType(typeof(CodeDescriptionWithGroupRegistryItemEditor))]
	sealed class CodeDescriptionWithGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestNewBoundWinFormsEditorPane()
		{
			var mock = new Mock<ICondition>();
			var editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"ThisIsTheCaption", mock.Object);

			var editor = new CodeDescriptionWithGroupRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			mock.Setup(m => m.IsMet).Returns(true);

			var flags = BindingFlags.NonPublic | BindingFlags.Instance;

			using (var editorPane = (CodeDescriptionWithGroupControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.GroupColumnCaption", "ThisIsTheCaption", typeof(CodeDescriptionWithGroupControl).GetField("GroupColumnCaption", flags).GetValue(editorPane));
				AssertEquals("EditorPane.IsGroupColumnVisible", true, typeof(CodeDescriptionWithGroupControl).GetField("IsGroupColumnVisible", flags).GetValue(editorPane));
				AssertEquals("EnglishDescription", editorPane.descriptionColumnStyleInfo.ColumnName);
			}

			mock.VerifyAll();
			mock.Setup(m => m.IsMet).Returns(false);

			using (var editorPane = (CodeDescriptionWithGroupControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.IsGroupColumnVisible", false, typeof(CodeDescriptionWithGroupControl).GetField("IsGroupColumnVisible", flags).GetValue(editorPane));
			}

			mock.VerifyAll();

			AssertEditorPaneDescriptionColumnTranslatable(true, "Description");
			AssertEditorPaneDescriptionColumnTranslatable(false, "EnglishDescription");
		}

		void AssertEditorPaneDescriptionColumnTranslatable(bool isDescriptionColumnTranslatable, string descriptionColumnName)
		{
			var editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"ThisIsTheCaption", true, true, isDescriptionColumnTranslatable);
			var editor = new CodeDescriptionWithGroupRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			using (var editorPane = (CodeDescriptionWithGroupControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals(descriptionColumnName, editorPane.descriptionColumnStyleInfo.ColumnName);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeDescriptionWithGroupRegistryItemEditor(RegistryItem.DataType, RegistryItem.EditorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionWithGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionWithGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionWithGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, (NoResString)"");
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CodeDescriptionWithGroupCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
