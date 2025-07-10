using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class MultiControlStatusbarExtensionTest : BaseExtensionTest<MultiControlStatusbarExtension>
	{
		public void TestExtraControlsAreHookedToEvents()
		{
			var statusbar = new StatusbarForTest();
			var extension = new MultiControlStatusbarExtension(statusbar);

			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				var control0 = new ZTextBox { Location = new Point(0, 10) };
				var control1 = new ZTextBox { Location = new Point(20, 10), BindTo = DummyBizoSchema.Constants.Z0_Code };
				var control2 = new ZTextBox { Location = new Point(40, 10), BindTo = DummyBizoSchema.Constants.Z0_Description };
				var control3 = new ZTextBox { Location = new Point(60, 10), BindTo = DummyBizoSchema.Constants.Z0_VarCharMax };
				var control4 = new ZTextBox { Location = new Point(80, 10), BindTo = DummyBizoSchema.Constants.Z0_VarBinaryMax };

				form.Controls.AddRange(new Control[] { control0, control1, control2, control3, control4 });

				extension.Initialize(control1);
				extension.AddExtraControls(new Control[] { control2, control3 });

				form.Show();
				Application.DoEvents();

				statusbar.Notification = "";
				control0.Focus();
				AssertEquals("Should not react on control 0", "", statusbar.Notification);

				control1.Focus();
				AssertEquals("Code", statusbar.Notification);
				control0.Focus();
				AssertEquals("", statusbar.Notification);

				control2.Focus();
				AssertEquals("Should use notification from main control", "Code", statusbar.Notification);
				control0.Focus();
				AssertEquals("", statusbar.Notification);

				control3.Focus();
				AssertEquals("Should use notification from main control", "Code", statusbar.Notification);
				control0.Focus();
				AssertEquals("", statusbar.Notification);

				control4.Focus();
				AssertEquals("Should not react on control 4", "", statusbar.Notification);
			}
		}

		#region Test classes

		class StatusbarForTest : Statusbar
		{
			public override bool CanUpdate()
			{
				return true;
			}

			public override void Update(string notification, INotificationType state)
			{
				Notification = notification;
			}

			public string Notification { get; set; }
		}

		#endregion
	}
}
