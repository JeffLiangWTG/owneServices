using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.ZArchitecture.GUI
{
	public class DefaultControlExtensionCollection : ControlExtensionCollection
	{
		public DefaultControlExtensionCollection(IExtendedControl owner) : base(owner)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				Add(new HintExtension());
				Add(new StatusbarExtension());
				Add(new TrainingModeExtension());
				Add(new ValidationExtension());
				Add(new NotificationExtension());
			}
			Add(new ZLabelCaptionRenderer());
		}
	}
}
