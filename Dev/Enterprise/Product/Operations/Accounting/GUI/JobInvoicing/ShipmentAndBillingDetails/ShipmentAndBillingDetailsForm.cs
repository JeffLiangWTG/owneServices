using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	internal partial class ShipmentAndBillingDetailsForm : ZChildForm
	{
		public ShipmentAndBillingDetailsForm(ShipmentAndBillingDetails details) : base(details)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;

		public void SelectDetailRow(ZGuid detailsRowPK) => DetailsGrid.SelectSingleElementByPK(detailsRowPK);
	}
}

