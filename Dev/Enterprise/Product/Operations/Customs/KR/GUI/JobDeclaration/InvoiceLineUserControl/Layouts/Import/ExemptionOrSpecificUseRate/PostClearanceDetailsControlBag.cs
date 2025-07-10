using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class PostClearanceDetailsControlBag : ControlBag
	{
		PostClearanceDetailsControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;
			PostClearanceYNDropEdit = RegisterControl(nameof(PostClearanceDetailsUserControl.PostClearanceYNDropEdit));
			UseCodeDescriptionTextBox = RegisterControl(nameof(PostClearanceDetailsUserControl.UseCodeDescriptionTextBox));
			ProductTypeDropEdit = RegisterControl(nameof(PostClearanceDetailsUserControl.ProductTypeDropEdit));
			SerialNumberTextBox = RegisterControl(nameof(PostClearanceDetailsUserControl.SerialNumberTextBox));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(PostClearanceDetailsUserControl.CustomsOfficeCodeFindBox));
			GoodsLocationAddressControl = RegisterControl(nameof(PostClearanceDetailsUserControl.GoodsLocationAddressControl));
		}

		readonly BindingContext bindingContext;
		public static PostClearanceDetailsControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new PostClearanceDetailsControlBag(BindingContext.InvoiceLine));
		public static PostClearanceDetailsControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new PostClearanceDetailsControlBag(BindingContext.MessageSending));

		[ThreadStatic]
		static PostClearanceDetailsControlBag instanceForDeclaration;
		[ThreadStatic]
		static PostClearanceDetailsControlBag instanceForMessageSendingObject;

		public ControlReference PostClearanceYNDropEdit { get; }
		public ControlReference UseCodeDescriptionTextBox { get; }
		public ControlReference ProductTypeDropEdit { get; }
		public ControlReference SerialNumberTextBox { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference GoodsLocationAddressControl { get; }

		protected override Control CreateTemplate()
		{
			var result = new PostClearanceDetailsUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}
	}
}
