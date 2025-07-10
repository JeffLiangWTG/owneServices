using System;
using System.Windows.Forms;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukShipmentAndHawbLinkerControl : ZUserControl
	{
		CcsukShipmentAndHawbLinkerControl()
		{
			InitializeComponent();
		}

		public CcsukShipmentAndHawbLinkerControl(NonPersistentShipmentToHawbMatcherHeader header, Action<DialogResult> closeMethod)
		{
			this.header = header;
			closeFormMethod = closeMethod;
			SetDataBinding(header, "");
			InitializeComponent();

			mawbDropEdit.AllowOverlap(zGrid1);
		}

		void buttonMakeLinks_Click(object sender, EventArgs e)
		{
			var checkResult = header.CheckIfCanProceedToMakeOrMatchAll();
			if (checkResult.CanProceed)
			{
				var matchedOk = header.MakeOrMatchAll();
				if (closeFormMethod != null)
				{
					closeFormMethod(matchedOk ? DialogResult.OK : DialogResult.Cancel);
				}
			}
			else
			{
				Globals.Message.ShowError(checkResult.ErrorMessage);
			}
		}

		void buttonOldMethod_Click(object sender, EventArgs e)
		{
			if (closeFormMethod != null)
			{
				closeFormMethod(DialogResult.None);
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			if (closeFormMethod != null)
			{
				closeFormMethod(DialogResult.Cancel);
			}
		}

		readonly NonPersistentShipmentToHawbMatcherHeader header;
		readonly Action<DialogResult> closeFormMethod;
	}
}
