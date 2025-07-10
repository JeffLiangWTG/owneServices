using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.CusTempStorage.Testing;

class CusTempStorageRegLineTest : TestCaseWithFactory
{
	public void TestCusTempStorageRegLineTransactions_OnDeleteKeyPressed()
	{
		header.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
		line.CusTempStorageRegLineTransactions.SetReadOnlyIncludingChildren(false);
		var transaction1 = Factory.New<CusTempStorageRegLineTransaction>();
		transaction1.SRT_PackageQty = 1;
		var transaction2 = Factory.New<CusTempStorageRegLineTransaction>();
		transaction2.SRT_PackageQty = 5;
		line.CusTempStorageRegLineTransactions.Add(transaction1);
		AssertEquals("SRL_PackagesRemaining added by 1", 1, line.SRL_PackagesRemaining);
		line.CusTempStorageRegLineTransactions.Add(transaction2);
		AssertEquals("SRL_PackagesRemaining added by 5", 6, line.SRL_PackagesRemaining);

		using (var form = new ZForm(Factory.New<CusTempStorageRegHeader>()))
		using (var transactionsGrid = new ZGrid())
		{
			transactionsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(CusTempStorageRegLineTransactionSchema.Constants.SRT_PackageQty, 50));
			form.Controls.Add(transactionsGrid);
			transactionsGrid.SetDataBinding(header, "CusTempStorageRegLines.CusTempStorageRegLineTransactions");
			form.Show();
			transactionsGrid.Select(GetIndex(transaction2));
			transactionsGrid.OnDeleteKeyPressed();
			AssertEquals("SRL_PackagesRemaining reduced by 5", 1, line.SRL_PackagesRemaining);

			int GetIndex(CusTempStorageRegLineTransaction bobj)
			{
				return transactionsGrid.ListManager.List.IndexOf(bobj);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
	}
	CusTempStorageRegHeader header;
	CusTempStorageRegLine line;
}
