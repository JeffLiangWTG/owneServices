using System;
using System.Linq;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Module.Testing
{
	abstract class BaseSendCatalogApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestApply()
		{
			var catalogs = BaseGoodsCatalogBatchMessageSenderTest.CreateGoodsCatalogs(Factory, BaseGoodsCatalogBatchMessageSenderTest.CreateOwner(Factory));

			using (BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var log = SimulateRun(catalogs.ToArray(), false);
				AssertMultilineASCIIEquals(ExpectedSendingLog, log.MessagesString());

				AssertEquals("Confirmation Message", ExpectedConfirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Confirmation Caption", ExpectedConfirmationCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		protected abstract string ExpectedConfirmationMessage { get; }
		protected abstract string ExpectedConfirmationCaption { get; }
		protected abstract string ExpectedSendingLog { get; }
	}
}
