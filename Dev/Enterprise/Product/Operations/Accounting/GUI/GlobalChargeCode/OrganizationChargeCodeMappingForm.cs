using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class OrganizationChargeCodeMappingForm : ZChildForm
	{
		public OrganizationChargeCodeMappingForm(BusinessObjectFactory factory)
			: base(new OrganizationChargeCodeMapping(factory))
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveAndCloseButton, CancelPostingButton, null);
		}

		protected OrganizationChargeCodeMappingForm()
			: base()
		{
			InitializeComponent();
		}
	}
}
