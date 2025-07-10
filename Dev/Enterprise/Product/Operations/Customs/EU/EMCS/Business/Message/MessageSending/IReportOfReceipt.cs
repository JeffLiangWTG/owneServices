using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public interface IReportOfReceipt
	{
		ZDateTime ArrivalDate { get; }
		ZString ReceiptResult { get; }
		ZString ComplementaryInformation { get; }
	}
}
