using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonTransportEquipmentWrapperTest : WrapperHelperTest<NCTS5CommonTransportEquipmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor for NctsDepartureHeaderContainer Throws Exception if container is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "departureHeaderContainer"), () => new NCTS5CommonTransportEquipmentWrapper((NctsDepartureHeaderContainer)null));

				AssertExceptionThrown("Constructor for NctsArrivalHeaderContainer Throws Exception if container is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "arrivalHeaderContainer"), () => new NCTS5CommonTransportEquipmentWrapper((NctsArrivalHeaderContainer)null));

				AssertExceptionThrown("Constructor for NctsContainer Throws Exception if container is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "container"), () => new NCTS5CommonTransportEquipmentWrapper(null, 1));
			});
		}

		public void TestSequenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled SequenceNumber wrapperDepartureHeader", "2", wrapperDepartureHeader.SequenceNumber);
				AssertEquals("Expected filled SequenceNumber wrapperArrivalHeader", "4", wrapperArrivalHeader.SequenceNumber);
				AssertEquals("Expected filled SequenceNumber wrapper", "3", wrapper.SequenceNumber);
			});
		}

		public void TestContainerIdentificationNumber()
		{
			CombineAssertions("containerDepartureHeader", () =>
			{
				containerDepartureHeader.BC_ContainerNum = "CONT1";
				containerDepartureHeader.BC_Mode = "NCT";
				AssertEquals("containerDepartureHeader expected empty ContainerIdentificationNumber when mode is NCT", ZString.Empty, wrapperDepartureHeader.ContainerIdentificationNumber);

				containerDepartureHeader.BC_Mode = "CNT";
				AssertEquals("containerDepartureHeader expected filled ContainerIdentificationNumber when mode is CNT", "CONT1", wrapperDepartureHeader.ContainerIdentificationNumber);

				containerDepartureHeader.Delete();
			});

			CombineAssertions("containerArrivalHeader", () =>
			{
				containerArrivalHeader.BC_ContainerNum = "CONT1";
				containerArrivalHeader.BC_UnloadedState = "NEW";
				containerArrivalHeader.BC_Mode = "NCT";
				AssertEquals("containerArrivalHeader expected empty ContainerIdentificationNumber when mode is NCT", ZString.Empty, wrapperArrivalHeader.ContainerIdentificationNumber);

				containerArrivalHeader.BC_Mode = "CNT";
				AssertEquals("containerArrivalHeader expected filled ContainerIdentificationNumber when mode is CNT and unloaded state is NEW", "CONT1", wrapperArrivalHeader.ContainerIdentificationNumber);
				containerArrivalHeader.BC_UnloadedState = "MIS";
				AssertEquals("containerArrivalHeader expected empty ContainerIdentificationNumber when mode is CNT but unloaded state is MIS", ZString.Empty, wrapperArrivalHeader.ContainerIdentificationNumber);
				containerArrivalHeader.BC_UnloadedState = "DIF";
				AssertEquals("containerArrivalHeader expected filled ContainerIdentificationNumber when mode is CNT and unloaded state is DIF", "CONT1", wrapperArrivalHeader.ContainerIdentificationNumber);
				containerArrivalHeader.BC_UnloadedState = "DEC";
				AssertEquals("containerArrivalHeader expected empty ContainerIdentificationNumber when mode is CNT but unloaded state is not NEW or DIF", ZString.Empty, wrapperArrivalHeader.ContainerIdentificationNumber);

				containerArrivalHeader.Delete();
			});

			CombineAssertions("container", () =>
			{
				container.BC_ContainerNum = "CONT1";
				container.BC_Mode = "NCT";
				AssertEquals("Container expected empty ContainerIdentificationNumber when mode is NCT", ZString.Empty, wrapper.ContainerIdentificationNumber);

				container.BC_Mode = "CNT";
				AssertEquals("Container expected filled ContainerIdentificationNumber when mode is CNT", "CONT1", wrapper.ContainerIdentificationNumber);
			});
		}

		public void TestNumberOfSeals()
		{
			CombineAssertions("containerDepartureHeader", () =>
			{
				AssertEquals("containerDepartureHeader expected 0 NumberOfSeals when no seals declared", "0", wrapperDepartureHeader.NumberOfSeals);

				containerDepartureHeader.Seal1 = "SEAL1";
				containerDepartureHeader.Seal2 = "SEAL2";
				containerDepartureHeader.AdditionalSeals.AddNew();
				containerDepartureHeader.AdditionalSeals.AddNew();
				AssertEquals("containerDepartureHeader expected 4 NumberOfSeals when declared", "4", wrapperDepartureHeader.NumberOfSeals);

				containerDepartureHeader.Delete();
			});

			CombineAssertions("containerArrivalHeader", () =>
			{
				containerArrivalHeader.BC_UnloadedState = "NEW";
				AssertEquals("containerArrivalHeader expected 0 NumberOfSeals when no seals declared", "0", wrapperArrivalHeader.NumberOfSeals);

				containerArrivalHeader.Seals.AddNew();
				containerArrivalHeader.Seals.AddNew();
				AssertEquals("containerArrivalHeader expected 2 NumberOfSeals when declared and unloaded state is NEW", "2", wrapperArrivalHeader.NumberOfSeals);
				containerArrivalHeader.BC_UnloadedState = "MIS";
				AssertEquals("containerArrivalHeader expected empty NumberOfSeals when declared but unloaded state is MIS", ZString.Empty, wrapperArrivalHeader.NumberOfSeals);
				containerArrivalHeader.BC_UnloadedState = "DIF";
				AssertEquals("containerArrivalHeader expected 2 NumberOfSeals when declared and unloaded state is DIF", "2", wrapperArrivalHeader.NumberOfSeals);
				containerArrivalHeader.BC_UnloadedState = "DEC";
				AssertEquals("containerArrivalHeader expected empty NumberOfSeals when declared but unloaded state is not NEW or DIF", ZString.Empty, wrapperArrivalHeader.NumberOfSeals);

				containerArrivalHeader.Delete();
			});

			CombineAssertions("container", () =>
			{
				AssertEquals("Container expected 0 NumberOfSeals when no seals declared", "0", wrapper.NumberOfSeals);

				container.BC_Seal1 = "SEAL1";
				container.BC_Seal2 = "SEAL2";
				container.Seals.AddNew();
				container.Seals.AddNew();
				AssertEquals("Container expected 4 NumberOfSeals when declared", "4", wrapper.NumberOfSeals);
			});
		}

		public void TestSeals()
		{
			CombineAssertions("containerDepartureHeader", () =>
			{
				AssertEquals("Expected empty Seals when no seals declared", 0, wrapperDepartureHeader.Seals.Count);

				containerDepartureHeader.Seal1 = "SEAL1";
				containerDepartureHeader.Seal2 = "SEAL2";
				var extraSeal1 = containerDepartureHeader.AdditionalSeals.AddNew();
				extraSeal1.BK_SealNumber = "SEAL4";
				var extraSeal2 = containerDepartureHeader.AdditionalSeals.AddNew();
				extraSeal2.BK_SealNumber = "SEAL3";

				AssertEquals("Prereq: extraSeal1.BK_SequenceNumber", (ZShort)3, extraSeal1.BK_SequenceNumber);
				AssertEquals("Prereq: extraSeal2.BK_SequenceNumber", (ZShort)4, extraSeal2.BK_SequenceNumber);

				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				var sealsHeader = wrapperDepartureHeader.Seals;
				AssertEquals("In container header, expected 4 Seals when declared", 4, sealsHeader.Count);
				AssertSame("In container header, cached Seals", wrapperDepartureHeader.Seals, sealsHeader);

				var sealsHeaderArray = sealsHeader.ToArray();

				AssertEquals("In container header, for first seal expected filled SealNumber", "SEAL1", sealsHeaderArray[0].SealNumber);
				AssertEquals("In container header, for first seal expected filled SequenceNumber", "1", sealsHeaderArray[0].SequenceNumber);

				AssertEquals("In container header, for second seal expected filled SealNumber", "SEAL2", sealsHeaderArray[1].SealNumber);
				AssertEquals("In container header, for second seal expected filled SequenceNumber", "2", sealsHeaderArray[1].SequenceNumber);

				AssertEquals("In container header, for third seal expected filled SealNumber (with SEAL4 since it's ordered by BK_SequenceNumber)", "SEAL4", sealsHeaderArray[2].SealNumber);
				AssertEquals("In container header, for third seal expected filled SequenceNumber", "3", sealsHeaderArray[2].SequenceNumber);

				AssertEquals("In container header, for fourth seal expected filled SealNumber (with SEAL3 since it's ordered by BK_SequenceNumber)", "SEAL3", sealsHeaderArray[3].SealNumber);
				AssertEquals("In container header, for fourth seal expected filled SequenceNumber", "4", sealsHeaderArray[3].SequenceNumber);

				containerDepartureHeader.Delete();
			});

			CombineAssertions("containerArrivalHeader", () =>
			{
				containerArrivalHeader.BC_UnloadedState = "NEW";
				AssertEquals("Expected empty Seals when no seals declared", 0, wrapperArrivalHeader.Seals.Count);

				var seal1 = containerArrivalHeader.Seals.AddNew();
				seal1.BK_SealNumber = "SEAL1";
				seal1.BK_UnloadingState = "NEW";
				var seal2 = containerArrivalHeader.Seals.AddNew();
				seal2.BK_SealNumber = "SEAL2";
				seal2.BK_UnloadingState = "MIS";
				var seal3 = containerArrivalHeader.Seals.AddNew();
				seal3.BK_SealNumber = "SEAL3";
				seal3.BK_UnloadingState = "DIF";
				var seal4 = containerArrivalHeader.Seals.AddNew();
				seal4.BK_SealNumber = "SEAL4";
				seal4.BK_UnloadingState = "DEC";

				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				var sealsHeader = wrapperArrivalHeader.Seals;
				AssertEquals("In container header, expected 3 Seals when declared (only those with unloading state MIS, DIF or NEW)", 3, sealsHeader.Count);
				AssertSame("In container header, cached Seals", wrapperArrivalHeader.Seals, sealsHeader);

				var sealsHeaderArray = sealsHeader.ToArray();

				AssertEquals("In container header, for first seal expected filled SealNumber", "SEAL1", sealsHeaderArray[0].SealNumber);
				AssertEquals("In container header, for first seal expected filled SequenceNumber", "1", sealsHeaderArray[0].SequenceNumber);

				AssertEquals("In container header, for second seal expected empty SealNumber because unloading state is MIS", ZString.Empty, sealsHeaderArray[1].SealNumber);
				AssertEquals("In container header, for second seal expected filled SequenceNumber", "2", sealsHeaderArray[1].SequenceNumber);

				AssertEquals("In container header, for third seal expected filled SealNumber", "SEAL3", sealsHeaderArray[2].SealNumber);
				AssertEquals("In container header, for third seal expected filled SequenceNumber", "3", sealsHeaderArray[2].SequenceNumber);

				containerArrivalHeader.BC_UnloadedState = "MIS";
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				AssertEquals("Expected empty Seals when seals declared but container's unloaded state is MIS", 0, wrapperArrivalHeader.Seals.Count);

				containerArrivalHeader.BC_UnloadedState = "DEC";
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				AssertEquals("In container header, expected 3 Seals when declared (only those with unloading state MIS, DIF or NEW) when container's unloaded state is not MIS", 3, wrapperArrivalHeader.Seals.Count);

				containerArrivalHeader.Delete();
			});

			CombineAssertions("container", () =>
			{
				AssertEquals("In container header, expected empty Seals when no seals declared", 0, wrapper.Seals.Count);

				container.BC_Seal1 = "SEAL1";
				container.BC_Seal2 = "SEAL2";
				var extraSeal1 = container.Seals.AddNew();
				extraSeal1.BK_SealNumber = "SEAL4";
				var extraSeal2 = container.Seals.AddNew();
				extraSeal2.BK_SealNumber = "SEAL3";

				AssertEquals("Prereq: extraSeal1.BK_SequenceNumber", (ZShort)1, extraSeal1.BK_SequenceNumber);
				AssertEquals("Prereq: extraSeal2.BK_SequenceNumber", (ZShort)2, extraSeal2.BK_SequenceNumber);

				wrapper = GetWrapper(container, 2);
				var seals = wrapper.Seals;
				AssertEquals("In container, expected 4 Seals when declared", 4, seals.Count);
				AssertSame("In container, cached Seals", wrapper.Seals, seals);

				var sealsArray = seals.ToArray();

				AssertEquals("In container, for first seal expected filled SealNumber", "SEAL1", sealsArray[0].SealNumber);
				AssertEquals("In container, for first seal expected filled SequenceNumber", "1", sealsArray[0].SequenceNumber);

				AssertEquals("In container, for second seal expected filled SealNumber", "SEAL2", sealsArray[1].SealNumber);
				AssertEquals("In container, for second seal expected filled SequenceNumber", "2", sealsArray[1].SequenceNumber);

				AssertEquals("In container, for third seal expected filled SealNumber (with SEAL3 since it's ordered by BK_SealNumber)", "SEAL3", sealsArray[2].SealNumber);
				AssertEquals("In container, for third seal expected filled SequenceNumber", "3", sealsArray[2].SequenceNumber);

				AssertEquals("In container, for fourth seal expected filled SealNumber (with SEAL4 since it's ordered by BK_SealNumber)", "SEAL4", sealsArray[3].SealNumber);
				AssertEquals("In container, for fourth seal expected filled SequenceNumber", "4", sealsArray[3].SequenceNumber);
			});
		}

		public void TestGoodsReference()
		{
			var bill = nctsHeaderDeparture.Bills.AddNew();

			containerDepartureHeader.BC_Mode = Core.Constants.ContainerModes.Containerised;
			containerDepartureHeader.BC_ContainerNum = "ETDU1111";

			var containerDepartureHeader2 = nctsHeaderDeparture.DepartureHeaderContainers.AddNew();
			containerDepartureHeader2.BC_Mode = Core.Constants.ContainerModes.Containerised;
			containerDepartureHeader2.BC_ContainerNum = "ETDU2222";

			var containerDepartureHeader3 = nctsHeaderDeparture.DepartureHeaderContainers.AddNew();
			containerDepartureHeader3.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			containerDepartureHeader3.BC_ContainerNum = "ETDU3333";

			var goodItem1 = bill.GoodsItems.AddNew();
			goodItem1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = goodItem1.Packages.AddNew();
			package1.ContainersPivot.AddPivotFor(containerDepartureHeader);
			package1.ContainersPivot.AddPivotFor(containerDepartureHeader2);
			package1.ContainersPivot.AddPivotFor(containerDepartureHeader3);
			package1.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
			package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
			package1.ContainersPivotsForBindingOnly[2].ContainerSelected = false;

			CombineAssertions(() =>
			{
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				AssertEquals("Expected empty GoodsReference when no containers1 selected in goodsItems", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected empty GoodsReference when no containers2 selected in goodsItems", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				AssertEquals("Expected empty GoodsReference when no containers3 selected in goodsItems", 0, wrapperDepartureHeader.GoodsReference.Count);

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				var goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 is the only one selected in all goodsItems(one)(BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				AssertSame("Cached GoodsReference", wrapperDepartureHeader.GoodsReference, goodsReference);
				var goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 is the only one selected in all goodsItems(one) (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 is the only one selected in all goodsItems(one) (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(one)(BC_Mode = CNT)(Container 2)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(one)(BC_Mode = CNT)(Container 3)", 0, wrapperDepartureHeader.GoodsReference.Count);

				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 2)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperDepartureHeader.GoodsReference.Count);

				var goodItem2 = bill.GoodsItems.AddNew();
				goodItem2.BY_DeclarationGoodsItemNumber = 2;
				var package2 = goodItem2.Packages.AddNew();
				package2.ContainersPivot.AddPivotFor(containerDepartureHeader);
				package2.ContainersPivot.AddPivotFor(containerDepartureHeader2);
				package2.ContainersPivot.AddPivotFor(containerDepartureHeader3);
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = false;

				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 2 GoodsReference when the container1 is the only one selected in all goodsItems(two)(BC_Mode = CNT)(Container 1)", 2, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(two)(BC_Mode = CNT)(Container 2)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(two)(BC_Mode = CNT)(Container 3)", 0, wrapperDepartureHeader.GoodsReference.Count);

				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 2)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperDepartureHeader.GoodsReference.Count);

				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 2)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperDepartureHeader.GoodsReference.Count);

				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 2 GoodsReference when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", 2, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 2 GoodsReference when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", 2, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "2", goodsReferenceArray[1].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperDepartureHeader.GoodsReference.Count);

				var goodItem3 = bill.GoodsItems.AddNew();
				goodItem3.BY_DeclarationGoodsItemNumber = 3;
				var package3 = goodItem3.Packages.AddNew();
				package3.ContainersPivot.AddPivotFor(containerDepartureHeader);
				package3.ContainersPivot.AddPivotFor(containerDepartureHeader2);
				package3.ContainersPivot.AddPivotFor(containerDepartureHeader3);
				package3.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package3.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package3.ContainersPivotsForBindingOnly[2].ContainerSelected = true;

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = false;

				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = true;

				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 1)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 2)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "3", goodsReferenceArray[2].SequenceNumber);

				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 1)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 2)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 3)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 3)", "3", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 3)", "1", goodsReferenceArray[0].SequenceNumber);

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when all containers is selected in all goodsItems (Container 1)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 1)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 1)", "3", goodsReferenceArray[2].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when all containers is selected in all goodsItems (Container 2)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 2)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 2)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 2)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 2)", "3", goodsReferenceArray[2].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when all containers is selected in all goodsItems (Container 3)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 3)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 3)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 3)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 3)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 3)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 3)", "3", goodsReferenceArray[2].SequenceNumber);

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package3.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "3", goodsReferenceArray[2].SequenceNumber);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader2);
				AssertEquals("Expected 0 GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 2)", 0, wrapperDepartureHeader.GoodsReference.Count);
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader3);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 3)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 3)", "2", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 3)", "1", goodsReferenceArray[0].SequenceNumber);
			});
		}

		public void TestGoodsReference_DepartureOnlyOneContainer()
		{
			var bill = nctsHeaderDeparture.Bills.AddNew();

			containerDepartureHeader.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			containerDepartureHeader.BC_ContainerNum = "ETDU1111";

			var goodItem1 = bill.GoodsItems.AddNew();
			goodItem1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = goodItem1.Packages.AddNew();
			package1.ContainersPivot.AddPivotFor(containerDepartureHeader);
			package1.ContainersPivotsForBindingOnly[0].ContainerSelected = false;

			CombineAssertions(() =>
			{
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				AssertEquals("Expected empty GoodsReference when no container selected in goodsItems", 0, wrapperDepartureHeader.GoodsReference.Count);

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				var goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when only container selected in goodsItems and it's NCT", 1, wrapperDepartureHeader.GoodsReference.Count);
				AssertSame("Cached GoodsReference", wrapperDepartureHeader.GoodsReference, goodsReference);
				var goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when it's NCT", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when it's NCT", "1", goodsReferenceArray[0].SequenceNumber);

				containerDepartureHeader.BC_Mode = Core.Constants.ContainerModes.Containerised;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				AssertEquals("Expected empty GoodsReference when only container selected in goodsItems but it's CNT", 0, wrapperDepartureHeader.GoodsReference.Count);

				var goodItem2 = bill.GoodsItems.AddNew();
				goodItem2.BY_DeclarationGoodsItemNumber = 2;
				var package2 = goodItem2.Packages.AddNew();
				package2.ContainersPivot.AddPivotFor(containerDepartureHeader);
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				var package3 = goodItem2.Packages.AddNew();
				package3.ContainersPivot.AddPivotFor(containerDepartureHeader);
				package3.ContainersPivotsForBindingOnly[0].ContainerSelected = false;

				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when only container is CNT and is selected in only one goodsItem", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when selected in only one goodsItem", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when selected in only one goodsItem", "1", goodsReferenceArray[0].SequenceNumber);

				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected empty GoodsReference when only container is CNT and is selected in at least one package in all goodsItems", 0, wrapperDepartureHeader.GoodsReference.Count);

				containerDepartureHeader.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);
				goodsReference = wrapperDepartureHeader.GoodsReference;
				AssertEquals("Expected 2 GoodsReference when only container is NCT and is selected in at least one package in all goodsItems", 2, wrapperDepartureHeader.GoodsReference.Count);
				AssertSame("Cached GoodsReference", wrapperDepartureHeader.GoodsReference, goodsReference);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when selected in all goodsItems and it's NCT", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when selected in all goodsItems and it's NCT", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For second goodsReference expected filled DeclarationGoodsItemNumber when selected in all goodsItems and it's NCT", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For second goodsReference expected filled SequenceNumber when selected in all goodsItems and it's NCT", "2", goodsReferenceArray[1].SequenceNumber);
			});
		}

		public void TestGoodsReference_Arrival()
		{
			var bill = nctsHeaderArrival.Bills.AddNew();

			containerArrivalHeader.BC_Mode = Core.Constants.ContainerModes.Containerised;
			containerArrivalHeader.BC_ContainerNum = "ETDU1111";
			containerArrivalHeader.BC_UnloadedState = "NEW";

			var containerArrivalHeader2 = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			containerArrivalHeader2.BC_Mode = Core.Constants.ContainerModes.Containerised;
			containerArrivalHeader2.BC_ContainerNum = "ETDU2222";
			containerArrivalHeader2.BC_UnloadedState = "NEW";

			var containerArrivalHeader3 = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			containerArrivalHeader3.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			containerArrivalHeader3.BC_ContainerNum = "ETDU3333";
			containerArrivalHeader3.BC_UnloadedState = "NEW";

			var goodItem1 = bill.ArrivalGoodsItems.AddNew();
			goodItem1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = goodItem1.Packages.AddNew();
			package1.ContainersPivot.AddPivotFor(containerArrivalHeader);
			package1.ContainersPivot.AddPivotFor(containerArrivalHeader2);
			package1.ContainersPivot.AddPivotFor(containerArrivalHeader3);
			package1.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
			package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
			package1.ContainersPivotsForBindingOnly[2].ContainerSelected = false;

			CombineAssertions(() =>
			{
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				AssertEquals("Expected empty GoodsReference when no containers1 selected in goodsItems", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected empty GoodsReference when no containers2 selected in goodsItems", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected empty GoodsReference when no containers3 selected in goodsItems", 0, wrapperArrivalHeader.GoodsReference.Count);

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				var goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 is the only one selected in all goodsItems(one)(BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				AssertSame("Cached GoodsReference", wrapperArrivalHeader.GoodsReference, goodsReference);
				var goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when container1 is selected in all goodsItems(one)(BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when container1 is selected in all goodsItems(one)(BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(one)(BC_Mode = CNT)(Container 2)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(one)(BC_Mode = CNT)(Container 3)", 0, wrapperArrivalHeader.GoodsReference.Count);

				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 2)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 and container2 are selected in a unique goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperArrivalHeader.GoodsReference.Count);

				var goodItem2 = bill.ArrivalGoodsItems.AddNew();
				goodItem2.BY_DeclarationGoodsItemNumber = 2;
				var package2 = goodItem2.Packages.AddNew();
				package2.ContainersPivot.AddPivotFor(containerArrivalHeader);
				package2.ContainersPivot.AddPivotFor(containerArrivalHeader2);
				package2.ContainersPivot.AddPivotFor(containerArrivalHeader3);
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = false;

				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 2 GoodsReference when container1 is the only one selected in all goodsItems(two)(BC_Mode = CNT)(Container 1)", 2, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when container1 is the only one selected in all goodsItems(two)(Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(two)(BC_Mode = CNT)(Container 2)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in all goodsItems(two)(BC_Mode = CNT)(Container 3)", 0, wrapperArrivalHeader.GoodsReference.Count);

				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 2)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 is the only one selected in only one goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperArrivalHeader.GoodsReference.Count);

				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 1)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 2)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 and container2 are selected in one goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperArrivalHeader.GoodsReference.Count);

				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 2 GoodsReference when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", 2, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 2 GoodsReference when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", 2, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 2)", "2", goodsReferenceArray[1].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected 0 GoodsReference when the container1 and container2 are selected in all goodsItems (BC_Mode = CNT)(Container 3)", 0, wrapperArrivalHeader.GoodsReference.Count);

				var goodItem3 = bill.ArrivalGoodsItems.AddNew();
				goodItem3.BY_DeclarationGoodsItemNumber = 3;
				var package3 = goodItem3.Packages.AddNew();
				package3.ContainersPivot.AddPivotFor(containerArrivalHeader);
				package3.ContainersPivot.AddPivotFor(containerArrivalHeader2);
				package3.ContainersPivot.AddPivotFor(containerArrivalHeader3);
				package3.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package3.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package3.ContainersPivotsForBindingOnly[2].ContainerSelected = true;

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = false;

				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = true;

				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 1)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 2)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when container3 is selected in all goodsItems (BC_Mode = NCT)(Container 3)", "3", goodsReferenceArray[2].SequenceNumber);

				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 1)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected 0 GoodsReference when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 2)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 3)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 3)", "3", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when container3 is selected in one goodsItems (BC_Mode = NCT)(Container 3)", "1", goodsReferenceArray[0].SequenceNumber);

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[1].ContainerSelected = true;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when all containers is selected in all goodsItems (Container 1)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 1)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 1)", "3", goodsReferenceArray[2].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when all containers is selected in all goodsItems (Container 2)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 2)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 2)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 2)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 2)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 2)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 2)", "3", goodsReferenceArray[2].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when all containers is selected in all goodsItems (Container 3)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 3)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 3)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 3)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 3)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when all containers is selected in all goodsItems (Container 3)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when all containers is selected in all goodsItems (Container 3)", "3", goodsReferenceArray[2].SequenceNumber);

				package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package1.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package1.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package2.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package2.ContainersPivotsForBindingOnly[2].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				package3.ContainersPivotsForBindingOnly[1].ContainerSelected = false;
				package3.ContainersPivotsForBindingOnly[2].ContainerSelected = false;
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 3 GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", 3, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "1", goodsReferenceArray[0].SequenceNumber);
				AssertEquals("For Second goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "2", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For Second goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "2", goodsReferenceArray[1].SequenceNumber);
				AssertEquals("For third goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "3", goodsReferenceArray[2].DeclarationGoodsItemNumber);
				AssertEquals("For third goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 1)", "3", goodsReferenceArray[2].SequenceNumber);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader2);
				AssertEquals("Expected 0 GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 2)", 0, wrapperArrivalHeader.GoodsReference.Count);
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				goodsReference = wrapperArrivalHeader.GoodsReference;
				AssertEquals("Expected 1 GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 3)", 1, goodsReference.Count);
				goodsReferenceArray = goodsReference.ToArray();
				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 3)", "2", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber when one containers is selected in all goodsItems and other with NCT is selected (Container 3)", "1", goodsReferenceArray[0].SequenceNumber);

				containerArrivalHeader.BC_UnloadedState = "DIF";
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);
				AssertEquals("Expected empty GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 1) but the container's unloaded state is nor NEW", 0, wrapperArrivalHeader.GoodsReference.Count);
				containerArrivalHeader3.BC_UnloadedState = "MIS";
				wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader3);
				AssertEquals("Expected empty GoodsReference when one containers is selected in all goodsItems and other with NCT is selected (Container 3) but the container's unloaded state is nor NEW", 0, wrapperArrivalHeader.GoodsReference.Count);
			});
		}

		public void TestDamagedSealsNotRetained()
		{
			containerArrivalHeader.BC_Seal1 = "seal1";
			containerArrivalHeader.BC_Seal2 = "seal2";
			var additionalSeal1 = containerArrivalHeader.Seals.AddNew();
			additionalSeal1.BK_SealNumber = "ADD1";
			additionalSeal1.BK_UnloadingState = "NEW";
			var additionalSeal2 = containerArrivalHeader.Seals.AddNew();
			additionalSeal2.BK_SealNumber = "ADD2";
			additionalSeal2.BK_UnloadingState = "DEC";
			var additionalSeal3 = containerArrivalHeader.Seals.AddNew();
			additionalSeal3.BK_SealNumber = "ADD3";
			additionalSeal3.BK_UnloadingState = "MIS";
			var additionalSeal4 = containerArrivalHeader.Seals.AddNew();
			additionalSeal4.BK_SealNumber = "ADD4";
			additionalSeal4.BK_UnloadingState = "DAM";

			AssertArrayEqualsByElements(new[] { "ADD1", "" },
				wrapperArrivalHeader.Seals.Select(x => x.SealNumber.ToString()).ToArray());
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);

			containerDepartureHeader = nctsHeaderDeparture.DepartureHeaderContainers.AddNew();
			containerDepartureHeader.BC_SequenceNumber = 2;
			wrapperDepartureHeader = GetWrapperDepartureHeader(containerDepartureHeader);

			nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);

			containerArrivalHeader = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			containerArrivalHeader.BC_SequenceNumber = 4;
			wrapperArrivalHeader = GetWrapperArrivalHeader(containerArrivalHeader);

			container = Factory.New<NctsContainer>();
			wrapper = GetWrapper(container, 3);
		}

		NctsHeader nctsHeaderDeparture;
		NctsDepartureHeaderContainer containerDepartureHeader;
		NCTS5CommonTransportEquipmentWrapper wrapperDepartureHeader;
		NctsHeader nctsHeaderArrival;
		NctsArrivalHeaderContainer containerArrivalHeader;
		NCTS5CommonTransportEquipmentWrapper wrapperArrivalHeader;
		NctsContainer container;
		ArrivalNCTS5TransportEquipmentWrapper wrapper;

		NCTS5CommonTransportEquipmentWrapper GetWrapperDepartureHeader(NctsDepartureHeaderContainer containerDepartureHeader) => new NCTS5CommonTransportEquipmentWrapper(containerDepartureHeader);

		NCTS5CommonTransportEquipmentWrapper GetWrapperArrivalHeader(NctsArrivalHeaderContainer containerArrivalHeader) => new NCTS5CommonTransportEquipmentWrapper(containerArrivalHeader);

		ArrivalNCTS5TransportEquipmentWrapper GetWrapper(NctsContainer container, ZShort seqNum) => new ArrivalNCTS5TransportEquipmentWrapper(container, seqNum);

		protected override NCTS5CommonTransportEquipmentWrapper GetProvider() => wrapperDepartureHeader;
	}
}
