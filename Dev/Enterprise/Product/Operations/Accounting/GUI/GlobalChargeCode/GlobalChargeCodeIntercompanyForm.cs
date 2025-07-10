using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class GlobalChargeCodeIntercompanyForm : ZChildForm
	{
		ZCheckBox IsActiveCheckBox;
		ZGrid ChargeCodeMappingGrid;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZTextBox GlobalCodeTextBox;
		ZTextBox DescriptionTextBox;
		ZTabControl zTabControl1;
		ZTabPage zTabPageGeneric;
		ZTabPage zTabPageLocalClientOverride;
		ZGrid chargeCodeMappingWithLocalClientOverrideGrid;
		System.ComponentModel.IContainer components;

		public GlobalChargeCodeIntercompanyForm(GlobalChargeCodeMapIntercompany businessEntity)
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

