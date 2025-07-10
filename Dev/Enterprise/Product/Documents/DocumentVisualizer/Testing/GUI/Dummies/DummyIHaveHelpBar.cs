using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.GUI;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	class DummyIHaveHelpBar : Control, IHaveHelpBar
	{
		public HelpBarUserControl HelpBar { get; set; }

		public IEnumerable<KeyValuePair<string, string>> KeyboardHints
		{
			get
			{
				if (!Empty)
				{
					yield return new KeyValuePair<string, string>("Tab", "Tab to another control");
					yield return new KeyValuePair<string, string>("Ctrl+K", "Make the world explode");
				}
			}
		}

		public bool Empty { get; set; }
	}
}
