using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	[TestedType(typeof(DocPackUnpackContainerRego))]
	internal class DocPackUnpackContainerRegoBaseTest : DocContainerBaseTest
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocPackUnpackContainerRego.New(PackUnpackContainer, Factory),
				DocPackUnpackContainerRego.New(Factory, PackUnpackContainer.PK)
			};
		}

		#region Overrides

		#region Wrapper Fields

		public override void TestJourneyOnePickUpAddress()
		{
			AssertNull("JourneyOnePickUpAddress", PackUnpackWrapper.JourneyOnePickUpAddress);

			AddContainerToLoadListConsolAndShipment();

			var departureAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var arrivalAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, departureAddress.PK));

			LoadList.JK_OA_ArrivalCTOAddress = arrivalAddress.PK;
			PackUnpackContainer.JC_JK = LoadList.PK;
			PackUnpackContainer.JC_OA_DepartureContainerYardAddress = departureAddress.PK;

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertNotNull("JourneyOnePickUpAddress", PackUnpackWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", departureAddress.OA_Code, PackUnpackWrapper.JourneyOnePickUpAddress.Code);

			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertNotNull("JourneyOnePickUpAddress", PackUnpackWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", arrivalAddress.OA_Code, PackUnpackWrapper.JourneyOnePickUpAddress.Code);
		}

		public override void TestJourneyOneDeliverToAddressForExport()
		{
			AddContainerToLoadListConsolAndShipment();
			OrgAddress address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			ShipReceival.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyOneDeliverToAddressForExport", PackUnpackWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport", PackUnpackWrapper.CurrentBranch.Organisation.DeliverAddress.Address1, PackUnpackWrapper.JourneyOneDeliverToAddressForExport.Address1);
			AssertEquals("JourneyOneDeliverToAddressForExport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyOneDeliverToAddressForExport.GetType());

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			PackUnpackContainer.OriginConfirm.PickupFrom.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyOneDeliverToAddressForExport", PackUnpackWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport", PackUnpackWrapper.CurrentBranch.Organisation.DeliverAddress.Address1, PackUnpackWrapper.JourneyOneDeliverToAddressForExport.Address1);
			AssertEquals("JourneyOneDeliverToAddressForExport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyOneDeliverToAddressForExport.GetType());
		}

		public override void TestJourneyOneDeliverToAddressForImport()
		{
			AddContainerToLoadListConsolAndShipment();
			OrgAddress address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			ShipReceival.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyOneDeliverToAddressForImport", PackUnpackWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport", PackUnpackWrapper.CurrentBranch.Organisation.DeliverAddress.Address1, PackUnpackWrapper.JourneyOneDeliverToAddressForImport.Address1);
			AssertEquals("JourneyOneDeliverToAddressForImport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyOneDeliverToAddressForImport.GetType());

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			PackUnpackContainer.DestinationConfirm.DeliverTo.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyOneDeliverToAddressForImport", PackUnpackWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport", PackUnpackWrapper.CurrentBranch.Organisation.DeliverAddress.Address1, PackUnpackWrapper.JourneyOneDeliverToAddressForImport.Address1);
			AssertEquals("JourneyOneDeliverToAddressForImport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyOneDeliverToAddressForImport.GetType());
		}

		public override void TestJourneyTwoPickUpAddressForExport()
		{
			AddContainerToLoadListConsolAndShipment();
			OrgAddress address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			ShipReceival.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyTwoPickUpAddressForExport", PackUnpackWrapper.JourneyTwoPickUpAddressForExport);
			AssertEquals("JourneyTwoPickUpAddressForExport", PackUnpackWrapper.CurrentBranch.Organisation.PickUpAddress.Address1, PackUnpackWrapper.JourneyTwoPickUpAddressForExport.Address1);
			AssertEquals("JourneyTwoPickUpAddressForExport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyTwoPickUpAddressForExport.GetType());

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			PackUnpackContainer.OriginConfirm.PickupFrom.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyTwoPickUpAddressForExport", PackUnpackWrapper.JourneyTwoPickUpAddressForExport);
			AssertEquals("JourneyTwoPickUpAddressForExport", PackUnpackWrapper.CurrentBranch.Organisation.PickUpAddress.Address1, PackUnpackWrapper.JourneyTwoPickUpAddressForExport.Address1);
			AssertEquals("JourneyTwoPickUpAddressForExport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyTwoPickUpAddressForExport.GetType());
		}

		public override void TestJourneyTwoPickUpAddressForImport()
		{
			AddContainerToLoadListConsolAndShipment();
			OrgAddress address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			ShipReceival.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyTwoPickUpAddressForImport", PackUnpackWrapper.JourneyTwoPickUpAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport", PackUnpackWrapper.CurrentBranch.Organisation.PickUpAddress.Address1, PackUnpackWrapper.JourneyTwoPickUpAddressForImport.Address1);
			AssertEquals("JourneyTwoPickUpAddressForImport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyTwoPickUpAddressForImport.GetType());

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			PackUnpackContainer.DestinationConfirm.DeliverTo.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyTwoPickUpAddressForImport", PackUnpackWrapper.JourneyTwoPickUpAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport", PackUnpackWrapper.CurrentBranch.Organisation.PickUpAddress.Address1, PackUnpackWrapper.JourneyTwoPickUpAddressForImport.Address1);
			AssertEquals("JourneyTwoPickUpAddressForImport is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyTwoPickUpAddressForImport.GetType());
		}

		public override void TestJourneyTwoDeliverToAddress()
		{
			AssertNull("JourneyTwoDeliverToAddress", PackUnpackWrapper.JourneyTwoDeliverToAddress);

			AddContainerToLoadListConsolAndShipment();

			var departureAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var arrivalAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, departureAddress.PK));

			LoadList.JK_OA_DepartureCTOAddress = departureAddress.PK;
			PackUnpackContainer.JC_JK = LoadList.PK;
			PackUnpackContainer.JC_OA_ArrivalContainerYardAddress = arrivalAddress.PK;

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertNotNull("JourneyTwoDeliverToAddress", PackUnpackWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", departureAddress.OA_Code, PackUnpackWrapper.JourneyTwoDeliverToAddress.Code);

			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertNotNull("JourneyTwoDeliverToAddress", PackUnpackWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), PackUnpackWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", arrivalAddress.OA_Code, PackUnpackWrapper.JourneyTwoDeliverToAddress.Code);
		}

		public void TestPortOfLoading()
		{
			AssertNull("PortOfLoading", PackUnpackWrapper.PortOfLoading);

			CreateSailing();

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Origin.JA_RL_NKPortOfLoading = uNLOCO.RL_Code;
			AssertNotNull("PortOfLoading", PackUnpackWrapper.PortOfLoading);
			AssertEquals("PortOfLoading is of type DocUNLOCO", typeof(DocUNLOCO), PackUnpackWrapper.PortOfLoading.GetType());
		}

		public void TestPortOfDischarge()
		{
			AssertNull("PortOfDischarge", PackUnpackWrapper.PortOfDischarge);

			CreateSailing();

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Destination.JB_RL_NKPortOfDischarge = uNLOCO.RL_Code;
			AssertNotNull("PortOfDischarge", PackUnpackWrapper.PortOfDischarge);
			AssertEquals("PortOfDischarge is of type DocUNLOCO", typeof(DocUNLOCO), PackUnpackWrapper.PortOfDischarge.GetType());
		}

		#endregion

		#region ZString Fields

		public void TestCartageInstructions()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			AssertEquals("CartageInstructions", ZString.Empty, PackUnpackWrapper.CartageInstructions);

			FreightHelperClass.AddNote(PackUnpackContainer, pickupDesc, "Container Pickup Instructions");
			FreightHelperClass.AddNote(PackUnpackContainer, deliveryDesc, "Container Delivery Instructions");
			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CartageInstructions", "Container Delivery Instructions", PackUnpackWrapper.CartageInstructions);
			AssertEquals("FullCartageInstructions", "Container Delivery Instructions", PackUnpackWrapper.FullCartageInstructions);

			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CartageInstructions", "Container Pickup Instructions", PackUnpackWrapper.CartageInstructions);
			AssertEquals("FullCartageInstructions", "Container Pickup Instructions", PackUnpackWrapper.FullCartageInstructions);

			AddContainerToLoadListConsolAndShipment();
			PackUnpackContainer.JC_JK = LoadList.PK;

			FreightHelperClass.AddNote(LoadList, pickupDesc, "LoadList Pickup Instructions");
			FreightHelperClass.AddNote(LoadList, deliveryDesc, "LoadList Delivery Instructions");
			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CartageInstructions", "LoadList Delivery Instructions", PackUnpackWrapper.CartageInstructions);
			AssertEquals("FullCartageInstructions", "LoadList Delivery Instructions", PackUnpackWrapper.FullCartageInstructions);

			PackUnpackWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CartageInstructions", "LoadList Pickup Instructions", PackUnpackWrapper.CartageInstructions);
			AssertEquals("FullCartageInstructions", "LoadList Pickup Instructions", PackUnpackWrapper.FullCartageInstructions);
		}

		public void TestFullHandlingInstructions()
		{
			AssertEquals("HandlingInstructions", ZString.Empty, PackUnpackWrapper.CartageInstructions);

			var note = PackUnpackContainer.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_Table = PackUnpackContainer.TableName;
			note.ST_ParentID = PackUnpackContainer.PK;
			note.ST_NoteDataAsText = "Handling Instruction from Container";

			AssertEquals("HandlingInstructions", "Handling Instruction from Container", PackUnpackWrapper.FullHandlingInstructions);

			AddContainerToLoadListConsolAndShipment();
			PackUnpackContainer.JC_JK = LoadList.PK;

			var handlingInstructions = LoadList.Notes.AddNew();
			handlingInstructions.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			handlingInstructions.ST_Table = LoadList.TableName;
			handlingInstructions.ST_ParentID = LoadList.PK;
			handlingInstructions.ST_NoteDataAsText = "Handling Instruction\nLine Two";

			AssertEquals("HandlingInstructions", handlingInstructions.ST_NoteDataAsText, PackUnpackWrapper.FullHandlingInstructions);
		}

		public void TestVoyage()
		{
			CreateSailing();
			ZString voyageString = new ZString("Voyage");
			Voyage.JV_VoyageFlight = voyageString;
			AssertEquals("Voyage", voyageString, PackUnpackWrapper.Voyage);
		}

		#endregion

		#region IDocIMO Members Override

		public void TestIMOShippersRef()
		{
			AssertEquals("", PackUnpackWrapper.IMOShippersRef);
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.Containers.Add(PackUnpackContainer);
			consol.JK_AgentsReference = "AGENTSREF84848";
			consol.JK_BookingReference = "BOOK384848";
			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			AssertEquals("AGENTSREF84848", PackUnpackWrapper.IMOShippersRef);
		}

		public void TestIMOForwardersRef()
		{
			AssertEquals("This should be empty for CFS", "", PackUnpackWrapper.IMOForwardersRef);
		}

		#endregion

		#endregion

		#region Implementation

		CFSContainer PackUnpackContainer;
		DocPackUnpackContainerRego PackUnpackWrapper;
		JobSailing Sailing;
		JobVoyage Voyage;
		VoyageOrigin Origin;
		VoyageDestination Destination;
		CFSLoadListConsol LoadList;
		CFSShipment ShipReceival;
		CFSPackLine PackLine;

		protected override void SetUp()
		{
			PackUnpackContainer = Factory.New<CFSContainer>();
			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);

			base.SetUp();
		}

		void AddContainerToLoadListConsolAndShipment()
		{
			LoadList = Factory.New<CFSLoadListConsol>();
			LoadList.Containers.Add(PackUnpackContainer);
			ShipReceival = LoadList.Shipments.AddNew();

			PackLine = ShipReceival.OuterPackLines.AddNew();
			PackLine.SetContainer(LoadList, PackUnpackContainer);
			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
		}

		void CreateSailing()
		{
			Sailing = Factory.New<JobSailing>();
			Voyage = Factory.New<JobVoyage>();
			Origin = Factory.New<VoyageOrigin>();
			Destination = Factory.New<VoyageDestination>();

			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;
			Origin.JA_JV = Voyage.PK;
			Destination.JB_JV = Voyage.PK;
			PackUnpackContainer.JC_JX = Sailing.PK;
		}

		#endregion
	}
}
