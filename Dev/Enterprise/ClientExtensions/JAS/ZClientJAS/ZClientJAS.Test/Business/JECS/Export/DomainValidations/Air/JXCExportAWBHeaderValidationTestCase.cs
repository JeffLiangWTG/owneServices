using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal abstract class JXCExportAWBHeaderValidationTestCase : JXCValidationTestCase
	{
		public void TestDomainValidationShouldSubclassFromAutoValidationType()
		{
			AssertEquals(typeof(Freight.Forwarding.AWB.Business.AutoExportAWBHeaderValidation), typeof(JXCExportAWBHeaderValidation).BaseType);
		}

		#region Overrides
		public void TestValidateEH_AWBOriginCode()
		{
			AssertHasNoJXCWarnings(AWBHeader.EH_AWBOriginCodeInfo);
			AWBHeader.EH_AWBOriginCode = "SY";
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_AWBOriginCodeInfo, JXCConstants.AWBFieldBoundaries.OriginCodeLength);
			AWBHeader.EH_AWBOriginCode = "SYD";
			AssertHasNoJXCWarnings(AWBHeader.EH_AWBOriginCodeInfo);
		}

		#region Shipper
		public void TestValidateEH_ShipperAccount()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ShipperAccountMaxLength, ExportAWBHeaderSchema.EH_ShipperAccount);
		}

		public void TestValidateEH_ShipperName()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ShipperNameMaxLength, ExportAWBHeaderSchema.EH_ShipperName);
			AWBHeader.EH_ShipperName = "Shipper Name OK";
			AssertHasNoJXCWarnings(AWBHeader.EH_ShipperNameInfo);
			AWBHeader.EH_ShipperName = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_ShipperNameInfo);
		}

		public void TestValidateEH_ShipperAddress()
		{
			AWBHeader.EH_ShipperAddress = "Shipper Address OK";
			AssertHasNoJXCWarnings(AWBHeader.EH_ShipperAddressInfo);
			AWBHeader.EH_ShipperAddress = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_ShipperAddressInfo);
		}

		public void TestValidateEH_ShipperPlace()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ShipperCityMaxLength, ExportAWBHeaderSchema.EH_ShipperPlace);
			AWBHeader.EH_ShipperPlace = "12345678901234567";
			AssertHasNoJXCWarnings(AWBHeader.EH_ShipperPlaceInfo);
			AWBHeader.EH_ShipperPlace = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_ShipperPlaceInfo);
		}

		public void TestValidateEH_ShipperState()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ShipperStateMaxLength, ExportAWBHeaderSchema.EH_ShipperState);
		}

		public void TestValidateEH_ShipperPostCode()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ShipperPostCodeMaxLength, ExportAWBHeaderSchema.EH_ShipperPostCode);
		}

		#endregion
		#region Consignee
		public void TestValidateEH_ConsigneeAccount()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ConsigneeAccountMaxLength, ExportAWBHeaderSchema.EH_ConsigneeAccount);
		}

		public void TestValidateEH_ConsigneeName()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ConsigneeNameMaxLength, ExportAWBHeaderSchema.EH_ConsigneeName);
			AWBHeader.EH_ConsigneeName = "Consignee Name OK";
			AssertHasNoJXCWarnings(AWBHeader.EH_ConsigneeNameInfo);
			AWBHeader.EH_ConsigneeName = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_ConsigneeNameInfo);
		}

		public void TestValidateEH_ConsigneeAddress()
		{
			AWBHeader.EH_ConsigneeAddress = "Consignee Address OK";
			AssertHasNoJXCWarnings(AWBHeader.EH_ConsigneeAddressInfo);
			AWBHeader.EH_ConsigneeAddress = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_ConsigneeAddressInfo);
		}

		public void TestValidateEH_ConsigneePlace()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ConsigneeCityMaxLength, ExportAWBHeaderSchema.EH_ConsigneePlace);
			AWBHeader.EH_ConsigneePlace = "12345678901234567";
			AssertHasNoJXCWarnings(AWBHeader.EH_ConsigneePlaceInfo);
			AWBHeader.EH_ConsigneePlace = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_ConsigneePlaceInfo);
		}

		public void TestValidateEH_ConsigneeState()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ConsigneeStateMaxLength, ExportAWBHeaderSchema.EH_ConsigneeState);
		}

		public void TestValidateEH_ConsigneePostCode()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.ConsigneePostCodeMaxLength, ExportAWBHeaderSchema.EH_ConsigneePostCode);
		}

		#endregion
		public void TestValidateEH_IssuingAgentName()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.CarrierNameMaxLength, ExportAWBHeaderSchema.EH_IssuingAgentName);
			AWBHeader.EH_IssuingAgentName = "Carrier Name OK";
			AssertHasNoJXCWarnings(AWBHeader.EH_IssuingAgentNameInfo);
			AWBHeader.EH_IssuingAgentName = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_IssuingAgentNameInfo);
		}

		public void TestValidateEH_AgentIATACodeFormatted()
		{
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Invalid Agent IATA Code format";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "101";
			AWBHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			AssertHasJXCWarning(AWBHeader.EH_AgentIATACodeFormattedInfo, expectedWarningMessage);
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "99-1 1234/5678";
			AWBHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			AssertHasNoJXCWarnings(AWBHeader.EH_AgentIATACodeFormattedInfo);
		}

		#region Routing
		public void TestValidateEH_To1st()
		{
			AWBHeader.EH_To1st = "1";
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_To1stInfo, JXCConstants.AWBFieldBoundaries.ToLength);
			AWBHeader.EH_To1st = "BNE";
			AssertHasNoJXCWarnings(AWBHeader.EH_To1stInfo);
		}

		public void TestValidateEH_By1st()
		{
			AWBHeader.EH_By1st = "A";
			AssertHasExactLengthJXCWarning(AWBHeader.EH_By1stInfo, JXCConstants.AWBFieldBoundaries.ByLength);
			AWBHeader.EH_By1st = "GA";
			AssertHasNoJXCWarnings(AWBHeader.EH_By1stInfo);
		}

		public void TestValidateEH_To2nd()
		{
			AWBHeader.EH_To2nd = "1";
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_To2ndInfo, JXCConstants.AWBFieldBoundaries.ToLength);
			AWBHeader.EH_To2nd = "LAX";
			AssertHasNoJXCWarnings(AWBHeader.EH_To2ndInfo);
			AWBHeader.EH_To2nd = "";
			AssertHasNoJXCWarnings(AWBHeader.EH_To2ndInfo);
			AWBHeader.EH_By2nd = "GA";
			AWBHeader.Validation.ValidateEH_To2nd();
			// If By2nd is populated, this needs to be populated as well
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_To2ndInfo, JXCConstants.AWBFieldBoundaries.ToLength);
		}

		public void TestValidateEH_By2nd()
		{
			AWBHeader.EH_By2nd = "A";
			AssertHasExactLengthJXCWarning(AWBHeader.EH_By2ndInfo, JXCConstants.AWBFieldBoundaries.ByLength);
			AWBHeader.EH_By2nd = "GA";
			AssertHasNoJXCWarnings(AWBHeader.EH_By2ndInfo);
			AWBHeader.EH_By2nd = "";
			AssertHasNoJXCWarnings(AWBHeader.EH_By2ndInfo);
			AWBHeader.EH_To2nd = "SYD";
			AWBHeader.Validation.ValidateEH_By2nd();
			// If To2nd is populated, this needs to be populated as well
			AssertHasExactLengthJXCWarning(AWBHeader.EH_By2ndInfo, JXCConstants.AWBFieldBoundaries.ByLength);
		}

		public void TestValidateEH_To3rd()
		{
			AWBHeader.EH_To3rd = "1";
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_To3rdInfo, JXCConstants.AWBFieldBoundaries.ToLength);
			AWBHeader.EH_To3rd = "LAX";
			AssertHasNoJXCWarnings(AWBHeader.EH_To3rdInfo);
			AWBHeader.EH_To3rd = "";
			AssertHasNoJXCWarnings(AWBHeader.EH_To3rdInfo);
			AWBHeader.EH_By3rd = "GA";
			AWBHeader.Validation.ValidateEH_To3rd();
			// If By3rd is populated, this needs to be populated as well
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_To3rdInfo, JXCConstants.AWBFieldBoundaries.ToLength);
		}

		public void TestValidateEH_By3rd()
		{
			AWBHeader.EH_By3rd = "A";
			AssertHasExactLengthJXCWarning(AWBHeader.EH_By3rdInfo, JXCConstants.AWBFieldBoundaries.ByLength);
			AWBHeader.EH_By3rd = "GA";
			AssertHasNoJXCWarnings(AWBHeader.EH_By3rdInfo);
			AWBHeader.EH_By3rd = "";
			AssertHasNoJXCWarnings(AWBHeader.EH_By3rdInfo);
			AWBHeader.EH_To3rd = "SYD";
			AWBHeader.Validation.ValidateEH_By3rd();
			// If To3rd is populated, this needs to be populated as well
			AssertHasExactLengthJXCWarning(AWBHeader.EH_By3rdInfo, JXCConstants.AWBFieldBoundaries.ByLength);
		}

		#endregion
		#region Freight Declarations
		public void TestValidateEH_ChargesCode()
		{
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Invalid " + AWBHeader.EH_ChargesCodeInfo.HumanReadableName;
			AWBHeader.EH_ChargesCode = "0_";
			AssertHasJXCWarning(AWBHeader.EH_ChargesCodeInfo, expectedWarningMessage);
			AWBHeader.EH_ChargesCode = "PP";
			AssertHasNoJXCWarnings(AWBHeader.EH_ChargesCodeInfo);
		}

		public void TestValidateEH_WeightVPPDCOL()
		{
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Invalid Weight / Valuation Charge Payment Type";
			AWBHeader.EH_WeightVPPDCOL = "XX";
			AssertHasJXCWarning(AWBHeader.EH_WeightVPPDCOLInfo, expectedWarningMessage);
			AWBHeader.EH_WeightVPPDCOL = "PPD";
			AssertHasNoJXCWarnings(AWBHeader.EH_WeightVPPDCOLInfo);
		}

		public void TestValidateEH_Currency()
		{
			AWBHeader.EH_Currency = "123";
			AssertHasInvalidCurrencyCodeJXCWarning(AWBHeader.EH_CurrencyInfo);
			AWBHeader.EH_Currency = Core.Constants.CurrencyCodes.Australia;
			AssertHasNoJXCWarnings(AWBHeader.EH_CurrencyInfo);
		}

		#endregion
		#region Flight Info
		public void TestValidateEH_AirportOfDepartureAndRequestRouteText()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.AirportOfDepartureMaxLength, ExportAWBHeaderSchema.EH_AirportOfDepartureAndRequestRouteText);
			AWBHeader.EH_AirportOfDepartureAndRequestRouteText = "Sydney International Airport";
			AssertHasNoJXCWarnings(AWBHeader.EH_AirportOfDepartureAndRequestRouteTextInfo);
			AWBHeader.EH_AirportOfDepartureAndRequestRouteText = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_AirportOfDepartureAndRequestRouteTextInfo);
		}

		public void TestValidateEH_AirportOfDestinationText()
		{
			AWBHeader.EH_AirportOfDestinationText = "012345678901234567890123456";
			AssertHasMinMaxLengthJXCWarning(AWBHeader.EH_AirportOfDestinationTextInfo, JXCConstants.AWBFieldBoundaries.AirportOfDestinationMinLength, JXCConstants.AWBFieldBoundaries.AirportOfDestinationMaxLength);
			AWBHeader.EH_AirportOfDestinationText = "";
			AssertHasMinMaxLengthJXCWarning(AWBHeader.EH_AirportOfDestinationTextInfo, JXCConstants.AWBFieldBoundaries.AirportOfDestinationMinLength, JXCConstants.AWBFieldBoundaries.AirportOfDestinationMaxLength);
			AWBHeader.EH_AirportOfDestinationText = "Brisbane airport";
			AssertHasNoJXCWarnings(AWBHeader.EH_AirportOfDestinationTextInfo);
		}

		public void TestValidateEH_Booking1stCarrier()
		{
			AWBHeader.EH_Booking1stCarrier = "1";
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_Booking1stCarrierInfo, JXCConstants.AWBFieldBoundaries.FlightCarrierCodeLength);
			AWBHeader.EH_Booking1stCarrier = "12";
			AssertHasNoJXCWarnings(AWBHeader.EH_Booking1stCarrierInfo);
		}

		public void TestValidateEH_Booking2ndCarrier()
		{
			AWBHeader.EH_Booking2ndCarrier = "1";
			AssertHasAlphanumericExactLengthJXCWarning(AWBHeader.EH_Booking2ndCarrierInfo, JXCConstants.AWBFieldBoundaries.FlightCarrierCodeLength);
			AWBHeader.EH_Booking2ndCarrier = "12";
			AssertHasNoJXCWarnings(AWBHeader.EH_Booking2ndCarrierInfo);
		}

		public void TestValidateEH_Booking1stFlight()
		{
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Invalid Flight Number";
			AWBHeader.EH_Booking1stFlight = "ABC";
			AssertHasJXCWarning(AWBHeader.EH_Booking1stFlightInfo, expectedWarningMessage);
			AWBHeader.EH_Booking1stFlight = "12";
			AssertHasNoJXCWarnings("Should be padded in the set_EH_Booking1stFlight", AWBHeader.EH_Booking1stFlightInfo);
			AWBHeader.EH_Booking1stFlight = "";
			AssertHasJXCWarning(AWBHeader.EH_Booking1stFlightInfo, expectedWarningMessage);
			AWBHeader.EH_Booking1stFlight = "12345";
			AssertHasJXCWarning(AWBHeader.EH_Booking1stFlightInfo, expectedWarningMessage);
			AWBHeader.EH_Booking1stFlight = "123";
			AssertHasNoJXCWarnings(AWBHeader.EH_Booking1stFlightInfo);
		}

		public void TestValidateEH_Booking1stFlightDate()
		{
			ChangeOverrideWayBillDefaultsFlag(true);
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Flight Date is empty or invalid";
			AWBHeader.EH_Booking1stFlightDate = "TT";
			AssertHasJXCWarning(AWBHeader.EH_Booking1stFlightDateInfo, expectedWarningMessage);
			AWBHeader.EH_Booking1stFlightDate = "";
			AssertHasJXCWarning(AWBHeader.EH_Booking1stFlightDateInfo, expectedWarningMessage);
			AWBHeader.EH_Booking1stFlightDate = "02";
			AssertHasNoJXCWarnings(AWBHeader.EH_Booking1stFlightDateInfo);
		}

		public void TestValidateEH_Booking2ndFlight()
		{
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Invalid Flight Number";
			AWBHeader.EH_Booking2ndFlight = "ABC";
			AssertHasJXCWarning(AWBHeader.EH_Booking2ndFlightInfo, expectedWarningMessage);
			AWBHeader.EH_Booking2ndFlight = "12";
			AssertHasNoJXCWarnings("Should be padded in the set_EH_Booking2ndFlight", AWBHeader.EH_Booking2ndFlightInfo);
			AWBHeader.EH_Booking2ndFlight = "";
			AssertHasNoJXCWarnings("Carrier is empty, should not be checking for the FlightNumber existance", AWBHeader.EH_Booking2ndFlightInfo);
			AWBHeader.EH_Booking2ndCarrier = "QF";
			AWBHeader.Validation.ValidateEH_Booking2ndFlight();
			AssertHasJXCWarning(AWBHeader.EH_Booking2ndFlightInfo, expectedWarningMessage);
			AWBHeader.EH_Booking2ndFlight = "12345";
			AssertHasJXCWarning(AWBHeader.EH_Booking2ndFlightInfo, expectedWarningMessage);
			AWBHeader.EH_Booking2ndFlight = "123";
			AssertHasNoJXCWarnings(AWBHeader.EH_Booking2ndFlightInfo);
		}

		public void TestValidateEH_Booking2ndFlightDate()
		{
			ChangeOverrideWayBillDefaultsFlag(true);
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Flight Date is empty or invalid";
			AWBHeader.EH_Booking2ndFlightDate = "TT";
			AssertHasJXCWarning(AWBHeader.EH_Booking2ndFlightDateInfo, expectedWarningMessage);
			AWBHeader.EH_Booking2ndFlightDate = "";
			AssertHasNoJXCWarnings("Carrier and/or Flight Number are empty, should not be checking for the FlightDate existance", AWBHeader.EH_Booking2ndFlightDateInfo);
			AWBHeader.EH_Booking2ndCarrier = "GA";
			AWBHeader.Validation.ValidateEH_Booking2ndFlightDate();
			AssertHasJXCWarning(AWBHeader.EH_Booking2ndFlightDateInfo, expectedWarningMessage);
			AWBHeader.EH_Booking2ndFlightDate = "02";
			AssertHasNoJXCWarnings(AWBHeader.EH_Booking2ndFlightDateInfo);
		}

		#endregion
		#region Footer
		public void TestValidateEH_AWBIssuePlace()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.PlaceOfIssueMaxLength, ExportAWBHeaderSchema.EH_AWBIssuePlace);
			AWBHeader.EH_AWBIssuePlace = "Montevideo";
			AssertHasNoJXCWarnings(AWBHeader.EH_AWBIssuePlaceInfo);
			AWBHeader.EH_AWBIssuePlace = "";
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_AWBIssuePlaceInfo);
		}

		public void TestValidateEH_ShippersSignature()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.SignatureMaxLength, ExportAWBHeaderSchema.EH_ShippersSignature);
		}

		public void TestValidateEH_AWBAgentsSignature()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.SignatureMaxLength, ExportAWBHeaderSchema.EH_AWBAgentsSignature);
		}

		public void TestValidateEH_AWBIssueDate()
		{
			AWBHeader.EH_AWBIssueDate = ZDateTime.Now;
			AssertHasNoJXCWarnings(AWBHeader.EH_AWBIssueDateInfo);
			AWBHeader.EH_AWBIssueDate = ZDateTime.Empty;
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_AWBIssueDateInfo);
		}

		#endregion
		#endregion
		#region Implementation
		protected ExportAWBHeader AWBHeader
		{
			get
			{
				if (fAWBHeader == null)
				{
					fAWBHeader = GetNewAWBHeader();
				}

				return fAWBHeader;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory.Validation.MainGroup.RegisterValidationType(AWBHeader.GetType(), ExpectedJXCExportAWBHeaderValidationTypeToTest);
		}

		protected abstract Type ExpectedJXCExportAWBHeaderValidationTypeToTest { get; }

		protected abstract ExportAWBHeader GetNewAWBHeader();
		protected abstract void ChangeOverrideWayBillDefaultsFlag(bool shouldOverride);
		ExportAWBHeader fAWBHeader;
		#endregion
	}
}
