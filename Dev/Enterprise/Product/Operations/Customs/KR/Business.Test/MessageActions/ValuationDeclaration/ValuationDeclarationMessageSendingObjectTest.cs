using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ValuationDeclarationMessageSendingObject))]
	sealed class ValuationDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			return new ValuationDeclarationMessageSendingObject(entry);
		}

		public void Test934Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entry.EntryNumber = "6N00221000004M";
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			var messageSendingObject = new ValuationDeclarationMessageSendingObject(entry);
			AssertEquals("6N002-21-000004M", messageSendingObject.FormattedEntryNumber);
			AssertEquals("10", messageSendingObject.ValuationCode);
		}
	}
}
