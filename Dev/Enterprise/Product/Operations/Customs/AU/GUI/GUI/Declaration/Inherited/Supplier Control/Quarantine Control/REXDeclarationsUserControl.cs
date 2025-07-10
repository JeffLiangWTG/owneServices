using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class REXDeclarationsUserControl : ZUserControl
	{
		public REXDeclarationsUserControl()
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
			}
		}

		void HookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceTypeInfo_ValueChanged;
			}

			ProduceTypeValueChanged();
		}

		void QH_ProduceTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ProduceTypeValueChanged();
		}

		void ProduceTypeValueChanged()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			var isVisible = exDocHeader?.IsNEXDOCSActive ?? false;

			QH_LegallyImportedFlagCaption.Visible = isVisible;
			QH_LegallyImportedFlagDropEdit.Visible = isVisible;

			var produceType = exDocHeader?.QH_ProduceType ?? ZString.Empty;

			REX_QH_ImportedProductFlagCaption.Text = produceType == EXDOCCommodityCodes.Codes.Fish
				? Res.GetString("0EC8681F-46CC-4374-AD89-9AB139B5A41E", "Are any of the products listed in the RFP imported?")
				: Res.GetString("5663A438-FC20-41DB-8533-2870B5129CEA", "Do any of the products listed in this RFP contain imported dairy ingredients, other than from New Zealand?");
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
