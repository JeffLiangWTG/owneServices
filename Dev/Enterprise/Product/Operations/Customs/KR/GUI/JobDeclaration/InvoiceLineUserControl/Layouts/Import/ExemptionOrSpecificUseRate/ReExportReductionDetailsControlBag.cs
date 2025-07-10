using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ReExportReductionDetailsControlBag : ControlBag
	{
		ReExportReductionDetailsControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;
			CustomsOfficeCodeFindBox = RegisterControl(nameof(ReExportReductionDetailsUserControl.CustomsOfficeCodeFindBox));
			DestCountryCodeFindBox = RegisterControl(nameof(ReExportReductionDetailsUserControl.DestCountryCodeFindBox));
			EstimateDateEdit = RegisterControl(nameof(ReExportReductionDetailsUserControl.EstimateDateEdit));
		}

		readonly BindingContext bindingContext;
		public static ReExportReductionDetailsControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new ReExportReductionDetailsControlBag(BindingContext.InvoiceLine));
		public static ReExportReductionDetailsControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new ReExportReductionDetailsControlBag(BindingContext.MessageSending));

		[ThreadStatic]
		static ReExportReductionDetailsControlBag instanceForDeclaration;
		[ThreadStatic]
		static ReExportReductionDetailsControlBag instanceForMessageSendingObject;

		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference DestCountryCodeFindBox { get; }
		public ControlReference EstimateDateEdit { get; }

		protected override Control CreateTemplate()
		{
			var result = new ReExportReductionDetailsUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}
	}
}
