using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IDurationBasedDateConverter
	{
		ZDateTime ConvertToDurationBasedDate(ZDateTime date);
	}
}
