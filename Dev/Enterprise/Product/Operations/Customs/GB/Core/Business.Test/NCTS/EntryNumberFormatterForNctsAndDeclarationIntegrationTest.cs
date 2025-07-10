using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Test
{
	public class EntryNumberFormatterForNctsAndDeclarationIntegrationTest :
		EntryNumberFormatterForNctsAndDeclarationIntegrationTestBase<JobComInvoiceLine, EntryNumberFormatterForNctsAndDeclarationIntegration>
	{
		public override void TestFormatEntryNumber()
		{
			var formatter = new EntryNumberFormatterForNctsAndDeclarationIntegration();

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = JobMessageTypeList.Codes.Import;
			cusEntryNumber.CE_EntryNum = "E1234";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2022, 7, 1);
			cusEntryNumber.CE_EntryLineReference = "L1234";
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.Other, "E1234-01-07-2022/L1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, ZString.Empty));

			cusEntryNumber.CE_EntryType = JobMessageTypeList.Codes.Import;
			cusEntryNumber.CE_EntryNum = "E1234";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2022, 7, 1);
			cusEntryNumber.CE_EntryLineReference = ZString.Empty;
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.Other, "E1234-01-07-2022", null),
				formatter.FormatEntryNumber(cusEntryNumber, ZString.Empty));

			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.EU.MasterUCR;
			cusEntryNumber.CE_EntryNum = "E1234";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2022, 7, 1);
			cusEntryNumber.CE_EntryLineReference = ZString.Empty;
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.Other, "E1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, ZString.Empty));
		}
	}
}
