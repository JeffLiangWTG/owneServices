using Enterprise.Customs.Business;
using Enterprise.Customs.MX.Business;

namespace Enterprise.Customs.MX.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Mexico);
	}
}
