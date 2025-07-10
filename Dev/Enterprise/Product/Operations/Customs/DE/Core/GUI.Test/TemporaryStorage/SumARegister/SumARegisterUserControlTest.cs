using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class SumARegisterUserControlTest : TestCaseWithFactory
	{
		public void TestCustomerReferenceTextBox()
		{
			using (var control = new SumARegisterUserControl())
			{
				var customerReferenceTextBox = control.CustomerReferenceTextBox;
				AssertEquals("CustomerReferenceTextBox is in HeadersGroupBox", true, control.HeadersGroupBox.Contains(customerReferenceTextBox));
				AssertEquals("BindTo", nameof(CusTempStorageRegHeader.SRH_InternalReference), customerReferenceTextBox.BindTo);
			}
		}

		public void TestLinesGridColumnSize()
		{
			using (var control = new SumARegisterUserControl())
			{
				var linesGrid = control.FindSingle<ZGrid>("LinesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Column Count", 14, linesGrid.ColumnStyles.Count);
					AssertEquals("SRL_LineNumber", 65, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LineNumber).Width);
					AssertEquals("SRL_OwnerReferenceType", 134, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType).Width);
					AssertEquals("SRL_OwnerReference", 149, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReference).Width);
					AssertEquals("SRL_LocationOfGoods", 110, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LocationOfGoods).Width);
					AssertEquals("SRL_GoodsDescription", 150, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsDescription).Width);
					AssertEquals("SRL_LimitDate", 73, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LimitDate).Width);
					AssertEquals("SRL_PackagesRemaining", 126, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_PackagesRemaining).Width);
					AssertEquals("SRL_PackageType", 90, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_PackageType).Width);
					AssertEquals("SRL_CustomsStatus", 98, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_CustomsStatus).Width);
					AssertEquals("SRL_CustodianIdentifier", 99, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_CustodianIdentifier).Width);
					AssertEquals("SRL_CustodianIdentifierBranchNo", 108, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_CustodianIdentifierBranchNo).Width);
					AssertEquals("SRL_GoodsOwnerIdentifier", 134, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsOwnerIdentifier).Width);
					AssertEquals("SRL_GoodsOwnerIdentifierBranchNo", 142, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsOwnerIdentifierBranchNo).Width);
					AssertEquals("SRL_UnionStatus", 85, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_UnionStatus).Width);
				});
			}
		}

		public void TestLinesGridColumnCaption()
		{
			using (var control = new SumARegisterUserControl())
			{
				var linesGrid = control.FindSingle<ZGrid>("LinesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("SRL_LineNumber", "Line No.", linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LineNumber).CaptionResourceString.Caption);
				});
			}
		}

		public void TestTransactionGridColumnCaptions()
		{
			using (var control = new SumARegisterUserControl())
			{
				var transactionGrid = control.FindSingle<ZGrid>("TransactionGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Column Count", 10, transactionGrid.ColumnStyles.Count);
					AssertEquals("SRT_TransactionType", "Transaction Type", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType).CaptionResourceString.Caption);
					AssertEquals("TransactionTypeDescription", "Transaction Type Description", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription).CaptionResourceString.Caption);
					AssertEquals("SRT_InternalReferenceNumber", "Internal Reference No.", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber).CaptionResourceString.Caption);
					AssertEquals("SRT_GrossWeight", "Gross Weight in KGs", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_GrossWeight).CaptionResourceString.Caption);
					AssertEquals("SRT_PackageQty", "Package Quantity", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_PackageQty).CaptionResourceString.Caption);
					AssertEquals("SRT_ReferenceType", "Reference Type", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType).CaptionResourceString.Caption);
					AssertEquals("SRT_Reference", "Reference Number", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Reference).CaptionResourceString.Caption);
					AssertEquals("SRT_Comments", "Comments", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Comments).CaptionResourceString.Caption);
				});
			}
		}

		public void TestCusTempStorageRegLineTransactions_OnDeleteKeyPressed()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
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

		public void TestOwnerReferenceNormalCasing()
		{
			using (var control = new SumARegisterUserControl())
			{
				var linesGrid = control.FindSingle<ZGrid>("LinesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("owners reference", CharacterCasing.Normal, control.FindSingle<ZTextBox>("OwnerReferenceNumberTextBox").CharacterCasing);
					AssertEquals("goods description", CharacterCasing.Normal, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReference).CharacterCasing);
				});
			}
		}

		public void TestGoodsDescriptionNormalCasing()
		{
			using (var control = new SumARegisterUserControl())
			{
				var linesGrid = control.FindSingle<ZGrid>("LinesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("owners reference", CharacterCasing.Normal, control.FindSingle<ZTextBox>("GoodsDescriptionTextBox").CharacterCasing);
					AssertEquals("goods description", CharacterCasing.Normal, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsDescription).CharacterCasing);
				});
			}
		}

		public void TestLineNumberCalcEditDecimals()
		{
			using (var control = new SumARegisterUserControl())
			{
				var lineNumberCalcEdit = control.FindSingle<ZCalcEdit>("LineNumberCalcEdit");
				CombineAssertions(() =>
				{
					AssertEquals("Decimal Places", 0, lineNumberCalcEdit.DecimalPlaces);
					AssertEquals("Decimals", 0, lineNumberCalcEdit.Decimals);
				});
			}
		}

		public void TestPresentationDateFormat()
		{
			using (var control = new SumARegisterUserControl())
			{
				AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, control.FindSingle<ZDateEdit>("PresentationDateEdit").DateTimeFormat);
			}
		}
	}
}
