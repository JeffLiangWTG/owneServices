using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderMessageSendingObjectLookups : ZLookups
	{
		public NctsHeaderMessageSendingObjectLookups(NctsHeaderMessageSendingObject parent) : base(parent)
		{
		}

		public new NctsHeaderMessageSendingObject Parent => (NctsHeaderMessageSendingObject)base.Parent;

		public CodeDescriptionPairList MessageTypeList
		{
			get
			{
				var nctsHeader = Parent.NctsHeader;
				return nctsHeader.Configuration.MessageSendingConfiguration.MessageTypeList(nctsHeader);
			}
		}

		public CodeDescriptionPairList ReleaseRequestedFlags => Factory.GetCachedValue<ReleaseRequestedFlagList>();

		public ConsigneeCollection Consignees => new ConsigneeCollection(Factory);

		public CustomsOfficeCodeCollection DestinationCustomsOfficeCodeList => GetCustomsOfficeCodeList(Parent.NctsHeader.GetRolesForDestinationOfficeLookup());

		public CustomsOfficeCodeCollection DepartureOfficeOfEnquiryCodeList => GetCustomsOfficeCodeList(OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

		CustomsOfficeCodeCollection GetCustomsOfficeCodeList(params ZString[] roles)
		{
			var customsOffices = EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(Factory, roles);
			customsOffices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZZRefCusCodeListFilters.ListType, "Property", new ZString(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice)));
			customsOffices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZZRefCusCodeListFilters.CountryOrGrouping, "Property", Parent.NctsHeader.DefaultDataGroupingCode));

			return customsOffices;
		}
	}
}
