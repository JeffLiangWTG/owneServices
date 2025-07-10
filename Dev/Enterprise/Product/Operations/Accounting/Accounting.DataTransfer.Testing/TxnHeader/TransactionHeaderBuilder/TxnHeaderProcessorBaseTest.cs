using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public abstract class TxnHeaderProcessorBaseTest : TestCaseWithFactory
	{
		public void TestGetTransactionFilter_ShouldHaveErrorWhenCreditorIsNotFound()
		{
			var processor = GetTestProcessor();
			processor.NotificationManager_ForTest = Notifier;
			processor.GetTransactionFilter_ForTest(LedgerTypes.AccountsPayable, new HashSet<string>() { "Invoice" }, new HashSet<string>() { "test123" },  false, ZGuid.Empty, new Dictionary<Xsd.TxnHeader, ZGuid>());

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Matching failed. Creditor is not found.");
		}

		protected abstract TxnHeaderProcessorBase GetTestProcessor();

		NotificationBuffer Notify;
		NotificationManager Notifier;
		NotificationTestHelper TestHelper;

		protected override void SetUp()
		{
			base.SetUp();

			TestHelper = new NotificationTestHelper();
			Notify = new NotificationBuffer();
			Notifier = new NotificationManager(Notify);
		}
	}
}
