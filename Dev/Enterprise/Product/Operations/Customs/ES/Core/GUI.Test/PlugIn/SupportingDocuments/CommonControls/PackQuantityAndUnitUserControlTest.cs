using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class PackQuantityAndUnitUserControlTest : TestCaseWithFactory
	{
		public void TestSupDocPackQuantityCalcEdit()
		{
			var calcEdit = control.FindSingle<ZCalcEdit>("SupDocPackQuantityCalcEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_PackQty), calcEdit.GetBindingMember());
				AssertEquals("Decimals", 5, calcEdit.Decimals);
				AssertEquals("TabIndex", 0, calcEdit.TabIndex);
			});
		}

		public void TestUnitOfPackQuantityDropEdit()
		{
			var dropEdit = control.FindSingle<ZDropEdit>("UnitOfPackQuantityDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_PackType), dropEdit.GetBindingMember());
				AssertEquals("CSI_PackType : CharacterCasing", CharacterCasing.Upper, dropEdit.CharacterCasing);
				AssertEquals("TabIndex", 1, dropEdit.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new PackQuantityAndUnitUserControl();
		}
		PackQuantityAndUnitUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
