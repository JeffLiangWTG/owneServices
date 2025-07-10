using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACN_Seal1()
		{
			const string expectedMessage = "A Seal is required";
			container.Validation.ValidateAll();
			AssertHasMessageError(container.ACN_Seal1Info, expectedMessage);
			container.ACN_Seal1 = "123";
			AssertNoMessageError(container.ACN_Seal1Info, expectedMessage);
		}

		public void TestCheckACN_RC_ContainerType()
		{
			const string expectedMessage = "Container Type is not related to an ISO type – review container type definition";
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40GP";
			container.ACN_RC_ContainerType = ZGuid.Empty;
			AssertHasMessageErrorContaining("when ContainerType is empty", container.ACN_RC_ContainerTypeInfo, MandatoryValidation.YouHaveNotEntered);
			refContainer.RC_ISOType = ZString.Empty;
			container.ACN_RC_ContainerType = refContainer.PK;
			AssertHasMessageError("when ContainerType without iso", container.ACN_RC_ContainerTypeInfo, expectedMessage);

			refContainer.RC_ISOType = "4ISO";
			container.Validation.ValidateACN_RC_ContainerType();
			AssertNoNotifications("when ContainerType with iso", container.ACN_RC_ContainerTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			container = header.Containers.AddNew();
		}

		AsycudaContainer container;
	}
}
