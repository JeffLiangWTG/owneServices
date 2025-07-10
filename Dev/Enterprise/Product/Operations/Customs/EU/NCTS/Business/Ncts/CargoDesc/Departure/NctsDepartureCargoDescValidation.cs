using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsDepartureCargoDescValidation : NctsCommonCargoDescValidation
	{
		public NctsDepartureCargoDescValidation(NctsDepartureCargoDesc parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateUNDGsAsString();
			ValidateAllSuspendListChanged();
		}

		public void ValidateUNDGsAsString()
		{
			ValidateCalculatedProperty(Parent.UNDGsAsStringInfo);
		}

		protected virtual void CheckUNDGsAsString()
		{
		}

		protected virtual void ValidateAllSuspendListChanged()
		{
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				CheckConditionC901();
			}
		}

		protected virtual void CheckConditionC901() => Parent.CheckConditionC901();

		protected INctsDepartureCargoDescPhase5ValidationDecider Decider => Parent.ValidationDecider as INctsDepartureCargoDescPhase5ValidationDecider;
		
		protected override void CheckBY_CustomsSecondQuantity()
		{
			base.CheckBY_CustomsSecondQuantity();

			var goodsItem = Parent;
			NctsValidationHelper.CheckRuleTR0084(goodsItem.Header, goodsItem.BY_CustomsSecondQuantityInfo, goodsItem.BY_CustomsSecondUnitQty, goodsItem.BY_CustomsSecondQuantity, NotificationType.MessageError);
		}

		protected override void CheckBY_RN_NKCountryOfDestination()
		{
			base.CheckBY_RN_NKCountryOfDestination();

			var parent = Parent;
			var info = parent.BY_RN_NKCountryOfDestinationInfo;

			parent.Consignee.Validation.ValidateOrganisationPK();
			ListValidation.MessageErrorIfInvalidCode(info);

			CheckCountryOfDestinationRule();
		}

		protected override void CheckBY_RN_NKCountryOfOrigin()
		{
			base.CheckBY_RN_NKCountryOfOrigin();

			NctsValidationHelper.CheckBY_RN_NKCountryOfOriginIsValid(Parent);
		}

		protected override void CheckBY_RN_NKCountryOfDispatch()
		{
			base.CheckBY_RN_NKCountryOfDispatch();

			var parent = Parent;

			new RuleE1301Validator(parent.Header).Validate(
			parent.BY_RN_NKCountryOfDispatchInfo,
			Res.GetString("CB9C312E-58A6-4AB3-B446-BB8F2651CBA2", "Country of Dispatch"),
			Decider is { IsRuleE1301Active: true });

			ListValidation.MessageErrorIfInvalidCode(parent.BY_RN_NKCountryOfDispatchInfo);
			CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code();
			CheckCountryOfDispatchRule();
		}

		protected virtual void CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code()
		{
			var parent = Parent;
			var dispatchCountry = parent.BY_RN_NKCountryOfDispatch;
			var info = parent.BY_RN_NKCountryOfDispatchInfo;

			if (!dispatchCountry.IsEmpty && parent.Header is NctsHeader header)
			{
				UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(parent.Factory, GetDeclarationTypeFallbackToParentIfReadOnly(header, parent), dispatchCountry, parent.BY_RN_NKCountryOfDestination, parent.DataGroupingCode, info);
			}
		}

		protected override void CheckBY_CustomsSecondUnitQty()
		{
			base.CheckBY_CustomsSecondUnitQty();

			NctsValidationHelper.CheckBY_CustomsSecondUnitQtyIsValid(Parent);
		}

		protected override void CheckBY_ZZF_NKTaxType()
		{
			base.CheckBY_ZZF_NKTaxType();
			if (Parent.Header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BY_ZZF_NKTaxTypeInfo, Parent.Lookups.TaxOrFeeCodeList);
			}
		}

		protected override void CheckBY_Type()
		{
			base.CheckBY_Type();
			if (Parent.Header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BY_TypeInfo, Parent.Lookups.DeclarationTypeList);
				if (!Parent.Header.IsPhase5)
				{
					CheckConditionR020(Parent, Parent.BY_TypeInfo);
					CheckConditionC045Phase4(Parent, Parent.BY_TypeInfo);
				}
			}

			void CheckConditionR020(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
			{
				var moveHeader = goodsItem.MoveHeader;
				if (moveHeader != null)
				{
					var departureCustomsOfficeCodeCountry = moveHeader.IsPhase5 ? moveHeader.DepartureCustomsOfficeCodeCountry : moveHeader.Header.DepartureCustomsOfficeCodeCountry;
					if (goodsItem.BY_Type == NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
						&& !departureCustomsOfficeCodeCountry.IsEmpty
						&& !NctsHeaderValidationHelper.IsEuMemberState(departureCustomsOfficeCodeCountry)
						&& !HasPreviousDocWithReference(goodsItem))
					{
						info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.R020, Res.GetString("5BBBF126-A524-48EE-9167-825E61D84513", "Previous Document Reference is missing for a T2 declaration issued in a non-EU country")));
					}
				}
			}

			bool HasPreviousDocWithReference(NctsDepartureCargoDesc goodsItem)
			{
				var allowedPrevDocs = new[] { "T2", "T2L", "T2F", "T2LF", "T2CIM", "T2TIR", "T2ATA" };
				var prevDocReference = goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>()
					.FirstOrDefault(p => allowedPrevDocs.Contains((string)p.CSI_Code) && !p.CSI_ReferenceNumber.IsEmpty)
					?.CSI_ReferenceNumber ?? ZString.Empty;

				return !prevDocReference.IsEmpty;
			}

			void CheckConditionC045Phase4(NctsCommonCargoDesc goodsItem, ZPropertyInfo info)
			{
				var moveHeader = goodsItem.MoveHeader;
				if (moveHeader != null)
				{
					var moveHeaderDeclarationType = moveHeader.BM_InBondEntryType;
					var goodsItemsDeclarationTypes = moveHeader.GoodsItems.Select(i => i.BY_Type).ToArray();
					if ((moveHeaderDeclarationType == NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4
							&& goodsItemsDeclarationTypes.Except(new ZString[] {
						NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure,
						NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure,
						NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories,
						NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino }).Any())
							||
						(moveHeaderDeclarationType != NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4
							&& goodsItemsDeclarationTypes.Except(new[] { ZString.Empty }).Distinct().Any()))
					{
						info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C045, Res.GetString("02867C53-A4E4-48F5-9995-3737EA30D544", "Declaration type at header level must be T- when all line level Declaration types are mixed T1, T2, T2F or T2SM otherwise Declaration type at line level cannot be used.")));
					}
				}
			}
		}

		protected abstract void CheckCountryOfDestinationRule();

		protected abstract void CheckCountryOfDispatchRule();

		protected new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		protected static ZString GetDeclarationTypeFallbackToParentIfReadOnly(NctsHeader header, NctsDepartureCargoDesc goodsItem)
		{
			var declarationType = goodsItem.BY_Type;
			if (goodsItem.DeclarationTypeReadOnly)
			{
				declarationType = header.MovementHeader?.BM_InBondEntryType ?? ZString.Empty;
			}
			return declarationType;
		}
	}
}
