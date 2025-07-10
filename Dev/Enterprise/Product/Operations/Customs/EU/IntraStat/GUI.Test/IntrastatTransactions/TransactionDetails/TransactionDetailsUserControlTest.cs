
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	sealed class TransactionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusIntrastatHeader), control.BindingSource.DataSourceType);
		}

		public void TestTransactionDate()
		{
			var transactionDate = control.TransactionDateEdit;

			AssertType<ZDateEdit>("Type", transactionDate);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_TransactionDate), transactionDate.BindTo);
			AssertEquals("DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Short, transactionDate.DateTimeFormat);
			AssertEquals("TabStop", true, transactionDate.TabStop);
		}

		public void TestCountryOfSupplyDropEdit()
		{
			var countryOfSupplyDropEdit = control.CountryOfSupplyDropEdit;

			AssertType<ZDropEdit>("Type", countryOfSupplyDropEdit);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_CountryOfSupply), countryOfSupplyDropEdit.BindTo);
			AssertEquals("TabStop", true, countryOfSupplyDropEdit.TabStop);
		}

		public void TestCountryOfReceiptDropEdit()
		{
			var countryOfReceiptDropEdit = control.CountryOfReceiptDropEdit;

			AssertType<ZDropEdit>("Type", countryOfReceiptDropEdit);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_CountryOfReceipt), countryOfReceiptDropEdit.BindTo);
			AssertEquals("TabStop", true, countryOfReceiptDropEdit.TabStop);
		}

		public void TestNatureOfTransaction()
		{
			var natureOfTransaction = control.NatureOfTransactionDropEdit;

			AssertType<ZDropEdit>("Type", natureOfTransaction);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_NatureOfTransaction), natureOfTransaction.BindTo);
			AssertEquals("TabStop", true, natureOfTransaction.TabStop);
		}

		public void TestModeOfTransport()
		{
			var modeOfTransport = control.ModeOfTransportDropEdit;

			AssertType<ZDropEdit>("Type", modeOfTransport);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_ModeOfTransport), modeOfTransport.BindTo);
			AssertEquals("TabStop", true, modeOfTransport.TabStop);
		}

		public void TestIncoTerm()
		{
			var incoTerm = control.IncoTermDropEdit;

			AssertType<ZDropEdit>("Type", incoTerm);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_IncoTerm), incoTerm.BindTo);
			AssertEquals("TabStop", true, incoTerm.TabStop);
		}

		public void TestSupplierVAT()
		{
			var supplierVat = control.SupplierVATTextBox;

			AssertType<ZTextBox>("Type", supplierVat);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_SupplierVAT), supplierVat.BindTo);
			AssertEquals("TabStop", true, supplierVat.TabStop);
		}

		public void TestConsigneeVAT()
		{
			var consigneeVat = control.ConsigneeVATTextBox;

			AssertType<ZTextBox>("Type", consigneeVat);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_ConsigneeVAT), consigneeVat.BindTo);
			AssertEquals("TabStop", true, consigneeVat.TabStop);
		}

		public void TestSupplierName()
		{
			var supplierName = control.SupplierNameTextBox;

			AssertType<ZTextBox>("Type", supplierName);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_SupplierName), supplierName.BindTo);
			AssertEquals("TabStop", true, supplierName.TabStop);
		}

		public void TestConsigneeName()
		{
			var consigneeName = control.ConsigneeNameTextBox;

			AssertType<ZTextBox>("Type", consigneeName);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_ConsigneeName), consigneeName.BindTo);
			AssertEquals("TabStop", true, consigneeName.TabStop);
		}

		public void TestTradersReference()
		{
			var tradersReference = control.TradersReferenceTextBox;

			AssertType<ZTextBox>("Type", tradersReference);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_TradersReference), tradersReference.BindTo);
			AssertEquals("TabStop", true, tradersReference.TabStop);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransactionDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransactionDetailsUserControl control;
	}
}
