using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Japan);
	}
}
