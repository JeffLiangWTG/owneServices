using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(BaseCustomsSupplierHeaderUserControl))]
	sealed class BaseCustomsSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestExchangeDateRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl as ImportSupplierHeaderUserControl)
				{
					AssertType<ZDateEdit>(control.ExchangeRateDateEdit);
				}
			}
		}
	}
}
