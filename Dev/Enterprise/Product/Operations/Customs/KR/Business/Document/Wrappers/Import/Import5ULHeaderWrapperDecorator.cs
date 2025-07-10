using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public static class Import5ULHeaderWrapperDecorator
	{
		public static void Decorate(this Import5ULHeaderWrapper wrapper, CusEntryHeader entry)
		{
			wrapper.Declarant = new OrganizationDocWrapper(entry.Declaration.BrokerAddress?.Header);
			wrapper.EntryReleaseDate = entry.CH_EntryReleaseDate;
			var entryNumber5UL = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);
			if (entryNumber5UL != null)
			{
				wrapper.EntryStatus = entryNumber5UL.CE_EntryStatus;
				wrapper.IssueDateTo5UL = entryNumber5UL.CE_IssueDate;
			}
		}
	}
}
