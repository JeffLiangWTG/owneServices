using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class DepositSlipRunDocsTest : BaseRunDocumentsTest
	{
		public DepositSlipRunDocsTest() { }
		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New<DepositBatch>(); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Test; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestDepositSlipBankForAccounting()
		{
			CreateDocumentMenuAndSetPivot("Deposit Slip Bank Test", "Deposit Slip Bank", nameof(Core.Constants.DataContext.DepositBatch));
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Deposit Slip Bank Test");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDepositSlipOfficeForAccounting()
		{
			CreateDocumentMenuAndSetPivot("Deposit Slip Office Test", "Deposit Slip Office", nameof(Core.Constants.DataContext.DepositBatch));
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Deposit Slip Office Test");
			RunDocument();
		}
	}
}
