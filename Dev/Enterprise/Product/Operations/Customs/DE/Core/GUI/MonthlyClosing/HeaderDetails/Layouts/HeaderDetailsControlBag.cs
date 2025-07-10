using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class HeaderDetailsControlBag : ControlBag
	{
		HeaderDetailsControlBag()
		{
			IsDeclarantImporterCheckBox = RegisterControl(nameof(HeaderDetailsTemplateUserControl.IsDeclarantImporterCheckBox));
			RegistrationNumberTextBox = RegisterControl(nameof(HeaderDetailsTemplateUserControl.RegistrationNumberTextBox));
			IsFinalizedCheckBox = RegisterControl(nameof(HeaderDetailsTemplateUserControl.IsFinalizedCheckBox));
			BranchGuidFindBox = RegisterControl(nameof(HeaderDetailsTemplateUserControl.BranchGuidFindBox));
			UnlinkedDeclarationsNumberLabel = RegisterControl(nameof(HeaderDetailsTemplateUserControl.UnlinkedDeclarationsNumberLabel));
		}

		public static HeaderDetailsControlBag Instance => instance ?? (instance = new HeaderDetailsControlBag());

		[ThreadStatic]
		static HeaderDetailsControlBag instance;

		protected override Control CreateTemplate() => new HeaderDetailsTemplateUserControl();

		public ControlReference IsDeclarantImporterCheckBox { get; }

		public ControlReference RegistrationNumberTextBox { get; }

		public ControlReference IsFinalizedCheckBox { get; }

		public ControlReference BranchGuidFindBox { get; }

		public ControlReference UnlinkedDeclarationsNumberLabel { get; }
	}
}
