using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTimeTimeEditTest : TestCaseWithDummy
	{
		public void TestMaxLengthIsNotBound()
		{
			AssertNull("Should not have bound MaxLength.", Control.DataBindings["MaxLength"]);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestMaxLengthCannotBeSet()
		{
			Control.MaxLength = 0;
		}

		public void TestAllowNegative()
		{
			AssertEquals(false, Control.AllowNegative);
			Control.AllowNegative = true;
			AssertEquals(true, Control.AllowNegative);
		}

		public void TestAllowNegativeActuallyWorks()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_Time = TimeSpan.FromHours(-4000);

			using (var testForm = new ZForm(Dummy))
			using (var timeEdit = new ZTimeTimeEdit())
			{
				testForm.Controls.Add(timeEdit);
				timeEdit.AllowNegative = false;

				testForm.Show();
				Application.DoEvents();

				timeEdit.SetDataBinding(bizo, DummyBizoSchema.Constants.Z0_Date);

				AssertEquals("     :", timeEdit.Text);
			}
		}

		public void TestFormatIsCalledOnBinding()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_Time = new ZTime(17, 45);

			using (var testForm = new ZForm(Dummy))
			using (var timeEdit = new ZTimeTimeEdit())
			{
				testForm.Controls.Add(timeEdit);

				testForm.Show();
				Application.DoEvents();

				timeEdit.SetDataBinding(bizo, DummyBizoSchema.Constants.Z0_Time);

				AssertEquals("17:45", timeEdit.Text);
			}
		}

		public void TestWidthUpdatedOnFontChange()
		{
			var width = Control.Width;
			Control.Font = new Font("Microsoft Sans Serif", 36);
			AssertNotEquals(width, Control.Width);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Control = new ZTimeTimeEdit();
			Control.SetDataBinding(Dummy, DummyBizoSchema.Constants.Z0_Date);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Control.Dispose();
		}

		ZTimeTimeEdit Control;

		#endregion
	}
}
