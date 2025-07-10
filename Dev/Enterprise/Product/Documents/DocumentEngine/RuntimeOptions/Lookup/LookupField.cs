using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class LookupField : LookupFilterFieldBase, IGuidChangeNotifier, IJsonSerializable
#if DEBUG
, IValueAsStringProviderForUnitTests
#endif
	{
		public LookupField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal LookupField(LookupFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			if (!string.IsNullOrEmpty(data.CollectionProviderName))
			{
				fCollectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, data.CollectionProviderName);
			}
			Value = data.Value;
		}

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is LookupField originalLookupField && originalLookupField.CollectionProvider.ModuleID == ModuleIDs.Organisation)
			{
				LinkToScheduledReportRecipientForOrganisation = originalLookupField.LinkToScheduledReportRecipientForOrganisation;
			}
		}

		internal override void PreSetValueForDeserializingBeforeExchange(FilterField deserializedFilter)
		{
			base.PreSetValueForDeserializingBeforeExchange(deserializedFilter);
			if (deserializedFilter is LookupField deserializedLookup)
			{
				ZValue = deserializedLookup.ZValue;
			}
		}

		#endregion

		void CreateParameters()
		{
			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.UniqueIdentifier);
			fParam.Value = DBNull.Value;
			ParameterList.Add(fParam);
		}

		public ZGuid ZValue
		{
			get { return zValue; }
			set
			{
				if (value != ZValue)
				{
					ZValueInfo.ClearAllNotifications();
					IsFiltered = value == ZGuid.Missing;

					fParam.Value = (value.IsValid || !IsValid ? ((IZTypeInternals)value).GetValueForLogicalDataLayer(true) : DBNull.Value);
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();

					zValue = value;
					ZValueInfo.RefreshBinding();

					FireLookupGuidChanged();
					FireChangeReadOnly();
					FireChangeDependencyValue();
				}
			}
		}
		ZGuid zValue;

		public void ValidateZValue()
		{
			ZValueInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ZValueInfo.AddError(ValidationError);
			}

			if (!((ZValueInfo.IsNullable && ZValue.IsEmpty) || ZValue.IsValid))
			{
				string error = Res.GetString("496879dc-377c-45a3-a219-955b520fc674", "Enter a valid selection.");
				ZValueInfo.AddError(error);
			}

			if (LinkToScheduledReportRecipientForOrganisation)
			{
				ZValueInfo.AddWarning(FilterBuilderPropertyCodeDescriptionList.Descriptions.LinkToScheduledReportRecipientForOrganisation);
			}
		}

		public ZPropertyInfo ZValueInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(ZValue));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		public Guid Value
		{
			get { return fParam.Value == DBNull.Value ? Guid.Empty : (Guid)fParam.Value; }
			set
			{
				zValue = new ZGuid(value);
				if (value != Value)
				{
					fParam.Value = (value == Guid.Empty ? DBNull.Value : value);
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					this.RefreshBinding();

					FireLookupGuidChanged();
					FireChangeReadOnly();
				}
			}
		}

		bool linkToScheduledReportRecipientForOrganisation;
		internal bool LinkToScheduledReportRecipientForOrganisation
		{
			get => linkToScheduledReportRecipientForOrganisation;
			set
			{
				if (linkToScheduledReportRecipientForOrganisation != value)
				{
					linkToScheduledReportRecipientForOrganisation = value;
					ValidateZValue();
				}
			}
		}

		#region Overrides

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			if (otherFilterField is LookupField)
			{
				return CheckCollectionProviderHasSameTypeAsAnotherField(otherFilterField);
			}
			return false;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var dependenceCollectionProvider = CollectionProvider as DependenceCollectionProvider;
			if (dependenceCollectionProvider != null && dependenceCollectionProvider.List.Count > 0)
			{
				dependenceCollectionProvider.SetDependencyValue(dependenceCollectionProvider.List.Keys.First());
			}

			IBusinessObjectCollection collection = CollectionProvider.Collection;

			bool valueWasSet = false;
			if (collection is BusinessObjectCollection)
			{
				ZQuery filterMaximumRows = new ZQuery();
				filterMaximumRows.MaximumRows = 1;
				((BusinessObjectCollection)collection).Load(filterMaximumRows);
				if (collection.Count > 0)
				{
					BusinessObject[] businessObjects = collection.ToArray();
					Value = businessObjects[0].PK.ToGuid();
					valueWasSet = true;
				}
			}
			else
			{
				BusinessObject @object = Factory.LoadTop1(BusinessObjectCollection.GetElementTypeFromCollectionType(collection.GetType()), new ZQuery());
				if (@object != null)
				{
					((IActiveBusinessObjectCollection)collection).AdditionalFilter = new ZQuery(@object.PKSchemaColumn, @object.PK);
					Value = @object.PK.ToGuid();
					valueWasSet = true;
				}
			}

			if (!valueWasSet)
			{
				if (collection.AllowNew)
				{
					BusinessObject newBizO = Factory.New(collection.TypeOfElements);
					newBizO.FillWithValidTestData(kind, propertyPath);
					Value = newBizO.PK.ToGuid();
				}
			}
			//				throw new Exception("No items found in BindToList on lookup with ModuleID : " + ModuleID.ToString());
		}

		public override void ClearValueForUnitTest()
		{
			fParam.Value = DBNull.Value;
		}

#endif

		public override object ValueAsObject
		{
			get
			{
				return IsEmpty ? DBNull.Value : Value;
			}
		}

		public override bool IsEmpty
		{
			get { return fParam.Value == DBNull.Value; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.LookupFieldUserControl; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateZValue();
		}

		#endregion

		#region ValueProviders stuff
		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".Code", new ValueReplacers.ReplacementProviderMethod(GetCodeReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".Description", new ValueReplacers.ReplacementProviderMethod(GetDescriptionReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.Code>", ResString.GetMultilingualString("12aedab0-85de-45f3-bec2-6893ef06c980", "Returns the code of the selected value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.Description>", ResString.GetMultilingualString("822509a7-56b8-43bb-89f0-b343bc7c64a7", "Returns the description of the selected value.")));
		}

		protected object GetCodeReplacement(string macro, Report report)
		{
			return ((IFindBoxListProvider)CollectionProvider.Collection).CodeFromPrimaryKey(Value);
		}

		protected object GetDescriptionReplacement(string macro, Report report)
		{
			return ((IFindBoxListProvider)CollectionProvider.Collection).DescriptionFromPrimaryKey(Value);
		}

		#endregion

		#region Implementation

		protected SqlParameter fParam;

		protected override string NonEmptyWhereClause()
		{
			return FieldName + " = " + fParam;
		}

		void FireLookupGuidChanged()
		{
			foreach (var relation in DetailRelations)
			{
				relation.RefreshBinding();
			}

			ZGuid value;
			if ((fLookupGuidChanged != null) && (CollectionProvider != null) && (value = ZValue).IsValid)
			{
				ICompositeCollection compositeCollection = CollectionProvider.Collection as ICompositeCollection;
				Type typeOfElements = (compositeCollection != null) ? compositeCollection.TypeOfElementFromPK(value) : CollectionProvider.Collection.TypeOfElements;
				var @object = CollectionProvider.Collection.Factory.Load(typeOfElements, value);
				fLookupGuidChanged(@object);
			}
		}

		#endregion

		#region LookupGuidChangeNotifier Members

		event LookupGuidChanged IGuidChangeNotifier.LookupGuidChanged
		{
			add { fLookupGuidChanged += value; }
			remove { fLookupGuidChanged -= value; }
		}
		event LookupGuidChanged fLookupGuidChanged;

		#endregion

		#region ILoadCodeDescriptions Members

		internal ICodeDescription GetCodeDescriptionForGUID(ZGuid guid)
		{
			var collection = fCollectionProvider.CollectionForFindbox;
			var businessObject = collection.Factory.Load(collection.GetTypeOfElementsFromPK(guid), guid);
			return businessObject == null ? null : businessObject as ICodeDescription;
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is LookupField)
			{
				this.Value = ((LookupField)source).Value;
			}
		}

		public override void ClearValues()
		{
			this.Value = Guid.Empty;
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return Value == Guid.Empty ? "" : Value.ToString(); }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					try
					{
						Value = new Guid(value);
					}
					catch (FormatException)
					{
						// Ignore it - the serialised value was not a Guid
					}
				}
			}
		}

		public ZBool IsFiltered { get; set; }
		public bool IsLinkedToContactUser { get; set; }
		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			if (Env.CurrentUser.IsWebUser && (IsLinkedToContactUser || !ObjectFactory.Get<IWebReportFilterHelper>().IsSupportedOnWeb(this)))
			{
				return;
			}

			var filterData = new LookupFilter();
			SetBaseFilterData(filterData);

			if (ZValue.IsValid && !ZValue.IsEmpty)
			{
				filterData.Value = ZValue.ToGuid();
			}
			filterData.LookupType = LookupType;

			reportFilterData.LookupFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.LookupFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				ZValue = selectedValue.Value;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var filterData = new LookupFieldJsonData();
			SetJsonData(filterData);
			filterData.Value = Value;
			filterData.LookupType = LookupType;

			if (CollectionProvider != null)
			{
				filterData.CollectionProviderName = CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(CollectionProvider.GetType());
			}

			return filterData;
		}

		#endregion
	}
}
