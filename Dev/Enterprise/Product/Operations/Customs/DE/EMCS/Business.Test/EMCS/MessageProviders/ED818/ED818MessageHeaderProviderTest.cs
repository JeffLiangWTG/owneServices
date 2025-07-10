using System;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED818MessageHeaderProvider))]
	class ED818MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<ED818MessageHeaderProvider, ED818HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED818MessageHeaderProvider(emcsDeclaration, null));
		}

		protected override void SetUp()
		{
			base.SetUp();

			reportOfReceipt = new ReportOfReceiptSendingAction(emcsDeclaration);
		}
		ReportOfReceiptSendingAction reportOfReceipt;

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new ED818MessageHeaderProvider(emcsDeclaration, reportOfReceipt);
	}
}
