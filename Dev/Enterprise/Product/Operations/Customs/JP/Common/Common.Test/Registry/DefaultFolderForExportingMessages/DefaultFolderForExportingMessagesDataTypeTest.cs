using System;
using System.IO;
using System.Text;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(DefaultFolderForExportingMessagesDataType))]
	sealed class DefaultFolderForExportingMessagesDataTypeTest : RegistryDataTypeTestCase<DefaultFolderForExportingMessagesDataType>
	{
		protected override DefaultFolderForExportingMessagesDataType GetNewDataType() => new DefaultFolderForExportingMessagesDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples() => new[]
		{
			new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
			new ValidSampleAndBinaryValueInDB(Directory.GetCurrentDirectory(), Encoding.Unicode.GetBytes(Directory.GetCurrentDirectory()))
		};

		protected override object GetNullRepresentation()
		{
			return Encoding.Unicode.GetBytes("*** NULL ***");
		}

		protected override object[] GetInvalidSamples() => new object[]
		{
			"ABC",
		};

		public void TestValidateBeforeRegistryFormSaveCore()
		{
			AssertExceptionThrown<RegistryValidationException>(
				"Messages for invalid directory",
				"Please select a valid directory path as default export folder path.",
				() => JPRegistry.Instance.DefaultFolderForExportingMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
					"ABC")
			);
		}
	}
}
