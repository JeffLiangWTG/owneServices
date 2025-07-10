using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MailManager.Module
{
	public class MailItemTemplateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.MailItemTemplate; }
		}

		protected override ZController GetNewController(CargoWise.EntityFramework.BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.MailItemTemplate);
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new MailItemTemplateFilterControl(GridCollection, (MailItemTemplateFilterBusinessObject)FilterBusinessObject);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new MailItemTemplateFilterBusinessObject();
		}

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return new MailItemTemplateCollection(Factory);
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.EmailTemplates; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}
	}
}
