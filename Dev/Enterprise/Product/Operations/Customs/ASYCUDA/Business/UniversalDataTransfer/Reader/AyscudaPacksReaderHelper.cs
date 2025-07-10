using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;

public class AyscudaPacksReaderHelper : CollectionReaderHelper<AsycudaPack>
{
	public void MarkUnprocessedExistingObjectFor(UniversalObjectFactory factory, AsycudaBill bill)
	{
		var query = new ZQuery(AsycudaPackSchema.APA_ABL_Bill, bill.PK);
		query.FetchOnlyFromLocalCache = !bill.IsInDatabase;
		base.MarkUnprocessedExistingObjectFor(factory, query);
	}
	protected override ZGuid GetParentBOPK(AsycudaPack pack)
	{
		return pack.APA_ABL_Bill;
	}
}
