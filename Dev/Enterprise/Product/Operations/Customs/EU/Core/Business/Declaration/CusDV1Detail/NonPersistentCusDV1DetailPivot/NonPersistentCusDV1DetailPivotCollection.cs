using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface INonPersistentCusDV1DetailPivotCollection<out T> : IBusinessObjectCollection<T>
		where T : NonPersistentCusDV1DetailPivot
	{
	}

	public class NonPersistentCusDV1DetailPivotCollection<TParent, T> : NonPersistentBusinessObjectCollection<T>
	, INonPersistentCusDV1DetailPivotCollection<T>
		where T : NonPersistentCusDV1DetailPivot
		where TParent : CusEntryInstruction
	{
		public NonPersistentCusDV1DetailPivotCollection(TParent entryInstruction, Func<TParent, T> newNonPersistentObjectFactory)
			: base(entryInstruction?.Factory)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			this.newNonPersistentObjectFactory = Argument.NotNull(newNonPersistentObjectFactory, nameof(newNonPersistentObjectFactory));
			if (entryInstruction.JobDeclaration != null && entryInstruction.JobDeclaration.DV1Details != null)
			{
				var dv1Details = entryInstruction.JobDeclaration.DV1Details;
				dv1Details.CountChanged += DV1Details_CountChanged;
				using (SuspendSettingHasChanges())
				{
					foreach (CusDV1Detail dv1Detail in dv1Details)
					{
						AddNewNonPersistentDV1Detail(dv1Detail);
					}
				}
			}
		}

		readonly TParent entryInstruction;
		readonly Func<TParent, T> newNonPersistentObjectFactory;

		protected virtual void DV1Details_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				AddNewNonPersistentDV1Detail((CusDV1Detail)e.BizObject);
			}
			else // removed
			{
				RemoveNonPersistentDV1Detail((CusDV1Detail)e.BizObject);
			}
		}

		public T AddNewNonPersistentDV1Detail(CusDV1Detail dv1Detail)
		{
			var npDV1Detail = AddNew();
			npDV1Detail.DV1Detail = dv1Detail;
			return npDV1Detail;
		}

		public void RemoveNonPersistentDV1Detail(CusDV1Detail dv1Detail)
		{
			if (!dv1Detail.IsDeleted)
			{
				foreach (var dv1DetailForDelete in this)
				{
					if (dv1DetailForDelete.Sequence == dv1Detail.Sequence)
					{
						RemoveAndDelete(dv1DetailForDelete);
						return;
					}
				}
			}
		}

		protected override bool AllowNewCore => false;
		protected override BusinessObject CreateNonPersistentBusinessObject() => newNonPersistentObjectFactory(entryInstruction);

		public IEnumerator<T> GetEnumerator()
		{
			return Cast<T>(this).GetEnumerator();
		}

		static IEnumerable<TResult> Cast<TResult>(IEnumerable source)
		{
			foreach (TResult result in source)
			{
				yield return result;
			}
		}
	}
}
