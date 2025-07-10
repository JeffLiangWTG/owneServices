
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IContactDetails
	{
		#region Custom Fields

		ZString PostalAddress { get; }
		ZString PostalAddressInEnglish { get; }
		ZString Name { get; }

		#endregion

		ZString Code { get; }
		ZString AttachmentType { get; }
		ZDateTime Birthday { get; }
		ZString ContactName { get; }
		ZString Email { get; }
		ZString Fax { get; }
		ZString HomePhone { get; }
		ZString Language { get; }
		ZString Mobile { get; }
		ZString NotifyMode { get; }
		DocAddress OrgAddress { get; }
		DocOrganisation Organisation { get; }
		DocOrganisation AddressOverride { get; }
		ZString OtherPhone { get; }
		ZString Pager { get; }
		ZString Password { get; }
		ZString PersonalInfo { get; }
		ZString Phone { get; }
		ZString Title { get; }
	}
}
