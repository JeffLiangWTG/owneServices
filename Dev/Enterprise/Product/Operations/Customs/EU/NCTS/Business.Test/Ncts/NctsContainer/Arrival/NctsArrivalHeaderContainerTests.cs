using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalHeaderContainer))]
	sealed class NctsArrivalHeaderContainerTests : Customs.Business.Testing.BaseCusInBondContainerTest<NctsArrivalHeaderContainer>
	{
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

		public void TestTotalSealCountCaption()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertCaptions(container.TotalSealCountInfo, "Seal Quantity", "Seal Qty.", "Seal Qty.");
		}

		public void TestSeals()
		{
			var (_, container) = GetHeaderAndContainer();
			var seals = container.Seals;
			CombineAssertions(() =>
			{
				AssertType<CusSealCollection>("Type", seals);
				AssertEquals("IsRegisteredEditableChildObject", true, container.IsRegisteredEditableChildObject(seals));
				AssertSame("Cached", seals, container.Seals);
				AssertEquals("IsLoaded", true, seals.IsLoaded);
			});
		}

		public void TestCusSealType()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertType(((ICusSealTypeSupporter)container).CusSealType, container.Seals.AddNew());
		}

		public void TestEnableOrDisableSealsEdit() => CombineAssertions(() =>
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovementHeader = header.ArrivalMovementHeader;
			arrivalMovementHeader.BM_StateOfSealsBoolean = true;
			var container = header.ArrivalHeaderContainers.AddNew();
			Factory.Save();

			var readOnlyFactory = new ReadOnlyBusinessObjectFactory();
			var loadedContainerForReadOnly = readOnlyFactory.Load<NctsArrivalHeaderContainer>(container.PK);
			AssertEquals("BM_StateOfSeals != 'N' -> Seals readonly", true, loadedContainerForReadOnly.Seals.ReadOnly);

			arrivalMovementHeader.BM_StateOfSealsBoolean = false;
			Factory.Save();

			readOnlyFactory = new ReadOnlyBusinessObjectFactory();
			var loadedContainerForEditable = readOnlyFactory.Load<NctsArrivalHeaderContainer>(container.PK);
			AssertEquals("BM_StateOfSeals == 'N' -> Seals editable", false, loadedContainerForEditable.Seals.ReadOnly);
		});

		public void TestIsPhase5()
		{
			var (header, container) = GetHeaderAndContainer();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				container = header.ArrivalHeaderContainers.AddNew();
				Assert("NCTS5", container.IsPhase5);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("NCTS4", false, container.IsPhase5);
			});
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

		public void TestDelete()
		{
			var (_, container) = GetHeaderAndContainer();
			var seal = container.Seals.AddNew();
			container.Delete();
			AssertEquals("Seals are deleted when container is deleted", true, seal.IsDeleted);
		}

		public void TestValidation() => CombineAssertions(() =>
		{
			var (header, container) = GetHeaderAndContainer();
			AssertType<NctsArrivalHeaderContainerPhase5Validation>(container.Validation);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsArrivalHeaderContainerValidation>(container.Validation);
		});

		public void TestLookups()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertType<NctsArrivalHeaderContainerLookups>(container.Lookups);
		}

		public void TestTotalSealCount()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				var seal1 = container.Seals.AddNew();
				AssertEquals("Container has seal count 1", 1, container.TotalSealCount);
				container.Seals.AddNew();
				AssertEquals("Container has seal count 2", 2, container.TotalSealCount);

				seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("Container has seal count 1 excluding MIS", 1, container.TotalSealCount);
			});
		}

		public void TestTotalSealCountInfoRefreshBinding_SealsCountChanged()
		{
			var (_, container) = GetHeaderAndContainer();
			var totalSealCountUpdated = false;
			container.TotalSealCountInfo.ValueChanged += (_, __) =>
			{
				totalSealCountUpdated = true;
			};

			container.Seals.AddNew();

			AssertEquals("TotalSealCount is updated", true, totalSealCountUpdated);
		}

		public void TestTotalSealCountInfoRefreshBinding_SealsHasChangesChanged()
		{
			var (_, container) = GetHeaderAndContainer();
			var seal1 = container.Seals.AddNew();
			var totalSealCountUpdated = false;
			container.TotalSealCountInfo.ValueChanged += (_, __) =>
			{
				totalSealCountUpdated = true;
			};

			seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals("TotalSealCount is updated", true, totalSealCountUpdated);
		}

		public void TestContainerInfoReadOnly()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("ContainerNumber should not be ReadOnly when UnloadedState = NEW", false, container.BC_ContainerNumInfo.ReadOnly);
				AssertEquals("ContainerMode should not be ReadOnly when UnloadedState = NEW", false, container.BC_ModeInfo.ReadOnly);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("ContainerNumber should not be ReadOnly when UnloadedState = DIF", false, container.BC_ContainerNumInfo.ReadOnly);
				AssertEquals("ContainerMode should not be ReadOnly when UnloadedState = DIF", false, container.BC_ModeInfo.ReadOnly);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("ContainerNumber should be ReadOnly when UnloadedState = DEC", true, container.BC_ContainerNumInfo.ReadOnly);
				AssertEquals("ContainerMode should be ReadOnly when UnloadedState = DEC", true, container.BC_ModeInfo.ReadOnly);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("ContainerNumber should be ReadOnly when UnloadedState = MIS", true, container.BC_ContainerNumInfo.ReadOnly);
				AssertEquals("ContainerMode should be ReadOnly when UnloadedState = MIS", true, container.BC_ModeInfo.ReadOnly);
			});
		}

		public void TestFKToHeader()
		{
			var (header, container) = GetHeaderAndContainer();
			var iContainer = container as ISequenceNumberLine;
			AssertEquals("FK should point to NCTS Header", header.PK, iContainer.FKToHeader);
		}

		public void TestSequenceNumber()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.BC_SequenceNumber = 1;
				AssertEquals("Get SequenceNumber returns BC_SequenceNumber", (ZShort)1, container.SequenceNumber);

				container.SequenceNumber = 2;
				AssertEquals("Set SequenceNumber sets BC_SequenceNumber", (ZShort)2, container.BC_SequenceNumber);
			});
		}

		public void TestSealsLineNumberGenerator()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Seals", container.SealsLineNumberGenerator);
		}

		public void TestISequenceNumberHeader()
		{
			var (_, container) = GetHeaderAndContainer();
			var iContainer = container as ISequenceNumberHeader;
			AssertSame("SequenceNumberHeader.Lines", container.Seals as IEnumerable<ISequenceNumberLine>, iContainer.Lines);
		}

		public void TestCanDelete()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status is NEW", true, container.CanDelete);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status is DEC", false, container.CanDelete);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("Status is MIS", false, container.CanDelete);

				container.BC_UnloadedState = ZString.Empty;
				AssertEquals("Status is empty", false, container.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertEquals("Cannot delete containers which were entered by customs", container.ReasonForNotAbleToDelete);
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			var (_, container) = GetHeaderAndContainer();
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(container.NctsArrival.ArrivalMovementHeader, x => ((NctsArrivalHeaderContainer)x).AreUnloadingRemarksFullyAccepted, container);
		}

		public void TestBC_UnloadedState_ReadOnlyForAcceptedUnloading()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, container.BC_UnloadedStateInfo.ReadOnly);
			});
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(container.NctsArrival.ArrivalMovementHeader, x => ((NctsArrivalHeaderContainer)x).BC_UnloadedStateInfo.ReadOnly, container);
		}

		public void TestBC_ContainerNum_ReadOnlyForAcceptedUnloading()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, container.BC_ContainerNumInfo.ReadOnly);
			});
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(container.NctsArrival.ArrivalMovementHeader, x => ((NctsArrivalHeaderContainer)x).BC_ContainerNumInfo.ReadOnly, container);
		}

		public void TestBC_Mode_ReadOnlyForAcceptedUnloading()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, container.BC_ModeInfo.ReadOnly);
			});
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(container.NctsArrival.ArrivalMovementHeader, x => ((NctsArrivalHeaderContainer)x).BC_ModeInfo.ReadOnly, container);
		}

		public void TestBC_UnloadedState_ReadOnlyForSentToCustoms()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, container.BC_UnloadedStateInfo.ReadOnly);
				container.NctsArrival.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, container.BC_UnloadedStateInfo.ReadOnly);
			});
		}

		public void TestBC_ContainerNum_ReadOnlyForSentToCustoms()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, container.BC_ContainerNumInfo.ReadOnly);
				container.NctsArrival.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, container.BC_ContainerNumInfo.ReadOnly);
			});
		}

		public void TestBC_Mode_ReadOnlyForSentToCustoms()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, container.BC_ModeInfo.ReadOnly);
				container.NctsArrival.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, container.BC_ModeInfo.ReadOnly);
			});
		}

		public void TestBC_UnloadedState_ReadOnly()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions(() =>
			{
				container.BC_UnloadedState = ZString.Empty;
				AssertEquals("Unloaded state is empty", false, container.BC_UnloadedStateInfo.ReadOnly);
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Unloaded State is entered (NEW)", true, container.BC_UnloadedStateInfo.ReadOnly);
			});
		}

		public void TestSealsForMessaging()
		{
			var (_, container) = GetHeaderAndContainer();
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			var additionalSeal1 = container.Seals.AddNew();
			additionalSeal1.BK_SealNumber = "ADD1";
			additionalSeal1.BK_UnloadingState = "NEW";
			var additionalSeal2 = container.Seals.AddNew();
			additionalSeal2.BK_SealNumber = "ADD2";
			additionalSeal2.BK_UnloadingState = "DEC";
			var additionalSeal3 = container.Seals.AddNew();
			additionalSeal3.BK_SealNumber = "ADD3";
			additionalSeal3.BK_UnloadingState = "MIS";
			var additionalSeal4 = container.Seals.AddNew();
			additionalSeal4.BK_SealNumber = "ADD4";
			additionalSeal4.BK_UnloadingState = "DAM";

			AssertArrayEqualsByElements(new ZString[] { "ADD1", "ADD2", "ADD3" },
				container.SealsForMessaging.Select(x => (x.BK_SealNumber)).ToArray());
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header.ArrivalHeaderContainers.AddNew();
		}

		(NctsHeader, NctsArrivalHeaderContainer) GetHeaderAndContainer()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container = header.ArrivalHeaderContainers.AddNew();
			return (header, container);
		}
	}
}
