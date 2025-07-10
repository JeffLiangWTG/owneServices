using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public abstract class SingleCusSupportingInfo : CusSupportingInfo
	{
		public SingleCusSupportingInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public abstract IEnumerable<ZPropertyInfo> GetUsedFieldsInfos();

		public bool IsEmpty => !GetUsedFieldsInfos().Any(x => !x.Value.IsEmpty);

		public override bool IsSavedByFactory => IsInDatabase ? base.IsSavedByFactory : !(IsDeleted || IsEmpty);

		public override void OnSaving()
		{
			if (!IsDeleted && IsInDatabase && IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}
	}
}
