using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionFinalizerPaymentControllerTest : TestCaseWithFactory
	{
		public void TestPromptUserForProcessPayment_WithUnapprovedLines()
		{
			var commissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine.CL0_ApprovedDateTimeUtc = ZDateTime.Empty;
			Factory.Save();

			var finalizer = new CommissionFinalizer();
			finalizer.ViewCommissionLineCollection.AdditionalFilter = new ZQuery(ViewCommissionLineSchema.PK, commissionLine.PK);
			finalizer.CommissionFinalizerLineItems.Single().IsSelected = true;

			using (var form = new ZForm(finalizer))
			{
				form.Show();

				OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

				var paymentController = new CommissionFinalizerPaymentController(form, finalizer);
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should not have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Unable to Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "At least one selected entity commission has not been approved yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}
}
