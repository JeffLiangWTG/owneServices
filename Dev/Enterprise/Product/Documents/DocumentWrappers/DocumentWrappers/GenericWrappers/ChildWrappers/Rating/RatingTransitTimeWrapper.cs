using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("FriendlyText")]
	[WrapperTypeName("Rating Transit Time")]
	public class RatingTransitTimeWrapper : GenericWrapper
	{
		public RatingTransitTimeWrapper(ZString value, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Value = value;
		}

		public ZString Value { get; private set; }

		public ZString FriendlyText
		{
			get
			{
				switch (Value)
				{
					case "":
						return "";

					case RatingConstants.TransitTimes.Overnight:
						return Res.GetString("c5a47693-283b-4888-9850-6933ebb816ff", "Overnight");

					case RatingConstants.TransitTimes.SameDay:
						return Res.GetString("7debf56a-febb-4e9b-873b-3b3e16baf705", "Same Day");

					case "1":
						return Res.GetString("53d6ea3a-a035-4f1f-a4ec-05367b560438", "1 Day");

					default:
						return Res.GetString("f12ff261-6021-4307-b322-4d63640a01f9", "{0} Days", Value);
				}
			}
		}
	}
}
