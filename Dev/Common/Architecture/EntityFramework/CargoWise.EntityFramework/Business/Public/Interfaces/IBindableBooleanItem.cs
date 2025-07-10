using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IBindableBooleanItem : IBusiness
	{
		ZBool BoolValue { get; set; }
		ZPropertyInfo BoolValueInfo { get; }
		ZString Text { get; }
	}
}
