using System;
using System.Collections;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectReaderTest : TestCaseWithFactory
	{
		public void TestApproximateCount()
		{
			BusinessObjectReaderForTest reader = new BusinessObjectReaderForTest(new BusinessObjectFactoryProvider());
			AssertEquals("If no actual or approximation could be made -1 should be returned as it is not a 'actual' number", -1, reader.ApproximateCount);
		}

		public void TestEnumerate()
		{
			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();
			ArrayList enumeratedDummyList = new ArrayList();
			foreach (DummyBusinessObject dummy in new BusinessObjectReaderForTest(factoryProvider))
			{
				enumeratedDummyList.Add(dummy);
			}

			DummyBusinessObject[] enumeratedDummies = (DummyBusinessObject[])enumeratedDummyList.ToArray(typeof(DummyBusinessObject));
			AssertEquals("1st item of first batch", "1", enumeratedDummies[0].Z0_Code);
			AssertEquals("1st item of first batch", "bill", enumeratedDummies[0].Z0_Description);
			AssertEquals("2nd item of first batch", "1", enumeratedDummies[1].Z0_Code);
			AssertEquals("2nd item of first batch", "bob", enumeratedDummies[1].Z0_Description);
			AssertEquals("1st item of second batch", "2", enumeratedDummies[2].Z0_Code);
			AssertEquals("1st item of second batch", "bill", enumeratedDummies[2].Z0_Description);
			AssertEquals("2nd item of second batch", "2", enumeratedDummies[3].Z0_Code);
			AssertEquals("2nd item of second batch", "bob", enumeratedDummies[3].Z0_Description);
			Assert("Factories should be thrown away between batches for performance", enumeratedDummies[0].Factory != enumeratedDummies[2].Factory);
		}

		public void TestSaveBeforeLoadNextEnabled()
		{
			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();
			ArrayList enumeratedDummyList = new ArrayList();
			BusinessObjectReaderForTest reader = new BusinessObjectReaderForTest(factoryProvider);
			reader.SaveBeforeLoadNextEnabled = false;
			int noOfDummyObject = reader.Factory.GetDatabaseCount(typeof(DummyBusinessObject));
			foreach (DummyBusinessObject dummy in reader)
			{
				enumeratedDummyList.Add(dummy);
			}
			AssertEquals("PreCondition: 4 Dummy object were added", 4, enumeratedDummyList.Count);
			AssertEquals("No new dummy object were saved", noOfDummyObject, reader.Factory.GetDatabaseCount(typeof(DummyBusinessObject)));

			factoryProvider.CreateNewWithoutSave();
			reader = new BusinessObjectReaderForTest(factoryProvider);
			reader.SaveBeforeLoadNextEnabled = true;
			enumeratedDummyList.Clear();
			foreach (DummyBusinessObject dummy in reader)
			{
				enumeratedDummyList.Add(dummy);
			}
			AssertEquals("PreCondition: 4 Dummy object were added", 4, enumeratedDummyList.Count);
			AssertEquals("4 new dummy object were saved", noOfDummyObject + 4, reader.Factory.GetDatabaseCount(typeof(DummyBusinessObject)));
		}

		#region BusinessObjectReaderForTest

		class BusinessObjectReaderForTest : BusinessObjectReader
		{
			public BusinessObjectReaderForTest(BusinessObjectFactoryProvider factoryProvider) : base(factoryProvider)
			{
			}

			public override bool HasRecords
			{
				get { return true; }
			}

			public override Type BusinessObjectType
			{
				get { return typeof(DummyBusinessObject); }
			}

			protected override BusinessObject[] LoadNextBatchCore(ZGuid pK)
			{
				return GetNextBatchWithBusinessObject(Factory.Load(BusinessObjectType, pK));
			}

			protected override BusinessObject[] LoadNextBatchCore(BusinessObject lastBusinessObjectRead)
			{
				return GetNextBatchWithBusinessObject(lastBusinessObjectRead);
			}

			BusinessObject[] GetNextBatchWithBusinessObject(BusinessObject lastBusinessObjectRead)
			{
				int lastBatchNumber = 1;
				if (lastBusinessObjectRead != null)
				{
					lastBatchNumber = int.Parse(((DummyBusinessObject)lastBusinessObjectRead).Z0_Code);
					lastBatchNumber++;
				}

				DummyBusinessObject[] result = Array.Empty<DummyBusinessObject>();
				if (lastBatchNumber < 3)
				{
					result = new DummyBusinessObject[2];
					result[0] = Factory.New<DummyBusinessObject>();
					result[0].Z0_Code = lastBatchNumber.ToString();
					result[0].Z0_Description = "bill";
					result[1] = Factory.New<DummyBusinessObject>();
					result[1].Z0_Code = lastBatchNumber.ToString();
					result[1].Z0_Description = "bob";
				}
				else
				{
					result = Array.Empty<DummyBusinessObject>();
				}
				return result;
			}
		}

		#endregion
	}
}
