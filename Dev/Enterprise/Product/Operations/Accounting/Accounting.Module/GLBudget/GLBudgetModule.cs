using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GLBudgetModule : ZFilterGridModule
	{
		public GLBudgetModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GLBudget; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GLBudget);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GLBudgetFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GLBudgetCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GLBudgetFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Budgets; }
		}
	}
}
