using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.DB.Helpers
{
	public static class XsdBooleanDataTypeHelper
	{
		public static string[] GetBooleanExcludedProperties()
		{
			var boolExcludedProperties
			= new[]
			{
				OrgContactSchema.Constants.OC_Gender
			};

			return boolExcludedProperties;
		}

		public static bool XsdIsBool(IColumnDef columnDef)
		{
			if (columnDef.DefaultValue != null &&
			   (columnDef.DefaultValue.ToString() == BoolDefaultValue.False || columnDef.DefaultValue.ToString() == BoolDefaultValue.True) &&
			   !GetBooleanExcludedProperties().Contains(columnDef.Name))
			{
				return true;
			}
			return false;
		}
	}
}
