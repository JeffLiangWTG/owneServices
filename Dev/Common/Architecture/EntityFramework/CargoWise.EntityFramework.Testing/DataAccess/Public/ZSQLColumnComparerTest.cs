using System;
using System.Data;
using System.Linq;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSQLColumnComparerTest : TestCase
	{
		#region Constructor

		public void TestConstructorThrowsAppropriateExceptions()
		{
			var column1 = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column1", 0, SqlDbType.VarChar, null, true, 10);
			var column2 = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column2", 0, SqlDbType.VarChar, null, true, 10);

			AssertExceptionThrown("null column should throw exception", typeof(ArgumentNullException), delegate
			{ new ZSQLColumnComparer(null, SQLComparisonOperator.Equal, column2); });
			AssertExceptionThrown("null operator should throw exception", typeof(ArgumentNullException), delegate
			{ new ZSQLColumnComparer(column1, null, column2); });
			AssertExceptionThrown("null with column should throw exception", typeof(ArgumentNullException), delegate
			{ new ZSQLColumnComparer(column1, SQLComparisonOperator.Equal, null); });

			AssertExceptionThrown("invalid operator should throw exception", typeof(ArgumentException), delegate
			{ new ZSQLColumnComparer(column1, SQLComparisonOperator.EqualToDatePartOnly, column2); });
			AssertExceptionThrown("invalid operator should throw exception", typeof(ArgumentException), delegate
			{ new ZSQLColumnComparer(column1, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, column2); });
			AssertExceptionThrown("invalid operator should throw exception", typeof(ArgumentException), delegate
			{ new ZSQLColumnComparer(column1, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, column2); });
		}

		#endregion

		#region IFilterPart Members

		public void TestGetSimplifiedVersion()
		{
			IFilterPart[] array = IFilterPart.GetSimplifiedVersion(null);
			AssertCollectionContains(IFilterPart, array);
		}

		[ExpectNoExceptions()]
		public void TestDisableModifications()
		{
			IFilterPart.DisableModifications();
		}

		public void TestDeepClone()
		{
			IFilterPart clone = IFilterPart.DeepClone();
			AssertEquals(clone.LiteralTextADO, IFilterPart.LiteralTextADO);
		}

		public void TestParameterisedSql()
		{
			AssertEquals("Column1 > Column2", IFilterPart.ParameterisedSql((ParameterNameFactory)null).ParameterisedQueryText);
		}

		public void TestLiteralTextADO()
		{
			AssertEquals("Column1 > Column2", IFilterPart.LiteralTextADO);
		}

		public void TestNeedsBrackets()
		{
			AssertEquals(false, IFilterPart.NeedsBrackets);
		}

		public void TestFilterIsEmpty()
		{
			AssertEquals(false, IFilterPart.FilterIsEmpty);
		}

		public void TestContainsOrOperator()
		{
			AssertEquals(false, IFilterPart.ContainsOrOperator);
		}

		public void TestBlobFilters()
		{
			AssertEquals(0, IFilterPart.BlobFilters.Count());
		}

		public void TestHasParameters()
		{
			AssertEquals(false, IFilterPart.HasParameters);
		}

		#endregion

		#region Equals / GetHashCode

		public void TestEquals()
		{
			AssertEquals(
				"Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
			AssertNotEquals(
				"Not Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Code),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
			AssertNotEquals(
				"Not Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
			AssertNotEquals(
				"Not Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThan, DummyBizoSchema.Z0_Description),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
		}

		#endregion

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var sqlColumnComparer = new ZSQLColumnComparer(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description);
			AssertEquals(sqlColumnComparer.HasComparisonOperatorLike, false);
		}

		#endregion

		#region Implementation

		IFilterPart IFilterPart
		{
			get { return FilterPart; }
		}

		ZSQLColumnComparer filterPart;
		internal ZSQLColumnComparer FilterPart
		{
			get
			{
				if (filterPart == null)
				{
					var column1 = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column1", 0, SqlDbType.VarChar, null, true, 10);
					var column2 = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column2", 0, SqlDbType.VarChar, null, true, 10);
					filterPart = new ZSQLColumnComparer(column1, SQLComparisonOperator.GreaterThan, column2);
				}
				return filterPart;
			}
		}

		#endregion
	}
}
