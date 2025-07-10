using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Core
{
	public interface IRefLocalLanguage
	{
		ZGuid PK { get; }
		ZString RA_Code { get; set; }
		ZString RA_RN_NKCountryCode { get; set; }
		ZString RA_Description { get; set; }
		ZBool RA_IsSystem { get; set; }
		ZBool RA_IsActive { get; set; }
		ZGuid RA_RA_ParentLanguage { get; set; }
		MultilingualString RA_DescriptionMultilingual { get; }
		ZString FullLanguageCode { get; }
		IRefLocalLanguage ParentLanguage { get; }
		ResourceLanguage GetResourceLanguage();
	}
}
