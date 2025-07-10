using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding
{
	public class TokenAuthenticationOnBoardingController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.TokenAuthenticationOnBoarding;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.TokenAuthenticationOnBoarding;

		public override Type TypeOfTopLevelBusinessObject => typeof(EdiTokenAuthOnBoardingData);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.TokenAuthenticationOnBoarding;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.TokenAuthenticationOnBoarding;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.TokenAuthenticationOnBoarding;

		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.TokenAuthenticationOnBoarding;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EdiTokenAuthOnBoardingDataForm(businessEntity as EdiTokenAuthOnBoardingData);
		}
	}
}
