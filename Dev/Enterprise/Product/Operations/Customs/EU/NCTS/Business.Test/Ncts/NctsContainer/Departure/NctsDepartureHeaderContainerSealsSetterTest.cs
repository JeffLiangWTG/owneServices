using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureHeaderContainerSealsSetterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When headerContainer is null",
				() => new NctsDepartureHeaderContainerSealsSetter(headerContainer: null));
		}

		public void TestSetInFirstAvailableSlot_DoNotSetIfEmpty()
		{
			nctsHeaderContainer.BC_Seal1 = "S1";

			headerContainerSealsSetter.SetInFirstAvailableSlot("");
			CombineAssertions(() => AssertSeals(nctsHeaderContainer, "S1", "", Array.Empty<string>()));
		}

		public void TestSetInFirstAvailableSlot_DoNotSetIfDuplicated()
		{
			nctsHeaderContainer.BC_Seal2 = "S2";

			headerContainerSealsSetter.SetInFirstAvailableSlot("S2");
			CombineAssertions(() => AssertSeals(nctsHeaderContainer, "", "S2", Array.Empty<string>()));
		}

		public void TestSetInFirstAvailableSlot_FillInBC_Seal1()
		{
			nctsHeaderContainer.BC_Seal1 = "";
			nctsHeaderContainer.BC_Seal2 = "S2";
			nctsHeaderContainer.AdditionalSeals.AddNew().BK_SealNumber = "S3";

			headerContainerSealsSetter.SetInFirstAvailableSlot("S1");
			CombineAssertions(() => AssertSeals(nctsHeaderContainer, "S1", "S2", new[] { "S3" }));
		}

		public void TestSetInFirstAvailableSlot_FillInBC_Seal2()
		{
			nctsHeaderContainer.BC_Seal1 = "S1";
			nctsHeaderContainer.BC_Seal2 = "";
			nctsHeaderContainer.AdditionalSeals.AddNew().BK_SealNumber = "S3";

			headerContainerSealsSetter.SetInFirstAvailableSlot("S2");
			CombineAssertions(() => AssertSeals(nctsHeaderContainer, "S1", "S2", new[] { "S3" }));
		}

		public void TestSetInFirstAvailableSlot_FillInAdditionalSeals()
		{
			nctsHeaderContainer.BC_Seal1 = "S1";
			nctsHeaderContainer.BC_Seal2 = "S2";

			headerContainerSealsSetter.SetInFirstAvailableSlot("S3");
			CombineAssertions(() => AssertSeals(nctsHeaderContainer, "S1", "S2", new[] { "S3" }));
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeaderContainer = Factory.New<NctsDepartureHeaderContainer>();
			headerContainerSealsSetter = new NctsDepartureHeaderContainerSealsSetter(nctsHeaderContainer);
		}

		NctsDepartureHeaderContainer nctsHeaderContainer;
		NctsDepartureHeaderContainerSealsSetter headerContainerSealsSetter;

		static void AssertSeals(
			NctsDepartureHeaderContainer container,
			string expectedSeal1,
			string expectedSeal2,
			string[] expectedAdditionalSeals)
		{
			AssertEquals("BC_Seal1", expectedSeal1, container.BC_Seal1);
			AssertEquals("BC_Seal2", expectedSeal2, container.BC_Seal2);
			AssertContainsExactElementsInAnyOrder("AdditionalSeals", expectedAdditionalSeals, container.AdditionalSeals.Select(x => x.BK_SealNumber));
		}
	}
}
