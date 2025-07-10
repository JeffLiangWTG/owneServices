using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class StatementRunDocsTest : BaseRunDocumentsTest
	{
		public StatementRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return new PrintStatement(Factory, GlbBranch.CurrentBranch) { CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency }; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Statement; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestStatementOfAccount()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Statement Of Account");
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
