using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(WineCodeData))]
	sealed class WineCodeDataTest : Customs.Business.Testing.CusCodeDataTest<WineCodeData>
	{
		public void TestIsMessageStatusSendOrAwaitOnParent()
		{
			var messageForReadOnly = @"Should only be readonly and can not delete with these two conditions:
a)The wine code links to a EMCS Invoice Line and EMCS Declaration.
c)The message stauts of parent declaration is SNT or ACK.";

			void AssertCanDeleteAndReadOnly(WineCodeData sadCode, bool expectedReadOnly)
			{
				AssertEquals(messageForReadOnly, !expectedReadOnly, sadCode.CanDelete);
				AssertEquals(messageForReadOnly, expectedReadOnly, sadCode.ReadOnly);
			}

			var defaultWineCode = Factory.New<WineCodeData>();
			AssertCanDeleteAndReadOnly(defaultWineCode, false);

			var declaration = Factory.New<EMCSJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;

			var wineCodeWithEMCSParent = invoiceLine.OperationCodeDataCollection.AddNew();

			declaration.JE_MessageStatus = ZString.Empty;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(wineCodeWithEMCSParent, false);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(wineCodeWithEMCSParent, true);

			declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(wineCodeWithEMCSParent, true);
		}

		public void TestSetDefaultValues()
		{
			var wineCodeData = Factory.New<WineCodeData>();
			AssertEquals("CY_Type", CusCodeDataTypeList.Codes.WineCode, wineCodeData.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WineCodeData>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().OperationCodeDataCollection.AddNew();
		}
	}
}
