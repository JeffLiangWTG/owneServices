using System.Data;
using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common
{
	public static class NativeXMLIsSystemValidator
	{
		public static bool IsSystemRowNonEditable(IEntity entity, DataRow row)
		{
			var listOfSystemNonEditableTableNames
			= new[]
			{
				ProcessTaskTemplateSchema.Constants.TableName
			};
			if (listOfSystemNonEditableTableNames.Contains(entity.TableName))
			{
				var isSystemProperty = entity.Definition.PropertyDefinitions.FirstOrDefault(p => p.PropertyName.Equals("IsSystem"));
				var isSystemFromRow = (bool)row[isSystemProperty.ColumnDef.Name];
				if (isSystemFromRow)
				{
					return true;
				}
			}
			return false;
		}
	}
}
