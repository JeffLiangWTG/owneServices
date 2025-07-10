using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnGuidWithListAttributeTest : ExcelExportColumnGuidTest
	{
		public void TestStringValueFromGuidProperty()
		{
			var bizoWithList = Factory.New<DummyBizOWithListAttribute>();
			bizoWithList.Z0_Guid = ZGuid.NewZGuid();

			var pair1 = new CodeElement(ZGuid.NewZGuid(), "Code1", "Description1");
			var pair2 = new CodeElement(bizoWithList.Z0_Guid, "Code2", "Description2");
			bizoWithList.List.Add(pair1);
			bizoWithList.List.Add(pair2);

			ZString expectedResult = "Code2";
			AssertFormattedResult(bizoWithList, expectedResult);
		}

		public void TestStringValueFromGuidPropertyWithNestedProperty()
		{
			var bizoWithNestedList = Factory.New<DummyBizOWithListAttributeWithNestedProperty>();
			bizoWithNestedList.Z0_Guid = ZGuid.NewZGuid();

			var pair1 = new CodeElement(ZGuid.NewZGuid(), "Code1", "Description1");
			var pair2 = new CodeElement(bizoWithNestedList.Z0_Guid, "Code2", "Description2");
			bizoWithNestedList.Lookups.List.Add(pair1);
			bizoWithNestedList.Lookups.List.Add(pair2);

			ZString expectedResult = "Code2";
			AssertFormattedResult(bizoWithNestedList, expectedResult);
		}

		public void TestStringValueFromGuidPropertyWithNestedPropertyAndMultiplePropertyMatches()
		{
			var bizoWithNestedListAndMultiplePropertyMatches = Factory.New<DummyBizOWithListAttributeWithNestedPropertyAndMultiplePropertiesWithSameNameUpTheHierarchy>();
			bizoWithNestedListAndMultiplePropertyMatches.Z0_Guid = ZGuid.NewZGuid();

			var pair1 = new CodeElement(ZGuid.NewZGuid(), "Code1", "Description1");
			var pair2 = new CodeElement(bizoWithNestedListAndMultiplePropertyMatches.Z0_Guid, "Code2", "Description2");
			bizoWithNestedListAndMultiplePropertyMatches.Lookups.List.Add(pair1);
			bizoWithNestedListAndMultiplePropertyMatches.Lookups.List.Add(pair2);

			ZString expectedResult = "Code2";
			AssertFormattedResult(bizoWithNestedListAndMultiplePropertyMatches, expectedResult);
		}

		public void TestStringValueFromGuidPropertyWithMissingPropertyReturnsNull()
		{
			var bizoWithMissingProperty = Factory.New<DummyBizOWithListAttributeForMissingProperty>();
			bizoWithMissingProperty.Z0_Guid = ZGuid.NewZGuid();

			AssertFormattedResult(bizoWithMissingProperty, null);
		}

		public void TestStringValueFromGuidPropertyWithNullRelatedObjectReturnsNull()
		{
			var bizoWithNullRelatedObject = Factory.New<DummyBizOWithListAttributeForNullRelatedObject>();
			bizoWithNullRelatedObject.Z0_Guid = ZGuid.NewZGuid();

			var column = new ExcelExportGuidColumn(new SchemaGuidColumn(DummyBizoSchema.Instance, "NullRelatedObject+Z0_Guid", 0, null, true));
			AssertFormattedResult(bizoWithNullRelatedObject, null, column);
		}

		protected override DummyBusinessObject GetBizObjForTest()
		{
			return bizO ?? (bizO = Factory.New<DummyBizOWithListAttribute>());
		}
		DummyBusinessObject bizO;

		#region Implementation

		class DummyBizOWithListAttribute : DummyBusinessObject
		{
			public DummyBizOWithListAttribute(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[List("List")]
			public override ZGuid Z0_Guid
			{
				get
				{
					return base.Z0_Guid;
				}
				set
				{
					base.Z0_Guid = value;
				}
			}

			internal CodeDescriptionPairList List
			{
				get
				{
					return list ?? (list = new CodeDescriptionPairList());
				}
			}
			CodeDescriptionPairList list;
		}

		class DummyBizOWithListAttributeWithNestedProperty : DummyBusinessObject
		{
			public DummyBizOWithListAttributeWithNestedProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[List("Lookups.List")]
			public override ZGuid Z0_Guid
			{
				get
				{
					return base.Z0_Guid;
				}
				set
				{
					base.Z0_Guid = value;
				}
			}

			public Lookups Lookups
			{
				get { return lookups ?? (lookups = new Lookups()); }
			}
			Lookups lookups;
		}

		class DummyBizOWithListAttributeWithNestedPropertyAndMultiplePropertiesWithSameNameUpTheHierarchy : DummyBizOWithListAttributeWithNestedProperty
		{
			public DummyBizOWithListAttributeWithNestedPropertyAndMultiplePropertiesWithSameNameUpTheHierarchy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new DifferentLookups Lookups
			{
				get { return lookups ?? (lookups = new DifferentLookups()); }
			}
			DifferentLookups lookups;
		}

		class DummyBizOWithListAttributeForMissingProperty : DummyBusinessObject
		{
			public DummyBizOWithListAttributeForMissingProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[List("SomeMissingList.Blah.Blah.Blah")]
			public override ZGuid Z0_Guid
			{
				get
				{
					return base.Z0_Guid;
				}
				set
				{
					base.Z0_Guid = value;
				}
			}
		}

		class DummyBizOWithListAttributeForNullRelatedObject : DummyBusinessObject
		{
			public DummyBizOWithListAttributeForNullRelatedObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyBizOWithListAttribute NullRelatedObject
			{
				get { return null; }
			}
		}

		class Lookups
		{
			internal CodeDescriptionPairList List
			{
				get
				{
					return list ?? (list = new CodeDescriptionPairList());
				}
			}
			CodeDescriptionPairList list;
		}

		class DifferentLookups
		{
			internal CodeDescriptionPairList List
			{
				get
				{
					return list ?? (list = new CodeDescriptionPairList());
				}
			}
			CodeDescriptionPairList list;
		}

		#endregion
	}
}
