using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class LocationAndRelationsAsASingleElement : ValueBase
	{
		public LocationAndRelationsAsASingleElement()
		{
			MaximumNumberOfValues = 9;
		}

		public string PlaceLocationQualifier3227;
		public string PlaceLocationIdentification3225;
		public string CodeListQualifier1131_1;
		public string CodeListResponsibleAgencyCoded3055_1;
		public string PlaceLocation3224;
		public string SubLocationIdentification3439;
		public string CodeListQualifier1131_2;
		public string CodeListResponsibleAgencyCoded3055_2;
		public string SubLocation3438;

		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				PlaceLocationQualifier3227,
				PlaceLocationIdentification3225,
				CodeListQualifier1131_1,
				CodeListResponsibleAgencyCoded3055_1,
				PlaceLocation3224,
				SubLocationIdentification3439   ,
				CodeListQualifier1131_2,
				CodeListResponsibleAgencyCoded3055_2,
				SubLocation3438
			};
		}

		protected override void SetValues()
		{
			PlaceLocationQualifier3227 = Values(0);
			PlaceLocationIdentification3225 = Values(1);
			CodeListQualifier1131_1 = Values(2);
			CodeListResponsibleAgencyCoded3055_1 = Values(3);
			PlaceLocation3224 = Values(4);
			SubLocationIdentification3439 = Values(5);
			CodeListQualifier1131_2 = Values(6);
			CodeListResponsibleAgencyCoded3055_2 = Values(7);
			SubLocation3438 = Values(8);
		}
	}
}
