using System;
using Enterprise.Customs.EU.Registry;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class TariffDescriptionSynchronizerTest : TransactionedTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TariffDescriptionSynchronizer(null));
		}

		public void TestShouldSynchronizeDescription()
		{
			supporterMock.Setup(m => m.RegistryBranchPK).Returns(GlbBranch.CurrentBranch.PK.ToGuid());
			CombineAssertions("When EUCustomsDataRegistry.EnableAutoTariffDescriptionPopulation is true:", () =>
			{
				using (EUCustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
				{
					supporterMock.Setup(m => m.CurrentTariffDescription).Returns("");
					AssertEquals("When TariffDescription is empty, ShouldSynchronizeDescription", true, synchronizer.ShouldSynchronizeDescription);

					supporterMock.Setup(m => m.CurrentTariffDescription).Returns("ABC");
					supporterMock.Setup(m => m.OfficialCustomsTariffDescription).Returns("ABC");
					AssertEquals("When TariffDescription and OfficialCustomsDescription are equal, ShouldSynchronizeDescription", true, synchronizer.ShouldSynchronizeDescription);

					supporterMock.Setup(m => m.CurrentTariffDescription).Returns("ABC");
					supporterMock.Setup(m => m.OfficialCustomsTariffDescription).Returns("DEF");
					AssertEquals("When TariffDescription and OfficialCustomsDescription are different, ShouldSynchronizeDescription", false, synchronizer.ShouldSynchronizeDescription);
				}
			});

			CombineAssertions("When EUCustomsDataRegistry.EnableAutoTariffDescriptionPopulation is false:", () =>
			{
				using (EUCustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false))
				{
					AssertEquals("ShouldSynchronizeDescription would be false anyway.", false, synchronizer.ShouldSynchronizeDescription);
				}
			});
			supporterMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestSynchronizerDescription()
		{
			supporterMock.Setup(m => m.OfficialCustomsTariffDescription).Returns("ABC");
			synchronizer.SynchronizeDescription();
			supporterMock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			supporterMock = new Mock<ITariffDescriptionSynchronizerSupporter>();
			synchronizer = new TariffDescriptionSynchronizer(supporterMock.Object);
		}

		Mock<ITariffDescriptionSynchronizerSupporter> supporterMock;
		TariffDescriptionSynchronizer synchronizer;
	}
}
