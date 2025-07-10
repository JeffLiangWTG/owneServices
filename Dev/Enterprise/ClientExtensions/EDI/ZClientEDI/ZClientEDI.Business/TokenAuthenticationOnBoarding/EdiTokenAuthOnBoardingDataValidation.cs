//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiTokenAuthOnBoardingDataValidation
//
//    This class should be used for overriding validation in AutoEdiTokenAuthOnBoardingDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business
{
	public class EdiTokenAuthOnBoardingDataValidation : AutoEdiTokenAuthOnBoardingDataValidation
	{
		public EdiTokenAuthOnBoardingDataValidation(AutoEdiTokenAuthOnBoardingData parent) : base(parent)
		{
		}

		EdiTokenAuthOnBoardingData EdiTokenAuthOnBoardingData => (EdiTokenAuthOnBoardingData)Parent;

		protected override void CheckTOD_IDT()
		{
			base.CheckTOD_IDT();
			if (EdiTokenAuthOnBoardingData.Tenant != null && !EdiTokenAuthOnBoardingData.Tenant.IDT_Onboarding)
			{
				Parent.TOD_IDTInfo.AddError(Res.GetString("B363645E-1753-4662-A466-E7760FAEE93A", "Only onboarding related tenants can be selected."));
			}
		}

		protected override void CheckTOD_ClaimMappingIdentifier()
		{
			base.CheckTOD_ClaimMappingIdentifier();
			ListValidation.ErrorIfInvalidCode(Parent.TOD_ClaimMappingIdentifierInfo);
		}

		protected override void CheckTOD_OIDCServer()
		{
			base.CheckTOD_OIDCServer();
			MandatoryValidation.CheckEntered(Parent.TOD_OIDCServerInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TOD_OIDCServerInfo);
		}

		protected override void CheckTOD_Status()
		{
			base.CheckTOD_Status();
			ListValidation.ErrorIfInvalidCode(Parent.TOD_StatusInfo);
		}

		protected override void CheckTOD_LE()
		{
			base.CheckTOD_LE();
			var filter = new ZQuery(EdiTokenAuthOnBoardingDataSchema.TOD_LE, Parent.TOD_LE);
			filter.AddToFilter(EdiTokenAuthOnBoardingDataSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			var isInDatabase = Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(EdiTokenAuthOnBoardingData)), filter);

			if (isInDatabase)
			{
				Parent.TOD_LEInfo.AddError(Res.GetString("EBB0DE9B-A95E-43C5-8EF1-CCB0CDF567D3", "A Token Authentication Onboarding form already exists for the selected customer. Please use the existing form for any changes. Only one Token Authentication Onboarding form can be created per customer."));
			}
		}

		protected override void CheckTOD_LEIsNotEmpty()
		{
			if (ZGuid.Empty.Equals(Parent.TOD_LE))
			{
				Parent.TOD_LEInfo.AddError(Res.GetString("3367B63B-6865-43AD-9BFA-A547F04A50F9", "Please make sure the selected incident has an enterprise code attached to it. A Token Authentication Onboarding form can only be created if an enterprise code is assigned."));
			}
		}

		protected override void CheckTOD_ValidTokenIssuerPrefix()
		{
			base.CheckTOD_ValidTokenIssuerPrefix();
			if (Enterprise.Registry.Business.OIDCServerTypesList.Codes.Okta == Parent.TOD_OIDCServer)
			{
				MandatoryValidation.CheckEntered(Parent.TOD_ValidTokenIssuerPrefixInfo);
			}
		}

		public override void ValidateAll()
		{
			using ((Parent as ISingleElementListInternal).SuspendListChanged())
			{
				base.ValidateAll();
				ValidateVerificationResultDetails();
			}
		}

		public void ValidateVerificationResultDetails()
		{
			ValidateCalculatedProperty(EdiTokenAuthOnBoardingData.VerificationResultDetailsInfo);
		}

		protected void CheckVerificationResultDetails()
		{
			if (!EdiTokenAuthOnBoardingData.VerificationResultDetails.IsEmpty)
			{
				EdiTokenAuthOnBoardingData.VerificationResultInfo.AddMessageError(EdiTokenAuthOnBoardingData.VerificationResultDetails);
			}
			else if (EdiTokenAuthOnBoardingData.VerificationResult == EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified)
			{
				EdiTokenAuthOnBoardingData.VerificationResultInfo.AddMessageError("Verification has not been run yet");
			}
		}
	}
}
