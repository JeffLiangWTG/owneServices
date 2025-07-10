using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MethodFourControlBag : ControlBag
	{
		[ThreadStatic]
		static MethodFourControlBag instanceForDeclaration;
		[ThreadStatic]
		static MethodFourControlBag instanceForSendingObject;

		public static MethodFourControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new MethodFourControlBag(BindingContext.InvoiceHeader));
		public static MethodFourControlBag InstanceForSendingObject => instanceForSendingObject ?? (instanceForSendingObject = new MethodFourControlBag(BindingContext.MessageSending));
		protected override Control CreateTemplate()
		{
			var result = new ImportMethodFourUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}
		MethodFourControlBag(BindingContext context)
		{
			SalesOfHighestQuantityAmountCalcFindBox = RegisterControl(nameof(ImportMethodFourUserControl.SalesOfHighestQuantityAmountCalcFindBox));
			SalesOfHighestQuantityExchangeRateCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.SalesOfHighestQuantityExchangeRateCalcEdit));
			SalesOfHighestQuantityAmountKRWCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.SalesOfHighestQuantityAmountKRWCalcEdit));

			DeductionCostCustomsReferenceNumberTextBox = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostCustomsReferenceNumberTextBox));
			DeductionCostConsignmentSalesFeeCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostConsignmentSalesFeeCalcEdit));
			DeductionCostGeneralCostCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostGeneralCostCalcEdit));
			DeductionCostCostRateCodeDropEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostCostRateCodeDropEdit));
			DeductionCostCostRateCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostCostRateCalcEdit));
			DeductionCostTransportationCostCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostTransportationCostCalcEdit));
			DeductionCosInsuranceCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCosInsuranceCalcEdit));
			DeductionCostUnloadCostCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostUnloadCostCalcEdit));
			DeductionCostOtherTransportationCostsCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostOtherTransportationCostsCalcEdit));
			DeductionCostAdditionalCostCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostAdditionalCostCalcEdit));
			DeductionCosTaxCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCosTaxCalcEdit));
			DeductionCostTotalDeductionAmountCalcEdit = RegisterControl(nameof(ImportMethodFourUserControl.DeductionCostTotalDeductionAmountCalcEdit));

			PercentageLabel = RegisterControl(nameof(ImportMethodFourUserControl.PercentageLabel));
			this.bindingContext = context;
		}
		readonly BindingContext bindingContext;

		public ControlReference SalesOfHighestQuantityAmountCalcFindBox { get; }
		public ControlReference SalesOfHighestQuantityExchangeRateCalcEdit { get; }
		public ControlReference SalesOfHighestQuantityAmountKRWCalcEdit { get; }

		public ControlReference DeductionCostCustomsReferenceNumberTextBox { get; }
		public ControlReference DeductionCostConsignmentSalesFeeCalcEdit { get; }
		public ControlReference DeductionCostGeneralCostCalcEdit { get; }
		public ControlReference DeductionCostCostRateCodeDropEdit { get; }
		public ControlReference DeductionCostCostRateCalcEdit { get; }
		public ControlReference DeductionCostTransportationCostCalcEdit { get; }
		public ControlReference DeductionCosInsuranceCalcEdit { get; }
		public ControlReference DeductionCostUnloadCostCalcEdit { get; }
		public ControlReference DeductionCostOtherTransportationCostsCalcEdit { get; }
		public ControlReference DeductionCostAdditionalCostCalcEdit { get; }
		public ControlReference DeductionCosTaxCalcEdit { get; }
		public ControlReference DeductionCostTotalDeductionAmountCalcEdit { get; }

		public ControlReference PercentageLabel { get; }
	}
}
