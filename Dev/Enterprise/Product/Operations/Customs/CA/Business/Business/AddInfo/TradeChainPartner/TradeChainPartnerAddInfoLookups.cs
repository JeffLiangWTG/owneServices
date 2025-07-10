//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTradeChainPartnerAddInfoLookups
//
//    This class should be used for overriding collections in AutoTradeChainPartnerAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TradeChainPartnerAddInfoLookups : AutoTradeChainPartnerAddInfoLookups
	{
		public TradeChainPartnerAddInfoLookups(AutoTradeChainPartnerAddInfo parent) : base(parent)
		{
		}

		public ConsigneeOrConsignorCollection ImportersList
		{
			get
			{
				if (importersList == null)
				{
					importersList = new ConsigneeOrConsignorCollection(Factory);
				}
				return importersList;
			}
		}

		protected new TradeChainPartnerAddInfo Parent => (TradeChainPartnerAddInfo)base.Parent;

		protected ConsigneeOrConsignorCollection importersList;

		public CodeDescriptionPairList CSAIDTypeList
		{
			get
			{
				switch (Parent.CA_Type)
				{
					case TradeChainPartnersTypeList.Codes.V:
						return CSAVendorIDTypeList;
					case TradeChainPartnersTypeList.Codes.C:
						return CSAConsigneeIDTypeList;
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		CodeDescriptionPairList CSAVendorIDTypeList
		{
			get
			{
				return Factory.GetCachedValue("CSAVendorIDTypeList" + Core.Constants.CountryCodes.Canada, delegate
				{
					return new CSAVendorIDTypeList();
				});
			}
		}

		CodeDescriptionPairList CSAConsigneeIDTypeList
		{
			get
			{
				return Factory.GetCachedValue("CSAConsigneeIDTypeList" + Core.Constants.CountryCodes.Canada, () =>
				{
					return new CSAConsigneeIDTypeList();
				});
			}
		}

		public TradeChainPartnersTypeList TradeChainPartnersTypeList
		{
			get
			{
				return Factory.GetCachedValue("TradeChainPartnersTypeList" + Core.Constants.CountryCodes.Canada, () =>
				{
					return new TradeChainPartnersTypeList();
				});
			}
		}

		public CSAStatusList CSAStatusList
		{
			get
			{
				return Factory.GetCachedValue("CSAStatusList" + Core.Constants.CountryCodes.Canada, () =>
				{
					return new CSAStatusList();
				});
			}
		}

		public CSAActionTypeList CSAActionTypeList
		{
			get
			{
				return Factory.GetCachedValue("CSAActionTypeList" + Core.Constants.CountryCodes.Canada, () =>
				{
					return new CSAActionTypeList();
				});
			}
		}

		public CodeDescriptionPairList CSAStatusEditableByUserList
		{
			get
			{
				return Factory.GetCachedValue("CSAStatusEditableByUserList" + Core.Constants.CountryCodes.Canada, () =>
				{
					return CSAStatusList.GetEditableCSAStatusList();
				});
			}
		}
	}
}
