
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BasePartyDetailsDataRow : BaseDataRow
	{
		public BasePartyDetailsDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.PartyDetails)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.PartyDetails.PartyCode, GetValue(ShipnetConstants.PartyDetails.PartyCodePosition, ShipnetConstants.PartyDetails.PartyCodeMaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyName, GetValue(ShipnetConstants.PartyDetails.PartyNamePosition, ShipnetConstants.PartyDetails.PartyNameMaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyAddress1, GetValue(ShipnetConstants.PartyDetails.PartyAddress1Position, ShipnetConstants.PartyDetails.PartyAddress1MaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyAddress2, GetValue(ShipnetConstants.PartyDetails.PartyAddress2Position, ShipnetConstants.PartyDetails.PartyAddress2MaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyAddress3, GetValue(ShipnetConstants.PartyDetails.PartyAddress3Position, ShipnetConstants.PartyDetails.PartyAddress3MaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyAddress4, GetValue(ShipnetConstants.PartyDetails.PartyAddress4Position, ShipnetConstants.PartyDetails.PartyAddress4MaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyAddress5, GetValue(ShipnetConstants.PartyDetails.PartyAddress5Position, ShipnetConstants.PartyDetails.PartyAddress5MaxLength));
		}
	}
}
