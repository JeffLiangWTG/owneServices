

using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceEnterpriseModule : ZFilterGridModule
	{
		public LicenceEnterpriseModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.LicenceEnterprise; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var controller = ZControllerFactory.Create(Modules.ClientControllerRegistration.LicenceEnterprise) as LicenceEnterpriseController;
			controller.TypeOfTopLevelBusinessObjectOverride = GridCollection?.TypeOfElements;
			return controller;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LicenceEnterpriseFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new LicenceEnterpriseCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LicenceEnterpriseFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Organisation; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
