using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE871MessageHeaderProvider))]
	sealed class IE871MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE871MessageHeaderProvider, IE871HeaderProvider>
	{
		public void TestIsDeclarantTypeConsignor()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				AssertEquals("Consignor", true, MessageHeaderProvider.IsDeclarantTypeConsignor);
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				AssertEquals("Consignee", false, MessageHeaderProvider.IsDeclarantTypeConsignor);
			});
		}

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE871MessageHeaderProvider(emcsDeclaration, string.Empty);

		new IIE871MessageHeader MessageHeaderProvider => (IIE871MessageHeader)base.MessageHeaderProvider;
	}
}
