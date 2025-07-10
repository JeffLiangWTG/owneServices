using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class EntryNumberFormatterForNctsAndDeclarationIntegrationTest :
		EntryNumberFormatterForNctsAndDeclarationIntegrationTestBase<JobComInvoiceLine, EntryNumberFormatterForNctsAndDeclarationIntegration>
	{
		public override void TestFormatEntryNumber()
		{
			var formatter = new EntryNumberFormatterForNctsAndDeclarationIntegration(Factory);

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryNum = "E1234";
			cusEntryNumber.CE_ParentTable = "JobDeclaration";

			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, PreviousDocumentCodeList.Codes._820, "E1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5));
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, PreviousDocumentCodeList.Codes._820, "E1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4));
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, PreviousDocumentCodeList.Codes._821, "E1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure));
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, PreviousDocumentCodeList.Codes._822, "E1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure));
			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, PreviousDocumentCodeList.Codes._952, "E1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, NctsTypeOfDeclaration.Codes.TirDeclaration));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F65", "", "EXP", "10P");
			Factory.Save();

			AssertTupleEquals((PreviousDocumentClassList.Codes.PreviousDocument, "EX", "E1234", null),
				formatter.FormatEntryNumber(cusEntryNumber, "10P"));
		}
	}
}
