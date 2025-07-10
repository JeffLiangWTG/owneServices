using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class K84ReportsModule : EDIMessageModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.K84Reports; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CA.K84Reports);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new K84MessageCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new K84MessageFilterBusinessObject();
		}

		protected override bool ShowRequeuingMenu
		{
			get { return false; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new K84ReportsFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ImportBroker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAK84Reports; }
		}
	}
}
