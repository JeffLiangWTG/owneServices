using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class OrgHeaderCodeListEditContainerTest : ZFormBasherTest
	{
		public void TestReadOnly()
		{
			using (ZForm form = new ZForm())
			using (DummyOrgHeaderCodeListEditContainer control = new DummyOrgHeaderCodeListEditContainer())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("OrgHeaderCodeGrid.ReadOnly", false, control.OrgHeaderCodeGrid.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("OrgHeaderCodeGrid.ReadOnly", true, control.OrgHeaderCodeGrid.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("OrgHeaderCodeGrid.ReadOnly", false, control.OrgHeaderCodeGrid.ReadOnly);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest form = new ZEmptyFormForBasherTest();
			form.MinimumSize = new Size(1024, 600);
			form.Size = new Size(1024, 600);
			form.CaptionRenderingEnabled = true;

			OrgHeaderCodeListEditContainer control = new OrgHeaderCodeListEditContainer();
			form.Controls.Add(control);

			return form;
		}

		#region class DummyOrgHeaderCodeListEditContainer

		class DummyOrgHeaderCodeListEditContainer : OrgHeaderCodeListEditContainer
		{
			public new ZGrid OrgHeaderCodeGrid
			{
				get { return base.OrgHeaderCodeGrid; }
			}
		}

		#endregion

		#endregion
	}
}
