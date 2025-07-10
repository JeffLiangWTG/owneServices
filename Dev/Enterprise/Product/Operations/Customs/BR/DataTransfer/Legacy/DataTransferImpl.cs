namespace Enterprise.Customs.BR.DataTransfer
{
	public class DataTransferImpl : Customs.DataTransfer.DataTransferImpl
	{
		protected override Customs.DataTransfer.FlatFileInvoiceDataImporter GetFlatFileImporter(string fileName, Customs.Business.BaseJobDeclaration toJobDec)
		{
			return new BRFlatFileInvoiceDataImporter(fileName, toJobDec);
		}
	}
}
