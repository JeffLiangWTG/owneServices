using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("FriendlyText")]
	[WrapperTypeName("Rating Frequency")]
	public class RatingFrequencyWrapper : GenericWrapper
	{
		public RatingFrequencyWrapper(ZInt value, ZString code, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Value = value;
			Unit = new UnitWrapper(code, Factory.GetCachedValue<FrequencyList>(), factory);
		}

		public ZInt Value { get; private set; }
		public UnitWrapper Unit { get; private set; }

		public ZString FriendlyText
		{
			get
			{
				if (Value == 0)
				{
					return ZString.Empty;
				}
				else
				{
					ZString freqUnit = Unit.Code;

					switch (freqUnit.ToUpper())
					{
						case "DAILY":
							return Res.GetString("bc8b1c45-cc3a-41c7-ae03-3270d3aa590a", "{0} per Day", Value);

						case "DAYS":
							if (Value == 1)
							{
								return Res.GetString("e257c386-5574-4512-bf86-f8a71da4e074", "Every Day");
							}
							else
							{
								return Res.GetString("75921972-14f7-46c7-b011-eda7a44c7f1d", "Every {0} Days", Value);
							}

						case "WEEK":
							return Res.GetString("a2386acb-4b11-4caf-a774-1010632186d4", "{0} per Week", Value);

						case "FORTNIGHT":
							return Res.GetString("0a2e4999-f8a4-4678-9c39-1a1c39f5e756", "{0} per Fortnight", Value);

						case "MONTHLY":
							return Res.GetString("f5052b6e-f801-408d-9ebd-36119c133964", "{0} per Month", Value);

						default:
							return ZString.Empty;
					}
				}
			}
		}
	}
}
