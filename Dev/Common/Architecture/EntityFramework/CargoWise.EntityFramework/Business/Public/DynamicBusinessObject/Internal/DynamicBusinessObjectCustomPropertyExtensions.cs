using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public static class DynamicBusinessObjectCustomPropertyExtensions
	{
		public static string[] GetOrderedCustomProperties(this IDynamicBusinessObject obj)
		{
			return obj.PropertyNames.OrderBy(name => obj.GetProperty(name), new DynamicBusinessObjectPropertyComparer()).ThenBy(n => n).ToArray();
		}

		public static int? GetPosition(this DynamicBusinessObjectProperty property)
		{
			var metadata = property.GetMetaData(MetaDataTypes.Position);
			if (metadata != null)
			{
				return (int?)metadata.Value;
			}

			return null;
		}

		public static string GetCaption(this DynamicBusinessObjectProperty property)
		{
			var metadata = property.GetMetaData(MetaDataTypes.Description);
			if (metadata != null)
			{
				var description = (IDescription)metadata.Value;
				return description.GetDescription(0, CultureInfo.CurrentCulture);
			}
			else
			{
				return null;
			}
		}
	}
}
