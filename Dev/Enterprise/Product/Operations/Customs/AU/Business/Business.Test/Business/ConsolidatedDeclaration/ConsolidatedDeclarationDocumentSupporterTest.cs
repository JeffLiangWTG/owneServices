using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationDocumentSupporter))]
	sealed class ConsolidatedDeclarationDocumentSupporterTest : BaseConsolidatedDeclarationDocumentSupporterTest
	{
		public void TestGetFilterValueMSGBKRCTY()
		{
			var consolidatedDeclaration = (ConsolidatedDeclaration)GetDocumentSupportableBusinessObject();
			Assertion.AssertEquals("For filter 'MSGBKRCTY' result is 'IMPAU'", "IMPAU", consolidatedDeclaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
		}

		public void TestGetDocBusinessObjects()
		{
			var consolidatedDeclaration = (ConsolidatedDeclaration)GetDocumentSupportableBusinessObject();
			var result = consolidatedDeclaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Docwrapper count = customs entry header count", 1, result.Length);
			AssertEquals("Docwrapper for data context CusEntryHeader is DocCusEntryHeader", mainNameSpace + "DocCusEntryHeader", result[0].GetType().ToString());
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryHeader = leadDeclaration.EntryHeader;
			entryHeader.EntryNumber = "AAA";
			Factory.Save();
			return consolidatedDeclaration;
		}

		readonly ZString mainNameSpace = "Enterprise.DocumentWrappers.Customs.AU.";
	}
}
