using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemWrapper : DocumentWrapper
{
	public static EVVGoodsItemWrapper New(IEvvGoodsItem goodsItem, BusinessObjectFactory factory, ZString documentLanguage)
		=> new EVVGoodsItemWrapper(Argument.NotNull(goodsItem, nameof(goodsItem)), Argument.NotNull(factory, nameof(factory)), documentLanguage);

	EVVGoodsItemWrapper(IEvvGoodsItem goodsItem, BusinessObjectFactory factory, ZString documentLanguage) : base(goodsItem, factory)
	{
		this.documentLanguage = documentLanguage;
	}
	readonly ZString documentLanguage;

	IEvvGoodsItem GoodsItem => (IEvvGoodsItem)WrappedObject;

	public ZString CustomsItemNumber => GoodsItem.CustomsItemNumber;

	public ZString Description => GoodsItem.Description;

	public ZString CommodityCode => GoodsItem.CommodityCode;

	public ZString StatisticalCode => GoodsItem.StatisticalCode?.PadLeft(3, '0');

	public ZBool IsCommercialGood => GoodsItem.Statistic?.IsCommercialGood ?? ZBool.False;

	public ZBool IsPreferenceOrigin => GoodsItem.IsOriginPreference ?? ZBool.False;

	public ZString CountryOfOrigin => GoodsItem.OriginCountry;

	public ZString CustomsClearanceType => GoodsItem.Statistic?.CustomsClearanceType?.PadLeft(2, '0') ?? ZString.Empty;

	public ZString CustomsClearanceTypeDescription => CustomsClearanceTypeList.GetDescriptionFromCode(CustomsClearanceType);

	public EVVGoodsItemDutyAndTaxesWrapperCollection DutyAndTaxes => dutyAndTaxes ??= EVVGoodsItemDutyAndTaxesWrapperCollection.New(GoodsItem.DutyAndTaxes, Factory, documentLanguage);
	EVVGoodsItemDutyAndTaxesWrapperCollection dutyAndTaxes;

	public ZDecimal TotalDutyAndTaxesAmount => GoodsItem.TotalDutyAndTaxesAmount ?? ZDecimal.Zero;

	public ZBool IsRepair => GoodsItem.Statistic?.IsRepair ?? ZBool.False;

	public ZString RepairReason => GoodsItem.RepairReason;

	public ZBool IsRepairOrRefinement => ProcedureCodesEdec.IsRepairOrRefinement(CustomsClearanceType) || (GoodsItem.Statistic?.IsRepair ?? ZBool.False);

	public ZDecimal NetMass => GoodsItem?.NetMass ?? ZDecimal.Zero;

	public ZBool NetMassConfirmation => GoodsItem?.NetMassConfirmation ?? ZBool.False;

	public ZDecimal GrossMass => GoodsItem.GrossMass;

	public ZBool GrossMassConfirmation => GoodsItem.GrossMassConfirmation ?? ZBool.False;

	public ZDecimal AdditionalUnit => GoodsItem?.AdditionalUnit ?? ZDecimal.Zero;

	public ZBool AdditionalUnitConfirmation => GoodsItem?.AdditionalUnitConfirmation ?? ZBool.False;

	public ZDecimal CustomsNetWeight => GoodsItem?.CustomsNetWeight ?? ZDecimal.Zero;

	public ZString PermitObligation => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, EdecTypes.PermitObligation, ZDateTime.Now, languageCode: documentLanguage.GetLanguageCode()).GetDescriptionFromCode(GoodsItem.PermitObligation);

	public ZString NonCustomsLawObligation => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, EdecTypes.NonCustomsLawObligation, ZDateTime.Now, languageCode: documentLanguage.GetLanguageCode()).GetDescriptionFromCode(GoodsItem.NonCustomsLawObligation);

	public ZString Direction => IsDirection ? RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, EdecTypes.Direction, ZDateTime.Now, languageCode: documentLanguage.GetLanguageCode()).GetDescriptionFromCode(GoodsItem.Direction) : ZString.Empty;

	public ZString RefinementType => IsRefinementType ? RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, EdecTypes.RefinementType, ZDateTime.Now, languageCode: documentLanguage.GetLanguageCode()).GetDescriptionFromCode(GoodsItem.RefinementType) : ZString.Empty;

	public ZString ProcessType => IsProcessType ? RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, EdecTypes.ProcessType, ZDateTime.Now, languageCode: documentLanguage.GetLanguageCode()).GetDescriptionFromCode(GoodsItem.ProcessType) : ZString.Empty;

	public ZString BillingType => IsBillingType ? RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, EdecTypes.BillingType, ZDateTime.Now, languageCode: documentLanguage.GetLanguageCode()).GetDescriptionFromCode(GoodsItem.BillingType) : ZString.Empty;

	public ZBool IsAdditionalUnit => GoodsItem.AdditionalUnit != null;

	public ZBool IsCustomsNetWeight => GoodsItem.CustomsNetWeight != null;

	public ZBool IsDirection => !GoodsItem.Direction.IsNullOrEmpty();

	public ZBool IsRefinementType => !GoodsItem.RefinementType.IsNullOrEmpty();

	public ZBool IsProcessType => !GoodsItem.ProcessType.IsNullOrEmpty();

	public ZBool IsBillingType => !GoodsItem.BillingType.IsNullOrEmpty();

	public EVVGoodsItemDetailWrapperCollection Details => details ??= EVVGoodsItemDetailWrapperCollection.New(GoodsItem.Details, Factory);
	EVVGoodsItemDetailWrapperCollection details;

	public EVVGoodsItemPackagingWrapperCollection Packagings => packagings ??= EVVGoodsItemPackagingWrapperCollection.New(GoodsItem.Packagings, Factory);
	EVVGoodsItemPackagingWrapperCollection packagings;

	public EVVGoodsItemPermitWrapperCollection Permits => permits ??= EVVGoodsItemPermitWrapperCollection.New(GoodsItem.Permits, Factory);
	EVVGoodsItemPermitWrapperCollection permits;

	public EVVGoodsItemProducedDocumentWrapperCollection ProducedDocuments => producedDocuments ??= EVVGoodsItemProducedDocumentWrapperCollection.New(GoodsItem.ProducedDocuments, Factory);
	EVVGoodsItemProducedDocumentWrapperCollection producedDocuments;

	public EVVGoodsItemSpecialMentionWrapperCollection SpecialMentions => specialMentions ??= EVVGoodsItemSpecialMentionWrapperCollection.New(GoodsItem.SpecialMentions, Factory);
	EVVGoodsItemSpecialMentionWrapperCollection specialMentions;

	internal ICodeDescriptionPairList CustomsClearanceTypeList => Factory.GetCachedValue($"CH.EVVGoodsItemWrapper.CustomsClearanceType.{documentLanguage}", () =>
	{
		var procedures = new RefCusProcedureCollection(Factory, Core.Constants.CountryCodes.Switzerland, ZDateTime.Now, ZString.Empty, CHJobMessageTypeList.Codes.Import);
		var list = new CodeDescriptionPairList();
		var language = documentLanguage.GetLanguageCode();
		foreach (var procedure in procedures)
		{
			list.AddPair(procedure.ZZ6_ProcedureCode, TranslationHelper.GetTranslatedValue(procedure, procedure.ZZ6_Description, RefCusProcedureLanguageSchema.ZXV_Description, language));
		}
		return list;
	});

	public ZDecimal VATValue => GoodsItem.VATDetails.VATValue;

	public ZDecimal VATDuties => GoodsItem.VATDetails.Duties;

	public ZDecimal VATRate => GoodsItem.VATDetails.Rate;

	public ZDecimal VATBasis => GoodsItem.VATDetails.Basis;

	public ZDecimal VATAmount => GoodsItem.VATDetails.Amount;
}
