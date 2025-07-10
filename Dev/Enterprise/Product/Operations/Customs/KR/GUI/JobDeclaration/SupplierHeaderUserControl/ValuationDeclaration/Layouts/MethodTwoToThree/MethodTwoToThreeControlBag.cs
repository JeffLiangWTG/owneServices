using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MethodTwoToThreeControlBag : ControlBag
	{
		protected override Control CreateTemplate()
		{
			var result = new ImportMethodTwoToThreeUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}

		[ThreadStatic]
		static MethodTwoToThreeControlBag instanceForDeclaration;
		[ThreadStatic]
		static MethodTwoToThreeControlBag instanceForSendingObject;

		public static MethodTwoToThreeControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new MethodTwoToThreeControlBag(BindingContext.InvoiceHeader));
		public static MethodTwoToThreeControlBag InstanceForSendingObject => instanceForSendingObject ?? (instanceForSendingObject = new MethodTwoToThreeControlBag(BindingContext.MessageSending));

		MethodTwoToThreeControlBag(BindingContext bindingContext)
		{
			ReplacementAmountCalcFindBox = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.ReplacementAmountCalcFindBox));
			ReplacementExchangeRateCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.ReplacementExchangeRateCalcEdit));
			ReplacementAmountKRWCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.ReplacementAmountKRWCalcEdit));

			AdditionalAdjustmentQuantityDiscountCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.AdditionalAdjustmentQuantityDiscountCalcEdit));
			AdditionalAdjustmentCommercialAmountCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.AdditionalAdjustmentCommercialAmountCalcEdit));
			AdditionalAdjustmentTransportationCostCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.AdditionalAdjustmentTransportationCostCalcEdit));
			AdditionalAdjustmentShippingPortCostCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.AdditionalAdjustmentShippingPortCostCalcEdit));
			AdditionalAdjustmentInsuranceCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.AdditionalAdjustmentInsuranceCalcEdit));
			TotalAdditionalAdjustmentAmountCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.TotalAdditionalAdjustmentAmountCalcEdit));

			DeductionAdjustmentQuantityDiscountCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.DeductionAdjustmentQuantityDiscountCalcEdit));
			DeductionAdjustmentCommercialAmountCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.DeductionAdjustmentCommercialAmountCalcEdit));
			DeductionAdjustmentTransportationCostCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.DeductionAdjustmentTransportationCostCalcEdit));
			DeductionAdjustmentShippingPortCostCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.DeductionAdjustmentShippingPortCostCalcEdit));
			DeductionAdjustmentInsuranceCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.DeductionAdjustmentInsuranceCalcEdit));
			TotalDeductionAdjustmentAmountCalcEdit = RegisterControl(nameof(ImportMethodTwoToThreeUserControl.TotalDeductionAdjustmentAmountCalcEdit));
			this.bindingContext = bindingContext;
		}
		readonly BindingContext bindingContext;

		public ControlReference ReplacementAmountCalcFindBox { get; }
		public ControlReference ReplacementExchangeRateCalcEdit { get; }
		public ControlReference ReplacementAmountKRWCalcEdit { get; }

		public ControlReference AdditionalAdjustmentQuantityDiscountCalcEdit { get; }
		public ControlReference AdditionalAdjustmentCommercialAmountCalcEdit { get; }
		public ControlReference AdditionalAdjustmentTransportationCostCalcEdit { get; }
		public ControlReference AdditionalAdjustmentShippingPortCostCalcEdit { get; }
		public ControlReference AdditionalAdjustmentInsuranceCalcEdit { get; }
		public ControlReference TotalAdditionalAdjustmentAmountCalcEdit { get; }

		public ControlReference DeductionAdjustmentQuantityDiscountCalcEdit { get; }
		public ControlReference DeductionAdjustmentCommercialAmountCalcEdit { get; }
		public ControlReference DeductionAdjustmentTransportationCostCalcEdit { get; }
		public ControlReference DeductionAdjustmentShippingPortCostCalcEdit { get; }
		public ControlReference DeductionAdjustmentInsuranceCalcEdit { get; }
		public ControlReference TotalDeductionAdjustmentAmountCalcEdit { get; }
	}
}
