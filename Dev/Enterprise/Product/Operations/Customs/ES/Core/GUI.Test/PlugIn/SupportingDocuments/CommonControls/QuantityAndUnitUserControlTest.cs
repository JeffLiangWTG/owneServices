using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class QuantityAndUnitUserControlTest : TestCaseWithFactory
	{
		public void TestSupDocQuantityCalcEdit()
		{
			var calcEdit = control.FindSingle<ZCalcEdit>("SupDocQuantityCalcEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_Quantity), calcEdit.GetBindingMember());
				AssertEquals("Decimals", 5, calcEdit.Decimals);
				AssertEquals("TabIndex", 0, calcEdit.TabIndex);
			});
		}

		public void TestUnitOfQuantityDropEdit()
		{
			var dropEdit = control.FindSingle<ZDropEdit>("UnitOfQuantityDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_UnitOfQuantity), dropEdit.GetBindingMember());
				AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Normal, dropEdit.CharacterCasing);
				AssertEquals("TabIndex", 1, dropEdit.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new QuantityAndUnitUserControl();
		}
		QuantityAndUnitUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
