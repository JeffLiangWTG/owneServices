using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusContainerAddInfoValidationTest : AUAddInfoValidationTest
	{
		public void TestSealNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var container = declaration.CusContainers.AddNew();
			container.CO_Seal = "123";
			container.SealStartNumber = "1";
			AssertHasMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealEntered);
			container.CO_Seal = ZString.Empty;
			container.SealStartNumber = "2";
			AssertNoMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealEntered);
			AssertHasMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealEndNumber = "3";
			container.SealStartNumber = "1";
			AssertNoMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealStartNumber = "4";
			AssertHasMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealStartNumber = "1";
			AssertNoMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealEndNumber = ZString.Empty;
			AssertHasMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealEndNumber = "2";
			AssertNoMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealStartNumber = "3";
			container.SealEndNumber = "3";
			AssertNoMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealEndNumber = "2";
			AssertHasMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealStartNumber = ZString.Empty;
			container.SealEndNumber = ZString.Empty;
			AssertNoMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			AssertNoMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealStartNumber = "BBB111";
			container.SealEndNumber = "AAA112";
			AssertNoMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			AssertNoMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealStartNumber = ZString.Empty;
			AssertNoMessageError(container.SealStartNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			AssertNoMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
			container.SealStartNumber = "BBB113";
			container.SealEndNumber = "AAA112";
			AssertHasMessageError(container.SealEndNumberInfo, CusContainerAddInfoValidation.sealNumbersInvalid);
		}
	}
}
