using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAJobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCA_TransportDocumentNumberIsNotCopied()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_TransportDocumentNumber = "812-123456789";
			var clonedDeclaration = (JobDeclaration)new CAJobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
			AssertEquals("Do not copy CA_TransportDocumentNumber", ZString.Empty, clonedDeclaration.CA_TransportDocumentNumber);
		}

		public void TestUpdateCA_B3AutoSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_B3AutoSend = true;
			using (CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var clonedDeclaration = (JobDeclaration)new CAJobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
				Assert("CA_B3AutoSend should not be set", !clonedDeclaration.CA_B3AutoSend);
			}
			declaration.CA_B3AutoSend = false;
			using (CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var clonedDeclaration = (JobDeclaration)new CAJobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
				Assert("CA_B3AutoSend should be set", clonedDeclaration.CA_B3AutoSend);
			}
		}

		public void TestCloneInternalUsingOldMessageSubType()
		{
			var oldDeclaration = Factory.New<JobDeclaration>();
			oldDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			oldDeclaration.JE_MessageSubType = "101";
			AssertEquals("10-1", oldDeclaration.JE_MessageSubType);

			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var newDeclaration = (JobDeclaration)new CAJobDeclarationDeepCloneStrategy(oldDeclaration, CloneType.TemplateCopy, Factory).Clone();
				AssertEquals("10-1", newDeclaration.JE_MessageSubType);
			});
		}
	}
}
