using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class UploadDocumentsSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestSendingActionTypeList()
		{
			AssertSendingActionTypeList(enableCentralizedClearanceForImport: true, isUCC5: false, "446, 483");
			AssertSendingActionTypeList(enableCentralizedClearanceForImport: true, isUCC5: true, "483");
			AssertSendingActionTypeList(enableCentralizedClearanceForImport: false, isUCC5: false, "483");
			AssertSendingActionTypeList(enableCentralizedClearanceForImport: false, isUCC5: true, "483");
		}

		void AssertSendingActionTypeList(bool enableCentralizedClearanceForImport, bool isUCC5, ZString expectedSendingActionTypeList)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCentralizedClearanceForImport))
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, isUCC5))
			{
				var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
				var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
				CombineAssertions($"Conditions: enableCentralizedClearanceForImport: {enableCentralizedClearanceForImport}, isUCC5: {isUCC5}", () => { 
					var lookups = uploadDocumentsSendingAction.Lookups;
					AssertEquals("SendingActionTypeList.CodesAsString", expectedSendingActionTypeList, lookups.SendingActionTypeList.CodesAsString);

					var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
					var anotherSendingAction = new UploadDocumentsSendingAction(entryHeader);
					AssertSame("Should be cached.", lookups.SendingActionTypeList, anotherSendingAction.Lookups.SendingActionTypeList);
				});
			}
		}

		public void TestSendingActionTypeListForDisplay()
		{
			AssertSendingActionTypeListForDisplay(enableCentralizedClearanceForImport: true, isUCC5: false, "IM446, IM483");
			AssertSendingActionTypeListForDisplay(enableCentralizedClearanceForImport: true, isUCC5: true, "IM483");
			AssertSendingActionTypeListForDisplay(enableCentralizedClearanceForImport: false, isUCC5: false, "IM483");
			AssertSendingActionTypeListForDisplay(enableCentralizedClearanceForImport: false, isUCC5: true, "IM483");
		}

		void AssertSendingActionTypeListForDisplay(bool enableCentralizedClearanceForImport, bool isUCC5, ZString expectedSendingActionTypeListForDisplay)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCentralizedClearanceForImport))
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, isUCC5))
			{
				var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
				var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
				CombineAssertions($"Conditions: enableCentralizedClearanceForImport: {enableCentralizedClearanceForImport}, isUCC5: {isUCC5}", () => {
						var lookups = uploadDocumentsSendingAction.Lookups;
					AssertEquals("SendingActionTypeListForDisplay.CodesAsString", expectedSendingActionTypeListForDisplay, lookups.SendingActionTypeListForDisplay.CodesAsString);

					var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
					var anotherSendingAction = new UploadDocumentsSendingAction(entryHeader);
					AssertSame("Should be cached.", lookups.SendingActionTypeListForDisplay, anotherSendingAction.Lookups.SendingActionTypeListForDisplay);
				});
			}
		}
	}
}
