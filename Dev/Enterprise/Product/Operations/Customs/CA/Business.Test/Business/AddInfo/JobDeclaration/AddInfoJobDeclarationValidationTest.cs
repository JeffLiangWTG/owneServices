using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	class AddInfoJobDeclarationValidationTest : CAAddInfoValidationTest<AddInfoJobDeclaration>
	{
		[TestDate(2020, 09, 16)]
		public void TestCheckCA_EstReleaseDate()
		{
			declaration.CA_EstReleaseDate = new ZDateTime(2019, 09, 15);
			AssertHasWarning(declaration.CA_EstReleaseDateInfo, "The date '15-Sep-2019' is more than 1 year old.");

			declaration.CA_EstReleaseDate = new ZDateTime(2020, 09, 15);
			AssertNoWarning(declaration.CA_EstReleaseDateInfo, "The date '15-Sep-2019' is more than 1 year old.");
		}

		protected override AddInfoJobDeclaration GetNewAddInfo()
		{
			return new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
		}

		protected JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
	}
}
