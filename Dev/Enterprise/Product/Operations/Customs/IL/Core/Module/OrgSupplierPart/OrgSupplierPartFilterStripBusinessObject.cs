using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Israel);
	}
}
