using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckArrivalMrnFromUser_Format()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMrnFromUser = "1111";
			AssertHasMessageErrorContaining(nctsHeader.ArrivalMrnFromUserInfo, "For NCTS,");
			AssertHasMessageErrorContaining(nctsHeader.ArrivalMrnFromUserInfo, "two numbers");
			AssertHasMessageErrorContaining(nctsHeader.ArrivalMrnFromUserInfo, "two letters");
			AssertHasMessageErrorContaining(nctsHeader.ArrivalMrnFromUserInfo, "thirteen alphanumeric characters");
			AssertHasMessageErrorContaining(nctsHeader.ArrivalMrnFromUserInfo, "one number check digit");

			nctsHeader.ArrivalMrnFromUser = "17DE123456789123E3";
			AssertNoMessageErrors(nctsHeader.ArrivalMrnFromUserInfo);
		}

		public void TestCheckDestinationTraderCustomCode()
		{
			const string messageString = "[NR0057] If Destination Trader is not Message Sender then EORI-Number, Branch and Participant Identification Number of sending Company must be maintained in Registry Setting: Customs/Country or Region Specific/ATLAS.";

			var orgHeader = Factory.New<OrgHeader>();
			var orgCusCode = Factory.New<OrgCusCode>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			orgHeader.OH_Code = "ACN";
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			orgCusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.CWC;
			nctsHeader.DestinationTrader.Validation.ValidateOrganisationPK();
			AssertNullOrEmpty("Precondition: ATLASEORIBranchSuffix", DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.Value);
			AssertNullOrEmpty("Precondition: ATLASEORINumber", DECustomsDataRegistry.Instance.ATLASEORINumber.Value);
			AssertNullOrEmpty("Precondition: ATLASParticipantIdentificationNumber", DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.Value);
			AssertEquals("Precondition: CustomsCodes", 0, nctsHeader.DestinationTrader.Organisation.CustomsCodes.Count);
			AssertHasMessageError(nctsHeader.DestinationTrader.OrganisationPKInfo, messageString);

			var customsCode = nctsHeader.DestinationTrader.Organisation.CustomsCodes.AddNew();
			customsCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;
			nctsHeader.DestinationTrader.Validation.ValidateOrganisationPK();
			AssertNullOrEmpty("Precondition: ATLASEORIBranchSuffix", DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.Value);
			AssertNullOrEmpty("Precondition: ATLASEORINumber", DECustomsDataRegistry.Instance.ATLASEORINumber.Value);
			AssertNullOrEmpty("Precondition: ATLASParticipantIdentificationNumber", DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.Value);
			AssertNullOrEmpty("Precondition: OK_CustomsRegNo", customsCode.OK_CustomsRegNo);
			AssertHasMessageError(nctsHeader.DestinationTrader.OrganisationPKInfo, messageString);

			customsCode.OK_CustomsRegNo = "1";
			nctsHeader.DestinationTrader.Validation.ValidateOrganisationPK();
			AssertNullOrEmpty("Precondition: ATLASEORIBranchSuffix", DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.Value);
			AssertNullOrEmpty("Precondition: ATLASEORINumber", DECustomsDataRegistry.Instance.ATLASEORINumber.Value);
			AssertNullOrEmpty("Precondition: ATLASParticipantIdentificationNumber", DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.Value);
			AssertNoMessageError(nctsHeader.DestinationTrader.OrganisationPKInfo, messageString);

			customsCode.OK_CustomsRegNo = ZString.Empty;
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "1234"))
			{
				AssertNullOrEmpty("Precondition: ATLASEORINumber", DECustomsDataRegistry.Instance.ATLASEORINumber.Value);
				AssertNullOrEmpty("Precondition: ATLASParticipantIdentificationNumber", DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.Value);
				nctsHeader.DestinationTrader.Validation.ValidateOrganisationPK();
				AssertHasMessageError(nctsHeader.DestinationTrader.OrganisationPKInfo, messageString);
			}

			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "123"))
			{
				AssertNullOrEmpty("Precondition: ATLASEORIBranchSuffix", DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.Value);
				AssertNullOrEmpty("Precondition: ATLASParticipantIdentificationNumber", DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.Value);
				AssertNullOrEmpty("Precondition: OK_CustomsRegNo", customsCode.OK_CustomsRegNo);
				nctsHeader.DestinationTrader.Validation.ValidateOrganisationPK();
				AssertHasMessageError(nctsHeader.DestinationTrader.OrganisationPKInfo, messageString);
			}

			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "1234567890123456789012345"))
			{
				AssertNullOrEmpty("Precondition: ATLASEORIBranchSuffix", DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.Value);
				AssertNullOrEmpty("Precondition: ATLASEORINumber", DECustomsDataRegistry.Instance.ATLASEORINumber.Value);
				AssertNullOrEmpty("Precondition: OK_CustomsRegNo", customsCode.OK_CustomsRegNo);
				nctsHeader.DestinationTrader.Validation.ValidateOrganisationPK();
				AssertHasMessageError(nctsHeader.DestinationTrader.OrganisationPKInfo, messageString);
			}
		}

		public void TestValidatePrincipalHasAuthorizationACR()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;

			CombineAssertions(() =>
			{
				nctsHeader.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No Principal, is SimplifiedNctsProcedure", nctsHeader.Principal.OrganisationPKInfo, PrincipalAuthorizationMessageError);

				var principal = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.Principal.OrganisationPK = principal.PK;
				AssertHasMessageError("Principal without ACR authorization, is SimplifiedNctsProcedure", nctsHeader.Principal.OrganisationPKInfo, PrincipalAuthorizationMessageError);

				principal.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "DEACR1");
				nctsHeader.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Principal with ACR authorisation, is SimplifiedNctsProcedure", nctsHeader.Principal.OrganisationPKInfo, PrincipalAuthorizationMessageError);
			});
		}

		public void TestValidatePrincipalHasAuthorizationACR_SimplifiedProcedure()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var principal = Factory.NewWithValidTestData<OrgHeader>();

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
				nctsHeader.Principal.OrganisationPK = principal.PK;
				AssertNoMessageError("Principal without ACR authorization, is not SimplifiedNctsProcedure", nctsHeader.Principal.OrganisationPKInfo, PrincipalAuthorizationMessageError);

				nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
				nctsHeader.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Principal without ACR authorization, is SimplifiedNctsProcedure", nctsHeader.Principal.OrganisationPKInfo, PrincipalAuthorizationMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
		}
		NctsHeader nctsHeader;

		const string PrincipalAuthorizationMessageError = "Principal must have an authorization of type 'ACR' to use Simplified Procedure.";
	}
}
