using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonAuthorisationWrapperTest : WrapperHelperTest<NCTS5CommonAuthorisationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if authorization is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "authorization"), () => new NCTS5CommonAuthorisationWrapper(null, 1));
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestType()
		{
			authorization.AGC_Code = "TST";
			authorization.AGC_Number = "Test";
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			_ = helper.CreateCusCodeType("AUTH", "Authorisation");
			_ = helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			_ = helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", isReadonly: true);
			_ = helper.CreateCusMap("EUNAU", "SAS", "C019", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			Factory.Save();

			CombineAssertions(() =>
			{
				authorization.AGC_Code = "SAS";
				AssertEquals("Prereq: CustomsCode is not empty", "C019", authorization.CustomsCode);
				AssertEquals("Expected filled Type with value in CustomsCode", "C019", wrapper.Type);

				authorization.AGC_Code = "REP";
				AssertEquals("Prereq: CustomsCode is empty", ZString.Empty, authorization.CustomsCode);
				AssertEquals("Expected filled Type with value in AGC_Code", "REP", wrapper.Type);
			});
		}

		public void TestReferenceNumber()
		{
			authorization.AGC_Number = "reference";
			AssertEquals("Expected filled ReferenceNumber", "reference", wrapper.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			authorization = header.CusAuthorizationUsages.AddNew();
			wrapper = new NCTS5CommonAuthorisationWrapper(authorization, 1);
		}

		EU.NCTS.Business.CusAuthorizationUsage authorization;
		NCTS5CommonAuthorisationWrapper wrapper;

		protected override NCTS5CommonAuthorisationWrapper GetProvider() => wrapper;
	}
}
