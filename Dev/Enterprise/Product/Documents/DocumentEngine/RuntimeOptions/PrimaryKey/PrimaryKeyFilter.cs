using System;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class PrimaryKeyFilter : IFilter, IJsonSerializable
	{
		public PrimaryKeyFilter(string fieldName, Guid guidValue)
		{
			CreateParameters(guidValue);
			this.FieldName = fieldName;
		}

		#region Constructor For IJsonSerializable

		internal PrimaryKeyFilter(PrimaryKeyFilterJsonData data)
		{
			CreateParameters(data.GuidValue);
			FieldName = data.FieldName;
		}

		#endregion

		void CreateParameters(Guid guidValue)
		{
			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), guidValue);
			ParameterList = new SqlParameterList();
			ParameterList.Add(fParam);
		}

		public string GetXmlFragment()
		{
			return "";
		}

		public string WhereClause()
		{
			return FieldName + " = " + fParam;
		}

		public SqlParameterList SqlParameters()
		{
			return ParameterList;
		}

		protected string FieldName;
		protected SqlParameter fParam;
		protected SqlParameterList ParameterList;

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new PrimaryKeyFilterJsonData
			{
				GuidValue = (Guid)fParam.Value,
				FieldName = FieldName
			};

		#endregion

		#region IFilter Members

		void IFilter.SafeCopyValuesFrom(IFilter source)
		{
			throw new NotImplementedException();
		}

		void IFilter.ClearValues()
		{
			throw new NotImplementedException();
		}

		#endregion

#if DEBUG
		public string GetFieldName()
		{
			return FieldName;
		}
#endif

	}
}
