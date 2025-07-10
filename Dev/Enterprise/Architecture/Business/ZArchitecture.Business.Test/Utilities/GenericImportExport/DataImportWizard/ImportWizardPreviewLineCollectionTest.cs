using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ImportWizardPreviewLineCollection))]
	sealed class ImportWizardPreviewLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportWizardPreviewLineCollection>
	{
		public void TestCollection()
		{
			var collection = GetCollectionToTest();
			var bizObj = collection.AddNew();
			var properties = (BusinessObjectPropertyDescriptorCollection)TypeDescriptor.GetProperties(bizObj);
			AssertNotNull(properties["DIW_G"]);
			AssertNotNull(properties["DIW_S"]);
			AssertNotNull(properties["DIW_M"]);
			AssertNotNull(properties["DIW_D"]);
			AssertNotNull(properties["DIW_O"]);
			AssertNotNull(properties["DIW_I"]);
			AssertNotNull(properties["DIW_B"]);

			AssertEquals(listG, MetaData.GetListDataSource(bizObj, properties["DIW_G"]));
			AssertEquals(listS, MetaData.GetListDataSource(bizObj, properties["DIW_S"]));
		}

		public void TestCollectionContainsFieldTypeColumnName()
		{
			var collection = GetCollectionToTest();
			var bizObj = collection.AddNew();
			var properties = (BusinessObjectPropertyDescriptorCollection)TypeDescriptor.GetProperties(bizObj);
			AssertNotNull(properties["TestFieldTypeColumnName"]);
		}

		#region Implementation

		protected override ImportWizardPreviewLineCollection GetCollectionToTest()
		{
			var lists = new Dictionary<string, IList>();
			lists.Add("DIW_G", listG);
			lists.Add("DIW_S", listS);
			return new ImportWizardPreviewLineCollection(PropertyInfos, lists);
		}

		IEnumerable<IImportPropertyInfo> PropertyInfos
		{
			get { return propertyInfos ?? (propertyInfos = GetPropertyInfos()); }
		}
		IImportPropertyInfo[] propertyInfos;

		IImportPropertyInfo[] GetPropertyInfos()
		{
			return new IImportPropertyInfo[]
			{
				new PropertyInfo<ZGuid>("DIW_G"),
				new PropertyInfo<ZString>("DIW_S") { IsMulti = true },
				new PropertyInfo<ZDecimal>("DIW_M"),
				new PropertyInfo<ZDateTime>("DIW_D"),
				new PropertyInfo<ZDateTimeOffset>("DIW_O"),
				new PropertyInfo<ZInt>("DIW_I"),
				new PropertyInfo<ZBool>("DIW_B"),
			};
		}

		class PropertyInfo<T> : IImportPropertyInfo
		{
			public PropertyInfo(string name)
			{
				MappingName = name;
			}

			public string MappingName { get; private set; }
			public Type PropertyType { get { return typeof(T); } }
			public Type ComponentType { get { return typeof(T); } }
			public string HeaderText { get { return MappingName; } }
			public int ColumnWidth { get { return 80; } }
			public bool IsMandatory => false;
			public IList GetBindToList(BusinessObject bizObj) { return null; }
			public Modules.ModuleIdentifier GetModuleID(BusinessObject bizObj) { return null; }
			public bool IsMultiControl => IsMulti;
			public Type GetExpectedTypeForMultiControl(BusinessObject bizObj) { return null; }
			public bool IsReadOnly { get { return false; } }
			public ZCharacterCasing CharacterCasing { get { return ZCharacterCasing.Normal; } }
			public string FieldTypeColumnName => "TestFieldTypeColumnName";
			public bool IsMulti { get; set; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ImportWizardPreviewLine(Properties);
		}

		IEnumerable<Tuple<Type, string, DynamicMetaData[]>> Properties
		{
			get { return properties ?? (properties = GetProperties()); }
		}
		Tuple<Type, string, DynamicMetaData[]>[] properties;

		readonly IList listG = Array.Empty<object>();
		readonly IList listS = Array.Empty<object>();

		Tuple<Type, string, DynamicMetaData[]>[] GetProperties()
		{
			return new Tuple<Type, string, DynamicMetaData[]>[]
			{
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZGuid), "DIW_G", new DynamicMetaData[] { DynamicMetaData.ListDataSource(listG) }),
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZString), "DIW_S", new DynamicMetaData[] { DynamicMetaData.ListDataSource(listS) }),
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZDecimal), "DIW_M", Array.Empty<DynamicMetaData>()),
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZDateTime), "DIW_D", Array.Empty<DynamicMetaData>()),
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZDateTimeOffset), "DIW_O", Array.Empty<DynamicMetaData>()),
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZInt), "DIW_I", Array.Empty<DynamicMetaData>()),
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZBool), "DIW_B", Array.Empty<DynamicMetaData>()),
			};
		}
		#endregion
	}
}
