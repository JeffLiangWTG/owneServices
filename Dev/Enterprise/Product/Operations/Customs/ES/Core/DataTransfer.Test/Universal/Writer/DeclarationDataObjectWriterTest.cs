using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.DataTransfer.Universal.Testing
{
	class DeclarationDataObjectWriterTest : DeclarationDataObjectWriterAbstractTest<JobDeclaration, DeclarationDataObjectWriter>
	{
		public void TestCommercialInvoiceHeaderDataObjectWriterType()
		{
			((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			var declaration = Factory.BOFactory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
			writer.GetDataObject(declaration);
			AssertType<CommercialInvoiceHeaderDataObjectWriter>("CommercialInvoiceHeader writer type", writer.GetNewCommercialInvoiceHeaderDataObjectWriterExposed(entryHeader));
		}
	}

	class DeclarationDataObjectWriterForTest : DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
		{
		}

		public Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriterExposed(Customs.Business.CusEntryHeader relatedEntry) => GetNewCommercialInvoiceHeaderDataObjectWriter(relatedEntry);
	}
}
