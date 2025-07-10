using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusSealValidationTest : TestCaseWithFactory
	{
		public void TestBK_UnloadingState_InvalidCode()
		{
			var seal = Factory.New<CusSeal>();
			CombineAssertions(() =>
			{
				AssertNoNotifications(seal.BK_UnloadingStateInfo);
				seal.BK_UnloadingState = "XXX";
				AssertHasErrorContaining(seal.BK_UnloadingStateInfo, ListValidation.InvalidCodeError);
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
				AssertNoErrorContaining(seal.BK_UnloadingStateInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckBK_SealNumber()
		{
			var expectedWarning = "Duplicate Seal Number entered.";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var seal1 = container1.AdditionalSeals.AddNew();
			var seal2 = container2.AdditionalSeals.AddNew();

			container1.Seal1 = "123";
			container1.Seal2 = "456";
			seal1.BK_SealNumber = "789";

			container2.Seal1 = "321";
			container2.Seal2 = "654";
			seal2.BK_SealNumber = "987";

			CombineAssertions(() =>
			{
				var seal = container1.AdditionalSeals.AddNew();
				seal.BK_SealNumber = "123";
				AssertHasWarning("Duplicate container1.Seal1", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "789";
				AssertHasWarning("Duplicate in container1.AdditionalSeals", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "654";
				AssertHasWarning("Duplicate container2.Seal2", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "987";
				AssertHasWarning("Duplicate in container2.AdditionalSeals", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "ABC";
				AssertNoWarning("Unique seal number", seal.BK_SealNumberInfo, expectedWarning);
			});
		}

		public void TestCheckBK_SealNumber_For_NctsArrivalHeaderContainer()
		{
			var expectedWarning = "Duplicate Seal Number entered.";
			var headerContainer = Factory.New<NctsArrivalHeaderContainer>();
			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var seal1 = headerContainer.Seals.AddNew();
			var seal2 = headerContainer.Seals.AddNew();

			CombineAssertions(() =>
			{
				seal1.BK_SealNumber = "111";
				seal2.BK_SealNumber = "111";
				AssertHasWarning("The seal number duplicates Seal1", seal2.BK_SealNumberInfo, expectedWarning);

				seal2.BK_SealNumber = "112";
				AssertNoWarning("The Seal number is unique", seal2.BK_SealNumberInfo, expectedWarning);
			});
		}

		public void TestCheckBK_SealNumber_ConditionNR0029()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			var seal = container.AdditionalSeals.AddNew();

			using var deciderTestContext = new CusSealValidationDeciderTestContext<ICusSealValidationDecider>(Factory);
			deciderTestContext.EnableRule(x => x.IsRuleNR0029Active);

			const string errorMessage = "[NR0029] If Seal Unloaded State is NEW, Seal Number cannot be empty.";

			CombineAssertions(() =>
			{
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
				seal.BK_SealNumber = ZString.Empty;
				seal.Validation.ValidateBK_SealNumber();
				AssertHasMessageError("If BK_UnloadingState is set to NEW and BK_SealNumber is empty, then the error message NR0029 should be displayed.",
					seal.BK_SealNumberInfo, errorMessage);

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
				seal.Validation.ValidateBK_SealNumber();
				AssertNoMessageError("If BK_UnloadingState is set to MIS and BK_SealNumber is empty, then the error message NR0029 should not be displayed.",
					seal.BK_SealNumberInfo, errorMessage);

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
				seal.BK_SealNumber = "1";
				seal.Validation.ValidateBK_SealNumber();
				AssertNoMessageError("If BK_UnloadingState is set to NEW and BK_SealNumber is not empty, then the error message NR0029 should not be displayed.",
					seal.BK_SealNumberInfo, errorMessage);

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
				seal.Validation.ValidateBK_SealNumber();
				AssertNoMessageError("If BK_UnloadingState is set to MIS and BK_SealNumber is not empty, then the error message NR0029 should not be displayed.",
					seal.BK_SealNumberInfo, errorMessage);
			});
		}
	}
}
