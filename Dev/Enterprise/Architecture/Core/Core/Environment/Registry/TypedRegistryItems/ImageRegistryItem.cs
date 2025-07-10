using System;
using System.Drawing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ImageRegistryItem : StronglyTypedRegistryItem<Image>
	{
		public ImageRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public ImageRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, RegistryOptions.Default)
		{
		}

		public ImageRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, int.MaxValue, int.MaxValue, null)
		{
		}

		public ImageRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Image defaultValue)
			: this(name, category, caption, hint, storage, options, int.MaxValue, int.MaxValue, defaultValue)
		{
		}

		public ImageRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int maxWidth, int maxHeight, Image defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ImageRegistryDataType(maxWidth, maxHeight), storage, options, defaultValue))
		{
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var value = base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK);
			if (DataType.IsNullDataRepresentation(value))
			{
				return null;
			}
			else
			{
				return value;
			}
		}
	}
}
