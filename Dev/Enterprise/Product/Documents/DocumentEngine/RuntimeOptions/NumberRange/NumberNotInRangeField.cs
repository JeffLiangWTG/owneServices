using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class NumberNotInRangeField : FilterFieldWithUTSupport, IJsonSerializable
	{
		public NumberNotInRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal NumberNotInRangeField(NumberNotInRangeFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			From = data.From;
			To = data.To;
		}

		#endregion

		void CreateParameters()
		{
			fFromParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Int);
			fToParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Int);
			fFromParam.Value = DBNull.Value;
			fToParam.Value = DBNull.Value;
			ParameterList.Add(fFromParam);
			ParameterList.Add(fToParam);
		}

		SqlParameter fFromParam, fToParam;

		[BusinessObjectTestExclude]
		public ZDecimal? From
		{
			get { return fFromParam.Value == DBNull.Value ? null : (Decimal?)fFromParam.Value; }
			set { fFromParam.Value = (Decimal?)value; }
		}

		protected bool FromIsEmpty
		{
			get { return fFromParam.Value == null || fFromParam.Value == DBNull.Value; }
		}

		[BusinessObjectTestExclude]
		public ZDecimal? To
		{
			get { return fToParam.Value == DBNull.Value ? null : (Decimal?)fToParam.Value; }
			set { fToParam.Value = (Decimal?)value; }
		}

		protected bool ToIsEmpty
		{
			get { return fToParam.Value == null || fToParam.Value == DBNull.Value; }
		}

		public override bool IsEmpty
		{
			get { return FromIsEmpty && ToIsEmpty; }
		}

		#region Debug Only
#if DEBUG
		public override void ClearValueForUnitTest()
		{
			fFromParam.Value = DBNull.Value;
			fToParam.Value = DBNull.Value;
		}
#endif
		#endregion

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			if (!FromIsEmpty && !ToIsEmpty)
			{
				return String.Format((NoResString)@"{0} < {1} OR {0} > {2}", FieldName, fFromParam, fToParam);
			}
			else if (!FromIsEmpty && ToIsEmpty)
			{
				return String.Format(@"{0} < {1}", FieldName, fFromParam);
			}
			else if (FromIsEmpty && !ToIsEmpty)
			{
				return String.Format(@"{0} > {1}", FieldName, fToParam);
			}
			else
			{
				// Should never happen - only called if IsEmpty() == false
				throw new DocumentEngineException("Where clause generation logic error");
			}
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get
			{
				return FilterFieldSuggestedUserControlType.NumberNotInRangeUserControl;
			}
		}

		#region Special Value Providers

		protected override void AddSpecialisedValueProviders()
		{
			base.AddSpecialisedValueProviders();
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ValueFrom", new ReplacementProviderMethod(GetValueFromReplacement)));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ValueTo", new ReplacementProviderMethod(GetValueToReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ValueFrom>", ResString.GetMultilingualString("3c871558-7de5-4edc-83bd-45d4514a9e23", "Returns the From value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ValueTo>", ResString.GetMultilingualString("62f7b189-10f9-4eec-8d62-a0f71663759c", "Returns the To value.")));
		}

		protected object GetValueFromReplacement(string macro, Report report)
		{
			return (object)From ?? DBNull.Value;
		}

		protected object GetValueToReplacement(string macro, Report report)
		{
			return (object)To ?? DBNull.Value;
		}

		#endregion

		#region ISingleValueProvider Members

		public override object ValueAsObject
		{
			get
			{
				return Res.GetString("eee0e0e0-c3de-417a-a6a7-190439128922", "Not between {0} and {1}", fFromParam.Value.ToString(), fToParam.Value.ToString());
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is NumberNotInRangeField)
			{
				this.To = ((NumberNotInRangeField)source).To;
				this.From = ((NumberNotInRangeField)source).From;
			}
		}

		public override void ClearValues()
		{
			this.To = null;
			this.From = null;
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			var pList = IsEmpty ? new SqlParameterList() : ParameterList;
			foreach (var parameter in pList)
			{
				if (parameter.SqlValue == null)
				{
					parameter.SqlValue = DBNull.Value;
				}
			}

			return pList;
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new NumberNotInRangeFilter();
			SetBaseFilterData(filterData);

			filterData.From = From;
			filterData.To = To;

			reportFilterData.NumberNotInRangeFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.NumberNotInRangeFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				From = selectedValue.From;
				To = selectedValue.To;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new NumberNotInRangeFieldJsonData()
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
