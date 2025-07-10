using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public abstract partial class HCProgramsBasedUserUserControl : ZUserControl
	{
		protected HCProgramsBasedUserUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			if (!isOnInvoiceLine)
			{
				DetailsGroupBox.Controls.Remove(ManufactureDateEdit);
				DetailsGroupBox.Controls.Remove(BrandNameTextBox);
				DetailsGroupBox.Controls.Remove(ExpiryDateEdit);
				DetailsGroupBox.Controls.Remove(TradeNameTextBox);
				DetailsGroupBox.Controls.Remove(ModelNameTextBox);

				ComponentGroupBox.Dispose();
				LPCOGroupBox.Dock = DockStyle.Fill;
			}
			else
			{
				SetComponentGrid();
			}
			SetLPCOGrid();
			SetControlsVisibility();
		}

		protected virtual void SetControlsVisibility()
		{
			if (!ComponentGroupBoxSupported && !ComponentGroupBox.IsDisposed)
			{
				ComponentGroupBox.Visible = false;
				LPCOGroupBox.Dock = DockStyle.Fill;
			}
		}

		internal protected virtual bool ComponentGroupBoxSupported => true;

		protected virtual void SetLPCOGrid()
		{
			LPCOGridUserControl.RemoveExceptAvailableColumns(HCPGAHeader.AvailableLPCOFields);
		}

		protected virtual void SetComponentGrid()
		{
			ComponentGridUserControl.RemoveFromAvailableColumns
			(
				Component.Schema.CA_Origin,
				Component.Schema.CA_QualityOrYield,
				Component.Schema.CA_Type,
				nameof(Component.TypeDescription)
			);
		}

		public new HCPGAHeader CurrentDataItem => base.CurrentDataItem as HCPGAHeader;
	}
}
