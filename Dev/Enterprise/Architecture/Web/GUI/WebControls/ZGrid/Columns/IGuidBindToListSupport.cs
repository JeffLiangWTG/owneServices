
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface IGuidBindToListSupport : IBindToListSupport
	{
		OComboBoxDropDownStyle DisplayStyle { get; }
	}
}
