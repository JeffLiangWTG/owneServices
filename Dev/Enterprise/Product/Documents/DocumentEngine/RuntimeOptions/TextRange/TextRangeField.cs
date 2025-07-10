using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class TextRangeField : FilterFieldWithUTSupport, IJsonSerializable
	{
		public TextRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal TextRangeField(TextRangeFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			From = data.From;
			To = data.To;
		}

		#endregion

		void CreateParameters()
		{
			fFromParam = new SqlParameter(SqlParameterNameGenerator.Next(), "");
			fToParam = new SqlParameter(SqlParameterNameGenerator.Next(), "");
			ParameterList.Add(fFromParam);
			ParameterList.Add(fToParam);
		}

		SqlParameter fFromParam, fToParam;

		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".FromText", new ValueReplacers.ReplacementProviderMethod(GetFromRangeReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ToText", new ValueReplacers.ReplacementProviderMethod(GetToRangeReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromText>", ResString.GetMultilingualString("deef529f-ad16-49db-8f34-87015a20357d", "Returns the From text of the filter.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToText>", ResString.GetMultilingualString("0edbab19-cc8f-4792-8dcc-bab0eb9d5611", "Returns the To text of the filter.")));
		}

		protected object GetFromRangeReplacement(string macro, Report report)
		{
			return From;
		}

		protected object GetToRangeReplacement(string macro, Report report)
		{
			return To;
		}

		public string From
		{
			get { return (string)fFromParam.Value; }
			set { fFromParam.Value = value; }
		}

		public string To
		{
			get { return (string)fToParam.Value; }
			set { fToParam.Value = value; }
		}

		public override bool IsEmpty
		{
			get
			{
				return string.IsNullOrEmpty(From) && string.IsNullOrEmpty(To);
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			if (!string.IsNullOrEmpty(From) && !string.IsNullOrEmpty(To))
			{
				return String.Format((NoResString)@"{0} BETWEEN {1} AND {2} OR {0} LIKE {2} + '%'", FieldName, fFromParam, fToParam);
			}
			else if (!string.IsNullOrEmpty(From) && string.IsNullOrEmpty(To))
			{
				return String.Format(@"{0} >= {1}", FieldName, fFromParam);
			}
			else if (string.IsNullOrEmpty(From) && !string.IsNullOrEmpty(To))
			{
				return String.Format((NoResString)@"{0} <= {1} OR {0} LIKE {1} + '%'", FieldName, fToParam);
			}
			else
			{
				// Should never happen - only called if IsEmpty() == false
				throw new DocumentEngineException("Where clause generation logic error");
			}
		}

#if DEBUG
		public override void ClearValueForUnitTest()
		{
			fFromParam.Value = "";
			fToParam.Value = "";
		}
#endif
		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get
			{
				return FilterFieldSuggestedUserControlType.TextRangeFieldUserControl;
			}
		}

		public override object ValueAsObject
		{
			get
			{
				return Res.GetString("93bb0160-d771-4b2a-82e5-95b86e2fbf2a", "From '{0}' to '{1}'", From.Replace("\r", ""), To.Replace("\r", ""));
			}
		}

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is TextRangeField)
			{
				this.From = ((TextRangeField)source).From;
				this.To = ((TextRangeField)source).To;
			}
		}

		public override void ClearValues()
		{
			this.From = String.Empty;
			this.To = String.Empty;
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new TextRangeFilter();
			SetBaseFilterData(filterData);

			filterData.From = From;
			filterData.To = To;

			reportFilterData.TextRangeFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.TextRangeFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				From = selectedValue.From;
				To = selectedValue.To;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new TextRangeFieldJsonData()
			{
				From = From,
				To = To
			};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
