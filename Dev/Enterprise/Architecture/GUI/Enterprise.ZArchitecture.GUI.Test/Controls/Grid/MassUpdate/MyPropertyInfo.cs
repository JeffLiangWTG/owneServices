using System;
using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	sealed class MyPropertyInfo : IImportPropertyInfo
	{
		public MyPropertyInfo(Type type)
		{
			PropertyType = type;
		}

		public ZCharacterCasing CharacterCasing { get { throw new NotImplementedException(); } }
		public int ColumnWidth { get { throw new NotImplementedException(); } }
		public Type ComponentType { get { return propertyType; } }
		public string HeaderText { get { throw new NotImplementedException(); } }
		public bool IsMandatory => false;
		public bool IsMultiControl { get { return false; } }
		public bool IsReadOnly { get { throw new NotImplementedException(); } }
		public string MappingName { get { return propertyType.Name; } }
		public Type PropertyType { get { return propertyType; } set { propertyType = value; } }
		public string FieldTypeColumnName => string.Empty;

		Type propertyType;

		public IList GetBindToList(BusinessObject bizObj)
		{
			throw new NotImplementedException();
		}
		public Type GetExpectedTypeForMultiControl(BusinessObject bizObj)
		{
			throw new NotImplementedException();
		}

		public ModuleIdentifier GetModuleID(BusinessObject bizObj)
		{
			throw new NotImplementedException();
		}
	}
}
