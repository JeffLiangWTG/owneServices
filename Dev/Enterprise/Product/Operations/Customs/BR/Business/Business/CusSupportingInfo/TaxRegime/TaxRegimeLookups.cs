using Enterprise.Customs.Business;
using ECC = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class TaxRegimeLookups : CusSupportingInfoLookups
	{
		public TaxRegimeLookups(TaxRegime parent)
			: base(parent)
		{
		}

		JobComInvoiceLine invoiceLine => Parent.Parent as JobComInvoiceLine;

		JobDeclaration declaration => invoiceLine.Declaration;

		public CodeDescriptionPairList TaxRegimeList
		{
			get
			{
				switch (Parent.CSI_SubType)
				{
					case TaxRegimeTypeList.Codes.Duty:
						return BRRefCusCodeListTypes.GetTaxRegimeList(Factory, declaration.JE_MessageType, declaration.JE_MessageSubType, invoiceLine.EffectiveAssessmentDate, Constants.ProcedureCategories.Duty);
					case TaxRegimeTypeList.Codes.PisCofins:
						return BRRefCusCodeListTypes.GetTaxRegimeList(Factory, declaration.JE_MessageType, declaration.JE_MessageSubType, invoiceLine.EffectiveAssessmentDate, Constants.ProcedureCategories.PisCofins);
					case TaxRegimeTypeList.Codes.IPI:
						return BRRefCusCodeListTypes.GetIPITaxRegimeMethodList(Factory);
					case TaxRegimeTypeList.Codes.ICMS:
						return BRRefCusCodeListTypes.GetICMSTaxRegimeMethodList(Factory);
					case TaxRegimeTypeList.Codes.FMM:
						return Factory.GetCachedValue<FMMBenefitTypeList>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public CodeDescriptionPairList LegalBaseList
		{
			get
			{
				if (declaration != null)
				{
					switch (Parent.CSI_SubType)
					{
						case TaxRegimeTypeList.Codes.Duty:
							return BRRefCusCodeListTypes.GetLegalBaseList(Factory, declaration.JE_MessageType, declaration.JE_MessageSubType, Parent.CSI_Code, invoiceLine.EffectiveAssessmentDate, Constants.ProcedureCategories.Duty, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode);
						case TaxRegimeTypeList.Codes.PisCofins:
							return BRRefCusCodeListTypes.GetLegalBaseList(Factory, declaration.JE_MessageType, declaration.JE_MessageSubType, Parent.CSI_Code, invoiceLine.EffectiveAssessmentDate, Constants.ProcedureCategories.PisCofins, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode);
						case TaxRegimeTypeList.Codes.ICMS:
							return BRRefCusCodeListTypes.GetICMSLegalBaseList(Factory);
					}
				}

				return new CodeDescriptionPairList();
			}
		}
	}
}
