using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ARTaxConfigurationCodeDescriptionPairListProviderTest : TaxConfigurationCodeDescriptionPairListProviderTest
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

		new ARTaxConfigurationCodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ARTaxConfigurationCodeDescriptionPairListProvider();
		}

		CodeDescriptionPairList ExpectedTaxConfigurationsForCompanyA()
		{
			var expectedResult = new CodeDescriptionPairList();

			expectedResult.AddPair(taxConfigA1_AR_Code, taxConfigA1_AR_Desc);
			expectedResult.AddPair(taxConfigA2_AR_Code, taxConfigA2_AR_Desc);

			return expectedResult;
		}

		CodeDescriptionPairList ExpectedTaxConfigurationsForCompanyB()
		{
			var expectedResult = new CodeDescriptionPairList();

			expectedResult.AddPair(taxConfigB1_AR_Code, taxConfigB1_AR_Desc);
			expectedResult.AddPair(taxConfigB2_AR_Code, taxConfigB2_AR_Desc);

			return expectedResult;
		}
	}
}
