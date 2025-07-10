
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoDescriptionsDataRow : BaseDataRow
	{
		public CargoDescriptionsDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.CargoDescriptions)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.CargoDescriptions.CGRSequence, GetValue(ShipnetConstants.CargoDescriptions.CGRSequencePosition, ShipnetConstants.CargoDescriptions.CGRSequenceMaxLength));
			SetField(ShipnetConstants.CargoDescriptions.Marks, GetValue(ShipnetConstants.CargoDescriptions.MarksPosition, ShipnetConstants.CargoDescriptions.MarksMaxLength));
			SetField(ShipnetConstants.CargoDescriptions.Description, GetValue(ShipnetConstants.CargoDescriptions.DescriptionPosition, ShipnetConstants.CargoDescriptions.DescriptionMaxLength));
		}
	}
}
