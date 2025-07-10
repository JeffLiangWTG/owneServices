using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message871HeaderProviderHelperTest : TestCaseWithFactory
	{
		public void TestSubmitterType()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("Correct value 1 from the Declaration", emcsDeclaration.JE_DeclarantType, helper.SubmitterType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			helper = new Message871HeaderProviderHelper(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		Message871HeaderProviderHelper helper;
	}
}
