using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class GlobalChargeCodeOrganizationForm : ZChildForm
	{
		ZCheckBox IsActiveCheckBox;
		ZGrid ChargeCodeMappingGrid;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZTextBox GlobalCodeTextBox;
		ZTextBox DescriptionTextBox;
		protected ZGroupBox SortByGroupBox;
		protected ZGuidFindBox OrganizationGuidFindBox;

		public GlobalChargeCodeOrganizationForm(GlobalChargeCodeMapOrganization businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
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

