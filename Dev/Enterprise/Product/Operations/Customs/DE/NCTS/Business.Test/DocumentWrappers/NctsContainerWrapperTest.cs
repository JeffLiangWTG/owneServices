using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(NctsContainerWrapper))]
	sealed class NctsContainerWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContainerNumber()
		{
			container.BC_ContainerNum = "ABC123";
			var wrapper = NctsContainerWrapper.New(container);
			AssertEquals("ABC123", wrapper.ContainerNumber);
		}

		public void TestSeals()
		{
			CombineAssertions(() =>
			{
				container.BC_Seal1 = "seal1";

				var wrapper = NctsContainerWrapper.New(container);
				AssertEquals("seal1", wrapper.Seals);

				container.BC_Seal2 = "seal2";
				AssertEquals("seal1, seal2", wrapper.Seals);

				var seal3 = container.AdditionalSeals.AddNew();
				seal3.BK_SealNumber = "seal3";

				var seal4 = container.AdditionalSeals.AddNew();
				seal4.BK_SealNumber = "seal4";

				AssertEquals("seal1, seal2, seal3, seal4", wrapper.Seals);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => NctsContainerWrapper.New(Factory.New<NctsDepartureHeaderContainer>());

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			container = header.DepartureHeaderContainers.AddNew();
		}
		NctsDepartureHeaderContainer container;
	}
}
