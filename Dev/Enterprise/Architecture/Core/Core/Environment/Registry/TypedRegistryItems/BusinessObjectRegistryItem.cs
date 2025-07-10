using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class BusinessObjectRegistryItem<T> : RegistryItemWrapper where T : BusinessObject
	{
		#region Constructors
		public BusinessObjectRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, Guid.Empty)
		{
		}

		public BusinessObjectRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, Guid.Empty)
		{
		}

		public BusinessObjectRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, Guid defaultValue)
			: this(name, category, caption, hint, null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public BusinessObjectRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Guid defaultValue)
			: this(name, category, caption, hint, null, storage, options, defaultValue)
		{
		}

		public BusinessObjectRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, GuidRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, Guid defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GuidRegistryDataType(), editorInfo, storage, GetRegistryOptions(options), defaultValue))
		{
		}
		#endregion

		public void SetValue(Guid companyPK, Guid branchPK, Guid departmentPK, Guid primaryKey)
		{
			SetValueCore(companyPK, branchPK, departmentPK, primaryKey);
		}

		public void SetValue(Guid companyPK, Guid branchPK, Guid departmentPK, T businessObject)
		{
			SetValue(companyPK, branchPK, departmentPK, businessObject != null ? businessObject.PK.ToGuid() : Guid.Empty);
		}

		public new BusinessObjectProxy<T> GetValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return new BusinessObjectProxy<T>((Guid)GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK));
		}

		public BusinessObjectProxy<T> Value
		{
			get { return GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		static RegistryOptions GetRegistryOptions(RegistryOptions options)
		{
			return (options & RegistryOptions.IsValueOptional) == RegistryOptions.IsValueOptional ? options : options | RegistryOptions.IsValueMandatory;
		}
	}
}
