using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class COOandFTAControlBag : ControlBag
	{
		COOandFTAControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;
			GoodsOriginCodeFindBox = RegisterControl(nameof(COOandFTAControlsUserControl.GoodsOriginCodeFindBox));
			CODeterminationRuleDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.CODeterminationRuleDropEdit));
			COLabelLocationDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.COLabelLocationDropEdit));
			COLabelTypeDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.COLabelTypeDropEdit));
			COLabelExemptionReasonDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.COLabelExemptionReasonDropEdit));
			CoveredByCOOExporterSystemCheckBox = RegisterControl(nameof(COOandFTAControlsUserControl.CoveredByCOOExporterSystemCheckBox));

			COIssuingCountryCodeFindBox = RegisterControl(nameof(COOandFTAControlsUserControl.COIssuingCountryCodeFindBox));
			COIssueDateEdit = RegisterControl(nameof(COOandFTAControlsUserControl.COIssueDateEdit));
			COReferenceNumberTextBox = RegisterControl(nameof(COOandFTAControlsUserControl.COReferenceNumberTextBox));
			COCodeDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.COCodeDropEdit));
			IssuingAgencyNameTextBox = RegisterControl(nameof(COOandFTAControlsUserControl.IssuingAgencyNameTextBox));
			IssuingAreaNameTextBox = RegisterControl(nameof(COOandFTAControlsUserControl.IssuingAreaNameTextBox));
			IssuingPersonNameTextBox = RegisterControl(nameof(COOandFTAControlsUserControl.IssuingPersonNameTextBox));
			COSplitYNDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.COSplitYNDropEdit));

			ProductTypeDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.ProductTypeDropEdit));
			CountryInvIssuedDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.CountryInvIssuedDropEdit));
			CountryCodeFindBox = RegisterControl(nameof(COOandFTAControlsUserControl.CountryCodeFindBox));
			ExporterNumberTextBox = RegisterControl(nameof(COOandFTAControlsUserControl.ExporterNumberTextBox));
			SplitOrderCalcEdit = RegisterControl(nameof(COOandFTAControlsUserControl.SplitOrderCalcEdit));
			SupportingDocTypeDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.SupportingDocTypeDropEdit));
			IssuerTypeDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.IssuerTypeDropEdit));
			TotalNetWeightCalcEdit = RegisterControl(nameof(COOandFTAControlsUserControl.TotalNetWeightCalcEdit));
			UQDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.UQDropEdit));

			SequenceNoCalcEdit = RegisterControl(nameof(COOandFTAControlsUserControl.SequenceNoCalcEdit));
			UsedQuantityCalcEdit = RegisterControl(nameof(COOandFTAControlsUserControl.UsedQuantityCalcEdit));
			UsedUQDropEdit = RegisterControl(nameof(COOandFTAControlsUserControl.UsedUQDropEdit));
		}

		readonly BindingContext bindingContext;
		public static COOandFTAControlBag Instance => instance ?? (instance = new COOandFTAControlBag(BindingContext.InvoiceLine));
		public static COOandFTAControlBag InstanceForInvoicerHeader => instanceForInvoicerHeader ?? (instanceForInvoicerHeader = new COOandFTAControlBag(BindingContext.InvoiceHeader));

		[ThreadStatic]
		static COOandFTAControlBag instance;
		[ThreadStatic]
		static COOandFTAControlBag instanceForInvoicerHeader;
		public ControlReference GoodsOriginCodeFindBox { get; }
		public ControlReference CODeterminationRuleDropEdit { get; }
		public ControlReference COLabelLocationDropEdit { get; }
		public ControlReference COLabelTypeDropEdit { get; }
		public ControlReference COLabelExemptionReasonDropEdit { get; }

		public ControlReference COIssuingCountryCodeFindBox { get; }
		public ControlReference COIssueDateEdit { get; }
		public ControlReference COReferenceNumberTextBox { get; }
		public ControlReference COCodeDropEdit { get; }
		public ControlReference IssuingAgencyNameTextBox { get; }
		public ControlReference IssuingAreaNameTextBox { get; }
		public ControlReference IssuingPersonNameTextBox { get; }
		public ControlReference COSplitYNDropEdit { get; }

		public ControlReference ProductTypeDropEdit { get; }
		public ControlReference CountryInvIssuedDropEdit { get; }
		public ControlReference CountryCodeFindBox { get; }
		public ControlReference CoveredByCOOExporterSystemCheckBox { get; }
		public ControlReference ExporterNumberTextBox { get; }
		public ControlReference SplitOrderCalcEdit { get; }
		public ControlReference SupportingDocTypeDropEdit { get; }
		public ControlReference IssuerTypeDropEdit { get; }
		public ControlReference TotalNetWeightCalcEdit { get; }
		public ControlReference UQDropEdit { get; }

		public ControlReference SequenceNoCalcEdit { get; }
		public ControlReference UsedQuantityCalcEdit { get; }
		public ControlReference UsedUQDropEdit { get; }

		protected override Control CreateTemplate()
		{
			var result = new COOandFTAControlsUserControl();
			if (bindingContext == BindingContext.InvoiceHeader)
			{
				result.BindToInvoiceHeader();
			}
			return result;
		}
	}
}
