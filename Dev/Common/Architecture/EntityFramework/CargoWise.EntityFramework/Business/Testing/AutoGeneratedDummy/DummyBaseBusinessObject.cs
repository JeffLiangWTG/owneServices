#if DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	public interface IDummy
	{
	}

	[AllowAllObjectsToBeLoaded]
	[CodeProperty(DummyBizoSchema.Constants.Z0_Code)]
	public class DummyBaseBusinessObject : AutoDummyBizo, IDummy
	{
		public new abstract class Schema : AutoDummyBizo.Schema
		{
			public const string TablePrefix = DummyBizoSchema.Constants.Prefix;
		}

		public DummyBaseBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public readonly static DummyBaseTypeDecider TypeDecider = new DummyBaseTypeDecider();

		public class DummyBaseTypeDecider : TypeDecider
		{
			public Type TypeForLoadOverride
			{
				get { return typeForLoadOverride; }
				set
				{
					typeForLoadOverride = value;
					TypeForLoadOverrides.Clear();
				}
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "For unit testing only, remove for static resetter rule")]
			Type typeForLoadOverride;

			public override Type GetTypeForNew()
			{
				return typeof(DummyBaseBusinessObject);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				if (TypeForLoadOverride != null)
				{
					return TypeForLoadOverride;
				}

				foreach (var typeForLoadOverride in TypeForLoadOverrides)
				{
					if (typeForLoadOverride.Item1(row, factory))
					{
						return typeForLoadOverride.Item2;
					}
				}

				return typeof(DummyBaseBusinessObject);
			}

			public override Type GetTypeForBinding()
			{
				return GetTypeForNew();
			}

			public void AddTypeForLoadOverride(Func<DataRow, BusinessObjectFactory, bool> predicate, Type typeForLoad)
			{
				TypeForLoadOverrides.Add(Tuple.Create(predicate, typeForLoad));
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "For unit testing only, remove for static resetter rule")]
			readonly List<Tuple<Func<DataRow, BusinessObjectFactory, bool>, Type>> TypeForLoadOverrides = new List<Tuple<Func<DataRow, BusinessObjectFactory, bool>, Type>>();
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Row[Schema.Z0_FK_Code] = "";
			Row[Schema.Z0_Code] = "";
			Row[Schema.Z0_Number] = 0;
			Row[Schema.Z0_AnotherNumber] = 0;
			Row[Schema.Z0_Decimal] = 0;
			Row[Schema.Z0_AnotherDecimal] = 0;
			Z0_Description = "Default";
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			OnLoadedCalled = true;
		}

		public override string TablePrefix
		{
			get { return Schema.TablePrefix; }
		}

		public override bool IsSavedByFactory
		{
			get { return (OverrideIsSavedByFactory) ? IsSavedByFactoryOverride : base.IsSavedByFactory; }
		}

		public override bool HasChangesInAuditDetails
		{
			get { return (OverrideHasChangesInAuditDetails) ? HasChangesInAuditDetailsOverride : base.HasChangesInAuditDetails; }
		}

		public override bool IsInDatabase
		{
			get { return isInDatabaseOverride ?? base.IsInDatabase; }
		}

		public void SetIsInDatabase(bool value)
		{
			isInDatabaseOverride = value;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			OnSavingCalled = true;
			OnSavingOpenTransactionCount = CargoWise.Data.Db.Connection.AppTransactionCount;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			SavedSucceded = saveSucceeded;
			OnSavedCalled = true;
			base.OnSaved(saveSucceeded);
		}

		public event EventHandler FactorySavingBeforeTransaction;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (FactorySavingBeforeTransaction != null)
			{
				FactorySavingBeforeTransaction(this, EventArgs.Empty);
			}
		}

		public event EventHandler FactorySaving;

		protected override void OnFactorySaving()
		{
			OnFactorySavingCalled = true;
			base.OnFactorySaving();
			if (FactorySaving != null)
			{
				FactorySaving(this, EventArgs.Empty);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			OnFactorySavedCalled = true;
			FactorySavedSucceded = saveSucceeded;
			base.OnFactorySaved(saveSucceeded);
		}

		public bool OnLoadedCalled;

		public bool OverrideIsSavedByFactory;
		public bool OverrideHasChangesInAuditDetails;
		public bool IsSavedByFactoryOverride = true;
		public bool HasChangesInAuditDetailsOverride = true;

		public int OnSavingOpenTransactionCount;
		public bool OnSavingCalled;
		public bool OnSavedCalled;
		public bool SavedSucceded;

		public bool OnFactorySavingCalled;
		public bool OnFactorySavedCalled;
		public bool FactorySavedSucceded;

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new BusinessObjectFetchStrategyForTest(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return HumanReadableNameForTest ?? base.HumanReadableNameCore;
			}
		}

		public ZString? HumanReadableNameForTest;

		#endregion

		#region Z0_Guid

		[RelatedBusinessObject("RelatedDummy")]
		public override ZGuid Z0_Guid
		{
			get { return base.Z0_Guid; }
			set { base.Z0_Guid = value; }
		}

		public virtual DummyBusinessObject RelatedDummy
		{
			get { return Factory.Load<DummyBusinessObject>(Z0_Guid); }
		}

		#endregion

		#region Z0_Number

		public bool Z0_NumberInfo_Accessed { get; set; }

		public override ZPropertyInfo Z0_NumberInfo
		{
			get
			{
				ZPropertyInfo result = base.Z0_NumberInfo;
				Z0_NumberInfo_Accessed = true;
				return result;
			}
		}

		#endregion

		#region Self

		public bool SelfAccessed { get; set; }

		public virtual DummyBaseBusinessObject Self
		{
			get
			{
				SelfAccessed = true;
				return this;
			}
		}

		#endregion

		#region NonPersistentProperty

		[BusinessObjectTestExclude]
		public ZString NonPersistentProperty
		{
			get { return fNonPersistentProperty; }
			set { SetNonPersistentPropertyValue(NonPersistentPropertyInfo, ref fNonPersistentProperty, value); }
		}

		public ZPropertyInfo NonPersistentPropertyInfo
		{
			get { return GetZPropertyInfo(nameof(NonPersistentProperty)); }
		}

		#endregion

		#region DoNotBindToMe

		public string DoNotBindToMe
		{
			get { return ""; }
			set { }
		}

		#endregion

		#region Testing Update

		public bool DescriptionChangedHasBeenCalled;
		public bool ElementChangedHasBeenCalled;

		protected void DummyBusinessObject_Z0_DescriptionChanged(object sender, EventArgs e)
		{
			DescriptionChangedHasBeenCalled = true;
		}

		protected internal override void OnElementChanged()
		{
			base.OnElementChanged();
			ElementChangedHasBeenCalled = true;
		}

		#endregion

		#region Validation testing

		public int Z0_DescriptionValidationCount { get; set; }

		#endregion

		public int Z0_FK_Code_MaxLength
		{
			get { return maxLengthOverride ?? Schema.Z0_FK_CodeMaxLength; }
			set { maxLengthOverride = value; }
		}
		int? maxLengthOverride;

		ZString fNonPersistentProperty;
		bool? isInDatabaseOverride;
	}
}
#endif
