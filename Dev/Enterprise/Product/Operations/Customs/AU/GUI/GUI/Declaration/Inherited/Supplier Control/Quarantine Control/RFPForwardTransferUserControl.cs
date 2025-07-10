using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPForwardTransferUserControl : ZUserControl
	{
		public RFPForwardTransferUserControl()
		{
			InitializeComponent();
		}

		JobComInvoiceHeader CurrentInvoice
		{
			get
			{
				if (currentInvoice?.IsDeleted ?? false)
				{
					currentInvoice = null;
				}
				return currentInvoice;
			}
			set
			{
				currentInvoice = value;
			}
		}
		JobComInvoiceHeader currentInvoice;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var invoice = CurrentDataItem as JobComInvoiceHeader;

			if (CurrentInvoice != invoice)
			{
				UnHookEvents();

				CurrentInvoice = invoice;

				HookEvents();
			}
		}

		void UnHookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceTypeInfo_ValueChanged;
				exDocHeader.QH_ForwardLocationInfo.ValueChanged -= QH_ForwardLocationInfo_ValueChanged;
				exDocHeader.QH_TransferEDIUserLocationInfo.ValueChanged -= QH_TransferEDIUserLocationInfo_ValueChanged;
				exDocHeader.QH_TransferExporterLocationInfo.ValueChanged -= QH_TransferExporterLocationInfo_ValueChanged;
			}
		}

		void HookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceTypeInfo_ValueChanged;
				exDocHeader.QH_ForwardLocationInfo.ValueChanged += QH_ForwardLocationInfo_ValueChanged;
				exDocHeader.QH_TransferEDIUserLocationInfo.ValueChanged += QH_TransferEDIUserLocationInfo_ValueChanged;
				exDocHeader.QH_TransferExporterLocationInfo.ValueChanged += QH_TransferExporterLocationInfo_ValueChanged;
			}

			ProduceTypeValueChanged();
			ChangeVisibilityQH_TransfereeExporterNumber();
			ChangeVisibilityQH_TransfereeEDIUserIdentifier();
			ChangeVisibilityQH_ForwardeeEDIUserIdentifier();
		}

		void QH_ProduceTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ProduceTypeValueChanged();
		}

		void QH_TransferExporterLocationInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibilityQH_TransfereeExporterNumber();
		}

		void QH_TransferEDIUserLocationInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibilityQH_TransfereeEDIUserIdentifier();
		}

		void QH_ForwardLocationInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibilityQH_ForwardeeEDIUserIdentifier();
		}

		void ProduceTypeValueChanged()
		{
			var isNEXDOCSActive = currentInvoice?.QuarantineExDocHeader?.IsNEXDOCSActive ?? false;
			RequiresAcceptanceCheckbox.Visible = isNEXDOCSActive;
		}

		void ChangeVisibilityQH_ForwardeeEDIUserIdentifier()
		{
			var forwardLocation = CurrentInvoice?.QuarantineExDocHeader?.QH_ForwardLocation ?? EXDOCCodeOrganisation.Codes.Code;

			QH_ForwardeeEDIUserIdentifierTextBox.Visible = forwardLocation == EXDOCCodeOrganisation.Codes.Code;
			QH_OH_ForwardLocationOrganisationGuidFindBox.Visible = forwardLocation == EXDOCCodeOrganisation.Codes.Organisation;

			if (forwardLocation == EXDOCCodeOrganisation.Codes.Code)
			{
				QH_ForwardeeEDIUserIdentifierTextBox.Focus();
			}

			if (forwardLocation == EXDOCCodeOrganisation.Codes.Organisation)
			{
				QH_OH_ForwardLocationOrganisationGuidFindBox.Focus();
			}
		}

		void ChangeVisibilityQH_TransfereeEDIUserIdentifier()
		{
			var transferEDIUserLocation = CurrentInvoice?.QuarantineExDocHeader?.QH_TransferEDIUserLocation ?? EXDOCCodeOrganisation.Codes.Code;

			QH_TransfereeEDIUserIdentifierTextBox.Visible = transferEDIUserLocation == EXDOCCodeOrganisation.Codes.Code;
			QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.Visible = transferEDIUserLocation == EXDOCCodeOrganisation.Codes.Organisation;

			if (transferEDIUserLocation == EXDOCCodeOrganisation.Codes.Code)
			{
				QH_TransfereeEDIUserIdentifierTextBox.Focus();
			}
			if (transferEDIUserLocation == EXDOCCodeOrganisation.Codes.Organisation)
			{
				QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.Focus();
			}
		}

		void ChangeVisibilityQH_TransfereeExporterNumber()
		{
			var transferExporterLocation = CurrentInvoice?.QuarantineExDocHeader?.QH_TransferExporterLocation ?? EXDOCCodeOrganisation.Codes.Code;

			QH_TransfereeExporterNumberTextBox.Visible = transferExporterLocation == EXDOCCodeOrganisation.Codes.Code;
			QH_OH_TransferExporterLocationOrganisationGuidFindBox.Visible = transferExporterLocation == EXDOCCodeOrganisation.Codes.Organisation;

			if (transferExporterLocation == EXDOCCodeOrganisation.Codes.Code)
			{
				QH_TransfereeExporterNumberTextBox.Focus();
			}
			if (transferExporterLocation == EXDOCCodeOrganisation.Codes.Organisation)
			{
				QH_OH_TransferExporterLocationOrganisationGuidFindBox.Focus();
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			UnHookEvents();

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
