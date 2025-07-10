using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Elements;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class BGMSegmentWithReferenceAndDates : Segment
	{
		public BGMSegmentWithReferenceAndDates()
		{
			MaximumNumberOfValues = 7;
		}

		public readonly DocumentMessageNameElements DocumentMessageName = new DocumentMessageNameElements();
		public readonly DocumentMessageIdentificationElements DocumentMessageIdentification = new DocumentMessageIdentificationElements();
		public readonly DateTimePeriodElements DateTimeC507_1 = new DateTimePeriodElements();
		public string XMessageFunctionCode;
		public readonly ReferenceElements ReferenceC506 = new ReferenceElements();
		public readonly DateTimePeriodElements DateTimeC507_2 = new DateTimePeriodElements();

		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				DocumentMessageName,
				DocumentMessageIdentification,
				DateTimeC507_1,
				XMessageFunctionCode,
				ReferenceC506,
				DateTimeC507_2
			};
		}

		protected override void SetValues()
		{
			DocumentMessageName.Parse(CharacterSet, Values(1));
			DocumentMessageIdentification.Parse(CharacterSet, Values(2));
			DateTimeC507_1.Parse(CharacterSet, Values(3));
			XMessageFunctionCode = Values(4);
			ReferenceC506.Parse(CharacterSet, Values(5));
			DateTimeC507_2.Parse(CharacterSet, Values(6));
		}

		protected override string SegmentNameOverride
		{
			get
			{
				return SegmentFakeName;
			}
		}

		public static string SegmentFakeName = "BGM-CCSUK";
	}
}
