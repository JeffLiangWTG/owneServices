using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType))]
	public class ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType>
	{
		protected override ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType GetNewDataType()
		{
			return new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var data = new ExcludedFullyDigitalizedElectronicInvoiceData
			{
				BuyerAddress = true
			};

			var data2 = new ExcludedFullyDigitalizedElectronicInvoiceData
			{
				BuyerBankAccount = true,
				BuyerPhoneNumber = true
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(data, new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType().Serialise(data)),
				new ValidSampleAndBinaryValueInDB(data2, new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType().Serialise(data2))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItemEditor"; }
		}
	}
}
