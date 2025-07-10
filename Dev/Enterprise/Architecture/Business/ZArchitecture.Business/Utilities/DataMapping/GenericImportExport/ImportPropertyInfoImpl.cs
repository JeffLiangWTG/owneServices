using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ImportPropertyInfoImpl : IImportPropertyInfo
	{
		public delegate FieldType GetFieldTypeDelegate(BusinessObject bizObj);
		[SuppressWeaklyTypedCollectionMessage]
		public delegate IList GetBindToListDelegate(BusinessObject bizObj);
		public delegate ModuleIdentifier GetModuleIDDelegate(BusinessObject bizObj);

		public ImportPropertyInfoImpl(PropertyDescriptor propertyDescriptor, bool isMandatory = false)
		{
			this.propertyDescriptor = propertyDescriptor;
			IsMandatory = isMandatory;
		}

		public ImportPropertyInfoImpl(PropertyDescriptor propertyDescriptor, GetFieldTypeDelegate getFieldType)
			: this(propertyDescriptor)
		{
			this.getFieldType = getFieldType;
		}

		public ImportPropertyInfoImpl(PropertyDescriptor propertyDescriptor, GetFieldTypeDelegate getFieldType, GetBindToListDelegate getBindToList)
			: this(propertyDescriptor, getFieldType, getBindToList, null)
		{
		}

		public ImportPropertyInfoImpl(PropertyDescriptor propertyDescriptor, GetFieldTypeDelegate getFieldType, GetBindToListDelegate getBindToList, GetModuleIDDelegate getModuleID)
			: this(propertyDescriptor, getFieldType)
		{
			this.getBindToList = getBindToList;
			this.getModuleID = getModuleID;
		}

		public ImportPropertyInfoImpl(PropertyDescriptor propertyDescriptor, GetBindToListDelegate getBindToList)
			: this(propertyDescriptor, getBindToList, null)
		{
		}

		public ImportPropertyInfoImpl(PropertyDescriptor propertyDescriptor, GetBindToListDelegate getBindToList, GetModuleIDDelegate getModuleID)
			: this(propertyDescriptor)
		{
			this.getBindToList = getBindToList;
			this.getModuleID = getModuleID;
		}

		public string HeaderText
		{
			get
			{
				if (headerText != null)
				{
					return headerText;
				}
				else
				{
					if (!hasHeaderResString.HasValue)
					{
						var data = DataBoundResourceStrings.GetDataForProperty(propertyDescriptor);
						if (data != null)
						{
							headerResString = data.Caption ?? data.FullDescription;
						}
						hasHeaderResString = headerResString != null;
					}

					return headerResString ?? MappingName;
				}
			}
			set { headerText = value; }
		}
		string headerText;
		bool? hasHeaderResString;
		string headerResString;

		public string MappingName
		{
			get { return propertyDescriptor.Name; }
		}

		public Type PropertyType
		{
			get { return propertyDescriptor.PropertyType; }
		}

		public Type ComponentType
		{
			get { return propertyDescriptor.ComponentType; }
		}

		public int ColumnWidth
		{
			get { return columnWidth; }
			set { columnWidth = value; }
		}
		int columnWidth = 80;

		[SuppressWeaklyTypedCollectionMessage]
		public IList GetBindToList(BusinessObject bizObj)
		{
			IList result = getBindToList == null ? MetaData.GetListDataSource(bizObj, propertyDescriptor) as IList : getBindToList(bizObj);
			return result;
		}

		public ModuleIdentifier GetModuleID(BusinessObject bizObj)
		{
			ModuleIdentifier result = null;
			if (getModuleID == null)
			{
				IList list = GetBindToList(bizObj);
				if (list != null)
				{
					result = ZMetaData.GetModuleId(list);
				}
			}
			else
			{
				result = getModuleID(bizObj);
			}
			return result;
		}

		public bool IsMandatory { get; }

		public bool IsMultiControl
		{
			get { return getFieldType != null; }
		}

		public Type GetExpectedTypeForMultiControl(BusinessObject bizObj)
		{
			if (getFieldType != null)
			{
				switch (getFieldType(bizObj))
				{
					case FieldType.Guid:
					case FieldType.OrganisationGuid:
						return typeof(ZGuid);

					case FieldType.DateTime:
					case FieldType.Date:
						return typeof(ZDateTime);

					case FieldType.DateTimeOffset:
						return typeof(ZDateTimeOffset);

					case FieldType.Time:
						return typeof(ZTime);

					case FieldType.Geography:
						return typeof(ZGeography);

					case FieldType.Decimal:
						return typeof(ZDecimal);

					case FieldType.Byte:
						return typeof(ZByte);

					case FieldType.Integer:
						return typeof(ZInt);
				}
			}

			return typeof(ZString);
		}

		public bool IsReadOnly
		{
			get { return propertyDescriptor.IsReadOnly; }
		}

		ZCharacterCasing characterCasing = ZCharacterCasing.Normal;
		public ZCharacterCasing CharacterCasing
		{
			get { return characterCasing; }
			set { characterCasing = value; }
		}

		public string FieldTypeColumnName => string.Empty;

		readonly GetFieldTypeDelegate getFieldType;
		readonly GetBindToListDelegate getBindToList;
		readonly GetModuleIDDelegate getModuleID;
		readonly PropertyDescriptor propertyDescriptor;
	}

	public class ImportPropertyInfoImpl<T> : ImportPropertyInfoImpl
	{
		public ImportPropertyInfoImpl(string propertyName, bool isMandatory = false)
			: base(GetPropertyDescriptor(propertyName), isMandatory)
		{ }

		public ImportPropertyInfoImpl(string propertyName, GetFieldTypeDelegate getFieldType)
			: base(GetPropertyDescriptor(propertyName), getFieldType)
		{ }

		public ImportPropertyInfoImpl(string propertyName, GetFieldTypeDelegate getFieldType, GetBindToListDelegate getBindToList)
			: base(GetPropertyDescriptor(propertyName), getFieldType, getBindToList)
		{ }

		public ImportPropertyInfoImpl(string propertyName, GetFieldTypeDelegate getFieldType, GetBindToListDelegate getBindToList, GetModuleIDDelegate getModuleID)
			: base(GetPropertyDescriptor(propertyName), getFieldType, getBindToList, getModuleID)
		{ }

		public ImportPropertyInfoImpl(string propertyName, GetBindToListDelegate getBindToList)
			: base(GetPropertyDescriptor(propertyName), getBindToList)
		{ }

		public ImportPropertyInfoImpl(string propertyName, GetBindToListDelegate getBindToList, GetModuleIDDelegate getModuleID)
			: base(GetPropertyDescriptor(propertyName), getBindToList, getModuleID)
		{ }

		static PropertyDescriptor GetPropertyDescriptor(string propertyName)
		{
			return ZCustomTypeDescriptor.GetProperties(typeof(T))[propertyName];
		}
	}

	public class ImportPropertyInfoCollection : IEnumerable<IImportPropertyInfo>
	{
		public void Add(IImportPropertyInfo property)
		{
			properties.Add(property);
		}

		readonly List<IImportPropertyInfo> properties = new List<IImportPropertyInfo>();

		#region IEnumerable<IImportPropertyInfo> Members

		IEnumerator<IImportPropertyInfo> IEnumerable<IImportPropertyInfo>.GetEnumerator()
		{
			return properties.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return properties.GetEnumerator();
		}

		#endregion
	}
}
