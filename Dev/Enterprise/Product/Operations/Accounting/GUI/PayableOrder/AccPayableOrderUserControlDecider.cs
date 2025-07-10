using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class AccPayableOrderUserControlDecider : ZUserControl
	{
		public AccPayableOrderUserControlDecider()
		{
			InitializeComponent();
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AccPayableOrderUserControl Inner
		{
			get { return fInner; }
			set
			{
				if (fInner != null)
				{
					Controls.Remove(fInner);
					fInner.Dispose();
				}
				Controls.Add(value);
				fInner = value;
				value.Dock = DockStyle.Fill;
			}
		}

		protected AccPayableOrderUserControl fInner;
	}
}
