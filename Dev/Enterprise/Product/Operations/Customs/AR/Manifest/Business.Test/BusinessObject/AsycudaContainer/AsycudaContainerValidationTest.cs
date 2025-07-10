using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class AsycudaContainerValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckACN_ExpireDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			container.ACN_ACEP = ZString.Empty;
			container.ACN_ExpireDate = ZDate.Empty;
			AssertNoMessageErrorContaining(container.ACN_ExpireDateInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			container.ACN_ACEP = ZString.Empty;
			container.ACN_ExpireDate = ZDate.Empty;
			AssertHasMessageErrorContaining(container.ACN_ExpireDateInfo, MandatoryValidation.YouHaveNotEntered);

			container.ACN_ACEP = "ACEP";
			container.ACN_ExpireDate = ZDate.Empty;
			AssertNoMessageErrorContaining(container.ACN_ExpireDateInfo, MandatoryValidation.YouHaveNotEntered);

			container.ACN_ACEP = ZString.Empty;
			container.ACN_ExpireDate = ZDate.Today;
			AssertNoMessageErrorContaining(container.ACN_ExpireDateInfo, MandatoryValidation.YouHaveNotEntered);

			container.ACN_ACEP = "ACEP";
			container.ACN_ExpireDate = ZDate.Today;
			AssertNoMessageErrorContaining(container.ACN_ExpireDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckACN_ACEP()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			container.ACN_ExpireDate = ZDate.Empty;
			container.ACN_ACEP = ZString.Empty;
			AssertNoMessageErrorContaining(container.ACN_ACEPInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			container.ACN_ExpireDate = ZDate.Empty;
			container.ACN_ACEP = ZString.Empty;
			AssertHasMessageErrorContaining(container.ACN_ACEPInfo, MandatoryValidation.YouHaveNotEntered);

			container.ACN_ExpireDate = ZDate.Empty;
			container.ACN_ACEP = "ACEP";
			AssertNoMessageErrorContaining(container.ACN_ACEPInfo, MandatoryValidation.YouHaveNotEntered);

			container.ACN_ExpireDate = ZDate.Today;
			container.ACN_ACEP = ZString.Empty;
			AssertNoMessageErrorContaining(container.ACN_ACEPInfo, MandatoryValidation.YouHaveNotEntered);

			container.ACN_ExpireDate = ZDate.Today;
			container.ACN_ACEP = "ACEP";
			AssertNoMessageErrorContaining(container.ACN_ACEPInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckACN_GoodsWeight()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
			container.ACN_GoodsWeight = 0;
			AssertNoNotifications(container.ACN_GoodsWeightInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			container.ACN_GoodsWeight = 0;
			AssertHasMessageError(container.ACN_GoodsWeightInfo, "If the container is not declared as empty, it cannot have a gross weight 0");

			container.ACN_GoodsWeight = 100;
			AssertNoNotifications(container.ACN_GoodsWeightInfo);

			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.EmptyContainer;
			container.ACN_GoodsWeight = 0;
			AssertNoNotifications(container.ACN_GoodsWeightInfo);

			container.ACN_GoodsWeight = 100;
			AssertNoNotifications(container.ACN_GoodsWeightInfo);
		}

		public void TestCheckACN_RC_ContainerType()
		{
			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "40GP";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);

			refcontainer.RC_ISOType = "40GP";
			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			container.ACN_RC_ContainerType = ZGuid.Empty;
			AssertHasMessageErrorContaining(container.ACN_RC_ContainerTypeInfo, MandatoryValidation.YouHaveNotEntered);
			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertNoNotifications(container.ACN_RC_ContainerTypeInfo);
			refcontainer.RC_ISOType = ZString.Empty;
			container.ACN_RC_ContainerType = refcontainer.PK;
			AssertHasMessageError(container.ACN_RC_ContainerTypeInfo, "The Container Type selected should have a ISO Type entered");
		}

		public void TestACN_Seal1()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			container.ACN_Seal1 = ZString.Empty;
			AssertNoNotifications(container.ACN_Seal1Info);
			container.ACN_Seal1 = "Seal1";
			AssertNoNotifications(container.ACN_Seal1Info);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			container.ACN_Seal1 = ZString.Empty;
			AssertHasMessageErrorContaining(container.ACN_Seal1Info, MandatoryValidation.YouHaveNotEntered);
			container.ACN_Seal1 = "Seal1";
			AssertNoNotifications(container.ACN_Seal1Info);
		}
	}
}
