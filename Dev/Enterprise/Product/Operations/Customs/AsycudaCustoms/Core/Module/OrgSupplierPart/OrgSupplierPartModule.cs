using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	[CodeAlive("Module dynamically hooked up for AsycudaCustoms countries.")]
	public class OrgSupplierPartModule : Customs.Module.OrgSupplierPartModule
	{
		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new OrgSupplierPartFilterStripControl(GridCollection, (OrgSupplierPartFilterStripBusinessObject)FilterBusinessObject);
		}
	}
}
