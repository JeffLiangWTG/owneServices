using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question5To7ControlBag : ControlBag
	{
		public static Question5To7ControlBag InstanceFor5SM => instanceFor5SM ?? (instanceFor5SM = new Question5To7ControlBag(BindingContext.Declaration5SM));
		[ThreadStatic]
		static Question5To7ControlBag instanceFor5SM;

		public static Question5To7ControlBag InstanceFor934 => instanceFor934 ?? (instanceFor934 = new Question5To7ControlBag(BindingContext.InvoiceHeader));
		[ThreadStatic]
		static Question5To7ControlBag instanceFor934;

		public static Question5To7ControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new Question5To7ControlBag(BindingContext.MessageSending));
		[ThreadStatic]
		static Question5To7ControlBag instanceForMessageSendingObject;

		Question5To7ControlBag(BindingContext bindingContext)
		{
			Question5ALabel = RegisterControl(nameof(Question5To7UserControl.Question5ALabel));
			Question5ALongLabel = RegisterControl(nameof(Question5To7UserControl.Question5ALongLabel));
			Question5ADropEdit = RegisterControl(nameof(Question5To7UserControl.Question5ADropEdit));
			Question5BLabel = RegisterControl(nameof(Question5To7UserControl.Question5BLabel));
			Question5BLongLabel = RegisterControl(nameof(Question5To7UserControl.Question5BLongLabel));
			Question5BDropEdit = RegisterControl(nameof(Question5To7UserControl.Question5BDropEdit));
			Question5CLabel = RegisterControl(nameof(Question5To7UserControl.Question5CLabel));
			Question5CDropEdit = RegisterControl(nameof(Question5To7UserControl.Question5CDropEdit));
			Question5DLabel = RegisterControl(nameof(Question5To7UserControl.Question5DLabel));
			Question5DDropEdit = RegisterControl(nameof(Question5To7UserControl.Question5DDropEdit));
			Question5ELabel = RegisterControl(nameof(Question5To7UserControl.Question5ELabel));
			Question5EDropEdit = RegisterControl(nameof(Question5To7UserControl.Question5EDropEdit));
			Question5ETextBox = RegisterControl(nameof(Question5To7UserControl.Question5ETextBox));
			Question5ETextLabel = RegisterControl(nameof(Question5To7UserControl.Question5ETextLabel));
			Question6ALabel = RegisterControl(nameof(Question5To7UserControl.Question6ALabel));
			Question6ADropEdit = RegisterControl(nameof(Question5To7UserControl.Question6ADropEdit));
			Question6BLabel = RegisterControl(nameof(Question5To7UserControl.Question6BLabel));
			Question6BDropEdit = RegisterControl(nameof(Question5To7UserControl.Question6BDropEdit));
			Question7ALabel = RegisterControl(nameof(Question5To7UserControl.Question7ALabel));
			Question7ADropEdit = RegisterControl(nameof(Question5To7UserControl.Question7ADropEdit));
			Question7BLabel = RegisterControl(nameof(Question5To7UserControl.Question7BLabel));
			Question7BDropEdit = RegisterControl(nameof(Question5To7UserControl.Question7BDropEdit));
			this.bindingContext = bindingContext;
		}
		readonly BindingContext bindingContext;

		protected override Control CreateTemplate()
		{
			var result = new Question5To7UserControl();
			if (bindingContext == BindingContext.InvoiceHeader)
			{
				result.BindTo934();
			}
			else if (bindingContext == BindingContext.MessageSending)
			{
				result.BindTo934();
				result.BindToMessageSendingObject();
			}
			return result;
		}

		public ControlReference Question5ALabel { get; set; }
		public ControlReference Question5ALongLabel { get; set; }
		public ControlReference Question5ADropEdit { get; set; }
		public ControlReference Question5BLabel { get; set; }
		public ControlReference Question5BLongLabel { get; set; }
		public ControlReference Question5BDropEdit { get; set; }
		public ControlReference Question5CLabel { get; set; }
		public ControlReference Question5CDropEdit { get; set; }
		public ControlReference Question5DLabel { get; set; }
		public ControlReference Question5DDropEdit { get; set; }
		public ControlReference Question5ELabel { get; set; }
		public ControlReference Question5ETextLabel { get; set; }
		public ControlReference Question5EDropEdit { get; set; }
		public ControlReference Question5ETextBox { get; set; }
		public ControlReference Question6ALabel { get; set; }
		public ControlReference Question6ADropEdit { get; set; }
		public ControlReference Question6BLabel { get; set; }
		public ControlReference Question6BDropEdit { get; set; }
		public ControlReference Question7ALabel { get; set; }
		public ControlReference Question7ADropEdit { get; set; }
		public ControlReference Question7BLabel { get; set; }
		public ControlReference Question7BDropEdit { get; set; }
	}
}
