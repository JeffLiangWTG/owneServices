using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class SingleCusCodeData : CusCodeData
{
	public SingleCusCodeData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected abstract IEnumerable<ZPropertyInfo> GetUsedFieldsInfos();

	public override bool IsSavedByFactory => IsInDatabase ? base.IsSavedByFactory : !(IsDeleted || IsEmpty);

	protected virtual bool IsEmpty => !GetUsedFieldsInfos().Any(x => !x.Value.IsDefault);

	public override void OnSaving()
	{
		if (!IsDeleted && IsInDatabase && IsEmpty)
		{
			Delete();
		}
		base.OnSaving();
	}
}
