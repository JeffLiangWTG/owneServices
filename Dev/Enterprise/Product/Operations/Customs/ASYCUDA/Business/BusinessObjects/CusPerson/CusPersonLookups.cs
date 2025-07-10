using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonLookups : Customs.Business.CusPersonLookups
	{
		public CusPersonLookups(CusPerson parent)
			: base(parent)
		{
		}

		protected new CusPerson Parent => (CusPerson)base.Parent;

		public GlbPersonCollection GlobalPersonsList => new GlbPersonCollection(Parent.Factory);
	}
}
