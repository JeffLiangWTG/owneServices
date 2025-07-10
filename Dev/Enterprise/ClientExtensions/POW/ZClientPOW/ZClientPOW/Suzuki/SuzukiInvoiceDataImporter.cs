using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Client.ZClientPOW.Suzuki
{
	public class SuzukiInvoiceDataImporter : FlatFileInvoiceDataImporter
	{
		public SuzukiInvoiceDataImporter(string fileName, BaseJobDeclaration toJobDec) : base(fileName, toJobDec)
		{
		}

		protected override void SetDataReader()
		{
			dataReader = new SuzukiInvoiceDataFileReader(FileName);
		}
	}
}
