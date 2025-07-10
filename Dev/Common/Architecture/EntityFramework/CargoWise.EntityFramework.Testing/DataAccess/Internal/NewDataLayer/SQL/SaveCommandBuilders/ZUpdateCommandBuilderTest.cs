using System;
using System.Data;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZUpdateCommandBuilderTest : ZSqlCommandBuilderTest
	{
		// Look at other tests in C:\Dev\Common\Architecture\EntityFramework.Testing\DataAccess\ZSqlSaverTest.cs

		public override void TestWithDummyRow()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessObject>();
			factory.Save();
			var description = "something else";
			bizO.Z0_Description = description;

			factory.Save();
			AssertEquals(0, Db.Connection.ExecuteScalar("select count(*) from dbo.DummyBizO where Z0_Description = @desc", cmd => cmd.AddParameterBasedOnDbColumn("@desc", "Original", DummyBizoSchema.Z0_Description)));
			AssertEquals(1, Db.Connection.ExecuteScalar("select count(*) from dbo.DummyBizO where Z0_Description = @desc", cmd => cmd.AddParameterBasedOnDbColumn("@desc", description, DummyBizoSchema.Z0_Description)));
		}

		protected override DataRow CreateDummyRow()
		{
			var row = base.CreateDummyRow();
			row.AcceptChanges();
			return row;
		}

		protected override ZSqlCommandBuilder GetBuilder(DataRow row)
		{
			return new ZUpdateCommandBuilder(row, "db.dbo.DummyBizo", true, 1, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(DummyBizoSchema.Constants.TableName));
		}

		public void TestIgnoreConcurrencyCheck()
		{
			var dummy = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			dummy.Z0_Number = 123;
			((ILightValidationInternals)dummy).IsValid = true;
			dummy.Factory.Save();

			var otherDummy = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK);
			otherDummy.Z0_Number = 777;
			otherDummy.Factory.Save();

			((ILightValidationInternals)dummy).IsValid = false;
			dummy.Z0_Code = "XYZ";
			ConcurrencyInfo.SetConcurrencyPolicy(dummy.Row, "Z0_Number", ConcurrencyPolicy.Strict);

			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(dummy.TableName);
			var stringBuilder = new StringBuilder();
			new ZUpdateCommandBuilder(dummy.Row, dummy.TableName, false, 1, schema).AppendCommandTextAndBlobSaver(stringBuilder);
			Assert("Should contain concurrency check", stringBuilder.ToString().Contains("\tAND Z0_Number = @"));

			AssertExceptionThrown(typeof(ZSaveConcurrencyException), () => dummy.Factory.Save());

			dummy.CancelChanges();
			((ILightValidationInternals)dummy).IsValid = false;
			ConcurrencyInfo.SetConcurrencyPolicy(dummy.Row, "Z0_Number", ConcurrencyPolicy.Strict);

			stringBuilder = new StringBuilder();
			new ZUpdateCommandBuilder(dummy.Row, dummy.TableName, false, 1, schema).AppendCommandTextAndBlobSaver(stringBuilder);
			var index = stringBuilder.ToString().IndexOf("\tAND Z0_PK = @");
			Assert("Should contain PK check", index > 0);
			Assert("Should not contain concurrency checks - no AND keyword", stringBuilder.ToString().IndexOf("\tAND ", index + 4) == -1);

			AssertNoExceptionThrown(() => dummy.Factory.Save());
		}

		/// <summary>
		/// Refer to CS00597654.
		/// </summary>
		public void TestConcurrencyCheckForLargeTextShouldBeFineWithVarcharWithSomeSpecialCharacters()
		{
			var dummy1 = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy1.Z0_Code = "ABC";
			dummy1.Z0_VarCharMax = "è123456°";
			((ILightValidationInternals)dummy1).IsValid = true;
			dummy1.Factory.Save();

			dummy1.Z0_VarCharMax = "123456";
			ConcurrencyInfo.SetConcurrencyPolicy(dummy1.Row, "Z0_VarCharMax", ConcurrencyPolicy.Strict);

			AssertNoExceptionThrown(() => dummy1.Factory.Save());
		}

		public void TestConcurrencyCheck_IsValid_False_OnlyChange()
		{
			var dummy = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			dummy.Z0_Number = 123;
			((ILightValidationInternals)dummy).IsValid = true;
			dummy.Factory.Save();

			((ILightValidationInternals)dummy).IsValid = false;

			var stringBuilder = new StringBuilder();
			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(dummy.TableName);

			dummy.OnSaving(); // To ensure correct concurrency is applied to column IsValid in method EnsureLightValidationIsIgnoredForConcurrency()

			var updateBuilder = new ZUpdateCommandBuilder(dummy.Row, dummy.TableName, false, 1, schema);
			updateBuilder.AppendCommandTextAndBlobSaver(stringBuilder);
			var updateCommand = stringBuilder.ToString();

			AssertContains("Should change column value", "Z0_IsValid = @", stringBuilder.ToString(), true);
			AssertNotContainsColumnConcurrencyCheck("Z0_IsValid", updateCommand);
			AssertNotContainsColumnConcurrencyCheck("Z0_Code", updateCommand);
			AssertNotContainsColumnConcurrencyCheck("Z0_Number", updateCommand);
		}

		public void TestConcurrencyCheck_IsValid_False_WithOtherChanges()
		{
			var dummy = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			dummy.Z0_Number = 123;
			((ILightValidationInternals)dummy).IsValid = true;
			dummy.Factory.Save();

			((ILightValidationInternals)dummy).IsValid = false;
			dummy.Z0_Code = "XYZ";

			var stringBuilder = new StringBuilder();
			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(dummy.TableName);

			dummy.OnSaving(); // To ensure correct concurrency is applied to column IsValid in method EnsureLightValidationIsIgnoredForConcurrency()

			var updateBuilder = new ZUpdateCommandBuilder(dummy.Row, dummy.TableName, false, 1, schema);
			updateBuilder.AppendCommandTextAndBlobSaver(stringBuilder);
			var updateCommand = stringBuilder.ToString();

			AssertContains("Should change column value", "Z0_IsValid = @", stringBuilder.ToString(), true);
			AssertNotContainsColumnConcurrencyCheck("Z0_IsValid", updateCommand);
			AssertContainsColumnConcurrencyCheck("Z0_Code", updateCommand);
			AssertContainsColumnConcurrencyCheck("Z0_Number", updateCommand);
		}

		public void TestConcurrencyCheck_IsValid_True()
		{
			var dummy = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			dummy.Z0_Number = 123;
			((ILightValidationInternals)dummy).IsValid = false;
			dummy.Factory.Save();

			((ILightValidationInternals)dummy).IsValid = true;

			var stringBuilder = new StringBuilder();
			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(dummy.TableName);

			dummy.OnSaving(); // To ensure correct concurrency is applied to column IsValid in method EnsureLightValidationIsIgnoredForConcurrency()

			var updateBuilder = new ZUpdateCommandBuilder(dummy.Row, dummy.TableName, false, 1, schema);
			updateBuilder.AppendCommandTextAndBlobSaver(stringBuilder);
			var updateCommand = stringBuilder.ToString();

			AssertContains("Should change column value", "Z0_IsValid = @", stringBuilder.ToString(), true);
			AssertContainsColumnConcurrencyCheck("Z0_IsValid", updateCommand);
			AssertContainsColumnConcurrencyCheck("Z0_Code", updateCommand);
			AssertContainsColumnConcurrencyCheck("Z0_Number", updateCommand);
		}

		void AssertContainsColumnConcurrencyCheck(string columnName, string sql)
		{
			var expression1 = $"AND {columnName} = @";
			var expression2 = $"AND ({columnName} = @"; // e.g. AND (Z0_IsValid = @15 OR Z0_IsValid = @16)

			var index1 = sql.IndexOf(expression1, StringComparison.OrdinalIgnoreCase);
			var index2 = sql.IndexOf(expression2, StringComparison.OrdinalIgnoreCase);

			if (index1 < 0 && index2 < 0)
			{
				Fail($"Expected to have {columnName} in concurrency check. Full sql expression:\r\n{sql}");
			}

			Assert(true);
		}

		void AssertNotContainsColumnConcurrencyCheck(string columnName, string sql)
		{
			var expression1 = $"AND {columnName} = @";
			var expression2 = $"AND ({columnName} = @"; // e.g. AND (Z0_IsValid = @15 OR Z0_IsValid = @16)

			var index1 = sql.IndexOf(expression1, StringComparison.OrdinalIgnoreCase);
			var index2 = sql.IndexOf(expression2, StringComparison.OrdinalIgnoreCase);

			if (index1 >= 0 || index2 >= 0)
			{
				Fail($"Expected not to have {columnName} in concurrency check. Full sql expression:\r\n{sql}");
			}

			Assert(true);
		}
	}
}
