using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;

namespace Enterprise.Customs.IT.NCTS.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class NctsHeaderDocumentWrapper : DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper
{
	protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
	{
	}

	public static DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper nctsHeaderDocumentWrapper;

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
	protected override ZString GetBoxDResult() => NctsHeader.ReleaseCode.IsEmpty ? ZString.Empty : $"CODICE SVINCOLO {NctsHeader.ReleaseCode}";
}
