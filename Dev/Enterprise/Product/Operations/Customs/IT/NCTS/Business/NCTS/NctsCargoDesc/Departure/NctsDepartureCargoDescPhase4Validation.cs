using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescPhase4Validation : EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation
{
	public NctsDepartureCargoDescPhase4Validation(NctsDepartureCargoDesc parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckPackages();
		CheckSupportingDocuments();
	}

	public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

	protected override void CheckBY_Description()
	{
		base.CheckBY_Description();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_DescriptionInfo);
	}

	protected override void CheckBY_GrossWeight()
	{
		base.CheckBY_GrossWeight();
		MandatoryValidation.MessageErrorIfIsNegative(Parent.BY_GrossWeightInfo);
		MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightInfo);
	}

	protected override void CheckBY_GrossWeightUnit()
	{
		base.CheckBY_GrossWeightUnit();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightUnitInfo);
	}

	protected override void CheckBY_NetWeight()
	{
		base.CheckBY_NetWeight();
		MandatoryValidation.MessageErrorIfIsNegative(Parent.BY_NetWeightInfo);
		if (MeetConditionCN91(Parent.PreviousDocuments) && Parent.BY_NetWeight.IsEmpty)
		{
			Parent.BY_NetWeightInfo.AddMessageError(ValidationCaptions.NctsCargoDesc.NetWeightIsRequired);
		}
	}

	bool MeetConditionCN91(EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> previousDocuments) => previousDocuments.Any(p => (p.IsTemporaryStorage && p.CSI_Quantity > 0) || p.IsIntoWarehouse);

	protected override void CheckBY_HarmonisedTariff()
	{
		base.CheckBY_HarmonisedTariff();
		if (MeetConditionCN90(Parent.PreviousDocuments) && Parent.BY_HarmonisedTariff.IsEmpty)
		{
			Parent.BY_HarmonisedTariffInfo.AddMessageError(ValidationCaptions.NctsCargoDesc.CommodityCodeIsRequired);
		}
	}

	bool MeetConditionCN90(EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> previousDocuments) => previousDocuments.Any(p => (p.IsTemporaryStorage && !p.CSI_Tariff.IsEmpty) || p.IsIntoWarehouse);

	protected override void CheckBY_RN_NKCountryOfOrigin()
	{
		base.CheckBY_RN_NKCountryOfOrigin();
		var countryOfOriginInfo = Parent.BY_RN_NKCountryOfOriginInfo;

		ListValidation.MessageErrorIfInvalidCode(countryOfOriginInfo);
		CheckCN34(countryOfOriginInfo);
	}

	protected override void CheckBY_RW_NKOriginState()
	{
		base.CheckBY_RW_NKOriginState();
		var originStateInfo = Parent.BY_RW_NKOriginStateInfo;

		ListValidation.MessageErrorIfInvalidCode(originStateInfo);
		CheckCN34(originStateInfo, goodsItem => goodsItem.BY_RN_NKCountryOfOrigin == Core.Constants.CountryCodes.Italy);
	}

	protected override void CheckBY_RN_NKCountryOfDispatch()
	{
		base.CheckBY_RN_NKCountryOfDispatch();
		if (MoveHeader != null && !MoveHeader.IsTIRDeclaration && MoveHeader.GoodsItems.AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem())
		{
			Parent.BY_RN_NKCountryOfDispatchInfo.AddMessageError(ValidationCaptions.NctsCargoDesc.AllOrNoneDispatchCountry);
		}
	}

	protected override void CheckBY_RN_NKCountryOfDestination()
	{
		base.CheckBY_RN_NKCountryOfDestination();
		if (MoveHeader != null && MoveHeader.GoodsItems.AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem())
		{
			Parent.BY_RN_NKCountryOfDestinationInfo.AddMessageError(ValidationCaptions.NctsCargoDesc.AllOrNoneDestinationCountry);
		}
	}

	protected override void CheckBY_CommercialReferenceNumber()
	{
		base.CheckBY_CommercialReferenceNumber();
		new CommercialReferenceAdditionalTextValidator(MoveHeader).ValidateCommercialReferenceNumber((ZPropertyInfoString)Parent.BY_CommercialReferenceNumberInfo);
	}

	protected override void CheckBY_Procedure()
	{
		base.CheckBY_Procedure();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BY_ProcedureInfo);
	}

	protected override void CheckBY_CommodityCode()
	{
		base.CheckBY_CommodityCode();

		ListValidation.MessageErrorIfInvalidCode(Parent.BY_CommodityCodeInfo);
	}

	protected override void CheckBY_CustomsSecondQuantity()
	{
		base.CheckBY_CustomsSecondQuantity();
		if (!Parent.BY_CustomsSecondUnitQty.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfIsZero(Parent.BY_CustomsSecondQuantityInfo);
		}
	}

	#region Implementation

	NctsDepartureMovementHeader MoveHeader => Parent.MoveHeader;

	void CheckPackages()
	{
		if (Parent.Packages.Count == 0)
		{
			Parent.AddRowMessageError(ValidationCaptions.NctsPackage.GoodsItemMustHaveAtLeastOnePackage);
		}
	}

	void CheckSupportingDocuments()
	{
		new NctsDepartureCargoDescAcrManager(Parent).Validate();
	}

	void CheckCN34(ZPropertyInfo targetPropertyInfo, Func<NctsDepartureCargoDesc, bool> additionalConditionForMandatoryValidation = null)
	{
		if (Parent.ActualCountryOfDispatch == Core.Constants.CountryCodes.Italy && (additionalConditionForMandatoryValidation?.Invoke(Parent) ?? true))
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
		else if (!targetPropertyInfo.Value.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);
		}
	}

	#endregion
}
