using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.Business.Testing.ValidationTestHelper;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class CusEntryInstructionJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			AssertYouHaveNotEnteredMessageError(instruction.MainAccountingAddress.OrganisationPKInfo, "You have not entered a Main Accounting Address.");
		}
	}
}
