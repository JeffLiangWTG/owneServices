using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

public static class NctsExtensionsForTest
{
	public static NctsHeader NewDepartureNctsHeader(this BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}

	public static NctsHeader NewDepartureNctsHeaderPhase5(this BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}

	public static StmPrintJob[] GetPrintedJobs(this BusinessObjectFactory factory, ZGuid parentGuid, ZString documentName)
	{
		var filterQuery = new ZQuery(StmPrintJobSchema.SP_ParentGuid, parentGuid)
			.AddToFilter(StmPrintJobSchema.SP_DocumentName, SQLComparisonOperator.Contains, documentName);

		return factory.Load<StmPrintJob>(filterQuery);
	}

	public static StmPrintJob[] GetTadPrintedJobs(this BusinessObjectFactory factory, NctsHeader nctsHeader) => factory.GetPrintedJobs(nctsHeader.PK, "TAD");
}
