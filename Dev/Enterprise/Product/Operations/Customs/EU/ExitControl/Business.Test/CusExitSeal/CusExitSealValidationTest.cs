using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitSealValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSequenceNumberNo0NoDuplicatedWihtoutControlSequence()
		{
			var header = Factory.New<CusExitHeaderWithoutSequenceForTest>();
			var container = header.CusExitContainers.AddNew();
			CombineAssertions(() =>
			{
				var seal1 = container.AllSealNumbers.AddNew();
				seal1.BK_SequenceNumber = 0;
				AssertNoErrors(seal1);
				seal1.BK_SequenceNumber = 1;
				var seal2 = container.AllSealNumbers.AddNew();
				seal1.BK_SequenceNumber = 2;

				var seal3 = container.AllSealNumbers.AddNew();
				seal3.BK_SequenceNumber = 2;
				AssertNoErrors(seal3);
			});
		}

		public void TestSequenceNumberNo0NoDuplicatedWihtControlSequence()
		{
			var valueCannotBeZeroMessage = "Sequence Number cannot be zero.";
			var messageErrorNotBeDuplicated = "Sequence Number (2) should not be duplicated";

			var header = Factory.New<CusExitHeaderWithSequenceForTest>();
			var container = header.CusExitContainers.AddNew();
			CombineAssertions(() =>
			{
				var seal1 = container.AllSealNumbers.AddNew();
				seal1.BK_SequenceNumber = 0;
				AssertHasErrorContaining("The test is for control of sequence 0", seal1.BK_SequenceNumberInfo, valueCannotBeZeroMessage);
				seal1.BK_SequenceNumber = 1;
				AssertNoErrorContaining("No test error 0", seal1.BK_SequenceNumberInfo, valueCannotBeZeroMessage);
				var seal2 = container.AllSealNumbers.AddNew();
				seal1.BK_SequenceNumber = 2;

				var seal3 = container.AllSealNumbers.AddNew();
				seal3.BK_SequenceNumber = 2;
				AssertHasError("The test is for control of sequence duplicated", seal3.BK_SequenceNumberInfo, messageErrorNotBeDuplicated);
				seal3.BK_SequenceNumber = 3;
				AssertNoError("No test Duplicated", seal3.BK_SequenceNumberInfo, messageErrorNotBeDuplicated);

				seal1.Validation.ValidateBK_SealNumber();
				seal2.Validation.ValidateBK_SealNumber();
				seal3.Validation.ValidateBK_SealNumber();
				AssertNoErrorContaining("No test error 0 in seal 1", seal1.BK_SequenceNumberInfo, valueCannotBeZeroMessage);
				AssertNoErrorContaining("No test error 0 in seal 2", seal2.BK_SequenceNumberInfo, valueCannotBeZeroMessage);
				AssertNoErrorContaining("No test error 0 in seal 3", seal3.BK_SequenceNumberInfo, valueCannotBeZeroMessage);
			});
		}

		public void TestDuplicateSealNumber()
		{
			var messageError = "is specified more than once.";
			(var container, _) = CusExitContainerTest.GetNewBusinessObject(Factory);
			var seal1 = container.AllSealNumbers.AddNew();
			seal1.BK_SealNumber = "SL1";
			var seal2 = container.AllSealNumbers.AddNew();
			seal2.BK_SealNumber = "SL2";
			var seal3 = container.AllSealNumbers.AddNew();
			seal3.BK_SealNumber = "SL2";
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Seal 3 duplicate with 2", seal3.BK_SealNumberInfo, messageError);
				seal3.BK_SealNumber = "SL1";
				AssertHasMessageErrorContaining("Seal 3 duplicate with 1", seal3.BK_SealNumberInfo, messageError);
				seal1.BK_SealNumber = "AH3";

				seal1.Validation.ValidateBK_SealNumber();
				seal2.Validation.ValidateBK_SealNumber();
				seal3.Validation.ValidateBK_SealNumber();
				AssertNoMessageErrorContaining("No duplication when different seal number", seal1.BK_SealNumberInfo, messageError);
				AssertNoMessageErrorContaining("No duplication when different seal number", seal2.BK_SealNumberInfo, messageError);
				AssertNoMessageErrorContaining("No duplication when different seal number", seal3.BK_SealNumberInfo, messageError);
			});
		}

		public void TestCheckBK_UnloadingState() => CombineAssertions(() =>
		{
			var seal = Factory.New<CusExitHeader>().CusExitContainers.AddNew().AllSealNumbers.AddNew();
			seal.BK_UnloadingState = "asd";
			AssertListValidationInvalidCodeError(seal.BK_UnloadingStateInfo, false);

			var sealUcc6 = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew().AllSealNumbers.AddNew();
			sealUcc6.BK_UnloadingState = ZString.Empty;
			AssertListValidationInvalidCodeError(sealUcc6.BK_UnloadingStateInfo, false);

			sealUcc6.BK_UnloadingState = "asd";
			AssertListValidationInvalidCodeError(sealUcc6.BK_UnloadingStateInfo, true);

			sealUcc6.BK_UnloadingState = DiscrepanciesStatusCodeList.Codes.Missing;
			AssertListValidationInvalidCodeError(sealUcc6.BK_UnloadingStateInfo, false);
		});

		class CusExitHeaderWithSequenceForTest : CusExitHeader
		{
			public CusExitHeaderWithSequenceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ShouldHaveSeqNumInContainersOrEquipmentsAndSealsCore => true;
		}

		class CusExitHeaderWithoutSequenceForTest : CusExitHeader
		{
			public CusExitHeaderWithoutSequenceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ShouldHaveSeqNumInContainersOrEquipmentsAndSealsCore => false;
		}
	}
}
