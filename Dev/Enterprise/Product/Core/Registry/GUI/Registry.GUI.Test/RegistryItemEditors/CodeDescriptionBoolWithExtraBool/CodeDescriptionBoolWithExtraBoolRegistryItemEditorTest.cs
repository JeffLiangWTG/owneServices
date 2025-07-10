using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithExtraBoolRegistryItemEditor))]
	public class CodeDescriptionBoolWithExtraBoolRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeDescriptionBoolWithExtraBoolRegistryItemEditor(RegistryItem.DataType, RegistryItem.EditorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionBoolWithExtraBoolControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionBoolWithExtraBoolControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(null, true));
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Bool = true, Bool2 = true, Code = "AAA", Description = (NoResString)"bbb" } };
			collection.SetDefaultCode("AAA", true);

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		public void TestColumnNames_SecondOnly()
		{
			CombineAssertions(() =>
			{
				Test("secondColumnName");
				Test("name2");
			});

			void Test(string secondColumnName)
			{
				// Arrange
				var regItemEditor = new CodeDescriptionBoolWithExtraBoolRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory, secondColumnName);
				var editorInfo = regItemEditor.EditorInfo;

				// Act
				var result = new
				{
					firstName = editorInfo.BoolColumnCaption,
					secondName = editorInfo.Bool2ColumnCaption,
				};

				// Assert
				AssertEquals("Default", result.firstName);
				AssertEquals(secondColumnName, result.secondName);
			}
		}

		public void TestColumnNames_Both()
		{
			CombineAssertions(() =>
			{
				Test("firstColumnName", "secondColumnName");
				Test("name1", "name2");
			});

			void Test(string firstColumnName, string secondColumnName)
			{
				// Arrange
				var regItemEditor = new CodeDescriptionBoolWithExtraBoolRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory, firstColumnName, secondColumnName);
				var editorInfo = regItemEditor.EditorInfo;

				// Act
				var result = new
				{
					firstName = editorInfo.BoolColumnCaption,
					secondName = editorInfo.Bool2ColumnCaption,
				};

				// Assert
				AssertEquals(firstColumnName, result.firstName);
				AssertEquals(secondColumnName, result.secondName);
			}
		}

		#endregion
	}
}
