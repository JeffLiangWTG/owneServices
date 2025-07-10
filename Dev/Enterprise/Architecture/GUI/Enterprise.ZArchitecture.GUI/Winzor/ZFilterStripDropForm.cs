using System.Linq;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.ZArchitecture.GUI.Controls.Interfaces;

namespace Enterprise.ZArchitecture.GUI.Internal;

public partial class ZFilterStripDropForm
{
	protected override bool IsCommonItem(ICodeDescription item)
	{
		return Helper.IsCommonItem(item);
	}

	protected override bool IsCategoryItem(ICodeDescription item)
	{
		return Helper.IsCategoryItem(item);
	}

	protected override string ZDropFormItemClassString(ICodeDescription item)
	{
		var classString = base.ZDropFormItemClassString(item);
		if (Helper.IsSeparator(item))
		{
			classString += " zdropform__item--separator";
		}
		else if (IsCategoryItem(item))
		{
			classString += " zdropform__item--category";
		}
		else if (Helper.IsFilterAlreadySelected(item))
		{
			classString += " zdropform__item--active";
		}
		else if (Helper.IsExclusiveItem(item))
		{
			classString += " zdropform__item--exclusive";
		}
		return classString;
	}
}
