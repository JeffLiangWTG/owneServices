using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderAddInfoValidation : AUAddInfoValidation
	{
		public CusEntryHeaderAddInfoValidation(CusEntryHeaderAddInfo parent)
			: base(parent)
		{
		}

		protected new CusEntryHeaderAddInfo Parent
		{
			get { return (CusEntryHeaderAddInfo)base.Parent; }
		}

		protected override void CheckZA_WarehouseNumberOfPacks_Hidden()
		{
			base.CheckZA_WarehouseNumberOfPacks_Hidden();
			if (Parent.EntryHeader.IsNature30)
			{
				if (Parent.EntryHeader.Declaration != null)
				{
					JobDeclaration declaration = Parent.EntryHeader.Declaration;

					if ((declaration.CustomsEntryHeaders.Count > 1 || (declaration.CustomsEntryHeaders.Count == 1 && declaration.JE_TotalNoOfPacks.IsEmpty))
						&& Parent.ZA_WarehouseNumberOfPacks_Hidden.IsEmpty)
					{
						Parent.ZA_WarehouseNumberOfPacks_HiddenInfo.AddMessageError("Please enter the Warehouse Number of Packs");
					}
				}
			}
		}

		protected override void CheckZA_RRC_Hidden()
		{
			base.CheckZA_RRC_Hidden();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_RRC_HiddenInfo, Parent.Lookups.ZA_RRC_List);
			if (Parent.EntryHeader != null)
			{
				if (!Parent.ZA_RRC_Hidden.IsEmpty)
				{
					if (!Parent.EntryHeader.IsCustomsChargePaid)
					{
						Parent.ZA_RRC_HiddenInfo.AddMessageError("This entry is not paid yet and so a refund cannot occur.");
					}
					else if (Parent.EntryHeader.Questions.IsGoodsDeliveredQuestionAnsweredNo &&
						Parent.ZA_RRC_Hidden != DeletedLineAmendment.RefundReasonForDeletedLines)
					{
						Parent.ZA_RRC_HiddenInfo.AddMessageError("According to declaration questions, goods are not delivered and the refund reason code should be '126A' prior to delivery.");
					}
					else if (!Parent.EntryHeader.IsARefundDue && Parent.EntryHeader.TotalDeferredDutyFromCustoms.IsEmpty)
					{
						Parent.ZA_RRC_HiddenInfo.AddWarning("The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
					}
				}
			}
		}
	}
}
