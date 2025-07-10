using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED815TransportProvider))]
	sealed class ED815TransportProviderTest : DataProviderTestCase<ED815TransportProvider>
	{
		public void TestIdentityOfUnit()
		{
			CombineAssertions(() =>
			{
				IEMCSTransport transportProvider = new ED815TransportProvider(container, emcsDeclaration);
				AssertEquals("DeferredSubmissionFlag is not 2 and CO_ContainerNumber is empty", ZString.Empty, transportProvider.IdentityOfUnit);
				container.CO_ContainerNumber = "PONU2864065";
				AssertEquals("DeferredSubmissionFlag is not 2 and CO_ContainerNumber is not empty", "PONU2864065", transportProvider.IdentityOfUnit);

				emcsDeclaration.ZG_DeferredSubmission = EmcsDeferredSubmissionList.Codes.JaZusammengefasstesEVd;
				transportProvider = new ED815TransportProvider(container, emcsDeclaration);
				AssertEquals("DeferredSubmissionFlag is 2 and CO_ContainerNumber is not empty", "PONU2864065", transportProvider.IdentityOfUnit);
				container.CO_ContainerNumber = ZString.Empty;
				AssertEquals("DeferredSubmissionFlag is 2 and CO_ContainerNumber is empty", Constants.ConsolidatedDocumentDefaultString, transportProvider.IdentityOfUnit);
			});
		}

		protected override ED815TransportProvider GetProvider() => new ED815TransportProvider(container, emcsDeclaration);

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			container = Factory.New<EMCSCusContainer>();
		}
		EMCSJobDeclaration emcsDeclaration;
		EMCSCusContainer container;
	}
}
