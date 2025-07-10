using System;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonLocationOfGoodsWrapperTest : WrapperHelperTest<NCTS5CommonLocationOfGoodsWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if location is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "location"), () => new NCTS5CommonLocationOfGoodsWrapper(null));
		}

		public void TestTypeOfLocation()
		{
			goodsLocation.CGL_Type = "B";
			AssertEquals("Expected filled TypeOfLocation", "B", wrapper.TypeOfLocation);
		}

		public void TestQualifierOfIdentification()
		{
			goodsLocation.CGL_Qualifier = "Y";
			AssertEquals("Expected filled QualifierOfIdentification", "Y", wrapper.QualifierOfIdentification);
		}

		public void TestAuthorisationNumber()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_AdditionalIdentifier = "ES00999900";
				AssertEquals("Expected filled AuthorisationNumber", "ES00999900", wrapper.AuthorisationNumber);

				goodsLocation.CGL_AdditionalIdentifier = "ES00999900DECO";
				AssertEquals("Expected filled AuthorisationNumber trimmed when longer than 10", "999900DECO", wrapper.AuthorisationNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			goodsLocation = Factory.New<NctsCusGoodsLocation>();

			wrapper = new NCTS5CommonLocationOfGoodsWrapper(goodsLocation);
		}
		NctsCusGoodsLocation goodsLocation;
		NCTS5CommonLocationOfGoodsWrapper wrapper;

		protected override NCTS5CommonLocationOfGoodsWrapper GetProvider() => wrapper;
	}
}
