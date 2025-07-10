using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;

namespace Enterprise.Customs.IT.NCTS.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class SecurityNctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.SecurityNctsHeaderDocumentWrapper
{
	protected SecurityNctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
	{
	}

	public static SecurityNctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
	{
		return new SecurityNctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
	}

	protected new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	protected override DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> GetLinesCore() => new ITNctsDepartureCargoDescWrapperCollection(NctsHeader, Factory);

	protected override ZString ConvertNumberToString(IZType number)
	{
		if (number is ZDecimal decimalValue)
		{
			return new IT5DecimalsWeightFormatter(decimalValue).GetFormattedValue();
		}
		return base.ConvertNumberToString(number);
	}

	protected override ZString GetNotReleasedWatermark()
	{
		if (IsPhase5)
		{
			return NctsHeader.ReleaseCode.IsEmpty
				? Tools.GetNotReleasedCaption()
				: ZString.Empty;
		}
		return base.GetNotReleasedWatermark();
	}

	protected override ZString GetBoxDResult() => NctsHeader.ReleaseCode.IsEmpty ? ZString.Empty : $"CODICE SVINCOLO {NctsHeader.ReleaseCode}";
}
