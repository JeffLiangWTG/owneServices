using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class ValueAndCurrencyUserControlTest : TestCaseWithFactory
	{
		public void TestValueCalcEdit()
		{
			var calcEdit = control.FindSingle<ZCalcEdit>("ValueCalcEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_Value), calcEdit.GetBindingMember());
				AssertEquals("Decimals", 5, calcEdit.Decimals);
				AssertEquals("TabIndex", 0, calcEdit.TabIndex);
			});
		}

		public void TestCurrencyCodeFindBox()
		{
			var findBox = control.FindSingle<ZCodeFindBox>("CurrencyCodeFindBox");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_RX_NKCurrency), findBox.GetBindingMember());
				AssertEquals("CharacterCasing", CharacterCasing.Upper, findBox.CodeBox.CharacterCasing);
				AssertEquals("TabIndex", 1, findBox.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ValueAndCurrencyUserControl();
		}
		ValueAndCurrencyUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
