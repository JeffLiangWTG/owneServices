using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DataTransfer;

public class CusSupplyChainActorReferencesDataObjectReader : DataObjectReader
{
	public CusSupplyChainActorReferencesDataObjectReader(IXmlImportLogger logger, UniversalObjectFactory factory)
		: base(logger)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	protected readonly UniversalObjectFactory factory;

	public void Read(CusSupplyChainActorReferenceCollection cusSupplyChainActorReferences, IDictionary<ZString, List<CustomsReference>> customsReferenceGroupByType)
	{
		if (customsReferenceGroupByType.TryGetValue(Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, out var actors))
		{
			var existingSupplyChainActorReferences = cusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().GroupBy(x => x.CFR_Code.ToUpperInvariant()).ToDictionary(x => x.Key, y => new Queue<CusSupplyChainActorReference>(y.OrderBy(o => o.CFR_SystemCreateTimeUtc)));
			foreach (var actor in actors)
			{
				var code = actor.SubType.GetCodeAsUpperCase();
				if (!code.IsEmpty)
				{
					IColumnIndexer supplyChainActorReferenceRow = null;
					CusSupplyChainActorReference supplyChainActorReference = null;
					if (existingSupplyChainActorReferences.TryGetValue(code, out var supplyChainActorReferences))
					{
						supplyChainActorReference = supplyChainActorReferences.Dequeue();
						if (supplyChainActorReferences.Count == 0)
						{
							existingSupplyChainActorReferences.Remove(code);
						}
						supplyChainActorReferenceRow = GetColumnIndexer(supplyChainActorReference);
					}
					else
					{
						supplyChainActorReference = cusSupplyChainActorReferences.AddNew();
						supplyChainActorReferenceRow = GetColumnIndexer(supplyChainActorReference);
						SetValue(supplyChainActorReferenceRow, CusReferenceSchema.CFR_Code, code);
					}

					var ownerDataObject = actor.Owner;
					if (ownerDataObject != null)
					{
						var ownerAddress = new OrganisationDataObjectReader(ownerDataObject, logger, factory).GetMatched();
						SetValue(supplyChainActorReferenceRow, CusReferenceSchema.CFR_OA_Owner, ownerAddress?.PK ?? ZGuid.Empty);
					}
					SetValue(supplyChainActorReferenceRow, CusReferenceSchema.CFR_Reference, actor.Reference);
				}
			}
			existingSupplyChainActorReferences.SelectMany(x => x.Value).DeleteAll();
		}
	}
}
