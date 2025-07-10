
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class ChargeDetailDataRow : BaseDataRow
	{
		public ChargeDetailDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.ChargeDetail)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.ChargeDetail.ChargeCode, GetValue(ShipnetConstants.ChargeDetail.ChargeCodePosition, ShipnetConstants.ChargeDetail.ChargeCodeMaxLength));
			SetField(ShipnetConstants.ChargeDetail.PrepaidCollect, GetValue(ShipnetConstants.ChargeDetail.PrepaidCollectPosition, ShipnetConstants.ChargeDetail.PrepaidCollectMaxLength));
			SetField(ShipnetConstants.ChargeDetail.Currency, GetValue(ShipnetConstants.ChargeDetail.CurrencyPosition, ShipnetConstants.ChargeDetail.CurrencyMaxLength));
			SetField(ShipnetConstants.ChargeDetail.Amount, GetValue(ShipnetConstants.ChargeDetail.AmountPosition, ShipnetConstants.ChargeDetail.AmountMaxLength));
		}
	}
}
