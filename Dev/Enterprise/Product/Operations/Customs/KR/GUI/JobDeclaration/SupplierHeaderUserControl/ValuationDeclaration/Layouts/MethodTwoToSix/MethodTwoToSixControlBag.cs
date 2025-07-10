using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MethodTwoToSixControlBag : ControlBag
	{
		MethodTwoToSixControlBag(BindingContext bindingContext)
		{
			ExpectedCustomsValueCalcEdit = RegisterControl(nameof(MethodTwoToSixUserControl.ExpectedCustomsValueCalcEdit));
			SupportingDocument1TextBox = RegisterControl(nameof(MethodTwoToSixUserControl.SupportingDocument1TextBox));
			SupportingDocument2TextBox = RegisterControl(nameof(MethodTwoToSixUserControl.SupportingDocument2TextBox));
			SampleItemCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.SampleItemCheckBox));
			AdvertisingUseCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.AdvertisingUseCheckBox));
			UseOfDefectiveRepairCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.UseOfDefectiveRepairCheckBox));
			ReplacementItemCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.ReplacementItemCheckBox));
			GiftOrFreeDonationCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.GiftOrFreeDonationCheckBox));
			ForProductionAndManufactureCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.ForProductionAndManufactureCheckBox));
			ItemUseCodeOtherReasonTextBox = RegisterControl(nameof(MethodTwoToSixUserControl.ItemUseCodeOtherReasonTextBox));
			PerformancePriceOfPaidTransactionCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.PerformancePriceOfPaidTransactionCheckBox));
			PriceListCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.PriceListCheckBox));
			ManufacturingCostCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.ManufacturingCostCheckBox));
			InvoiceCheckBox = RegisterControl(nameof(MethodTwoToSixUserControl.InvoiceCheckBox));
			GoodsPricingBasisOtherReasonTextBox = RegisterControl(nameof(MethodTwoToSixUserControl.GoodsPricingBasisOtherReasonTextBox));
			this.bindingContext = bindingContext;
		}
		readonly BindingContext bindingContext;

		[ThreadStatic]
		static MethodTwoToSixControlBag instanceForDeclaration;
		[ThreadStatic]
		static MethodTwoToSixControlBag instanceForSendingObject;

		public static MethodTwoToSixControlBag Instance => InstanceForDeclaration;
		public static MethodTwoToSixControlBag InstanceForDeclaration => instanceForDeclaration ??= new MethodTwoToSixControlBag(BindingContext.InvoiceHeader);
		public static MethodTwoToSixControlBag InstanceForSendingObject => instanceForSendingObject ??= new MethodTwoToSixControlBag(BindingContext.MessageSending);
		protected override Control CreateTemplate()
		{
			var result = new MethodTwoToSixUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}
		public ControlReference ExpectedCustomsValueCalcEdit { get; }
		public ControlReference SupportingDocument1TextBox { get; }
		public ControlReference SupportingDocument2TextBox { get; }
		public ControlReference SampleItemCheckBox { get; }
		public ControlReference AdvertisingUseCheckBox { get; }
		public ControlReference UseOfDefectiveRepairCheckBox { get; }
		public ControlReference ReplacementItemCheckBox { get; }
		public ControlReference GiftOrFreeDonationCheckBox { get; }
		public ControlReference ForProductionAndManufactureCheckBox { get; }
		public ControlReference ItemUseCodeOtherReasonTextBox { get; }
		public ControlReference PerformancePriceOfPaidTransactionCheckBox { get; }
		public ControlReference PriceListCheckBox { get; }
		public ControlReference ManufacturingCostCheckBox { get; }
		public ControlReference InvoiceCheckBox { get; }
		public ControlReference GoodsPricingBasisOtherReasonTextBox { get; }
	}
}
