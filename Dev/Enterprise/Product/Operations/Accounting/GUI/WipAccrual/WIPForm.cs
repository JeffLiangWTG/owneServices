using System.ComponentModel;

namespace Enterprise.Accounting.GUI.WipAccrual
{
	public partial class WIPForm : WIPAccrualForm
	{
		readonly IContainer components;

		public WIPForm()
		{
		}

		public WIPForm(Business.WIPAccrual.WIP wIP) : base(wIP)
		{
			this.Name = "WIPAndAccrualForm"; //this is set to the same name as WIPAccrualForm so that previous and next button will bring up forms in the same position.
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}

