using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CustomsEntryWrapperFromCusEntryHeader : CustomsEntryWrapper
	{
		public CustomsEntryWrapperFromCusEntryHeader(CusEntryHeader entryHeaderBO, BusinessObjectFactory factory)
			: base(entryHeaderBO, factory)
		{
			EntryHeaderBO = entryHeaderBO ?? Factory.GetNull<CusEntryHeader>();
		}
		readonly CusEntryHeader EntryHeaderBO;

		protected override CodeAndDescriptionWrapper GetEntryType()
		{
			return new CodeAndDescriptionWrapper(EntryHeaderBO.CH_MessageType, EntryHeaderBO.Lookups.CH_MessageTypeList, Factory);
		}

		protected override ZString GetEntryNumber()
		{
			return EntryHeaderBO.EntryNumber;
		}

		protected override ZString GetInformation()
		{
			return EntryHeaderBO.CH_BGMReference;
		}

		protected override ZDateTime GetIssueDate()
		{
			return EntryHeaderBO.CH_EntryReleaseDate;
		}

		protected override ZString GetEntryCategory()
		{
			return EntryHeaderBO.CusEntryNumber != null ? EntryHeaderBO.CusEntryNumber.CE_Category : base.GetEntryCategory();
		}
	}
}
