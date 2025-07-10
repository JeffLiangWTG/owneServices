using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AE.Module;

public class JobDeclarationModule : Customs.Module.JobDeclarationModule
{
	protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterStripBusinessObject();

	protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl() => new JobDeclarationFilterControl(this, GridCollection, FilterBusinessObject);

	protected override IBusinessObjectCollection GetNewGridCollection()
	{
		SetGuiProviders(Factory);
		return new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
