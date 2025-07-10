using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class StatusAndProcedureUserControlTest : TestCaseWithFactory
	{
		public void TestStatusDropEdit()
		{
			var dropEdit = control.FindSingle<ZDropEdit>("StatusDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_Status), dropEdit.GetBindingMember());
				AssertEquals("CharacterCasing", CharacterCasing.Upper, dropEdit.CharacterCasing);
				AssertEquals("TabIndex", 0, dropEdit.TabIndex);
			});
		}

		public void TestProcedureDropEdit()
		{
			var dropEdit = control.FindSingle<ZDropEdit>("ProcedureDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_Procedure), dropEdit.GetBindingMember());
				AssertEquals("CharacterCasing", CharacterCasing.Upper, dropEdit.CharacterCasing);
				AssertEquals("TabIndex", 1, dropEdit.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new StatusAndProcedureUserControl();
		}
		StatusAndProcedureUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
