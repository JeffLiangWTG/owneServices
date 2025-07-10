using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OrganisationTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		#region GetCodeDescriptionPairList

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("LCI", ResString.GetMultilingualString("740b6966-ab86-4966-8f1d-6dfa88e89ab6", "Consignee / Importer"));
			list.AddPair("LCF", ResString.GetMultilingualString("0e34494d-361e-4918-9345-87cdcd636abc", "CFS / Depot"));
			list.AddPair("LCT", ResString.GetMultilingualString("de611653-8c97-4f93-a423-084cbcd392ff", "CTO / Wharf"));
			list.AddPair("LCW", ResString.GetMultilingualString("92664b90-e269-481a-859f-329167e18e4c", "Warehouse"));
			list.AddPair("LCY", ResString.GetMultilingualString("49cd3901-05ac-4b5f-b5e2-5d8d4a7a6f58", "Container Yard"));
			return list;
		}

		#endregion
	}
}
