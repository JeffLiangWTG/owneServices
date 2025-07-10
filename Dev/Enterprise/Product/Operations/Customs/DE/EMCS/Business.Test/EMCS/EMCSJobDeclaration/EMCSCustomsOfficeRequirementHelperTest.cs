using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSCustomsOfficeRequirementHelperTest : TestCaseWithFactory
	{
		public void TestDispatchOfficeRequirement_NonConsolidated()
		{
			var customsOfficeRequirementHelper = new EMCSCustomsOfficeRequirementHelper(Factory.New<EMCSJobDeclaration>());
			var dispatchOfficeRequirement = customsOfficeRequirementHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfDispatch);
			CombineAssertions(() =>
			{
				AssertEquals("Office Role", EuOfficeCodesTypes.Codes.Excise, dispatchOfficeRequirement.OfficeRolesForLookup.Single());
				AssertEquals("Mandatory", true, dispatchOfficeRequirement.IsMandatory);
				AssertEquals("Local Country", false, dispatchOfficeRequirement.IsLocalCountryOnly);
			});
		}

		public void TestDispatchOfficeRequirement_Consolidated()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.SetConsolidatedDocument();
			var customsOfficeRequirementHelper = new EMCSCustomsOfficeRequirementHelper(declaration);
			var dispatchOfficeRequirement = customsOfficeRequirementHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfDispatch);
			AssertEquals("Local Country", true, dispatchOfficeRequirement.IsLocalCountryOnly);
		}
	}
}
