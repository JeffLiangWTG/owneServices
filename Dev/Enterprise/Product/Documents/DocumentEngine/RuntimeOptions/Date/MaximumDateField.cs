using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class MaximumDateField : DateField
	{
		public MaximumDateField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal MaximumDateField(MaximumDateFieldJsonData data)
			: base(data)
		{
		}

		#endregion

		protected override BaseDateFieldJsonData<DateTime> CreateJsonDataCore() => new MaximumDateFieldJsonData();

		protected override string NonEmptyWhereClause()
		{
			return FieldName + " < " + UpperParam;
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			var result = new SqlParameterList();
			if (IsEmpty)
			{
				return result;
			}

			SetUpperSQLParameterValue();

			result.Add(UpperParam);
			return result;
		}
	}
}
