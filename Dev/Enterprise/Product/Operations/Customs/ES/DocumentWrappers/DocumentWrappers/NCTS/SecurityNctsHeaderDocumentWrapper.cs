using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class SecurityNctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.SecurityNctsHeaderDocumentWrapper
	{
		protected SecurityNctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public new static SecurityNctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
		{
			return new SecurityNctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
		}

		protected override DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> GetLinesCore()
		{
			return new ESNctsDepartureCargoDescWrapperCollection(NctsHeader, Factory);
		}

		protected override ZString ConvertNumberToString(IZType number) => CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain(number, NctsHeader.IsInPhase5TransitionPeriod);

		protected override ZString ConvertDateToString(ZDateTime? date) => CommonNctsHeaderDocumentWrapper.GetDateFormatSpain(date);

		protected override ZString GetBoxCOfficeOfDepartureForPhase4() => CommonNctsHeaderDocumentWrapper.GetBoxCOfficeOfDepartureData(NctsHeader);

		protected override ZString GetBoxCDate() => ZString.Empty;

		protected override ZString GetBoxDResult() => CommonNctsHeaderDocumentWrapper.GetBoxDResult(NctsHeader);

		protected override ZString GetBoxDTimeLimitDate() => CommonNctsHeaderDocumentWrapper.GetBoxDTimeLimitDate(NctsHeader);

		protected override ZString GetBoxDClearance() => CommonNctsHeaderDocumentWrapper.GetBoxDClearance(NctsHeader);

		protected override ZString GetBox30LocationOfGoodsForPhase4() => MovementHeader?.BM_LocationOfGoodsCode ?? ZString.Empty;

		protected override ZString GetBox35GrossMassForPhase4() => CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(NctsHeader);

		protected override ZString GetBox35GrossMassForPhase5() => CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(NctsHeader);
	}
}
