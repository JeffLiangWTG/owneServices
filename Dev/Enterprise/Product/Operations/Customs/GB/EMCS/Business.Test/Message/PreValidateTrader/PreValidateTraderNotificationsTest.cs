using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(PreValidateTraderNotifications))]
	sealed class PreValidateTraderNotificationsTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor()
		{
			_ = new PreValidateTraderNotifications(null);
		}

		public void TestNotifications()
		{
			AssertEquals(4, notificationCollection.Count);
			AssertEquals(2, notificationCollection.ErrorCount);
			Assert(notificationCollection.ContainsError("Error"));
			Assert(notificationCollection.ContainsError("Error1"));
			AssertEquals(1, notificationCollection.WarningCount);
			Assert(notificationCollection.ContainsWarning("Warning"));
			AssertEquals(1, notificationCollection.InformationCount);
			Assert(notificationCollection.ContainsInformation("Information"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			notificationCollection = new MessageSendingNotificationCollection();
			notificationCollection.AddError("Error");
			notificationCollection.AddError("Error1");
			notificationCollection.AddWarning("Warning");
			notificationCollection.AddInformation("Information");
		}

		MessageSendingNotificationCollection notificationCollection;
	}
}
