using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class OpportunitySourceRelatedItemProvider
	{
#if DEBUG
		internal
#endif
		Dictionary<string, RegistryBusinessObjectCollection> Mapping
		{
			get
			{
				if (mapping == null)
				{
					mapping = new Dictionary<string, RegistryBusinessObjectCollection>();
					mapping.Add("CL2", OrganisationsDataRegistry.Instance.CampaignCategory2List.Value);
				}
				return mapping;
			}
		}
		Dictionary<string, RegistryBusinessObjectCollection> mapping;

		public static ReadOnlyCodeDescriptionPairList SecondaryList
		{
			get
			{
				if (secondaryList == null)
				{
					secondaryList = new CodeDescriptionPairList();
					//code and description must be switched as there is no drop down control which supports showing only description
					secondaryList.AddPair(ResString.GetMultilingualString("78beefdd-0674-4282-aa12-75fb91ca12ce", "Campaign Management - Category List 2"), "CL2");
				}
				return secondaryList;
			}
		}
		static CodeDescriptionPairList secondaryList;

		public CodeDescriptionPairList GetRelatedItemList(ZString code)
		{
			CodeDescriptionPairList result = null;
			RegistryBusinessObjectCollection value;
			if (Mapping.TryGetValue(code, out value))
			{
				result = value.GetCodeDescriptionPairList();
			}
			return result;
		}

		public CodeDescriptionPairList GetActiveRelatedItemList(ZString code)
		{
			CodeDescriptionPairList result = null;
			RegistryBusinessObjectCollection value;
			if (Mapping.TryGetValue(code, out value))
			{
				CodeDescriptionBoolCollection collection = value as CodeDescriptionBoolCollection;
				if (collection != null)
				{
					result = collection.GetActiveCodeDescriptionPairList();
				}
				else
				{
					result = value.GetCodeDescriptionPairList();
				}
			}
			return result;
		}
	}
}
