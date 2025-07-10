using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Elements;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class RFFSegmentWithDatetime : Segment
	{
		public RFFSegmentWithDatetime()
		{
			MaximumNumberOfValues = 3;
		}

		public readonly ReferenceElements Reference = new ReferenceElements();
		public DateTimePeriodElements DateTimeC507 = new DateTimePeriodElements();

		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				Reference,
				DateTimeC507
			};
		}

		protected override void SetValues()
		{
			Reference.Parse(CharacterSet, Values(1));
			DateTimeC507.Parse(CharacterSet, Values(2));
		}

		protected override string SegmentNameOverride
		{
			get
			{
				return SegmentFakeName;
			}
		}

		public static string SegmentFakeName = "RFF-CCSUK";
	}
}
