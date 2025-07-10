using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	class JobDeclarationB3SendingStrategyTest : TestCaseWithFactory
	{
		public void TestShouldAddB3LateSendingWarningEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_CSAEntry = false;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			declaration.CA_K84AccountingDate = ZDateTime.Empty;
			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			using (CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, delayFactorRegistryBO))
			{
				var strategy = JobDeclarationB3SendingStrategy.GetJobDeclarationB3SendingStrategy(declaration);
				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
				AssertEquals(true, strategy.ShouldAddB3LateSendingWarningEvent);
			}
		}

		public void TestShouldAutoSendB3Message()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			declaration.CA_K84AccountingDate = ZDateTime.Empty;
			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			using (CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, delayFactorRegistryBO))
			{
				var strategy = JobDeclarationB3SendingStrategy.GetJobDeclarationB3SendingStrategy(declaration);
				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
				AssertEquals(true, strategy.ShouldAutoSendB3Message);
			}
		}
	}
}
