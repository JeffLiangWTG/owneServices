//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPayableOrderHeaderValidation
//
//    This class should be used for overriding validation in AutoAccPayableOrderHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.PayableOrder
{
	using CargoWise.EntityFramework;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;

	public class AccPayableOrderHeaderValidation : AutoAccPayableOrderHeaderValidation
	{
		public AccPayableOrderHeaderValidation(AutoAccPayableOrderHeader parent) : base(parent)
		{
		}

		public new AccPayableOrderHeader Parent
		{
			get { return (AccPayableOrderHeader)base.Parent; }
		}

		public void ValidateSupplierPK(JobDocAddressValidation validation)
		{
			if (validation.Parent.OrganisationPK.IsValid)
			{
				var organization = validation.Parent.Factory.Load<OrgHeader>(validation.Parent.OrganisationPK);
				// TO DO ??? Do we need to add validation logic here?				
			}
		}

		#region Buyer and OrderNumber and Split is Unique

		protected BuyerAndOrderNumAndSplitIsUniqueValidation BuyerAndOrderNumAndSplitIsUniqueValidation
		{
			get
			{
				if (fBuyerAndOrderNumAndSplitIsUniqueValidation == null)
				{
					fBuyerAndOrderNumAndSplitIsUniqueValidation = new BuyerAndOrderNumAndSplitIsUniqueValidation(Parent);
				}
				return fBuyerAndOrderNumAndSplitIsUniqueValidation;
			}
		}

		BuyerAndOrderNumAndSplitIsUniqueValidation fBuyerAndOrderNumAndSplitIsUniqueValidation;

		#endregion

		protected override void CheckAPH_OA_Buyer()
		{
			base.CheckAPH_OA_Buyer();
			BuyerAndOrderNumAndSplitIsUniqueValidation.CheckBuyerAndOrderNumAndSplitIsUnique(Parent.APH_OA_BuyerInfo);
		}

		protected override void CheckAPH_OrderNumber()
		{
			base.CheckAPH_OrderNumber();
			OrderNumberValidation.ErrorIfOrderNumberNotValid(Parent.APH_OrderNumberInfo);
			BuyerAndOrderNumAndSplitIsUniqueValidation.CheckBuyerAndOrderNumAndSplitIsUnique(Parent.APH_OrderNumberInfo);
		}

		protected override void CheckAPH_OrderNumberSplit()
		{
			base.CheckAPH_OrderNumberSplit();
			ValidateAPH_OrderNumber();
			BuyerAndOrderNumAndSplitIsUniqueValidation.CheckBuyerAndOrderNumAndSplitIsUnique(Parent.APH_OrderNumberSplitInfo);
		}

		protected override void CheckAPH_Stage()
		{
			base.CheckAPH_Stage();
			ListValidation.ErrorIfInvalidCode(Parent.APH_StageInfo, Parent.APH_Stage_List);
			MandatoryValidation.WarnIfNotEntered(Parent.APH_StageInfo);
		}

		protected override void CheckAPH_Disposition()
		{
			base.CheckAPH_Disposition();
			ListValidation.ErrorIfInvalidCode(Parent.APH_DispositionInfo, Parent.APH_Disposition_List);
			MandatoryValidation.WarnIfNotEntered(Parent.APH_DispositionInfo);
		}

		protected override void CheckAPH_Type()
		{
			base.CheckAPH_Type();
			ListValidation.ErrorIfInvalidCode(Parent.APH_TypeInfo, Parent.APH_Type_List);
			MandatoryValidation.WarnIfNotEntered(Parent.APH_TypeInfo);
		}

		protected override void CheckAPH_GoodsReceivedStatus()
		{
			base.CheckAPH_GoodsReceivedStatus();
			ListValidation.ErrorIfInvalidCode(Parent.APH_GoodsReceivedStatusInfo, Parent.APH_GoodsReceivedStatus_List);
			MandatoryValidation.WarnIfNotEntered(Parent.APH_GoodsReceivedStatusInfo);
		}

		protected override void CheckAPH_RX_NKOrderCurrency()
		{
			base.CheckAPH_RX_NKOrderCurrency();
			Parent.APH_RX_NKOrderCurrencyInfo.RunAdditionalValidation();
			ListValidation.ErrorIfInvalidCode(Parent.APH_RX_NKOrderCurrencyInfo);
		}

		protected override void CheckAPH_EstimatedExchangeRate()
		{
			base.CheckAPH_EstimatedExchangeRate();
			Parent.APH_EstimatedExchangeRateInfo.RunAdditionalValidation();
		}
	}
}

