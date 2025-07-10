using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NCTSMessageHeaderProvider<INCTSHeader>))]
	abstract class NCTSMessageHeaderProviderAbstractTest<TMessageHeader, THeader> : Customs.Business.Testing.DataProviderTestCase<TMessageHeader>
		where TMessageHeader : NCTSMessageHeaderProvider<THeader>
		where THeader : INCTSHeader
	{
		[ExpectNoExceptions]
		public void TestHeader()
		{
			AssertNotNull(MessageHeaderProvider.Header);
		}

		public void TestHeaderType()
		{
			AssertType<THeader>(MessageHeaderProvider.Header);
		}

		public void TestHeaderCached()
		{
			AssertSame(MessageHeaderProvider.Header, MessageHeaderProvider.Header);
		}

		protected INCTSMessageHeader MessageHeaderProvider => messageHeaderProvider ?? (messageHeaderProvider = GetMessageHeaderProvider());
		INCTSMessageHeader messageHeaderProvider;

		protected abstract INCTSMessageHeader GetMessageHeaderProvider();

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		protected NctsHeader nctsHeader;

		protected override TMessageHeader GetProvider() => (TMessageHeader)MessageHeaderProvider;
	}
}
