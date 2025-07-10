using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		public MiscOptionsControlBag() : base()
		{
			PaymentSeparatorUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.PaymentSeparatorUserControl));
		}

		public ControlReference PaymentSeparatorUserControl { get; }

		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
	}
}
