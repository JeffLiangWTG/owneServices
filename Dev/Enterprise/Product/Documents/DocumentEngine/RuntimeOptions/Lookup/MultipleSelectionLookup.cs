using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class MultipleSelectionLookup : LookupFilterFieldBase, IJsonSerializable
#if DEBUG
, IValueAsStringProviderForUnitTests
#endif
	{
		public MultipleSelectionLookup(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal MultipleSelectionLookup(MultipleSelectionLookupJsonData data)
			: base(data)
		{
			IsFilterValueExcluded = data.IsFilterValueExcluded;
			if (!string.IsNullOrEmpty(data.LookupType))
			{
				var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, data.LookupType);
				SetCollectionProvider(provider);
			}
			SerialisedByPK = data.SerialisedByPK;
			Columns = data.Columns;
			UseCodesForWhereClause = data.UseCodesForWhereClause;
			ValueAsStringForSerialisation = data.ValueAsString ?? string.Empty;
		}

		#endregion

		public int MaxAllowableSelections { get; set; }

		public void DefaultCurrentCountryToCountryMultipleSelectionLookup()
		{
			if (BindToList is RefCountryCollection refCountries && refCountries.Count == 0 && GlbCompany.CurrentCompany?.Country is RefCountry currentCountry)
			{
				refCountries.Add(currentCountry);
			}
		}

		#region Overrides

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			if (otherFilterField is MultipleSelectionLookup)
			{
				return CheckCollectionProviderHasSameTypeAsAnotherField(otherFilterField);
			}
			return false;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 1;

			if (BindToFindBoxList is ICompositeCollection)
			{
				return;
			}

			if (BindToFindBoxList is BusinessObjectCollection)
			{
				filter.OrderBy = EnterpriseSchema.GetTableSchema(BindToFindBoxList.TableName).All[0].Name;

				((BusinessObjectCollection)BindToFindBoxList).Load(filter);
			}
			else
			{
				((IActiveBusinessObjectCollection)BindToFindBoxList).AdditionalFilter = filter;
			}
			BusinessObject[] businessObjects = BindToFindBoxList.ToArray();
			if (businessObjects.Length > 0)
			{
				if (BindToList is BusinessObjectCollection)
				{
					BindToList.Add(businessObjects[0]);
				}
				else
				{
					((IActiveBusinessObjectCollection)BindToList).AdditionalFilter = (((IActiveBusinessObjectCollection)BindToFindBoxList).AdditionalFilter);
				}
			}
			else
			{
				throw new DocumentEngineException("No items found in BindToList on lookup with ModuleID : " + ModuleID.ToString());
			}
		}

		public override void ClearValueForUnitTest()
		{
			if (BindToList is BusinessObjectCollection)
			{
				((BusinessObjectCollection)BindToList).RemoveAll();
			}
			else
			{
				((IActiveBusinessObjectCollection)BindToList).AdditionalFilter = new ZQuery();
			}
		}
#endif

		protected override SqlParameterList GetSqlParametersCore()
		{
			SqlParameterList parameters = new SqlParameterList();
			foreach (BusinessObject bizO in BindToList)
			{
				SqlParameter param = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.UniqueIdentifier);
				param.Value = bizO.PK.ToGuid();
				parameters.Add(param);
			}
			return parameters;
		}

		public override bool IsEmpty
		{
			get { return BindToList.Count == 0; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get
			{
				FilterFieldSuggestedUserControlType result = FilterFieldSuggestedUserControlType.None;
				switch (Style) // allowing for others in future
				{
					case Styles.None:
						result = FilterFieldSuggestedUserControlType.None;
						break;
					case Styles.Grid:
					default:
						result = FilterFieldSuggestedUserControlType.MultipleSelectionLookupUserControl;
						break;
				}
				return result;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			if (!IsValid)
			{
				this.AddRowError(ValidationError);
			}
			if ((BindToList.Notifications.FirstOrDefault(msg => msg.Message.Contains(invalidBizoErroMsg))) != null)
			{
				this.AddRowError(Res.GetString("d931aee4-cb55-412b-9a6b-af61f0074ce0", "Invalid code(s). Please update this filter."));
			}
			ValidateMaxAllowableSelections();
		}

		void ValidateMaxAllowableSelections()
		{
			if (MaxAllowableSelections != 0 && BindToList.Count > MaxAllowableSelections)
			{
				AddRowError(Res.GetString("1639702f-f573-461c-83de-8ea44af54c25", "You have chosen {0} items. Please choose up to {1} items or consider other filter criteria.", BindToList.Count, MaxAllowableSelections));
			}
		}

		public override object ValueAsObject
		{
			get { return GetCodeValues(); }
		}

		protected override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			PropertyDescriptorCollection result;

			if (listAccessors != null && listAccessors.Length == 1 && listAccessors[0].Name == "BindToList")
			{
				result = ZCustomTypeDescriptor.GetProperties(BindToList.TypeOfElements);
			}
			else
			{
				result = base.GetItemProperties(listAccessors);
			}

			return result;
		}

		protected override string NonEmptyWhereClause()
		{
			string whereClause = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} {1}IN (", FieldName, IsFilterValueExcluded ? "NOT " : "");

			if (UseCodesForWhereClause)
			{
				foreach (BusinessObject bizO in BindToList)
				{
					whereClause += string.Format("'{0}',", DataUtils.EscapeSingleQuotes(GetCodeFromBizo(bizO)));
				}
			}
			else
			{
				foreach (BusinessObject bizO in BindToList)
				{
					whereClause += string.Format("'{0}',", bizO.PK);
				}
			}

			return whereClause.Trim(',') + ")";
		}

		#endregion

		#region Specialised providers

		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".PKs", GetPKsReplacement));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".PKsAsTVP", GetPKsAsTVPReplacement));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".CodesAsTVP", GetCodesAsTVPReplacement));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.PKs>", ResString.GetMultilingualString("c4825f78-ad59-4046-88aa-ceee51dae9f7", "Returns PKs for the selected values as a comma separated list. Can be used for a SQL IN operation.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.PKsAsTVP>", ResString.GetMultilingualString("7bce9fcb-a39b-406c-b559-ffbf039dbfb4", "Returns PKS for the selected values as a SQL Table-Valued Parameter containing GUIDs")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.CodesAsTVP>", ResString.GetMultilingualString("5f973654-5fcf-4d6e-962d-980cfcb7dc70", "Returns Codes for the selected values as a SQL Table-Valued Parameter containing VARCHAR")));
		}

		object GetCodesAsTVPReplacement(string macro, Report report)
		{
			DataTable mappingDataTable = null;
			try
			{
				mappingDataTable = new DataTable();
				mappingDataTable.Locale = CultureInfo.InvariantCulture;
				mappingDataTable.Columns.Add((NoResString)"Value", typeof(string));

				foreach (BusinessObject bizO in BindToList)
				{
					mappingDataTable.Rows.Add(GetCodeFromBizo(bizO));
				}

				var result = new ReplacementWithParameterType(mappingDataTable, TVPHelper.TVP_varchar);
				mappingDataTable = null;
				return result;
			}
			finally
			{
				if (mappingDataTable != null)
				{
					mappingDataTable.Dispose();
				}
			}
		}

		object GetPKsAsTVPReplacement(string macro, Report report)
		{
			DataTable mappingDataTable = null;
			try
			{
				mappingDataTable = new DataTable();
				mappingDataTable.Locale = CultureInfo.InvariantCulture;
				mappingDataTable.Columns.Add((NoResString)"Value", typeof(Guid));

				foreach (BusinessObject bizO in BindToList)
				{
					mappingDataTable.Rows.Add(bizO.PK.ToGuid());
				}

				var result = new ReplacementWithParameterType(mappingDataTable, TVPHelper.TVP_uniqueidentifier);
				mappingDataTable = null;
				return result;
			}
			finally
			{
				if (mappingDataTable != null)
				{
					mappingDataTable.Dispose();
				}
			}
		}

		object GetPKsReplacement(string macro, Report report)
		{
			string pKs = "";
			foreach (BusinessObject bizO in BindToList)
			{
				pKs += string.Format("'{0}',", bizO.PK);
			}
			return pKs.Trim(',');
		}

		#endregion

		#region Style

		public Styles Style
		{
			get { return fStyle; }
			set { fStyle = value; }
		}

		Styles fStyle = Styles.Grid;

		public bool IsHidden => Style == Styles.None;

		public enum Styles { Grid, None }

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is MultipleSelectionLookup lookup)
			{
				BindToList.AddRange(lookup.GetCollectionProvider().Collection);
			}
		}

		public override void ClearValues()
		{
			BindToList.Clear();
		}

		#endregion

		public bool SerialisedByPK
		{
			get;
			set;
		}

		public bool UseCodesForWhereClause { get; set; }

		public T GetCollection<T>() where T : IBusinessObjectCollection
		{
			return CollectionProvider != null ? (T)CollectionProvider.Collection : default(T);
		}

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return SerialisedByPK ? GetPKValues() : GetCodeValues(); }
			set
			{
				if (SerialisedByPK)
				{
					LoadPKValues(value);
				}
				else
				{
					LoadCodeValues(value);
				}
			}
		}

		string GetCodeValues()
		{
			var result = new ZStringBuilder();

			if (BindToList != null)
			{
				foreach (BusinessObject bizO in BindToList)
				{
					result.Append(GetCodeFromBizo(bizO));
				}
			}

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		static string GetCodeFromBizo(BusinessObject bizO)
		{
			try
			{
				return CodePropertyAttribute.CodeFromBusinessObject(bizO);
			}
			catch (OdysseyException)
			{
				return bizO.HumanReadableName;
			}
		}

		string GetPKValues()
		{
			var result = new ZStringBuilder();

			if (BindToList != null)
			{
				foreach (BusinessObject businessObject in BindToList)
				{
					result.Append(businessObject.PK.ToString());
				}
			}

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		void LoadPKValues(string value)
		{
			if (CollectionProvider == null)
			{
				return;
			}

			ZQuery additionalFilter = new ZQuery();
			if (!CollectionProvider.Collection.CompleteFilter.IsEmpty && !string.IsNullOrEmpty(CollectionProvider.Collection.CompleteFilter.LiteralTextADO))
			{
				additionalFilter = CollectionProvider.Collection.CompleteFilter;
			}

			string[] pKs = value.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			if (BindToFindBoxList is ICompositeCollection compositeCollection)
			{
				foreach (string pk in pKs)
				{
					var pkGuid = new ZGuid(pk);
					var type = compositeCollection.TypeOfElementFromPK(pkGuid);
					var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(BusinessObjectFactory.GetTableNameFromType(type));
					var matchingBizOs = Factory.Load(type, new ZQuery(new ZQuery(column, pkGuid), additionalFilter));
					if (matchingBizOs.Length > 0)
					{
						BindToList.AddRange(matchingBizOs);
					}
				}
			}
			else
			{
				var type = BindToFindBoxList.TypeOfElements;
				var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(BusinessObjectFactory.GetTableNameFromType(type));
				foreach (string pk in pKs)
				{
					var matchingBizOs = Factory.Load(type, new ZQuery(new ZQuery(column, new ZGuid(pk)), additionalFilter));
					if (matchingBizOs.Length > 0)
					{
						BindToList.AddRange(matchingBizOs);
					}
				}
			}
		}

		ReadOnlyBusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (readOnlyFactory == null)
				{
					readOnlyFactory = new ReadOnlyBusinessObjectFactory();
				}
				return readOnlyFactory;
			}
		}
		ReadOnlyBusinessObjectFactory readOnlyFactory;

		void LoadCodeValues(string value)
		{
			if (CollectionProvider == null)
			{
				return;
			}

			var additionalFilter = new ZQuery();
			if (!CollectionProvider.Collection.CompleteFilter.IsEmpty && !string.IsNullOrEmpty(CollectionProvider.Collection.CompleteFilter.LiteralTextADO))
			{
				additionalFilter = CollectionProvider.Collection.CompleteFilter;
			}

			var codes = value.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			if (BindToFindBoxList is ICompositeCollection compositeCollection)
			{
				foreach (var code in codes)
				{
					var type = compositeCollection.TypeOfElementFromCode(code);
					var codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(type);
					var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(codePropertyName, BusinessObjectFactory.GetTableNameFromType(type));
					var matchingBizOs = Factory.Load(type, new ZQuery(new ZQuery(column, code), additionalFilter));
					if (matchingBizOs.Length > 0)
					{
						BindToList.AddRange(matchingBizOs);
					}
					else
					{
						var invalidBizo = ReadOnlyFactory.New(type);
						var codeFixed = code.Length > column.MaxLength ? code.Substring(0, column.MaxLength) : code;
						invalidBizo[codePropertyName] = codeFixed;
						invalidBizo.AddRowError(invalidBizoErroMsg);
						BindToList.Add(invalidBizo);
					}
				}
			}
			else
			{
				var type = BindToFindBoxList.TypeOfElements;
				var codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(type);
				var tableName = BusinessObjectFactory.GetTableNameFromType(type);
				var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(codePropertyName, tableName);
				MethodInfo codeQueryMethod = null;
				if (column == null)
				{
					codeQueryMethod = CodePropertyAttribute.QueryMethodFromType(type);
					if (codeQueryMethod == null)
					{
						ErrorReporter.ReportOnce("WrongCodePropertyAttributeImplementation1", $"Column '{column.Name}' does not exist in table '{column.TableName}'. Please add a code query method within CodePropertyAttribute.");
					}
				}

				foreach (var code in codes)
				{
					BusinessObject[] matchingBizOs = null;
					if (column != null)
					{
						matchingBizOs = Factory.Load(type, new ZQuery(new ZQuery(column, code), additionalFilter));
					}
					else if (codeQueryMethod != null)
					{
						var codeQuery = codeQueryMethod.Invoke(null, new object[] { SQLComparisonOperator.Equal, (ZString)code });
						if (codeQuery is ZQuery query)
						{
							matchingBizOs = Factory.Load(type, query);
						}
						else
						{
							ErrorReporter.ReportOnce("WrongCodePropertyAttributeImplementation2", $"CodePropertyAttribute implement a method '{codeQueryMethod.Name}', which should return ZQuery.");
						}
					}

					if (matchingBizOs != null && matchingBizOs.Length > 0)
					{
						BindToList.AddRange(matchingBizOs);
					}
					else
					{
						var invalidBizo = ReadOnlyFactory.New(type);
						if (column != null)
						{
							var codeFixed = code.Length > column.MaxLength ? code.Substring(0, column.MaxLength) : code;
							invalidBizo[codePropertyName] = codeFixed;
						}
						invalidBizo.AddRowError(invalidBizoErroMsg);
						BindToList.Add(invalidBizo);
					}
				}
			}
		}

		static string invalidBizoErroMsg => Res.GetString("457efd69-b1f3-4b39-b9dd-8f45c8e547e7", "No match has been found for this code. Please remove it");

		public readonly List<ColumnInfo> Columns = new List<ColumnInfo>();
		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			if (Env.CurrentUser.IsWebUser && !ObjectFactory.Get<IWebReportFilterHelper>().IsSupportedOnWeb(this))
			{
				return;
			}

			var filterData = new MultipleSelectionLookupFilter();
			SetBaseFilterData(filterData);

			filterData.LookupType = LookupType;
			if (BindToList != null)
			{
				filterData.SelectedValue.AddRange(BindToList.OfType<BusinessObject>().Select(bizObj => bizObj.PK.ToGuid()));
			}

			reportFilterData.MultipleSelectionLookupFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.MultipleSelectionLookupFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				BindToList.Clear();
				var elementType = BindToList.TypeOfElements;
				selectedValue.SelectedValue.ForEach(g =>
				{
					var bizObj = Factory.Load(elementType, g);
					if (bizObj != null)
					{
						BindToList.Add(bizObj);
					}
				});
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var filterData = new MultipleSelectionLookupJsonData();
			SetJsonData(filterData);
			filterData.ModuleID = ModuleID?.ToString();
			filterData.IsFilterValueExcluded = IsFilterValueExcluded;
			filterData.ValueAsString = ValueAsStringForSerialisation;
			filterData.SerialisedByPK = SerialisedByPK;
			filterData.Columns = Columns;
			filterData.UseCodesForWhereClause = UseCodesForWhereClause;

			if (CollectionProvider != null)
			{
				filterData.LookupType = CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(CollectionProvider.GetType());
			}

			return filterData;
		}

		#endregion
	}
}
