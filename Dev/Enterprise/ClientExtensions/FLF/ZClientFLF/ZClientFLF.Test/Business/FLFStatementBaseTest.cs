using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.FLF.Testing
{
	abstract class FLFStatementBaseTest : StatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			ClientDocumentTestHelper.SetupClientDocumentsFromSupplementaryContentPath("FLF");
		}

		protected override BusinessObject GetNewBusinessObject() => FLFStatement.New(GlbBranch.CurrentBranch);

		protected override ZBool ExpectedIsClientSpecific => ZBool.True;
	}
}
