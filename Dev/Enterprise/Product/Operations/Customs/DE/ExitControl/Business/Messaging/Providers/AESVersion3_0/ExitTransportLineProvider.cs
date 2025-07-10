using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class ExitTransportLineProvider : IExitTransportLine
	{
		public ExitTransportLineProvider(CusExitReportItem reportItem)
		{
			this.reportItem = Argument.NotNull(reportItem, nameof(reportItem));
		}
		protected readonly CusExitReportItem reportItem;

		public string SequenceNumber => ConsignmentItem.CCI_LineNumber.ToString();

		protected CusExitConsignmentItem ConsignmentItem => consignmentItem ?? (consignmentItem = (CusExitConsignmentItem)reportItem.ConsignmentItem);
		CusExitConsignmentItem consignmentItem;
	}
}
