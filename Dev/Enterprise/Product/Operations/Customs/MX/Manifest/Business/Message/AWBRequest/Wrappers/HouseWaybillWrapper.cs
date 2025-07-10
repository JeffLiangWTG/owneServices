using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class HouseWaybillWrapper : IHouseWaybill
	{
		public HouseWaybillWrapper(AsycudaBill bill, ZString action)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.action = action;
		}
		readonly AsycudaBill bill;
		readonly ZString action;

		IMessageHeaderDocument IHouseWaybill.MessageHeader => messageHeader ?? (messageHeader = new MessageHeaderDocumentWrapper(bill, action));
		IMessageHeaderDocument messageHeader;

		IBusinessHeaderDocument IHouseWaybill.BusinessHeader => businessHeader ?? (businessHeader = new BusinessHeaderDocumentWrapper(bill));
		IBusinessHeaderDocument businessHeader;

		IMasterConsignment IHouseWaybill.MasterConsignment => masterConsignment ?? (masterConsignment = new MasterConsignmentWrapper(bill));
		IMasterConsignment masterConsignment;
	}
}
