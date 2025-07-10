using System.Windows.Forms;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class InvoiceLineFiscalMarkUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EMCSJobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestFiscalMarkTextBox()
		{
			CombineAssertions(() =>
			{
				var fiscalMarkControl = control.FiscalMarkTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, fiscalMarkControl.CharacterCasing);
				AssertType<LongTextControl>("Type", fiscalMarkControl);
			});
		}

		public void TestFiscalMarkUsedCheckBox()
		{
			AssertType<ZCheckBox>(control.FiscalMarkUsedCheckBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineFiscalMarkUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		InvoiceLineFiscalMarkUserControl control;
	}
}
