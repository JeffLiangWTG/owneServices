using System;
using System.Globalization;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class RowWrapperRepositoryTest : TransactionedTestCase
	{
		public void TestActiveRowWrapper()
		{
			var a1 = new ActiveRowWrapper(DummyBizoSchema.Instance) { [DummyBizoSchema.Z0_Code] = "AAA" };
			var a2 = new ActiveRowWrapper(DummyBizoSchema.Instance) { [DummyBizoSchema.Z0_Code] = "BBB" };

			var d1 = new ActiveRowWrapper(DummyDependentBizoSchema.Instance)
			{
				[DummyDependentBizoSchema.ZD1_Z0] = a1.PK,
				[DummyDependentBizoSchema.ZD1_Number] = 45
			};

			AssertEquals(1, a1.Save());
			AssertEquals(1, a2.Save());
			AssertEquals(1, d1.Save());

			var repository = new RowWrapperRepository();

			var dummies = repository.Load(DummyBizoSchema.Instance, "SELECT * FROM dbo.DummyBizo");
			var dummy1 = dummies.FirstOrDefault(dummy => dummy.PK == a1.PK);
			var dummy2 = dummies.FirstOrDefault(dummy => dummy.PK == a2.PK);

			AssertNotNull(dummy1);
			AssertNotNull(dummy2);
			AssertEquals("AAA  ", dummy1[DummyBizoSchema.Z0_Code]);
			AssertEquals("BBB  ", dummy2[DummyBizoSchema.Z0_Code]);

			var dummyDependents = repository.Load(DummyDependentBizoSchema.Instance, string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.DummyDependentBizo WHERE ZD1_Z0 = '{0}'", a1.PK));

			AssertEquals(1, dummyDependents.Count);
			AssertEquals(a1.PK, dummyDependents[0][DummyDependentBizoSchema.ZD1_Z0]);
			AssertEquals(45, dummyDependents[0][DummyDependentBizoSchema.ZD1_Number]);
		}

		public void TestRepository()
		{
			var repository = new RowWrapperRepository();

			var dummy1 = repository.New(DummyBizoSchema.Instance);
			dummy1[DummyBizoSchema.Z0_Code] = "AAA";

			var dummy2 = repository.New(DummyBizoSchema.Instance);
			dummy2[DummyBizoSchema.Z0_Code] = "BBB";

			var dummyDependent = repository.New(DummyDependentBizoSchema.Instance);
			dummyDependent[DummyDependentBizoSchema.ZD1_Z0] = dummy1.PK;
			dummyDependent[DummyDependentBizoSchema.ZD1_Number] = 45;

			repository.Save();

			repository = new RowWrapperRepository();

			var dummies = repository.Load(DummyBizoSchema.Instance, "SELECT * FROM dbo.DummyBizo");
			dummy1 = dummies.FirstOrDefault(dummy => dummy.PK == dummy1.PK);
			dummy2 = dummies.FirstOrDefault(dummy => dummy.PK == dummy2.PK);

			AssertNotNull(dummy1);
			AssertNotNull(dummy2);
			AssertEquals("AAA  ", dummy1[DummyBizoSchema.Z0_Code]);
			AssertEquals("BBB  ", dummy2[DummyBizoSchema.Z0_Code]);

			var dummyDependents = repository.Load(DummyDependentBizoSchema.Instance, string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.DummyDependentBizo WHERE ZD1_Z0 = '{0}'", dummy1.PK));

			AssertEquals(1, dummyDependents.Count);
			AssertEquals(dummy1.PK, dummyDependents[0][DummyDependentBizoSchema.ZD1_Z0]);
			AssertEquals(45, dummyDependents[0][DummyDependentBizoSchema.ZD1_Number]);

			dummy1[DummyBizoSchema.Z0_Code] = "CCC";

			repository.Save();

			repository = new RowWrapperRepository();

			dummies = repository.Load(DummyBizoSchema.Instance, "SELECT * FROM dbo.DummyBizo");
			dummy1 = dummies.FirstOrDefault(dummy => dummy.PK == dummy1.PK);
			dummy2 = dummies.FirstOrDefault(dummy => dummy.PK == dummy2.PK);

			AssertNotNull(dummy1);
			AssertNotNull(dummy2);
			AssertEquals("CCC  ", dummy1[DummyBizoSchema.Z0_Code]);
			AssertEquals("BBB  ", dummy2[DummyBizoSchema.Z0_Code]);

			AssertContainsExactElementsInAnyOrder(dummies.Select(dummy => dummy.PK), repository.Load(DummyBizoSchema.Instance).Select(dummy => dummy.PK));
		}

		public void TestLoadInBatches()
		{
			var repository = new RowWrapperRepository();

			var dummy1 = repository.New(DummyBizoSchema.Instance);
			dummy1[DummyBizoSchema.Z0_Code] = "AAA";
			var dummy1PK = dummy1.PK;

			var dummy2 = repository.New(DummyBizoSchema.Instance);
			dummy2[DummyBizoSchema.Z0_Code] = "BBB";
			var dummy2PK = dummy2.PK;

			repository.Save();
			repository = new RowWrapperRepository();

			var pks = new Guid[]
			{
				dummy1PK,
				dummy2PK
			};

			var dummies = repository.Load(DummyBizoSchema.Instance, pks);

			AssertContainsExactElementsInAnyOrder(pks, dummies.Select(x => x.PK));

			repository = new RowWrapperRepository();
			dummies = repository.Load(DummyBizoSchema.Instance, Array.Empty<Guid>());

			AssertEquals("no results expected without PK", false, dummies.Any());
		}
	}
}
