using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class LOCSegmentWithLotsOfLocations : Segment
	{
		public LOCSegmentWithLotsOfLocations()
		{
			MaximumNumberOfValues = 9;
		}

		public LocationAndRelationsAsASingleElement Location1 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location2 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location3 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location4 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location5 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location6 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location7 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location8 = new LocationAndRelationsAsASingleElement();
		public LocationAndRelationsAsASingleElement Location9 = new LocationAndRelationsAsASingleElement();

		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				Location1,
				Location2,
				Location3,
				Location4,
				Location5,
				Location6,
				Location7,
				Location8,
				Location9
			};
		}

		protected override void SetValues()
		{
			Location1.Parse(CharacterSet, Values(1));
			Location2.Parse(CharacterSet, Values(2));
			Location3.Parse(CharacterSet, Values(3));
			Location4.Parse(CharacterSet, Values(4));
			Location5.Parse(CharacterSet, Values(5));
			Location6.Parse(CharacterSet, Values(6));
			Location7.Parse(CharacterSet, Values(7));
			Location8.Parse(CharacterSet, Values(8));
			Location9.Parse(CharacterSet, Values(9));
		}

		protected override string SegmentNameOverride
		{
			get
			{
				return SegmentFakeName;
			}
		}

		public static string SegmentFakeName = "LOC-CCSUK";
	}
}
