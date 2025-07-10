using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.GUI.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class BaseCustomsSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestNetWeightCalcDropEditDecimalPlaces()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header = jobDeclaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				header.JZ_NetWeight = 78.9876543210;
				using (var invoiceHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl)
				{
					var control = form.GetControl<ZArchitecture.GUI.ZCalcDropEdit>("NetWeightCalcDropEdit");
					AssertEquals(3, control.Decimals);
					AssertEquals(new ZDecimal(78.988), header.JZ_NetWeight);
				}
			}
		}
	}
}
