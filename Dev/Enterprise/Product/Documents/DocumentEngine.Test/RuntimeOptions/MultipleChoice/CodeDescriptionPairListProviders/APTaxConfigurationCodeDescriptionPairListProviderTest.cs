using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class APTaxConfigurationCodeDescriptionPairListProviderTest : TaxConfigurationCodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA.PK.ToGuid(), Guid.Empty))
			{
				var actualResult = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
				var expectedResult = ExpectedTaxConfigurationsForCompanyA();

				AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchB.PK.ToGuid(), Guid.Empty))
			{
				var actualResult = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
				var expectedResult = ExpectedTaxConfigurationsForCompanyB();

				AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
			}
		}

		new APTaxConfigurationCodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new APTaxConfigurationCodeDescriptionPairListProvider();
		}

		CodeDescriptionPairList ExpectedTaxConfigurationsForCompanyA()
		{
			var expectedResult = new CodeDescriptionPairList();

			expectedResult.AddPair(taxConfigA1_AP_Code, taxConfigA1_AP_Desc);
			expectedResult.AddPair(taxConfigA2_AP_Code, taxConfigA2_AP_Desc);

			return expectedResult;
		}

		CodeDescriptionPairList ExpectedTaxConfigurationsForCompanyB()
		{
			var expectedResult = new CodeDescriptionPairList();

			expectedResult.AddPair(taxConfigB1_AP_Code, taxConfigB1_AP_Desc);
			expectedResult.AddPair(taxConfigB2_AP_Code, taxConfigB2_AP_Desc);

			return expectedResult;
		}
	}
}
