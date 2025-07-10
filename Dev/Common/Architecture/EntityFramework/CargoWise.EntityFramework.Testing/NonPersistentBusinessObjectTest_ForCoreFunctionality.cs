using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class NonPersistentBusinessObjectTest_ForCoreFunctionality : TestCaseWithFactory
	{
		#region TestUnsupportedMembersFromBaseHiddenFromMisuse

		public void TestUnsupportedMembersFromBaseHiddenFromMisuse()
		{
			string[] unsupportedMembersFromBase = new string[]
			{
				"CheckCanCopyPersistentValuesFrom",
				"ReloadCore"
			};

			foreach (string unsupportedMember in unsupportedMembersFromBase)
			{
				MemberInfo[] candidateMembers = typeof(NonPersistentBusinessObject).GetMember(unsupportedMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				AssertEquals("Expected member " + unsupportedMember + " to exist for this test", 1, candidateMembers.Length);
				AssertMemberIsHiddenFromIntellisenseAndSealed(candidateMembers[0]);
			}
		}

		void AssertMemberIsHiddenFromIntellisenseAndSealed(MemberInfo member)
		{
			AssertEquals(
				"Member " + member.Name + " is unsupported for a " + nameof(NonPersistentBusinessObject) + " and should not be visible to intellisense, or compilable",
				true, IsMemberObsoleteAndHiddenFromIntellisense(member));

			MethodInfo method = member as MethodInfo;
			PropertyInfo property = member as PropertyInfo;
			if (property != null)
			{
				AssertEquals("Property " + property.Name + " should be marked as sealed", true, property.GetGetMethod(true).IsFinal);
			}
			else if (method != null)
			{
				AssertEquals("Method " + method.Name + " should be marked as sealed", true, method.IsFinal);
			}
		}

		bool IsMemberObsoleteAndHiddenFromIntellisense(MemberInfo member)
		{
			return IsMemberObsolete(member) && IsMemberHiddenFromIntellisense(member);
		}

		bool IsMemberHiddenFromIntellisense(MemberInfo member)
		{
			EditorBrowsableAttribute[] editorBrowsableAttributes = (EditorBrowsableAttribute[])member.GetCustomAttributes(typeof(EditorBrowsableAttribute), false);
			return (editorBrowsableAttributes.Length > 0 && editorBrowsableAttributes[0].State == EditorBrowsableState.Never);
		}

		bool IsMemberObsolete(MemberInfo member)
		{
			ObsoleteAttribute[] obsoleteAttributes = (ObsoleteAttribute[])member.GetCustomAttributes(typeof(ObsoleteAttribute), false);
			return obsoleteAttributes.Length > 0 && obsoleteAttributes[0].IsError;
		}

		#endregion

		public void TestName()
		{
			AssertEquals("No table so use default", "record", new DummyNonPersistentBusinessObject(Factory).HumanReadableName);
		}

		public void TestPKOverriding()
		{
			DummyNonPersistentBusinessObject bizO = new DummyNonPersistentBusinessObject();
			bizO.PKOverride = ZGuid.NewZGuid();
			AssertEquals("GetPK should be overriding the default PK implementation", bizO.PKOverride, bizO.PK);
		}

		public void TestResetState()
		{
			var obj = new DummyNonPersistentBusinessObject();
			AssertEquals("Initialization. IsDeleted false", false, obj.IsDeleted);
			obj.Delete();
			AssertEquals("After Delete. IsDeleted true", true, obj.IsDeleted);

			obj.ResetState();

			AssertEquals("Reset State. IsDeleted false", false, obj.IsDeleted);
		}

		public void TestIsInDatabase()
		{
			DummyNonPersistentBusinessObject dummy = new DummyNonPersistentBusinessObject(Factory);
			AssertEquals("IsInDatabase should always return false", false, dummy.IsInDatabase);
		}

		public void TestIsNotExcludedFromSavingByFactory()
		{
			DummyNonPersistentBusinessObject dummy = new DummyNonPersistentBusinessObject(Factory);
			AssertEquals("IsNotExcludedFromSavingByFactory should always return true", true, dummy.IsNotExcludedFromSavingByFactory);
		}

		public void TestIsSavedByFactory()
		{
			DummyNonPersistentBusinessObject dummy = new DummyNonPersistentBusinessObject(Factory);
			AssertEquals("IsSavedByFactory should always return false", false, dummy.IsSavedByFactory);
		}

		public void TestCanBeCreatedWithNoRowOrFactory()
		{
			DummyNonPersistentBusinessObject bizO = new DummyNonPersistentBusinessObject();
			Assert("PK should be set normally to a random new guid", bizO.PK.IsValid);
			AssertEquals("Dummy.TableName", "", bizO.TableName);
		}

		public void TestCanBeCreatedWithJustAFactory()
		{
			DummyNonPersistentBusinessObject bizO = new DummyNonPersistentBusinessObject(Factory);
			Assert("PK should be set normally to a random new guid", bizO.PK.IsValid);
			AssertEquals("Dummy.TableName", "", bizO.TableName);
		}

		public void TestCanBeCreatedWithARowAndFactory()
		{
			DummyNonPersistentBusinessObjectWithRow bizO = new DummyNonPersistentBusinessObjectWithRow(Factory);
			AssertEquals("IsInDatabase should always return false", false, bizO.IsInDatabase);
			AssertEquals("IsSavedByFactory should always return false", false, bizO.IsSavedByFactory);
			AssertEquals("BizO.TableName", "DummyNonPersistentBusinessObjectWithRow", bizO.TableName);

			AssertEquals("Deafult: BizO.Z0_String", "", bizO.Z0_String);
			bizO.Z0_String = "My String";
			AssertEquals("After Setting: BizO.Z0_String", "My String", bizO.Z0_String);

			AssertEquals("initial OnSavedCount", 0, bizO.OnSavedCount);
			AssertEquals("initial OnSavingCount", 0, bizO.OnSavingCount);
			AssertEquals("initial OnFactorySavedCount", 0, bizO.OnFactorySavedCount);
			AssertEquals("initial OnFactorySavingCount", 0, bizO.OnFactorySavingCount);

			Factory.Save();

			AssertEquals("after 1 save OnSavedCount", 0, bizO.OnSavedCount);
			AssertEquals("after 1 save OnSavingCount", 0, bizO.OnSavingCount);
			AssertEquals("after 1 save OnFactorySavedCount", 1, bizO.OnFactorySavedCount);
			AssertEquals("after 1 save OnFactorySavingCount", 1, bizO.OnFactorySavingCount);

			AssertEquals("IsInDatabase should always return false", false, bizO.IsInDatabase);
			AssertEquals("IsSavedByFactory should always return false", false, bizO.IsSavedByFactory);
		}

		public void TestFactorySaving()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyNonPersistentBusinessObject dummy = new DummyNonPersistentBusinessObject(factory);

			AssertEquals("initial state", 0, dummy.OnSavedCount);
			AssertEquals("initial state", 0, dummy.OnSavingCount);
			AssertEquals("initial state", 0, dummy.OnFactorySavedCount);
			AssertEquals("initial state", 0, dummy.OnFactorySavingCount);

			factory.Save();

			AssertEquals("after 1 save", 0, dummy.OnSavedCount);
			AssertEquals("after 1 save", 0, dummy.OnSavingCount);
			AssertEquals("after 1 save", 1, dummy.OnFactorySavedCount);
			AssertEquals("after 1 save", 1, dummy.OnFactorySavingCount);

			factory.Save();

			AssertEquals("after 2 saves", 0, dummy.OnSavedCount);
			AssertEquals("after 2 saves", 0, dummy.OnSavingCount);
			AssertEquals("after 2 saves", 2, dummy.OnFactorySavedCount);
			AssertEquals("after 2 saves", 2, dummy.OnFactorySavingCount);
		}

		[ExpectNoExceptions]
		public void TestFactoryLoad()
		{
			AssertNull(Factory.Load(typeof(DummyNonPersistentBusinessObject), Guid.NewGuid()));
			var existingObj = new DummyNonPersistentBusinessObject(Factory);
			AssertEquals(existingObj, Factory.Load(typeof(DummyNonPersistentBusinessObject), existingObj.PK));
		}

		[ExpectNoExceptions]
		public void TestDeleteWhenFactoryIsNull()
		{
			var dummy = new DummyNonPersistentBusinessObject();
			((IBusiness)dummy).DeleteForDataRefresh();
		}

		#region TestMatchFilter

		public void TestMatchesFilter()
		{
			var dummy1 = new DummyNonPersistentBizoWithProperties_WithAndWithoutPropertyInfos { Number = 123, Text = "ABCDE" };
			var dummy2 = new DummyNonPersistentBizoWithProperties_WithAndWithoutPropertyInfos { Number = 0, Text = "QWERTY" };

			var numberSchemaColumn = new SchemaIntColumn(CargoWise.Schema.Schema.GenericTableSchema, "Number", 0, 0, false);
			var textSchemaColumn = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Text", 0, SqlDbType.NVarChar, "", false, 16);

			var query = new ZQuery();
			query.AddToFilter(numberSchemaColumn, 123);
			query.AddToFilter(textSchemaColumn, "ABCDE");

			AssertExceptionThrown<InvalidOperationException>(() => dummy1.MatchesFilter(query));

			AssertMatchesFilter(dummy1, query, true);
			AssertMatchesFilter(dummy2, query, false);

			query = new ZQuery();
			query.AddToFilter(numberSchemaColumn, 0);
			query.AddToFilter(textSchemaColumn, "QWERTY");

			AssertMatchesFilter(dummy1, query, false);
			AssertMatchesFilter(dummy2, query, true);

			query = new ZQuery(numberSchemaColumn, SQLComparisonOperator.GreaterThanOrEqualTo, 0);
			AssertMatchesFilter(dummy1, query, true);
			AssertMatchesFilter(dummy2, query, true);

			query = new ZQuery(numberSchemaColumn, SQLComparisonOperator.LessThanOrEqualTo, 0);
			AssertMatchesFilter(dummy1, query, false);
			AssertMatchesFilter(dummy2, query, true);

			query = new ZQuery(textSchemaColumn, SQLComparisonOperator.StartsWith, "AB");
			AssertMatchesFilter(dummy1, query, true);
			AssertMatchesFilter(dummy2, query, false);

			query = new ZQuery(textSchemaColumn, SQLComparisonOperator.EndsWith, "TY");
			AssertMatchesFilter(dummy1, query, false);
			AssertMatchesFilter(dummy2, query, true);

			query = new ZQuery(textSchemaColumn, SQLComparisonOperator.Contains, "C");
			AssertMatchesFilter(dummy1, query, true);
			AssertMatchesFilter(dummy2, query, false);

			query = new ZQuery(textSchemaColumn, SQLComparisonOperator.NotContains, "X");
			AssertMatchesFilter(dummy1, query, true);
			AssertMatchesFilter(dummy2, query, true);
		}

		void AssertMatchesFilter(BusinessObject bizo, ZQuery query, bool matches)
		{
			((ISupportMainElement)query).SetMainElement(bizo);
			AssertEquals(matches, bizo.MatchesFilter(query));
		}

		class DummyNonPersistentBizoWithProperties_WithAndWithoutPropertyInfos : NonPersistentBusinessObject
		{
			public ZInt Number { get; set; }

			public ZString Text { get; set; }
			public ZPropertyInfo TextInfo { get { return GetZPropertyInfo(nameof(Text)); } }
		}

		public void TestMatchesFilter_DBOnlyQuery_GenericPKColumn()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			Factory.Save();

			var dummyWrapper = new DummyNonPersistentBusinessObjectWrapper(dummy);

			var query = new ZDBOnlyQuery(typeof(DummyNonPersistentBusinessObjectWrapper));
			query.AddToFilter(DummyBizoSchema.Z0_Code, "ABC");

			AssertExceptionThrown<InvalidOperationException>("Should throw exception",
				"Cannot match db-only filter with business object of type " + dummyWrapper.GetType().FullName + " with non-persistent PK schema column.",
				() => dummyWrapper.MatchesFilter(query));
		}

		public void TestMatchesFilter_DBOnlyQuery_PersistentPKColumn()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			Factory.Save();

			var dummyWrapper = new DummyNonPersistentBusinessObjectWrapperWithPkColumn(dummy);

			var query = new ZDBOnlyQuery(typeof(DummyNonPersistentBusinessObjectWrapperWithPkColumn));
			query.AddToFilter(DummyBizoSchema.Z0_Code, "ABC");

			Assert("Should match to filter", dummyWrapper.MatchesFilter(query));
		}

		class DummyNonPersistentBusinessObjectWrapper : NonPersistentBusinessObject
		{
			public class Schema
			{
				public const string TableName = DummyBusinessObject.Schema.TableName;
			}

			public DummyNonPersistentBusinessObjectWrapper(DummyBusinessObject dummy)
				: base(dummy.Factory)
			{
				this.dummy = dummy;
				using (((ISingleElementListInternal)this).SuspendListChanged())
				{
					base.AddToFactoryCache();
				}
			}

			protected readonly DummyBusinessObject dummy;

			protected override void AddToFactoryCache()
			{
				// should be called after reconWrappedJobDeclaration is set
			}

			protected override ZGuid GetPK() => dummy.PK;
		}

		class DummyNonPersistentBusinessObjectWrapperWithPkColumn : DummyNonPersistentBusinessObjectWrapper
		{
			public DummyNonPersistentBusinessObjectWrapperWithPkColumn(DummyBusinessObject dummy)
				: base(dummy)
			{
			}

			public override SchemaGuidColumn PKSchemaColumn => dummy.PKSchemaColumn;
		}

		#endregion

		#region TestNonPersistentRow

		public void TestNonPersistentRow()
		{
			var dummy1 = new DummyNonPersistentBusinessObject();
			AssertNull(dummy1.Row);
			AssertNull(dummy1.Table);
			AssertEquals("", dummy1.TableName);
			AssertNotNull(dummy1.NonPersistentRow);
			AssertNull("Row should not be initialized by NonPersistentRow", dummy1.Row);
			AssertNull("Table should not be initialized by NonPersistentRow", dummy1.Table);
			AssertEquals("TableName should not be initialized by NonPersistentRow", "", dummy1.TableName);
			var row1 = dummy1.NonPersistentRow;
			var row2 = dummy1.NonPersistentRow;
			AssertSame("Should only generate one row", row1, row2);

			var dummy2 = new DummyNonPersistentBusinessObjectWithRow(Factory);
			AssertNotNull(dummy2.Row);
			AssertNotNull(dummy2.Table);
			AssertEquals(dummy2.Table.TableName, dummy2.TableName);
			AssertNotNull(dummy2.NonPersistentRow);
			AssertSame("Should use same data row", dummy2.Row, dummy2.NonPersistentRow);
			AssertNotNull("Should remain", dummy2.Table);
			AssertEquals("Should remain", dummy2.Table.TableName, dummy2.TableName);

			var dummy3 = new DummyNonPersistentBizoWithProperties_WithAndWithoutPropertyInfos { Number = 123, Text = "ABC" };
			var row3 = dummy3.NonPersistentRow;
			AssertEquals(1, row3.Table.Columns.Count);
			AssertEquals(dummy3.PK, row3[dummy3.PKSchemaColumn.Name]);
		}

		#endregion

		#region DummyNonPersistentBusinessObject

		class DummyNonPersistentBusinessObject : NonPersistentBusinessObject
		{
			public DummyNonPersistentBusinessObject()
			{
			}

			public DummyNonPersistentBusinessObject(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZGuid PKOverride;

			protected override ZGuid GetPK()
			{
				return PKOverride.IsEmpty ? base.GetPK() : PKOverride;
			}

			public int OnSavingCount;
			public override void OnSaving()
			{
				OnSavingCount++;
				base.OnSaving();
			}

			public int OnSavedCount;
			public override void OnSaved(bool saveSucceeded)
			{
				OnSavedCount++;
				base.OnSaved(saveSucceeded);
			}

			public int OnFactorySavingCount;
			protected override void OnFactorySaving()
			{
				OnFactorySavingCount++;
				base.OnFactorySaving();
			}

			public int OnFactorySavedCount;
			protected override void OnFactorySaved(bool saveSucceeded)
			{
				OnFactorySavedCount++;
				base.OnFactorySaved(saveSucceeded);
			}
		}

		#endregion
	}
}
