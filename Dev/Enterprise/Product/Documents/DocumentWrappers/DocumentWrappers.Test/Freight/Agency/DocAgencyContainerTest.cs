using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyContainer))]
	sealed class DocAgencyContainerTest : DocumentWrapperTestCase
	{
		public void TestBookingCutOffDate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_CutOff = ZDateTime.Today;

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			sailing.JX_DepotCutOff = ZDateTime.Today.AddDays(4);

			Shipment.JS_JX = sailing.PK;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("BookingCutOffDate", ZDateTime.Today.AddDays(4), DocWrapper.BookingCutOffDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("BookingCutOffDate", ZDateTime.Today, DocWrapper.BookingCutOffDate);
		}

		public void TestPickUpDate()
		{
			AssertEquals("PickUpDate", ZDateTime.Empty, DocWrapper.PickUpDate);

			Shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Today;
			AssertEquals("PickUpDate", ZDateTime.Today, DocWrapper.PickUpDate);
		}

		public void TestAvailableDate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			origin.JA_CutOff = ZDateTime.Today;

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Shipment.JS_JX = sailing.PK;
			Shipment.DocsAndCartage.JP_FCLAvailable = ZDateTime.Today.AddDays(3);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("AvailableDate on shipment", Shipment.DocsAndCartage.JP_FCLAvailable, DocWrapper.AvailableDate);

			Container.JC_FCLAvailable = ZDateTime.Today;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("AvailableDate", Container.JC_FCLAvailable, DocWrapper.AvailableDate);
		}

		public void TestStorageCommenceDate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			origin.JA_CutOff = ZDateTime.Today;

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Shipment.JS_JX = sailing.PK;
			Shipment.DocsAndCartage.JP_FCLStorageCommences = ZDateTime.Today.AddDays(3);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("StorageCommenceDate on shipment", Shipment.DocsAndCartage.JP_FCLStorageCommences, DocWrapper.StorageCommenceDate);

			Container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("StorageCommenceDate", Container.JC_ArrivalCTOStorageStartDate, DocWrapper.StorageCommenceDate);
		}

		public void TestCutOffOrAvailableDate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			origin.JA_CutOff = ZDateTime.Today;

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Shipment.JS_JX = sailing.PK;
			Shipment.Sailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(2);
			Container.JC_FCLAvailable = ZDateTime.Today.AddDays(3);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			docDirection = DocumentDirection.DEP;
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(2), DocWrapper.CutOffOrAvailableDate);

			docDirection = DocumentDirection.ARV;
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(3), DocWrapper.CutOffOrAvailableDate);
		}

		public void TestPickupOrStorageCommenceDate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			origin.JA_CutOff = ZDateTime.Today;

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Shipment.JS_JX = sailing.PK;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Today;
			Container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(1);

			docDirection = DocumentDirection.DEP;
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today, DocWrapper.PickupOrStorageCommenceDate);

			docDirection = DocumentDirection.ARV;
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(1), DocWrapper.PickupOrStorageCommenceDate);

			Container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(2);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			docDirection = DocumentDirection.DEP;
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today, DocWrapper.PickupOrStorageCommenceDate);

			docDirection = DocumentDirection.ARV;
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(2), DocWrapper.PickupOrStorageCommenceDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("FCLBookingContainers.Count", 0, Shipment.BookedContainers.Count);
		}

		public void TestDetentionChargesText()
		{
			AssertEquals("", DocWrapper.DetentionChargesText);

			DocumentsDataRegistry.Instance.ImportDeliveryOrderDetentionChargesText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "random text");
			AssertEquals("random text", DocWrapper.DetentionChargesText);
		}

		public void TestShipmentAndOrgCartageInstruction()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			OrgHeader orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsignorPK = orgConsignor.PK;

			FreightHelperClass.AddNote(Shipment, pickupDesc, "Shipment Pickup Instructions");
			FreightHelperClass.AddNote(Shipment, deliveryDesc, "Shipment Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions");

			docDirection = DocumentDirection.DEP;
			AssertEquals("All Cartage Instructions", "Shipment Pickup Instructions\nConsignee Pickup Instructions\nConsignor Pickup Instructions", DocWrapper.ShipmentAndOrgCartageInstruction);

			docDirection = DocumentDirection.ARV;
			AssertEquals("All Cartage Instructions", "Shipment Delivery Instructions\nConsignor Delivery Instructions\nConsignee Delivery Instructions", DocWrapper.ShipmentAndOrgCartageInstruction);
		}

		public void TestShipmentAndOrgHandlingInstructions()
		{
			StmNote shipmentNote = Shipment.Notes.AddNew();
			shipmentNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			shipmentNote.ST_Table = Shipment.TableName;
			shipmentNote.ST_ParentID = Shipment.PK;
			shipmentNote.ST_NoteDataAsText = "Shipment Handling Instructions";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeNote = consignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = consignee.PK;
			consigneeNote.ST_Table = consignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgConsignee Note";

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorNote = consignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNote.ST_ParentID = consignor.PK;
			consignorNote.ST_Table = consignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgConsignor Note";

			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPK = consignor.PK;

			docDirection = DocumentDirection.DEP;
			AssertEquals("All Handling Instructions", "Shipment Handling Instructions\nOrgConsignor Note", DocWrapper.ShipmentAndOrgHandlingInstructions);

			docDirection = DocumentDirection.ARV;
			AssertEquals("All Handling Instructions", "Shipment Handling Instructions\nOrgConsignee Note", DocWrapper.ShipmentAndOrgHandlingInstructions);
		}

		public void TestEquipmentType()
		{
			AssertEquals("EquipmentType", ZString.Empty, DocWrapper.EquipmentType);
			AssertEquals("EquipmentType", DocWrapper.Shipment.EquipmentType, DocWrapper.EquipmentType);
		}

		public void TestCartageInstructions()
		{
			AssertEquals("CartageInstructions", "", DocWrapper.FullCartageInstructions);
		}

		public void TestForwardingInstructionWeight()
		{
			AssertEquals("", DocWrapper.ForwardingInstructionWeight);
		}

		public void TestForwardingInstructionVolume()
		{
			AssertEquals("", DocWrapper.ForwardingInstructionVolume);
		}

		public void TestForwardingInstructionPackages()
		{
			AssertEquals("", DocWrapper.ForwardingInstructionPackages);
		}

		public void TestDescriptionAndStatus()
		{
			AssertEquals("DescriptionAndStatus ", ZString.Empty, DocWrapper.DescriptionAndStatus);
		}

		public void TestTotalAllocatedShipmentWeight()
		{
			AssertEquals("TotalAllocatedShipmentWeight", 0M, DocWrapper.TotalAllocatedShipmentWeight);
		}

		public void TestTotalPackLineWeight()
		{
			AssertEquals("TotalPackLineWeight", 0M, DocWrapper.TotalPackLineWeight);
		}

		public void TestTotalAllocatedShipmentVolume()
		{
			AssertEquals("TotalAllocatedShipmentVolume", 0M, DocWrapper.TotalAllocatedShipmentVolume);
		}

		public void TestTotalPackLineVolume()
		{
			AssertEquals("TotalPackLineVolume", 0M, DocWrapper.TotalPackLineVolume);
		}

		public void TestTotalPackLinePackages()
		{
			AssertEquals("TotalPackLinePackages", 0, DocWrapper.TotalPackLinePackages);
		}

		public void TestShipmentInheritsDocumentDirectionFromContainer()
		{
			AgencyShipment shipmentBizO = Factory.New<AgencyShipment>();
			AgencyShipmentContainer containerBizO = shipmentBizO.BookedContainers.AddNew();
			DocAgencyContainer containerWrapper = DocAgencyContainer.New(containerBizO, Factory);
			containerWrapper.SetDocumentDirectionForTesting("DEP");
			AssertEquals("Shipment should inherit container wrapper's DocumentDirection", containerWrapper.DocumentDirection, containerWrapper.Shipment.DocumentDirection);
		}

		public void TestConsignee()
		{
			AssertNull("No Consignee", DocWrapper.Consignee);

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee.OH_FullName = "Consignee";
			Shipment.ConsigneePK = consignee.PK;
			AssertEquals("Consignee should be Consignee", "Consignee", DocWrapper.Consignee.Name);
		}

		public void TestConsignor()
		{
			AssertNull("No Consignor", DocWrapper.Consignor);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignor.OH_FullName = "Consignor";
			Shipment.ConsignorPK = consignor.PK;
			AssertEquals("Consignor should be Consignor", "Consignor", DocWrapper.Consignor.Name);
		}

		public void TestDeliveryClerkNote()
		{
			AssertEquals("", DocWrapper.DeliveryClerkNote);

			DocumentsDataRegistry.Instance.ImportDeliveryOrderDeliveryClerkNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "random comment");
			AssertEquals("random comment", DocWrapper.DeliveryClerkNote);
		}

		public void TestContainerQuality()
		{
			Container.JC_ContainerQuality = "XXX";
			AssertEquals("XXX", DocWrapper.ContainerQuality.Code);
		}

		public void TestContainerYard()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipmentContainer>();
			DocAgencyContainer docContainer = DocAgencyContainer.New(container, Factory);

			AssertEquals(ZString.Empty, docContainer.ContainerYard);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			OrgAddress orgAddress = Factory.New<OrgAddress>();
			orgHeader.OH_FullName = "Some org";
			container.JC_OA_DepartureContainerYardAddress = orgAddress.PK;

			AssertEquals(ZString.Empty, docContainer.ContainerYard);

			orgAddress.OA_OH = orgHeader.PK;

			AssertEquals("Some org", docContainer.ContainerYard);
		}

		public void TestShowContainerImportDORelease()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			AgencyShipmentContainer container = Factory.New<AgencyShipmentContainer>();
			DocAgencyContainer docContainer = DocAgencyContainer.New(container, Factory);

			EIDOMessagingHeader detail;

			detail = new EIDOMessagingHeader();
			AgencyRegistry.Instance.EIDOMessagingDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, detail);

			Assert(!docContainer.ShowContainerImportDORelease);

			detail = new EIDOMessagingHeader();
			detail.Email = "someemail@gmail.com";

			EIDOMessagingIdentity identity = detail.Identities.AddNew();
			identity.PrincipalPK = principal.PK;
			identity.Password = "password";
			identity.SenderID = "SenderID";
			identity.RecipientID = "RecipientID";

			AgencyRegistry.Instance.EIDOMessagingDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, detail);

			Assert(docContainer.ShowContainerImportDORelease);
		}

		public void TestGeneral()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			DocAgencyContainer docContainer = DocAgencyContainer.New(container, Factory);
			container.JC_RH_NKContainerCommodityCode = Core.Constants.CargoTypes.Frozen;

			Assert(!docContainer.General);

			container.JC_RH_NKContainerCommodityCode = Core.Constants.CargoTypes.General;

			Assert(docContainer.General);
		}

		public void TestHazardous()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			DocAgencyContainer docContainer = DocAgencyContainer.New(container, Factory);

			container.JC_RH_NKContainerCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals(true, docContainer.Hazardous);

			container.JC_RH_NKContainerCommodityCode = Core.Constants.CargoTypes.Frozen;
			AssertEquals(false, docContainer.Hazardous);

			container.JC_RH_NKContainerCommodityCode = "";
			AssertEquals(false, docContainer.Hazardous);

			packLine.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals(true, docContainer.Hazardous);

			packLine.JL_RH_NKCommodityCode = "";
			AssertEquals(false, docContainer.Hazardous);

			packLine.UNDGs.AddNew();
			AssertEquals(true, docContainer.Hazardous);
		}

		public void TestReefer()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			RefContainer refContainer = RefContainer.New(Factory);
			container.JC_RC = refContainer.PK;
			DocAgencyContainer docContainer = DocAgencyContainer.New(container, Factory);

			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.Bolster;
			Assert(!docContainer.Reefer);

			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			Assert(docContainer.Reefer);
		}

		public void TestPrintTACImage()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container2 = shipment.BookedContainers.AddNew();
			DocAgencyContainer docContainer1 = DocAgencyContainer.New(container1, Factory);
			DocAgencyContainer docContainer2 = DocAgencyContainer.New(container2, Factory);

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			OrgCompanyData companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;

			shipment.JS_OH_DeliveryAgent = principal.PK;

			Factory.Save();

			Assert(!docContainer1.PrintTACImage);
			Assert(!docContainer2.PrintTACImage);

			DeliveryOrderCollection collection = new DeliveryOrderCollection();
			DeliveryOrder element = collection.AddNew();
			element.PrincipalPK = principal.PK;
			element.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			element.Image = new Bitmap(10, 10);
			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Assert(docContainer1.PrintTACImage);
			Assert(!docContainer2.PrintTACImage);

			element.PrintParameter = DeliveryOrder.PrintConstants.Code.PCT;
			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			Assert(docContainer1.PrintTACImage);
			Assert(docContainer2.PrintTACImage);
		}

		public void TestTotalAllocatedShipmentPacksWeightAndVolume()
		{
			AgencyShipmentPackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine1.JL_ActualVolume = 1;
			packLine1.JL_ActualVolumeUQ = "M3";
			packLine1.JL_PackageCount = 11;
			packLine1.JL_ActualWeight = 111;
			packLine1.JL_JC = Container.PK;

			AgencyShipmentPackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine2.JL_ActualVolume = 2;
			packLine2.JL_ActualVolumeUQ = "CF";
			packLine2.JL_PackageCount = 22;
			packLine2.JL_ActualWeight = 222;

			AssertEquals(Core.Constants.PkgUnit.Pallet, DocWrapper.TotalAllocatedShipmentPackagesPackType);
			AssertEquals(1m, DocWrapper.TotalAllocatedShipmentVolume);
			AssertEquals("M3", DocWrapper.TotalAllocatedShipmentVolumeUQ);
			AssertEquals(11, DocWrapper.TotalAllocatedShipmentPackages);
			AssertEquals(111m, DocWrapper.TotalAllocatedShipmentWeight);

			AgencyShipmentPackLine packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine3.JL_ActualVolume = 30;
			packLine3.JL_ActualVolumeUQ = "CF";
			packLine3.JL_PackageCount = 33;
			packLine3.JL_ActualWeight = 333;
			packLine3.JL_JC = Container.PK;

			AssertEquals(Core.Constants.PkgUnit.Package, DocWrapper.TotalAllocatedShipmentPackagesPackType);
			AssertEquals(1.849505398m, DocWrapper.TotalAllocatedShipmentVolume);
			AssertEquals("M3", DocWrapper.TotalAllocatedShipmentVolumeUQ);
			AssertEquals(44, DocWrapper.TotalAllocatedShipmentPackages);
			AssertEquals(444m, DocWrapper.TotalAllocatedShipmentWeight);
		}

		public void TestTotalVolumeAndUnit()
		{
			var pack1 = Shipment.OuterPackLines.AddNew();
			pack1.SetContainer(Container.PK);

			var pack2 = Shipment.OuterPackLines.AddNew();
			pack2.SetContainer(Container.PK);

			pack1.JL_ActualVolume = 2;
			pack1.JL_ActualVolumeUQ = "M3";

			pack2.JL_ActualVolume = 3;
			pack2.JL_ActualVolumeUQ = "M3";

			AssertEquals(5M, DocWrapper.TotalVolume);
			AssertEquals("M3", DocWrapper.TotalVolumeUnit);

			pack2.JL_ActualVolumeUQ = "L";

			AssertEquals(2.003M, DocWrapper.TotalVolume);
			AssertEquals("M3", DocWrapper.TotalVolumeUnit);

			pack1.JL_ActualVolumeUQ = "CF";
			pack2.JL_ActualVolumeUQ = "CF";

			AssertEquals(5M, DocWrapper.TotalVolume);
			AssertEquals("CF", DocWrapper.TotalVolumeUnit);
		}

		#region Implementation

		#region Shipment

		AgencyShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<AgencyShipment>()); }
		}
		AgencyShipment shipment;

		#endregion

		#region Container

		AgencyShipmentContainer Container
		{
			get
			{
				if (container == null || container.IsDeleted)
				{
					container = Shipment.BookedContainers.AddNew();
				}

				return container;
			}
		}
		AgencyShipmentContainer container;

		#endregion

		#region DocWrapper

		DocAgencyContainer DocWrapper
		{
			get
			{
				DocAgencyContainer result = DocAgencyContainer.New(Container, Factory);
				result.SetDocumentDirectionForTesting(docDirection.ToString());
				return result;
			}
		}

		DocumentDirection docDirection;

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();

			return new DocumentWrapper[]
			{
				DocAgencyContainer.New(container, Factory)
			};
		}

		#endregion
	}
}
