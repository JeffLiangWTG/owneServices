using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Module;

public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
{
	public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory);

	protected override bool NeedCustomsTypeFilter => false;
}
