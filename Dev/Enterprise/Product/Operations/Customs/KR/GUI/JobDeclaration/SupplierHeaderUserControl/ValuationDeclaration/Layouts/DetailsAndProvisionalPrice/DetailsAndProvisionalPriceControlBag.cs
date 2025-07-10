using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DetailsAndProvisionalPriceControlBag : ControlBag
	{
		DetailsAndProvisionalPriceControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;
			InvoiceNoTextBox = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.InvoiceNoTextBox));
			InvoiceDateEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.InvoiceDateEdit));
			PurchaseOrderNoTextBox = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.PurchaseOrderNoTextBox));
			PurchaseOrderDateEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.PurchaseOrderDateEdit));
			ContractNoTextBox = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.ContractNoTextBox));
			ContractDateEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.ContractDateEdit));
			TotalCustomsValueCalcEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.TotalCustomsValueCalcEdit));

			ProvisionalPricingDropEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.ProvisionalPricingDropEdit));
			ProvisionalAdditionRateCalcEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.ProvisionalAdditionRateCalcEdit));
			ProvisionalAdditionAmountCalcEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.ProvisionalAdditionAmountCalcEdit));
			EstimatedDateOfFinalPriceDateEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.EstimatedDateOfFinalPriceDateEdit));
			ContractExpirationDateEdit = RegisterControl(nameof(DetailsAndProvisionalPriceUserControl.ContractExpirationDateEdit));
		}

		readonly BindingContext bindingContext;
		public static DetailsAndProvisionalPriceControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new DetailsAndProvisionalPriceControlBag(BindingContext.InvoiceHeader));
		public static DetailsAndProvisionalPriceControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new DetailsAndProvisionalPriceControlBag(BindingContext.MessageSending));

		[ThreadStatic]
		static DetailsAndProvisionalPriceControlBag instanceForDeclaration;
		[ThreadStatic]
		static DetailsAndProvisionalPriceControlBag instanceForMessageSendingObject;

		public ControlReference InvoiceNoTextBox { get; }
		public ControlReference InvoiceDateEdit { get; }
		public ControlReference PurchaseOrderNoTextBox { get; }
		public ControlReference PurchaseOrderDateEdit { get; }
		public ControlReference ContractNoTextBox { get; }
		public ControlReference ContractDateEdit { get; }
		public ControlReference TotalCustomsValueCalcEdit { get; }

		public ControlReference ProvisionalPricingDropEdit { get; }
		public ControlReference ProvisionalAdditionRateCalcEdit { get; }
		public ControlReference ProvisionalAdditionAmountCalcEdit { get; }
		public ControlReference EstimatedDateOfFinalPriceDateEdit { get; }
		public ControlReference ContractExpirationDateEdit { get; }

		protected override Control CreateTemplate()
		{
			var result = new DetailsAndProvisionalPriceUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}
	}
}
