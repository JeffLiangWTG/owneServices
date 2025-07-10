using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class OnSavingServiceTestCase : TestCaseWithFactory
	{
		#region Implementation

		class CloneBizoService : IOnSavingServiceBuilder
		{
			public CloneBizoService(params ZGuid[] bizosToClone)
			{
				this.bizosToClone = bizosToClone;
			}

			readonly ZGuid[] bizosToClone;

			public IEnumerable<IOnSavingService> Build(IEnumerable<BusinessObject> rows)
			{
				yield return new Clone(bizosToClone);
			}

			class Clone : IOnSavingService
			{
				public Clone(ZGuid[] bizosToClone)
				{
					this.bizosToClone = bizosToClone;
				}
				readonly ZGuid[] bizosToClone;
				readonly List<DummyBusinessObject> bizosCreated = new List<DummyBusinessObject>();

				public void Apply(IEnumerable<BusinessObject> bizos)
				{
					foreach (var bizo in bizos)
					{
						if (bizo is DummyBusinessObject dummy && bizosToClone.Contains(bizo.PK))
						{
							bizosCreated.Add((DummyBusinessObject)dummy.Clone());
						}
					}
				}

				public void OnSaveFailed()
				{
					bizosCreated.ForEach(b => b.Delete());
				}
			}
		}

		class DummyCountingService : IOnSavingServiceBuilder
		{
			public IEnumerable<IOnSavingService> Build(IEnumerable<BusinessObject> rows)
			{
				var count = rows.Count(t => t.Table.TableName == DummyBizoSchema.Constants.TableName && ((ZInt)t[DummyBizoSchema.Z0_Number.Name]) == 0);
				if (count > 0)
				{
					yield return new CountRecordoService(count);
				}
			}

			class CountRecordoService : IOnSavingService
			{
				public CountRecordoService(int count)
				{
					this.count = count;
				}
				readonly int count;
				readonly List<DummyBusinessObject> modified = new List<DummyBusinessObject>();

				public void Apply(IEnumerable<BusinessObject> bizos)
				{
					foreach (var bizo in bizos)
					{
						if (bizo is DummyBusinessObject dummy)
						{
							dummy.Z0_Number = count;
							modified.Add(dummy);
						}
					}
				}

				public void OnSaveFailed()
				{
					foreach (var item in modified)
					{
						item.Z0_Number = 0;
					}
				}
			}
		}

		#endregion

		public void TestRowServiceOnSave()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();

			Factory.ServiceContainer.AddOnSavingService(new DummyCountingService());

			AssertEquals(0, dummy1.Z0_Number);
			AssertEquals(0, dummy2.Z0_Number);
			AssertEquals(0, dummy3.Z0_Number);
			Factory.Save();

			AssertEquals(3, dummy1.Z0_Number);
			AssertEquals(3, dummy2.Z0_Number);
			AssertEquals(3, dummy3.Z0_Number);
		}

		public void TestMulitpleBizosAroundARow()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var sameDummy = Factory.Load<DummyBaseBusinessObject>(dummy1.PK);
			Assert(sameDummy is DummyBaseBusinessObject);

			Factory.ServiceContainer.AddOnSavingService(new DummyCountingService());

			AssertEquals(0, dummy1.Z0_Number);
			AssertEquals(0, sameDummy.Z0_Number);
			Factory.Save();
			AssertEquals(1, dummy1.Z0_Number);
			AssertEquals(1, sameDummy.Z0_Number);
		}

		public void TestNewRowsDuringOnSavingAreCompensatedFor()
		{
			// In this test, we are expecting that the two dummies cloned by the clone bizo service get counted.
			// Additionally this test proves that on saving is not called a second time on any bizo.
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();

			Factory.ServiceContainer.AddOnSavingService(new CloneBizoService(dummy1.PK, dummy2.PK));
			Factory.ServiceContainer.AddOnSavingService(new DummyCountingService());

			AssertEquals(0, dummy1.Z0_Number);
			Factory.Save();
			AssertEquals(3, dummy1.Z0_Number);

			var query = new ZQuery(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, dummy1.PK) { FetchOnlyFromLocalCache = true }
			.AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, dummy2.PK)
			.AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, dummy3.PK);

			var newDummies = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(2, newDummies.Length);
			AssertEquals(2, newDummies[0].Z0_Number);
		}

		public void TestSaveFailed()
		{
			// In this test, we are proving that if save fails, we reset the value back to zero.
			var dummy1 = Factory.New<DummyBusinessObject>();

			Factory.Save();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loaded = newFactory.Load<DummyBusinessObject>(dummy1.PK);
			loaded.Z0_Description = "Thankyou";
			newFactory.Save();

			Factory.ServiceContainer.AddOnSavingService(new DummyCountingService());
			dummy1.Z0_Description = "NoThankYou";

			AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
			AssertEquals(0, dummy1.Z0_Number);
		}
	}
}
