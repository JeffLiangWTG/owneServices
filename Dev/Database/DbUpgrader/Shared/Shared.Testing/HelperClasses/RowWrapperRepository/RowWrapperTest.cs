using System;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class RowWrapperTest : TestCase
	{
		public void TestRowWrapper()
		{
			var repository = new RowWrapperRepository();
			var dummy = repository.New(DummyBizoSchema.Instance);

			AssertNotEquals(Guid.Empty, dummy.PK);

			AssertExceptionThrown("PK column [Z0_PK] should be initialized via constructor", typeof(InvalidOperationException), () => dummy[DummyBizoSchema.PK] = Guid.NewGuid());
			AssertExceptionThrown("[DummyBizo.Z0_Code] cannot get value that has not been set", typeof(InvalidOperationException), () => { var code = dummy[DummyBizoSchema.Z0_Code]; });
			AssertExceptionThrown("column [ZDP_AddInfo] does not belong to [DummyBizo]", typeof(InvalidOperationException), () => dummy[DummyPivotSchema.ZDP_AddInfo] = 6);
			AssertExceptionThrown("value [6] set for [Z0_Code] is not a valid String", typeof(InvalidOperationException), () => dummy[DummyBizoSchema.Z0_Code] = 6);
			AssertExceptionThrown("value [null] set for [Z0_Bool] is not a valid Boolean", typeof(InvalidOperationException), () => dummy[DummyBizoSchema.Z0_Bool] = null);

			dummy[DummyBizoSchema.Z0_Code] = "AAA";
			dummy[DummyBizoSchema.Z0_Bool] = true;
			dummy[DummyBizoSchema.Z0_Date] = new DateTime(2011, 12, 1);
			dummy[DummyBizoSchema.Z0_Decimal] = 10.3m;
			dummy[DummyBizoSchema.Z0_Number] = 23;
			dummy[DummyBizoSchema.Z0_Description] = null;

			AssertEquals("AAA", dummy[DummyBizoSchema.Z0_Code]);
			AssertEquals(true, dummy[DummyBizoSchema.Z0_Bool]);
			AssertEquals(new DateTime(2011, 12, 1), dummy[DummyBizoSchema.Z0_Date]);
			AssertEquals(10.3m, dummy[DummyBizoSchema.Z0_Decimal]);
			AssertEquals(23, dummy[DummyBizoSchema.Z0_Number]);
			AssertEquals(null, dummy[DummyBizoSchema.Z0_Description]);
		}

		public void TestRowWrapper_PK()
		{
			var dummy = new RowWrapper(DummyBizoSchema.Instance);

			AssertNotEquals(Guid.Empty, dummy.PK);

			var pk = Guid.NewGuid();

			dummy = new RowWrapper(DummyBizoSchema.Instance, pk);

			AssertEquals(pk, dummy.PK);
		}

		public void TestSchemaColumns()
		{
			var repository = new RowWrapperRepository();
			var dummy = repository.New(DummyBizoSchema.Instance);

			AssertContainsExactElementsInAnyOrder(new SchemaColumn[]
			{
				DummyBizoSchema.PK
			},
			dummy.SchemaColumns);

			dummy[DummyBizoSchema.Z0_Code] = "AAA";
			dummy[DummyBizoSchema.Z0_Bool] = true;

			AssertContainsExactElementsInAnyOrder(new SchemaColumn[]
			{
				DummyBizoSchema.PK,
				DummyBizoSchema.Z0_Code,
				DummyBizoSchema.Z0_Bool
			},
			dummy.SchemaColumns);
		}
	}
}
