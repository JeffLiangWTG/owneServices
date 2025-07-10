using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public class LocationOfGoodsTypeConverter : EnumConverter<LocationOfGoodsType>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"ARR",
				"DEC",
				"DEP",
				"CEI",
				"TST",
				"INS",
				"EXT",
				"CPR"
			};
		}

		protected override LocationOfGoodsType[] GetEnumValues()
		{
			return new LocationOfGoodsType[]
			{
				LocationOfGoodsType.Arrival,
				LocationOfGoodsType.Declaration,
				LocationOfGoodsType.Departure,
				LocationOfGoodsType.EntryInstruction,
				LocationOfGoodsType.TemporaryStorage,
				LocationOfGoodsType.Inspection,
				LocationOfGoodsType.ExitControl,
				LocationOfGoodsType.CusPermitRule
			};
		}
	}
}
