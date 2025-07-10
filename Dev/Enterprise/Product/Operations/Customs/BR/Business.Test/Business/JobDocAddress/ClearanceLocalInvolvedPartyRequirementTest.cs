using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ClearanceLocalInvolvedPartyRequirementTest : TestCaseWithFactory
	{
		public void TestGetGovRegTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var docAddress = declaration.ClearanceLocalInvolvedParty;
			AssertEquals(2, docAddress.Lookups.GovRegNumTypes.Count);
			AssertEquals("CJN, CPF", docAddress.Lookups.GovRegNumTypes.CodesAsString);
		}

		public void TestGetRegNumResult()
		{
			var testOrg = Factory.New<OrgHeader>();
			testOrg.PrimaryRegistrationNumber.Number = "97442770000126";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var docAddress = declaration.ClearanceLocalInvolvedParty;
			docAddress.OrganisationPK = testOrg.PK;

			var requirement = docAddress.Requirement;
			AssertEquals("97442770000126", requirement.GetRegistrationNumberResult(docAddress).RegistrationNumber);
			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, requirement.GetRegistrationNumberResult(docAddress).RegistrationNumberType);
		}

		public void TestDocAddressType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var docAddress = declaration.ClearanceLocalInvolvedParty;
			AssertEquals("Address Type should be 163", DocAddressType.ClearanceLocalInvolvedParty, docAddress.DocAddressType);
		}
	}
}
