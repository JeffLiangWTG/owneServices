using System.Collections.Generic;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class BGMSegmentWithReferenceAndDatesAnd4343 : BGMSegmentWithReferenceAndDates
	{
		public BGMSegmentWithReferenceAndDatesAnd4343()
		{
			MaximumNumberOfValues = 7;
		}

		public string DocumentTypeCode4343;

		protected override void GetValues()
		{
			base.GetValues();
			List<object> baseItems = new List<object>();
			baseItems.AddRange(ValueItems);
			baseItems.Add(DocumentTypeCode4343);
			ValueItems = baseItems.ToArray();
		}

		protected override void SetValues()
		{
			base.SetValues();
			DocumentTypeCode4343 = Values(6);
		}
	}
}
