#if DEBUG
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace CargoWise.EntityFramework.Testing
{
	#region SuperDummy

	public class SuperDummyBusinessObject : BusinessObject, IObsoleteValidation
	{
		#region Property Constants

		public abstract class Schema
		{
			public const string TableName = "DUMMYBIZO";
			public const string SS_Name = "Z0_Description";
		}

		#endregion

		public SuperDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DummyBusinessObject.SetDataRowDefaultValues(Row);
			Row[Schema.SS_Name] = "";
		}

		#endregion

		#region PK

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return DummyBizoSchema.PK; }
		}

		protected ZGuid SS_PK
		{
			get { return new ZGuid(Row[PKSchemaColumn.Name]); }
		}

		#endregion

		#region Dummy Collection

		DummyBusinessObjectCollection fCollection;
		public DummyBusinessObjectCollection Collection
		{
			get
			{
				if (fCollection == null)
				{
					fCollection = new DummyBusinessObjectCollection(Factory);
				}
				return fCollection;
			}
		}

		#endregion

		#region SS_Name

		[List("SS_NameList")]
		public virtual ZString SS_Name
		{
			get { return new ZString(Row[Schema.SS_Name]); }
			set
			{
				if (value != SS_Name)
				{
					HasChanges = true;

					Row[Schema.SS_Name] = ((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
					SS_NameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SS_NameInfo
		{
			get { return GetZPropertyInfo(nameof(SS_Name)); }
		}

		public IList SS_NameList { get; set; }

		#endregion
	}

	#endregion

	#region DummyBusinessObjectCollection

	public class DummyBusinessObjectCollection : BusinessObjectCollection<DummyBusinessObject>
	{
		public DummyBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyBusinessObjectCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		IBusinessObjectCollectionFetchStrategy lastStrategy;
		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			if (lastStrategy == null)
			{
				lastStrategy = new BusinessObjectCollectionFetchStrategyForTest(this);
			}
			return lastStrategy;
		}

		protected internal override void SetDefaultsForNewChild(BusinessObject child)
		{
			((DummyBusinessObject)child).Z0_Description = "NowSet";
		}

		public ZQuery LastLoadedAdditionalFilter_Exposed
		{
			get { return LastLoadedAdditionalFilter; }
		}

		public IFindBoxListProvider FindBoxListProviderOverride { get; set; }

		protected override IFindBoxListProvider FindBoxListProvider => FindBoxListProviderOverride ?? base.FindBoxListProvider;
	}

	#endregion

	#region DummyBusinessObjectCollectionView

	public class DummyBusinessObjectCollectionView : BusinessObjectCollectionView<DummyBusinessObject>
	{
		public DummyBusinessObjectCollectionView(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		IBusinessObjectCollectionFetchStrategy lastStrategy;
		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			if (lastStrategy == null)
			{
				lastStrategy = new BusinessObjectCollectionFetchStrategyForTest(this);
			}
			return lastStrategy;
		}

		protected internal override void SetDefaultsForNewChild(BusinessObject child)
		{
			((DummyBusinessObject)child).Z0_Description = "NowSet";
		}

		public ZQuery LastLoadedAdditionalFilter_Exposed
		{
			get { return LastLoadedAdditionalFilter; }
		}

		public IFindBoxListProvider FindBoxListProviderOverride { get; set; }

		protected override IFindBoxListProvider FindBoxListProvider => FindBoxListProviderOverride ?? base.FindBoxListProvider;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return true;
		}
	}

	#endregion

	#region DummyChildBusinessObjectCollection

	public class DummyChildBusinessObjectCollection : BusinessObjectCollection<DummyChildBusinessObject>, IBusinessObjectCollectionInternals
	{
		public DummyChildBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyChildBusinessObjectCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Custom Sort

		class Big2Comparer : PropertyComparer
		{
			public Big2Comparer(PropertyDescriptor property, ListSortDirection direction)
				: base(property, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				int result = 0;
				if (Direction == ListSortDirection.Ascending)
				{
					if (PropertyDescriptor.GetValue(y) is ZInt && PropertyDescriptor.GetValue(y).Equals(2))
					{
						result = 1; // put the 2 at the top!
					}
					else
					{
						result = ((IComparable)PropertyDescriptor.GetValue(x)).CompareTo(PropertyDescriptor.GetValue(y));
					}
				}
				else
				{
					throw new NotSupportedException();
				}

				return result;
			}
		}

		protected internal override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return UseBig2Sort ? new Big2Comparer(property, direction) : base.GetComparerForSort(property, direction);
		}

		#endregion

		public bool UseBig2Sort;
		public bool OnAddedCalled;
		public bool OnRemovingCalled;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			OnAddedCalled = true;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			OnRemovingCalled = true;
		}

		public bool ExposedIsLoading
		{
			get { return IsLoading; }
		}

		public override bool ReadOnly
		{
			get { return readOnlyExposed ?? base.ReadOnly; }
		}

		public void SetReadOnly(bool value)
		{
			readOnlyExposed = value;
			FireListResetEvent();
		}
		bool? readOnlyExposed;

		protected override bool AllowNewCore
		{
			get { return allowNewExposed ?? base.AllowNewCore; }
		}

		public void SetAllowNew(bool value)
		{
			allowNewExposed = value;
			FireListResetEvent();
		}
		bool? allowNewExposed;

		protected override bool AllowRemoveCore
		{
			get { return allowRemoveExposed ?? base.AllowRemoveCore; }
		}

		public void SetAllowRemove(bool value)
		{
			allowRemoveExposed = value;
			FireListResetEvent();
		}
		bool? allowRemoveExposed;

		protected override bool AllowSort
		{
			get { return allowSortExposed ?? base.AllowSort; }
		}

		public void SetAllowSort(bool value)
		{
			allowSortExposed = value;
			FireListResetEvent();
		}
		bool? allowSortExposed;

		internal DummyBusinessObject Dummy;

		#region IBusinessObjectCollectionInternals Members

		public bool MastersAreInDatabase
		{
			get { return Dummy.IsInDatabase; }
		}

		#endregion
	}

	#endregion

	#region DummyChildBusinessObject

	public class DummyChildBusinessObject : DummyBaseBusinessObject
	{
		public static DummyChildBusinessObject New(BusinessObjectFactory factory)
		{
			return (DummyChildBusinessObject)factory.New(typeof(DummyChildBusinessObject));
		}

		public DummyChildBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Z0_ChildOnly

		public virtual ZInt Z0_ChildOnly
		{
			get
			{
				//simulate this being a property that can do Factory.Load on get
				this.Factory.ThreadSentry.EnsureCurrentThreadIsOwner();
				return fZ0_ChildOnly;
			}
			set
			{
				if (value != Z0_ChildOnly)
				{
					HasChanges = true;

					fZ0_ChildOnly = value;
					Z0_ChildOnlyInfo.RefreshBinding();
				}
			}
		}

		ZInt fZ0_ChildOnly;

		public ZPropertyInfo Z0_ChildOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(Z0_ChildOnly)); }
		}

		#endregion

		bool fZ0_NVarCharMax_ReadOnly;
		public bool Z0_NVarCharMax_ReadOnly
		{
			get => fZ0_NVarCharMax_ReadOnly;
			set
			{
				fZ0_NVarCharMax_ReadOnly = value;
				Z0_NVarCharMaxInfo.RefreshBinding();
			}
		}

		bool fZ0_VarCharMax_ReadOnly;
		public bool Z0_VarCharMax_ReadOnly
		{
			get => fZ0_VarCharMax_ReadOnly;
			set
			{
				fZ0_VarCharMax_ReadOnly = value;
				Z0_VarCharMaxInfo.RefreshBinding();
			}
		}

		public override string QuickViewCard
		{
			get { return QuickViewCard_Override; }
		}
		public string QuickViewCard_Override { get; set; }

		protected override bool SupportsCloneCore()
		{
			return true;
		}
	}

	#endregion

	#region DummyBusinessObject

	public class DummyBusinessObject : DummyBaseBusinessObject
	{
		public DummyBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static DummyBusinessObject New(BusinessObjectFactory factory)
		{
			return (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
		}

		public event EventHandler OnBeforeSuccessfulDelete;

		public Action OnSavingHook { get; set; }

		public static Guid SetDataRowDefaultValues(DataRow row)
		{
			object realPK = row[Schema.PK];
			row[Schema.PK] = DBNull.Value; // so that dummy bizo constructor thinks it should set defaults
			new DummyBusinessObject(null, row); // wraps row and sets defaults
			row[Schema.PK] = realPK == DBNull.Value ? row[Schema.PK] : realPK;
			return (Guid)row[Schema.PK];
		}

		public void CopyPersistentValuesFromPublic(BusinessObject sourceObject)
		{
			CopyPersistentValuesFrom(sourceObject);
		}

		public void CopyValuesFromPublic(BusinessObject sourceObject)
		{
			CopyValuesFrom(sourceObject);
		}

		public override void Delete()
		{
			base.Delete();
			Collection.RemoveAll();
		}

		public override void OnSaving()
		{
			OnSavingHook?.Invoke();
			base.OnSaving();
			OnSavingCount++;
		}

		protected override void OnSavingForDelete()
		{
			base.OnSavingForDelete();
			OnSavingForDeleteCount++;
		}

		protected override void OnSavedForDeletedObject(bool saveSucceeded)
		{
			base.OnSavedForDeletedObject(saveSucceeded);
			OnSavedForDeletedObjectCount++;
		}

		public int ValidationSecondsForZ0_Code;
		public int ValidationTimesForZ0_Code;
		public int OnSavingCount;
		public int RunPreSaveValidationCount;

		public int OnSavingForDeleteCount;
		public int OnSavedForDeletedObjectCount;

		public ZGuid Z0_CalculatedGuid
		{
			get { return Z0_Guid; }
		}

		#region Z0_Calculated

		public ZInt Z0_Calculated
		{
			get { return 5; }
		}

		public ZPropertyInfo Z0_CalculatedInfo
		{
			get { return GetZPropertyInfo(nameof(Z0_Calculated)); }
		}

		#endregion

		bool fZ0_Description_ReadOnly;
		public bool Z0_Description_ReadOnly
		{
			get => fZ0_Description_ReadOnly;
			set
			{
				fZ0_Description_ReadOnly = value;
				Z0_DescriptionInfo.RefreshBinding();
			}
		}

		bool fZ0_Guid_ReadOnly;
		public bool Z0_Guid_ReadOnly
		{
			get => fZ0_Guid_ReadOnly;
			set
			{
				fZ0_Guid_ReadOnly = value;
				Z0_GuidInfo.RefreshBinding();
			}
		}

		bool fZ0_AnotherDecimal_ReadOnly;
		public bool Z0_AnotherDecimal_ReadOnly
		{
			get => fZ0_AnotherDecimal_ReadOnly;
			set
			{
				fZ0_AnotherDecimal_ReadOnly = value;
				Z0_AnotherDecimalInfo.RefreshBinding();
			}
		}

		bool fZ0_FK_Code_ReadOnly;
		public bool Z0_FK_Code_ReadOnly
		{
			get => fZ0_FK_Code_ReadOnly;
			set
			{
				fZ0_FK_Code_ReadOnly = value;
				Z0_FK_CodeInfo.RefreshBinding();
			}
		}

		bool fZ0_Code_ReadOnly;
		public bool Z0_Code_ReadOnly
		{
			get => fZ0_Code_ReadOnly;
			set
			{
				fZ0_Code_ReadOnly = value;
				Z0_CodeInfo.RefreshBinding();
			}
		}

		bool fZ0_Decimal_ReadOnly;
		public bool Z0_Decimal_ReadOnly
		{
			get => fZ0_Decimal_ReadOnly;
			set
			{
				fZ0_Decimal_ReadOnly = value;
				Z0_DecimalInfo.RefreshBinding();
			}
		}

		public ZBlob Z0_VarBinaryMax_HTML
		{
			get
			{
				var varBinaryMax = base.Z0_VarBinaryMax.ToUTF8();
				if (varBinaryMax.StartsWith("{\\rtf", StringComparison.Ordinal))
				{
					var rtfToHtml = new RtfToHtmlConverter();
					return ZBlob.FromUTF8(rtfToHtml.Convert(varBinaryMax));
				}
				else
				{
					var rtfToHtml = new PlainTextToHtmlConverter();
					return ZBlob.FromUTF8(rtfToHtml.Convert(varBinaryMax));
				}
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.Z0_VarBinaryMax = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public DummyChildBusinessObjectCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = NewCollection();
					collection.Dummy = this;
				}
				return collection;
			}
			set { collection = value; }
		}

		protected virtual DummyChildBusinessObjectCollection NewCollection()
		{
			return new DummyChildBusinessObjectCollection(Factory);
		}

		public DummyChildBusinessObjectCollection FilteredCollection
		{
			get
			{
				if (filteredCollection == null)
				{
					filteredCollection = new DummyChildBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Number, 5));
					filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Schema.Z0_Number, "Property1", new ZDecimal(5)));
				}
				return filteredCollection;
			}
			set { filteredCollection = value; }
		}

		public IBusinessObjectCollection CollectionWithPublicSetter { get; set; }

		internal void SetDeleteCheckers(params DeleteChecker[] deleteCheckers)
		{
			this.strategies = deleteCheckers;
		}

		#region Implementation

		protected override DummyBizoValidation GetNewValidation()
		{
			return new DummyBusinessObjectValidation(this);
		}

		protected class DummyBusinessObjectValidation : DummyBizoValidation
		{
			public DummyBusinessObjectValidation(DummyBusinessObject bizO)
				: base(bizO)
			{
			}

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();
				((DummyBusinessObject)Parent).ValidationTimesForZ0_Code++;
				int sleepSeconds = ((DummyBusinessObject)Parent).ValidationSecondsForZ0_Code * 1000;
				if (sleepSeconds > 0)
				{
					System.Threading.Thread.Sleep(sleepSeconds);
				}
			}

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();
				if (Parent.Z0_Description == "error")
				{
					Parent.Z0_DescriptionInfo.AddError("Error");
				}
				if (Parent.Z0_Description == "warning")
				{
					Parent.Z0_DescriptionInfo.AddWarning("Warning");
				}
				if (Parent.Z0_Description == "message error")
				{
					Parent.Z0_DescriptionInfo.AddMessageError("Message Error");
				}
			}
		}

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			return strategies ?? base.GetStrategies();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			RunPreSaveValidationCount++;
			if (SleepDuringValidate.HasValue)
			{
				System.Threading.Thread.Sleep(SleepDuringValidate.Value);
			}
		}
		public int? SleepDuringValidate;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			((IBusinessObjectInternals)this).Row[Schema.Z0_Code] = "NCODE";
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();
			if (OnBeforeSuccessfulDelete != null)
			{
				OnBeforeSuccessfulDelete(this, EventArgs.Empty);
			}
		}

		IBusinessObjectStrategy[] strategies;
		DummyChildBusinessObjectCollection collection;
		DummyChildBusinessObjectCollection filteredCollection;

		#endregion
	}

	#endregion

	#region Dummies With TypeDeciders

	public class DummyConcreteTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (row["Z0_Description"].ToString().Trim() == "BASE")
			{
				return typeof(DummyBaseBusinessObject);
			}
			else
			{
				return typeof(DummyBusinessObject);
			}
		}

		public override Type GetTypeForNew()
		{
			if (MakeBase) // this would usually be a check to Env or something like that.
			{
				return typeof(DummyBaseBusinessObject);
			}
			else
			{
				return typeof(DummyBusinessObject);
			}
		}

		public override Type GetTypeForBinding()
		{
			return typeof(DummyBaseBusinessObject);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "For unit testing only, remove for static resetter rule")]
		public bool MakeBase;
	}

	public class DummyAbstractTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch (row["Z0_Description"].ToString().Trim())
			{
				case "ABSTRACT":
					return typeof(DummyAbstractBusinessObjectWithTypeDecider);
				default:
					return typeof(DummyBusinessObject);
			}
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}
	}

	public class DummyConcreteBusinessObjectWithTypeDecider : DummyBusinessObject
	{
		public readonly static new DummyConcreteTypeDecider TypeDecider = new DummyConcreteTypeDecider();

		public DummyConcreteBusinessObjectWithTypeDecider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	public abstract class DummyAbstractBusinessObjectWithTypeDecider : DummyBusinessObject
	{
		public readonly static new DummyAbstractTypeDecider TypeDecider = new DummyAbstractTypeDecider();

		public DummyAbstractBusinessObjectWithTypeDecider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	#endregion

	#region Dummy with ActiveFilter

	public class DummyBusinessObjectWithActiveFilter : DummyWithDependentsBusinessObject
	{
		public DummyBusinessObjectWithActiveFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(DummyBizoSchema.Z0_Bool, ZBool.True); }
		}

		public bool IsActive
		{
			get { return Z0_Bool; }
			set { Z0_Bool = value; }
		}
	}

	public class DummyBusinessObjectWithActiveFilterCollection : BusinessObjectCollection<DummyBusinessObjectWithActiveFilter>
	{
		public DummyBusinessObjectWithActiveFilterCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyBusinessObjectWithActiveFilterCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
	#endregion

	#region Dummy Read Only Object

	public class DummyReadOnlyBusinessObject : DummyBusinessObject
	{
		public DummyReadOnlyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		DummyChildReadOnlyBusinessObjectCollection collection;
		DummyChildReadOnlyBusinessObjectCollection filteredCollection;

		public new DummyChildReadOnlyBusinessObjectCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = NewCollection();
					collection.Dummy = this;
				}
				return collection;
			}
			set { collection = value; }
		}

		new DummyChildReadOnlyBusinessObjectCollection NewCollection()
		{
			return new DummyChildReadOnlyBusinessObjectCollection(Factory);
		}

		public new DummyChildReadOnlyBusinessObjectCollection FilteredCollection
		{
			get
			{
				if (filteredCollection == null)
				{
					filteredCollection = new DummyChildReadOnlyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Number, 5));
					filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Schema.Z0_Number, "Property1", new ZDecimal(5)));
				}
				return filteredCollection;
			}
			set { filteredCollection = value; }
		}
	}

	#endregion

	#region Dummy Read Only Fields

	public class DummyChildReadOnlyBusinessObject : DummyBaseBusinessObject
	{
		public static DummyChildReadOnlyBusinessObject New(BusinessObjectFactory factory)
		{
			return (DummyChildReadOnlyBusinessObject)factory.New(typeof(DummyChildReadOnlyBusinessObject));
		}

		public DummyChildReadOnlyBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Z0_ChildOnly

		public virtual ZInt Z0_ChildOnly
		{
			get { return fZ0_ChildOnly; }
			set
			{
				if (value != Z0_ChildOnly)
				{
					HasChanges = true;

					fZ0_ChildOnly = value;
					Z0_ChildOnlyInfo.RefreshBinding();
				}
			}
		}

		ZInt fZ0_ChildOnly;

		public ZPropertyInfo Z0_ChildOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(Z0_ChildOnly)); }
		}

		#endregion

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region ReadOnly Attributes
		[ReadOnly(true)]
		public override ZInt Z0_AnotherNumber { get { return base.Z0_AnotherNumber; } }

		[ReadOnlyMember(nameof(DecimalReadOnly))]
		public override ZDecimal Z0_Decimal { get { return base.Z0_Decimal; } }

		public bool DecimalReadOnly { get { return false; } }

		public bool Z0_Date_ReadOnly { get { return false; } }

		public bool Z0_BoolReadOnly { get { return false; } }

		public bool Z0_Short_ReadOnly => true;

		#endregion
	}
	#endregion

	#region Dummy Read Only Collection

	public class DummyChildReadOnlyBusinessObjectCollection : BusinessObjectCollection<DummyChildReadOnlyBusinessObject>, IBusinessObjectCollectionInternals
	{
		public DummyChildReadOnlyBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyChildReadOnlyBusinessObjectCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Custom Sort

		class Big2Comparer : PropertyComparer
		{
			public Big2Comparer(PropertyDescriptor property, ListSortDirection direction)
				: base(property, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				int result = 0;
				if (Direction == ListSortDirection.Ascending)
				{
					if (PropertyDescriptor.GetValue(y) is ZInt && PropertyDescriptor.GetValue(y).Equals(2))
					{
						result = 1; // put the 2 at the top!
					}
					else
					{
						result = ((IComparable)PropertyDescriptor.GetValue(x)).CompareTo(PropertyDescriptor.GetValue(y));
					}
				}
				else
				{
					throw new NotSupportedException();
				}

				return result;
			}
		}

		protected internal override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return UseBig2Sort ? new Big2Comparer(property, direction) : base.GetComparerForSort(property, direction);
		}

		#endregion

		public bool UseBig2Sort;
		public bool OnAddedCalled;
		public bool OnRemovingCalled;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			OnAddedCalled = true;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			OnRemovingCalled = true;
		}

		public bool ExposedIsLoading
		{
			get { return IsLoading; }
		}

		public override bool ReadOnly
		{
			get { return readOnlyExposed ?? base.ReadOnly; }
		}

		public void SetReadOnly(bool value)
		{
			readOnlyExposed = value;
			FireListResetEvent();
		}
		bool? readOnlyExposed;

		protected override bool AllowNewCore
		{
			get { return allowNewExposed ?? base.AllowNewCore; }
		}

		public void SetAllowNew(bool value)
		{
			allowNewExposed = value;
			FireListResetEvent();
		}
		bool? allowNewExposed;

		protected override bool AllowRemoveCore
		{
			get { return allowRemoveExposed ?? base.AllowRemoveCore; }
		}

		public void SetAllowRemove(bool value)
		{
			allowRemoveExposed = value;
			FireListResetEvent();
		}
		bool? allowRemoveExposed;

		protected override bool AllowSort
		{
			get { return allowSortExposed ?? base.AllowSort; }
		}

		public void SetAllowSort(bool value)
		{
			allowSortExposed = value;
			FireListResetEvent();
		}
		bool? allowSortExposed;

		internal DummyBusinessObject Dummy;

		#region IBusinessObjectCollectionInternals Members

		public bool MastersAreInDatabase
		{
			get { return Dummy.IsInDatabase; }
		}

		#endregion
	}

	#endregion
}
#endif
