using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class TransportEquipmentProviderTest : DataProviderTestCase<TransportEquipmentProvider>
	{
		public void TestConstructor()
		{
			AssertNull(TransportEquipmentProvider.NewOrNull(null));
		}

		public void TestContainerIdentificationNumber()
		{
			const string expected = "ID1234";
			container.ACN_ContainerNumber = expected;
			AssertEquals(expected, Provider.ContainerIdentificationNumber);
		}

		public void TestContainerPackedStatus()
		{
			var expected = "A";
			container.ACN_EmptyFullIndicator = "MT";

			CombineAssertions(() =>
			{
				AssertEquals(expected, Provider.ContainerPackedStatus);

				expected = "B";
				container.ACN_EmptyFullIndicator = string.Empty;
				AssertEquals(expected, Provider.ContainerPackedStatus);
				container.ACN_EmptyFullIndicator = "FCL";
				AssertEquals(expected, Provider.ContainerPackedStatus);
				container.ACN_EmptyFullIndicator = "LCL";
				AssertEquals(expected, Provider.ContainerPackedStatus);
			});
		}

		public void TestContainerSizeAndType()
		{
			const string expected = "MYSIZETYPE";
			var containerTypeWithSizeType = Factory.New<RefContainer>();
			containerTypeWithSizeType.RC_Code = "CT2";
			containerTypeWithSizeType.RC_ISOEquipmentSizeTypeCode = expected;
			container.ACN_RC_ContainerType = containerTypeWithSizeType.PK;

			AssertEquals(expected, Provider.ContainerSizeAndType);
		}

		public void TestContainerType_Null()
		{
			AssertNull(Provider.ContainerSizeAndType);
		}

		public void TestContainerSupplierType()
		{
			container.ACN_IsShipperOwned = true;

			CombineAssertions(() =>
			{
				AssertEquals("1", Provider.ContainerSupplierType);

				container.ACN_IsShipperOwned = false;
				AssertEquals("2", Provider.ContainerSupplierType);
			});
		}

		public void TestNumberOfSealsValue()
		{
			AssertNullOrEmpty("Precondition", container.ACN_Seal1);
			AssertNullOrEmpty("Precondition", container.ACN_Seal2);
			AssertNullOrEmpty("Precondition", container.ACN_Seal3);
			AssertEquals(0m, Provider.NumberOfSealsValue);

			container.ACN_Seal1 = "S1";
			container.ACN_Seal2 = "S2";
			container.ACN_Seal3 = "S3";

			AssertEquals(3m, GetProvider().NumberOfSealsValue);
		}

		public void TestSealIdentifiers()
		{
			AssertNullOrEmpty("Precondition", container.ACN_Seal1);
			AssertNullOrEmpty("Precondition", container.ACN_Seal2);
			AssertNullOrEmpty("Precondition", container.ACN_Seal3);
			AssertEquals(0, Provider.SealIdentifiers.Count);

			const string seal = "S1";
			const string seal2 = "S2";
			const string seal3 = "S3";
			container.ACN_Seal1 = seal;
			container.ACN_Seal2 = seal2;
			container.ACN_Seal3 = seal3;

			AssertContainsExactElementsInExactOrder(new[] { seal, seal2, seal3 }, GetProvider().SealIdentifiers.ToArray());
		}

		protected override TransportEquipmentProvider GetProvider() => TransportEquipmentProvider.NewOrNull(container);

		protected override void SetUp()
		{
			base.SetUp();

			container = Factory.New<AsycudaContainer>();
		}

		AsycudaContainer container;
	}
}
