using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();

		MiscOptionsControlBag()
		{
			AsycudaRelatedDeclarationsUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.AsycudaRelatedDeclarationsUserControl));
			PaymentAccountNumberTextBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.PaymentAccountNumberTextBox));
		}

		public ControlReference AsycudaRelatedDeclarationsUserControl { get; }
		public ControlReference PaymentAccountNumberTextBox { get; }
	}
}
