using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class Phase5NctsHeaderTADDocumentWrapper : DocumentWrappers.Customs.EU.NCTS.Phase5NctsHeaderTADDocumentWrapper
{
	public static Phase5NctsHeaderTADDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factory)
	{
		return new Phase5NctsHeaderTADDocumentWrapper(nctsHeader, factory);
	}

	Phase5NctsHeaderTADDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
	{
	}

	new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	protected override DocBaseWrapperCollection<DocumentWrappers.Customs.EU.NCTS.Phase5NctsTADItemWrapper> GetLinesCore() => new Phase5NctsTADItemWrapperCollection(NctsHeader, Factory);
}

