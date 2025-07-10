using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.OperationalActions;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD.Testing
{
	public class DocAapurerWrapperTest : TestCaseWithFactory
	{
		public void TestDocumentCode()
		{
			applicator.D48DocumentCode = "D48";
			AssertEquals("D48", wrapper.DocumentCode);
		}

		public void TestDocumentReference()
		{
			applicator.ReferenceNumber = "100";
			AssertEquals("100", wrapper.DocumentReference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			applicator = new FrDeclarationCreditD48Applicator(Factory);
			wrapper = new DocAapurerWrapper(applicator);
		}

		FrDeclarationCreditD48Applicator applicator;
		DocAapurerWrapper wrapper;
	}
}
