using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public static class ZDateTimeExtensions
	{
		public static ZDateTime ConvertToDurationBasedDate(this ZDateTime value, ZPropertyInfo propertyInfo)
		{
			var attribute = propertyInfo.PropertyDescriptor.Attributes
							.OfType<IDurationBasedDateConverter>()
							.FirstOrDefault();

			return attribute?.ConvertToDurationBasedDate(value) ?? value;
		}
	}
}
