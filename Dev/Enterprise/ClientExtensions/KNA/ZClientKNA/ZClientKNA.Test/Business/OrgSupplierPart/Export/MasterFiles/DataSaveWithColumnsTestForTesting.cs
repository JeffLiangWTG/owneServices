using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DataSaveWithColumnsTestForTesting : DataSaveWithColumns
	{
		protected override List<String> NewColumnNames()
		{
			List<String> result = new List<string>(3);
			result.Add("Column 1");
			result.Add("Column 2");
			result.Add("Column 3");
			return result;
		}

		protected override OCsvLine ProcessDataForThisBizo(BusinessObject bizo)
		{
			OrgHeader org = bizo as OrgHeader;
			List<String> values = new List<string>(3);
			values.Add(org.OH_Code);
			values.Add(org.OH_FullName);
			values.Add(org.OH_Language);
			return new OCsvLine(values.ToArray());
		}

		protected override BusinessObjectCollection NewBusinessObjectCollection()
		{
			BusinessObjectCollection result = new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "ABI"));
			result.Load();
			return result;
		}
	}
}
