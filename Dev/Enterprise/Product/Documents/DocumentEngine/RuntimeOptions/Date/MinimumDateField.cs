using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class MinimumDateField : DateField
	{
		public MinimumDateField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal MinimumDateField(MinimumDateFieldJsonData data)
			: base(data)
		{
		}

		#endregion

		protected override BaseDateFieldJsonData<DateTime> CreateJsonDataCore() => new MinimumDateFieldJsonData();

		protected override string NonEmptyWhereClause()
		{
			return FieldName + " >= " + LowerParam;
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			var result = new SqlParameterList();
			if (IsEmpty)
			{
				return result;
			}

			SetLowerSQLParameterValue();

			result.Add(LowerParam);
			return result;
		}
	}
}
