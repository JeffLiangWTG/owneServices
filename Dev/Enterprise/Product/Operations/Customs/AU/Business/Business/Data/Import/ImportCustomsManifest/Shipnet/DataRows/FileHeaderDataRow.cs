
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FileHeaderDataRow : BaseDataRow
	{
		public FileHeaderDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.FileHeader)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.FileHeader.Context, GetValue(ShipnetConstants.FileHeader.ContextPosition, ShipnetConstants.FileHeader.ContextMaxLength));
			SetField(ShipnetConstants.FileHeader.SenderID, GetValue(ShipnetConstants.FileHeader.SenderIDPosition, ShipnetConstants.FileHeader.SenderIDMaxLength));
			SetField(ShipnetConstants.FileHeader.RecipientID, GetValue(ShipnetConstants.FileHeader.RecipientIDPosition, ShipnetConstants.FileHeader.RecipientIDMaxLength));
			SetField(ShipnetConstants.FileHeader.MessageType, GetValue(ShipnetConstants.FileHeader.MessageTypePosition, ShipnetConstants.FileHeader.MessageTypeMaxLength));
			SetField(ShipnetConstants.FileHeader.MessageVersion, GetValue(ShipnetConstants.FileHeader.MessageVersionPosition, ShipnetConstants.FileHeader.MessageVersionMaxLength));
			SetField(ShipnetConstants.FileHeader.VesselCode, GetValue(ShipnetConstants.FileHeader.VesselCodePosition, ShipnetConstants.FileHeader.VesselCodeMaxLength));
			SetField(ShipnetConstants.FileHeader.VesselName, GetValue(ShipnetConstants.FileHeader.VesselNamePosition, ShipnetConstants.FileHeader.VesselNameMaxLength));
			SetField(ShipnetConstants.FileHeader.VesselLloyds, GetValue(ShipnetConstants.FileHeader.VesselLloydsPosition, ShipnetConstants.FileHeader.VesselLloydsMaxLength));
			SetField(ShipnetConstants.FileHeader.VesselOperator, GetValue(ShipnetConstants.FileHeader.VesselOperatorPosition, ShipnetConstants.FileHeader.VesselOperatorMaxLength));
			SetField(ShipnetConstants.FileHeader.Carrier, GetValue(ShipnetConstants.FileHeader.CarrierPosition, ShipnetConstants.FileHeader.CarrierMaxLength));
			SetField(ShipnetConstants.FileHeader.ModeOfTransport, GetValue(ShipnetConstants.FileHeader.ModeOfTransportPosition, ShipnetConstants.FileHeader.ModeOfTransportMaxLength));
			SetField(ShipnetConstants.FileHeader.VoyageNumber, GetValue(ShipnetConstants.FileHeader.VoyageNumberPosition, ShipnetConstants.FileHeader.VoyageNumberMaxLength));
			SetField(ShipnetConstants.FileHeader.AddTareWeight, GetValue(ShipnetConstants.FileHeader.AddTareWeightPosition, ShipnetConstants.FileHeader.AddTareWeightMaxLength));
			SetField(ShipnetConstants.FileHeader.LastPortOfLoadCode, GetValue(ShipnetConstants.FileHeader.LastPortOfLoadCodePosition, ShipnetConstants.FileHeader.LastPortOfLoadCodeMaxLength));
			SetField(ShipnetConstants.FileHeader.LastPortOfLoadName, GetValue(ShipnetConstants.FileHeader.LastPortOfLoadNamePosition, ShipnetConstants.FileHeader.LastPortOfLoadNameMaxLength));
			SetField(ShipnetConstants.FileHeader.SailDate, GetValue(ShipnetConstants.FileHeader.SailDatePosition, ShipnetConstants.FileHeader.SailDateMaxLength));
			SetField(ShipnetConstants.FileHeader.SailTime, GetValue(ShipnetConstants.FileHeader.SailTimePosition, ShipnetConstants.FileHeader.SailTimeMaxLength));
			SetField(ShipnetConstants.FileHeader.FirstPODCode, GetValue(ShipnetConstants.FileHeader.FirstPODCodePosition, ShipnetConstants.FileHeader.FirstPODCodeMaxLength));
			SetField(ShipnetConstants.FileHeader.FirstPODName, GetValue(ShipnetConstants.FileHeader.FirstPODNamePosition, ShipnetConstants.FileHeader.FirstPODNameMaxLength));
			SetField(ShipnetConstants.FileHeader.ArrivalDate, GetValue(ShipnetConstants.FileHeader.ArrivalDatePosition, ShipnetConstants.FileHeader.ArrivalDateMaxLength));
			SetField(ShipnetConstants.FileHeader.ArrivalTime, GetValue(ShipnetConstants.FileHeader.ArrivalTimePosition, ShipnetConstants.FileHeader.ArrivalTimeMaxLength));
			SetField(ShipnetConstants.FileHeader.BerthCTO, GetValue(ShipnetConstants.FileHeader.BerthCTOPosition, ShipnetConstants.FileHeader.BerthCTOMaxLength));
			SetField(ShipnetConstants.FileHeader.SendersABN, GetValue(ShipnetConstants.FileHeader.SendersABNPosition, ShipnetConstants.FileHeader.SendersABNMaxLength));
		}
	}
}
