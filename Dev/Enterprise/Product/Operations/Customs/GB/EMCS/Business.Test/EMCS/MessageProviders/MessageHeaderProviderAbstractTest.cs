using CargoWise.Customs.GB.MessageContracts.EMCS;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(MessageHeaderProvider<IEMCSHeader>))]
	abstract class MessageHeaderProviderAbstractTest<TMessageHeader, THeader> : Customs.Business.Testing.DataProviderTestCase<TMessageHeader> where TMessageHeader : MessageHeaderProvider<THeader> where THeader : IEMCSHeader
	{
		[ExpectNoExceptions]
		public void TestHeader()
		{
			AssertNotNull(MessageHeaderProvider.Header);
		}

		public void TestHeader_Cached()
		{
			AssertSame(MessageHeaderProvider.Header, MessageHeaderProvider.Header);
		}

		public void TestHeaderType()
		{
			AssertType<THeader>(MessageHeaderProvider.Header);
		}

		protected IEMCSMessageHeader MessageHeaderProvider => messageHeaderProvider ?? (messageHeaderProvider = GetMessageHeaderProvider());
		IEMCSMessageHeader messageHeaderProvider;

		protected abstract IEMCSMessageHeader GetMessageHeaderProvider();

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
		}
		protected EMCSJobDeclaration emcsDeclaration;

		protected override TMessageHeader GetProvider() => (TMessageHeader)MessageHeaderProvider;
	}
}
