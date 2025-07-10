using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CodeLookupField : LookupFilterFieldBase, IJsonSerializable
#if DEBUG
, IValueAsStringProviderForUnitTests
#endif
	{
		public CodeLookupField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal CodeLookupField(CodeLookupFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			Value = data.Value;

			if (!string.IsNullOrEmpty(data.CollectionProviderName))
			{
				var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, data.CollectionProviderName);
				SetCollectionProvider(provider);
			}
		}

		#endregion

		void CreateParameters()
		{
			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.VarChar);
			fParam.Value = DBNull.Value;
			ParameterList.Add(fParam);
		}

		public ZString ZValue
		{
			get { return fParam.Value == DBNull.Value ? ZString.Empty : new ZString(fParam.Value); }
			set
			{
				if (value != ZValue)
				{
					CheckMaximumLength(ZValueInfo, value);
					fParam.Value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					ValidateZValue();
					ZValueInfo.RefreshBinding();
					foreach (MasterDetailRelation relation in DetailRelations)
					{
						relation.RefreshBinding();
					}
				}
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			if (otherFilterField is CodeLookupField)
			{
				if (((LookupFilterFieldBase)(otherFilterField)).CollectionProvider == null) // for those existing CodelookupField, the serialized value does not support CollectionProvider, i.e. it's null						
				{
					return true;
				}
				else
				{
					return CheckCollectionProviderHasSameTypeAsAnotherField(otherFilterField);
				}
			}
			return false;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateZValue();
		}

		public void ValidateZValue()
		{
			ZValueInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ZValueInfo.AddError(ValidationError);
			}

			if (Globals.IsWeb)
			{
				if (!ZValue.IsEmpty)
				{
					var listProvider = BindToFindBoxList as IFindBoxListProvider;
					if (listProvider != null)
					{
						if (listProvider.GetBusinessObjectFromCode(ZValue) == null)
						{
							ZValueInfo.AddError(Res.GetString("9264eea4-5a20-4736-a8eb-b1eaed83180d", "Invalid Selection"));
						}
					}
				}
			}
		}

		public ZPropertyInfo ZValueInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ZValue), DisplayName);
			}
		}

		public int ZValue_MaxLength
		{
			get { return CollectionProvider == null ? 6 : (CollectionProvider as CollectionProviderWithCodeSupport).MaxLength; }
		}

		public string Value
		{
			get { return (fParam.Value == DBNull.Value) ? "" : (string)fParam.Value; }
			set
			{
				if (value != Value)
				{
					fParam.Value = value;
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
				}
			}
		}

		public override object ValueAsObject
		{
			get { return IsEmpty ? DBNull.Value : Value; }
		}

		public override bool IsEmpty
		{
			get
			{
				if (Globals.IsWeb)
				{
					return (fParam.Value == DBNull.Value || ZValue.IsEmpty);
				}
				return fParam.Value == DBNull.Value;
			}
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.CodeLookupFieldUserControl; }
		}

		#region Debug Only
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			IFindBoxListProvider listProvider = CollectionProvider.CollectionForFindbox as IFindBoxListProvider;
			if (listProvider != null)
			{
				Value = listProvider.NearestMatch("", true, -1).Item1;
			}

			if (string.IsNullOrEmpty(Value))
			{
				throw new DocumentEngineException("No items found in BindToList on lookup with ModuleID : " + ModuleID.ToString());
			}
		}

		public override void ClearValueForUnitTest()
		{
			fParam.Value = DBNull.Value;
		}
#endif
		#endregion

		#region Implementation

		protected SqlParameter fParam;

		protected override string NonEmptyWhereClause()
		{
			return Invariant($"{fParam} IN ({FieldName})");
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is CodeLookupField)
			{
				this.Value = ((CodeLookupField)source).Value;
			}
		}

		public override void ClearValues()
		{
			this.Value = String.Empty;
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return this.Value; }
			set { this.Value = value; }
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			if (Env.CurrentUser.IsWebUser && !ObjectFactory.Get<IWebReportFilterHelper>().IsSupportedOnWeb(this))
			{
				return;
			}

			var filterData = new CodeLookupFilter();
			SetBaseFilterData(filterData);

			filterData.Value = ZValue;
			filterData.LookupType = LookupType;

			reportFilterData.CodeLookupFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.CodeLookupFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				ZValue = selectedValue.Value;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var filterData = new CodeLookupFieldJsonData();
			SetJsonData(filterData);
			filterData.Value = Value;

			if (CollectionProvider != null)
			{
				filterData.CollectionProviderName = CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(CollectionProvider.GetType());
			}

			return filterData;
		}

		#endregion
	}
}
