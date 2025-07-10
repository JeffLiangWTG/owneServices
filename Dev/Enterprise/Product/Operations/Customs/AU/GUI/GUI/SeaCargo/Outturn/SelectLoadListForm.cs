using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SelectLoadListForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public SelectLoadListForm()
		{
		}

		public SelectLoadListForm(CFSLoadListSelector selector) : base(selector)
		{
			this.Selector = selector;
		}

		public readonly CFSLoadListSelector Selector;

		internal void OKButton1_Click(object sender, System.EventArgs e)
		{
			if (Selector != null && !Selector.HasErrors)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowError("Please select a valid consol");
			}
		}

		void CancelButton1_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == oKButton1 && previousControl == cancelButton1;
		}

		#endregion
	}
}
