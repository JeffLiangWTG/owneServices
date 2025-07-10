using CargoWise.BuildTools.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class DocumentCustomisationDevToolTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestShow()
		{
			DummyBODocSupportable parent = Factory.New<DummyBODocSupportable>();

			using (ZForm form = new ZForm(parent))
			{
				new DocumentCustomisationDevTool().Show(form);
				AssertType(typeof(DocumentCustomisationForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		#endregion
	}
}
