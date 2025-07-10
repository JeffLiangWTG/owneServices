using System.ComponentModel.Design;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Implement on a designable component to provide designer action items.
	/// For example, you could implement the interface on a text box to provide smart tags at design time.
	/// </summary>
	public interface IDesignerActionItemSource
	{
		DesignerActionItem[] GetSortedActionItems(DesignerActionList actionList);
	}
}
