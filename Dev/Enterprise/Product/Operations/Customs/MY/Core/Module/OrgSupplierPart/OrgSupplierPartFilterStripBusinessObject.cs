using Enterprise.Customs.Business;
using Enterprise.Customs.MY.Business;

namespace Enterprise.Customs.MY.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Malaysia);

		protected override bool NeedCustomsTypeFilter => false;
	}
}
