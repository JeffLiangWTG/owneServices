using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		MiscOptionsControlBag()
		{
			NACCSCredentialGuidDropEdit = RegisterControl(nameof(NACCSCredentialGuidDropEdit));
			PaymentOptionsSeparatorUserControl = RegisterControl(nameof(PaymentOptionsSeparatorUserControl));
			PaymentPartyDropEdit = RegisterControl(nameof(PaymentPartyDropEdit));
			PaymentDeadlineExtensionDropEdit = RegisterControl(nameof(PaymentDeadlineExtensionDropEdit));
		}

		public ControlReference NACCSCredentialGuidDropEdit { get; }
		public ControlReference PaymentOptionsSeparatorUserControl { get; }
		public ControlReference PaymentPartyDropEdit { get; }
		public ControlReference PaymentDeadlineExtensionDropEdit { get; }

		protected override Control CreateTemplate() => new MiscOptionsLayoutTemplate();
	}
}
