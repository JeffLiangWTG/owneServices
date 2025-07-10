using System;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	public partial class ServiceTaskHostConfigurationForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ServiceTaskHostConfigurationForm()
		{
			InitializeComponent();
		}

		public ServiceTaskHostConfigurationForm(StmServiceHost host)
			: base(host)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}

