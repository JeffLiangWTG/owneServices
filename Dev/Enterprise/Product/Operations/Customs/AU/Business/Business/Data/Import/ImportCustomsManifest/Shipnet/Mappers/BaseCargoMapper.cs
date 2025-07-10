using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseCargoMapper : BaseMapper
	{
		public BaseCargoMapper(ZString cargoType)
			: base()
		{
			this.cargoType = cargoType;
		}

		internal void FromCargoGroupDescriptionsRow(FlatFileDataRow row)
		{
			goodsDescription = row[ShipnetConstants.CargoDescriptions.Description];
			marksAndNumbers = row[ShipnetConstants.CargoDescriptions.Marks];
		}

		internal void ForDescriptionRow(FlatFileDataRow row)
		{
			switch (row[ShipnetConstants.Common.RecordID])
			{
				case ShipnetConstants.Codes.RecordID.CargoGroupDescriptions:
					FromCargoGroupDescriptionsRow(row);
					break;

				case ShipnetConstants.Codes.RecordID.CargoItemDescriptions:
					if (goodsDescription.IsEmpty && marksAndNumbers.IsEmpty)
					{
						FromCargoGroupDescriptionsRow(row);
					}
					break;
			}
		}

		internal readonly ZString cargoType;
		internal ZString goodsDescription;
		internal ZString marksAndNumbers;
	}
}
