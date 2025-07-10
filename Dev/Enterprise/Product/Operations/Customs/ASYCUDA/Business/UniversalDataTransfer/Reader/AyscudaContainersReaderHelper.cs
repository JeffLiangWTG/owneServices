using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;

public class AyscudaContainersReaderHelper : CollectionReaderHelper<AsycudaContainer>
{
	public void MarkUnprocessedExistingObjectFor(UniversalObjectFactory factory, AsycudaManifestHeader header)
	{
		var query = new ZQuery(AsycudaContainerSchema.ACN_AMA_Manifest, header.PK);
		query.FetchOnlyFromLocalCache = !header.IsInDatabase;
		base.MarkUnprocessedExistingObjectFor(factory, query);
	}

	protected override ZGuid GetParentBOPK(AsycudaContainer container)
	{
		return container.ACN_AMA_Manifest;
	}
}
