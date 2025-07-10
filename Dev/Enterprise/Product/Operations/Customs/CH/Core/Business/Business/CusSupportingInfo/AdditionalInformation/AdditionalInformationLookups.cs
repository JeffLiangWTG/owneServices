using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class AdditionalInformationLookups : Customs.Business.CusSupportingInfoLookups
{
	public AdditionalInformationLookups(AdditionalInformation parent) : base(parent)
	{
	}

	public CodeDescriptionPairList AdditionalInformationCodeList
	{
		get
		{
			if (Parent.Parent?.JobDeclaration is JobDeclaration declaration)
			{
				if (declaration.IsImport)
				{
					return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CHAdditionalInformation, Parent.Parent.EffectiveAssessmentDate);
				}
				if (declaration.IsExportOrExportDeclarationActivation)
				{
					var level = Parent.Parent is CusEntryInstruction ? UniversalReferenceConstants.RefCusCodeList.AttributeValues.Header : UniversalReferenceConstants.RefCusCodeList.AttributeValues.Item;
					return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.Switzerland,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation, Parent.Parent.EffectiveAssessmentDate, false,
						UniversalReferenceConstants.RefCusCodeList.Attributes.Level, new ZString[] { level });
				}
			}
			return new CodeDescriptionPairList();
		}
	}

	public CodeDescriptionPairList ReferenceNumberCodeList
	{
		get
		{
			switch (Parent.CSI_Code)
			{
				case UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic:
					return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FreeZoneTraffic, Parent.Parent.EffectiveAssessmentDate);
				case UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic:
					return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BorderZoneTraffic, Parent.Parent.EffectiveAssessmentDate);
				case UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil:
					return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ExportCodeMineralOil, Parent.Parent.EffectiveAssessmentDate);
				default:
					return new CodeDescriptionPairList();
			}
		}
	}

	public CodeDescriptionPairList DescriptionCodeList
	{
		get
		{
			if (Parent.Parent is JobComInvoiceLine invoiceLine && invoiceLine.IsTobaccoRefundType)
			{
				switch (Parent.CSI_Code)
				{
					case UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup:
						return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBMG, Parent.Parent.EffectiveAssessmentDate);
					case UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup:
						foreach (var additionalInformation in invoiceLine.AdditionalInformations)
						{
							if (additionalInformation.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup)
							{
								switch (additionalInformation.CSI_Description)
								{
									case UniversalReferenceConstants.TobaccoMainGroupCodes.Cigars:
										return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGA, Parent.Parent.EffectiveAssessmentDate);
									case UniversalReferenceConstants.TobaccoMainGroupCodes.Cigarettes:
										return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGB, Parent.Parent.EffectiveAssessmentDate);
									case UniversalReferenceConstants.TobaccoMainGroupCodes.CutTobacco:
										return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGC, Parent.Parent.EffectiveAssessmentDate);
									case UniversalReferenceConstants.TobaccoMainGroupCodes.Assortment:
										return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGD, Parent.Parent.EffectiveAssessmentDate);
									case UniversalReferenceConstants.TobaccoMainGroupCodes.ECigarettes:
										return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGE, Parent.Parent.EffectiveAssessmentDate);
								}
							}
						}
						break;
				}
			}
			return new CodeDescriptionPairList();
		}
	}
	protected new AdditionalInformation Parent => (AdditionalInformation)base.Parent;
}
