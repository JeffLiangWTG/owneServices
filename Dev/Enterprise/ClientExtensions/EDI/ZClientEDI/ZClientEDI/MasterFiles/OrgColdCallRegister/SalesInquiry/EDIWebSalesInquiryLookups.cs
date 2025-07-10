using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIWebSalesInquiryLookups : SalesEnquiryLookups
	{
		public EDIWebSalesInquiryLookups(EDIWebSalesInquiry parent)
			: base(parent)
		{
		}

		#region Job Roles

		public CodeDescriptionPairList JobRoleList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddRange(EDIDataRegistry.Instance.UserRegistrationJobRoleList.Value);
				return list;
			}
		}

		#endregion

		#region Company Sizes

		public CodeDescriptionPairList CompanySizeList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddRange(EDIDataRegistry.Instance.UserRegistrationCompanySizeList.Value);
				return list;
			}
		}

		#endregion

		#region Types of Business

		public CodeDescriptionPairList TypeOfBusinessList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddRange(EDIDataRegistry.Instance.UserRegistrationTypeOfBusinessList.Value);
				return list;
			}
		}

		#endregion

		#region Reasons of Requesting Access

		public CodeDescriptionBoolCollection ReasonForRequestingAccessList
		{
			get
			{
				CodeDescriptionBoolCollection list = new CodeDescriptionBoolCollection();
				list.AddRange(EDIDataRegistry.Instance.UserRegistrationReasonForRequestingAccessList.Value);
				return list;
			}
		}

		#endregion

		#region Countries

		public RefCountryCollection Countries
		{
			get
			{
				return Factory.GetCachedValue("EDIWebSalesInquiryLookups.Countries", () =>
					{
						RefCountryCollection result = new RefCountryCollection(Factory);
						result.AdditionalFilter.AddToFilter(RefCountrySchema.RN_IsActive, true);
						return result;
					});
			}
		}

		#endregion
	}
}
