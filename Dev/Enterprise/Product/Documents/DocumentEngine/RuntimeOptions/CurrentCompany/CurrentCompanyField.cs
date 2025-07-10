using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class CurrentCompanyField : FilterField, IJsonSerializable
	{
		public CurrentCompanyField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal CurrentCompanyField(CurrentCompanyFieldJsonData data)
			: base(data)
		{
			CreateParameters();
		}

		#endregion

		internal void CreateParameters()
		{
			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), GlbCompany.CurrentCompany.PK.ToGuid());
			ParameterList.Add(fParam);
		}

		SqlParameter fParam;

		#region Overrides

		public override bool IsEmpty
		{
			get
			{
				return false;
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		protected override string NonEmptyWhereClause()
		{
			return FieldName + " = " + fParam;
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get
			{
				return FilterFieldSuggestedUserControlType.None;
			}
		}

		public override object ValueAsObject
		{
			get
			{
				return fParam.Value;
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
		}

		public override void ClearValues()
		{
		}

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new CurrentCompanyFieldJsonData();
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
