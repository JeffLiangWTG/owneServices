using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public class LegTypeConverter : EnumConverter<LegType>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"OTH",
				"FL1",
				"FL2",
				"FL3",
				"MAI",
				"ONF",
				"PRE",
				"LTL",
			};
		}

		protected override LegType[] GetEnumValues()
		{
			return new LegType[]
			{
				LegType.Other,
				LegType.Flight1,
				LegType.Flight2,
				LegType.Flight3,
				LegType.Main,
				LegType.OnForwarding,
				LegType.PreCarriage,
				LegType.LocalTransport,
			};
		}
	}
}
