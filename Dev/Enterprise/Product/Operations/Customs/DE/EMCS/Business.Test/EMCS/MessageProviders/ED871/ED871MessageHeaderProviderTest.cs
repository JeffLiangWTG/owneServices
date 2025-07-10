using CargoWise.Customs.DE.MessageContracts.EMCS;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED871MessageHeaderProvider))]
	class ED871MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<ED871MessageHeaderProvider, ED871HeaderProvider>
	{
		public void TestIsDeclarantTypeConsignor()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
				AssertEquals("Consignor", true, MessageHeaderProvider.IsDeclarantTypeConsignor);
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
				AssertEquals("Consignee", false, MessageHeaderProvider.IsDeclarantTypeConsignor);
			});
		}

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new ED871MessageHeaderProvider(emcsDeclaration, string.Empty);

		protected new IED871MessageHeader MessageHeaderProvider => (IED871MessageHeader)base.MessageHeaderProvider;
	}
}
