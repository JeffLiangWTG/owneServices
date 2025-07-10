using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	public class ContainerStoragePaymentUserControlTest : TestCaseWithFactory
	{
		public void TestAmountDecimalPlaces()
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)aPInv.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();

				RefCurrency currency0DP = Factory.NewWithValidTestData<RefCurrency>();
				currency0DP.RX_SubUnitRatio = 0;
				RefCurrency currency2DP = Factory.NewWithValidTestData<RefCurrency>();
				currency2DP.RX_SubUnitRatio = 100;
				Factory.Save();

				ZCalcEditColumnStyleInfo oSAmountColumn = (ZCalcEditColumnStyleInfo)form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(TransactionLine.Schema.AL_OSExTaxAmount);
				aPInv.AH_RX_NKTransactionCurrency = currency2DP.RX_Code;
				AssertEquals("Decimals should be 2", 2, line[oSAmountColumn.BindToDecimalPlaces]);
				aPInv.AH_RX_NKTransactionCurrency = currency0DP.RX_Code;
				AssertEquals("Decimals should be 0", 0, line[oSAmountColumn.BindToDecimalPlaces]);

				IDataGridLayoutIdentifierRoot idRoot = form;
				AssertEquals(form.Name + aPInv.GetType().Name, idRoot.ID);
			}
		}

		public void TestIsFinalChargeSet()
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OH = TestObjectCreator.ABIGAS.PK;
			aPInv.AH_TransactionNum = "I001";
			APInvoiceLine line = (APInvoiceLine)aPInv.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.AttachJobToAPLine(line);
			line.AL_OSAmount = 100M;
			line.AL_LineAmount = 100M;
			line.AL_IsFinalCharge = true;
			TestObjectCreator.AttachChargeToAPLine(line);
			Factory.Save();
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				Assert(line.AL_IsFinalCharge);
			}
		}

		#region Implementation

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
