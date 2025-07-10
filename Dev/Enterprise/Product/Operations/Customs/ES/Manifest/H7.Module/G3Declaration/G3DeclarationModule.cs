using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public class G3DeclarationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.ES.G3Declaration;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.G3Declaration;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.ES.G3Declaration);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new G3DeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new G3DeclarationFilterControl(GridCollection, (G3DeclarationFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new G3DeclarationMessageCollection(Factory);

		public override bool AllowNew => false;

		public override bool AllowEdit => false;
	}
}
