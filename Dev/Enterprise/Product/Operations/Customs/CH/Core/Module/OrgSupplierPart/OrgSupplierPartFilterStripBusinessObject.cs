using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.Module;

public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
{
	public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Switzerland);
}
