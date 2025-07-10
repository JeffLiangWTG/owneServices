using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business
{
	public class EntryNumberFormatterForNctsAndDeclarationIntegration : IEntryNumberFormatterForNctsAndDeclarationIntegration
	{
		public (ZString Class, ZString Type, ZString Reference, ZInt? EntryLineNumber) FormatEntryNumber(CusEntryNumber sourceEntryNumber, ZString jobType)
		{
			var reference = sourceEntryNumber.CE_EntryNum;
			if (sourceEntryNumber.CE_EntryType == JobMessageTypeList.Codes.Import || sourceEntryNumber.CE_EntryType == JobMessageTypeList.Codes.Export)
			{
				reference = $"{sourceEntryNumber.CE_EntryNum}-{sourceEntryNumber.CE_IssueDate:dd-MM-yyyy}";
				if (!sourceEntryNumber.CE_EntryLineReference.IsEmpty)
				{
					reference += $"/{sourceEntryNumber.CE_EntryLineReference}";
				}
			}
			return (PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.Other, reference, null);
		}
	}
}
