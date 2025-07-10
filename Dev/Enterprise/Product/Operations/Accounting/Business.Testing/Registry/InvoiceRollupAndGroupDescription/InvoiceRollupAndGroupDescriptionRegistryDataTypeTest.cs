using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceRollupAndGroupDescriptionRegistryDataType))]
	class InvoiceRollupAndGroupDescriptionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InvoiceRollupAndGroupDescriptionRegistryDataType>
	{
		protected override InvoiceRollupAndGroupDescriptionRegistryDataType GetNewDataType()
		{
			return new InvoiceRollupAndGroupDescriptionRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InvoiceRollupAndGroupDescriptionRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new InvoiceRollupAndGroupDescriptionCollection();
			byte[] byteArrayValue1 = new InvoiceRollupAndGroupDescriptionRegistryDataType().Serialise(sample1);
			var sample2 = AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.Value;
			byte[] byteArrayValue2 = new InvoiceRollupAndGroupDescriptionRegistryDataType().Serialise(sample2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(sample2, byteArrayValue2)
			};
		}
	}
}
