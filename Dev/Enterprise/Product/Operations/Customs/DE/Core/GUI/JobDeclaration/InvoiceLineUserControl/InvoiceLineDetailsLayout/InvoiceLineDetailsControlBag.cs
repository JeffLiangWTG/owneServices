using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		InvoiceLineDetailsControlBag()
		{
			CessionManagementFlagDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CessionManagementFlagDropEdit));
			SupplementaryInformationTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryInformationTextBox));
			OriginFederalStateDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.OriginFederalStateDropEdit));
			InvoiceNumberDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.InvoiceNumberDropEdit));
			QuotaQtyCalcDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.QuotaQtyCalcDropEdit));
			TobaccoStampTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.TobaccoStampTextBox));
			IsMainPackCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.IsMainPackCheckBox));
			UsualReplacementCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.UsualReplacementCheckBox));
			ReimportDateEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ReimportDateEdit));
			ExportCountryCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExportCountryCodeFindBox));
			DecisiveDateEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.DecisiveDateEdit));
			OutwardMRNTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.OutwardMRNTextBox));
			OutwardDecisiveDateEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.OutwardDecisiveDateEdit));
			NetPriceCurrencyCalcFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.NetPriceCurrencyCalcFindBox));
			DgSubstanceUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.DgSubstanceUserControl));
			DescriptionLongTextControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.DescriptionLongTextControl));
			FixedMaxLengthEntryInstructionGuidDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.FixedMaxLengthEntryInstructionGuidDropEdit));
			FixedMaxLengthWithDescriptionTariffFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.FixedMaxLengthWithDescriptionTariffFindBox));
			FixedMaxLengthInvoiceNumberDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.FixedMaxLengthInvoiceNumberDropEdit));
			BondedWHSOrderNumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.BondedWHSOrderNumberTextBox));
			BondedWHSOrderLineNumberCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.BondedWHSOrderLineNumberCalcEdit));
		}

		public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

		[ThreadStatic]
		static InvoiceLineDetailsControlBag instance;

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

		public ControlReference CessionManagementFlagDropEdit { get; }

		public ControlReference SupplementaryInformationTextBox { get; }

		public ControlReference OriginFederalStateDropEdit { get; }

		public ControlReference InvoiceNumberDropEdit { get; }

		public ControlReference QuotaQtyCalcDropEdit { get; }

		public ControlReference TobaccoStampTextBox { get; }

		public ControlReference IsMainPackCheckBox { get; }

		public ControlReference UsualReplacementCheckBox { get; }

		public ControlReference ReimportDateEdit { get; }

		public ControlReference ExportCountryCodeFindBox { get; }

		public ControlReference DecisiveDateEdit { get; }

		public ControlReference OutwardMRNTextBox { get; }

		public ControlReference OutwardDecisiveDateEdit { get; }

		public ControlReference BondedWHSOrderNumberTextBox { get; }

		public ControlReference BondedWHSOrderLineNumberCalcEdit { get; }

		public ControlReference NetPriceCurrencyCalcFindBox { get; }

		public ControlReference DgSubstanceUserControl { get; }

		public ControlReference DescriptionLongTextControl { get; set; }

		public ControlReference FixedMaxLengthInvoiceNumberDropEdit { get; }

		public ControlReference FixedMaxLengthWithDescriptionTariffFindBox { get; }

		public ControlReference FixedMaxLengthEntryInstructionGuidDropEdit { get; }
	}
}
