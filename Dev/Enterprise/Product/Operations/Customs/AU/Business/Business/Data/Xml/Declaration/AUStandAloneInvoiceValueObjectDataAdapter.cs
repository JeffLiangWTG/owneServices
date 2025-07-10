using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUStandAloneInvoiceValueObjectDataAdapter : StandAloneInvoiceValueObjectDataAdapter, Integration.Customs.AU.IAUStandAloneInvoiceValueObjectDataAdapter
	{
		protected override string AddInfoPrefix
		{
			get { return "ZA_"; }
		}

		protected override bool ShouldFieldBeExported(ZPropertyInfo info)
		{
			return info.Name.StartsWith(AddInfoPrefix) && base.ShouldFieldBeExported(info);
		}

		protected override InvoiceDataTransferTool GetInvoiceDataTransferTool()
		{
			return new AUInvoiceDataTransferTool(true, AddInfoDataTransferTool);
		}
	}
}
