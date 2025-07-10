using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentWrappers;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS;

[CodeAlive("Used in documents DataContext")]
public class SecurityNctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.SecurityNctsHeaderDocumentWrapper
{
	protected SecurityNctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
	{
	}

	public static SecurityNctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
	{
		return new SecurityNctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
	}

	protected override DocBaseWrapperCollection<Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore()
	{
		return new NctsDepartureCargoDescWrapperCollection(NctsHeader, Factory);
	}
}
