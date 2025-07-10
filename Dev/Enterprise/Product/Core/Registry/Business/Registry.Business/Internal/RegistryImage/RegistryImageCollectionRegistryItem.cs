using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class RegistryImageCollectionRegistryItem : RegistryImageCollectionRegistryItem<RegistryImageCollection>
	{
		public RegistryImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryImageCollectionRegistryItemImpl(name, category, caption, hint, new RegistryImageCollectionRegistryDataType(), storage))
		{
		}

		public RegistryImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryImageCollectionRegistryItemImpl(name, category, caption, hint, new RegistryImageCollectionRegistryDataType(), storage, options))
		{
		}

		protected override RegistryImageCollection GetEmptyValue()
		{
			return new RegistryImageCollection();
		}

		[RegistryEditor("Enterprise.Registry.GUI.RegistryImageCollectionRegistryItemEditor, Enterprise.Registry.GUI")]
		internal class RegistryImageCollectionRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<RegistryImageCollection>
		{
			protected override bool ValuesAreEqualCore(RegistryImageCollection a, RegistryImageCollection b)
			{
				return a.ContainsSameElementsInAnyOrder(b);
			}
		}
	}

	public abstract class RegistryImageCollectionRegistryItem<T> : StronglyTypedRegistryItem<T> where T : RegistryImageCollection
	{
		internal RegistryImageCollectionRegistryItem(RegistryImageCollectionRegistryItemImpl inner)
			: base(inner)
		{
		}

		internal T LatestValue(FallbackLevel fallback)
		{
			T result = GetEmptyValue();

			if (fallback != null)
			{
				RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(this, fallback);
				result = (T)retriever.GetFallBackValue().Value;
			}

			return result;
		}

		protected abstract T GetEmptyValue();

		#region Code Description Lists

		internal CodeDescriptionPairList LatestImagesCodeDescList(FallbackLevel fallback)
		{
			return GetImagesCodeDescList(LatestValue(fallback));
		}

		internal virtual void AddCodeAndDescriptionToList(CodeDescriptionPairList list, RegistryImage element)
		{
			list.AddPair(element.Code, element.Description);
		}

		CodeDescriptionPairList GetImagesCodeDescList(RegistryImageCollection collection)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			foreach (RegistryImage registryImage in collection)
			{
				AddCodeAndDescriptionToList(result, registryImage);
			}

			return result;
		}

		#endregion

		#region class RegistryImageCollectionRegistryItemImpl

		internal class RegistryImageCollectionRegistryItemImpl : RegistryItemImpl
		{
			public RegistryImageCollectionRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, category, caption, hint, dataType, storage)
			{
			}

			public RegistryImageCollectionRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, dataType, storage, options)
			{
			}

			protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
			{
				string fallbackKey = GetFallbackKey(companyOrOwnerPK, branchPK, departmentPK);
				RegistryImageCollection collection = (RegistryImageCollection)newValue;
				collection.SetFallbackKey(fallbackKey);
				base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, collection);
				collection.CleanUpDeletedElements(fallbackKey);
			}

			protected override void DeleteValueCore(Guid companyPk, Guid branchPk, Guid departmentPk)
			{
				RegistryImageCollection collection = (RegistryImageCollection)GetValueWithoutFallback(companyPk, branchPk, departmentPk);
				base.DeleteValueCore(companyPk, branchPk, departmentPk);
				collection.RemoveAndDeleteAll();
				collection.CleanUpDeletedElements(GetFallbackKey(companyPk, branchPk, departmentPk));
			}

			string GetFallbackKey(Guid companyPk, Guid branchPk, Guid departmentPk)
			{
				return new RegistryStorageKey(companyPk, branchPk, departmentPk).Key;
			}
		}

		#endregion
	}
}
