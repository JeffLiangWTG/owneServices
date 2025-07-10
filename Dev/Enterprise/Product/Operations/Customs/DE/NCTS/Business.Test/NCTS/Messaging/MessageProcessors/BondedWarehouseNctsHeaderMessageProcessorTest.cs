using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class BondedWarehouseNctsHeaderMessageProcessorTest : TestCaseWithFactory
	{
		public void TestReferenceDetail()
		{
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationIsBondedWarehouseSupported(Factory, isBondedWarehouseSupported: true))
			{
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				var message = Factory.NewWithValidTestData<TestEdiMessage>();
				message.EM_LinkedObject = nctsHeader.MovementHeader;
				nctsHeader.MovementHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
				Factory.Save();

				EmailDef createdEmail = null;
				var processor = new BondedWarehouseNctsHeaderMessageProcessor(message.PK, null, (email, message) => { createdEmail = email; });
				processor.ProcessAfterSaved(true);

				CombineAssertions(() =>
				{
					AssertEquals("Failed to Update Stock Release for NCTS: ATB150000620520195875", createdEmail.Subject);
					AssertContains("Job Reference: " + EmailDefBuilder.GetJobLink(nctsHeader, nctsHeader.BH_JobReference), createdEmail.Body);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_JobReference = "ATB150000620520195875";
		}

		NctsHeader nctsHeader;
	}
}
