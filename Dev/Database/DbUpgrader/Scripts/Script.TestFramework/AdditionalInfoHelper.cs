using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Build.Database.Script.TestFramework
{
	public class AdditionalInfoHelper
	{
		public AdditionalInfoHelper(IEnumerable<AdditionalInfoConfig> additionalInfoConfigs)
		{
			Configurations = additionalInfoConfigs;
		}

		public IEnumerable<AdditionalInfoConfig> Configurations { get; private set; }

		public string SelectList => string.Join(", ", Configurations.Select(c => c.ColumnName));

		public string AdditionalInfoText => string.Join("*", Configurations.Select(c => $"{c.AdditionalInfoKey}={(c.Value is DateTime d ? d.ToSqlFormat() : c.Value)}"));
	}

	public class AdditionalInfoConfig
	{
		public AdditionalInfoConfig(string additionalInfoKey, object value)
		{
			ColumnName = additionalInfoKey;
			AdditionalInfoKey = additionalInfoKey;
			Value = value;
		}

		public AdditionalInfoConfig(string columnName, string additionalInfoKey, object value)
		{
			ColumnName = columnName;
			AdditionalInfoKey = additionalInfoKey;
			Value = value;
		}

		public string ColumnName { get; private set; }
		public string AdditionalInfoKey { get; private set; }
		public object Value { get; private set; }
	}
}
