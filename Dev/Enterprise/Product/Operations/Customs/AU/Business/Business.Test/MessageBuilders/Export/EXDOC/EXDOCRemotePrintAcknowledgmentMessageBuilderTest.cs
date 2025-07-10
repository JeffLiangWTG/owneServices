using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCRemotePrintAcknowledgmentMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateMessage()
		{
			builder.GenerateMessage();
			AssertMultilineEquals("Check message matches",
				"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM++219873+99'UNT+3+<<MSGNO PLACEHOLDER>>'",
				builder.sancrtMessage.ToString(new Edifact.UNOACharacterSet()),
				'\'');
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			helper.Header1.JobComInvoiceLines.RemoveAndDeleteAll();
			builder = new EXDOCRemotePrintAcknowledgmentMessageBuilder(helper.Header1.QuarantineExDocHeader, "219873");
		}
		EXDOCRemotePrintAcknowledgmentMessageBuilder builder;
	}
}
