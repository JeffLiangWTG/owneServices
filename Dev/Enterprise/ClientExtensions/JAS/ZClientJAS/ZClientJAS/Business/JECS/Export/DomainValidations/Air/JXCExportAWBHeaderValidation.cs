using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCExportAWBHeaderValidation : Freight.Forwarding.AWB.Business.AutoExportAWBHeaderValidation
	{
		public JXCExportAWBHeaderValidation(ExportAWBHeader aWBHeader)
			: base(aWBHeader)
		{
		}

		#region Overrides

		protected override void CheckEH_AWBOriginCode()
		{
			ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Parent.EH_AWBOriginCodeInfo, JXCConstants.AWBFieldBoundaries.OriginCodeLength);
		}

		#region Shipper

		protected override void CheckEH_ShipperAccount()
		{
			ValidationHelper.ValidateFreeTextField(Parent.EH_ShipperAccountInfo, JXCConstants.AWBFieldBoundaries.ShipperAccountMaxLength);
		}

		protected override void CheckEH_ShipperName()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_ShipperNameInfo);
		}

		protected override void CheckEH_ShipperAddress()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_ShipperAddressInfo);
		}

		protected override void CheckEH_ShipperPlace()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_ShipperPlaceInfo);
		}

		#endregion

		#region Consignee

		protected override void CheckEH_ConsigneeAccount()
		{
			ValidationHelper.ValidateFreeTextField(Parent.EH_ConsigneeAccountInfo, JXCConstants.AWBFieldBoundaries.ConsigneeAccountMaxLength);
		}

		protected override void CheckEH_ConsigneeName()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_ConsigneeNameInfo);
		}

		protected override void CheckEH_ConsigneeAddress()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_ConsigneeAddressInfo);
		}

		protected override void CheckEH_ConsigneePlace()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_ConsigneePlaceInfo);
		}

		#endregion

		protected override void CheckEH_IssuingAgentName()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_IssuingAgentNameInfo);
		}

		protected void CheckEH_AgentIATACodeFormatted()
		{
			string errorMessage = "Invalid Agent IATA Code format";
			ValidationHelper.ValidateRegexField(Parent.EH_AgentIATACodeFormattedInfo, JXCConstants.AWBFieldBoundaries.AgentIATACodeRegex, errorMessage);
		}

		#region Routing

		protected override void CheckEH_To1st()
		{
			ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Parent.EH_To1stInfo, JXCConstants.AWBFieldBoundaries.ToLength);
		}

		protected override void CheckEH_By1st()
		{
			ValidationHelper.ValidateFreeTextFieldWithExactLength(Parent.EH_By1stInfo, JXCConstants.AWBFieldBoundaries.ByLength);
		}

		protected override void CheckEH_To2nd()
		{
			if (!Parent.EH_To2nd.IsEmpty || !Parent.EH_By2nd.IsEmpty)
			{
				ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Parent.EH_To2ndInfo, JXCConstants.AWBFieldBoundaries.ToLength);
			}
		}

		protected override void CheckEH_By2nd()
		{
			if (!Parent.EH_By2nd.IsEmpty || !Parent.EH_To2nd.IsEmpty)
			{
				ValidationHelper.ValidateFreeTextFieldWithExactLength(Parent.EH_By2ndInfo, JXCConstants.AWBFieldBoundaries.ByLength);
			}
		}

		protected override void CheckEH_To3rd()
		{
			if (!Parent.EH_To3rd.IsEmpty || !Parent.EH_By3rd.IsEmpty)
			{
				ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Parent.EH_To3rdInfo, JXCConstants.AWBFieldBoundaries.ToLength);
			}
		}

		protected override void CheckEH_By3rd()
		{
			if (!Parent.EH_By3rd.IsEmpty || !Parent.EH_To3rd.IsEmpty)
			{
				ValidationHelper.ValidateFreeTextFieldWithExactLength(Parent.EH_By3rdInfo, JXCConstants.AWBFieldBoundaries.ByLength);
			}
		}

		#endregion

		#region Freight Declarations

		protected override void CheckEH_Currency()
		{
			ValidationHelper.ValidateCurrencyCode(Parent.Factory, Parent.EH_CurrencyInfo, Parent.EH_Currency);
		}

		protected override void CheckEH_ChargesCode()
		{
			string warningMessage = "Invalid " + Parent.EH_ChargesCodeInfo.HumanReadableName;
			ValidationHelper.ValidateRegexField(Parent.EH_ChargesCodeInfo, JXCConstants.AWBFieldBoundaries.ChargeCode, warningMessage);
		}

		protected override void CheckEH_WeightVPPDCOL()
		{
			string warningMessage = "Invalid Weight / Valuation Charge Payment Type";
			ValidationHelper.ValidateRegexField(Parent.EH_WeightVPPDCOLInfo, Parent.EH_WeightVPPDCOL.Left(1), JXCConstants.AWBFieldBoundaries.WeightPPDorCOL, warningMessage);
		}

		#endregion

		#region Flight Info

		protected override void CheckEH_AirportOfDepartureAndRequestRouteText()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_AirportOfDepartureAndRequestRouteTextInfo);
		}

		protected override void CheckEH_AirportOfDestinationText()
		{
			ValidationHelper.ValidateFreeTextField(Parent.EH_AirportOfDestinationTextInfo, JXCConstants.AWBFieldBoundaries.AirportOfDestinationMinLength, JXCConstants.AWBFieldBoundaries.AirportOfDestinationMaxLength);
		}

		protected override void CheckEH_Booking1stCarrier()
		{
			ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Parent.EH_Booking1stCarrierInfo, JXCConstants.AWBFieldBoundaries.FlightCarrierCodeLength);
		}

		protected override void CheckEH_Booking1stFlight()
		{
			ValidateFlightNumber(Parent.EH_Booking1stFlightInfo);
		}

		protected override void CheckEH_Booking1stFlightDate()
		{
			if (Parent.Booking1stFlightDate.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(Parent.EH_Booking1stFlightDateInfo, "Flight Date is empty or invalid");
			}
		}

		protected override void CheckEH_Booking2ndCarrier()
		{
			if (!Parent.EH_Booking2ndCarrier.IsEmpty || !Parent.EH_Booking2ndFlight.IsEmpty)
			{
				ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Parent.EH_Booking2ndCarrierInfo, JXCConstants.AWBFieldBoundaries.FlightCarrierCodeLength);
			}
		}

		protected override void CheckEH_Booking2ndFlight()
		{
			if (!Parent.EH_Booking2ndFlight.IsEmpty || !Parent.EH_Booking2ndCarrier.IsEmpty)
			{
				ValidateFlightNumber(Parent.EH_Booking2ndFlightInfo);
			}
		}

		protected override void CheckEH_Booking2ndFlightDate()
		{
			if ((!Parent.EH_Booking2ndFlightDate.IsEmpty || !Parent.EH_Booking2ndCarrier.IsEmpty || !Parent.EH_Booking2ndFlight.IsEmpty) && Parent.Booking2ndFlightDate.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(Parent.EH_Booking2ndFlightDateInfo, "Flight Date is empty or invalid");
			}
		}

		void ValidateFlightNumber(ZPropertyInfo propertyInfo)
		{
			ValidationHelper.ValidateRegexField(propertyInfo, JXCConstants.AWBFieldBoundaries.FlightNumberRegex, "Invalid Flight Number");
		}

		#endregion

		#region Footer

		protected override void CheckEH_AWBIssuePlace()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_AWBIssuePlaceInfo);
		}

		protected override void CheckEH_AWBIssueDate()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_AWBIssueDateInfo);
		}

		#endregion

		#endregion

		protected new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		protected ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}
				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;
		public readonly JXCConstants.AWBFieldBoundaries FieldBoundaries;
	}
}

#region Overrides
#region Shipper
#endregion
#region Consignee
#endregion
#region Routing
#endregion
#region Freight Declarations
#endregion
#region Flight Info
#endregion
#region Footer
#endregion
#endregion
#region Implementation
#endregion
