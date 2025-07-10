using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ParentJobTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		#region GetCodeDescriptionPairList

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("NON", ResString.GetMultilingualString("a5f8bbc4-06f5-4c47-911c-4cec7152cf06", "Standalone Booking (No Parent Job)"));
			list.AddPair("SHP", ResString.GetMultilingualString("b4a4194d-ab58-47cd-aaac-3e79df28953c", "Forwarding Shipment"));
			list.AddPair("ASH", ResString.GetMultilingualString("0671f7d2-8fd3-4c59-ad26-df3bd2490ec0", "Agency Shipment"));
			list.AddPair("WHR", ResString.GetMultilingualString("adcfd6c3-08e1-4a3e-8f9f-0a0d4154c0da", "Warehouse Receive"));
			list.AddPair("WHO", ResString.GetMultilingualString("499d6b2c-392d-4abf-bdb2-a9a7275394f2", "Warehouse Order"));
			list.AddPair("CUS", ResString.GetMultilingualString("3c100b23-09d7-438d-8af3-5d658a71ae6e", "Customs Declaration"));
			return list;
		}

		#endregion
	}
}
