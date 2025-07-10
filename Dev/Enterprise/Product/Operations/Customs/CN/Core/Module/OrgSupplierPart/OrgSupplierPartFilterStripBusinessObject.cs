using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<BaseCusClassification>(Factory, Core.Constants.CountryCodes.China);
	}
}
