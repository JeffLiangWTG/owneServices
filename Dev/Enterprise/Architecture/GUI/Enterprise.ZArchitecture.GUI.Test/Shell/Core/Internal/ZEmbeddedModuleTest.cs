using System.Drawing;
using System.Windows.Forms;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ZEmbeddedModuleTest : TestCase
	{
		public void TestGetNewEmbeddedControl()
		{
			using (var testModule = new ZTestEmbeddedModule())
			{
				AssertEquals("EmbeddedControl.Name", "NewControl", testModule.EmbeddedControl.Name);
			}
		}

		public void TestShowPopup()
		{
			using (var testModule = new ZTestEmbeddedModule())
			using (var form = (Form)testModule.ShowPopup())
			{
				Application.DoEvents();

				AssertNotNull(form);
				AssertEquals("Test Module", form.Text);
				AssertEquals(true, form.Controls.Contains(testModule.EmbeddedControl));
				AssertEquals(true, form.Visible);
				AssertEquals(form.MinimumSize, testModule.EmbeddedControl.Size);
			}
		}

		#region Test Classes

		internal class ZTestEmbeddedModule : ZEmbeddedModule
		{
			public ZTestEmbeddedModule()
			{
			}

			public override ModuleIdentifier ID
			{
				get { return DummyModuleIDs.Dummy; }
			}

			protected override Control GetNewEmbeddedControl()
			{
				var newControl = new UserControl();
				newControl.Name = "NewControl";
				newControl.MinimumSize = new Size(1000, 700);
				return newControl;
			}

			public override SecurityCheckpoint SecurityCheckpoint
			{
				get { return null; }
			}

			protected override LicenceCheckpoint LicenceCheckPointCore
			{
				get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.Core; }
			}
		}

		#endregion
	}
}
