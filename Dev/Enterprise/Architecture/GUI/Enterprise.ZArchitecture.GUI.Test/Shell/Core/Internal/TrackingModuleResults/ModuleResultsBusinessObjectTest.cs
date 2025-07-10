using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestedType(typeof(ModuleResultsBusinessObject))]
	sealed class ModuleResultsBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNumberOfResult()
		{
			AssertEquals("Empty", 0, new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>())).NumberOfResults);
			AssertEquals("Empty", 3, new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid() }))).NumberOfResults);
		}

		public void TestConstruction()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var bizO = new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 })));
			AssertEquals("List passed through correctly", bizO.PKList[0].PK, pK1);
			AssertEquals("List passed through correctly", bizO.PKList[1].PK, pK2);
		}

		public void TestCurrentRecordNumber()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var bizO = new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 })));

			bizO.CurrentRecordNumber = 1;
			AssertEquals("Current set", bizO.CurrentPK, pK1);
			AssertEquals("Current set", bizO.CurrentRecordNumber, 1);

			bizO.CurrentRecordNumber = 2;
			AssertEquals("Current set", bizO.CurrentPK, pK2);
			AssertEquals("Current set", bizO.CurrentRecordNumber, 2);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestCurrentRecordNumberThrowsException()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var bizO = new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 })));
			bizO.CurrentRecordNumber = 234;
		}

		const int FirstIndex = 1;
		public void AssertRemovingCurrentRecordGrabsTheCorrectOne(int listSize, int firstRecord, int delta)
		{
			if (listSize <= firstRecord + delta * 2)
			{
				throw new ArgumentException("Invalid argument.", nameof(listSize));
			}

			var pkCollection = Enumerable
				.Range(0, listSize)
				.Select(n => ZGuid.NewZGuid())
				.ToList();

			var bizo = new ModuleResultsBusinessObject(new ZPKCollection(pkCollection));
			bizo.CurrentRecordNumber = firstRecord;

			var nextPk = pkCollection[bizo.CurrentRecordNumber + delta - FirstIndex];

			bizo.CurrentRecordNumber += delta;
			AssertEquals(nextPk, bizo.CurrentPK);

			nextPk = pkCollection[bizo.CurrentRecordNumber + delta - FirstIndex];

			bizo.CurrentRecordNumberChanging += args => pkCollection.RemoveAt(bizo.CurrentRecordNumber - FirstIndex);
			bizo.CurrentRecordNumber += delta;

			AssertEquals(nextPk, bizo.CurrentPK);
		}

		public void TestModifyingRecordSoItIsRemovedWillNotBreakNextFuntionality()
		{
			AssertRemovingCurrentRecordGrabsTheCorrectOne(5, FirstIndex, 1);
			AssertRemovingCurrentRecordGrabsTheCorrectOne(5, 4, -1);
			AssertRemovingCurrentRecordGrabsTheCorrectOne(10, 2, 3);
		}

		public void TestIsCurrentPKInResults()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var bizO = new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 })));

			Assert("No current", !bizO.IsCurrentPKInResults);

			bizO.CurrentPK = pK1;
			Assert("OK current", bizO.IsCurrentPKInResults);

			bizO.CurrentPK = pK2;
			Assert("OK current", bizO.IsCurrentPKInResults);

			bizO.CurrentPK = ZGuid.NewZGuid();
			Assert("not in list current", !bizO.IsCurrentPKInResults);
		}

		public void TestCurrentRecordNumberWithInvalidValidation()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var bizO = new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 })));

			Assert("No errors", !bizO.CurrentRecordNumberAllowingInvalidInfo.HasErrors());
			bizO.CurrentRecordNumberAllowingInvalid = 23324;
			Assert("Has errors", bizO.CurrentRecordNumberAllowingInvalidInfo.HasErrors());

			bizO.CurrentRecordNumberAllowingInvalid = 1;
			Assert("No errors", !bizO.CurrentRecordNumberAllowingInvalidInfo.HasErrors());

			bizO.CurrentRecordNumberAllowingInvalid = 2;
			Assert("No errors", !bizO.CurrentRecordNumberAllowingInvalidInfo.HasErrors());

			bizO.CurrentRecordNumberAllowingInvalid = 0;
			Assert("Has errors", bizO.CurrentRecordNumberAllowingInvalidInfo.HasErrors());
		}

		public void TestCurrentRecordNumberWithInvalidSetsCurrentWhenValid()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var bizO = new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 })));

			bizO.CurrentRecordNumberAllowingInvalid = 1;
			AssertEquals("Set", 1, bizO.CurrentRecordNumber);

			bizO.CurrentRecordNumberAllowingInvalid = 2;
			AssertEquals("Set", 2, bizO.CurrentRecordNumber);

			bizO.CurrentRecordNumberAllowingInvalid = 90328;
			AssertEquals("Not set real current", 2, bizO.CurrentRecordNumber);
		}

		public void TestCurrentAndNumberOfRecordsUpdateOnListChanged()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var pKCollection = new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 }));
			var bizO = new ModuleResultsBusinessObject(pKCollection);

			bizO.CurrentRecordNumber = 1;
			AssertEquals("Set", 1, bizO.CurrentRecordNumber);
			AssertEquals("Set", pK1, bizO.CurrentPK);
			AssertEquals("Set", true, bizO.IsCurrentPKInResults);
			AssertEquals("Set", 2, bizO.NumberOfResults);

			bizO.PKList.Rebuild(new List<ZGuid>());
			AssertEquals("Invalid record number", 0, bizO.CurrentRecordNumber);
			AssertEquals("Still set", pK1, bizO.CurrentPK);
			AssertEquals("No longer in list", false, bizO.IsCurrentPKInResults);
			AssertEquals("Updated", 0, bizO.NumberOfResults);

			bizO.PKList.Rebuild(new List<ZGuid>(new ZGuid[] { pK2, pK1 }));
			AssertEquals("Updated", 2, bizO.CurrentRecordNumber);
			AssertEquals("Still set", pK1, bizO.CurrentPK);
			AssertEquals("Back in results", true, bizO.IsCurrentPKInResults);
			AssertEquals("Set", 2, bizO.NumberOfResults);
		}

		public void TestPKListChanged()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var pKCollection = new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 }));
			var bizO = new ModuleResultsBusinessObject(pKCollection);

			bizO.PKListChanged += new EventHandler(BizO_PKListChanged);
			PKListChangedCount = 0;

			bizO.PKList.Rebuild(new List<ZGuid>());
			AssertEquals("Called OK", 1, PKListChangedCount);

			bizO.PKList.Rebuild(new List<ZGuid>());
			AssertEquals("Called OK", 2, PKListChangedCount);
		}

		void BizO_PKListChanged(object sender, EventArgs e)
		{
			PKListChangedCount++;
		}

		int PKListChangedCount;

		public void TestCancelOnCurrentRecordNumberChangingEvent()
		{
			ZGuid pK1 = ZGuid.NewZGuid(), pK2 = ZGuid.NewZGuid();

			var bizo = new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>(new[] { pK1, pK2 })));

			bizo.CurrentRecordNumberAllowingInvalid = 1;
			AssertEquals("Set", 1, bizo.CurrentRecordNumber);

			var cancelChange = new ModuleResultsBusinessObject.NumberChangingEvent(args => args.Cancel = true);
			bizo.CurrentRecordNumberChanging += cancelChange;

			bizo.CurrentRecordNumberAllowingInvalid = 2;
			AssertEquals("Not set as cancelled", 1, bizo.CurrentRecordNumberAllowingInvalid);

			bizo.CurrentRecordNumberAllowingInvalid = 452;
			AssertEquals("Set as is invalid - no real change", 452, bizo.CurrentRecordNumberAllowingInvalid);
			AssertEquals("Set as is invalid - no real change", 1, bizo.CurrentRecordNumber);

			bizo.CurrentRecordNumberChanging -= cancelChange;
			bizo.CurrentRecordNumberChanging += args => args.Cancel = false;

			bizo.CurrentRecordNumberAllowingInvalid = 2;
			AssertEquals("Set", 2, bizo.CurrentRecordNumberAllowingInvalid);
		}

		#region Testing Events Fire

		public void TestCurrentRecordNumberEvents()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var pKCollection = new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 }));
			var bizO = new ModuleResultsBusinessObject(pKCollection);

			bizO.CurrentRecordNumberChanging += new ModuleResultsBusinessObject.NumberChangingEvent(BizO_CurrentRecordNumberChanging);
			bizO.CurrentRecordNumberChanged += new EventHandler(BizO_CurrentRecordNumberChanged);

			CurrentRecordNumberChangingCount = 0;
			CurrentRecordNumberChangedCount = 0;

			bizO.CurrentRecordNumber = 2;
			AssertEquals("CurrentRecordNumberChanging", 1, CurrentRecordNumberChangingCount);
			AssertEquals("CurrentRecordNumberChanged", 1, CurrentRecordNumberChangedCount);

			bizO.CurrentRecordNumber = 1;
			AssertEquals("CurrentRecordNumberChanging", 2, CurrentRecordNumberChangingCount);
			AssertEquals("CurrentRecordNumberChanged", 2, CurrentRecordNumberChangedCount);
		}

		void BizO_CurrentRecordNumberChanging(ModuleResultsBusinessObject.CurrentRecordNumberChangingEventArgs args)
		{
			AssertNotNull("PKList should be included in the event", args.PKList);
			CurrentRecordNumberChangingCount++;
			HadCurrentRecordNumberChanging = true;
		}
		int CurrentRecordNumberChangingCount;
		bool HadCurrentRecordNumberChanging;

		void BizO_CurrentRecordNumberChanged(object sender, EventArgs e)
		{
			if (!HadCurrentRecordNumberChanging)
			{
				throw new Exception("Have to have Changing before Changed");
			}

			CurrentRecordNumberChangedCount++;
			HadCurrentRecordNumberChanging = false;
		}
		int CurrentRecordNumberChangedCount;

		[ExpectNoExceptions]
		public void TestCurrentRecordNumberChanging_WhenPKsRemovedFromListInEvent()
		{
			var pK1 = ZGuid.NewZGuid();
			var pK2 = ZGuid.NewZGuid();

			var pKCollection = new ZPKCollection(new List<ZGuid>(new ZGuid[] { pK1, pK2 }));
			var bizO = new ModuleResultsBusinessObject(pKCollection);

			bizO.CurrentRecordNumberChanging += new ModuleResultsBusinessObject.NumberChangingEvent(BizO_CurrentRecordNumberChanging_RemoveAllPKsFromList);
			bizO.CurrentRecordNumberAllowingInvalid = 2;
		}

		void BizO_CurrentRecordNumberChanging_RemoveAllPKsFromList(ModuleResultsBusinessObject.CurrentRecordNumberChangingEventArgs args)
		{
			var emptyGuidCollection = new List<ZGuid>();
			args.PKList.Rebuild(emptyGuidCollection);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>()));
		}
	}
}
