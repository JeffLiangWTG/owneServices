using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class DocUserNotificationTest : TestCase
	{
		public void TestGetPercentComplete()
		{
			DeliveryInstructions deliveryInstructions = new DeliveryInstructions();
			using (DocUserNotification docUserNotificationToTest = new DocUserNotification(deliveryInstructions, 1))
			{
				deliveryInstructions.OnDocPackStarted(0);
				AssertEquals(0, docUserNotificationToTest.GetPercentCompleteForTesting(0, 1));
				AssertEquals(0, docUserNotificationToTest.GetPercentCompleteForTesting(0, 2));
				AssertEquals(50, docUserNotificationToTest.GetPercentCompleteForTesting(1, 2));

				AssertEquals(100, docUserNotificationToTest.GetPercentCompleteForTesting(1, 1));
				AssertEquals(50, docUserNotificationToTest.GetPercentCompleteForTesting(1, 2));
				AssertEquals(100, docUserNotificationToTest.GetPercentCompleteForTesting(2, 2));
			}

			using (DocUserNotification docUserNotificationToTest = new DocUserNotification(deliveryInstructions, 10))
			{
				deliveryInstructions.OnDocPackStarted(0);
				AssertEquals(0, docUserNotificationToTest.GetPercentCompleteForTesting(0, 1));
				AssertEquals(0, docUserNotificationToTest.GetPercentCompleteForTesting(0, 2));
				AssertEquals(10, docUserNotificationToTest.GetPercentCompleteForTesting(1, 1));
				AssertEquals(5, docUserNotificationToTest.GetPercentCompleteForTesting(1, 2));
				AssertEquals(10, docUserNotificationToTest.GetPercentCompleteForTesting(2, 2));

				deliveryInstructions.OnDocPackStarted(1);
				AssertEquals(10, docUserNotificationToTest.GetPercentCompleteForTesting(0, 1));
				AssertEquals(10, docUserNotificationToTest.GetPercentCompleteForTesting(0, 2));
				AssertEquals(20, docUserNotificationToTest.GetPercentCompleteForTesting(1, 1));
				AssertEquals(15, docUserNotificationToTest.GetPercentCompleteForTesting(1, 2));
				AssertEquals(20, docUserNotificationToTest.GetPercentCompleteForTesting(2, 2));
			}
		}

		public void TestGetPercentCompletePrintingDoc3OutOf2()
		{
			DeliveryInstructions deliveryInstructions = new DeliveryInstructions();
			using (DocUserNotification docUserNotificationToTest = new DocUserNotification(deliveryInstructions, 1))
			{
				deliveryInstructions.OnDocPackStarted(0);
				AssertEquals(100, docUserNotificationToTest.GetPercentCompleteForTesting(3, 2));
				AssertEquals(@"Progress.PercentComplete is greater than 100!
PercentComplete = 150
CurrentDocPack = 0
DocNumber = 3
TotalDocsInPack = 2
TotalDocPacks = 1", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestGetPercentCompletePrintingDocPack2OutOf1()
		{
			try
			{
				var deliveryInstructions = new DeliveryInstructions();
				using (var docUserNotificationToTest = new DocUserNotification(deliveryInstructions, 1))
				{
					deliveryInstructions.OnDocPackStarted(2);
					AssertEquals(100, docUserNotificationToTest.GetPercentCompleteForTesting(1, 2));
					AssertEquals(@"Progress.PercentComplete is greater than 100!
PercentComplete = 250
CurrentDocPack = 2
DocNumber = 1
TotalDocsInPack = 2
TotalDocPacks = 1", ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
	}
}
