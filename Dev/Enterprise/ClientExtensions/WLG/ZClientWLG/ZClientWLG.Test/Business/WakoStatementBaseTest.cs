using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.WLG
{
	abstract class WakoStatementBaseTest : StatementBaseTest
	{
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			ClientDocumentTestHelper.SetupClientDocumentsFromSupplementaryContentPath("WLG");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return WakoStatement.New(GlbBranch.CurrentBranch);
		}

		protected override ZBool ExpectedIsClientSpecific
		{
			get
			{
				return ZBool.True;
			}
		}
		#endregion
	}
}
