using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CartageInfoWrapperFromIDocCartageAdvice))]
	sealed class CartageInfoFromIDocCartageAdviceTest : CartageInfoWrapperTest
	{
		public void TestJourneyOneDeliverToDateIsReadFromDeclarationAndFallsBackToContainer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "Supplies for the Stars";

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Awesome Emporium";

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			container.CO_Seal = "SEALYA";
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			declaration.JE_EstimatedDeliveryOrPickup = new ZDateTime(2010, 4, 13);

			var wrapper = FreightWrapper.New(declaration, container, Factory)[0];
			AssertEquals("wrapper.CartageInfo.JourneyOneDeliverToDate from Declaration", new ZDateTime(2010, 4, 13), wrapper.CartageInfo.JourneyOneDeliverToDate);

			container.JobContainer.JC_ArrivalEstimatedDelivery = new ZDateTime(2010, 4, 12);
			wrapper = FreightWrapper.New(declaration, container, Factory)[0];
			AssertEquals("wrapper.CartageInfo.JourneyOneDeliverToDate from Container", new ZDateTime(2010, 4, 12), wrapper.CartageInfo.JourneyOneDeliverToDate);
		}

		public void TestDocCommonCartageLeg()
		{
			CommonCartageLeg cartageLegBO = Factory.New<CommonCartageLeg>();
			DocCommonCartageLeg docCartageLeg = DocCommonCartageLeg.New(cartageLegBO, Factory);
			CartageInfoWrapper wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(docCartageLeg, Factory);
			AssertEquals("wrapperFull.LegNotes", "", wrapperFull.LegNotes);

			cartageLegBO.JU_LegNotes = "LegNotes";
			wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(docCartageLeg, Factory);
			AssertEquals("wrapperFull.LegNotes", "LegNotes", wrapperFull.LegNotes);
		}

		public void TestDocCusCommonCartageLeg()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			ZDateTime now = ZDateTime.Now;
			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec.CusContainers.AddNew();
			Customs.AU.DocCusContainer docCusContainer = Enterprise.DocumentWrappers.Customs.AU.DocCusContainer.New(dec.CusContainers[0], Factory);
			docCusContainer.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			CartageInfoWrapper wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(docCusContainer, Factory);
			AssertEquals("wrapperFull.JourneyOnePickUpDate", ZDateTime.Empty, wrapperFull.JourneyOnePickUpDate);

			dec.CusContainers[0].JobContainer.JC_ArrivalSlotDateTime = now;

			docCusContainer = Enterprise.DocumentWrappers.Customs.AU.DocCusContainer.New(dec.CusContainers[0], Factory);
			docCusContainer.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(docCusContainer, Factory);
			AssertEquals("wrapperFull.JourneyOnePickUpDate", now, wrapperFull.JourneyOnePickUpDate);

			dec.CusContainers[0].JobContainer.JC_ContainerImportDORelease = "DOREL";
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			docCusContainer = Enterprise.DocumentWrappers.Customs.AU.DocCusContainer.New(dec.CusContainers[0], Factory);
			wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(docCusContainer, Factory);
			AssertEquals("DOREL", wrapperFull.JourneyOnePickUpReleaseNum);
		}

		public void TestDocConfirmation()
		{
			ZDateTime now = ZDateTime.Now;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_PlannedPickupDeliveryTime = now;
			DocPickupDeliveryConfirm confirmWrapper = DocPickupDeliveryConfirm.New(confirm, shipment, Factory);
			CartageInfoWrapper wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(confirmWrapper, Factory);
			AssertEquals("wrapperFull.JourneyOnePickUpDate", now, wrapperFull.JourneyOnePickUpDate);
			AssertEquals("wrapperFull.JourneyOneDeliverToDate", ZDateTime.Empty, wrapperFull.JourneyOneDeliverToDate);

			confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PlannedPickupDeliveryTime = now;
			confirmWrapper = DocPickupDeliveryConfirm.New(confirm, shipment, Factory);
			wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(confirmWrapper, Factory);
			AssertEquals("wrapperFull.JourneyOnePickUpDate", ZDateTime.Empty, wrapperFull.JourneyTwoPickUpDate);
			AssertEquals("wrapperFull.JourneyOneDeliverToDate", now, wrapperFull.JourneyTwoDeliverToDate);
		}

		public void TestAddresses()
		{
			ZDateTime now = ZDateTime.Now;
			CommonConsol fclConsol = Factory.New<CommonConsol>();
			fclConsol.JK_TransportMode = Constants.TransportModes.Sea;
			fclConsol.JK_ConsolMode = Constants.ContainerModes.FCL;
			CommonContainer fclContainer = fclConsol.Containers.AddNew();
			CommonShipment fclShipment = fclConsol.Shipments.AddNew();
			fclShipment.JS_TransportMode = Constants.TransportModes.Sea;
			fclShipment.JS_PackingMode = Constants.ContainerModes.FCL;

			CommonConsol lclConsol = Factory.New<CommonConsol>();
			lclConsol.JK_TransportMode = Constants.TransportModes.Sea;
			lclConsol.JK_ConsolMode = Constants.ContainerModes.LCL;
			CommonContainer lclContainer = lclConsol.Containers.AddNew();
			CommonShipment lclShipment = lclConsol.Shipments.AddNew();
			lclShipment.JS_TransportMode = Constants.TransportModes.Sea;
			lclShipment.JS_PackingMode = Constants.ContainerModes.LCL;

			OrgHeader originCTOOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			fclConsol.JK_OA_DepartureCTOAddress = originCTOOrg.MainAddress.PK;
			lclConsol.JK_OA_DepartureCTOAddress = originCTOOrg.MainAddress.PK;

			OrgHeader destinationCTOOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			fclConsol.JK_OA_ArrivalCTOAddress = destinationCTOOrg.MainAddress.PK;
			lclConsol.JK_OA_ArrivalCTOAddress = destinationCTOOrg.MainAddress.PK;

			OrgHeader cnrOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			fclShipment.ConsignorPickupAddress.E2_OA_Address = cnrOrg.MainAddress.PK;
			lclShipment.ConsignorPickupAddress.E2_OA_Address = cnrOrg.MainAddress.PK;

			OrgHeader cneOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D"));
			fclShipment.ConsigneeDeliveryAddress.E2_OA_Address = cneOrg.MainAddress.PK;
			lclShipment.ConsigneeDeliveryAddress.E2_OA_Address = cneOrg.MainAddress.PK;

			OrgHeader originCFSOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "E"));
			fclConsol.JK_OA_PackDepotAddress = originCFSOrg.MainAddress.PK;
			lclConsol.JK_OA_PackDepotAddress = originCFSOrg.MainAddress.PK;

			OrgHeader destinationCFSOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "F"));
			fclConsol.JK_OA_UnpackDepotAddress = destinationCFSOrg.MainAddress.PK;
			lclConsol.JK_OA_UnpackDepotAddress = destinationCFSOrg.MainAddress.PK;

			OrgHeader originCYDOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "G"));
			fclConsol.JK_OA_ContainerYardEmptyPickupAddress = originCYDOrg.MainAddress.PK;
			lclConsol.JK_OA_ContainerYardEmptyPickupAddress = originCYDOrg.MainAddress.PK;

			OrgHeader destinationCYDOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "H"));
			fclConsol.JK_OA_ContainerYardEmptyReturnAddress = destinationCYDOrg.MainAddress.PK;
			lclConsol.JK_OA_ContainerYardEmptyReturnAddress = destinationCYDOrg.MainAddress.PK;

			fclContainer.JC_DepartureSlotDateTime = now;
			fclContainer.JC_DepartureSlotReference = "DEPREF1";
			fclContainer.JC_ArrivalSlotDateTime = now.AddDays(6);
			fclContainer.JC_ArrivalSlotReference = "ARVREF1";

			lclContainer.JC_DepartureSlotDateTime = now.AddDays(1);
			lclContainer.JC_DepartureSlotReference = "DEPREF1";
			lclContainer.JC_ArrivalSlotDateTime = now.AddDays(7);
			lclContainer.JC_ArrivalSlotReference = "ARVREF1";

			PackLine fclLine = fclShipment.OuterPackLines.AddNew();
			PackLine lclLine = lclShipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm containerisedPickupConfirm = fclContainer.OriginConfirm;
			CommonPickupDeliveryConfirm containerisedDeliveryConfirm = fclContainer.DestinationConfirm;
			CommonPickupDeliveryConfirm loosePickupConfirm = lclShipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm looseDeliveryConfirm = lclShipment.DeliveryConfirms.AddNew();

			containerisedPickupConfirm.EU_PlannedPickupDeliveryTime = now.AddDays(-1);
			containerisedDeliveryConfirm.EU_PlannedPickupDeliveryTime = now.AddDays(8);
			loosePickupConfirm.EU_PlannedPickupDeliveryTime = now.AddDays(-2);
			looseDeliveryConfirm.EU_PlannedPickupDeliveryTime = now.AddDays(9);

			DocPickupDeliveryConfirm docContainerisedPickupConfirm = DocPickupDeliveryConfirm.New(containerisedPickupConfirm, Factory);
			DocPickupDeliveryConfirm docContainerisedDeliveryConfirm = DocPickupDeliveryConfirm.New(containerisedDeliveryConfirm, Factory);
			DocPickupDeliveryConfirm docLoosePickupConfirm = DocPickupDeliveryConfirm.New(loosePickupConfirm, Factory);
			DocPickupDeliveryConfirm docLooseDeliveryConfirm = DocPickupDeliveryConfirm.New(looseDeliveryConfirm, Factory);

			CartageInfoWrapper infoContainerisedPickupConfirm = new CartageInfoWrapperFromIDocCartageAdvice(docContainerisedPickupConfirm, Factory);
			CartageInfoWrapper infoContainerisedDeliveryConfirm = new CartageInfoWrapperFromIDocCartageAdvice(docContainerisedDeliveryConfirm, Factory);
			CartageInfoWrapper infoLoosePickupConfirm = new CartageInfoWrapperFromIDocCartageAdvice(docLoosePickupConfirm, Factory);
			CartageInfoWrapper infoLooseDeliveryConfirm = new CartageInfoWrapperFromIDocCartageAdvice(docLooseDeliveryConfirm, Factory);

			AssertEquals(cnrOrg.MainAddress.OA_Address1, infoContainerisedPickupConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(originCTOOrg.MainAddress.OA_Address1, infoContainerisedPickupConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(cnrOrg.MainAddress.OA_Address1, infoContainerisedPickupConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(originCTOOrg.MainAddress.OA_Address1, infoContainerisedPickupConfirm.JourneyTwoDeliverToAddress.Address1);

			AssertEquals(destinationCTOOrg.MainAddress.OA_Address1, infoContainerisedDeliveryConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, infoContainerisedDeliveryConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(destinationCTOOrg.MainAddress.OA_Address1, infoContainerisedDeliveryConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, infoContainerisedDeliveryConfirm.JourneyTwoDeliverToAddress.Address1);

			AssertEquals(cnrOrg.MainAddress.OA_Address1, infoLoosePickupConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(originCFSOrg.MainAddress.OA_Address1, infoLoosePickupConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(cnrOrg.MainAddress.OA_Address1, infoLoosePickupConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(originCFSOrg.MainAddress.OA_Address1, infoLoosePickupConfirm.JourneyTwoDeliverToAddress.Address1);

			AssertEquals(destinationCFSOrg.MainAddress.OA_Address1, infoLooseDeliveryConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, infoLooseDeliveryConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(destinationCFSOrg.MainAddress.OA_Address1, infoLooseDeliveryConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, infoLooseDeliveryConfirm.JourneyTwoDeliverToAddress.Address1);

			AssertEquals(now.AddDays(-1), infoContainerisedPickupConfirm.JourneyOnePickUpDate);
			AssertEquals(ZDateTime.Empty, infoContainerisedPickupConfirm.JourneyOneDeliverToDate);
			AssertEquals(now.AddDays(-1), infoContainerisedPickupConfirm.JourneyTwoPickUpDate);
			AssertEquals(now, infoContainerisedPickupConfirm.JourneyTwoDeliverToDate);

			AssertEquals(now.AddDays(6), infoContainerisedDeliveryConfirm.JourneyOnePickUpDate);
			AssertEquals(now.AddDays(8), infoContainerisedDeliveryConfirm.JourneyOneDeliverToDate);
			AssertEquals(ZDateTime.Empty, infoContainerisedDeliveryConfirm.JourneyTwoPickUpDate);
			AssertEquals(now.AddDays(8), infoContainerisedDeliveryConfirm.JourneyTwoDeliverToDate);

			AssertEquals(now.AddDays(-2), infoLoosePickupConfirm.JourneyOnePickUpDate);
			AssertEquals(ZDateTime.Empty, infoLoosePickupConfirm.JourneyOneDeliverToDate);
			AssertEquals(now.AddDays(-2), infoLoosePickupConfirm.JourneyTwoPickUpDate);
			AssertEquals(ZDateTime.Empty, infoLoosePickupConfirm.JourneyTwoDeliverToDate);

			AssertEquals(ZDateTime.Empty, infoLooseDeliveryConfirm.JourneyOnePickUpDate);
			AssertEquals(now.AddDays(9), infoLooseDeliveryConfirm.JourneyOneDeliverToDate);
			AssertEquals(ZDateTime.Empty, infoLooseDeliveryConfirm.JourneyTwoPickUpDate);
			AssertEquals(now.AddDays(9), infoLooseDeliveryConfirm.JourneyTwoDeliverToDate);
		}

		public override void TestWrapperMappingsEmpty()
		{
			CartageInfoWrapper wrapperEmpty = new CartageInfoWrapperFromIDocCartageAdvice(null, Factory);
			AssertEquals("wrapperEmpty.EmailSubjectNumber", "", wrapperEmpty.EmailSubjectNumber);
			AssertEquals("wrapperEmpty.JourneyOnePickUpHeading", "", wrapperEmpty.JourneyOnePickUpHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToHeading", "", wrapperEmpty.JourneyOneDeliverToHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpHeading", "", wrapperEmpty.JourneyTwoPickUpHeading);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToHeading", "", wrapperEmpty.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperEmpty.JourneyOnePickUpAddress.AddressAsASingleLine", "", wrapperEmpty.JourneyOnePickUpAddress.AddressAsASingleLine);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToAddress.AddressAsASingleLine", "", wrapperEmpty.JourneyOneDeliverToAddress.AddressAsASingleLine);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpAddress.AddressAsASingleLine", "", wrapperEmpty.JourneyTwoPickUpAddress.AddressAsASingleLine);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToAddress.AddressAsASingleLine", "", wrapperEmpty.JourneyTwoDeliverToAddress.AddressAsASingleLine);
			AssertEquals("wrapperEmpty.JourneyOnePickUpContactName", "", wrapperEmpty.JourneyOnePickUpContactName);
			AssertEquals("wrapperEmpty.JourneyOnePickUpContactPhone", "", wrapperEmpty.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToContactName", "", wrapperEmpty.JourneyOneDeliverToContactName);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToContactPhone", "", wrapperEmpty.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpContactName", "", wrapperEmpty.JourneyTwoPickUpContactName);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpContactPhone", "", wrapperEmpty.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToContactName", "", wrapperEmpty.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToContactPhone", "", wrapperEmpty.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperEmpty.PrintAsContainers", false, wrapperEmpty.PrintAsContainers);
			AssertEquals("wrapperEmpty.PrintTwoJourneys", false, wrapperEmpty.PrintTwoJourneys);
			AssertEquals("wrapperEmpty.EquipmentType", "", wrapperEmpty.EquipmentType);
			AssertEquals("wrapperEmpty.FullHandlingInstructions", "", wrapperEmpty.FullHandlingInstructions);
			AssertEquals("wrapperEmpty.FullCartageInstructions", "", wrapperEmpty.FullCartageInstructions);
			AssertEquals("wrapperEmpty.AddressesWithWareHousing", 0, wrapperEmpty.AddressesWithWareHousing.Count);
			AssertNull("wrapperEmpty.CartageAdvice", wrapperEmpty.CartageAdvice);
			AssertNotNull("wrapperEmpty.CurrentCompany", wrapperEmpty.CurrentCompany);
			AssertEquals("wrapperEmpty.IsAir", false, wrapperEmpty.IsAir);

			AssertEquals("wrapperEmpty.JourneyOnePickUpDate", ZDateTime.Empty, wrapperEmpty.JourneyOnePickUpDate);
			AssertEquals("wrapperEmpty.JourneyOnePickUpDateHeading", "Date:", wrapperEmpty.JourneyOnePickUpDateHeading);
			AssertEquals("wrapperEmpty.JourneyOnePickUpRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyOnePickUpRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyOnePickUpRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyOnePickUpRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToDate", ZDateTime.Empty, wrapperEmpty.JourneyOneDeliverToDate);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToDateHeading", "Date:", wrapperEmpty.JourneyOneDeliverToDateHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyOneDeliverToRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyOneDeliverToRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoPickUpDate);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpDateHeading", "Date:", wrapperEmpty.JourneyTwoPickUpDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoPickUpRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyTwoPickUpRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoDeliverToDate);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToDateHeading", "Date:", wrapperEmpty.JourneyTwoDeliverToDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoDeliverToRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyTwoDeliverToRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyOnePickUpReleaseNum", "", wrapperEmpty.JourneyOnePickUpReleaseNum);
			AssertEquals("wrapperEmpty.JourneyOnePickUpSlofRef", "", wrapperEmpty.JourneyOnePickUpSlofRef);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToReleaseNum", "", wrapperEmpty.JourneyTwoDeliverToReleaseNum);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToSlofRef", "", wrapperEmpty.JourneyTwoDeliverToSlofRef);
			AssertEquals("wrapperEmpty.LegNotes", "", wrapperEmpty.LegNotes);
		}

		public override void TestWrapperMappingFull()
		{
			IDocCartageAdvice cartageInfo = new TestIDocCartageAdvice(Factory);
			CartageInfoWrapper wrapperFull = new CartageInfoWrapperFromIDocCartageAdvice(cartageInfo, Factory);
			AssertEquals("wrapperFull.EmailSubjectNumber", "EmailSubjectNumber", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP FULL", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO FULL", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "JourneyOnePickUpAddress", wrapperFull.JourneyOnePickUpAddress.Address1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "JourneyOneDeliverToAddress", wrapperFull.JourneyOneDeliverToAddress.Address1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "JourneyTwoPickUpAddress", wrapperFull.JourneyTwoPickUpAddress.Address1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "JourneyTwoDeliverToAddress", wrapperFull.JourneyTwoDeliverToAddress.Address1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "JourneyOnePickUpContactName", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "JourneyOnePickUpContactPhone", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "JourneyOneDeliverToContactName", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "JourneyOneDeliverToContactPhone", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "JourneyTwoPickUpContactName", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "JourneyTwoPickUpContactPhone", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "JourneyTwoDeliverToContactName", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "JourneyTwoDeliverToContactPhone", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", true, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", true, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "EquipmentType", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "FullHandlingInstructions", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "FullCartageInstructions", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "AddressesWithWareHousing", wrapperFull.AddressesWithWareHousing[0].Address1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", true, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", new ZDateTime(2009, 1, 1), wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", new ZDateTime(2009, 1, 15), wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", new ZDateTime(2008, 12, 12), wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", new ZDateTime(2009, 2, 1), wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2009, 2, 15), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", new ZDateTime(2008, 12, 25), wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "Pickup or Storage Heading", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		public void TestHeadingsChangeWhenLanguageChanges()
		{
			using (var grmMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				grmMockData.Put("208b00cd-4a0f-4441-8760-1ccfbb7f069b", new ResourceStringData("208b00cd-4a0f-4441-8760-1ccfbb7f069b", string.Empty, string.Empty, "Etad:", string.Empty));

				CartageInfoWrapper wrapperEmpty = new CartageInfoWrapperFromIDocCartageAdvice(null, Factory);
				AssertEquals("wrapperEmpty.JourneyOnePickUpDateHeading", "Date:", wrapperEmpty.JourneyOnePickUpDateHeading);
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
				{
					wrapperEmpty = new CartageInfoWrapperFromIDocCartageAdvice(null, Factory);
					AssertEquals("wrapperEmpty.JourneyOnePickUpDateHeading", "Etad:", wrapperEmpty.JourneyOnePickUpDateHeading);
				}

				wrapperEmpty = new CartageInfoWrapperFromIDocCartageAdvice(null, Factory);
				AssertEquals("wrapperEmpty.JourneyOnePickUpDateHeading", "Date:", wrapperEmpty.JourneyOnePickUpDateHeading);
			}
		}

		class TestIDocCartageAdvice : IDocCartageAdvice
		{
			public TestIDocCartageAdvice(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly BusinessObjectFactory factory;

			public ZString EmailSubjectNumber
			{
				get { return "EmailSubjectNumber"; }
			}

			public MultilingualString JourneyOnePickUpHeading
			{
				get { return (NoResString)"PICKUP some crap text"; }
			}

			public MultilingualString JourneyOneDeliverToHeading
			{
				get { return (NoResString)"DELIVER TO some crap text"; }
			}

			public MultilingualString JourneyTwoPickUpHeading
			{
				get { return (NoResString)"PICKUP FULL some crap text"; }
			}

			public MultilingualString JourneyTwoDeliverToHeading
			{
				get { return (NoResString)"DELIVER TO FULL some crap text"; }
			}

			public DocDocAddress JourneyOnePickUpAddress
			{
				get { return GetDocDocAddress("JourneyOnePickUpAddress"); }
			}

			public DocDocAddress JourneyOneDeliverToAddress
			{
				get { return GetDocDocAddress("JourneyOneDeliverToAddress"); }
			}

			public DocDocAddress JourneyTwoPickUpAddress
			{
				get { return GetDocDocAddress("JourneyTwoPickUpAddress"); }
			}

			public DocDocAddress JourneyTwoDeliverToAddress
			{
				get { return GetDocDocAddress("JourneyTwoDeliverToAddress"); }
			}

			DocDocAddress GetDocDocAddress(ZString seed)
			{
				OrgAddress address = factory.New<OrgAddress>();
				address.OA_Address1 = seed;
				return DocDocAddress.New(address, factory);
			}

			public ZString JourneyOnePickUpContactName
			{
				get { return "JourneyOnePickUpContactName"; }
			}

			public ZString JourneyOnePickUpContactPhone
			{
				get { return "JourneyOnePickUpContactPhone"; }
			}

			public ZString JourneyOneDeliverToContactName
			{
				get { return "JourneyOneDeliverToContactName"; }
			}

			public ZString JourneyOneDeliverToContactPhone
			{
				get { return "JourneyOneDeliverToContactPhone"; }
			}

			public ZString JourneyTwoPickUpContactName
			{
				get { return "JourneyTwoPickUpContactName"; }
			}

			public ZString JourneyTwoPickUpContactPhone
			{
				get { return "JourneyTwoPickUpContactPhone"; }
			}

			public ZString JourneyTwoDeliverToContactName
			{
				get { return "JourneyTwoDeliverToContactName"; }
			}

			public ZString JourneyTwoDeliverToContactPhone
			{
				get { return "JourneyTwoDeliverToContactPhone"; }
			}

			public ZBool PrintAsContainers
			{
				get { return true; }
			}

			public ZBool PrintTwoJourneys
			{
				get { return true; }
			}

			public ZString EquipmentType
			{
				get { return "EquipmentType"; }
			}

			public ZString FullHandlingInstructions
			{
				get { return "FullHandlingInstructions"; }
			}

			public ZString FullCartageInstructions
			{
				get { return "FullCartageInstructions"; }
			}

			public DocDocAddressCollection AddressesWithWareHousing
			{
				get
				{
					DocDocAddressCollection result = new DocDocAddressCollection(factory);
					result.Add(GetDocDocAddress("AddressesWithWareHousing"));
					return result;
				}
			}

			public CartageAdviceHelper CartageAdvice
			{
				get { return new CartageAdviceHelper(this, factory); }
			}

			public DocCompany CurrentCompany
			{
				get { return DocCompany.New(GlbCompany.CurrentCompany, factory); }
			}

			public ZBool IsAir
			{
				get { return true; }
			}

			public ZDateTime CartageCutOffDate
			{
				get { return new ZDateTime(2009, 1, 1); }
			}

			public ZDateTime CartageAvailableDate
			{
				get { return new ZDateTime(2009, 1, 15); }
			}

			public ZDateTime CutOffOrAvailableDate
			{
				get { return new ZDateTime(2008, 12, 12); }
			}

			public ZDateTime CartageReceivalDate
			{
				get { return new ZDateTime(2009, 2, 1); }
			}

			public ZDateTime CartageStorageCommenceDate
			{
				get { return new ZDateTime(2009, 2, 15); }
			}

			public ZDateTime PickupOrStorageCommenceDate
			{
				get { return new ZDateTime(2008, 12, 25); }
			}

			public ZString PickupOrStorageCommenceDateHeading
			{
				get { return "Pickup or Storage Heading"; }
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
JourneyOneDeliverToAddress : 
JourneyOnePickUpAddress : 
JourneyTwoDeliverToAddress : 
JourneyTwoPickUpAddress : 
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CartageInfoWrapperFromIDocCartageAdvice(null, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CartageInfoWrapperFromIDocCartageAdvice(null, Factory);
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			base.SetUp();
		}
	}
}
