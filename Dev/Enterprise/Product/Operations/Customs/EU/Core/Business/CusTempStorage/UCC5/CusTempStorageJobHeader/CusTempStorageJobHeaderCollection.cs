using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderCollection<T> : CusTempStorageJobHeaderCollection
		where T : CusTempStorageJobHeader
	{
		public CusTempStorageJobHeaderCollection(BusinessObjectFactory factory, GlbBranch branch)
			: base(factory, branch)
		{
		}

		public new T this[int i] => (T)base[i];

		public new T AddNew() => (T)base.AddNew();
	}
}
