using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using Enterprise.ZArchitecture.Core;
using ESDepartureGoodsItemWrapper = Enterprise.Customs.ES.DocumentWrappers.NCTS.DepartureGoodsItemWrapper;
using ESNctsDepartureCargoDesc = Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS
{
	public class ESNctsDepartureCargoDescWrapper : NctsDepartureCargoDescWrapper
	{
		public ESNctsDepartureCargoDescWrapper(ESNctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
		{
		}

		public static ESNctsDepartureCargoDescWrapper New(ESNctsDepartureCargoDesc line, BusinessObjectFactory factory)
		{
			return new ESNctsDepartureCargoDescWrapper(line, factory);
		}
		protected override IDepartureGoodsItem GetDepartureGoodsItemWrapper(NctsDepartureCargoDesc line) => new ESDepartureGoodsItemWrapper((ESNctsDepartureCargoDesc)line);

		protected override ZString GetBox33Commodity() => base.GetBox33Commodity().Left(8);

		protected override ZString GetPreviousDocuments()
		{
			var previousDocuments = GetAllPreviousDocuments();
			return new ESCompactPreviousDocumentsBuilder().GetPreviousDocumentsFormatted(previousDocuments, Line.IsPhase5Departure);
		}

		protected override ZString GetSupportingDocuments()
		{
			var supportingDocuments = GetAllSupportingDocuments();
			return new ESProducedDocumentsCertificatesBuilder().GetProducedDocumentsCertificatesFormatted(supportingDocuments, Line.IsPhase5Departure);
		}

		protected override ZString GetBox1Regime() => Line.MoveHeader.BM_InBondEntryType;

		protected override ZString GetBox35GrossMass() => Tools.ValueOrThreeDashes(CommonNctsHeaderDocumentWrapper.GetBox35GrossMass(DepartureGoodsItemWrapper.GrossMass, Line.IsInPhase5TransitionPeriod));

		protected override ZString GetBox38NetMass() => Tools.ValueOrThreeDashes(CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain(DepartureGoodsItemWrapper.NetMass, Line.IsInPhase5TransitionPeriod));

		protected override CodeDescriptionPairList GetPackageUnitTypeList => NctsPackageLookups.GetPackageUnitTypeList(Line.Factory, TranslationHelper.GetLanguageCode(Core.SharedConstants.Languages.Spanish));
	}
}
