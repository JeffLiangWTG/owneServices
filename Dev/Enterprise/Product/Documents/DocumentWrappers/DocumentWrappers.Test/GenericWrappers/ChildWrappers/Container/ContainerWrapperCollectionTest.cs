using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperCollection))]
	public class ContainerWrapperCollectionTest : GenericWrapperCollectionTest<ContainerWrapperCollection>
	{
		public void TestLoadFromCommonContainer()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "OOCL0000006";
			ContainerWrapperCollection wrappers = new ContainerWrapperCollection(container, Factory);
			AssertEquals(1, wrappers.Count);
			AssertEquals("OOCL0000006", wrappers[0].ContainerNo);
		}

		public void TestContainerSummary()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			ContainerWrapperCollection collection = new ContainerWrapperCollection(consol, Factory);
			AssertEquals("", collection.ContainerSummary);

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			CommonContainer container4 = consol.Containers.AddNew();
			container4.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40NOR").PK;
			container4.JC_ContainerCount = 5;

			collection = new ContainerWrapperCollection(consol, Factory);
			AssertEquals("20GP x 2, 40GP x 1, 40NOR x 5", collection.ContainerSummary);
			AssertEquals(8, collection.ContainerCount);
		}

		public void TestLoadFromConsolWithDeliveryAgent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_IsForwardRegistered = true;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_IsForwardRegistered = true;

			DeliveryAgentOrgHeader deliveryAgent1 = Factory.New<DeliveryAgentOrgHeader>();
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			DeliveryAgentOrgHeader deliveryAgent2 = Factory.New<DeliveryAgentOrgHeader>();
			shipment2.JS_OH_DeliveryAgent = deliveryAgent2.PK;

			ForwardingContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			ForwardingContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "OOCL0000011";
			ForwardingContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";

			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			PackLine packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			PackLine packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.JL_JC = container3.PK;

			ContainerWrapperCollection collection = new ContainerWrapperCollection(consol, deliveryAgent1, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("collection[0].WrappedObject", container1, collection[0].WrappedObject);
			AssertEquals("collection[1].WrappedObject", container2, collection[1].WrappedObject);
			collection = new ContainerWrapperCollection(consol, deliveryAgent2, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals("collection[0].WrappedObject", container3, collection[0].WrappedObject);
		}

		public void TestLoadFromDetentionAdviceHeader()
		{
			DetentionAdviceHeader header = new DetentionAdviceHeader(Factory);

			BillOfLadingContainer container1 = header.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			BillOfLadingContainer container2 = header.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100048";

			RefContainer containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = containerType.PK;
			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100027";
			stock2.R6_RC = containerType.PK;

			ContainerMovement movement1 = header.Movements.AddNew();
			movement1.E9_R6 = stock1.PK;
			ContainerMovement movement2 = header.Movements.AddNew();
			movement2.E9_R6 = stock2.PK;

			ContainerWrapperCollection collection = new ContainerWrapperCollection(header, Factory);
			AssertEquals("collection.Count", 4, collection.Count);
			AssertEquals("collection[0].ContainerNo", "FAKE4100011", collection[0].ContainerNo);
			AssertEquals("collection[1].ContainerNo", "FAKE4100048", collection[1].ContainerNo);
			AssertEquals("collection[2].ContainerNo", "TEST4100013", collection[2].ContainerNo);
			AssertEquals("collection[3].ContainerNo", "TEST4100027", collection[3].ContainerNo);
		}

		public void TestLoadFromReleaseInstance()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			ReleaseHeader header = new ReleaseHeader(shipment, true);
			header.Init();
			var instance = header.Instances.AddNew();

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "OOCL0000011";
			AgencyShipmentContainer container3 = shipment.RealContainers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";

			ReleaseDetail release1 = new ReleaseDetail(container1);
			release1.ReleaseCount = 1;
			instance.Header.Details.Add(release1);
			ReleaseDetail release2 = new ReleaseDetail(container2);
			release2.ReleaseCount = 1;
			instance.Header.Details.Add(release2);
			ReleaseDetail release3 = new ReleaseDetail(container3);
			release3.ReleaseCount = 1;
			instance.Header.Details.Add(release3);

			ContainerWrapperCollection collection = new ContainerWrapperCollection(instance, Factory);

			AssertEquals("collection.Count", 3, collection.Count);
			AssertEquals("collection[0].WrappedObject", container1, collection[0].WrappedObject);
			AssertEquals("collection[1].WrappedObject", container2, collection[1].WrappedObject);
			AssertEquals("collection[2].WrappedObject", container3, collection[2].WrappedObject);
		}

		public void TestLoadFromForwardingShipment()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = "AUBNE";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "USLAX";

			ForwardingContainer container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			ForwardingContainer container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "OOCL0000011";
			ForwardingContainer container3 = consol1.Containers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";

			ForwardingContainer container4 = consol2.Containers.AddNew();
			container4.JC_ContainerNum = "ABCD1234567";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";

			ForwardingPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PKG";
			packLine1.JL_JC = container1.PK;

			ForwardingPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_JC = container3.PK;

			ForwardingPackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.CurrentConsol = consol2;
			packLine3.JL_PackageCount = 5;
			packLine3.JL_F3_NKPackType = "PKG";
			packLine3.JL_JC = container4.PK;

			shipment.CurrentConsolForDocuments = null;
			AssertEquals("Pre-condition: the departure consol on the shipment should be consol1", consol1, shipment.DepartureConsolForDocuments);
			AssertEquals("Pre-condition: the arrival consol on the shipment should be consol2", consol2, shipment.ArrivalConsolForDocuments);

			ContainerWrapperCollection departureCollection = new ContainerWrapperCollection(shipment, "DEP", Factory);

			AssertEquals("collection.Count (DEP)", 2, departureCollection.Count);
			AssertEquals("collection[0].WrappedObject (DEP)", container1, departureCollection[0].WrappedObject);
			AssertEquals("collection[1].WrappedObject (DEP)", container3, departureCollection[1].WrappedObject);

			ContainerWrapperCollection arrivalCollection = new ContainerWrapperCollection(shipment, "ARV", Factory);

			AssertEquals("collection.Count (ARV)", 1, arrivalCollection.Count);
			AssertEquals("collection[0].WrappedObject (ARV)", container4, arrivalCollection[0].WrappedObject);

			ContainerWrapperCollection allCollection = new ContainerWrapperCollection(shipment, "ANY", Factory);

			AssertEquals("collection.Count (ANY)", 3, allCollection.Count);
			AssertEquals("collection[0].WrappedObject (ANY)", container1, allCollection[0].WrappedObject);
			AssertEquals("collection[1].WrappedObject (ANY)", container3, allCollection[1].WrappedObject);
			AssertEquals("collection[2].WrappedObject (ANY)", container4, allCollection[2].WrappedObject);

			shipment.CurrentConsolForDocuments = consol2;
			ContainerWrapperCollection newCollection = new ContainerWrapperCollection(shipment, "ANY", Factory);

			AssertEquals("collection[0].WrappedObject (ARV)", container4, newCollection[0].WrappedObject);
		}

		public void TestLoadFromCFSShipment()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			CFSContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "OOCL0000011";
			CFSContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";

			CFSShipment shipment = consol.Shipments.AddNew();

			CFSPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PKG";
			packLine1.JL_JC = container1.PK;

			CFSPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_JC = container3.PK;

			ContainerWrapperCollection collection = new ContainerWrapperCollection(shipment, Factory);

			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("collection[0].WrappedObject", container1, collection[0].WrappedObject);
			AssertEquals("collection[1].WrappedObject", container3, collection[1].WrappedObject);
		}

		public void TestLoadFromAgencyShipment()
		{
			var shipment = GetAgencyShipment(Constants.ContainerModes.FCL);
			Assert("shipment has containers", shipment.ShippingContainers.Count > 0);
			var collection = new ContainerWrapperCollection(shipment, Factory);

			var wrappedObjects = collection.Cast<ContainerWrapper>().Select(w => w.WrappedObject);
			AssertContainsExactElementsInAnyOrder(shipment.ShippingContainers, wrappedObjects);

			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				shipment = GetAgencyShipment(mode);
				Assert("shipment has containers", shipment.ShippingContainers.Count > 0);

				collection = new ContainerWrapperCollection(shipment, Factory);
				AssertEquals("No container wrappers created for non-FCL shipment", 0, collection.Count);
			}
		}

		AgencyShipment GetAgencyShipment(string cargoType)
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = cargoType;

			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			AgencyShipmentContainer container2 = shipment.BookedContainers.AddNew();
			container2.JC_ContainerNum = "OOCL0000011";
			AgencyShipmentContainer container3 = shipment.BookedContainers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";

			AgencyShipmentPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PKG";
			packLine1.JL_JC = container1.PK;

			AgencyShipmentPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_JC = container3.PK;

			return shipment;
		}

		public void TestLoadFromAgencyShipmentContainer()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerNum = "OOCL0000006";

			AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "PKG";
			packLine.JL_JC = container.PK;

			ContainerWrapperCollection collection = new ContainerWrapperCollection(container, Factory);

			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals("collection[0].WrappedObject", container, collection[0].WrappedObject);
		}

		public void TestLoadFromDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();

			ContainerWrapperCollection collection = new ContainerWrapperCollection(declaration, Factory);

			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("collection[0].WrappedObject", container1, collection[0].WrappedObject);
			AssertEquals("collection[1].WrappedObject", container2, collection[1].WrappedObject);
		}

		public void TestLoadFromCartage()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			var containerBookedMovesComparer = new ContainerBookedMovesComparer();
			cartage.ContainerBookedMoves.ApplySort(containerBookedMovesComparer);
			CommonContainer container1 = cartage.ContainerBookedMoves.AddNew().Container;
			container1.JC_ContainerNum = "OCLM123456";
			container1.JC_SealNum = "SEAL23";
			CommonContainer container2 = cartage.ContainerBookedMoves.AddNew().Container;
			container2.JC_ContainerNum = "LLKF09999123";
			container2.JC_SealNum = "SEAL43";

			Factory.Save();

			ContainerWrapperCollection collection = new ContainerWrapperCollection(new FreightWrapperFromCartage(cartage, Factory), Factory);

			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("First object is correct", container1, collection[0].WrappedObject);
			AssertEquals("First container number", "OCLM123456", collection[0].ContainerNo);
			AssertEquals("First seal number", "SEAL23", collection[0].SealNo);
			AssertEquals("Second object is correct", container2, collection[1].WrappedObject);
			AssertEquals("Second container number", "LLKF09999123", collection[1].ContainerNo);
			AssertEquals("Second seal number", "SEAL43", collection[1].SealNo);
		}

		public void TestLoadFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "OCLU123456";
			container1.JC_SealNum = "POP1234";
			ForwardingContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BACU8979832";
			container2.JC_SealNum = "SMTP9876";

			ContainerWrapperCollection collection = new ContainerWrapperCollection(consol, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("First object is correct", container1, collection[0].WrappedObject);
			AssertEquals("First container number", "OCLU123456", collection[0].ContainerNo);
			AssertEquals("First seal number", "POP1234", collection[0].SealNo);
			AssertEquals("Second object is correct", container2, collection[1].WrappedObject);
			AssertEquals("Second container number", "BACU8979832", collection[1].ContainerNo);
			AssertEquals("Second seal number", "SMTP9876", collection[1].SealNo);
		}

		public void TestLoadFromLoadList()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "OCLU123456";
			container1.JC_SealNum = "POP1234";
			CFSContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BACU8979832";
			container2.JC_SealNum = "SMTP9876";

			ContainerWrapperCollection collection = new ContainerWrapperCollection(consol, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("First object is correct", container1, collection[0].WrappedObject);
			AssertEquals("First container number", "OCLU123456", collection[0].ContainerNo);
			AssertEquals("First seal number", "POP1234", collection[0].SealNo);
			AssertEquals("Second object is correct", container2, collection[1].WrappedObject);
			AssertEquals("Second container number", "BACU8979832", collection[1].ContainerNo);
			AssertEquals("Second seal number", "SMTP9876", collection[1].SealNo);
		}

		public void TestLoadFromQuotedBooking()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			var container1 = booking.QuotedBookingContainers.AddNew();
			container1.JC_ContainerNum = "ASDF0987654";
			var container2 = booking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerNum = "LKJH0909090";

			var collection = new ContainerWrapperCollection(booking, Factory);
			AssertEquals("Collection Count", 2, collection.Count);
			AssertEquals(container1, collection[0].WrappedObject);
			AssertEquals(container2, collection[1].WrappedObject);
		}

		public void TestLoadFromOrder()
		{
			Order order = Factory.New<Order>();
			OrderContainer container1 = order.PlannedContainers.AddNew();
			container1.J1_ContainerNumber = "OESD2132898";
			container1.J1_SealNum = "JKDH3242";
			OrderContainer container2 = order.PlannedContainers.AddNew();
			container2.J1_ContainerNumber = "KJAD9879834";
			container2.J1_SealNum = "3478SAD";

			ContainerWrapperCollection collection = new ContainerWrapperCollection(order, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("First object is correct", container1, collection[0].WrappedObject);
			AssertEquals("First container number", "OESD2132898", collection[0].ContainerNo);
			AssertEquals("First seal number", "JKDH3242", collection[0].SealNo);
			AssertEquals("Second object is correct", container2, collection[1].WrappedObject);
			AssertEquals("Second container number", "KJAD9879834", collection[1].ContainerNo);
			AssertEquals("Second seal number", "3478SAD", collection[1].SealNo);
		}

		public void TestLoadFromCommonPickupDeliveryConfirm()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "ASDF1234567";
			container1.JC_SealNum = "JKLH7898";
			CFSContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "QWER2345678";
			container2.JC_SealNum = "PLMN9876";

			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			CFSPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PKG";
			packLine1.JL_JC = container1.PK;

			CFSPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 20;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_JC = container2.PK;

			CFSPackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 15;
			packLine3.JL_F3_NKPackType = "PKG";
			packLine3.JL_JC = container1.PK;

			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot1 = packLine1.ConfirmDivots[0];
			CommonConfirmDivot divot2 = packLine2.ConfirmDivots[0];
			CommonConfirmDivot divot3 = packLine3.ConfirmDivots[0];
			divot1.J8_PackagesDelivered = 6;
			divot1.J8_PackagesDelivered = 15;
			divot1.J8_PackagesDelivered = 9;

			ContainerWrapperCollection collection = new ContainerWrapperCollection(confirm, Factory);
			AssertEquals("collection count", 2, collection.Count);
		}

		public void TestLoadFromContainerDetention()
		{
			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100029";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			ContainerMovement movement1 = stock1.Movements.AddNew();
			movement1.E9_OtherLocation = "Movement1";

			ContainerMovement movement2 = stock2.Movements.AddNew();
			movement2.E9_OtherLocation = "Movement2";

			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.Movements.Add(movement1);
			detention.Movements.Add(movement2);

			ContainerWrapperCollection collection = new ContainerWrapperCollection(detention, Factory);
			AssertContainsExactElementsInAnyOrder("",
				m => m.E9_OtherLocation,
				new ContainerMovement[] { movement1, movement2 },
				Array.ConvertAll(collection.ToArray<ContainerWrapper>(), w => (ContainerMovement)w.WrappedObject));
		}

		#region TestLoadFromDtbBooking

		public void TestLoadFromDtbBooking()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJob = Factory.New<PkgPackageJob>();
			PkgPackage container1_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage container2_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage container3_Instruction2 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage container4_NoInstruction = packageJob.Packages.AddNew(Constants.PkgUnit.Container);

			// way to determine which container is which and that qty is used from packages rather that from Divots.
			container1_Instruction1.KP_PackageQty = 1;
			container2_Instruction1.KP_PackageQty = 2;
			container3_Instruction2.KP_PackageQty = 3;
			container4_NoInstruction.KP_PackageQty = 4;

			var booking = helper.CreateBooking();
			DtbBookingInstruction instruction1 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instruction2 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instruction3 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot1 = helper.CreatePackageDivot(instruction1, container1_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot2 = helper.CreatePackageDivot(instruction1, container2_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction2_PkgDivot1 = helper.CreatePackageDivot(instruction2, container3_Instruction2, 1);

			var containerWrapperCollection = new ContainerWrapperCollection(booking, Factory);
			AssertEquals("Wrong number of Containers were created", 3, containerWrapperCollection.Count);
			AssertContainsContainerFor(containerWrapperCollection, container1_Instruction1);
			AssertContainsContainerFor(containerWrapperCollection, container2_Instruction1);
			AssertContainsContainerFor(containerWrapperCollection, container3_Instruction2);
		}

		void AssertContainsContainerFor(ContainerWrapperCollection containerWrapperCollection, PkgPackage expectedContainer)
		{
			foreach (ContainerWrapper containerWrapper in containerWrapperCollection)
			{
				if (containerWrapper.ContainerCount == expectedContainer.KP_PackageQty)
				{
					return;
				}
			}
			Fail(string.Format("No Container could be found for container package with Qty '{0}'", expectedContainer.KP_PackageQty));
		}

		public void TestAddingContainerFromTransportBooking_ShouldCheckInstructionPackageIsNotNull()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJob = Factory.New<PkgPackageJob>();

			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			package1.KP_PackageQty = 1;

			var booking = helper.CreateBooking();

			var instruction1 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			var packageDivot1 = instruction1.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var instruction2 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			_ = instruction2.PackageDivots.AddNew();

			AssertNoExceptionThrown(() => { new ContainerWrapperCollection(booking, Factory); });
		}

		#endregion

		#region TestLoadFromDtbBookingConsolidation

		public void TestLoadFromDtbBookingConsolidation()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJobA = Factory.New<PkgPackageJob>();
			PkgPackage containerA1_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerA2_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerA3_InstructionA2 = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerA4_NoInstruction = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);

			var packageJobB = Factory.New<PkgPackageJob>();
			PkgPackage containerB1_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerB2_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerB3_InstructionB2 = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerB4_NoInstruction = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);

			// way to determine which container is which and that qty is used from packages rather that from Divots.
			containerA1_InstructionA1.KP_PackageQty = 1;
			containerA2_InstructionA1.KP_PackageQty = 2;
			containerA3_InstructionA2.KP_PackageQty = 3;
			containerA4_NoInstruction.KP_PackageQty = 4;
			containerB1_InstructionB1.KP_PackageQty = 5;
			containerB2_InstructionB1.KP_PackageQty = 6;
			containerB3_InstructionB2.KP_PackageQty = 7;
			containerB4_NoInstruction.KP_PackageQty = 8;

			var bookingConsolidation = helper.CreateConsolidation();
			var bookingA = helper.CreateBooking(bookingConsolidation);
			var bookingB = helper.CreateBooking(bookingConsolidation);

			DtbBookingInstruction instructionA1 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA2 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA3 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA1 = helper.CreatePackageDivot(instructionA1, containerA1_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA2 = helper.CreatePackageDivot(instructionA1, containerA2_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA2_PkgDivotA1 = helper.CreatePackageDivot(instructionA2, containerA3_InstructionA2, 1);

			DtbBookingInstruction instructionB1 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB2 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB3 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB1 = helper.CreatePackageDivot(instructionB1, containerB1_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB2 = helper.CreatePackageDivot(instructionB1, containerB2_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB2_PkgDivotB1 = helper.CreatePackageDivot(instructionB2, containerB3_InstructionB2, 1);

			var containerWrapperCollection = new ContainerWrapperCollection(bookingConsolidation, Factory);
			AssertEquals("Wrong number of Containers were created", 6, containerWrapperCollection.Count);
			AssertContainsContainerFor(containerWrapperCollection, containerA1_InstructionA1);
			AssertContainsContainerFor(containerWrapperCollection, containerA2_InstructionA1);
			AssertContainsContainerFor(containerWrapperCollection, containerA3_InstructionA2);
			AssertContainsContainerFor(containerWrapperCollection, containerB1_InstructionB1);
			AssertContainsContainerFor(containerWrapperCollection, containerB2_InstructionB1);
			AssertContainsContainerFor(containerWrapperCollection, containerB3_InstructionB2);
		}

		#endregion

		#region Implementation

		protected override ContainerWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ContainerWrapperCollection((BaseJobDeclaration)null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ContainerWrapperFromCustoms(null, Factory);
		}

		#endregion
	}
}
