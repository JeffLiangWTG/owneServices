using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(UsageUpdateForm))]
	class UsageUpdateFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestUpdate()
		{
			EServicesBillingTestHelper.CreateTable();

			UsageUpdateForm.Update(BillingTestHelper.MonthToday, "ODM", "", 0, false);
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new UsageUpdateForm();
		}

		#endregion
	}
}
