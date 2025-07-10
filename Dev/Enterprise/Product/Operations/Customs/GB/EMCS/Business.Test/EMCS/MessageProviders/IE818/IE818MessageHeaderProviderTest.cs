using System;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE818MessageHeaderProvider))]
	sealed class IE818MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE818MessageHeaderProvider, IE818HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE818MessageHeaderProvider(emcsDeclaration, null));
		}

		protected override void SetUp()
		{
			base.SetUp();

			reportOfReceipt = new ReportOfReceiptSendingAction(emcsDeclaration);
		}
		ReportOfReceiptSendingAction reportOfReceipt;

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE818MessageHeaderProvider(emcsDeclaration, reportOfReceipt);
	}
}
