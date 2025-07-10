using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI.Testing
{
	class FormForCurrentQueueUserControl : ZChildForm
	{
		public FormForCurrentQueueUserControl(IBusiness businessEntity) : base(businessEntity)
		{
		}

		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			UserControl = new UPECurrentQueueUserControl();
			UserControl.BindToPrefix = BindToPrefix;
			Controls.Add(UserControl);
		}

		public UPECurrentQueueUserControl UserControl;
		public virtual string BindToPrefix
		{
			get
			{
				return "";
			}
		}
	}
}
