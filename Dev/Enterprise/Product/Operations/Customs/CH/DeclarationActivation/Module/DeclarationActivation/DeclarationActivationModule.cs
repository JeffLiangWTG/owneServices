using CargoWise.EntityFramework;
using Enterprise.Customs.CH.DeclarationActivation.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CH.DeclarationActivation.Module;

public sealed class DeclarationActivationModule : ZFilterGridModule
{
	public override ModuleIdentifier ID => ModuleIDs.Customs.CH.DeclarationActivation;

	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new DeclarationActivationController();

	protected override IBusinessObjectCollection GetNewGridCollection() => new DeclarationActivationHeaderCollection(Factory);

	protected override IFilterControl GetNewFilterControl() => new DeclarationActivationFilterControl(GridCollection, FilterBusinessObject);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new DeclarationActivationFilterStripBusinessObject();

	protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

	public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsDeclarationEnquiryEdit;

	public override bool AllowDelete => false;
}
