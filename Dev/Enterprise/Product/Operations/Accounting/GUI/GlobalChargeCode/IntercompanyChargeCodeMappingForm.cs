using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class IntercompanyChargeCodeMappingForm : ZChildForm
	{
		public IntercompanyChargeCodeMappingForm(BusinessObjectFactory factory)
			: base(new GlobalChargeCodeMapPivotIntercompanyCollection(factory))
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveAndCloseButton, CancelPostingButton, null);
			Collection.Load();
		}

		GlobalChargeCodeMapPivotIntercompanyCollection Collection
		{
			get { return (GlobalChargeCodeMapPivotIntercompanyCollection)BusinessEntity; }
		}

		protected IntercompanyChargeCodeMappingForm()
			: base()
		{
			InitializeComponent();
		}
	}
}
