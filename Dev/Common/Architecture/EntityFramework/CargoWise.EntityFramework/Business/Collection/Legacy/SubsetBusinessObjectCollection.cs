using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	public interface ISubsetBusinessObjectCollection : IBusinessObjectCollection
	{
		BusinessObjectCollection CollectionToFilter { get; }

		void Load();
		void Load(ZQuery alternativeAdditionalFilter);
		void Rebuild();
		void RemoveAndDelete(BusinessObject businessObject);
		void SwapCollectionToFilter(BusinessObjectCollection newCollection);
	}

	/// <summary>
	/// A view presenting a subset of another collection
	/// </summary>
	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection]
	public abstract class SubsetBusinessObjectCollection<TBusinessObject> : BusinessObjectCollection<TBusinessObject>, IBusinessObjectCollectionInternals, ISubsetBusinessObjectCollection
		where TBusinessObject : BusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "It is not time to change this")]
		protected SubsetBusinessObjectCollection(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter.Factory)
		{
			this.collectionToFilter = collectionToFilter;
			RebuildOnConstruction();
		}

		/// <summary>
		/// Determines whether a BusinessObject is part of the CollectionView
		/// </summary>
		protected abstract bool IsThisPartOfTheCollection(BusinessObject element);

		protected virtual void RebuildOnConstruction()
		{
			Rebuild();
		}

		/// <summary>
		/// Rebuilds the collection from the CollectionToFilter, evaluating the filter for each item
		/// </summary>
		public void Rebuild()
		{
			if (suppressRebuild == 0)
			{
				ForceRebuild();
			}
		}

		public void ForceRebuild() => RebuildCore();

		public IDisposable SuppressRebuild()
		{
			suppressRebuild++;
			return new UnsuppressRebuild(this);
		}

		sealed class UnsuppressRebuild : IDisposable
		{
			readonly SubsetBusinessObjectCollection<TBusinessObject> subset;

			public UnsuppressRebuild(SubsetBusinessObjectCollection<TBusinessObject> subset)
			{
				this.subset = subset;
			}

			public void Dispose() => subset.suppressRebuild--;
		}

		int suppressRebuild;
		bool fIsRebuilding;
		public bool IsRebuilding
		{
			get { return fIsRebuilding; }
		}

		protected virtual Rebuilder GetRebuilder() => new Rebuilder(this);

		protected class Rebuilder
		{
			protected internal Rebuilder(SubsetBusinessObjectCollection<TBusinessObject> collection)
			{
				this.collection = collection;
			}

			readonly SubsetBusinessObjectCollection<TBusinessObject> collection;

			protected virtual bool IsInFilteredCollection(BusinessObject element) => collection.CollectionToFilter.Contains(element);

			protected virtual IEnumerable<BusinessObject> GetElementsForRebuild() => collection.CollectionToFilter.Cast<BusinessObject>();

			protected virtual void Remove(BusinessObject element) => collection.Remove(element, false);

			protected virtual void RebuildCore()
			{
				List<BusinessObject> removes = null;

				foreach (var bizO in collection)
				{
					if (bizO.IsDeleted ||
						!(IsInFilteredCollection(bizO) && collection.IsThisPartOfTheCollection(bizO))
						&& !collection.IsNonCommittedCollectionElement(bizO))
					{
						if (removes == null)
						{
							removes = new List<BusinessObject>();
						}

						removes.Add(bizO);
					}
				}

				if (removes != null)
				{
					foreach (BusinessObject bizO in removes)
					{
						Remove(bizO);
					}
				}

				foreach (var bizO in GetElementsForRebuild().ToArray())
				{
#if DEBUG
					if (Interlocked.Read(ref NeedsSignalStuffForTest) == 1L)
					{
						CargoWise.EntityFramework.Testing.SubsetBusinessObjectCollectionTest<TBusinessObject>.workThreadForASignal.Set();
						CargoWise.EntityFramework.Testing.SubsetBusinessObjectCollectionTest<TBusinessObject>.mainThreadForASignal.WaitOne();
					}
#endif
					if (!IsInFilteredCollection(bizO))
					{
						continue;
					}

					if (!bizO.IsDeleted && collection.IsThisPartOfTheCollection(bizO) && !collection.Contains(bizO))
					{
						collection.RebuildAddingBizoPk = bizO.PK;
						collection.Add(bizO);
					}
				}
				collection.RebuildAddingBizoPk = ZGuid.Empty;
			}

			public void Rebuild()
			{
				collection.fIsRebuilding = true;
				try
				{
					using (collection.SuspendListChanged())
					{
						RebuildCore();
					}
				}
				finally
				{
					collection.fIsRebuilding = false;
				}
			}
		}

		protected virtual void RebuildCore() => GetRebuilder().Rebuild();

#if DEBUG
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		internal static long NeedsSignalStuffForTest = 0L;
#endif

		internal ZGuid RebuildAddingBizoPk { get => rebuildAddingBizoPk; private set => rebuildAddingBizoPk = value; }
		protected ZGuid rebuildAddingBizoPk;

		public override void Load(ZQuery filter)
		{
			ErrorReporter.ReportOnce(GetType().FullName + ".Load", "Cannot load this collection. Please load the FilteredCollection instead.");
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return CollectionToFilter.GetTypeOfElementsFromPK(pk);
		}

		public BusinessObjectCollection CollectionToFilter
		{
			get { return collectionToFilter; }
		}

		public void SwapCollectionToFilter(BusinessObjectCollection newCollection)
		{
			if (newCollection == null)
			{
				throw new ArgumentNullException(nameof(newCollection));
			}

			if (newCollection != CollectionToFilter)
			{
				SwapFactoryAndRemoveAll(newCollection.Factory);
				SwapCollectionToFilterCore(newCollection);
			}
		}

		protected virtual void SwapCollectionToFilterCore(BusinessObjectCollection newCollection)
		{
			collectionToFilter = newCollection;
			Rebuild();
		}

		protected BusinessObjectCollection collectionToFilter;

		protected override ITypeDeciderContext GetTypeDeciderContextCore() => collectionToFilter.GetTypeDeciderContext();

		#region IBusinessObjectCollectionInternals Members

		protected override bool HasChangedFromDeleteCore
		{
			get { return base.HasChangedFromDeleteCore || ((IBusinessObjectCollectionInternals)collectionToFilter).HasChangesFromDelete; }
			set
			{
				base.HasChangedFromDeleteCore = value;
				((IBusinessObjectCollectionInternals)collectionToFilter).HasChangesFromDelete = value;
			}
		}

		bool IBusinessObjectCollectionInternals.MastersAreInDatabase
		{
			get { return ((IBusinessObjectCollectionInternals)collectionToFilter).MastersAreInDatabase; }
		}

		bool IBusinessObjectCollectionInternals.MastersAreDeleted
		{
			get { return ((IBusinessObjectCollectionInternals)collectionToFilter).MastersAreDeleted; }
		}

		#endregion
	}
}

#region Test

#if DEBUG

namespace CargoWise.EntityFramework.Testing
{
	abstract class SubsetBusinessObjectCollectionTest<TBusinessObject> : TestCaseWithDummy
		where TBusinessObject : BusinessObject
	{
		public static EventWaitHandle workThreadForASignal = new EventWaitHandle(false, EventResetMode.ManualReset);
		public static EventWaitHandle mainThreadForASignal = new EventWaitHandle(true, EventResetMode.ManualReset);
	}
}

#endif

#endregion
