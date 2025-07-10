using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ImportWizardPreviewLine))]
	sealed class ImportWizardPreviewLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDynamicFields()
		{
			var today = ZDateTime.Today;
			var todayOffset = ZDateTimeOffset.Today;

			AssertEquals(ZString.Empty, Obj["DIW_S"]);
			AssertEquals(ZDecimal.Zero, Obj["DIW_M"]);
			AssertEquals(ZDateTime.Empty, Obj["DIW_D"]);
			AssertEquals(ZDateTimeOffset.Empty, Obj["DIW_O"]);
			AssertEquals(ZInt.Zero, Obj["DIW_I"]);
			AssertEquals(ZBool.False, Obj["DIW_B"]);

			Guid guid1 = Guid.NewGuid();
			Obj["DIW_G"] = guid1;
			Obj["DIW_S"] = "test1";
			Obj["DIW_M"] = 1.1m;
			Obj["DIW_D"] = today;
			Obj["DIW_O"] = todayOffset;
			Obj["DIW_I"] = 1;

			AssertEquals(guid1, Obj["DIW_G"]);
			AssertEquals("test1", Obj["DIW_S"]);
			AssertEquals(1.1m, Obj["DIW_M"]);
			AssertEquals(today, Obj["DIW_D"]);
			AssertEquals(todayOffset, Obj["DIW_O"]);
			AssertEquals(1, Obj["DIW_I"]);

			ZGuid guid2 = ZGuid.NewZGuid();
			Obj["DIW_G"] = guid2;
			Obj["DIW_S"] = (ZString)"test2";
			Obj["DIW_M"] = (ZDecimal)2.2m;
			Obj["DIW_D"] = today.AddDays(1);
			Obj["DIW_O"] = todayOffset.AddDays(1);
			Obj["DIW_I"] = (ZInt)2;
			Obj["DIW_B"] = ZBool.True;

			AssertEquals(guid2, Obj["DIW_G"]);
			AssertEquals("test2", Obj["DIW_S"]);
			AssertEquals(2.2m, Obj["DIW_M"]);
			AssertEquals(today.AddDays(1), Obj["DIW_D"]);
			AssertEquals(todayOffset.AddDays(1), Obj["DIW_O"]);
			AssertEquals(2, Obj["DIW_I"]);
			AssertEquals(true, Obj["DIW_B"]);
		}

		public void TestPropertyDescriptors()
		{
			BusinessObjectPropertyDescriptorCollection properties = (BusinessObjectPropertyDescriptorCollection)TypeDescriptor.GetProperties(Obj);
			AssertNotNull(properties["DIW_G"]);
			AssertNotNull(properties["DIW_S"]);
			AssertNotNull(properties["DIW_M"]);
			AssertNotNull(properties["DIW_D"]);
			AssertNotNull(properties["DIW_O"]);
			AssertNotNull(properties["DIW_I"]);
			AssertNotNull(properties["DIW_B"]);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportWizardPreviewLine(Properties);
		}

		ImportWizardPreviewLine Obj
		{
			get
			{
				if (obj == null)
				{
					obj = (ImportWizardPreviewLine)GetNewBusinessObject();
				}

				return obj;
			}
		}
		ImportWizardPreviewLine obj;

		IEnumerable<Tuple<Type, string, DynamicMetaData[]>> Properties
		{
			get { return properties ?? (properties = GetPropertyInfos()); }
		}
		Tuple<Type, string, DynamicMetaData[]>[] properties;

		Tuple<Type, string, DynamicMetaData[]>[] GetPropertyInfos()
		{
			return new Tuple<Type, string, DynamicMetaData[]>[]
			{
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZGuid), "DIW_G", Array.Empty<DynamicMetaData>()),
				new Tuple<Type, string, DynamicMetaData[]>(typeof(ZString), "DIW_S", new DynamicMetaData[] { DynamicMetaData.MaxLength(Int32.MaxValue) }),
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
