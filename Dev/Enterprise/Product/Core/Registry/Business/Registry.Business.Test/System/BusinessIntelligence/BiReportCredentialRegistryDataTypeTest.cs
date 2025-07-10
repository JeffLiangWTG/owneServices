using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BiReportCredentialRegistryDataType))]
	sealed class BiReportCredentialRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BiReportCredentialRegistryDataType>
	{
		protected override BiReportCredentialRegistryDataType GetNewDataType()
		{
			return new BiReportCredentialRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new BiReportCredential();
			var second = new BiReportCredential { Domain = "XYZ", UserName = "123", Password = "456" };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, DataType.Serialise(first)),
				new ValidSampleAndBinaryValueInDB(second, DataType.Serialise(second)),
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "BiReportCredentialRegistryItemEditor"; }
		}
		protected override bool HasEditor
		{
			get { return true; }
		}
	}
}
