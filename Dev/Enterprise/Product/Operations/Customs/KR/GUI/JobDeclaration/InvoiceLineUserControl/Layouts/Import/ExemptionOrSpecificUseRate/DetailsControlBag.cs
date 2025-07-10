using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DetailsControlBag : ControlBag
	{
		DetailsControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;
			DutyReductionTypeDropEdit = RegisterControl(nameof(DetailsUserControl.DutyReductionTypeDropEdit));
			SpecificUseCheckBox = RegisterControl(nameof(DetailsUserControl.SpecificUseCheckBox));
			DutyReductionCodeFindBox = RegisterControl(nameof(DetailsUserControl.DutyReductionCodeFindBox));
			InstalmentCodeFindBox = RegisterControl(nameof(DetailsUserControl.InstalmentCodeFindBox));
			RemarkLongTextControl = RegisterControl(nameof(DetailsUserControl.RemarkLongTextControl));
			RemarkTextBox = RegisterControl(nameof(DetailsUserControl.RemarkTextBox));
		}

		readonly BindingContext bindingContext;
		public static DetailsControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new DetailsControlBag(BindingContext.InvoiceLine));
		public static DetailsControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new DetailsControlBag(BindingContext.MessageSending));

		[ThreadStatic]
		static DetailsControlBag instanceForDeclaration;
		[ThreadStatic]
		static DetailsControlBag instanceForMessageSendingObject;

		public ControlReference DutyReductionTypeDropEdit { get; }
		public ControlReference SpecificUseCheckBox { get; }
		public ControlReference DutyReductionCodeFindBox { get; }
		public ControlReference InstalmentCodeFindBox { get; }
		public ControlReference RemarkLongTextControl { get; }
		public ControlReference RemarkTextBox { get; }

		protected override Control CreateTemplate()
		{
			var result = new DetailsUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}
	}
}
