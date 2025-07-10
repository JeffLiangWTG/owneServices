using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class AISProgramUserControl : ZUserControl
	{
		public AISProgramUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			this.isOnInvoiceLine = isOnInvoiceLine;

			InitializeLazyCreate();
			UpdateDataBinding();
		}

		readonly bool isOnInvoiceLine;

		protected void InitializeLazyCreate()
		{
			var list = new List<string>(DFOPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);
		}

		void UpdateDataBinding()
		{
			if (!isOnInvoiceLine)
			{
				CommonNameTextBox.Dispose();
				CountIntEdit.Dispose();
			}
		}
	}
}
