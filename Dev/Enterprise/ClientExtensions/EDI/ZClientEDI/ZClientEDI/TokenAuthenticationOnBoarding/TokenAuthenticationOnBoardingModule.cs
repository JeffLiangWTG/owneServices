using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding
{
	internal class TokenAuthenticationOnBoardingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ClientModuleRegistration.TokenAuthenticationOnBoarding;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.TokenAuthenticationOnBoarding;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.TokenAuthenticationOnBoarding);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TokenAuthenticationOnBoardingFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new TokenAuthenticationOnBoardingFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EdiTokenAuthOnBoardingDataCollection(Factory);
		}
	}
}
