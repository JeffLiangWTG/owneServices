using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.Module;

public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
{
	public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.India);
}
