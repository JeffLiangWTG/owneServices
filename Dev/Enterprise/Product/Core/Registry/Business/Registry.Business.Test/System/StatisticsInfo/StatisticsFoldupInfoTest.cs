using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatisticsFoldupInfo))]
	sealed class StatisticsFoldupInfoTest : RegistryBusinessObjectTemplateTestCase<StatisticsFoldupInfo>
	{
		public void TestBaseValidation()
		{
			var collection = new StatisticsFoldupInfoCollection();
			var rule_1 = AddRule(collection, ZString.Empty, 0, ZString.Empty, 0);
			var rule_2 = AddRule(collection, "AAA", 1, "AAA", 1);

			var rule_3 = AddRule(collection, TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);

			var rule_4 = AddRule(collection, TimeFrameList.Codes.Minute, 10000, TimeFrameList.Codes.Minute, 10000);
			var rule_5 = AddRule(collection, TimeFrameList.Codes.Hour, 10000, TimeFrameList.Codes.Hour, 10000);
			var rule_6 = AddRule(collection, TimeFrameList.Codes.Day, 10000, TimeFrameList.Codes.Day, 10000);
			var rule_7 = AddRule(collection, TimeFrameList.Codes.Week, 10000, TimeFrameList.Codes.Week, 10000);
			var rule_8 = AddRule(collection, TimeFrameList.Codes.Quarter, 10000, TimeFrameList.Codes.Quarter, 10000);
			var rule_9 = AddRule(collection, TimeFrameList.Codes.Year, 10000, TimeFrameList.Codes.Year, 10000);

			AssertHasError(rule_1.WaitScaleInfo, "Please enter a Wait scale.");
			AssertHasError(rule_1.WaitAmountInfo, "Please enter a 'Wait amount' greater than or equal to 1.");
			AssertHasError(rule_1.AggregateScaleInfo, "Please enter an Aggregate scale.");
			AssertHasError(rule_1.AggregateAmountInfo, "Please enter an 'Aggregate amount' greater than or equal to 1.");

			AssertHasError(rule_2.WaitScaleInfo, "Enter a valid Wait scale.");
			AssertNoNotifications(rule_2.WaitAmountInfo);
			AssertHasError(rule_2.AggregateScaleInfo, "Enter a valid Aggregate scale.");
			AssertNoNotifications(rule_2.AggregateAmountInfo);

			AssertNoNotifications(rule_3);

			AssertHasError(rule_4.WaitAmountInfo, "Please enter a 'Wait amount' within the range 1 to 1440.");
			AssertHasError(rule_4.AggregateAmountInfo, "Please enter an 'Aggregate amount' within the set [1, 2, 3, 4, 5, 6, 10, 12, 15, 20, 30].");

			AssertHasError(rule_5.WaitAmountInfo, "Please enter a 'Wait amount' within the range 1 to 168.");
			AssertHasError(rule_5.AggregateAmountInfo, "Please enter an 'Aggregate amount' within the set [1, 2, 3, 4, 6, 8, 12].");

			AssertHasError(rule_6.WaitAmountInfo, "Please enter a 'Wait amount' within the range 1 to 365.");
			AssertHasError(rule_6.AggregateAmountInfo, "Please enter an 'Aggregate amount' within the set [1].");

			AssertHasError(rule_7.WaitAmountInfo, "Please enter a 'Wait amount' within the range 1 to 156.");
			AssertHasError(rule_7.AggregateScaleInfo, "Enter a valid Aggregate scale.");

			AssertHasError(rule_8.WaitAmountInfo, "Please enter a 'Wait amount' within the range 1 to 12.");
			AssertHasError(rule_8.AggregateAmountInfo, "Please enter an 'Aggregate amount' within the set [1, 2].");

			AssertHasError(rule_9.WaitAmountInfo, "Please enter a 'Wait amount' within the range 1 to 100.");
			AssertHasError(rule_9.AggregateAmountInfo, "Please enter an 'Aggregate amount' within the range 1 to 100.");
		}

		public void TestWaitOverlaps()
		{
			//Duplicates
			var collection = new StatisticsFoldupInfoCollection();
			var rule_1 = AddRule(collection, TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);
			var rule_2 = AddRule(collection, TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);
			var expected = "The wait period Minute(1) causes overlapping with the wait period Minute(1).";

			AssertHasError(rule_1.WaitAmountInfo, expected);
			AssertHasError(rule_2.WaitAmountInfo, expected);

			//No errors
			collection = new StatisticsFoldupInfoCollection();
			rule_1 = collection.AddNew(TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);
			rule_2 = collection.AddNew(TimeFrameList.Codes.Minute, 2, TimeFrameList.Codes.Minute, 1);

			AssertNoNotifications(rule_1);
			AssertNoNotifications(rule_2);

			//Overlaps simple
			collection = new StatisticsFoldupInfoCollection();
			rule_1 = AddRule(collection, TimeFrameList.Codes.Minute, 60, TimeFrameList.Codes.Minute, 1);
			rule_2 = AddRule(collection, TimeFrameList.Codes.Hour, 1, TimeFrameList.Codes.Minute, 1);
			expected = "The wait period Minute(60) causes overlapping with the wait period Hour(1).";

			AssertHasError(rule_1.WaitAmountInfo, expected);
			AssertHasError(rule_2.WaitAmountInfo, expected);

			//Overlaps based on ranges. 2 months can contain from 59 to 62 days
			collection = new StatisticsFoldupInfoCollection();
			rule_1 = AddRule(collection, TimeFrameList.Codes.Day, 58, TimeFrameList.Codes.Minute, 1);
			rule_2 = AddRule(collection, TimeFrameList.Codes.Month, 2, TimeFrameList.Codes.Minute, 1);

			AssertNoNotifications(rule_1);
			AssertNoNotifications(rule_2);

			rule_1.WaitAmount = 59;
			expected = "The wait period Day(59) causes overlapping with the wait period Month(2).";
			AssertHasError(rule_1.WaitAmountInfo, expected);
			AssertHasError(rule_2.WaitAmountInfo, expected);

			rule_1.WaitAmount = 60;
			expected = "The wait period Day(60) causes overlapping with the wait period Month(2).";
			AssertHasError(rule_1.WaitAmountInfo, expected);
			AssertHasError(rule_2.WaitAmountInfo, expected);

			rule_1.WaitAmount = 61;
			expected = "The wait period Day(61) causes overlapping with the wait period Month(2).";
			AssertHasError(rule_1.WaitAmountInfo, expected);
			AssertHasError(rule_2.WaitAmountInfo, expected);

			rule_1.WaitAmount = 62;
			expected = "The wait period Day(62) causes overlapping with the wait period Month(2).";
			AssertHasError(rule_1.WaitAmountInfo, expected);
			AssertHasError(rule_2.WaitAmountInfo, expected);

			rule_1.WaitAmount = 63;
			AssertNoNotifications(rule_1);
			AssertNoNotifications(rule_2);
		}

		public void TestFoldInOrder()
		{
			var collection = new StatisticsFoldupInfoCollection();
			var rule_1 = AddRule(collection, TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);
			var rule_2 = AddRule(collection, TimeFrameList.Codes.Minute, 2, TimeFrameList.Codes.Minute, 2);
			var rule_3 = AddRule(collection, TimeFrameList.Codes.Minute, 3, TimeFrameList.Codes.Minute, 1);
			var expected = "The aggregate period Minute(1) is less then previous aggregate period Minute(2).\r\nEach next aggregate period MUST be greater or equal to previous one.";

			AssertNoNotifications(rule_1);
			AssertHasError(rule_2.AggregateAmountInfo, expected);
			AssertHasError(rule_3.AggregateAmountInfo, expected);

			collection = new StatisticsFoldupInfoCollection();
			rule_1 = AddRule(collection, TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);
			rule_2 = AddRule(collection, TimeFrameList.Codes.Minute, 2, TimeFrameList.Codes.Minute, 1);
			rule_3 = AddRule(collection, TimeFrameList.Codes.Minute, 3, TimeFrameList.Codes.Minute, 2);

			AssertNoNotifications(rule_1);
			AssertNoNotifications(rule_2);
			AssertNoNotifications(rule_3);
		}

		public void TestFoldRanges()
		{
			var collection = new StatisticsFoldupInfoCollection();
			var rule_1 = AddRule(collection, TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);
			var rule_2 = AddRule(collection, TimeFrameList.Codes.Minute, 2, TimeFrameList.Codes.Minute, 2);
			var rule_3 = AddRule(collection, TimeFrameList.Codes.Minute, 3, TimeFrameList.Codes.Minute, 3);
			var rule_4 = AddRule(collection, TimeFrameList.Codes.Minute, 4, TimeFrameList.Codes.Minute, 4);
			var expected_1 = "The aggregate period Minute(2) causes overlapping with the aggregate period Minute(3).";
			var expected_2 = "The aggregate period Minute(3) causes overlapping with the aggregate period Minute(4).";

			AssertNoNotifications(rule_1.WaitAmountInfo);
			AssertHasError(rule_2.AggregateAmountInfo, expected_1);
			AssertHasError(rule_3.AggregateAmountInfo, expected_1);
			AssertHasError(rule_3.AggregateAmountInfo, expected_2);
			AssertHasError(rule_4.AggregateAmountInfo, expected_2);

			collection = new StatisticsFoldupInfoCollection();
			rule_1 = AddRule(collection, TimeFrameList.Codes.Minute, 1, TimeFrameList.Codes.Minute, 1);
			rule_2 = AddRule(collection, TimeFrameList.Codes.Minute, 2, TimeFrameList.Codes.Minute, 3);
			rule_3 = AddRule(collection, TimeFrameList.Codes.Minute, 3, TimeFrameList.Codes.Minute, 6);
			rule_4 = AddRule(collection, TimeFrameList.Codes.Minute, 4, TimeFrameList.Codes.Minute, 12);

			AssertNoNotifications(rule_1.AggregateAmountInfo);
			AssertNoNotifications(rule_2.AggregateAmountInfo);
			AssertNoNotifications(rule_3.AggregateAmountInfo);
			AssertNoNotifications(rule_4.AggregateAmountInfo);
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override StatisticsFoldupInfo GetBusinessObjectToClone()
		{
			return new StatisticsFoldupInfo(null, new BusinessObjectFactory(), new StatisticsFoldupInfoCollection());
		}

		protected override StatisticsFoldupInfo GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		StatisticsFoldupInfo AddRule(StatisticsFoldupInfoCollection collection, string waitScale, int waitAmount, string foldScale, int foldAmount)
		{
			var rule = collection.AddNew();
			rule.WaitScale = waitScale;
			rule.WaitAmount = waitAmount;
			rule.AggregateScale = foldScale;
			rule.AggregateAmount = foldAmount;
			return rule;
		}
	}
}
