using System.Collections.Generic;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class DOCSegmentWith8Elements : DOCSegment
	{
		public DOCSegmentWith8Elements()
		{
			MaximumNumberOfValues = 8;
		}

		public string Element7;
		public string Element8;

		protected override void GetValues()
		{
			base.GetValues();
			List<object> baseItems = new List<object>();
			baseItems.AddRange(ValueItems);
			baseItems.Add(Element7);
			baseItems.Add(Element8);
			ValueItems = baseItems.ToArray();
		}

		protected override void SetValues()
		{
			base.SetValues();
			Element7 = Values(7);
			Element8 = Values(8);
		}

		protected override string SegmentNameOverride
		{
			get
			{
				return SegmentFakeName;
			}
		}

		public static string SegmentFakeName = "DOC-CCSUK";
	}
}
