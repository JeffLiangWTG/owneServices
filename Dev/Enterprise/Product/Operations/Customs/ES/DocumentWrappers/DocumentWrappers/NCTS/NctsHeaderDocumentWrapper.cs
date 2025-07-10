using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class NctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper
	{
		protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public new static Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));
			Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper nctsHeaderDocumentWrapper;

			if (ShouldUseSecurityNctsHeaderDocumentWrapper(nctsHeader))
			{
				nctsHeaderDocumentWrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, factoryToWrap);
			}
			else
			{
				nctsHeaderDocumentWrapper = new NctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
			}
			return nctsHeaderDocumentWrapper;
		}

		protected override DocBaseWrapperCollection<Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore()
		{
			return new ESNctsDepartureCargoDescWrapperCollection(NctsHeader, Factory);
		}

		protected override ZString ConvertNumberToString(IZType number) => CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain(number, NctsHeader.IsInPhase5TransitionPeriod);

		protected override ZString ConvertDateToString(ZDateTime? date) => CommonNctsHeaderDocumentWrapper.GetDateFormatSpain(date);

		protected override ZString GetBoxCOfficeOfDeparture() => CommonNctsHeaderDocumentWrapper.GetBoxCOfficeOfDepartureData(NctsHeader);

		protected override ZString GetBoxCDate() => ZString.Empty;

		protected override ZString GetBoxDResult() => CommonNctsHeaderDocumentWrapper.GetBoxDResult(NctsHeader);

		protected override ZString GetBoxDTimeLimitDate() => CommonNctsHeaderDocumentWrapper.GetBoxDTimeLimitDate(NctsHeader);

		protected override ZString GetBoxDClearance() => CommonNctsHeaderDocumentWrapper.GetBoxDClearance(NctsHeader);

		protected override ZString GetBox35GrossMass() => CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(NctsHeader);
	}
}
