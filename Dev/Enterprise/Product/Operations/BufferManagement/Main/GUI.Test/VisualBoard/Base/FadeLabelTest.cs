using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class FadeLabelTest : TestCaseWithFactory
	{
		class WeirdFadeLabel : FadeLabel
		{
			public WeirdFadeLabel(CellContent cell, float gradientAngle) : base(cell, gradientAngle)
			{
			}

			public override Color BackColor
			{
				get
				{
					this.cell.BackgroundFadeColor = null;
					return base.BackColor;
				}
				set => base.BackColor = value;
			}
		}

		[ExpectNoExceptions]
		public void TestRaceCondition()
		{
			var cell = new CellContent(0, 0, CellContentType.Cards);
			cell.BackgroundFadeColor = Color.Red;
			using (var form = new Form())
			using (var label = new WeirdFadeLabel(cell, 22))
			{
				form.Controls.Add(label);
				label.Dock = DockStyle.Fill;
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
