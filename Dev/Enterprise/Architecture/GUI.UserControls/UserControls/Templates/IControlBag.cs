using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IControlBag
	{
		Control TemplateControl { get; }
		void CreateControls(Control container, IDictionary<ControlReference, Control> controlsByReference);
		bool ContainsControl(string name);
		IReadOnlyCollection<ControlReference> Controls { get; }
	}
}
