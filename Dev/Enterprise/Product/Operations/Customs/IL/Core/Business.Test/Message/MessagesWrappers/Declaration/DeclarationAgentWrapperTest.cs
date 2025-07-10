using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class DeclarationAgentWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationAgent>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarationAgentWrapper.NewOrNull(null));
			AssertNotNull(Provider);
		}

		public void TestID()
		{
			AssertEquals("560122458", Provider.ID.Value);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "560122459", Core.Constants.CountryCodes.Latvia);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var wrapper = DeclarationAgentWrapper.NewOrNull(declaration);
			AssertNull("ID should be null when no IL VAT defined", wrapper.ID);
		}

		public void TestRoleCode()
		{
			AssertEquals("1", Provider.RoleCode.Value);
		}

		protected override IDeclarationAgent GetProvider()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "560122458", Core.Constants.CountryCodes.Israel);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			return DeclarationAgentWrapper.NewOrNull(declaration);
		}
	}
}
