using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPMessagingUserControl : ZUserControl
	{
		public RFPMessagingUserControl()
		{
			InitializeComponent();

			var queryInterchangeCreator = new QueryInterchangeCreator(MessageCollectionGrid);
			queryInterchangeCreator.AddColumnAndMenuForQuery();
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

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var invoice = CurrentDataItem as JobComInvoiceHeader;
			declaration = invoice?.JobDeclaration;

			if (declaration != null)
			{
				declaration.AddInfo.ZA_IsAQISCertificateRequest_HiddenInfo.ValueChanged += ZA_IsAQISCertificateRequest_ValueChanged;
				CertificateRequestViewChanges();
			}
		}

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
			}
		}

		void HookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceTypeInfo_ValueChanged;
			}

			QH_ProduceTypeInfo_ValueChanged(null, null);
		}

		void QH_ProduceTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetQH_RequestForPermitNumberTextBoxCaption();
		}

		void SetQH_RequestForPermitNumberTextBoxCaption()
		{
			if (declaration?.IsAQISCertificateRequest ?? false)
			{
				QH_RequestForPermitNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("8c9f0e29-af03-4e4e-826e-3175a51942ab", "Certificate Identification");
			}
			else if (CurrentInvoice?.QuarantineExDocHeader?.IsNEXDOCSActive ?? false)
			{
				QH_RequestForPermitNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("332ecd6d-f55f-45f8-b622-e7424375df4f", "REX Number");
			}
			else
			{
				QH_RequestForPermitNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("3c2e87c6-568c-48df-8262-f947cf3c52c4", "RFP Number");
			}
		}

		#region ZA_IsAQISCertificateRequest_ValueChanged

		void ZA_IsAQISCertificateRequest_ValueChanged(object sender, EventArgs e)
		{
			CertificateRequestViewChanges();
		}

		void CertificateRequestViewChanges()
		{
			if (declaration?.IsAQISCertificateRequest ?? false)
			{
				BindingSource.SetBindingMember(QH_RequestForPermitNumberStatusDescriptionTextBox, nameof(QuarantineExDocHeader) + "+" + nameof(QuarantineExDocHeader.CertificateStatusDescription));
				BindingSource.SetBindingMember(QH_RequestForPermitNumberTextBox, nameof(QuarantineExDocHeader) + "+" + nameof(QuarantineExDocHeader.CertificateRequestNumber));
				QH_RequestForPermitNumberStatusDescriptionTextBox.ReadOnly = QH_RequestForPermitNumberTextBox.ReadOnly = true;
			}
			else
			{
				BindingSource.SetBindingMember(QH_RequestForPermitNumberStatusDescriptionTextBox, nameof(QuarantineExDocHeader) + "+" + nameof(QuarantineExDocHeader.QH_RequestForPermitNumberStatusDescription));
				BindingSource.SetBindingMember(QH_RequestForPermitNumberTextBox, nameof(QuarantineExDocHeader) + "+" + nameof(QuarantineExDocHeader.QH_RequestForPermitNumber));
			}

			QH_RequestForPermitNumberStatusDescriptionTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("ca2e2da9-798b-402c-bd57-6d4df4547bb3", "Status Desc.");
			SetQH_RequestForPermitNumberTextBoxCaption();
		}

		JobDeclaration declaration;

		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			UnHookEvents();

			if (declaration != null)
			{
				declaration.AddInfo.ZA_IsAQISCertificateRequest_HiddenInfo.ValueChanged -= ZA_IsAQISCertificateRequest_ValueChanged;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
