using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class EntryNumberFormatterForNctsAndDeclarationIntegrationTest :
		EntryNumberFormatterForNctsAndDeclarationIntegrationTestBase<JobComInvoiceLine, EntryNumberFormatterForNctsAndDeclarationIntegration>
	{
		public override void TestFormatEntryNumber()
		{
			var formatter = new EntryNumberFormatterForNctsAndDeclarationIntegration();
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryNum = "E1234";
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.N830, "E1234", null), formatter.FormatEntryNumber(cusEntryNumber, ZString.Empty));
		}
	}
}
