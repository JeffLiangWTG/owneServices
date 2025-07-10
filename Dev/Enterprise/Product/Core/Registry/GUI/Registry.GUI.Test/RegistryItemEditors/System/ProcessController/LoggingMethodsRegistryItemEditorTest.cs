using System;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.RegistryItemEditors.System.ProcessController.Testing
{
	[TestedType(typeof(LoggingMethodsRegistryItemEditor))]
	sealed class LoggingMethodsRegistryItemEditorTest : CodeDescriptionBoolWithExtraBoolRegistryItemEditorTest
	{
		public void TestColumnNames()
		{
			// Arrange
			var regItemEditor = new LoggingMethodsRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

			// Act
			var result = regItemEditor.EditorInfo;

			// Assert
			AssertEquals("Read from", result.BoolColumnCaption);
			AssertEquals("Write to", result.Bool2ColumnCaption);
		}

		protected override RegistryItemEditor GetEditor() => new LoggingMethodsRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
	}
}
