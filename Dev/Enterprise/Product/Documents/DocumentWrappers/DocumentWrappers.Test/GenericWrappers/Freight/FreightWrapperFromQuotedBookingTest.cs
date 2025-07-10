using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromQuotedBooking))]
	sealed class FreightWrapperFromQuotedBookingTest : FreightWrapperTest
	{
		public void TestAdditionalTerms()
		{
			AssertEquals(string.Empty, wrapper.AdditionalTerms);

			booking.JS_AdditionalTerms = "Additional Terms";

			AssertEquals("Additional Terms", wrapper.AdditionalTerms);
		}

		public void TestDeliveryDueDate()
		{
			AssertEquals("wrapper.DeliveryDueDate", ZDateTime.Empty, wrapper.DeliveryDueDate);
			AssertEquals("wrapper.RevisedDeliveryDueDate", ZDateTimeOffset.Empty, wrapper.RevisedDeliveryDueDate);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			AssertEquals("TrackingBusinessObjectPK", booking.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestHouseBillForDirectBooking()
		{
			booking.JS_IsDirectBooking = false;
			booking.JS_HouseBill = "1111";

			AssertEquals("1111", wrapper.HouseBill);

			booking.JS_IsDirectBooking = true;
			booking.JS_HouseBill = "2222";
			AssertEquals(string.Empty, wrapper.HouseBill);
		}

		public void TestMasterBillForDirectBooking()
		{
			booking.JS_IsDirectBooking = false;
			booking.JS_HouseBill = "1111";

			AssertEquals(string.Empty, wrapper.MasterBill);

			booking.JS_IsDirectBooking = true;
			booking.JS_HouseBill = "2222";
			AssertEquals("2222", wrapper.MasterBill);
		}

		public void TestMasterBillHeadingForDirectBooking()
		{
			booking.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("MAWB", wrapper.MasterBillHeading);

			booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			wrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
			AssertEquals("Ocean Bill Of Lading", wrapper.MasterBillHeading);

			booking.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			wrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
			AssertEquals("Master Bill", wrapper.MasterBillHeading);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			shipment.JS_INCO = Constants.IncoTerms.DeliveredDutyPaid;
			shipment.JS_RS_NKServiceLevel = "SLV";
			shipment.JS_ShipmentStatus = "MSA";
			shipment.JS_RL_NKOrigin = "USDNV";
			shipment.JS_RL_NKDestination = "ERXXX";

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;

			shipment.JS_OH_ImportBroker = GetOrgHeader("IMP_BROKER").PK;
			shipment.JS_OH_ExportBroker = GetOrgHeader("EXP_BROKER").PK;

			shipment.JS_OuterPacks = 400;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_TotalPackageCount = 354;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_ActualWeight = 55.4;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 32.45;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_GoodsValue = 665.33m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 898.22m;
			shipment.JS_RX_NKInsuranceCurrency = "EUR";
			shipment.JS_ShippedOnBoard = "SOB";
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY THING";

			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "TEST 1 ADDRESS";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;
			shipment.JS_A_RCV = new ZDateTime(2017, 9, 12);
			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST 2 ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;

			Transport shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_LegOrder = 2;
			shipmentTransport.JW_RL_NKDiscPort = "ERXXX";

			return new FreightWrapperFromQuotedBooking(booking, Factory);
		}

		public override void TestWrapperNotes()
		{
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "TEST HANDLING INFO");
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "TEST CERTIFICATE OF ORIGIN NOTE INCEST");
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO");

			FreightWrapperFromQuotedBooking noteWrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
			AssertEquals("fullWrapper.DangerousGoodsAdditionalHandlingInformation", "TEST HANDLING INFO", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("fullWrapper.CertificateOfOriginNotes", "TEST CERTIFICATE OF ORIGIN NOTE INCEST", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("fullWrapper.PreAlertArrivalNoticeRemarks", "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			return ((QuotedBooking)parent).QuotedBookingContainers.AddNew();
		}

		public void TestGetBookingReference()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);

			shipment.JS_CFSReference = "DO NOT SHOW ME";

			var wrapper = new FreightWrapperFromQuotedBooking(booking, Factory);
			AssertEquals(ZString.Empty, wrapper.BookingReference);

			var number1 = shipment.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number1.CE_EntryNum = "ENTRY1";
			AssertEquals("ENTRY1", wrapper.BookingReference);

			var number2 = shipment.Numbers.AddNew();
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number2.CE_EntryNum = "ENTRY2";
			AssertEquals("ENTRY1, ENTRY2", wrapper.BookingReference);
		}

		public void TestRequiredDocuments()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);

			shipment.DocsAndCartage.RequiredDocuments.AddNew();
			shipment.DocsAndCartage.RequiredDocuments.AddNew();

			var wrapper = new FreightWrapperFromQuotedBooking(booking, Factory);
			AssertEquals("RequiredDocuments.Count", 2, wrapper.RequiredDocuments.Count);
		}

		public void TestRequiredDocumentsWithNullObject()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var booking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var wrapper = new FreightWrapperFromQuotedBooking(booking, Factory);
			AssertNoExceptionThrown(() => { var count = wrapper.RequiredDocuments.Count; });
		}

		public void TestCO2eEmissions()
		{
			AssertEquals(0, wrapper.CO2eEmissions.Count);

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "GBLON";
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			booking.JS_JX = sailing.PK;

			wrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
			AssertEquals(1, wrapper.CO2eEmissions.Count);
		}

		public override void TestFormattedTotalCO2e()
		{
			AssertEquals(ZString.Empty, wrapper.FormattedTotalCO2e);

			quotedBooking.SetTotalCO2e(9.07244m);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			wrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
			AssertEquals("9.072", wrapper.FormattedTotalCO2e);

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			wrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
			AssertEquals(ZString.Empty, wrapper.FormattedTotalCO2e);
		}

		public override void TestCO2eCalculationDate()
		{
			AssertEquals(ZDateTime.Empty, wrapper.CO2eCalculationDate);

			quotedBooking.SetTotalCO2e(200m);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			wrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
			AssertEquals(((JobCO2e)quotedBooking.GetOrCreateJobCO2e()).JCO_SystemLastEditTimeUtc, wrapper.CO2eCalculationDate);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "ActualReceive", "12-Sep-17 00:00:00" },
					{ "HBLContainerMode", "LCL" },
					{ "HouseBillHeading", "House Bill" },
					{ "JobNumberHeading", "Booking" },
					{ "MasterBillHeading", "Master Bill" },
					{ "NoCopyBills", "3" },
					{ "NoOriginalBills", "3" },
					{ "SecondaryHeading", "Quote No." }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
ChargeableWeight : 32.450 M3
Consignee : IMPORTER\nAUSTRALIA
Consignor : SUPPLIER\nAUSTRALIA
DeliveryAddress : IMPORTER\nAUSTRALIA
DeliveryAgent : DELIVERYCARTAGE\nAUSTRALIA
DeliveryLocation : ERXXX
Destination : ERXXX
ExportBroker : EXP_BROKER\nAUSTRALIA
ExportReceivingCTOAddress : TEST 1 ADDRESS\nAUSTRALIA
ExportReceivingDepotAddress : TEST 1 ADDRESS\nAUSTRALIA
GoodsAvailableAt : TEST 2 ADDRESS\nAUSTRALIA
GoodsValue : 665.33 HKD
ImportBroker : IMP_BROKER\nAUSTRALIA
IncoTerm : DDP - Delivered Duty Paid
InsuranceValue : 898.22 EUR
Origin : USDNV - Dunnville
PickupAddress : SUPPLIER\nAUSTRALIA
PickupCFSAddress : TEST 1 ADDRESS\nAUSTRALIA
PickupLocation : USDNV - Dunnville
ServiceLevel : SLV
ShipmentContainerMode : LCL - Less Container Load
ShipmentInnerPacksQty : 354 BOX
ShipmentOuterPacksQty : 400 PKG
ShipmentStatus : MSA
ShipmentType : IMP - Import
Volume : 32.450 M3
Weight : 55.400 KG";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			booking = QuotedBooking.CreateNewBooking(Factory);
			return QuotedBooking.New(quote.PK, booking.PK, Factory);
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((QuotedBooking)WrappedBO).OH_Carrier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return new FreightWrapperFromQuotedBooking((QuotedBooking)WrappedBO, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			wrapper = new FreightWrapperFromQuotedBooking(quotedBooking, Factory);
		}
		FreightWrapperFromQuotedBooking wrapper;
		QuotedBooking quotedBooking;
		ForwardingShipment booking;

		#endregion
	}
}
