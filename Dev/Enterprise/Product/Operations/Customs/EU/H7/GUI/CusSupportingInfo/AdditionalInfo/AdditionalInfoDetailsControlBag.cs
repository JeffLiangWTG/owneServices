using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class AdditionalInfoDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new AdditionalInfoDetailsControl();

		[ThreadStatic]
		static AdditionalInfoDetailsControlBag instance;
		public static AdditionalInfoDetailsControlBag Instance => instance ?? (instance = new AdditionalInfoDetailsControlBag());

		protected AdditionalInfoDetailsControlBag()
		{
			AddInfoTypeCodeDropEdit = RegisterControl(nameof(AdditionalInfoDetailsControl.AddInfoTypeCodeDropEdit));
			AddInfoDescriptionTextBox = RegisterControl(nameof(AdditionalInfoDetailsControl.AddInfoDescriptionTextBox));
		}

		public ControlReference AddInfoTypeCodeDropEdit { get; }
		public ControlReference AddInfoDescriptionTextBox { get; }
	}
}
