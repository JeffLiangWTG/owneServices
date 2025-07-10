using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class TCVehicleBasedUserControl : ZUserControl
	{
		public TCVehicleBasedUserControl()
		{
			InitializeComponent();
		}

		public TCVehicleBasedUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();

			if (!DesignMode)
			{
				InitializeLazyCreate(isOnInvoiceLine);
				SetupLPCOsGrid();
			}
		}

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			if (!isOnInvoiceLine)
			{
				VINNumberTextBox.Parent.Controls.Remove(VINNumberTextBox);
				ModelYearDropEdit.Parent.Controls.Remove(ModelYearDropEdit);
			}
		}

		void SetupLPCOsGrid()
		{
			LPCOsGridUserControl.RemoveExceptAvailableColumns(TCPGAHeader.AvailableLPCOFields);
		}

		void StateButton_Click(object sender, EventArgs e)
		{
			var pgaHeader = CurrentDataItem as TCPGAHeader;
			if (pgaHeader != null)
			{
				MultilingualString state = null;
				switch (pgaHeader.CA_SubProgram)
				{
					case TCPGAVehicleProgramCodes.Codes.VVP:
						state = TCComplicanceStatements.Descriptions.TC04;
						break;
					case TCPGAVehicleProgramCodes.Codes.VFS:
					case TCPGAVehicleProgramCodes.Codes.VFC:
						state = TCComplicanceStatements.Descriptions.TC06;
						break;
				}

				if (!string.IsNullOrEmpty(state))
				{
					var form = new ImporterDeclarationStateForm(state);
					ZFormModaliser.Show(form, ParentForm as ZForm);
				}
			}
		}
	}
}
