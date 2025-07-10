using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory);
	}
}
