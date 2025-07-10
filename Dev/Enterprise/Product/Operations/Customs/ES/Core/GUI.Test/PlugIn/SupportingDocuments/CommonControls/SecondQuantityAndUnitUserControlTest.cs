using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class SecondQuantityAndUnitUserControlTest : TestCaseWithFactory
	{
		public void TestQuantity2CalcEdit()
		{
			var calcEdit = control.FindSingle<ZCalcEdit>("Quantity2CalcEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_Quantity2), calcEdit.GetBindingMember());
				AssertEquals("Decimals", 5, calcEdit.Decimals);
				AssertEquals("TabIndex", 0, calcEdit.TabIndex);
			});
		}

		public void TestUnitOfQuantity2TextBox()
		{
			var textBox = control.FindSingle<ZTextBox>("UnitOfQuantity2TextBox");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_UnitOfQuantity2), textBox.GetBindingMember());
				AssertEquals("CharacterCasing", CharacterCasing.Normal, textBox.CharacterCasing);
				AssertEquals("TabIndex", 1, textBox.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SecondQuantityAndUnitUserControl();
		}
		SecondQuantityAndUnitUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
