using System.Drawing;
using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EdiCommissionAgreementControl : CommissionAgreementControl
	{
		#region Register/Unregister SubType Override

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		public static new EdiCommissionAgreementControl New()
		{
			return new EdiCommissionAgreementControl();
		}

		#endregion

		public EdiCommissionAgreementControl()
		{
			InitializeComponent();
			CA0_OH_CustomerGuidDropEdit.Visible = false;
		}

		#region CurrentDataItem

		public new EdiCommissionAgreement CurrentDataItem
		{
			get { return (EdiCommissionAgreement)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			customerFiltersButton.Enabled = (!CustomerFiltersButtonShouldBeBrowseOnly || CurrentDataItemHasCustomization);
			RefreshCustomerFiltersButton();
		}

		#endregion

		#region Customer Filters Button

		bool CurrentDataItemHasCustomization
		{
			get { return CurrentDataItem != null && CurrentDataItem.Customization != null; }
		}

		bool CustomerFiltersButtonShouldBeBrowseOnly
		{
			get { return CurrentDataItem == null || CurrentDataItem.ReadOnly || ((ZForm)ParentForm).DisplayMode == ZArchitecture.Core.ODisplayMode.ReadOnly; }
		}

		void RefreshCustomerFiltersButton()
		{
			var colorTheme = SystemDataRegistry.Instance.ColorTheme;
			customerFiltersButton.ForeColor = CurrentDataItemHasCustomization ? colorTheme.NavBarTextColor : Color.Black;
			customerFiltersButton.BackColor = CurrentDataItemHasCustomization ? colorTheme.NavBarButtonColor1 : colorTheme.ButtonColor;
			customerFiltersButton.Text = CurrentDataItemHasCustomization ? "View Filters" : "Add Filters";
		}

		void CustomerFiltersButton_Click(object sender, System.EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				var customization = CurrentDataItem.GetOrCreateCustomization();
				var form = new EdiCommissionAgreementCustomizationForm(customization);
				if (CustomerFiltersButtonShouldBeBrowseOnly)
				{
					form.DisplayMode = ZArchitecture.Core.ODisplayMode.ReadOnly;
				}

				form.FormClosed += Form_FormClosed;
				ZFormModaliser.Show(form, ParentForm);
			}
		}

		void Form_FormClosed(object sender, FormClosedEventArgs e)
		{
			var form = (EdiCommissionAgreementCustomizationForm)sender;
			form.FormClosed -= Form_FormClosed;

			RefreshCustomerFiltersButton();
		}

		#endregion
	}
}
