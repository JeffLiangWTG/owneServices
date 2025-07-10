using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionAgreementApproveProgressForm))]
	public class CommissionAgreementApproveProgressFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CommissionAgreementApproveProgressForm();
		}
	}
}
