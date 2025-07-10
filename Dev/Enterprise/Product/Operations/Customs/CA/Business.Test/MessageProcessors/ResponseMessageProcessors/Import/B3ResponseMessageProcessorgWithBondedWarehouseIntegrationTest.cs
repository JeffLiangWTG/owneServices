using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class B3ResponseMessageProcessorgWithBondedWarehouseIntegrationTest : EDIFACTMessageProcessorTest
	{
		#region Inward Original

		public void TestProcessInwardClearOriginal()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(declaration, MessageSubTypes.Create);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", "AWO", declaration.B3EntryHeader.CH_Status);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(declaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ClearOriginal, declaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);
				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Accepted B3 CUSDEC Response for 000000012"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardErrorOriginal()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(declaration, MessageSubTypes.Create);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", "AWO", declaration.B3EntryHeader.CH_Status);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessErrorB3Message(declaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ErrorOriginal, declaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
			}
		}

		#endregion

		#region Inward Change

		public void TestProcessInwardClearChange()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(declaration, MessageSubTypes.Create);
				ProcessClearB3Message(declaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				declaration.InvoiceLines[0].JI_InvoiceQuantity = 20m;
				declaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(declaration, MessageSubTypes.Change);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(declaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ClearChange, declaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 20m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Accepted B3 CUSDEC Response for 000000012"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardErrorChange()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(declaration, MessageSubTypes.Create);
				ProcessClearB3Message(declaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				declaration.InvoiceLines[0].JI_InvoiceQuantity = 20m;
				declaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(declaration, MessageSubTypes.Change);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessErrorB3Message(declaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ErrorChange, declaration.B3EntryHeader.CH_Status);
				Factory.Save();

				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Error B3 CUSDEC Response for 000000012"; }));
				AssertContains("Previous Stock Levels have been restored. (WHS Receipt: ", email.Body);
			}
		}

		#endregion

		#region Inward Withdraw

		public void TestProcessInwardClearWithdraw()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(declaration, MessageSubTypes.Create);
				ProcessClearB3Message(declaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				declaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(declaration, MessageSubTypes.Withdraw);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(declaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ClearDelete, declaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Cancellation accepted B3 CUSDEC Response for 000000012"; }));
				AssertContains("Stock Levels Update has been canceled.", email.Body);
			}
		}

		public void TestProcessInwardErrorWithdraw()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(declaration, MessageSubTypes.Create);
				ProcessClearB3Message(declaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				declaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(declaration, MessageSubTypes.Withdraw);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessErrorB3Message(declaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ErrorDelete, declaration.B3EntryHeader.CH_Status);
				Factory.Save();

				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Error B3 CUSDEC Response for 000000012"; }));
				AssertContains("Previous Stock Levels have been restored. (WHS Receipt: ", email.Body);
			}
		}

		#endregion

		#region Outward Original

		public void TestProcessOutwardClearOriginal()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(inwardDeclaration, MessageSubTypes.Create);
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsReceive)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var outwardDeclaration = CreateTestDeclaration("B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, 2m);
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_PreviousEntryNumber = "12345000000012-1";
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Create);
				result = outwardDeclaration.PublishShipmentForWHSOutward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsOrder)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("entryHeader.CH_Status", "AWO", outwardDeclaration.B3EntryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(outwardDeclaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ClearOriginal, outwardDeclaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Accepted B3 CUSDEC Response for 000000023"; }));
				AssertContains("Stock Release can be finalized. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardErrorOriginal()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(inwardDeclaration, MessageSubTypes.Create);
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsReceive)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var outwardDeclaration = CreateTestDeclaration("B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, 2m);
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_PreviousEntryNumber = "12345000000012-1";
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Create);
				result = outwardDeclaration.PublishShipmentForWHSOutward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsOrder)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("entryHeader.CH_Status", "AWO", outwardDeclaration.B3EntryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessErrorB3Message(outwardDeclaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ErrorOriginal, outwardDeclaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);
				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Error B3 CUSDEC Response for 000000023"; }));
				AssertContains("Stock Release has been canceled. (WHS Order: ", email.Body);
			}
		}

		#endregion

		#region Outward Change

		public void TestProcessOutwardClearChange()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(inwardDeclaration, MessageSubTypes.Create);
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsReceive)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var outwardDeclaration = CreateTestDeclaration("B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, 2m);
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_PreviousEntryNumber = "12345000000012-1";
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Create);
				result = outwardDeclaration.PublishShipmentForWHSOutward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsOrder)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("entryHeader.CH_Status", "AWO", outwardDeclaration.B3EntryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(outwardDeclaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				outwardDeclaration.InvoiceLines[0].JI_InvoiceQuantity = 4m;
				outwardDeclaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Change);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 6m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(outwardDeclaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 6m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ClearChange, outwardDeclaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 6m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Accepted B3 CUSDEC Response for 000000023"; }));
				AssertContains("Stock Release has been updated. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardErrorChange()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(inwardDeclaration, MessageSubTypes.Create);
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsReceive)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var outwardDeclaration = CreateTestDeclaration("B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, 2m);
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_PreviousEntryNumber = "12345000000012-1";
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Create);
				result = outwardDeclaration.PublishShipmentForWHSOutward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsOrder)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("entryHeader.CH_Status", "AWO", outwardDeclaration.B3EntryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(outwardDeclaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				outwardDeclaration.InvoiceLines[0].JI_InvoiceQuantity = 4m;
				outwardDeclaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Change);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 6m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessErrorB3Message(outwardDeclaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 6m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ErrorChange, outwardDeclaration.B3EntryHeader.CH_Status);
				Factory.Save();

				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Error B3 CUSDEC Response for 000000023"; }));
				AssertContains("Previous Stock Release has been restored. (WHS Order:", email.Body);
			}
		}

		#endregion

		#region Outward Withdraw

		public void TestProcessOutwardClearWithdraw()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(inwardDeclaration, MessageSubTypes.Create);
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsReceive)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var outwardDeclaration = CreateTestDeclaration("B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, 2m);
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_PreviousEntryNumber = "12345000000012-1";
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Create);
				result = outwardDeclaration.PublishShipmentForWHSOutward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsOrder)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("entryHeader.CH_Status", "AWO", outwardDeclaration.B3EntryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(outwardDeclaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				outwardDeclaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Withdraw);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(outwardDeclaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ClearDelete, outwardDeclaration.B3EntryHeader.CH_Status);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Cancellation accepted B3 CUSDEC Response for 000000023"; }));
				AssertContains("Stock Release has been canceled. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardErrorWithdraw()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = CreateTestDeclaration("B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, 10m);
				SentB3Message(inwardDeclaration, MessageSubTypes.Create);
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsReceive)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var outwardDeclaration = CreateTestDeclaration("B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, 2m);
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_PreviousEntryNumber = "12345000000012-1";
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Create);
				result = outwardDeclaration.PublishShipmentForWHSOutward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				AssertNotNull((Warehouse.Integration.IWhsOrder)result.FindJobIfExists());
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("entryHeader.CH_Status", "AWO", outwardDeclaration.B3EntryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessClearB3Message(outwardDeclaration);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				outwardDeclaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				Factory.Save();
				SentB3Message(outwardDeclaration, MessageSubTypes.Withdraw);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ProcessErrorB3Message(outwardDeclaration);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", MessageStatusList.Codes.ErrorDelete, outwardDeclaration.B3EntryHeader.CH_Status);
				Factory.Save();

				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Error B3 CUSDEC Response for 000000023"; }));
				AssertContains("Stock Release can be finalized. (WHS Order:", email.Body);
			}
		}

		#endregion

		#region Implementation

		JobDeclaration CreateTestDeclaration(ZString declarationReference, ZString sequentialNumber, ZString entryType, ZDecimal quantity)
		{
			var declaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, declarationReference, sequentialNumber, entryType, Helper.Importer, Helper.Warehouse);
			BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(declaration, Helper.Part.OP_PartNum, quantity);
			declaration.DoMerge();
			Factory.Save();
			return declaration;
		}

		void SentB3Message(JobDeclaration declaration, MessageSubTypes actionCode)
		{
			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(declaration.B3EntryHeader));
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(actionCode);
			Factory.Save();
		}

		void ProcessClearB3Message(JobDeclaration declaration)
		{
			string messageText = string.Format(@"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20161012:102'ERP+:I99'RFF+ABO:{0}'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'",
				declaration.TransactionNumber.SequentialNumber + declaration.TransactionNumber.CheckDigit.ToString());

			var ediMessage = GetEDIMessage<B3Message>(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
		}

		void ProcessErrorB3Message(JobDeclaration declaration)
		{
			string messageText = string.Format(@"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20161012:102'ERP+:I99'RFF+ABO:{0}'ERC+942855'ERP+:I99'DOC+961'CST++1+1+0'UNT+9+1'",
				declaration.TransactionNumber.SequentialNumber + declaration.TransactionNumber.CheckDigit.ToString());

			var ediMessage = GetEDIMessage<B3Message>(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			impMessageProcessor = new IMPMessageProcessor(logger);
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			BondedWarehousingHelperTest.CreateWarehouse(Factory, Helper.Warehouse, "WH1");
			BondedWarehousingHelperTest.CreateWarehouse(Factory, Helper.Warehouse2, "WH2");
		}

		IMPMessageProcessor impMessageProcessor;

		WhsDataTestHelper Helper
		{
			get { return helper ?? (helper = new WhsDataTestHelper(Factory)); }
		}
		WhsDataTestHelper helper;

		#endregion
	}
}
