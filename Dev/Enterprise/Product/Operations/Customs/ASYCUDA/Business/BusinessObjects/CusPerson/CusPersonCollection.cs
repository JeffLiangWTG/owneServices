using System;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonCollection<B, MasterB> : CusPersonCollection
		where B : CusPerson
		where MasterB : AsycudaManifestHeader
	{
		public CusPersonCollection(MasterB master)
			: base(master)
		{
		}

		public new B this[int i] => (B)base[i];
		public new MasterB Master => (MasterB)base.Master;
		public new B AddNew() => (B)base.AddNew();
		public new B AddNew(Type bizOType) => (B)base.AddNew(bizOType);
	}
}
