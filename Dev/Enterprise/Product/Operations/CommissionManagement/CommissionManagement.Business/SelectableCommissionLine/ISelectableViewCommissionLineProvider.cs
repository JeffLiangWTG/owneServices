using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public interface ISelectableViewCommissionLineProvider : IViewCommissionLineProvider
	{
		ZBool IsSelected { get; set; }
		ZPropertyInfo IsSelectedInfo { get; }
	}
}
