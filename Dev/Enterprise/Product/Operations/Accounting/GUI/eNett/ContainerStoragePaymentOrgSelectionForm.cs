using System;
using Enterprise.Accounting.Business.eNett;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.eNett
{
	public partial class ContainerStoragePaymentOrgSelectionForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ContainerStoragePaymentOrgSelectionForm()
		{
			InitializeComponent();
		}

		public ContainerStoragePaymentOrgSelectionForm(ComPayRegisteredOrganisationDataSource bo)
			: base(bo)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("93d8812f-a004-48b0-88cf-06578481e309", "Select Container Storage Payment Organization"); }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DataSource != null)
			{
				containerStoragePaymentOrgSelectionControl1.CloseParentForm += new EventHandler(containerStoragePaymentOrgSelectionControl1_CloseParentForm);
			}
		}

		void containerStoragePaymentOrgSelectionControl1_CloseParentForm(object sender, EventArgs e)
		{
			Close();
		}

		public new ComPayRegisteredOrganisationDataSource DataSource
		{
			get { return (ComPayRegisteredOrganisationDataSource)base.DataSource; }
		}
	}
}
