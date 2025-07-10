
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BookingHeaderDataRow : BaseDataRow
	{
		public BookingHeaderDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.BookingHeader)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.BookingHeader.ReferenceNo, GetValue(ShipnetConstants.BookingHeader.ReferenceNoPosition, ShipnetConstants.BookingHeader.ReferenceNoMaxLength));
			SetField(ShipnetConstants.BookingHeader.SecondaryReference, GetValue(ShipnetConstants.BookingHeader.SecondaryReferencePosition, ShipnetConstants.BookingHeader.SecondaryReferenceMaxLength));
			SetField(ShipnetConstants.BookingHeader.AcceptanceLocationCode, GetValue(ShipnetConstants.BookingHeader.AcceptanceLocationCodePosition, ShipnetConstants.BookingHeader.AcceptanceLocationCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.AcceptanceLocationName, GetValue(ShipnetConstants.BookingHeader.AcceptanceLocationNamePosition, ShipnetConstants.BookingHeader.AcceptanceLocationNameMaxLength));
			SetField(ShipnetConstants.BookingHeader.AcceptancePortCode, GetValue(ShipnetConstants.BookingHeader.AcceptancePortCodePosition, ShipnetConstants.BookingHeader.AcceptancePortCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.LoadPortCode, GetValue(ShipnetConstants.BookingHeader.LoadPortCodePosition, ShipnetConstants.BookingHeader.LoadPortCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.TranshipmentPortCode, GetValue(ShipnetConstants.BookingHeader.TranshipmentPortCodePosition, ShipnetConstants.BookingHeader.TranshipmentPortCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.DischargePortCode, GetValue(ShipnetConstants.BookingHeader.DischargePortCodePosition, ShipnetConstants.BookingHeader.DischargePortCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.DeliveryPortCode, GetValue(ShipnetConstants.BookingHeader.DeliveryPortCodePosition, ShipnetConstants.BookingHeader.DeliveryPortCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.DeliveryLocationCode, GetValue(ShipnetConstants.BookingHeader.DeliveryLocationCodePosition, ShipnetConstants.BookingHeader.DeliveryLocationCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.DeliveryLocationName, GetValue(ShipnetConstants.BookingHeader.DeliveryLocationNamePosition, ShipnetConstants.BookingHeader.DeliveryLocationNameMaxLength));
			SetField(ShipnetConstants.BookingHeader.OriginCountryCode, GetValue(ShipnetConstants.BookingHeader.OriginCountryCodePosition, ShipnetConstants.BookingHeader.OriginCountryCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.OriginCountryName, GetValue(ShipnetConstants.BookingHeader.OriginCountryNamePosition, ShipnetConstants.BookingHeader.OriginCountryNameMaxLength));
			SetField(ShipnetConstants.BookingHeader.DestinationCountryCode, GetValue(ShipnetConstants.BookingHeader.DestinationCountryCodePosition, ShipnetConstants.BookingHeader.DestinationCountryCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.DestinationCountryName, GetValue(ShipnetConstants.BookingHeader.DestinationCountryNamePosition, ShipnetConstants.BookingHeader.DestinationCountryNameMaxLength));
			SetField(ShipnetConstants.BookingHeader.OriginTerms, GetValue(ShipnetConstants.BookingHeader.OriginTermsPosition, ShipnetConstants.BookingHeader.OriginTermsMaxLength));
			SetField(ShipnetConstants.BookingHeader.DestinationTerms, GetValue(ShipnetConstants.BookingHeader.DestinationTermsPosition, ShipnetConstants.BookingHeader.DestinationTermsMaxLength));
			SetField(ShipnetConstants.BookingHeader.PaymentTerms, GetValue(ShipnetConstants.BookingHeader.PaymentTermsPosition, ShipnetConstants.BookingHeader.PaymentTermsMaxLength));
			SetField(ShipnetConstants.BookingHeader.EDIFACTCargoType, GetValue(ShipnetConstants.BookingHeader.EDIFACTCargoTypePosition, ShipnetConstants.BookingHeader.EDIFACTCargoTypeMaxLength));
			SetField(ShipnetConstants.BookingHeader.FAKIndicator, GetValue(ShipnetConstants.BookingHeader.FAKIndicatorPosition, ShipnetConstants.BookingHeader.FAKIndicatorMaxLength));
			SetField(ShipnetConstants.BookingHeader.ETA, GetValue(ShipnetConstants.BookingHeader.ETAPosition, ShipnetConstants.BookingHeader.ETAMaxLength));
			SetField(ShipnetConstants.BookingHeader.ETD, GetValue(ShipnetConstants.BookingHeader.ETDPosition, ShipnetConstants.BookingHeader.ETDMaxLength));
			SetField(ShipnetConstants.BookingHeader.UnderBond, GetValue(ShipnetConstants.BookingHeader.UnderBondPosition, ShipnetConstants.BookingHeader.UnderBondMaxLength));
			SetField(ShipnetConstants.BookingHeader.OriginPremiseCode, GetValue(ShipnetConstants.BookingHeader.OriginPremiseCodePosition, ShipnetConstants.BookingHeader.OriginPremiseCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.OriginPremiseName, GetValue(ShipnetConstants.BookingHeader.OriginPremiseNamePosition, ShipnetConstants.BookingHeader.OriginPremiseNameMaxLength));
			SetField(ShipnetConstants.BookingHeader.DestPremiseCode, GetValue(ShipnetConstants.BookingHeader.DestPremiseCodePosition, ShipnetConstants.BookingHeader.DestPremiseCodeMaxLength));
			SetField(ShipnetConstants.BookingHeader.DestPremiseName, GetValue(ShipnetConstants.BookingHeader.DestPremiseNamePosition, ShipnetConstants.BookingHeader.DestPremiseNameMaxLength));
			SetField(ShipnetConstants.BookingHeader.Berth, GetValue(ShipnetConstants.BookingHeader.BerthPosition, ShipnetConstants.BookingHeader.BerthMaxLength));
			SetField(ShipnetConstants.BookingHeader.CAN, GetValue(ShipnetConstants.BookingHeader.CANPosition, ShipnetConstants.BookingHeader.CANMaxLength));
		}
	}
}
