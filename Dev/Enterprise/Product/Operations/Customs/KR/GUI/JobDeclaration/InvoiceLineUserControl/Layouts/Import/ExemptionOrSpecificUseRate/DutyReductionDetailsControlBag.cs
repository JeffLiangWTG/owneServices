using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DutyReductionDetailsControlBag : ControlBag
	{
		DutyReductionDetailsControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;
			GroupNumberDropEdit = RegisterControl(nameof(DutyReductionDetailsUserControl.GroupNumberDropEdit));
			SeqNumberTextBox = RegisterControl(nameof(DutyReductionDetailsUserControl.SeqNumberTextBox));
			ItemNumberTextBox = RegisterControl(nameof(DutyReductionDetailsUserControl.ItemNumberTextBox));
		}

		readonly BindingContext bindingContext;
		public static DutyReductionDetailsControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new DutyReductionDetailsControlBag(BindingContext.InvoiceLine));
		public static DutyReductionDetailsControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new DutyReductionDetailsControlBag(BindingContext.MessageSending));

		[ThreadStatic]
		static DutyReductionDetailsControlBag instanceForDeclaration;
		[ThreadStatic]
		static DutyReductionDetailsControlBag instanceForMessageSendingObject;

		public ControlReference GroupNumberDropEdit { get; }
		public ControlReference SeqNumberTextBox { get; }
		public ControlReference ItemNumberTextBox { get; }

		protected override Control CreateTemplate()
		{
			var result = new DutyReductionDetailsUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}
	}
}
