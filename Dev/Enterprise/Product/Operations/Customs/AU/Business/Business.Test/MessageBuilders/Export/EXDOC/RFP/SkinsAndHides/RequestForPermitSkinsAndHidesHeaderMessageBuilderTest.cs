using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitSkinsAndHidesHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateDTM()
		{
			quarantineHeader.QH_PackDate = new ZDateTime(2006, 12, 12);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Pack Date Test", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+S::AQ:9++13'DTM+365:20061212:102'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var invoiceHeader = helper.Header1;
			invoiceHeader.JobComInvoiceLines.RemoveAndDeleteAll();
			quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			builder = new RequestForPermitSkinsAndHidesHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);
		}
		QuarantineExDocHeader quarantineHeader;
		RequestForPermitSkinsAndHidesHeaderMessageBuilder builder;
	}
}
