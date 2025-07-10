using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class WarehouseCommonWrapperTest : WrapperHelperTest<WarehouseCommonWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if authorization is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","authorization"), () => new WarehouseCommonWrapper(null));
		}

		public void TestType()
		{
			CombineAssertions(() =>
			{
				authorization.AGC_Code = "CW1";
				AssertEquals("Expected filled Type for CW1", "R", wrapper.Type);

				authorization.AGC_Code = "DDAP";
				AssertEquals("Expected filled Type for DDAP", "Y", wrapper.Type);

				authorization.AGC_Code = "CW2";
				AssertEquals("Expected filled Type for CW2", "S", wrapper.Type);

				authorization.AGC_Code = "DDA1";
				AssertEquals("Expected filled Type for DDA1", "Y", wrapper.Type);

				authorization.AGC_Code = "CWP";
				AssertEquals("Expected filled Type for CWP", "U", wrapper.Type);

				authorization.AGC_Code = "DDA2";
				AssertEquals("Expected filled Type for DDA2", "Y", wrapper.Type);

				authorization.AGC_Code = "TST";
				AssertEquals("Expected filled Type for TST", "V", wrapper.Type);

				authorization.AGC_Code = "DREF";
				AssertEquals("Expected filled Type for DREF", "Y", wrapper.Type);

				authorization.AGC_Code = "AAA";
				AssertEquals("Expected empty Type when other", ZString.Empty, wrapper.Type);
			});
		}

		public void TestIdentifier()
		{
			authorization.AGC_Number = "reference";
			AssertEquals("Expected filled Identifier", "reference", wrapper.Identifier);
		}

		protected override void SetUp()
		{
			base.SetUp();

			authorization = Factory.New<CusAuthorizationUsage>();

			wrapper = new WarehouseCommonWrapper(authorization);
		}

		CusAuthorizationUsage authorization;
		WarehouseCommonWrapper wrapper;

		protected override WarehouseCommonWrapper GetProvider() => wrapper;
	}
}
