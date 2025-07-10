using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPInspectionDetailsUserControl : ZUserControl
	{
		public RFPInspectionDetailsUserControl()
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
				exDocHeader.QH_StorageLocationInfo.ValueChanged -= QH_StorageLocationInfo_ValueChanged;
				exDocHeader.QH_AuthorisationLocationInfo.ValueChanged -= QH_AuthorisationLocationInfo_ValueChanged;
			}
		}

		void HookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceTypeInfo_ValueChanged;
				exDocHeader.QH_StorageLocationInfo.ValueChanged += QH_StorageLocationInfo_ValueChanged;
				exDocHeader.QH_AuthorisationLocationInfo.ValueChanged += QH_AuthorisationLocationInfo_ValueChanged;
			}

			ProduceTypeValueChanged();
			ChangeVisibilityQH_AuthorisationEstablishment();
			ChangeVisibilityQH_StorageEstablishment();
		}

		void QH_AuthorisationLocationInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibilityQH_AuthorisationEstablishment();
		}

		void QH_StorageLocationInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibilityQH_StorageEstablishment();
		}

		void QH_ProduceTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ProduceTypeValueChanged();
		}

		void ProduceTypeValueChanged()
		{
			var isNEXDOCSActive = CurrentInvoice?.QuarantineExDocHeader?.IsNEXDOCSActive ?? false;

			QH_AuthorisationDateDateEdit.Visible = isNEXDOCSActive;
			QH_AuthorisationCommentsTextBox.Visible = isNEXDOCSActive;
			QH_AuthorisationFlagCheckBox.Visible = isNEXDOCSActive;
			AuthorisationEstablishmentGroupBox.Enabled = !isNEXDOCSActive || (!CurrentInvoice?.QuarantineExDocHeader?.IsWoolOrSkinsProduceType ?? true);

			QH_OriginCatchZoneTextBox.Visible = !isNEXDOCSActive;
			NexDocCatchZonesGroupBox.Visible = isNEXDOCSActive;
		}

		void ChangeVisibilityQH_AuthorisationEstablishment()
		{
			var authorisationLocation = CurrentInvoice?.QuarantineExDocHeader?.QH_AuthorisationLocation ?? EXDOCCodeOrganisation.Codes.Code;

			QH_AuthorisationEstablishmentCodeFindBox.Visible = authorisationLocation == QuarantineExDocHeaderLookups.AqisPlaceCode;
			QH_AuthorisationEstablishmentTextBox.Visible = authorisationLocation == EXDOCCodeOrganisation.Codes.Code;
			QH_OA_AuthorisationEstablishmentAddressGuidFindBox.Visible = authorisationLocation == EXDOCCodeOrganisation.Codes.Organisation;

			switch (authorisationLocation)
			{
				case QuarantineExDocHeaderLookups.AqisPlaceCode:
					{
						QH_AuthorisationEstablishmentCodeFindBox.Focus();
						break;
					}
				case EXDOCCodeOrganisation.Codes.Code:
					{
						QH_AuthorisationEstablishmentTextBox.Focus();
						break;
					}
				case EXDOCCodeOrganisation.Codes.Organisation:
					{
						QH_OA_AuthorisationEstablishmentAddressGuidFindBox.Focus();
						break;
					}
			}
		}

		void ChangeVisibilityQH_StorageEstablishment()
		{
			var storageLocation = CurrentInvoice?.QuarantineExDocHeader?.QH_StorageLocation ?? EXDOCCodeOrganisation.Codes.Code;

			QH_StorageEstablishmentTextBox.Visible = storageLocation == EXDOCCodeOrganisation.Codes.Code;
			QH_OA_StorageEstablishmentAddressGuidFindBox.Visible = storageLocation == EXDOCCodeOrganisation.Codes.Organisation;

			if (storageLocation == EXDOCCodeOrganisation.Codes.Code)
			{
				QH_StorageEstablishmentTextBox.Focus();
			}

			if (storageLocation == EXDOCCodeOrganisation.Codes.Organisation)
			{
				QH_OA_StorageEstablishmentAddressGuidFindBox.Focus();
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
