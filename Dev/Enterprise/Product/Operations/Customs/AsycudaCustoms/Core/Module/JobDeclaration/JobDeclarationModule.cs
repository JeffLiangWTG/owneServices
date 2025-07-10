using CargoWise.EntityFramework;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	[CodeAlive("Module dynamically hooked up for AsycudaCustoms countries.")]
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
