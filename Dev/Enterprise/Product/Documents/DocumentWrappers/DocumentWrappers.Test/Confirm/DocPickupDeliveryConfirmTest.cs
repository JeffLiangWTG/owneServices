using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocPickupDeliveryConfirm))]
	sealed class DocPickupDeliveryConfirmTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { ConfirmWrapper };
		}

		public void TestUniqueID_Saving()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S12345678";
			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 5;
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			Factory.Save();
			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("S12345678/A", doc.ContainerLegReference);
		}

		public void TestIsPickupConfirmation()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();

			foreach (System.Reflection.FieldInfo info in typeof(Constants.PickupDeliveryConfirmTypes).GetFields())
			{
				if (info.FieldType == typeof(string))
				{
					string confirmType = new ZString(info.GetValue(null));
					bool expectedResult;
					switch (confirmType)
					{
						case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
						case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
						case Constants.PickupDeliveryConfirmTypes.OriginPickup:
							expectedResult = true;
							break;
						case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
						case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
						case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
							expectedResult = false;
							break;
						default:
							throw new NotSupportedException("Confirm Type note supported. Please add.");
					}

					confirm.EU_PickupDeliveryType = confirmType;
					DocPickupDeliveryConfirm confirmWrapper = DocPickupDeliveryConfirm.New(confirm, Factory);
					AssertEquals(expectedResult, confirmWrapper.IsPickupConfirmation);
				}
			}
		}

		public void TestIsDeliveryConfirmation()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();

			foreach (System.Reflection.FieldInfo info in typeof(Constants.PickupDeliveryConfirmTypes).GetFields())
			{
				if (info.FieldType == typeof(string))
				{
					string confirmType = new ZString(info.GetValue(null));
					bool expectedResult;
					switch (confirmType)
					{
						case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
						case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
						case Constants.PickupDeliveryConfirmTypes.OriginPickup:
							expectedResult = false;
							break;
						case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
						case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
						case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
							expectedResult = true;
							break;
						default:
							throw new NotSupportedException("Confirm Type note supported. Please add.");
					}

					confirm.EU_PickupDeliveryType = confirmType;
					DocPickupDeliveryConfirm confirmWrapper = DocPickupDeliveryConfirm.New(confirm, Factory);
					AssertEquals(expectedResult, confirmWrapper.IsDeliveryConfirmation);
				}
			}
		}

		public void TestDimensions()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine2.JL_PackageCount = 20;

			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot1 = packLine1.ConfirmDivots[0];
			CommonConfirmDivot divot2 = packLine2.ConfirmDivots[0];
			divot1.J8_PackagesDelivered = 6;
			divot2.J8_PackagesDelivered = 13;

			DocPickupDeliveryConfirm confirmWrapper = DocPickupDeliveryConfirm.New(confirm, Factory);
			Assert(!confirmWrapper.HasDimensions);
			AssertContains("No Dimensions Specified X 6", confirmWrapper.Dimensions);
			AssertContains("No Dimensions Specified X 13", confirmWrapper.Dimensions);

			packLine1.JL_Length = 1;
			packLine1.JL_Width = 2;
			packLine1.JL_Height = 3;
			confirmWrapper = DocPickupDeliveryConfirm.New(confirm, Factory);
			Assert(confirmWrapper.HasDimensions);
			AssertContains("(L): 1  (W): 2  (H): 3 M X 6", confirmWrapper.Dimensions);
			AssertContains("No Dimensions Specified X 13", confirmWrapper.Dimensions);

			packLine2.JL_Length = 7;
			packLine2.JL_Width = 3;
			packLine2.JL_Height = 6;
			confirmWrapper = DocPickupDeliveryConfirm.New(confirm, Factory);
			Assert(confirmWrapper.HasDimensions);
			AssertContains("(L): 1  (W): 2  (H): 3 M X 6", confirmWrapper.Dimensions);
			AssertContains("(L): 7  (W): 3  (H): 6 M X 13", confirmWrapper.Dimensions);
		}

		public void TestWeightDelivered()
		{
			CommonPickupDeliveryConfirm confirm = null;
			CommonConfirmDivot divot = null;
			PackLine line = null;

			GetConfirmDivotPackline(out confirm, out divot, out line);

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);

			divot.J8_DeliveryWeight = 10;
			divot = confirm.Divots.AddNew();
			divot.J8_DeliveryWeight = 20;

			AssertEquals((ZDecimal)30, doc.WeightDelivered);
		}

		public void TestContainerisedContainerJobNumberUnpackShed()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			var line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 1;
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerJobID = "Cont";
			container.JC_UnpackShed = "us";
			container.JC_ContainerNum = "cn";

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.Divots[0].J8_PackagesDelivered = 1;
			var doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("Cont", doc.ContainerJobNumber);
			AssertEquals("us", doc.UnpackShed);
			AssertEquals("CN", doc.ContainerNumber);

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			var line2 = shipment2.OuterPackLines.AddNew();
			line2.JL_PackageCount = 1;
			var consol2 = shipment2.Consols.AddNew();
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerJobID = "Cont";
			container2.JC_UnpackShed = "us";
			container2.JC_ContainerNum = "cn";

			var confirm2 = container2.DestinationConfirm;
			var doc2 = DocPickupDeliveryConfirm.New(confirm2, Factory);
			AssertEquals("Cont", doc2.ContainerJobNumber);
			AssertEquals("us", doc2.UnpackShed);
			AssertEquals("CN", doc2.ContainerNumber);
		}

		public void TestVolumeDelivered()
		{
			CommonPickupDeliveryConfirm confirm = null;
			CommonConfirmDivot divot = null;
			PackLine line = null;

			GetConfirmDivotPackline(out confirm, out divot, out line);

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);

			divot.J8_DeliveryVolume = 10;
			divot = confirm.Divots.AddNew();
			divot.J8_DeliveryVolume = 20;

			AssertEquals((ZDecimal)30, doc.VolumeDelivered);
		}

		public void TestPackagesDelivered()
		{
			CommonPickupDeliveryConfirm confirm = null;
			CommonConfirmDivot divot = null;
			PackLine line = null;

			GetConfirmDivotPackline(out confirm, out divot, out line);

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);

			divot.J8_PackagesDelivered = 10;
			divot = confirm.Divots.AddNew();
			divot.J8_PackagesDelivered = 20;

			AssertEquals("30 PKG", doc.PackageCountDelivered);
		}

		public void TestDescription()
		{
			CommonPickupDeliveryConfirm confirm = null;
			CommonConfirmDivot divot = null;
			PackLine line = null;

			GetConfirmDivotPackline(out confirm, out divot, out line);

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);

			line.Shipment.JS_GoodsDescription = "NY09";
			AssertEquals("NY09", doc.Description);
		}

		public void TestConsignee()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocAddress address = shipment.ConsigneeDocumentaryAddress;
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "There is no spoon";
			address.OrganisationPK = org.PK;

			PackLine line = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("There is no spoon", doc.Consignee.Name);

			address.E2_AddressOverride = true;
			address.E2_CompanyName = "Spoon boy";
			doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("Spoon boy", doc.Consignee.Name);
		}

		public void TestConsignor()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocAddress address = shipment.ConsignorDocumentaryAddress;
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "There is no spoon";
			address.OrganisationPK = org.PK;

			PackLine line = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("There is no spoon", doc.Consignor.Name);

			address.E2_AddressOverride = true;
			address.E2_CompanyName = "Spoon boy";
			doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("Spoon boy", doc.Consignor.Name);
		}

		public void TestConfirmAddress()
		{
			CommonPickupDeliveryConfirm confirm = null;
			CommonConfirmDivot divot = null;
			PackLine line = null;

			GetConfirmDivotPackline(out confirm, out divot, out line);

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);

			AssertEquals(confirm.ConfirmAddress, doc.ConfirmAddress.WrappedObject);
		}

		public void GetConfirmDivotPackline(out CommonPickupDeliveryConfirm confirm, out CommonConfirmDivot divot, out PackLine line)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			line = shipment.OuterPackLines.AddNew();
			confirm = shipment.DeliveryConfirms.AddNew();
			divot = confirm.Divots[0];
		}

		public void TestContainerEstimatedTimes()
		{
			ZDateTime now = ZDateTime.Now;
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_DepartureEstimatedPickup = now;
			container.JC_ArrivalEstimatedDelivery = now;
			DocPickupDeliveryConfirm originConfirmWrapper = DocPickupDeliveryConfirm.New(container.OriginConfirm, Factory);
			DocPickupDeliveryConfirm destinationConfirmWrapper = DocPickupDeliveryConfirm.New(container.DestinationConfirm, Factory);

			AssertEquals(container.JC_DepartureEstimatedPickup, originConfirmWrapper.PlannedPickupTime);
			AssertEquals(container.JC_ArrivalEstimatedDelivery, destinationConfirmWrapper.EstimatedDeliveryTime);
		}

		#region IDoc Cartage Advice

		#region Headings

		public void TestJourneyOnePickUpHeading()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.PickupConfirms.AddNew();

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("JourneyOnePickUpHeading: null", "PICKUP", doc.JourneyOnePickUpHeading);

			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("PICKUP - EST. {0}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString()), doc.JourneyOnePickUpHeading);

			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("PICKUP - EST. {0} - REQ. BY {1}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString(), confirm.EU_RequestedPickupDeliveryTime.ToLongTimeString()), doc.JourneyOnePickUpHeading);

			confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("A Delivery Confirm shows no pickup times atm", "PICKUP", doc.JourneyOnePickUpHeading);
		}

		public void TestJourneyOneDeliverToHeading()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("JourneyOneDeliverToHeading: null", "DELIVER TO", doc.JourneyOneDeliverToHeading);

			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("DELIVER TO - EST. {0}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString()), doc.JourneyOneDeliverToHeading);

			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("DELIVER TO - EST. {0} - REQ. BY {1}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString(), confirm.EU_RequestedPickupDeliveryTime.ToLongTimeString()), doc.JourneyOneDeliverToHeading);

			confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			AssertEquals("A Pickup Confirm shows no delivery times atm", "DELIVER TO", doc.JourneyOneDeliverToHeading);
		}

		public void TestJourneyTwoPickUpHeading()
		{
			AssertEquals("JourneyTwoPickUpHeading is empty", "", ConfirmWrapper.JourneyTwoPickUpHeading);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.PickupConfirms.AddNew();

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			doc.ShowContainerYardAsJourneyTwo = false;
			AssertEquals("JourneyOnePickUpHeading: null", "PICKUP", doc.JourneyTwoPickUpHeading);

			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("PICKUP - EST. {0}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString()), doc.JourneyTwoPickUpHeading);

			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("PICKUP - EST. {0} - REQ. BY {1}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString(), confirm.EU_RequestedPickupDeliveryTime.ToLongTimeString()), doc.JourneyTwoPickUpHeading);

			confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			doc.ShowContainerYardAsJourneyTwo = false;
			AssertEquals("A Delivery Confirm shows no pickup times atm", "PICKUP", doc.JourneyTwoPickUpHeading);
		}

		public void TestJourneyTwoDeliverToHeading()
		{
			AssertEquals("JourneyTwoDeliverToHeading is empty", "", ConfirmWrapper.JourneyTwoDeliverToHeading);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();

			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			doc.ShowContainerYardAsJourneyTwo = false;
			AssertEquals("JourneyOneDeliverToHeading: null", "DELIVER TO", doc.JourneyTwoDeliverToHeading);

			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("DELIVER TO - EST. {0}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString()), doc.JourneyTwoDeliverToHeading);

			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			AssertEquals("JourneyOnePickUpHeading: Empty container with date", string.Format("DELIVER TO - EST. {0} - REQ. BY {1}", confirm.EU_PlannedPickupDeliveryTime.ToLongTimeString(), confirm.EU_RequestedPickupDeliveryTime.ToLongTimeString()), doc.JourneyTwoDeliverToHeading);

			confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now.AddHours(1);
			doc = DocPickupDeliveryConfirm.New(confirm, Factory);
			doc.ShowContainerYardAsJourneyTwo = false;
			AssertEquals("A Pickup Confirm shows no delivery times atm", "DELIVER TO", doc.JourneyTwoDeliverToHeading);
		}

		#endregion

		#region Addresses

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

			PackLine fclLine = fclShipment.OuterPackLines.AddNew();
			PackLine lclLine = lclShipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm containerisedPickupConfirm = fclContainer.OriginConfirm;
			CommonPickupDeliveryConfirm containerisedDeliveryConfirm = fclContainer.DestinationConfirm;
			CommonPickupDeliveryConfirm loosePickupConfirm = lclShipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm looseDeliveryConfirm = lclShipment.DeliveryConfirms.AddNew();

			DocPickupDeliveryConfirm docContainerisedPickupConfirm = DocPickupDeliveryConfirm.New(containerisedPickupConfirm, Factory);
			DocPickupDeliveryConfirm docContainerisedDeliveryConfirm = DocPickupDeliveryConfirm.New(containerisedDeliveryConfirm, Factory);
			DocPickupDeliveryConfirm docLoosePickupConfirm = DocPickupDeliveryConfirm.New(loosePickupConfirm, Factory);
			DocPickupDeliveryConfirm docLooseDeliveryConfirm = DocPickupDeliveryConfirm.New(looseDeliveryConfirm, Factory);

			AssertEquals(cnrOrg.MainAddress.OA_Address1, docContainerisedPickupConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(originCTOOrg.MainAddress.OA_Address1, docContainerisedPickupConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(originCYDOrg.MainAddress.OA_Address1, docContainerisedPickupConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(null, docContainerisedPickupConfirm.JourneyTwoDeliverToAddress);

			AssertEquals(destinationCTOOrg.MainAddress.OA_Address1, docContainerisedDeliveryConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, docContainerisedDeliveryConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(destinationCYDOrg.MainAddress.OA_Address1, docContainerisedDeliveryConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(null, docContainerisedDeliveryConfirm.JourneyTwoDeliverToAddress);

			AssertEquals(cnrOrg.MainAddress.OA_Address1, docLoosePickupConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(originCFSOrg.MainAddress.OA_Address1, docLoosePickupConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(null, docLoosePickupConfirm.JourneyTwoPickUpAddress);
			AssertEquals(null, docLoosePickupConfirm.JourneyTwoDeliverToAddress);

			AssertEquals(destinationCFSOrg.MainAddress.OA_Address1, docLooseDeliveryConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, docLooseDeliveryConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(null, docLooseDeliveryConfirm.JourneyTwoPickUpAddress);
			AssertEquals(null, docLooseDeliveryConfirm.JourneyTwoDeliverToAddress);

			docContainerisedPickupConfirm.ShowContainerYardAsJourneyTwo = false;
			docContainerisedDeliveryConfirm.ShowContainerYardAsJourneyTwo = false;
			docLoosePickupConfirm.ShowContainerYardAsJourneyTwo = false;
			docLooseDeliveryConfirm.ShowContainerYardAsJourneyTwo = false;

			AssertEquals(cnrOrg.MainAddress.OA_Address1, docContainerisedPickupConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(originCTOOrg.MainAddress.OA_Address1, docContainerisedPickupConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(cnrOrg.MainAddress.OA_Address1, docContainerisedPickupConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(originCTOOrg.MainAddress.OA_Address1, docContainerisedPickupConfirm.JourneyTwoDeliverToAddress.Address1);

			AssertEquals(destinationCTOOrg.MainAddress.OA_Address1, docContainerisedDeliveryConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, docContainerisedDeliveryConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(destinationCTOOrg.MainAddress.OA_Address1, docContainerisedDeliveryConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, docContainerisedDeliveryConfirm.JourneyTwoDeliverToAddress.Address1);

			AssertEquals(cnrOrg.MainAddress.OA_Address1, docLoosePickupConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(originCFSOrg.MainAddress.OA_Address1, docLoosePickupConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(cnrOrg.MainAddress.OA_Address1, docLoosePickupConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(originCFSOrg.MainAddress.OA_Address1, docLoosePickupConfirm.JourneyTwoDeliverToAddress.Address1);

			AssertEquals(destinationCFSOrg.MainAddress.OA_Address1, docLooseDeliveryConfirm.JourneyOnePickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, docLooseDeliveryConfirm.JourneyOneDeliverToAddress.Address1);
			AssertEquals(destinationCFSOrg.MainAddress.OA_Address1, docLooseDeliveryConfirm.JourneyTwoPickUpAddress.Address1);
			AssertEquals(cneOrg.MainAddress.OA_Address1, docLooseDeliveryConfirm.JourneyTwoDeliverToAddress.Address1);
		}

		#endregion

		#region Dates

		public void TestIDocCartageAdviceDates()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();

			shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2011, 4, 18);
			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2011, 4, 20);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2011, 4, 22);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2011, 4, 25);

			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_ETD = new ZDateTime(2011, 2, 1);
			transport1.JW_ETA = new ZDateTime(2011, 2, 15);

			Transport transport2 = shipment.Transports.AddNew();
			transport2.JW_ETD = new ZDateTime(2011, 1, 1);
			transport2.JW_ETA = new ZDateTime(2011, 1, 15);
			transport2.JW_TerminalCutOff = new ZDateTime(2011, 1, 18);
			transport2.JW_DepotCutOff = new ZDateTime(2011, 1, 20);
			transport2.JW_TerminalReceivalCommences = new ZDateTime(2011, 1, 22);
			transport2.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 25);

			Transport transport3 = shipment.Transports.AddNew();
			transport3.JW_ETD = new ZDateTime(2011, 4, 1);
			transport3.JW_ETA = new ZDateTime(2011, 4, 15);

			Transport transport4 = shipment.Transports.AddNew();
			transport4.JW_ETD = new ZDateTime(2011, 3, 1);
			transport4.JW_ETA = new ZDateTime(2011, 3, 15);

			DocPickupDeliveryConfirm wrapper = DocPickupDeliveryConfirm.New(confirm, shipment, Factory);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 18), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 22), wrapper.CartageStorageCommenceDate);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 20), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 20), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 25), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 25), wrapper.CartageStorageCommenceDate);
		}

		#endregion

		#endregion

		public void TestLegNotes()
		{
			AssertEquals("LegNotes", "", ConfirmWrapper.LegNotes);

			Confirm.EU_PickupDeliveryInstruction = "Leg Notes";
			AssertEquals("LegNotes", "Leg Notes", ConfirmWrapper.LegNotes);
		}

		public void TestIdentityDocument()
		{
			AssertEquals("IdentityDocument", "", ConfirmWrapper.IdentityDocument);

			Confirm.EU_DriversLicence = "DrivLic";
			AssertEquals("IdentityDocument", "DrivLic", ConfirmWrapper.IdentityDocument);
		}

		public void TestPackTypeDescription()
		{
			CommonPickupDeliveryConfirm confirm = null;
			CommonConfirmDivot divot = null;
			PackLine line = null;
			GetConfirmDivotPackline(out confirm, out divot, out line);
			DocPickupDeliveryConfirm doc = DocPickupDeliveryConfirm.New(confirm, Factory);

			line.JL_F3_NKPackType = Constants.PkgUnit.Package;
			AssertEquals("Package", doc.PackTypeDescription);

			line.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals("Pallet", doc.PackTypeDescription);
		}

		#region Implementation

		CommonPickupDeliveryConfirm Confirm;
		DocPickupDeliveryConfirm ConfirmWrapper;

		protected override void SetUp()
		{
			Confirm = Factory.New<CommonPickupDeliveryConfirm>();
			ConfirmWrapper = DocPickupDeliveryConfirm.New(Confirm, Factory);
			AssertNotNull("Created wrapper should not be null", ConfirmWrapper);

			base.SetUp();
		}

		#endregion
	}
}
