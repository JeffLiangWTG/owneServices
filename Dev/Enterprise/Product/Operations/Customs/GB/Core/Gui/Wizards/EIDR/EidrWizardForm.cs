using System;
using Enterprise.Customs.GB.Business.Wizards.EIDR;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Wizards
{
	public partial class EidrWizardForm : ZChildForm
	{
		public EidrWizardForm()
		{
			InitializeComponent();
		}

		public EidrWizardForm(IEidrWizardManager manager)
			: base(manager.EidrWizard)
		{
			this.manager = manager;
			InitializeComponent();
		}
		readonly IEidrWizardManager manager;

		void ButtonCreateEidr_Click(object sender, EventArgs e)
		{
			manager.CreateEidr();
		}
	}
}
