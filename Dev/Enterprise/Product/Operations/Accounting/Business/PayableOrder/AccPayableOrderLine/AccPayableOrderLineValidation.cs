//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPayableOrderLineValidation
//
//    This class should be used for overriding validation in AutoAccPayableOrderLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.PayableOrder
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using ResString = ResString;

	public class AccPayableOrderLineValidation : AutoAccPayableOrderLineValidation
	{
		public AccPayableOrderLineValidation(AutoAccPayableOrderLine parent) : base(parent)
		{
		}

		new AccPayableOrderLine Parent
		{
			get { return (AccPayableOrderLine)base.Parent; }
		}

		protected override void CheckAPL_GB()
		{
			base.CheckAPL_GB();
			ListValidation.ErrorIfInvalidPK(Parent.APL_GBInfo, Parent.Lookups.Branches);
		}

		protected override void CheckAPL_GE()
		{
			base.CheckAPL_GE();
			ListValidation.ErrorIfInvalidPK(Parent.APL_GEInfo, Parent.DepartmentCollection);
		}

		protected override void CheckAPL_Quantity()
		{
			base.CheckAPL_Quantity();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.APL_QuantityInfo, 0);
			ValidateAPL_QtyReceived();
		}

		protected override void CheckAPL_LinePrice()
		{
			base.CheckAPL_LinePrice();
			if (!Parent.ReadOnly && Parent.APL_LinePrice != Utilities.Round(Parent.APL_LinePrice, Parent.OrderCurrencyDecimals))
			{
				Parent.APL_LinePriceInfo.AddError(GetCheckAPL_LinePriceErrorMessage(Parent));
			}
		}

		protected override void CheckAPL_QtyReceived()
		{
			base.CheckAPL_QtyReceived();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.APL_QtyReceivedInfo, 0);
			QtyReceivedEqualsQtyOrderedValidation.CheckQtyReceivedEqualsQtyOrdered();
		}

		protected QtyReceivedEqualsQtyOrderedValidation QtyReceivedEqualsQtyOrderedValidation
		{
			get { return fQtyReceivedEqualsQtyOrderedValidation ?? (fQtyReceivedEqualsQtyOrderedValidation = new QtyReceivedEqualsQtyOrderedValidation(Parent)); }
		}

		QtyReceivedEqualsQtyOrderedValidation fQtyReceivedEqualsQtyOrderedValidation;

		protected override void CheckAPL_InnerPacks()
		{
			base.CheckAPL_InnerPacks();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.APL_InnerPacksInfo, 0);
		}

		protected override void CheckAPL_OuterPacks()
		{
			base.CheckAPL_OuterPacks();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.APL_OuterPacksInfo, 0);
		}

		protected override void CheckAPL_ItemPrice()
		{
			base.CheckAPL_ItemPrice();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.APL_ItemPriceInfo, 0);
		}

		protected override void CheckAPL_InnerPacksUQ()
		{
			base.CheckAPL_InnerPacksUQ();
			ListValidation.ErrorIfInvalidCode(Parent.APL_InnerPacksUQInfo, Parent.Lookups.PackTypes);
		}

		protected override void CheckAPL_OuterPacksUQ()
		{
			base.CheckAPL_OuterPacksUQ();
			ListValidation.ErrorIfInvalidCode(Parent.APL_OuterPacksUQInfo, Parent.Lookups.PackTypes);
		}

		protected override void CheckAPL_F3_NKPackType()
		{
			base.CheckAPL_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.APL_F3_NKPackTypeInfo, Parent.APL_F3_NKPackType_List);
		}

		protected override void CheckAPL_Status()
		{
			base.CheckAPL_Status();
			ListValidation.ErrorIfInvalidCode(Parent.APL_StatusInfo, Parent.APL_Status_List);
		}

		public void ValidateGenericCharge()
		{
			ValidateCalculatedProperty(Parent.GenericChargeInfo);
		}

		protected virtual void CheckGenericCharge()
		{
			MandatoryValidation.CheckEntered(Parent.GenericChargeInfo);
			ListValidation.ErrorIfInvalidPK(Parent.GenericChargeInfo, Parent.ChargeList);
		}

		protected override void CheckAPL_LineNo()
		{
			base.CheckAPL_LineNo();

			if (Parent.APL_LineNo <= 0)
			{
				Parent.APL_LineNoInfo.AddError(Res.GetString("f8b23773-2c54-462e-aee3-c30639036231", "Line Number must be greater than or equal to 1."));
			}

			if (IsDuplicateLine)
			{
				Parent.APL_LineNoInfo.AddError(DuplicateOrderLineMessage);
			}
		}

		bool IsDuplicateLine
		{
			get
			{
				ZQuery filter = new ZQuery(AccPayableOrderLineSchema.APL_APH, SQLComparisonOperator.Equal, Parent.APL_APH);
				filter.AddToFilter(AccPayableOrderLineSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(JoinCondition.And, AccPayableOrderLineSchema.APL_LineNo, SQLComparisonOperator.Equal, Parent.APL_LineNo);

				if (Parent.Order == null || !Parent.Order.IsInDatabase)
				{
					filter.FetchOnlyFromLocalCache = true;
				}

				AccPayableOrderLine duplicateLine = Parent.Factory.LoadTop1<AccPayableOrderLine>(filter);

				return (duplicateLine != null);
			}
		}

		static string DuplicateOrderLineMessage
		{
			get { return Res.GetString("17c32f2e-ccdf-4660-a5b4-11dafec43ad5", "A Line with this Order No already exists on this Order"); }
		}

		protected override void CheckAPL_PartNo()
		{
			base.CheckAPL_PartNo();
			ListValidation.WarnIfInvalidCode(Parent.APL_PartNoInfo, Parent.APL_PartNo_List, GetPartnoValidationMessage());
		}

		protected virtual MultilingualString GetPartnoValidationMessage()
		{
			return ResString.GetMultilingualString("0081ec53-cfd7-4081-b1f8-b6b214db7a94", "You have not entered an existing product number that is related to the supplier or buyer.");
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGenericCharge();
		}

		public string GetCheckAPL_LinePriceErrorMessage(AccPayableOrderLine line)
		{
			return Res.GetString("60e3e76b-ed36-40f2-9dee-5dd3e7805519", "The Item Price and number of Items entered on this line result in a Line Price of {0}\r\n\r\nLine Price must be in whole numbers to the decimals allowed by the Purchase Order Currency.\r\n\r\nPlease either adjust the Item Price, or clear the Item Price and enter the Line Price and allow the Item Price to calculate from the Line Price and number of Items.", Utilities.Round(line.APL_LinePrice, line.QuantityRateDecimals));
		}
	}
}

