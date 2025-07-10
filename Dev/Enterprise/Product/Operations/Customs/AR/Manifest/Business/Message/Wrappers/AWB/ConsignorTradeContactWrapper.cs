using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class ConsignorTradeContactWrapper : ITradeContact
	{
		internal ConsignorTradeContactWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string ITradeContact.PersonName => ARHelperClass.GetStaffAssignmentName(bill.Shipper?.Header?.StaffAssignments);

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => bill.ABL_ShipperPhone;

		string ITradeContact.FaxCommunicationCompleteNumber => bill.Shipper?.OA_Fax ?? ZString.Empty;

		string ITradeContact.EmailCommunicationID => bill.Shipper?.OA_Email ?? ZString.Empty;
	}
}
