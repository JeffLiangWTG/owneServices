using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPDetailsUserControl : ZUserControl
	{
		public RFPDetailsUserControl()
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
				exDocHeader.QH_PrintLocationInfo.ValueChanged -= QH_PrintLocationInfo_ValueChanged;
			}
		}

		void HookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceTypeInfo_ValueChanged;
				exDocHeader.QH_PrintLocationInfo.ValueChanged += QH_PrintLocationInfo_ValueChanged;
			}

			ProduceTypeValueChanged();
			ChangeVisibilityQH_CertificateRequiredLocation();
		}

		void QH_PrintLocationInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibilityQH_CertificateRequiredLocation();
			ChangeFocusQH_CertificateRequiredLocation();
		}

		void QH_ProduceTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ProduceTypeValueChanged();
		}

		void ProduceTypeValueChanged()
		{
			var isNEXDOCSActive = CurrentInvoice?.QuarantineExDocHeader?.IsNEXDOCSActive ?? false;

			QH_CustomsConsigneeNameTextBox.Visible = isNEXDOCSActive;
			QH_ExemptionCodeTextBox.Visible = isNEXDOCSActive;
			QH_AQISRegionCodeFindBox.Visible = !isNEXDOCSActive;
		}

		void ChangeVisibilityQH_CertificateRequiredLocation()
		{
			var printLocation = CurrentInvoice?.QuarantineExDocHeader?.QH_PrintLocation ?? EXDOCCodeOrganisation.Codes.Code;

			QH_CertificateRequiredLocationCodeFindBox.Visible = printLocation == QuarantineExDocHeaderLookups.AqisPlaceCode;
			QH_CertificateRequiredLocationTextBox.Visible = printLocation == EXDOCCodeOrganisation.Codes.Code;
			QH_OH_PrintLocationOrganisationGuidFindBox.Visible = printLocation == EXDOCCodeOrganisation.Codes.Organisation;
		}

		void ChangeFocusQH_CertificateRequiredLocation()
		{
			var printLocation = CurrentInvoice?.QuarantineExDocHeader?.QH_PrintLocation ?? ZString.Empty;

			switch (printLocation)
			{
				case QuarantineExDocHeaderLookups.AqisPlaceCode:
					{
						QH_CertificateRequiredLocationCodeFindBox.Focus();
						break;
					}
				case EXDOCCodeOrganisation.Codes.Code:
					{
						QH_CertificateRequiredLocationTextBox.Focus();
						break;
					}
				case EXDOCCodeOrganisation.Codes.Organisation:
					{
						QH_OH_PrintLocationOrganisationGuidFindBox.Focus();
						break;
					}
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
