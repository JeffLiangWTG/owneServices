using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.EndpointManagement.Module
{
	public class EdiTrustedMessagingConfigModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.EdiTrustedMessagingConfig; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.EdiTrustedMessagingConfig; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		#region Implementation

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.EdiTrustedMessagingConfig);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EdiTrustedMessagingConfigFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EdiTrustedMessagingConfigGlobalCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EdiTrustedMessagingConfigFilterControl(GridCollection, FilterBusinessObject);
		}

		#endregion Implementation
	}
}
