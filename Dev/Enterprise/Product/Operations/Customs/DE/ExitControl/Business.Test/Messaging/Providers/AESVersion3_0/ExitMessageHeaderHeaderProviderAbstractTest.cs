using CargoWise.Customs.DE.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	[TestsSubclassesOf(typeof(ExitMessageHeaderProvider))]
	abstract class ExitMessageHeaderProviderAbstractTest<TMessageHeader> : Customs.Business.Testing.DataProviderTestCase<TMessageHeader>
			where TMessageHeader : ExitMessageHeaderProvider
	{
		[ExpectNoExceptions]
		public void TestExitHeader()
		{
			AssertNotNull(MessageHeaderProvider.ExitHeader);
		}

		public void TestExitHeaderType()
		{
			AssertEquals(true, MessageHeaderProvider.ExitHeader is IExitHeader);
		}

		public void TestExitHeaderCached()
		{
			AssertSame(MessageHeaderProvider.ExitHeader, MessageHeaderProvider.ExitHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			exitReport = header.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;
		}

		protected IExitMessageHeader MessageHeaderProvider => messageHeaderProvider ?? (messageHeaderProvider = GetMessageHeaderProvider());
		IExitMessageHeader messageHeaderProvider;

		protected CusExitReport exitReport;

		protected abstract IExitMessageHeader GetMessageHeaderProvider();

		protected override TMessageHeader GetProvider() => (TMessageHeader)MessageHeaderProvider;
	}
}
