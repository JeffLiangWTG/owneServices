using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureCargoDescPhase4Validation : NctsDepartureCargoDescValidation
	{
		public NctsDepartureCargoDescPhase4Validation(NctsDepartureCargoDesc parent) : base(parent)
		{
		}

		protected override void CheckCountryOfDestinationRule()
		{
			var parent = Parent;
			if (parent.MoveHeader is NctsDepartureMovementHeader moveHeader)
			{
				var info = parent.BY_RN_NKCountryOfDestinationInfo;
				var goodsItemDestinationCountry = parent.BY_RN_NKCountryOfDestination;
				var declarationDestinationCountry = moveHeader.BM_RL_NKDestinationPort;
				if (!goodsItemDestinationCountry.IsEmpty)
				{
					UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(parent.Factory, GetDeclarationTypeFallbackToParentIfReadOnly(moveHeader.Header, parent), parent.BY_RN_NKCountryOfDispatch, goodsItemDestinationCountry, parent.DataGroupingCode, info);
				}

				var numberOfTimesDestinationCountrySpecified = (goodsItemDestinationCountry.IsEmpty ? 0 : 1) + (declarationDestinationCountry.IsEmpty ? 0 : 1);

				if (numberOfTimesDestinationCountrySpecified == 0)
				{
					info.AddMessageError(Res.GetString("8571FBF6-7D49-4A98-90A9-48CFC01B92A6", "Destination Country must be filled either in Declaration tab or Goods tab."));
				}
				else if (numberOfTimesDestinationCountrySpecified > 1)
				{
					info.AddMessageError(Res.GetString("8571FBF6-7D49-4A98-90A9-48CFC01B92A7", "Destination Country must be filled either in Declaration tab or Goods tab but not both."));
				}
			}
		}

		protected override void CheckCountryOfDispatchRule()
		{
			var parent = Parent;
			if (parent.Header is NctsHeader header)
			{
				var info = parent.BY_RN_NKCountryOfDispatchInfo;
				var goodsItemDispatchCountry = parent.BY_RN_NKCountryOfDispatch;
				var declarationDispatchCountry = header.BH_RL_NKImportLoadPort;
				var numberOfTimesDispatchCountrySpecified = (goodsItemDispatchCountry.IsEmpty ? 0 : 1) + (declarationDispatchCountry.IsEmpty ? 0 : 1);

				if (numberOfTimesDispatchCountrySpecified == 0)
				{
					info.AddMessageError(Res.GetString("8333D0F9-7245-46B1-8D27-83C03D5F6497", "Dispatch Country must be filled either in Declaration tab or Goods tab."));
				}
				else if (numberOfTimesDispatchCountrySpecified > 1)
				{
					info.AddMessageError(Res.GetString("8333D0F9-7245-46B1-8D27-83C03D5F6498", "Dispatch Country must be filled either in Declaration tab or Goods tab but not both."));
				}
			}
		}

		protected override void CheckBY_RN_NKCountryOfOrigin()
		{
			base.CheckBY_RN_NKCountryOfOrigin();
			var parent = Parent;
			MandatoryValidation.WarnIfNotEntered(parent.BY_RN_NKCountryOfOriginInfo, Res.GetString("56BD9940-703F-456E-A47C-0DD26E518A28", "Origin Country. The ADD/CVD/Safeguarding won't be calculated when the Value is blank"));
		}

		protected override void CheckBY_CustomsThirdQuantity()
		{
			base.CheckBY_CustomsThirdQuantity();
			TypeValidation.CheckValidDecimal(Parent.BY_CustomsThirdQuantityInfo, 14, NctsDepartureCargoDesc.Schema.BY_CustomsThirdQuantityDecimalPlacesPhase4);
		}

		protected override void CheckBY_CustomsThirdUnitQty()
		{
			base.CheckBY_CustomsThirdUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.BY_CustomsThirdUnitQtyInfo, Parent.Lookups.CustomsUnitOfQuantityList);
		}
	}
}
