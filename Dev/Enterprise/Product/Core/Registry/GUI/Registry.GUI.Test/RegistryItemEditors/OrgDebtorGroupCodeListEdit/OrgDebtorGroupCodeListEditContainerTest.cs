using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class OrgDebtorGroupCodeListEditContainerTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestReadOnly()
		{
			using (ZForm form = new ZForm())
			using (DummyOrgDebtorGroupCodeListEditContainer control = new DummyOrgDebtorGroupCodeListEditContainer())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("OrgDebtorGroupCodeGrid.ReadOnly", false, control.OrgDebtorGroupCodeGrid.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("OrgDebtorGroupCodeGrid.ReadOnly", true, control.OrgDebtorGroupCodeGrid.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("OrgDebtorGroupCodeGrid.ReadOnly", false, control.OrgDebtorGroupCodeGrid.ReadOnly);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest form = new ZEmptyFormForBasherTest();
			form.MinimumSize = new Size(1024, 600);
			form.Size = new Size(1024, 600);
			form.CaptionRenderingEnabled = true;

			OrgDebtorGroupCodeListEditContainer control = new OrgDebtorGroupCodeListEditContainer();
			form.Controls.Add(control);

			return form;
		}

		#region class DummyOrgDebtorGroupCodeListEditContainer

		class DummyOrgDebtorGroupCodeListEditContainer : OrgDebtorGroupCodeListEditContainer
		{
			public new ZGrid OrgDebtorGroupCodeGrid
			{
				get { return base.OrgDebtorGroupCodeGrid; }
			}
		}

		#endregion

		#endregion
	}
}
