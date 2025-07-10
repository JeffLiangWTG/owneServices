using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACN_RC_ContainerType()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40GP";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.NonContainerised;

			var container = header.Containers.AddNew();
			container.ACN_RC_ContainerType = refContainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);

			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;

			container.ACN_RC_ContainerType = refContainer.PK;
			AssertHasMessageErrorContaining(container.ACN_RC_ContainerTypeInfo, "The Container Type selected should have a Customs Container Code entered");

			var containerMap = refContainer.CodeMapCollection.AddNew();
			containerMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Colombia;
			containerMap.RCM_Code = "1";

			container.ACN_RC_ContainerType = refContainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);
		}
	}
}
