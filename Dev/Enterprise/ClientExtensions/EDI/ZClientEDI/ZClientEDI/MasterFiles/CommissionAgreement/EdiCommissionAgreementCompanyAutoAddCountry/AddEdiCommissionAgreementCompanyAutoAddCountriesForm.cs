using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class AddEdiCommissionAgreementCompanyAutoAddCountriesForm : ZChildForm
	{
		public AddEdiCommissionAgreementCompanyAutoAddCountriesForm(AddEdiCommissionAgreementCompanyAutoAddCountriesAction addCompanyCountries)
			: base(addCompanyCountries)
		{
			InitializeComponent();
		}

		public new AddEdiCommissionAgreementCompanyAutoAddCountriesAction BusinessEntity
		{
			get { return (AddEdiCommissionAgreementCompanyAutoAddCountriesAction)base.BusinessEntity; }
		}

		#region Add Button

		void AddButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntityForValidation.RunPreSaveValidation();
			if (BusinessEntityForValidation.HasErrors())
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.Cancel;
				return;
			}

			BusinessEntity.Execute();
			DialogResult = DialogResult.OK;
			Close();
		}

		#endregion

		#region Cancel Button

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}
