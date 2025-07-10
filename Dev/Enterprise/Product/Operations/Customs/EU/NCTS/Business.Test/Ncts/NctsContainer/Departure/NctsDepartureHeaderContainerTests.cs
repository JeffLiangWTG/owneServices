using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureHeaderContainer))]
	sealed class NctsDepartureHeaderContainerTests : Customs.Business.Testing.BaseCusInBondContainerTest<NctsDepartureHeaderContainer>
	{
		public void TestTypeDecider()
		{
			AssertType<NctsDepartureHeaderContainerTypeDecider>(NctsDepartureHeaderContainer.TypeDecider);
		}

		public void TestISynchableContainer()
		{
			var (_, container) = GetHeaderAndContainer();
			var provider = (ISynchableContainer)container;
			CombineAssertions(() =>
			{
				AssertEquals("ContainerNumInfo", "BC_ContainerNum", provider.ContainerNumInfo.Name);
				AssertEquals("Seal1Info", "Seal1", provider.Seal1Info.Name);
				AssertEquals("Seal2Supported", true, provider.Seal2Supported);
				AssertEquals("Seal2Info", "Seal2", provider.Seal2Info.Name);
				AssertEquals("Seal3Supported", false, provider.Seal3Supported);
				AssertNull("Seal3Info",  provider.Seal3Info);
				AssertEquals("!IsNonContainerized", false, provider.IsNonContainerized);
				container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
				AssertEquals("IsNonContainerized", true, provider.IsNonContainerized);
			});
		}

		public void TestHeader()
		{
			var (header, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				AssertEquals("BC_ParentID set", container.Header, header);
				AssertNull("BC_ParentID null", Factory.New<NctsDepartureHeaderContainer>().Header);
			});
		}

		public void TestHeaderUnknownTableCode()
		{
			AssertExceptionThrown<DeveloperNotificationException>("Unknown BC_ParentTableCode set", () =>
			{
				var nctsHeaderContainer = Factory.New<NctsDepartureHeaderContainer>();
				nctsHeaderContainer.BC_ParentTableCode = "XX";
				_ = nctsHeaderContainer.Header;
			});
		}

		public void TestBC_SequenceNumberCaption()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertCaptions(container.BC_SequenceNumberInfo, "Sequence Number", "Sequence No.", "Seq.No.");
		}

		public void TestBC_ModeCaption()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertCaptions(container.BC_ModeInfo, "Container/Equipment Mode", "Container Mode", "Mode");
		}

		public void TestBC_ContainerNumCaption()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertCaptions(container.BC_ContainerNumInfo, NctsHeader.Phase5CaptionKey, "Container/Equipment Number",
				"Container/Equipment No.", "Container/Equipment");
			NCTSTestHelper.AssertCaptions(container.BC_ContainerNumInfo, NctsHeader.Phase4CaptionKey, "Container Number",
				"", "Container");
		}

		public void TestBC_Seal1Caption()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertCaptions(container.BC_Seal1Info, "Container Seal 1", "Cont. Seal 1", "Seal 1");
		}

		public void TestBC_Seal2Caption()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertCaptions(container.BC_Seal2Info, "Container Seal 2", "Cont. Seal 2", "Seal 2");
		}

		public void TestTotalSealCountCaption()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertCaptions(container.TotalSealCountInfo, "Seal Quantity", "Seal Qty.", "Seal Qty.");
		}

		public void TestBC_UnloadedState_ReadOnly_NCTS5_Arrival_ART_FRC()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.FullReleaseOfGoodsMovementClosed;
			var container = header.DepartureHeaderContainers.AddNew();

			CombineAssertions(() =>
			{
				container.BC_UnloadedState = "NEW";
				AssertEquals("Status not MIS, DIF, DEC", false, container.BC_UnloadedStateInfo.ReadOnly);
				container.BC_UnloadedState = "MIS";
				AssertEquals("Status MIS", true, container.BC_UnloadedStateInfo.ReadOnly);
			});
		}

		public void TestBC_ContainerNum_ReadOnly_NCTS5_Arrival_ART_FRC()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var moveHeader = header.ArrivalMovementHeader;
			var container = header.DepartureHeaderContainers.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("not NCTS5_Arrival_ART_FRC", false, container.BC_ContainerNumInfo.ReadOnly);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.FullReleaseOfGoodsMovementClosed;
				AssertEquals("NCTS5_Arrival_ART_FRC", true, container.BC_ContainerNumInfo.ReadOnly);
			});
		}

		public void TestAdditionalSeals()
		{
			var (_, container) = GetHeaderAndContainer();
			var additionalSeals = container.AdditionalSeals;
			CombineAssertions(() =>
			{
				AssertType<CusSealCollection>("Type", additionalSeals);
				AssertEquals("IsRegisteredEditableChildObject", true, container.IsRegisteredEditableChildObject(additionalSeals));
				AssertSame("Cached", additionalSeals, container.AdditionalSeals);
				AssertEquals("IsLoaded", true, additionalSeals.IsLoaded);
			});
		}

		public void TestCusSealType()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertType(((ICusSealTypeSupporter)container).CusSealType, container.AdditionalSeals.AddNew());
		}

		public void TestIsContainerised()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.BC_Mode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("Containerised", true, container.IsContainerised);
				container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				AssertEquals("NonContainerised", false, container.IsContainerised);
			});
		}

		public void TestIsPhase5() => CombineAssertions(() =>
		{
			var (header, container) = GetHeaderAndContainer();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			container = header.DepartureHeaderContainers.AddNew();
			Assert("NCTS5", container.IsPhase5);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("NCTS4", false, container.IsPhase5);
		});

		public void TestDelete()
		{
			var (_, container) = GetHeaderAndContainer();
			var seal = container.AdditionalSeals.AddNew();
			container.Delete();
			AssertEquals("AdditionalSeals are deleted when container is deleted", true, seal.IsDeleted);
		}

		public void TestValidation() => CombineAssertions(() =>
		{
			var (header, container) = GetHeaderAndContainer();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsDepartureHeaderContainerPhase5Validation>(container.Validation);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsDepartureHeaderContainerPhase4Validation>(container.Validation);
		});

		public void TestLookups()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertType<NctsDepartureHeaderContainerLookups>(container.Lookups);
		}

		public void TestTotalSealCount()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.Seal1 = "aaa";
				AssertEquals("1st seal", 1, container.TotalSealCount);
				container.Seal2 = "aaa";
				AssertEquals("2nd seal", 2, container.TotalSealCount);
				container.AdditionalSeals.AddNew();
				AssertEquals("Additional seal", 3, container.TotalSealCount);
				container.AdditionalSeals.AddNew();
				AssertEquals("Additional seal", 4, container.TotalSealCount);
			});
		}

		public void TestTotalSealCountInfoRefreshBindingAdditionalSealsCountChanges()
		{
			var (_, container) = GetHeaderAndContainer();
			var totalSealCountUpdated = false;
			container.TotalSealCountInfo.ValueChanged += (_, __) =>
			{
				totalSealCountUpdated = true;
			};

			container.AdditionalSeals.AddNew();

			AssertEquals("TotalSealCount is updated", true, totalSealCountUpdated);
		}

		public void TestAllSeals()
		{
			var (_, container) = GetHeaderAndContainer();
			container.Seal1 = "123";
			container.Seal2 = "456";
			container.AdditionalSeals.AddNew().BK_SealNumber = "789";

			AssertContainsExactElementsInAnyOrder(new string[] { "123", "456", "789" }, container.AllSeals);
		}

		public void TestAdditionalSealsLineNumberGenerator()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Additional Seals", container.AdditionalSealsLineNumberGenerator);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header.DepartureHeaderContainers.AddNew();
		}

		(NctsHeader, NctsDepartureHeaderContainer) GetHeaderAndContainer()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container = header.DepartureHeaderContainers.AddNew();
			return (header, container);
		}
	}
}
