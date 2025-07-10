using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMControlCustomisationForm : ZTemplateForm
	{
		public BMControlCustomisationForm()
		{
			InitializeComponent();
		}

		public BMControlCustomisationForm(BMControlCustomisation businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		public new BMControlCustomisation BusinessEntity
		{
			get { return (BMControlCustomisation)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return BusinessEntity.HumanReadableName; }
		}

		protected override bool SupportsEDocs => true;

		protected override bool ShowAuditTab => true;
	}
}
