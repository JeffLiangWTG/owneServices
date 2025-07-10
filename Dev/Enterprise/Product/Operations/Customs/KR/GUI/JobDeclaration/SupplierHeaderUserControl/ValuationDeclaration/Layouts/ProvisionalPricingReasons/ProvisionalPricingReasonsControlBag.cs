using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ProvisionalPricingReasonsControlBag : ControlBag
	{
		[ThreadStatic]
		static ProvisionalPricingReasonsControlBag instanceForDeclaration;
		[ThreadStatic]
		static ProvisionalPricingReasonsControlBag instanceForMessageSendingObject;

		public static ProvisionalPricingReasonsControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new ProvisionalPricingReasonsControlBag(BindingContext.InvoiceHeader));
		public static ProvisionalPricingReasonsControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new ProvisionalPricingReasonsControlBag(BindingContext.MessageSending));

		ProvisionalPricingReasonsControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;
			ProvisionalPricingReason101CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason101CheckBox));
			ProvisionalPricingReason102CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason102CheckBox));
			ProvisionalPricingReason103CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason103CheckBox));
			ProvisionalPricingReason104CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason104CheckBox));
			ProvisionalPricingReason105CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason105CheckBox));
			ProvisionalPricingReason106CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason106CheckBox));
			ProvisionalPricingReason107CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason107CheckBox));
			ProvisionalPricingReason108CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason108CheckBox));
			ProvisionalPricingReason109CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason109CheckBox));
			ProvisionalPricingReason110CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason110CheckBox));
			ProvisionalPricingReason111CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason111CheckBox));
			ProvisionalPricingReason112CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason112CheckBox));
			ProvisionalPricingReason113CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason113CheckBox));
			ProvisionalPricingReason114CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason114CheckBox));
			ProvisionalPricingReason115CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason115CheckBox));
			ProvisionalPricingReason116CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason116CheckBox));
			ProvisionalPricingReason117CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason117CheckBox));
			ProvisionalPricingReason120CheckBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.ProvisionalPricingReason120CheckBox));
			OtherReasonTextBox = RegisterControl(nameof(ProvisionalPricingReasonsUserControl.OtherReasonTextBox));
		}

		protected override Control CreateTemplate()
		{
			var result = new ProvisionalPricingReasonsUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}

		readonly BindingContext bindingContext;
		public ControlReference ProvisionalPricingReason101CheckBox { get; }
		public ControlReference ProvisionalPricingReason102CheckBox { get; }
		public ControlReference ProvisionalPricingReason103CheckBox { get; }
		public ControlReference ProvisionalPricingReason104CheckBox { get; }
		public ControlReference ProvisionalPricingReason105CheckBox { get; }
		public ControlReference ProvisionalPricingReason106CheckBox { get; }
		public ControlReference ProvisionalPricingReason107CheckBox { get; }
		public ControlReference ProvisionalPricingReason108CheckBox { get; }
		public ControlReference ProvisionalPricingReason109CheckBox { get; }
		public ControlReference ProvisionalPricingReason110CheckBox { get; }
		public ControlReference ProvisionalPricingReason111CheckBox { get; }
		public ControlReference ProvisionalPricingReason112CheckBox { get; }
		public ControlReference ProvisionalPricingReason113CheckBox { get; }
		public ControlReference ProvisionalPricingReason114CheckBox { get; }
		public ControlReference ProvisionalPricingReason115CheckBox { get; }
		public ControlReference ProvisionalPricingReason116CheckBox { get; }
		public ControlReference ProvisionalPricingReason117CheckBox { get; }
		public ControlReference ProvisionalPricingReason120CheckBox { get; }
		public ControlReference OtherReasonTextBox { get; }
	}
}
