using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI
{
	public class DeclarationRequestedDocumentsUserControl : RequestedDocumentsUserControl
	{
		public DeclarationRequestedDocumentsUserControl()
			: base()
		{
			RequestedDocumentsGrid.AfterBind += new EventHandler(RequestedDocumentsGrid_AfterBind);
		}

		void RequestedDocumentsGrid_AfterBind(object sender, EventArgs e)
		{
			if (BindingSource.Current is CusEntryInstruction instruction && !instruction.JobDeclaration.IsImport)
			{
				RequestedDocumentsGrid.RemoveFromAvailableColumns(CusSupportingInfoSchema.Constants.CSI_ReferenceNumber);
			}
		}

		protected override void CustomizeLayoutCore()
		{
			var referenceNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			referenceNumberTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("49aa6c5d-d99d-4cce-8aa6-1b368722d48d", "Ref. No.", "Reference No.", "Reference Number", "Requested document reference number.");
			referenceNumberTextBoxColumnStyleInfo.ColumnName = CusSupportingInfoSchema.Constants.CSI_ReferenceNumber;
			referenceNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			RequestedDocumentsGrid.ColumnStyles.Add(referenceNumberTextBoxColumnStyleInfo);
		}
	}
}
