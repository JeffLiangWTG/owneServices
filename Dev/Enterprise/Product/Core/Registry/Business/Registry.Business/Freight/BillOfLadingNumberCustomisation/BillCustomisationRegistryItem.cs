using System;
using CargoWise.Common.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class BillCustomisationRegistryItem : StronglyTypedRegistryItem<BillOfLadingNumberCustomisation>
	{
		public BillCustomisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, BillCustomisationRegistryDataType dataType)
			: base(new BillCustomisationItemImpl(name, category, caption, hint, dataType, storage, null)) { }

		public BillCustomisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, BillCustomisationRegistryDataType dataType, BillCustomisationRegistryItem parentItem)
			: base(new BillCustomisationItemImpl(name, category, caption, hint, dataType, storage, parentItem)) { }

		public BillCustomisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BillCustomisationRegistryDataType dataType)
			: base(new BillCustomisationItemImpl(name, category, caption, hint, dataType, storage, options, null)) { }

		public BillCustomisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BillCustomisationRegistryDataType dataType, BillCustomisationRegistryItem parentItem)
			: base(new BillCustomisationItemImpl(name, category, caption, hint, dataType, storage, options, parentItem)) { }

		[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
		class BillCustomisationItemImpl : RegistryItemImpl
		{
			public BillCustomisationItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillCustomisationRegistryDataType dataType, RegistryStorageFlags storage, BillCustomisationRegistryItem parentItem)
				: base(name, category, caption, hint, dataType, storage)
			{
				this.parentItem = parentItem;
			}

			public BillCustomisationItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillCustomisationRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, BillCustomisationRegistryItem parentItem)
				: base(name, category, caption, hint, dataType, storage, options)
			{
				this.parentItem = parentItem;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				if (parentItem != null)
				{
					RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(parentItem, new FallbackLevel(companyPK, branchPK, departmentPK));
					return ProcessDefaultForDependantRegistryItem((BillOfLadingNumberCustomisation)retriever.GetCurrentValue().Value);
				}
				else
				{
					BillOfLadingNumberCustomisation defaultValue = base.GetDefaultValueCore(companyPK, branchPK, departmentPK) as BillOfLadingNumberCustomisation;
					defaultValue.MaxAllowedLength = ((BillCustomisationRegistryDataType)DataType).MaxLength;
					return defaultValue;
				}
			}

			BillOfLadingNumberCustomisation ProcessDefaultForDependantRegistryItem(BillOfLadingNumberCustomisation parentValue)
			{
				BillCustomisationRegistryDataType dataType = (BillCustomisationRegistryDataType)DataType;

				BillOfLadingNumberCustomisation result = (BillOfLadingNumberCustomisation)parentValue.Clone(parentValue.CurrentFallbackLevel, parentValue.Factory);
				result.Categories = dataType.Categories;
				result.AllowNonAlphanumericCharacters = dataType.AllowNonAlphanumericCharacters;
				result.EnableMacroInsertion = dataType.EnableMacroInsertion;
				result.PrefixLength = dataType.PrefixLength;
				result.UseShipmentSequenceNumber = true;
				result.MaxAllowedLength = dataType.MaxLength;

				foreach (BillOfLadingNumberCustomisationElement element in result.UnFilteredElements)
				{
					element.Fountain = false;
				}

				return result;
			}

			readonly BillCustomisationRegistryItem parentItem;
		}
	}
}
