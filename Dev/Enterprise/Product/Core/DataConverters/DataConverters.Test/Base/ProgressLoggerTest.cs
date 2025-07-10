using System;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Base
{
	sealed internal class ProgressLoggerTest : TestCase
	{
		public void TestCanAddAndQueryAsACollection()
		{
			var logger = new ProgressLogger();
			logger.Add("Go you big brown hound!");
			AssertEquals("Collection Count", 1, logger.Count);
			AssertEquals("Indexer Returns correct String", "Go you big brown hound!", logger[0]);
			logger[0] = "Quick Brown Fox";
			AssertEquals("Indexer Returns correct String", "Quick Brown Fox", logger[0]);
		}

		public void TestToString()
		{
			var logger = new ProgressLogger();
			logger.Add("Go you big brown hound!");
			AssertEquals("Logger.ToString() after add", "Go you big brown hound!\r\n", logger.ToString());
		}

		public void TestStartLog()
		{
			var expectedResult = @"======================================================================
Loading FRED from: JOHN
======================================================================
";
			var logger = new ProgressLogger();
			logger.StartLog("FRED", "JOHN");
			AssertEquals("Logger.ToString() after StartLog", expectedResult, logger.ToString());
		}

		public void TestDisplayFormatLogMessageWithRowDescription()
		{
			var expectedResult = @"ERROR READING ROW 0: [THIS IS ME]
Error In Something.
";
			var logger = new ProgressLogger();
			logger.DisplayFormatLogMessage("[THIS IS ME]", "Error In Something.");
			AssertEquals("Logger.ToString() after DisplayFormatLogMessage", expectedResult, logger.ToString());
		}

		public void TestDisplayFormatLogMessageWithoutRowDescription()
		{
			var expectedResult = @"ERROR READING ROW 0: Error In Something.
";
			var logger = new ProgressLogger();
			logger.DisplayFormatLogMessage("", "Error In Something.");
			AssertEquals("Logger.ToString() after DisplayFormatLogMessage", expectedResult, logger.ToString());
		}

		public void TestOutputFinalTotals()
		{
			var expectedResult = @"======================================================================
F I N A L   T O T A L S : 
Records Created  = 0
Records Updated  = 0
Records Excluded = 0
Records Invalid  = 0
======================================================================
";
			var logger = new ProgressLogger();
			logger.OutputFinalTotals();
			AssertEquals("Logger.ToString() after OutputFinalTotals", expectedResult, logger.ToString());
		}

		public void TestOnUpdateCounters()
		{
			var consumer = new TestConsumingClass();
			AssertEquals("Precondition: Consumer.UpdateCountersCalled", false, consumer.UpdateCountersCalled);
			consumer.Logger.OnUpdateCounters();
			AssertEquals("Consumer.UpdateCountersCalled when Event not linked", false, consumer.UpdateCountersCalled);
			consumer.LinkEvents();
			consumer.Logger.OnUpdateCounters();
			AssertEquals("Consumer.UpdateCountersCalled when Event linked", true, consumer.UpdateCountersCalled);
		}

		public void TestOnUpdateLogText()
		{
			var consumer = new TestConsumingClass();
			AssertEquals("Precondition: Consumer.UpdateLogTextCalled", false, consumer.UpdateLogTextCalled);
			consumer.Logger.Add("LALALA");
			AssertEquals("Consumer.UpdateLogTextCalled when Event not linked", false, consumer.UpdateLogTextCalled);
			consumer.LinkEvents();
			consumer.Logger.Add("LALALA");
			AssertEquals("Consumer.UpdateLogTextCalled when Event linked", true, consumer.UpdateLogTextCalled);
		}

		class TestConsumingClass
		{
			public TestConsumingClass()
			{
				Logger = new ProgressLogger();
			}

			public void LinkEvents()
			{
				Logger.UpdateCounters += new EventHandler(Logger_UpdateCounters);
				Logger.UpdateLogText += new EventHandler(Logger_UpdateLogText);
			}

			public readonly ProgressLogger Logger;
			public bool UpdateCountersCalled;
			public bool UpdateLogTextCalled;

			void Logger_UpdateCounters(object sender, EventArgs e)
			{
				if (sender is ProgressLogger)
				{
					UpdateCountersCalled = true;
				}
			}

			void Logger_UpdateLogText(object sender, EventArgs e)
			{
				if (sender is string)
				{
					UpdateLogTextCalled = true;
				}
			}
		}
	}
}
