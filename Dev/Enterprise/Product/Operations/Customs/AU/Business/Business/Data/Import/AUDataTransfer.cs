using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUDataTransfer : DataTransferImpl
	{
		public AUDataTransfer()
			: base()
		{
		}

		protected override FlatFileInvoiceDataImporter GetFlatFileImporter(string fileName, Customs.Business.BaseJobDeclaration toJobDec)
		{
			return new AUFlatFileInvoiceDataImporter(fileName, toJobDec);
		}
	}
}
