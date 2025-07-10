using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleSQLFilter))]
	public sealed class ModuleSQLFilterTest : ModuleFilterTestCase<ModuleSQLFilter>
	{
		#region ReadOnly

		public void TestShallowCloneAndClearValues_ReadOnly()
		{
			var originalFilter = new ModuleSQLFilter("Original Filter", typeof(ModuleSQLFilter))
			{
				Property1 = "Original Property",
				ReadOnly = true
			};
			AssertEquals("GIVEN filter with ReadOnly", true, originalFilter.ReadOnly);

			var clonedFilter = (ModuleSQLFilter)originalFilter.ShallowCloneAndClearValues("Cloned Filter");
			AssertNotEquals("WHEN executed ShallowCloneAndClearValues", originalFilter, clonedFilter);
			AssertEquals("THEN the cloned-filter's description should be 'Cloned Filter'", "Cloned Filter", clonedFilter.Description);
			AssertEquals("THEN the cloned filter's property should be cleared", ZString.Empty, clonedFilter.Property1);
		}

		public void TestShallowCloneAndClearValues_NonReadOnly()
		{
			var originalFilter = new ModuleSQLFilter("Original Filter", typeof(ModuleSQLFilter))
			{
				Property1 = "Original Property",
				ReadOnly = false
			};
			AssertEquals("GIVEN filter with Non-ReadOnly", false, originalFilter.ReadOnly);

			var clonedFilter = (ModuleSQLFilter)originalFilter.ShallowCloneAndClearValues("Cloned Filter");
			AssertNotEquals("WHEN executed ShallowCloneAndClearValues", originalFilter, clonedFilter);
			AssertEquals("THEN the cloned-filter's description should be 'Cloned Filter'", "Cloned Filter", clonedFilter.Description);
			AssertEquals("THEN the cloned filter's property should be cleared", ZString.Empty, clonedFilter.Property1);
		}

		public void TestWhenNotUserInteractive_ShouldBeReadOnly()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;

			var sqlFilter = new ModuleSQLFilter("SQL Filter", typeof(ModuleSQLFilter));

			AssertEquals("GIVEN user with permission", true, EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed);
			Assert("AND is user interactive", Globals.IsUserInteractive);
			AssertEquals("THEN filter should not be ReadOnly", false, sqlFilter.ReadOnly);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				sqlFilter = new ModuleSQLFilter("SQL Filter", typeof(ModuleSQLFilter));

				AssertEquals("GIVEN user with permission", true, EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed);
				AssertEquals("AND is NOT user interactive", false, Globals.IsUserInteractive);
				AssertEquals("THEN filter should ReadOnly", true, sqlFilter.ReadOnly);
			}
		}

		#endregion

		public void TestPermission()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;
			var sqlFilter1 = new ModuleSQLFilter("SQL Filter 1", typeof(ModuleSQLFilter));
			AssertEquals("GIVEN user with permission", true, EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed);
			AssertEquals("THEN filter should not ReadOnly", false, sqlFilter1.ReadOnly);

			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = false;
			var sqlFilter2 = new ModuleSQLFilter("SQL Filter 1", typeof(ModuleSQLFilter));
			AssertEquals("GIVEN user with no permission", false, EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed);
			AssertEquals("THEN filter should be ReadOnly", true, sqlFilter2.ReadOnly);
		}

		public void TestQueryWithInlineComment()
		{
			var numberToFind = 1;
			var numberToNotFind = 2;

			Dummy1.Z0_Number = numberToFind;
			Dummy1.Z0_Code = "AH";
			Dummy2.Z0_Number = numberToNotFind;
			Dummy2.Z0_Code = "OH";
			Factory.Save();

			Filter.Property1 = "1 = 1 --";
			var query = Filter.Query;
			query.AddToFilter(DummyBizoSchema.Z0_Number, 1);
			var dummies = new DummyBusinessObjectCollection(Factory);
			dummies.Load(query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			Filter.Property1 = "Z0_Number = 1 --blah blah blah";
			dummies = new DummyBusinessObjectCollection(Factory);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			Filter.Clear();
			Filter.Property1 = "Z0_Code = 'AH' --blah blah blah";
			dummies = new DummyBusinessObjectCollection(Factory);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			Filter.Clear();
			Filter.Property1 = "Z0_Code = 'AH'; --blah blah blah";
			dummies = new DummyBusinessObjectCollection(Factory);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
		}

		public void TestQueryWithInvalidDataType()
		{
			var f = new ModuleSQLFilter("Apparently this is compulsory", typeof(StmALog))
			{
				Property1 = "SL_FireWorkflow = 'Y'" //querying with char when column is bit
			};
			f.Validation.ValidateAll();
			TestCaseWithFactory.AssertHasErrors(f.Property1Info);
		}

		#region TestProperty1Validation

		public void TestQueryTriesToUseTableName()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;
			Filter.Property1 = "DummyBizo.Z0_Code = 'GAS'";
			Filter.Validation.ValidateSqlOnFind();
			AssertHasError(Filter.Property1Info, "The SQL where clause you have typed is attempting refer to the queried table by name. Please only refer to columns.");

			Filter.Property1 = "Z0_Code = 'GAS'";
			Filter.Validation.ValidateAll();
			Filter.Validation.ValidateSqlOnFind();
			AssertNoError(Filter.Property1Info, "The SQL where clause you have typed is attempting refer to the queried table by name. Please only refer to columns.");
		}

		public void TestProperty1SqlValidationGetDate()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;
			var warningText = "Your statement includes the function 'GetDate()', which is likely to produce incorrect results as 'GetDate()' uses the date and time on the database server, and most date columns store UTC values. Please use 'GetUtcDate()' instead";
			Filter.Property1 = "Z0_Code = 'hi'";
			AssertNoWarning(Filter.Property1Info, warningText);

			Filter.Property1 = "GetDate()";
			AssertHasWarning(Filter.Property1Info, warningText);
		}

		public void TestQueryIsAccessingSensitiveData()
		{
			var errorText = "Custom SQL Filters cannot access sensitive information.";
			Filter.Property1 = "Password = 'a'";
			Filter.Validation.ValidateSqlOnFind();
			AssertNoError(Filter.Property1Info, errorText);

			var passwords = new string[] { "S6_Password", "QH_AuthorisingOfficerID", "GS_PasswordHash", "GP_CurrentPassword", "GS_Passwordhash", "/*comment*/GS_Passwordhash/*comment*/",
				"@@VERSION", "dbo.s6_passwOrd", "[GS_passwordhash]", "GlbStaff.[GS_PasswordHash]", "GlbStaff.GS_PasswordHash", "dbo.GlbStaff.[GS_PasswordHash]", "dbo.GlbStaff.GS_PasswordHash",
			"SERVERPROPERTY('ProductVersion')", };

			foreach (var password in passwords)
			{
				Filter.Property1 = password + " = ";
				Filter.Validation.ValidateSqlOnFind();
				AssertHasError(Filter.Property1Info, errorText);
			}

			Filter.Property1 = "Password = sys.dm_exec_sessions";
			Filter.Validation.ValidateSqlOnFind();
			AssertNoError(Filter.Property1Info, errorText);
		}

		public void TestProperty1ScalarFunction()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE FUNCTION [dbo].[Foo] (@Text varchar(128))
RETURNS int with schemabinding
AS
BEGIN
	DECLARE @notImportantTime datetimeoffset = GETDATE(); -- A statement to avoid inline function
    return 1;
END");
			Filter.Property1 = "dbo.Foo(Z0_Code) = '1'";
			SystemDataRegistry.Instance.AllowScalarFunctionsInCustomSql.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Filter.Validation.ValidateSqlOnFind();
			AssertHasError(Filter.Property1Info, "Scalar functions are not permitted to be called from filters due to their negative performance implications.");
		}

		public void TestProperty1ThrowSqlException()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "CDZ";
			Factory.Save();

			Filter.Property1 = "Z0_MyCode = 'CDZ'";
			SystemDataRegistry.Instance.AllowScalarFunctionsInCustomSql.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Filter.Validation.ValidateSqlOnFind();
			AssertHasError(Filter.Property1Info, @"Could not apply SQL filter due to the following errors:
Invalid column name 'Z0_MyCode'.");
		}

		#endregion

		#region TestProperty1SqlValidation

		public void TestProperty1ValidateSqlSyntaxOnlyOnFind()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;

			Filter.Property1 = "OH_PK is okay";

			Filter.Validation.ValidateProperty1();
			AssertNoErrors(Filter.Property1Info);

			Filter.Validation.ValidateSqlOnFind();
			var errorMessage = @"The Custom SQL Filter should be in the format of F1 = 'Value' AND F2 = 'Value 2'.
What you have typed is: OH_PK is okay
Error message: Incorrect syntax near 'okay'.
Filters: OH_PK is okay";
			AssertHasError(Filter.Property1Info, errorMessage);
			AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestConversionErrorsAreHidden()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "ASD";
			Factory.Save();

			Filter.Property1 = "convert(int, Z0_Code) > 0";

			Filter.Validation.ValidateProperty1();
			AssertNoErrors(Filter.Property1Info);

			Filter.Validation.ValidateSqlOnFind();
			var errorMessage = @"The Custom SQL Filter should be in the format of F1 = 'Value' AND F2 = 'Value 2'.
What you have typed is: convert(int, Z0_Code) > 0
Error message: Conversion failed.
Filters: convert(int, Z0_Code) > 0";
			AssertHasError(Filter.Property1Info, errorMessage);
			AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			var sql = "PK in blah";

			Filter.Property1 = sql;

			AssertEquals("Precondition", sql, Filter.Property1);

			Filter.Clear();
			AssertEquals(ZString.Empty, Filter.Property1);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			Filter.Property1 = "icecream!";
			AssertEquals(false, Filter.IsEmpty);
			Filter.Property1 = ZString.Empty;
			AssertEquals(true, Filter.IsEmpty);
		}

		#endregion

		#region Testing the Query results

		public void TestQueryWithWhereFilter()
		{
			var numberToFind = 1;
			var numberToNotFind = 2;

			Dummy1.Z0_Number = numberToFind;
			Dummy2.Z0_Number = numberToNotFind;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = "Z0_Number = 1";
			dummies.Load(Filter.Query);
			var lastQuery = SqlEventTracker.Instance.LastSqlQuery;
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
			Assert("The query should have 'REVERT WITH COOKIE' " + lastQuery, lastQuery.Contains("REVERT WITH COOKIE"));
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new ModuleSQLFilter("DummyDateFilter", typeof(DummyBusinessObject));

			var sqlValue = "1";
			filter.Property1 = sqlValue;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleSQLFilter)filterStripBizO[filter.Description];

			AssertEquals(sqlValue, loadedFilter.Property1);
		}

		public void TestFilterOnBLOB()
		{
			Filter.Property1 = "Z0_VarCharMax = 'this is a blob field' AND Z0_Code = 'hi'";
			Filter.Validation.ValidateSqlOnFind();

			AssertEquals("Blob inside filter must not cause error in ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			AssertEquals("Blob inside filter must not cause error in ErrorReporter", "", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Filter.Validation.ValidateProperty1();

			AssertEquals("Blob inside filter must not cause error in ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			AssertEquals("Blob inside filter must not cause error in ErrorReporter", "", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestIsValidSql
		public void TestIsValidSql()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Filter.Property1 = "same invalid sql";
			AssertEquals(false, Filter.IsValidSql(true));
			Assert("Silent Mode", UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals(false, Filter.IsValidSql(false));
			AssertContains("Non-Silent Mode", "The Custom SQL Filter should be in the format of F1 = 'Value' AND F2 = 'Value 2'.", UnitTestUserNotification.Instance.LastMessage.Text);

			Filter.Property1 = "1=1";
			AssertEquals(true, Filter.IsValidSql(true));
			AssertEquals(true, Filter.IsValidSql(false));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
		}

		DummyBusinessObject Dummy1;
		DummyBusinessObject Dummy2;

		protected override ModuleSQLFilter GetNewModuleFilter()
		{
			return new ModuleSQLFilter("moo", typeof(DummyBusinessObject));
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion
	}
}
