using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	public class ApplicationActiveLoggerModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ApplicationActiveLogger; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.ApplicationActiveLogger);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ApplicationActiveLoggerFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ApplicationActiveLoggerCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ApplicationActiveLoggerFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLogger; }
		}
	}
}
