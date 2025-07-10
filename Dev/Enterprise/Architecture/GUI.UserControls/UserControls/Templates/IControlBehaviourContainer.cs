using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IControlBehaviourContainer
	{
		ControlBehaviour ControlBehaviour { get; }

		IReadOnlyCollection<ZPropertyInfo> GetDependencies(BusinessObject businessObject);

		void UpdateControlBehaviour(Control control, BusinessObject dataItem);

		bool IsRefreshRequired(BusinessObject dataItem, Control control);
	}
}
