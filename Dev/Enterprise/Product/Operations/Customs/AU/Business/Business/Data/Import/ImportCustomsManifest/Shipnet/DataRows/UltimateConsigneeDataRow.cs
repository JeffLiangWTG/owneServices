
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UltimateConsigneeDataRow : BasePartyDetailsDataRow
	{
		public UltimateConsigneeDataRow(ZString rawRow)
			: base(rawRow)
		{
		}

		protected override void ParseRawRowCore()
		{
			base.ParseRawRowCore();
			SetField(ShipnetConstants.PartyDetails.PartyACNNumber, GetValue(ShipnetConstants.PartyDetails.PartyACNNumberPosition, ShipnetConstants.UltimateConsigneeDetails.PartyACNNumberMaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyFwdrRegNo, GetValue(ShipnetConstants.UltimateConsigneeDetails.PartyFwdrRegNoPosition, ShipnetConstants.PartyDetails.PartyFwdrRegNoMaxLength));
			SetField(ShipnetConstants.PartyDetails.PartyReference, GetValue(ShipnetConstants.UltimateConsigneeDetails.PartyReferencePosition, ShipnetConstants.PartyDetails.PartyReferenceMaxLength));
		}
	}
}
