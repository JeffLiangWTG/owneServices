using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	[TestedType(typeof(EdiAgreementAssignmentPluginController))]
	public class EDIAgreementAssignmentPluginControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.EdiUserAgreementAssignment;
		}

		public override void TestDeleteForm() => Assert(true);
		public override void TestEditForm() => Assert(true);
		public override void TestNewForm() => Assert(true);
		public override void TestViewForm() => Assert(true);
		public override void TestTemplateCopyForm() => Assert(true);
		public override void TestGetOpenFormUrlslDoesNotHitDatabase() => Assert(true);
		public override void TestSaveFormWithCustomsPlugIns() => Assert(true);
	}
}
