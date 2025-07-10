using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Manifest.Business;

public abstract class AsycudaBill : ASYCUDA.Business.AsycudaBill, ISupportMultipleResourceStringData
{
	public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	#region ISupportMultipleResourceStringData

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => Header?.MultipleKeysToUse;

	#endregion
}
