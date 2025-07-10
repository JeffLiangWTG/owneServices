using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class EMCSInvoiceLineArrivalInformationUserControlTest : TestCaseWithFactory
	{
		public void TestExplanationTextBox_UpperLowerCase()
		{
			using (var control = new EMCSInvoiceLineArrivalInformationUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.ExplanationTextBox.CharacterCasing);
			}
		}

		public void TestReportOfReceiptGroupBox_ReadOnly_Consignor()
		{
			AssertReportOfReceiptGroupBox_ReadOnly(EMCSEntryTypeList.Codes.Consignor, ZBool.True);
		}

		public void TestReportOfReceiptGroupBox_ReadOnly_Consignee()
		{
			AssertReportOfReceiptGroupBox_ReadOnly(EMCSEntryTypeList.Codes.Consignee, ZBool.False);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			_ = invoiceLine.Outturn;
		}
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine invoiceLine;

		void AssertReportOfReceiptGroupBox_ReadOnly(ZString declarantType, ZBool readOnly)
		{
			declaration.JE_DeclarantType = declarantType;
			using (var form = new ZForm(invoiceLine))
			using (var control = new EMCSInvoiceLineArrivalInformationUserControl())
			{
				control.SetDataBinding(declaration, nameof(declaration.FilteredInvoiceLines));
				form.Controls.Add(control);
				form.Show();

				AssertEquals(readOnly, control.ReportOfReceiptGroupBox.GetReadOnly());
			}
		}
	}
}
