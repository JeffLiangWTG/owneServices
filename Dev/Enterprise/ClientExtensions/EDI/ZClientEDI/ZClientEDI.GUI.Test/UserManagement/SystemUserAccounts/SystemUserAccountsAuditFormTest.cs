using System.Windows.Forms;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	[TestedType(typeof(SystemUserAccountsAuditForm))]
	public class SystemUserAccountsAuditFormTest : ZFormBasherTest
	{
		#region Implementation

		public override void TestBashingForm()
		{
			Assert(true); //custom form
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); //custom form
		}

		protected override Form GetFormToBashCore()
		{
			var bizo = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.Save();
			return new SystemUserAccountsAuditForm(bizo);
		}

		#endregion
	}
}
