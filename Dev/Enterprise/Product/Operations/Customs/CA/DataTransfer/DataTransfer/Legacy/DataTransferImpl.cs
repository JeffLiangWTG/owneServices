
namespace Enterprise.Customs.CA.DataTransfer
{
	public class DataTransferImpl : Customs.DataTransfer.DataTransferImpl
	{
		protected override Customs.DataTransfer.FlatFileInvoiceDataImporter GetFlatFileImporter(string fileName, Customs.Business.BaseJobDeclaration toJobDec)
		{
			return new CAFlatFileInvoiceDataImporter(fileName, toJobDec);
		}
	}
}
