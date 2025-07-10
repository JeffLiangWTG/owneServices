using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class PrinterHelpControlTest : TestCaseWithFactory
	{
		#region Tests

		public void TestLinkLabelClick()
		{
			using (var form = new PrinterHelpControlTestForm())
			{
				form.PrintingHelpControl.PerformLinkLabelClick();
				AssertEquals(form.PrintingHelpControl.PrintingHelpURL, WebUrlLauncher.LastUrlLaunched);
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		public void TestPictureBoxClick()
		{
			using (var form = new PrinterHelpControlTestForm())
			{
				form.PrintingHelpControl.PerformPictureBoxClick();
				AssertEquals(form.PrintingHelpControl.PrintingHelpURL, WebUrlLauncher.LastUrlLaunched);
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		#endregion

		#region Implementation

		internal class PrinterHelpControlTestForm : ZForm
		{
			public PrinterHelpControlTestForm()
				: base()
			{
				PrintingHelpControl = new PrinterHelpControlTestControl();
				this.Controls.Add(PrintingHelpControl);
			}

			public PrinterHelpControlTestControl PrintingHelpControl;
		}

		internal class PrinterHelpControlTestControl : PrinterHelpControl
		{
			public PrinterHelpControlTestControl()
				: base()
			{
			}

			public void PerformPictureBoxClick()
			{
				var pictureBox = this.Controls.OfType<Enterprise.ZArchitecture.GUI.ZPictureBox>().First();
				InvokeOnClick(pictureBox, EventArgs.Empty);
			}

			public void PerformLinkLabelClick()
			{
				var linkLabel = this.Controls.OfType<ZLinkLabel>().First();
				InvokeOnClick(linkLabel, EventArgs.Empty);
			}

			public string PrintingHelpURL { get { return printingHelpUrl; } }
		}

		#endregion
	}
}
