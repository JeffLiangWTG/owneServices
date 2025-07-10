using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		}
	}
}
