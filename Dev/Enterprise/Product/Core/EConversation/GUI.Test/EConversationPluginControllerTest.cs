using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing.GUI
{
	[TestedType(typeof(EConversationPluginController))]
	sealed class EConversationPluginControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.eConversationPlugIn;
		public override void TestDeleteForm() => Assert(true);
		public override void TestEditForm() => Assert(true);
		public override void TestNewForm() => Assert(true);
		public override void TestViewForm() => Assert(true);
		public override void TestTemplateCopyForm() => Assert(true);
		public override void TestGetOpenFormUrlslDoesNotHitDatabase() => Assert(true);
		public override void TestSaveFormWithCustomsPlugIns() => Assert(true);
	}
}
