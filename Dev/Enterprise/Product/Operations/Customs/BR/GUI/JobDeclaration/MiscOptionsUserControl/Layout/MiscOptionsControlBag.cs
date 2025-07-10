using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		public MiscOptionsControlBag() : base()
		{
			PaymentSeparatorUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.PaymentSeparatorUserControl));
			BankAccountGuidFindBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.BankAccountGuidFindBox));
		}

		public ControlReference PaymentSeparatorUserControl { get; }

		public ControlReference BankAccountGuidFindBox { get; }

		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
	}
}
