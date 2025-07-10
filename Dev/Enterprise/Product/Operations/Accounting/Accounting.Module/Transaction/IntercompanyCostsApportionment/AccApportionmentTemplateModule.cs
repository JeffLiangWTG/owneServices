using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccApportionmentTemplateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccApportionmentTemplate; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Environment.Env.Licence.Accountant; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Environment.Env.Security.ApportionmentTemplate; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccApportionmentTemplate);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccApportionmentTemplateFilterBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new AccApportionmentTemplateFilterControl(GridCollection, (AccApportionmentTemplateFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccApportionmentTemplateCollection(Factory);
		}
	}
}
