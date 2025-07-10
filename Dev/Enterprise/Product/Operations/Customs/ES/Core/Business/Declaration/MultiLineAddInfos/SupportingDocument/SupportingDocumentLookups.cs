using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CustomsUQList => RefCusCodeListTypes.GetCachedList(Factory,
																	Parent.Declaration?.GetDefaultDataGroupingCode() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
																	Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
																	Parent.Declaration?.DateOfValuation ?? ZDateTime.Today);

		public CodeDescriptionPairList PackageCodeList => RefCusCodeListTypes.GetCachedList(Factory,
								Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
								ZDateTime.Today);

		public CodeDescriptionPairList SupportingDocumentProcedureList => Factory.GetCachedValue<SupportingDocumentProcedureCodeList>();
	}
}
