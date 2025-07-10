using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class AsycudaContainerValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckACN_RC_ContainerType()
		{
			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "40GP";
			refcontainer.RC_ISOType = "40GP";
			refcontainer.RC_Length = 0;
			refcontainer.RC_Width = 0;
			refcontainer.RC_Height = 0;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var container = header.Containers.AddNew();

			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);
			refcontainer.RC_ISOType = ZString.Empty;
			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertHasMessageError(container.ACN_RC_ContainerTypeInfo, "Container Type must have an ISO type entered, or alternatively, length, width and height");
			refcontainer.RC_Length = 1m;
			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertHasMessageError(container.ACN_RC_ContainerTypeInfo, "Container Type must have an ISO type entered, or alternatively, length, width and height");
			refcontainer.RC_Width = 2m;
			refcontainer.RC_Height = 3m;
			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);
			refcontainer.RC_ISOType = "40GP";
			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);
		}

		public void TestCheckACN_Seal1()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var container = header.Containers.AddNew();

			container.ACN_Seal1 = ZString.Empty;
			AssertNoNotifications(container.ACN_Seal1Info);
			container.ACN_Seal1 = "1";
			AssertHasMessageError(container.ACN_Seal1Info, "Seal 1 Number must only contain letters or numbers and must be at least 2 and at most 15 characters in length");
			container.ACN_Seal1 = "1b";
			AssertNoNotifications(container.ACN_Seal1Info);
			container.ACN_Seal1 = "2/";
			AssertHasMessageError(container.ACN_Seal1Info, "Seal 1 Number must only contain letters or numbers and must be at least 2 and at most 15 characters in length");
			container.ACN_Seal1 = "12345678901234a";
			AssertNoNotifications(container.ACN_Seal1Info);
			container.ACN_Seal1 = "123456789012345b";
			AssertHasMessageError(container.ACN_Seal1Info, "Seal 1 Number must only contain letters or numbers and must be at least 2 and at most 15 characters in length");
		}

		public void TestCheckACN_Seal2()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var container = header.Containers.AddNew();

			container.ACN_Seal1 = "XX";
			container.ACN_Seal2 = ZString.Empty;
			AssertNoNotifications(container.ACN_Seal2Info);
			container.ACN_Seal2 = "1";
			AssertHasMessageError(container.ACN_Seal2Info, "Seal 2 Number must only contain letters or numbers and must be at least 2 and at most 15 characters in length");
			container.ACN_Seal2 = "1b";
			AssertNoNotifications(container.ACN_Seal2Info);
			container.ACN_Seal2 = "2/";
			AssertHasMessageError(container.ACN_Seal2Info, "Seal 2 Number must only contain letters or numbers and must be at least 2 and at most 15 characters in length");
			container.ACN_Seal2 = "12345678901234a";
			AssertNoNotifications(container.ACN_Seal2Info);
			container.ACN_Seal2 = "123456789012345b";
			AssertHasMessageError(container.ACN_Seal2Info, "Seal 2 Number must only contain letters or numbers and must be at least 2 and at most 15 characters in length");
		}
	}
}
