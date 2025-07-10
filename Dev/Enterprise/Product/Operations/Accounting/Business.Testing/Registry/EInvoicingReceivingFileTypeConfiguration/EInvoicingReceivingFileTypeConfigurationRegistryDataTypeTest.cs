using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EInvoicingReceivingFileTypeConfigurationRegistryDataType))]
	public class EInvoicingReceivingFileTypeConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EInvoicingReceivingFileTypeConfigurationRegistryDataType>
	{
		protected override EInvoicingReceivingFileTypeConfigurationRegistryDataType GetNewDataType()
		{
			return new EInvoicingReceivingFileTypeConfigurationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var collection = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration { FileFormat = "OFD", DebtorType = "ORG", DebtorCode =  testObjectCreator.Debtor.PK },
			};

			var collection1 = new EInvoicingReceivingFileTypeConfigurationCollection
			{
				new EInvoicingReceivingFileTypeConfiguration { FileFormat = "OFD", DebtorType = "GRP", DebtorCode =  new ZGuid("3A57AF93-D305-4B62-8B5A-6E94F22A766A") },
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new EInvoicingReceivingFileTypeConfigurationRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection1, new EInvoicingReceivingFileTypeConfigurationRegistryDataType().Serialise(collection1))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "EInvoicingReceivingFileTypeConfigurationRegistryItemEditor"; }
		}
	}
}
