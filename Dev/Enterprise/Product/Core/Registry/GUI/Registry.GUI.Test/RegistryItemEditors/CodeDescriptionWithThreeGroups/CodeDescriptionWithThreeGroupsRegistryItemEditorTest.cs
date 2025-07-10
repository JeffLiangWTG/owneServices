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
	[TestedType(typeof(CodeDescriptionWithThreeGroupsRegistryItemEditor))]
	sealed class CodeDescriptionWithThreeGroupsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestNewBoundWinFormsEditorPane()
		{
			var mock = new Mock<ICondition>();

			var editorInfo = new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"ThisIsTheCaption", (NoResString)"ThisIsTheCaption2", (NoResString)"ThisIsTheCaption3", (NoResString)"ThisIsTheCaptionExtra", (NoResString)"ThisIsTheCaptionMainDescr", mock.Object, true, false, false);

			var editor = new CodeDescriptionWithThreeGroupsRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			mock.Setup(m => m.IsMet).Returns(true);

			var flags = BindingFlags.NonPublic | BindingFlags.Instance;

			using (var editorPane = (CodeDescriptionWithThreeGroupsControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.GroupColumnCaption", "ThisIsTheCaption", typeof(CodeDescriptionWithThreeGroupsControl).GetField("GroupColumnCaption", flags).GetValue(editorPane));
				AssertEquals("EditorPane.Group2ColumnCaption", "ThisIsTheCaption2", typeof(CodeDescriptionWithThreeGroupsControl).GetField("Group2ColumnCaption", flags).GetValue(editorPane));
				AssertEquals("EditorPane.Group3ColumnCaption", "ThisIsTheCaption3", typeof(CodeDescriptionWithThreeGroupsControl).GetField("Group3ColumnCaption", flags).GetValue(editorPane));
				AssertEquals("EditorPane.ExtraDescriptionColumnCaption", "ThisIsTheCaptionExtra", typeof(CodeDescriptionWithThreeGroupsControl).GetField("ExtraDescriptionColumnCaption", flags).GetValue(editorPane));
				AssertEquals("EditorPane.MainDescriptionColumnCaption", "ThisIsTheCaptionMainDescr", typeof(CodeDescriptionWithThreeGroupsControl).GetField("MainDescriptionColumnCaption", flags).GetValue(editorPane));
				AssertEquals("EditorPane.AreGroupColumnsVisible", true, typeof(CodeDescriptionWithThreeGroupsControl).GetField("AreGroupColumnsVisible", flags).GetValue(editorPane));
			}

			mock.VerifyAll();
			mock.Setup(m => m.IsMet).Returns(false);

			using (var editorPane = (CodeDescriptionWithThreeGroupsControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.AreGroupColumnsVisible", false, typeof(CodeDescriptionWithThreeGroupsControl).GetField("AreGroupColumnsVisible", flags).GetValue(editorPane));
			}

			mock.VerifyAll();
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeDescriptionWithThreeGroupsRegistryItemEditor(RegistryItem.DataType, RegistryItem.EditorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionWithThreeGroupsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionWithThreeGroupsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetterDefaultValue = (companyPK, branchPK, departmentPK) => new CodeDescriptionWithThreeGroupsCollection(8);

			return new CodeDescriptionWithThreeGroupsRegistryItem(
				"", null, null, null, 8, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport,
				new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Group1", (NoResString)"Group2", (NoResString)"Group3", (NoResString)"ExtraDescription", (NoResString)"MainDescription", null, true, false, false),
				valueGetterDefaultValue,
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CodeDescriptionWithThreeGroupsCollection(17) };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
