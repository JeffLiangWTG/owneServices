using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class AssociatedTradeContactWrapper : ITradeContact
	{
		internal AssociatedTradeContactWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string ITradeContact.PersonName => ARHelperClass.GetStaffAssignmentName(bill.NotifyParty?.Header?.StaffAssignments);

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => bill.ABL_NotifyPartyPhone;

		string ITradeContact.FaxCommunicationCompleteNumber => bill.NotifyParty?.OA_Fax ?? ZString.Empty;

		string ITradeContact.EmailCommunicationID => bill.NotifyParty?.OA_Email ?? ZString.Empty;
	}
}
