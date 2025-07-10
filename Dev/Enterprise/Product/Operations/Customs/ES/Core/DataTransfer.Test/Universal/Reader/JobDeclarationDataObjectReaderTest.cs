using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.DataTransfer.Universal.Testing
{
	class JobDeclarationDataObjectReaderTest : JobDeclarationDataObjectReaderAbstractTest<JobDeclaration, JobDeclarationDataObjectReader>
	{
		public void TestCommercialInvoiceHeaderDataObjectReaderType()
		{
			var declaration = Factory.BOFactory.New<JobDeclaration>();
			var declarationData = new Shipment();
			var declarationDataObjectReader = new JobDeclarationDataObjectReaderForTest(declarationData, new TestErrorLogger(), Factory);
			AssertType<CommercialInvoiceHeaderDataObjectReader>("CommercialInvoiceHeader reader type", declarationDataObjectReader.CreateNewCommercialInvoiceHeaderDataObjectReaderExposed(declaration.JobComInvoiceGroupHeaders[0], new CommercialInvoiceHeader(), declarationData, null));
		}
	}

	class JobDeclarationDataObjectReaderForTest : JobDeclarationDataObjectReader
	{
		public JobDeclarationDataObjectReaderForTest(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(declarationDataObject, logger, factory, forwardingShipment: null)
		{
		}

		public Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<EU.Business.Declaration.JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReaderExposed(EU.Business.Declaration.JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return CreateNewCommercialInvoiceHeaderDataObjectReader(groupHeader, invoiceData, dataObject, landedCostDataReader);
		}
	}
}
