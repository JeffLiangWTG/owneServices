using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IValueSetStrategy
	{
		void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue);
	}
}
