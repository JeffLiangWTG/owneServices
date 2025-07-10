using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Elements;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class MOASegmentWithFunction : Segment
	{
		public MOASegmentWithFunction()
		{
			MaximumNumberOfValues = 3;
		}

		public readonly MonetaryAmountElements MonetaryAmount = new MonetaryAmountElements();
		public string MonetaryFunctionQualifier;

		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				MonetaryFunctionQualifier,
				MonetaryAmount
			};
		}

		protected override void SetValues()
		{
			MonetaryFunctionQualifier = Values(1);
			MonetaryAmount.Parse(CharacterSet, Values(2));
		}

		protected override string SegmentNameOverride
		{
			get
			{
				return SegmentFakeName;
			}
		}

		public static string SegmentFakeName = "MOA-CCSUK";
	}
}
