using System;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class BillCustomisationByServiceLevelRegistryItem : StronglyTypedRegistryItem<BillOfLadingNumberCustomisationsByServiceLevel>
	{
		public BillCustomisationByServiceLevelRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, BillCustomisationByServiceLevelRegistryDataType dataType)
			: base(new BillCustomisationByServiceLevelItemImpl(name, category, caption, hint, dataType, storage, null)) { }

		public BillCustomisationByServiceLevelRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BillCustomisationByServiceLevelRegistryDataType dataType)
			: base(new BillCustomisationByServiceLevelItemImpl(name, category, caption, hint, dataType, storage, options, null)) { }

		public BillCustomisationByServiceLevelRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, BillCustomisationByServiceLevelRegistryDataType dataType, BillCustomisationByServiceLevelRegistryItem parentItem)
			: base(new BillCustomisationByServiceLevelItemImpl(name, category, caption, hint, dataType, storage, parentItem)) { }

		public BillCustomisationByServiceLevelRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, BillCustomisationByServiceLevelRegistryDataType dataType, BillCustomisationRegistryItem parentItem)
			: base(new BillCustomisationItemImpl(name, category, caption, hint, dataType, storage, parentItem)) { }

		public BillCustomisationByServiceLevelRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BillCustomisationByServiceLevelRegistryDataType dataType, BillCustomisationRegistryItem parentItem)
			: base(new BillCustomisationItemImpl(name, category, caption, hint, dataType, storage, options, parentItem)) { }

		[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
		class BillCustomisationByServiceLevelItemImpl : RegistryItemImpl
		{
			public BillCustomisationByServiceLevelItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillCustomisationByServiceLevelRegistryDataType dataType, RegistryStorageFlags storage, BillCustomisationByServiceLevelRegistryItem parentItem)
				: base(name, category, caption, hint, dataType, storage)
			{
				this.parentItem = parentItem;
			}

			public BillCustomisationByServiceLevelItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillCustomisationByServiceLevelRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, BillCustomisationByServiceLevelRegistryItem parentItem)
				: base(name, category, caption, hint, dataType, storage, options)
			{
				this.parentItem = parentItem;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				if (parentItem != null)
				{
					var retriever = new RegistryItemProposedValueAccessor(parentItem, new FallbackLevel(companyPK, branchPK, departmentPK));
					return ProcessDefaultForDependantRegistryItem((BillOfLadingNumberCustomisationsByServiceLevel)retriever.GetCurrentValue().Value);
				}
				else
				{
					var defaultValue = base.GetDefaultValueCore(companyPK, branchPK, departmentPK) as BillOfLadingNumberCustomisationsByServiceLevel;
					defaultValue.MaxAllowedLength = ((BillCustomisationByServiceLevelRegistryDataType)DataType).MaxLength;
					return defaultValue;
				}
			}

			BillOfLadingNumberCustomisationsByServiceLevel ProcessDefaultForDependantRegistryItem(BillOfLadingNumberCustomisationsByServiceLevel parentValue)
			{
				var dataType = (BillCustomisationByServiceLevelRegistryDataType)DataType;

				var result = (BillOfLadingNumberCustomisationsByServiceLevel)parentValue.Clone(parentValue.CurrentFallbackLevel, parentValue.Factory);
				result.AllowNonAlphanumericCharacters = dataType.AllowNonAlphanumericCharacters;
				result.EnableMacroInsertion = dataType.EnableMacroInsertion;
				result.Categories = dataType.Categories;
				result.MaxAllowedLength = dataType.MaxLength;

				return result;
			}

			readonly BillCustomisationByServiceLevelRegistryItem parentItem;
		}

		[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
		class BillCustomisationItemImpl : RegistryItemImpl
		{
			public BillCustomisationItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillCustomisationByServiceLevelRegistryDataType dataType, RegistryStorageFlags storage, BillCustomisationRegistryItem parentItem)
				: base(name, category, caption, hint, dataType, storage)
			{
				this.parentItem = parentItem;
			}

			public BillCustomisationItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillCustomisationByServiceLevelRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, BillCustomisationRegistryItem parentItem)
				: base(name, category, caption, hint, dataType, storage, options)
			{
				this.parentItem = parentItem;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				if (parentItem != null)
				{
					RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(parentItem, new FallbackLevel(companyPK, branchPK, departmentPK));
					return ProcessDefaultForDependantRegistryItem((BillOfLadingNumberCustomisationsByServiceLevel)base.GetDefaultValueCore(companyPK, branchPK, departmentPK), (BillOfLadingNumberCustomisation)retriever.GetCurrentValue().Value);
				}
				else
				{
					return base.GetDefaultValueCore(companyPK, branchPK, departmentPK);
				}
			}

			BillOfLadingNumberCustomisationsByServiceLevel ProcessDefaultForDependantRegistryItem(BillOfLadingNumberCustomisationsByServiceLevel current, BillOfLadingNumberCustomisation parentValue)
			{
				BillCustomisationByServiceLevelRegistryDataType dataType = (BillCustomisationByServiceLevelRegistryDataType)DataType;

				foreach (BillOfLadingNumberCustomisation customisation in current.BillOfLadingNumberCustomisations) //by service level
				{
					ZString serviceLevel = customisation.ServiceLevel;
					customisation.CopyValuesFrom(parentValue);
					customisation.ServiceLevel = serviceLevel;
					customisation.AllowNonAlphanumericCharacters = dataType.AllowNonAlphanumericCharacters;
					customisation.EnableMacroInsertion = dataType.EnableMacroInsertion;
					customisation.Categories = dataType.Categories;
				}

				return current;
			}

			readonly BillCustomisationRegistryItem parentItem;
		}
	}
}
