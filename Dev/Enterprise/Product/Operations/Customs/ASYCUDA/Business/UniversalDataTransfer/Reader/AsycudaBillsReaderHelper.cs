using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;

public class AsycudaBillsReaderHelper : CollectionReaderHelper<AsycudaBill>
{
	public void MarkUnprocessedExistingObjectFor(UniversalObjectFactory factory, AsycudaManifestHeader header)
	{
		var query = new ZQuery(AsycudaBillSchema.ABL_AMA, header.PK);
		query.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
		query.FetchOnlyFromLocalCache = !header.IsInDatabase;
		base.MarkUnprocessedExistingObjectFor(factory, query);
	}

	public void DeleteUnprocessedObjectsFor(BusinessObject parentBO, IXmlImportLogger logger, bool isSubShipmentContentPartial)
	{
		if (isSubShipmentContentPartial)
		{
			foreach (var pair in ExistingObjectProcessingDictionary.ToArray())
			{
				if (!pair.Value && GetParentBOPK(pair.Key) == parentBO.PK)
				{
					var bizO = pair.Key;
					var contextManager = bizO.GetUniversalDataContextManager();
					var message = Res.GetString("E3E0DA93-D4B5-420D-B8DB-4A9ED09D4795", "Could not delete {0} (Type: {1} | Key: {2}) due to the following reason: Sub Shipment Collection Content=Partial is not supported", bizO.HumanReadableName, contextManager.DataContextType, contextManager.DataContextKey);
					logger.Log(LogType.Warning, message);
					MarkProcessed(bizO);
				}
			}
		}
		else
		{
			base.DeleteUnprocessedObjectsFor(parentBO, logger);
		}
	}

	protected override ZGuid GetParentBOPK(AsycudaBill bill)
	{
		return bill.ABL_AMA;
	}
}
