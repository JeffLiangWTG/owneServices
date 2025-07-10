using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class ConsigneeTradeContactWrapper : ITradeContact
	{
		internal ConsigneeTradeContactWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string ITradeContact.PersonName => ARHelperClass.GetStaffAssignmentName(bill.Consignee?.Header?.StaffAssignments);

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => bill.ABL_ConsigneePhone;

		string ITradeContact.FaxCommunicationCompleteNumber => bill.Consignee?.OA_Fax ?? ZString.Empty;

		string ITradeContact.EmailCommunicationID => bill.Consignee?.OA_Email ?? ZString.Empty;
	}
}
