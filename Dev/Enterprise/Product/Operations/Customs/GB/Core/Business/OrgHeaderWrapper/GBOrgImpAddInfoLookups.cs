//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM EUOrgImpAddInfoLookups
//
//    This class should be used for overriding collections in EUOrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.EU.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business
{
	public class GBOrgImpAddInfoLookups : AutoGBOrgImpAddInfoLookups
	{
		public GBOrgImpAddInfoLookups(AutoGBOrgImpAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList DefermentMethodList => Factory.GetCachedValue<DefermentMethodList>();

		public CodeDescriptionPairList ClientsDucrSourceAttributeFieldList
		{
			get
			{
				var ca1 = FreightDataRegistry.Instance.ShipmentCustomText1.Value;
				var ca2 = FreightDataRegistry.Instance.ShipmentCustomText2.Value;
				var list = new CodeDescriptionPairList();
				if (ca1.Caption.IsEmpty && ca2.Caption.IsEmpty)
				{
					list.AddPair(ClientsDucrSourceAttributeFieldList_None, "None (set up values in Registry>" + FreightDataRegistry.Instance.ShipmentCustomText1.Category.Replace("/", ">") + ")");
				}
				else
				{
					list.AddPair(ClientsDucrSourceAttributeFieldList_None, "None");
				}

				if (!ca1.Caption.IsEmpty)
				{
					list.AddPair(ClientsDucrSourceAttributeFieldList_CA1, "Custom Attribute 1 (" + ca1.Caption + ")");
				}

				if (!ca1.Caption.IsEmpty)
				{
					list.AddPair(ClientsDucrSourceAttributeFieldList_CA2, "Custom Attribute 2 (" + ca2.Caption + ")");
				}

				return list;
			}
		}

		const string ClientsDucrSourceAttributeFieldList_None = "NON";
		public const string ClientsDucrSourceAttributeFieldList_CA1 = "CA1";
		public const string ClientsDucrSourceAttributeFieldList_CA2 = "CA2";
	}
}
