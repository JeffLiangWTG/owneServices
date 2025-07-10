using System;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ECU.ConsolExport
{
	sealed class ECUFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestIsValidForExport()
		{
			notification.Clear();
			var recvAgentError = "Receiving Agent is not Specified";
			var consol = new Xsd.Consol();
			var consolIdentifier = consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "11111111";
			consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			dataConverter = new ECUFlatFileConverter(notification, Factory, dummyFileID);
			rows = dataConverter.InternalMapExport(consol);
			var errorMessage = GetErrorMessage();
			AssertEquals("No DataRow return", 0, rows.Count);
			AssertEquals("Notification should have errors", true, notification.HasErrors);
			AssertContains("Should return Error Message:'" + recvAgentError + "'", recvAgentError, errorMessage);
			consol = GetPopulatedConsolXsd();
			notification.Clear();
			rows = dataConverter.InternalMapExport(consol);
			AssertEquals("DataRow return", true, rows.Count > 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 6, 20, 13, 06, 38)]
		public void TestMapExport()
		{
			var consol = GetPopulatedConsolXsd();
			notification.Clear();
			dataConverter = new ECUFlatFileConverter(notification, Factory, dummyFileID);
			var dataRows = dataConverter.InternalMapExport(consol);
			var actualOutput = new StringBuilder();
			var format = new ECUFlatFileFormat();
			foreach (FlatFileDataRow row in dataRows)
			{
				var line = format.ConvertToLine(row);
				actualOutput.AppendLine(line);
				if (line.Contains(Constants.Header.BMKN) || line.Contains(Constants.Header.BDSC))
				{
					AssertEquals("marks and numbers shouldn't container carriage feed '\r\n'", true, line.IndexOf("\r") == -1 && line.IndexOf("\n") == -1);
				}
			}

			AssertASCIIFileSameAsString(Path.Combine(BaseSourcePath, @"Enterprise\ClientExtensions\ECU\ZClientECU\ZClientECU\ConsolExport\TestFiles\00001234.ECU"), actualOutput.ToString());
		}

		ZString dummyFileID;
		NotificationBuffer notification;
		FlatFileDataRowCollection rows;
		ECUFlatFileConverter dataConverter;
		IDisposable userEmailOverride;
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			userEmailOverride = Env.CurrentUser.SetUserEmailAddressInTESTINGOnly("abc@abc.com");
			dummyFileID = "00001234";
			notification = new NotificationBuffer();
		}

		protected override void TearDown()
		{
			userEmailOverride.Dispose();
			base.TearDown();
		}

		Xsd.Consol GetPopulatedConsolXsd()
		{
			#region Add Organisations (Consignee, Consignor, Notify Party, Delivery Agent)
			var consignee = CreateOrganisation("BIGBIRDINC", "BigBird Incorporated Consignee", "Level 3", "1 Sesame St", "Sydney", "NSW", "33333333", "44444444");
			var consignor = CreateOrganisation("COOKIEINC", "CookieMan Incorporated Consignor", "2 Sesame St", "Los Angeles", "CA", "55555555", "66666666");
			var notifyParty = CreateOrganisation("BERTINC", "Bert Incorporated Notify Party", "3 Sesame St", "Melbourne", "VIC", "77777777", "88888888");
			var deliveryAgent = CreateOrganisation("ERNIEINC", "Ernie Incorporated Delivery Agent", "4 Sesame St", "Sydney", "NSW", "99999999", "00000000");
			var gTN = deliveryAgent.OrganisationDetails.RegistrationNumbers.AddNew();
			gTN.CountryOfRegistration = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gTN.Number = "ECUSENDAGENTCODE";
			gTN.NumberType = Xsd.RegistrationNumberTypes.GTN;
			var receivingAgent = CreateOrganisation("DINGO", "Dingo Company Receiving Agent", "Level 2", "141 Ambercombie St", "Sydney", "NSW", "11111111", "22222222");
			receivingAgent.OrganisationDetails.EDITransmissionDetails.Type = Xsd.OrganisationDetailEDITransmissionDetailsType.EMA;
			receivingAgent.OrganisationDetails.EDITransmissionDetails.Address = "Test@test.com";
			gTN = receivingAgent.OrganisationDetails.RegistrationNumbers.AddNew();
			gTN.CountryOfRegistration = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gTN.Number = "ECURECVAGENTCODE";
			gTN.NumberType = Xsd.RegistrationNumberTypes.GTN;
			var shippingLine = CreateOrganisation("SHREK", "SHREK Shipping Line", "Line 1", "5 Shrek Avenue", "Sydney", "NSW", "33333333", "11111111");
			#endregion
			#region Consol
			var consol = new Xsd.Consol();
			var consolIdentifier = consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "11111111";
			consolIdentifier = consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			consolIdentifier.Value = "C00001002";
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = new ZDateTime(2005, 07, 01);
			consol.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = new ZDateTime(2005, 07, 20);
			consol.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			consol.ConsolDetail.PaymentType = Xsd.PaymentType.PPD;
			consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consol.ConsolDetail.ReceivingAgent = receivingAgent;
			consol.ConsolDetail.Carrier = shippingLine;
			consol.ConsolDetail.SendingAgent = deliveryAgent;
			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.ETD = new ZDateTime(2005, 04, 01);
			sailing.ETA = new ZDateTime(2005, 05, 01);
			sailing.VesselName = "AMERICA STAR";
			sailing.VoyageNo = "Voyage123";
			consol.ConsolDetail.Item = sailing;
			#endregion
			#region Shipment 1
			var shipment = consol.Shipments.AddNew();
			var shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "22222222";
			shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;
			shipmentIdentifier.Value = "S00001110";
			shipment.ShipmentDetails.Consignor = consignor;
			shipment.ShipmentDetails.Consignee = consignee;
			shipment.ShipmentDetails.NotifyParty = new Xsd.ContactReference();
			shipment.ShipmentDetails.NotifyParty.Organisation = notifyParty;
			shipment.ShipmentDetails.NotifyParty.ContactSequenceRef = 1;
			shipment.ShipmentDetails.Deliver.DeliveryAgent = deliveryAgent;
			shipment.ShipmentDetails.PortOfOrigin = new Xsd.Movement();
			shipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = new ZDateTime(2005, 04, 01);
			shipment.ShipmentDetails.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			shipment.ShipmentDetails.PortofDestination = new Xsd.Movement();
			shipment.ShipmentDetails.PortofDestination.EstimatedDateTime = new ZDateTime(2005, 05, 01);
			shipment.ShipmentDetails.PortofDestination.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			shipment.ShipmentDetails.NoCopyBills = "2";
			shipment.ShipmentDetails.NoOriginalBills = "1";
			shipment.ShipmentDetails.FreightRate.CurrencyCode = "USD";
			shipment.ShipmentDetails.FreightRate.Value = 0.74m;
			shipment.ShipmentDetails.ShippedOnBoardDate = new ZDateTime(2005, 04, 01);
			shipment.ShipmentDetails.Incoterm = "FOB";
			shipment.ShipmentDetails.TotalOuterPacksQty.Value = 2;
			shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = Core.Constants.PkgUnit.Package;
			shipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit((ZInt)3, Core.Constants.Volume.CubicMetres);
			shipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit((ZInt)2, Core.Constants.Weight.Kilograms);
			shipment.ShipmentDetails.GoodsDescription = "SHIPMENTGOODSDESC1\r\nSHIPMENTGOODSDESC2";
			shipment.ShipmentDetails.MarksAndNumbers = "SHIPMENTMARKSANDNUMBERS\r\nSHIPMENTMARKSANDNUMBERS";
			var package1 = shipment.ShipmentDetails.Packages.AddNew();
			package1.Volume = Xsd.DimensionValue.FromAmountAndUnit((ZInt)3, Core.Constants.Volume.CubicMetres);
			package1.Weight = Xsd.DimensionValue.FromAmountAndUnit((ZInt)2, Core.Constants.Weight.Kilograms);
			package1.ContainerNumber = "ABCD1111";
			package1.GoodsDescription = "GOODSDESC1";
			package1.MarksAndNumbers = "MARKSANDNUMBERS1";
			package1.NumberOfPacks = 2;
			package1.PackType = Core.Constants.PkgUnit.Package;
			#endregion
			#region Shipment 2
			shipment = consol.Shipments.AddNew();
			shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "3333333";
			shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;
			shipmentIdentifier.Value = "S00001112";
			shipment.ShipmentDetails.Consignor = consignor;
			shipment.ShipmentDetails.Consignee = consignee;
			shipment.ShipmentDetails.NotifyParty = new Xsd.ContactReference();
			shipment.ShipmentDetails.NotifyParty.Organisation = notifyParty;
			shipment.ShipmentDetails.NotifyParty.ContactSequenceRef = 1;
			shipment.ShipmentDetails.Deliver.DeliveryAgent = deliveryAgent;
			shipment.ShipmentDetails.PortOfOrigin = new Xsd.Movement();
			shipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = new ZDateTime(2005, 04, 01);
			shipment.ShipmentDetails.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			shipment.ShipmentDetails.PortofDestination = new Xsd.Movement();
			shipment.ShipmentDetails.PortofDestination.EstimatedDateTime = new ZDateTime(2005, 05, 01);
			shipment.ShipmentDetails.PortofDestination.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			shipment.ShipmentDetails.NoCopyBills = "2";
			shipment.ShipmentDetails.NoOriginalBills = "1";
			shipment.ShipmentDetails.FreightRate.CurrencyCode = "USD";
			shipment.ShipmentDetails.FreightRate.Value = 0.74m;
			shipment.ShipmentDetails.ShippedOnBoardDate = new ZDateTime(2005, 04, 01);
			shipment.ShipmentDetails.Incoterm = "FOB";
			shipment.ShipmentDetails.TotalOuterPacksQty.Value = 2;
			shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = Core.Constants.PkgUnit.Package;
			shipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit((ZInt)3, Core.Constants.Volume.CubicMetres);
			shipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit((ZInt)2, Core.Constants.Weight.Kilograms);
			shipment.ShipmentDetails.GoodsDescription = "2NDSHIPMENTGOODSDESC";
			shipment.ShipmentDetails.MarksAndNumbers = "2NDSHIPMENTMARKSANDNUMBERS";
			package1 = shipment.ShipmentDetails.Packages.AddNew();
			package1.Volume = Xsd.DimensionValue.FromAmountAndUnit((ZInt)3, Core.Constants.Volume.CubicMetres);
			package1.Weight = Xsd.DimensionValue.FromAmountAndUnit((ZInt)2, Core.Constants.Weight.Kilograms);
			package1.ContainerNumber = "ABCD2222";
			package1.GoodsDescription = "GOODSDESC1";
			package1.MarksAndNumbers = "MARKSANDNUMBERS1";
			package1.NumberOfPacks = 2;
			package1.PackType = Core.Constants.PkgUnit.Package;
			#endregion
			var container1 = consol.ConsolDetail.Containers.AddNew();
			container1.ContainerNumber = "ABCD1111";
			container1.Seal = "SEAL1";
			container1.ContainerType.ContainerCode = "20GP";
			var container2 = consol.ConsolDetail.Containers.AddNew();
			container2.ContainerNumber = "ABCD2222";
			container2.Seal = "SEAL2";
			container2.ContainerType.ContainerCode = "40RE";
			return consol;
		}

		Xsd.Organisation CreateOrganisation(ZString eDICode, ZString orgName, ZString addressLine1, ZString cityOrSuburb, ZString stateOrProvince, ZString phoneNumber, ZString faxNumber)
		{
			return CreateOrganisation(eDICode, orgName, addressLine1, ZString.Empty, cityOrSuburb, stateOrProvince, phoneNumber, faxNumber);
		}

		Xsd.Organisation CreateOrganisation(ZString eDICode, ZString orgName, ZString addressLine1, ZString addressLine2, ZString cityOrSuburb, ZString stateOrProvince, ZString phoneNumber, ZString faxNumber)
		{
			var organisation = new Xsd.Organisation();
			organisation.EDICode = eDICode;
			organisation.OrganisationDetails.Name = orgName;
			var orginsationAddress = organisation.OrganisationDetails.Addresses.AddNew();
			orginsationAddress.AddressLine1 = addressLine1;
			orginsationAddress.AddressLine2 = addressLine2;
			orginsationAddress.CityOrSuburb = cityOrSuburb;
			orginsationAddress.StateOrProvince = stateOrProvince;
			var organisationPhone = orginsationAddress.TelephoneNumbers.AddNew();
			organisationPhone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			organisationPhone.Value = phoneNumber;
			var organisationFax = orginsationAddress.TelephoneNumbers.AddNew();
			organisationFax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			organisationFax.Value = faxNumber;
			return organisation;
		}

		string GetErrorMessage()
		{
			var errorsMsg = new StringBuilder();
			foreach (Notification eventNotification in notification.Events)
			{
				if (eventNotification is ErrorNotification)
				{
					errorsMsg.Append(eventNotification.Message);
					errorsMsg.Append("\n");
				}
			}

			return errorsMsg.ToString();
		}
	}
}
