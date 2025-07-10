using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC141CProvider : NctsDepartureHeaderProvider, ICC141C
	{
		readonly MessageSendingAction action;
		public CC141CProvider(MessageSendingAction sendingAction) : base(Argument.NotNull(sendingAction, nameof(sendingAction)).Header)
		{
			action = sendingAction;
		}

		public override string CustomsOfficeOfDestination => !action.QueryInformation.IsEmpty ? action.ActualOfficeOfDestination.ToString() : null;

		public string CustomsOfficeOfEnquiryAtDeparture => nctsHeader.MovementHeader.EnquiryCustomsOfficeCode;

		public DateTime? EnquiryTC11DeliveryDate => action.TCI11.IsValid ? action.TCI11.ToDateTime() : null;

		public string EnquiryText => !action.QueryInformation.IsEmpty
			? (string)action.QueryInformation
			: null;

		public IParty ConsignmentConsignee => CachedValueHelper.GetValue(ref consignmentConsignee, () => TCIDocumentIsFilled ? new CC141CConsigneeProvider(action.ActualConsignee) : null);
		CachedValue<IParty> consignmentConsignee;

		public override string MessageType => Constants.MessageTypes.CC141C;

		bool TCIDocumentIsFilled => !action.TCI11.IsEmpty || !action.QueryInformation.IsEmpty;
	}
}
