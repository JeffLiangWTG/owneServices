using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MethodFiveToSixControlBag : ControlBag
	{
		[ThreadStatic]
		static MethodFiveToSixControlBag instanceForDeclaration;
		[ThreadStatic]
		static MethodFiveToSixControlBag instanceForSendingObject;

		public static MethodFiveToSixControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new MethodFiveToSixControlBag(BindingContext.InvoiceHeader));
		public static MethodFiveToSixControlBag InstanceForSendingObject => instanceForSendingObject ?? (instanceForSendingObject = new MethodFiveToSixControlBag(BindingContext.MessageSending));
		protected override Control CreateTemplate()
		{
			var result = new ImportMethodFiveToSixUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}

		MethodFiveToSixControlBag(BindingContext bindingContext)
		{
			AmountAgreedUponWithCustomsKRWCalcEdit = RegisterControl(nameof(ImportMethodFiveToSixUserControl.AmountAgreedUponWithCustomsKRWCalcEdit));
			AdditionalCostFreightToArrivalPortCalcEdit = RegisterControl(nameof(ImportMethodFiveToSixUserControl.AdditionalCostFreightToArrivalPortCalcEdit));
			AdditionalCostFreightToDeparturePortCalcEdit = RegisterControl(nameof(ImportMethodFiveToSixUserControl.AdditionalCostFreightToDeparturePortCalcEdit));
			AdditionalCostInsuranceCalcEdit = RegisterControl(nameof(ImportMethodFiveToSixUserControl.AdditionalCostInsuranceCalcEdit));
			AdditionalCostTotalAdditionalAmountCalcEdit = RegisterControl(nameof(ImportMethodFiveToSixUserControl.AdditionalCostTotalAdditionalAmountCalcEdit));
			this.bindingContext = bindingContext;
		}
		readonly BindingContext bindingContext;

		public ControlReference AmountAgreedUponWithCustomsKRWCalcEdit { get; }
		public ControlReference AdditionalCostFreightToArrivalPortCalcEdit { get; }
		public ControlReference AdditionalCostFreightToDeparturePortCalcEdit { get; }
		public ControlReference AdditionalCostInsuranceCalcEdit { get; }
		public ControlReference AdditionalCostTotalAdditionalAmountCalcEdit { get; }
	}
}
