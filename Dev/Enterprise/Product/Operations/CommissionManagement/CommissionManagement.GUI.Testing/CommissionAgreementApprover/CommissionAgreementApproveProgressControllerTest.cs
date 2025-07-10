using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionAgreementApproveProgressControllerTest : TestCaseWithFactory
	{
		public void TestShow()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Show();

				var controller = new CommissionAgreementApproveProgressController();
				controller.Show((progress) => { }, parentForm);

				using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(CommissionAgreementApproveProgressForm), ZFormModaliser.LastFormShownForTest);
				}
			}
		}
	}
}
