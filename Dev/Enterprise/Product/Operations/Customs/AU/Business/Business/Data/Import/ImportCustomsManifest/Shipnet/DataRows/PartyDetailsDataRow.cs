
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for PartyDetailsRow.
	/// </summary>
	public class PartyDetailsDataRow : BasePartyDetailsDataRow
	{
		public PartyDetailsDataRow(ZString rawRow)
			: base(rawRow)
		{
		}

		protected override void ParseRawRowCore()
		{
			base.ParseRawRowCore();
			SetField(ShipnetConstants.PartyDetails.PartyACNNumber, GetValue(ShipnetConstants.PartyDetails.PartyACNNumberPosition, ShipnetConstants.PartyDetails.PartyACNNumberMaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyFwdrRegNo, GetValue(ShipnetConstants.PartyDetails.PartyFwdrRegNoPosition, ShipnetConstants.PartyDetails.PartyFwdrRegNoMaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyReference, GetValue(ShipnetConstants.PartyDetails.PartyReferencePosition, ShipnetConstants.PartyDetails.PartyReferenceMaxLength));
		}
	}
}
