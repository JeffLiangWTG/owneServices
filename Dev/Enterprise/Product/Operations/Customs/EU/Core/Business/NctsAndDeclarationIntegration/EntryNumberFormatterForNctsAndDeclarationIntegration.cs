using CargoWise.Types;
using Enterprise.Customs.Common;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration
{
	public class EntryNumberFormatterForNctsAndDeclarationIntegration : IEntryNumberFormatterForNctsAndDeclarationIntegration
	{
		public (ZString Class, ZString Type, ZString Reference, ZInt? EntryLineNumber) FormatEntryNumber(CusEntryNumber sourceEntryNumber, ZString jobType)
			=> (PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.N830, sourceEntryNumber.CE_EntryNum, null);
	}
}
