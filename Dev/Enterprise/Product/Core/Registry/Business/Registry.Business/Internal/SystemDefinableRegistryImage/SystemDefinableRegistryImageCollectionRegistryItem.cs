using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business;

public class SystemDefinableRegistryImageCollectionRegistryItem : RegistryImageCollectionRegistryItem<SystemDefinableRegistryImageCollection>
{
	public SystemDefinableRegistryImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
		: this(name, category, caption, hint, storage, RegistryOptions.Default)
	{
	}

	public SystemDefinableRegistryImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
		: this(name, category, caption, hint, storage, options, false)
	{
	}

	public SystemDefinableRegistryImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool emptyDefaultValue)
		: this(name, category, caption, hint, storage, RegistryOptions.Default, emptyDefaultValue)
	{
	}

	public SystemDefinableRegistryImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool emptyDefaultValue)
		: base(new RegistryItemImplWithDefaultValue(name, category, caption, hint, new SystemDefinableRegistryImageCollectionRegistryDataType(), storage, options, emptyDefaultValue))
	{
	}

	protected override SystemDefinableRegistryImageCollection GetEmptyValue()
	{
		return new SystemDefinableRegistryImageCollection();
	}

	protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
	{
		var value = base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as SystemDefinableRegistryImageCollection;
		if (value != null)
		{
			var defaults = DefaultValue;
			if (defaults != null)
			{
				FixMissingSystemDefinedImages(value, defaults);
			}
		}

		return value;
	}

	static void FixMissingSystemDefinedImages(SystemDefinableRegistryImageCollection value, SystemDefinableRegistryImageCollection defaults)
	{
		foreach (SystemDefinableRegistryImage defaultItem in defaults.Cast<SystemDefinableRegistryImage>()
					.Where(d => d.SystemDefined))
		{
			var item = value.FindByCodeAndSystemDefined(defaultItem.Code, systemDefined: true);

			if (item == null)
			{
				item = value.AddNew();
				item.Code = defaultItem.Code;
				item.Description = defaultItem.Description;
				item.SystemDefined = defaultItem.SystemDefined;
				item.Image = defaultItem.Image;
			}
		}
	}

	#region class RegistryItemImplWithDefaultValue

	class RegistryItemImplWithDefaultValue : RegistryImageCollectionRegistryItemImpl
	{
		public RegistryItemImplWithDefaultValue(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, SystemDefinableRegistryImageCollectionRegistryDataType dataType, RegistryStorageFlags storage, bool emptyDefaultValue)
			: this(name, category, caption, hint, dataType, storage, RegistryOptions.Default, emptyDefaultValue)
		{
		}

		public RegistryItemImplWithDefaultValue(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, SystemDefinableRegistryImageCollectionRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, bool emptyDefaultValue)
			: base(name, category, caption, hint, dataType, storage, options)
		{
			this.emptyDefaultValue = emptyDefaultValue;
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return emptyDefaultValue ? new SystemDefinableRegistryImageCollection() : GetDefaultValue();
		}

		SystemDefinableRegistryImageCollection GetDefaultValue()
		{
			SystemDefinableRegistryImageCollection result = new SystemDefinableRegistryImageCollection();
			var image1 = result.AddNew();

			image1.Code = "QRB";
			image1.SystemDefined = true;
			image1.Description = ResString.GetMultilingualString("47463640-97D6-43B6-879D-29EDA457C15E", "QR Bill Logo");
			image1.DefaultImageResourceName = "Enterprise.Registry.Business.Internal.SystemDefinableRegistryImage.QRB_Flag_of_Switzerland.png";

			return result;
		}

		readonly bool emptyDefaultValue;
	}

	#endregion

	[RegistryEditor("Enterprise.Registry.GUI.SystemDefinableRegistryImageCollectionRegistryItemEditor, Enterprise.Registry.GUI")]
	internal class SystemDefinableRegistryImageCollectionRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<SystemDefinableRegistryImageCollection>
	{
		protected override bool ValuesAreEqualCore(SystemDefinableRegistryImageCollection a, SystemDefinableRegistryImageCollection b)
		{
			return a.ContainsSameElementsInAnyOrder(b);
		}
	}
}
