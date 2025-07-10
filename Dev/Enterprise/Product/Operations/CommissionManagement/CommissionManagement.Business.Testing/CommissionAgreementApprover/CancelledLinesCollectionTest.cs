using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CancelledLinesCollection))]
	internal class CancelledLinesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CancelledLinesCollection>
	{
		#region Add / Remove

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Add Lines

		public void TestAddLines()
		{
			var lines = new List<ViewCommissionLine>();
			Enumerable.Range(1, 5).ForEach(x => lines.Add(Factory.New<ViewCommissionLine>()));

			var collection = GetCollectionToTest();
			AssertEquals("Pre-Condition", 0, collection.Count);

			collection.AddLines(Enumerable.Empty<ViewCommissionLine>());
			AssertEquals("Nothing should have been added", 0, collection.Count);

			collection.AddLines(lines);
			var viewLinesCollection = collection.Select(excludeLine => excludeLine.DataItem);
			AssertContainsExactElementsInAnyOrder("Lines should be added and the same", lines, viewLinesCollection);
		}

		#endregion

		#region Tick/Untick

		public void TestTickAndUntickAll()
		{
			var lines = new List<ViewCommissionLine>();
			Enumerable.Range(1, 5).ForEach(x => lines.Add(Factory.New<ViewCommissionLine>()));

			var collection = GetCollectionToTest();
			collection.AddLines(lines);

			foreach (ExcludeViewCommissionLine line in collection)
			{
				Assert("Pre-Condition", line.IsExcluded);
			}

			collection.UntickAll();

			foreach (ExcludeViewCommissionLine line in collection)
			{
				Assert("All lines should be unticked", !line.IsExcluded);
			}

			collection.TickAll();

			foreach (ExcludeViewCommissionLine line in collection)
			{
				Assert("All lines should be ticked", line.IsExcluded);
			}
		}

		#endregion

		#region Implementation

		protected override CancelledLinesCollection GetCollectionToTest()
		{
			return new CancelledLinesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExcludeViewCommissionLine(Factory.New<ViewCommissionLine>());
		}

		#endregion
	}

	[TestedType(typeof(ExcludeViewCommissionLine))]
	internal class ExcludeViewCommissionLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDataItem()
		{
			var line = new ExcludeViewCommissionLine(LineForTest);
			AssertEquals(LineForTest, line.DataItem);
		}

		public void TestIsExcluded()
		{
			var line = new ExcludeViewCommissionLine(LineForTest);
			Assert(line.IsExcluded);
		}

		public void TestSourceNumber_ForNull()
		{
			LineForTest.VCL_GroupingSourceTableCode = string.Empty;
			LineForTest.VCL_GroupingSourceID = ZGuid.Empty;

			var line = new ExcludeViewCommissionLine(LineForTest);
			AssertEquals("SourceNumber should return ZString.Empty as there is no grouping source.", ZString.Empty, line.SourceNumber);
		}

		public void TestSourceNumber_ForJobHeader()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobLoader = new JobHeader.Loader(shipment);
			var header = jobLoader.TryCreate();

			LineForTest.VCL_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			LineForTest.VCL_GroupingSourceID = header.PK;
			LineForTest.VCL_JobNumber = header.JH_JobNum;

			var line = new ExcludeViewCommissionLine(LineForTest);
			AssertEquals("SourceNumber should return the Job Number.", header.JH_JobNum, line.SourceNumber);
		}

		public void TestSourceNumber_ForAccTransactionHeader()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionNum = "WTGTEST";

			LineForTest.VCL_GroupingSourceTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			LineForTest.VCL_GroupingSourceID = header.PK;

			var line = new ExcludeViewCommissionLine(LineForTest);
			AssertEquals("SourceNumber should return Transaction Number.", header.AH_TransactionNum, line.SourceNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExcludeViewCommissionLine(LineForTest);
		}

		ViewCommissionLine LineForTest => lineForTest ?? (lineForTest = Factory.NewWithValidTestData<ViewCommissionLine>());
		ViewCommissionLine lineForTest;
	}
}
