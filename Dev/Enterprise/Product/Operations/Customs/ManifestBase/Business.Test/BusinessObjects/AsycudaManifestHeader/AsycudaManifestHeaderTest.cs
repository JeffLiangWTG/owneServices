using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestsSubclassesOf(typeof(AsycudaManifestHeader))]
	public abstract class AsycudaManifestHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeOfContainers()
		{
			AssertEquals("Containers' type should be expected.", ExpectedTypeOfContainer, ((AsycudaManifestHeader)GetNewBusinessObject()).Containers.GetType());
		}

		public void TestGetEUMemberStateCommunicationType() => AssertEquals(ExpectedEUMemberStateCommunicationType, ((AsycudaManifestHeader)GetNewBusinessObject()).GetEUMemberStateCommunicationType());

		public void TestEnsurePackedItemRelationshipSettingIsCorrect()
		{
			var bizObj = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill = bizObj.Bills.AddNew();
			AssertEquals("Header.PackedItemRelationship should match Bill.PackedItemRelationship", bill.PackedItemRelationship, bizObj.PackedItemRelationship);
		}

		protected abstract Type ExpectedTypeOfContainer { get; }

		protected virtual Type ExpectedEUMemberStateCommunicationType => typeof(EUMemberStateCommunication);
	}

	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderBaseOnlyTest : AsycudaManifestHeaderTest
	{
		public void TestVessel()
		{
			var vessel = CreateVessel();
			Factory.Save();

			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				AssertNull("Default", header.Vessel);
				header.AMA_VesselName = "VESSEL";
				AssertEquals("AMA_VesselName", vessel.PK, header.Vessel.PK);
				header.AMA_LloydsNumber = "9832343";
				AssertEquals("AMA_VesselName, AMA_LloydsNumber", vessel.PK, header.Vessel.PK);
				header.AMA_RadioCallSign = "CALLME";
				AssertEquals("AMA_VesselName, AMA_LloydsNumber, AMA_RadioCallSign", vessel.PK, header.Vessel.PK);
				header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Australia;
				AssertEquals("AMA_VesselName, AMA_LloydsNumber, AMA_RadioCallSign, AMA_RN_NKConveyanceNationality", vessel.PK, header.Vessel.PK);
			});
		}

		public void TestPopulateVesselFields_AMA_VesselNameChanged()
		{
			CreateVessel();
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_VesselName = "VESSEL";
			CombineAssertions(() =>
			{
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});

			header.AMA_LloydsNumber = ZString.Empty;
			header.AMA_VesselName = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_LloydsNumber", ZString.Empty, header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});
		}

		public void TestPopulateVesselFields_AMA_LloydsNumberChanged()
		{
			CreateVessel();
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_LloydsNumber = "9832343";
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});

			header.AMA_VesselName = ZString.Empty;
			header.AMA_LloydsNumber = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", ZString.Empty, header.AMA_VesselName);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});
		}

		public void TestPopulateVesselFields_AMA_RadioCallSignChanged()
		{
			CreateVessel();
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RadioCallSign = "CALLME";
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});

			header.AMA_VesselName = ZString.Empty;
			header.AMA_RadioCallSign = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", ZString.Empty, header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});
		}

		public void TestPopulateVesselFields_AMA_RN_NKConveyanceNationalityChanged_OnlyItHasValue()
		{
			CreateVessel();
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Australia;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", ZString.Empty, header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", ZString.Empty, header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", ZString.Empty, header.AMA_RadioCallSign);
			});
		}

		public void TestPopulateVesselFields_AMA_RN_NKConveyanceNationalityChanged()
		{
			CreateVessel();
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_VesselName = "VESSEL";
			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Australia;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
			});

			header.AMA_VesselName = ZString.Empty;
			header.AMA_RN_NKConveyanceNationality = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", ZString.Empty, header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
			});
		}

		public void TestDefaultVesselValues()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.DefaultVesselValues(CreateVessel());
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});
		}

		public void TestDefaultVesselValues_Null()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.DefaultVesselValues(CreateVessel());
			header.DefaultVesselValues(null);
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});
		}

		RefVessel CreateVessel()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_LloydsNumber = "9832343";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Australia;
			return vessel;
		}

		public void TestPackedItemRelationship()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("IsManyPackedItemRelationship", false, header.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", true, header.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", false, header.IsOnePackedItemRelationship);
			header.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			AssertEquals("IsManyPackedItemRelationship", false, header.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", false, header.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", true, header.IsOnePackedItemRelationship);
			header.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			AssertEquals("IsManyPackedItemRelationship", true, header.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", false, header.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", false, header.IsOnePackedItemRelationship);
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(GlbBranch.CurrentBranch.PK, header.AMA_GB);
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			Factory.Save();

			bill.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Bill should not be deleted after the Header", false);
				}
			};
			container.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Container should not be deleted after the Header", false);
				}
			};

			header.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaBill)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaContainer)));
		}

		public void TestArrivalHeaderType()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(typeof(AsycudaArrivalHeader), header.GetArrivalHeaderType());
		}

		public void TestBills()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_ApplicationCode = "BAS";
			var bills = header.Bills;
			AssertEquals(0, bills.Count);
			_ = bills.AddNew();
			AssertEquals(1, bills.Count);
			Factory.Save();

			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			var billsReloaded = headerReloaded.Bills;
			AssertEquals(1, billsReloaded.Count);
			AssertEquals(typeof(AsycudaBill), billsReloaded[0].GetType());
			AssertEquals(true, headerReloaded.IsRegisteredEditableChildObject(billsReloaded));
		}

		public void TestClusterKey()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "HKD3232432";
			var bill1 = header1.Bills.AddNew();
			var container1 = header1.Containers.AddNew();
			var pack1 = bill1.Packs.AddNew();
			var packedItem1 = pack1.PackedItem;
			pack1.ContainerPK = container1.PK;
			var pivot1 = pack1.Pivot;
			Factory.Save();
			AssertEquals("Cluster key is not 0", 1, header1.AMA_ClusterKey);
			AssertEquals(bill1.ABL_ClusterKey, header1.AMA_ClusterKey);
			AssertEquals(container1.ACN_ClusterKey, header1.AMA_ClusterKey);
			AssertEquals(pack1.APA_ClusterKey, header1.AMA_ClusterKey);
			AssertEquals(packedItem1.API_ClusterKey, header1.AMA_ClusterKey);
			AssertEquals(pivot1.APC_ClusterKey, header1.AMA_ClusterKey);

			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "HKD3232433";
			var bill2 = header2.Bills.AddNew();
			var container2 = header2.Containers.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			pack2.ContainerPK = container2.PK;
			var pivot2 = pack2.Pivot;
			Factory.Save();

			AssertEquals("Cluster key is not 0", 2, header2.AMA_ClusterKey);
			AssertEquals(bill2.ABL_ClusterKey, header2.AMA_ClusterKey);
			AssertEquals(container2.ACN_ClusterKey, header2.AMA_ClusterKey);
			AssertEquals(pack2.APA_ClusterKey, header2.AMA_ClusterKey);
			AssertEquals(packedItem2.API_ClusterKey, header2.AMA_ClusterKey);
			AssertEquals(pivot2.APC_ClusterKey, header2.AMA_ClusterKey);
		}

		public void TestFetchStrategyType()
		{
			AssertType<AsycudaManifestHeaderFetchStrategy>(Factory.New<AsycudaManifestHeader>().FetchStrategy);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			header.AMA_JobReference = "X";
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
