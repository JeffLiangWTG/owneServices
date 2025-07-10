using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CustomsEntryWrapperFromCusEntryNumber : CustomsEntryWrapper
	{
		public CustomsEntryWrapperFromCusEntryNumber(CusEntryNumber entryNumberBO, BusinessObjectFactory factory)
			: base(entryNumberBO, factory)
		{
			EntryNumberBO = entryNumberBO ?? Factory.GetNull<CusEntryNumber>();
		}
		protected readonly CusEntryNumber EntryNumberBO;

		protected override CodeAndDescriptionWrapper GetEntryType()
		{
			return new CodeAndDescriptionWrapper(CMRExportExemptionCodes.Get4CharCode(EntryNumberBO.CE_EntryType), EntryNumberBO.Lookups.AdditionalReferenceNumberTypes, Factory);
		}

		protected override ZString GetEntryNumber()
		{
			return EntryNumberBO.CE_EntryNum;
		}

		protected override ZString GetEntryCategory()
		{
			return EntryNumberBO.CE_Category;
		}

		protected override ZString GetInformation()
		{
			return EntryNumberBO.CE_EntryLineReference;
		}

		protected override ZDateTime GetIssueDate()
		{
			return EntryNumberBO.CE_IssueDate;
		}

		protected override ZString GetCountry()
		{
			return EntryNumberBO.CE_RN_NKCountryCode;
		}
	}
}
