using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class BaseCusSeaManOBLHeaderValidationTestCase : TestCaseWithFactory
	{
		public void TestMessageValidation()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			BaseCusSeaManOBLHeaderValidation validation = new BaseCusSeaManOBLHeaderValidation(header);
			AssertNotNull("nullness", validation.MessageValidation);
			AssertEquals(typeof(MessageValidation), validation.MessageValidation.GetType());
		}
	}
}
