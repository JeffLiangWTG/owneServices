using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class LookupFilterFieldBase : FilterFieldValueSerialisable
	{
		public LookupFilterFieldBase(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected LookupFilterFieldBase(BaseFieldJsonData data) : base(data)
		{
		}

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is LookupFilterFieldBase { CollectionProvider: not null } originalLookupFilterFieldBase)
			{
				if (CollectionProvider is null)
				{
					SetCollectionProvider(originalLookupFilterFieldBase.CollectionProvider);
				}
				CollectionProvider.SetDependencyValue(originalLookupFilterFieldBase.DependencyValue);
			}
		}

		protected internal CollectionProvider CollectionProvider
		{
			get { return fCollectionProvider; }
		}
		[NonSerialized]
		protected CollectionProvider fCollectionProvider;

		#region ILookupFilterField

		public IBusinessObjectCollection BindToList
		{
			get { return CollectionProvider == null ? null : CollectionProvider.Collection; }
		}

		public IBusinessObjectCollection BindToFindBoxList
		{
			get { return CollectionProvider == null ? null : CollectionProvider.CollectionForFindbox; }
		}

		public void SetCollectionProvider(CollectionProvider collectionProvider)
		{
			fCollectionProvider = collectionProvider;
		}

		internal CollectionProvider GetCollectionProvider()
		{
			return fCollectionProvider;
		}

		public ModuleIdentifier ModuleID
		{
			get { return CollectionProvider == null ? null : CollectionProvider.ModuleID; }
		}

		public string LookupType
		{
			get { return CollectionProvider == null ? null : CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(CollectionProvider.GetType()); }
		}

		#endregion

		protected override void SetDependencyValue(string value)
		{
			base.SetDependencyValue(value);
			CollectionProvider.SetDependencyValue(value);
		}

		public void AddMasterRelation(string relateToFilterField, string relationType)
		{
			MasterRelations.Add(new MasterDetailRelation(this, relateToFilterField, relationType));
		}

		public void SetupAllMasterDetailRelations(CollectionOfIFilter lookupFilters)
		{
			foreach (MasterDetailRelation relation in MasterRelations)
			{
				LookupFilterFieldBase masterFilterField = FindFilterField(lookupFilters, relation.MasterFilterFieldName);
				relation.SetMasterFilter(masterFilterField);
				masterFilterField.DetailRelations.Add(relation);
			}
		}

		protected bool CheckCollectionProviderHasSameTypeAsAnotherField(FilterField otherFilterField)
		{
			LookupFilterFieldBase otherLookupFilter = otherFilterField as LookupFilterFieldBase;
			return
				otherLookupFilter != null &&
				otherLookupFilter.CollectionProvider != null &&
				CollectionProvider.GetType().FullName == otherLookupFilter.CollectionProvider.GetType().FullName;
		}

		protected internal List<MasterDetailRelation> MasterRelations
		{
			get { return masterRelations; }
		}

		protected internal List<MasterDetailRelation> DetailRelations
		{
			get { return detailRelations; }
		}

		LookupFilterFieldBase FindFilterField(CollectionOfIFilter lookupFilters, string filterName)
		{
			foreach (LookupFilterFieldBase lookupFilter in lookupFilters)
			{
				if (lookupFilter.DisplayName.ToLower().Trim() == filterName.ToLower().Trim())
				{
					return lookupFilter;
				}
			}

			throw new DocumentEngineException(string.Format("Filter {0} could not be found in master-detail relation.", filterName));
		}

		readonly List<MasterDetailRelation> masterRelations = new List<MasterDetailRelation>();
		readonly List<MasterDetailRelation> detailRelations = new List<MasterDetailRelation>();
	}
}
