using System;
using System.Data;

namespace Enterprise.BusinessObjectGenerator
{
	class AutoIsValidProperty : AutoProperty
	{
		public AutoIsValidProperty(BusinessObjectInfo info, DataColumn column, int longestColumnNameLength)
			: base(info, column, longestColumnNameLength)
		{
		}

		public static bool MatchesNamingRequirements(string columnName)
		{
			return
				columnName.EndsWith(CargoWise.Schema.Schema.IsValidColumnSuffix, StringComparison.CurrentCultureIgnoreCase) ||
				columnName.EndsWith(CargoWise.Schema.Schema.IsValidColumnSuffix + "_Hidden", StringComparison.CurrentCultureIgnoreCase); // for AU AddInfo
		}

		public override string Code
		{
			get
			{
				return LinesOfCode(
					"",
					"		#region Light Validation Persistence",
					"",
					"		SchemaBoolColumn ILightValidationInternals.IsValidSchemaColumn",
					"		{",
					"			get { return " + Info.Table + "Schema." + ColumnName + "; }",
					"		}",
					"",
					"		" + ReturnType + " ILightValidationInternals.IsValid",
					"		{",
								CodeForPropertyGet,
					"			set",
					"			{",
					"				if (((ILightValidationInternals)this).IsValid != value)",
					"				{",
					"					((IBusinessObjectInternals)this).Row[Schema." + ColumnName + "] = ((IZTypeInternals)value).GetValueForLogicalDataLayer(false);",
					"				}",
					"			}",
					"		}",
					"",
					"		bool ILightValidationInternals.IsValidHasChanges",
					"		{",
					"			get { return IsInDatabase && !((ILightValidationInternals)this).IsValid.Equals(((IBusinessObjectInternals)this).GetValueFromRowSafely(Schema." + ColumnName + ", DataRowVersion.Original)); }",
					"		}",
					"",
					"		#endregion"
				);
			}
		}

		public override bool GenerateValidation
		{
			get { return false; }
		}
	}
}
