using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestedType(typeof(CMREventProcessor))]
	class CMREventProcessorTest : AMREventProcessorAbstractTest<CMREventProcessor>
	{
		protected override CMREventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new CMREventProcessor(eventDataObject, logger, factory);
		}

		public void TestProcessClearEventFromJapanCustomsForCMR()
		{
			CombineAssertions(() =>
			{
				AssertProcessClearEventFromJapanCustomsForMaster(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, MessageStatusList.Codes.AwaitingMasterBillAddAfterATD, MessageStatusList.Codes.ClearMasterBillAddAfterATD, "Update Advance Cargo Information Registration");
				AssertProcessClearEventFromJapanCustomsForMaster(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, MessageStatusList.Codes.AwaitingMasterBillDelete, MessageStatusList.Codes.ClearMasterBillDelete, "Update Advance Cargo Information Registration");
				AssertProcessClearEventFromJapanCustomsForMaster(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, MessageStatusList.Codes.AwaitingMasterBillUpdate, MessageStatusList.Codes.ClearMasterBillUpdate, "Update Advance Cargo Information Registration");
			});
		}

		public void TestProcessErrorEventFromJapanCustomsForCMR()
		{
			CombineAssertions(() =>
			{
				AssertProcessErrorEventFromJapanCustomsForMaster(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, MessageStatusList.Codes.AwaitingMasterBillAddAfterATD, MessageStatusList.Codes.ErrorMasterBillAddAfterATD, "Update Advance Cargo Information Registration");
				AssertProcessErrorEventFromJapanCustomsForMaster(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, MessageStatusList.Codes.AwaitingMasterBillDelete, MessageStatusList.Codes.ErrorMasterBillDelete, "Update Advance Cargo Information Registration");
				AssertProcessErrorEventFromJapanCustomsForMaster(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, MessageStatusList.Codes.AwaitingMasterBillUpdate, MessageStatusList.Codes.ErrorMasterBillUpdate, "Update Advance Cargo Information Registration");
			});
		}
	}
}
