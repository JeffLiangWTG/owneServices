using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumber : IBusiness
		{
			ZString AdditionalReferenceNumberTypeDescription { get; }
			ZPropertyInfo AdditionalReferenceNumberTypeDescriptionInfo { get; }

			ZString CE_Category { get; set; }
			ZPropertyInfo CE_CategoryInfo { get; }

			ZString CE_EntryLineReference { get; set; }
			ZPropertyInfo CE_EntryLineReferenceInfo { get; }

			ZString CE_EntryNum { get; set; }
			ZPropertyInfo CE_EntryNumInfo { get; }

			ZString CE_EntryStatus { get; set; }
			ZPropertyInfo CE_EntryStatusInfo { get; }

			ZDateTime CE_IssueDate { get; set; }
			ZPropertyInfo CE_IssueDateInfo { get; }

			ICusEntryNumLookups Lookups { get; }
			BusinessObject Parent { get; set; }

			[List("Lookups.AdditionalReferenceNumberTypes")]
			ZString CE_EntryType { get; set; }

			ZBool CE_EntryIsSystemGenerated { get; set; }

			ZPropertyInfo CE_EntryTypeInfo { get; }

			[List("Lookups.Countries")]
			ZString CE_RN_NKCountryCode { get; set; }

			ZPropertyInfo CE_RN_NKCountryCodeInfo { get; }
		}
	}
}
