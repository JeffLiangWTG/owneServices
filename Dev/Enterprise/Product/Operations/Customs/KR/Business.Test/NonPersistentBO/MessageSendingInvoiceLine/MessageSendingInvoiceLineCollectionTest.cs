using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(MessageSendingInvoiceLineCollection))]
	sealed class MessageSendingInvoiceLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingInvoiceLineCollection>
	{
		protected override MessageSendingInvoiceLineCollection GetCollectionToTest() => new MessageSendingInvoiceLineCollection(Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageSendingInvoiceLine(Factory.New<JobComInvoiceLine>());
	}
}

