using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(InventoryManagementHelper))]
sealed class InventoryManagementHelperTest : TestCaseWithFactory
{
	#region PDC

	public void TestInventoryManagementAction_PDC_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods();
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithoutSUMdoc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(setCorrectPremisesInHeader: false);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	#endregion

	#region PDC CMP

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods();
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithoutSUMdoc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(setCorrectPremisesInHeader: false);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 2, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, docReference: FormattedDocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 9;
			regLineTransaction3.SRT_GrossWeight = 19.8m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, regHeaderReference: DocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 9;
			regLineTransaction3.SRT_GrossWeight = 19.8m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 9;
			regLineTransaction3.SRT_GrossWeight = 19.8m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = 18m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -1.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = 18m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -1.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers();

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = 18m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -1.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, docReference: FormattedDocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_GrossWeight = 22m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine3, -9, -28m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, regHeaderReference: DocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_GrossWeight = 22m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine3, -9, -28m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_GrossWeight = 22m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine3, -9, -28m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers();
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, docReference: FormattedDocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 9;
			regLineTransaction3.SRT_GrossWeight = 19.8m;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, regHeaderReference: DocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 9;
			regLineTransaction3.SRT_GrossWeight = 19.8m;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 9;
			regLineTransaction3.SRT_GrossWeight = 19.8m;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -19.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -19.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestInventoryManagementAction_PDC_CMP_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers();

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -19.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, AdjustmentTransactionComment);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	#endregion

	#region DVX CMP

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWithout337doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(setCorrectPremisesInHeader: false, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 2, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = 18m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -1.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = 18m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -1.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = 18m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -1.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, docReference: FormattedDocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_GrossWeight = 22m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine3, -9, -28m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, regHeaderReference: DocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_GrossWeight = 22m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine3, -9, -28m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 2m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_GrossWeight = 22m;

			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transaction", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine3, -9, -28m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -19.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -19.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestInventoryManagementAction_DVX_CMP_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, regLineItem) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			regLineTransaction3.SRT_PackageQty = 1;

			var declaration = entryHeader1.Declaration;

			var regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference1;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			var regLineTransaction5 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction5.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 3, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine2, 0, 18m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine3, -8, -19.8m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, AdjustmentTransactionComment);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
			});
		}
	}

	#endregion

	#region PDS

	public void TestInventoryManagementAction_PDS_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods();
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithoutSUMdoc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(setCorrectPremisesInHeader: false);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	#endregion

	public void TestInventoryManagementAction_PDI_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_NothingIsDone()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	#region EXS

	public void TestInventoryManagementAction_EXS_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithoutSUMdoc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(setCorrectPremisesInHeader: false, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeightOBLForVINs: 22m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithXSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, docCode: "XSUM", transactionGrossWeightOBLForVINs: 22m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithXSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m, docCode: "XSUM", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithXSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeightOBLForVINs: 22m, docCode: "XSUM", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, docCode: "N337", transactionGrossWeightOBLForVINs: 22m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m, docCode: "N337", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_EXS_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeightOBLForVINs: 22m, docCode: "N337", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	#endregion

	#region H2

	public void TestInventoryManagementAction_H2_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWithout337doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(setCorrectPremisesInHeader: false, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_NotBulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestInventoryManagementAction_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeightOBLForVINs: 22m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	#endregion

	#region EDP

	public void TestInventoryManagementAction_EDP_TemporaryStorageNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWithout1217doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docInJobDeclaration_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = declaration.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 0 elements", 0, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docInEntryInstruction_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.EntryInstruction.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docInInvoiceHeader_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceHeaders.FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWithMultiple1217docsWithoutQuantity_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";
			supDoc.CSI_UnitOfQuantity = "KGM";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWithMultiple1217docsWithoutUnitOfQuantity_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";
			supDoc.CSI_Quantity = 20m;
			supDoc.CSI_UnitOfQuantity = "AAA";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 0 elements", 0, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(false, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regLineItemNo: 2, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_NotBulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_NotBulk_WithPackagesInSupportingDocuments_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, packageQtyNotBulkInSupportingDocuments: 3);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithGrossWeightError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -34m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			});
		}
	}

	#endregion

	#region EDN

	public void TestInventoryManagementAction_EDN_TemporaryStorageNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWithout1217doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docInJobDeclaration_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = declaration.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 0 elements", 0, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docInEntryInstruction_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.EntryInstruction.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docInInvoiceHeader_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceHeaders.FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWithMultiple1217docsWithoutQuantity_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";
			supDoc.CSI_UnitOfQuantity = "KGM";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWithMultiple1217docsWithoutUnitOfQuantity_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";
			supDoc.CSI_Quantity = 20m;
			supDoc.CSI_UnitOfQuantity = "AAA";

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 1 element", 1, messageBuildersData.Count);
				AssertEquals("List returned has 0 elements", 0, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				AssertEquals("regLine has no new transactions", 1, regLine.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithoutPremises_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(false, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithoutRegLineItem_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regLineItemNo: 2, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithVINError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_NotBulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_NotBulk_WithPackagesInSupportingDocuments_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, packageQtyNotBulkInSupportingDocuments: 3);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_Bulk_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithGrossWeightError_RemoveMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 1 element", 1, messageBuildersDataAfterAction.Count);
				AssertEquals("MessageBuider returned has correct entryHeader", entryHeader2, messageBuildersDataAfterAction[0].EntryHeader);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithGrossWeightForVINsError_KeepMessageBuilder()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestInventoryManagementAction_EDN_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			var messageBuildersData = GetMessageBuildersData(declaration, DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);

			CombineAssertions(() =>
			{
				var messageBuildersDataAfterAction = InventoryManagementHelper.InventoryManagementAction(Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrorsForTest);

				AssertEquals("List before running action has 2 elements", 2, messageBuildersData.Count);
				AssertEquals("List returned has 2 elements", 2, messageBuildersDataAfterAction.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -34m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			});
		}
	}

	#endregion

	const string EntryReference1 = "ES00001";
	const string EntryReference2 = "ES00002";
	const string LocationInEntry = "9999000002";
	const string DocCode = "SUM";
	const string DocReference = "24ES00999980001282";
	const string FormattedDocReference = "99994000128";
	const string AdjustmentTransactionComment = "Adjustment for Complementary Declaration";

	List<MessageBuilderData> GetMessageBuildersData(JobDeclaration declaration, ZString messageType, ZString messageSubType)
	{
		var messageSendingObject = new MessageSendingObject(declaration, staff);
		messageSendingObject.Factory.RefreshEnabled = false;
		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
		foreach (JobDeclarationMessageSendingObject sendingObject in messageSendingObjectParent.SendingObjectsCollection)
		{
			sendingObject.ShouldSend = true;
			sendingObject.MessageSubType = messageSubType;
			sendingObject.MessageType = messageType;
			sendingObject.SecurityFlag = "2";
		}

		var sender = new ESMessageSender(messageSendingObjectParent);
		var messageBuildersData = sender.GetMessageBuildersData();
		return messageBuildersData;
	}

	List<MessageBuilderData> ReserveTemporaryStorageGoodsWhenErrorsForTest(CusEntryHeader entryHeader, ZString errorMessage, ZString errorMessageVINs, IEnumerable<DataToReserveTSGoods> dataToReserve, MessageBuilderData builderData, List<MessageBuilderData> messageBuildersDataToContinue)
	{
		var shouldRemoveMessageBuilder = false;
		if (!errorMessage.IsEmpty)
		{
			shouldRemoveMessageBuilder = true;
		}
		if (!errorMessageVINs.IsEmpty)
		{
			shouldRemoveMessageBuilder = false;
		}

		if (shouldRemoveMessageBuilder)
		{
			messageBuildersDataToContinue.Remove(builderData);
		}

		return messageBuildersDataToContinue;
	}

	(CusEntryHeader entryHeader, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine) SetUpDataForForReserveTemporaryStorageGoods
			(bool shouldAddDoc = true, string docCode = DocCode, string docReference = DocReference, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, string regHeaderReference = FormattedDocReference,
			string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Import, string regLineTransactionInternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, string entryInstructionSubStyle = EntrySubStyleList.Codes.A, string entryInstructionStyle = "")
	{
		var isExportLAME = regLineTransactionInternalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration;

		var declaration = Factory.GetNewJobDeclaration(Staff, Supplier, Importer, Declarant, messageType: messageType, entryInstructionSubStyle1: entryInstructionSubStyle, entryInstructionStyle1: entryInstructionStyle);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		declaration.Factory.Save();
		entryHeader.CH_BGMReference = EntryReference1;

		if (shouldAddDoc)
		{
			var invoiceLine = entryHeader.AllEntryLines[0].RandomLine;

			if (isExportLAME)
			{
				var supportingDoc = invoiceLine.SupportingDocuments.AddNew();
				supportingDoc.CSI_Code = docCode;
				supportingDoc.CSI_ReferenceNumber = docReference;
			}
			else
			{
				var previousDoc = invoiceLine.PreviousDocuments.AddNew();
				previousDoc.CSI_Code = docCode;
				previousDoc.CSI_ReferenceNumber = docReference;
			}
		}

		declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = locationInEntry;

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = isExportLAME ? "LAM" : "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransaction.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;

		return (entryHeader, regLineTransaction, regLine);
	}

	(CusEntryHeader entryHeader1, CusEntryHeader entryHeader2, CusTempStorageRegLineTransaction regLineTransaction1, CusTempStorageRegLineTransaction regLineTransaction2, CusTempStorageRegLine regLine1, CusTempStorageRegLine regLine2, CusTempStorageRegLine regLine3, EU.TemporaryStorage.Business.CusTempStorageRegLineItem regLineItem)
		SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bool setCorrectPremisesInHeader = true, int prevDocLineNo = 1, int regLineItemNo = 1, string packageVin = "VIN1", int packageQtyNotBulk = 9, decimal transactionGrossWeight = 40m, bool isForAmendment = false, string bulkPackageTypeForRegLine = "VG",
		string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Import, string regLineTransactionInternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, string docCode = DocCode, string docReference = DocReference,
		string entryInstructionSubStyle1 = EntrySubStyleList.Codes.A, string entryInstructionSubStyle2 = EntrySubStyleList.Codes.B, string entryInstructionStyle1 = "", string entryInstructionStyle2 = "", string regHeaderReference = FormattedDocReference, decimal transactionGrossWeightOBLForVINs = 5m, int packageQtyNotBulkInSupportingDocuments = 0)
	{
		Factory.SetBulkTypeHelper();
		var isExportLAME = regLineTransactionInternalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration;

		var declaration = Factory.GetNewJobDeclaration(Staff, Supplier, Importer, Declarant, messageType: messageType, createSecondEntry: true, createInvLinesInDifferentInvHeaders: true, entryInstructionSubStyle1: entryInstructionSubStyle1, entryInstructionSubStyle2: entryInstructionSubStyle2, entryInstructionStyle1: entryInstructionStyle1, entryInstructionStyle2: entryInstructionStyle2);
		var entryHeader1 = declaration.CustomsEntryHeaders.First(e => e.EntryInstruction.CEI_SubStyle == entryInstructionSubStyle1);
		var entryHeader2 = declaration.CustomsEntryHeaders.First(e => e != entryHeader1 && e.EntryInstruction.CEI_SubStyle == entryInstructionSubStyle2);
		declaration.Factory.Save();
		entryHeader1.CH_BGMReference = EntryReference1;
		entryHeader2.CH_BGMReference = EntryReference2;

		var invoiceLine = entryHeader1.AllEntryLines[0].RandomLine;
		var vehicle = invoiceLine.Vehicles.AddNew();
		invoiceLine.JI_Weight = 22m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		vehicle.CVH_VehicleIdentificationNumber = "VIN1";

		if (isExportLAME)
		{
			var supportingDoc = invoiceLine.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = docCode;
			supportingDoc.CSI_ReferenceNumber = docReference;
			if (packageQtyNotBulkInSupportingDocuments != 0)
			{
				supportingDoc.CSI_PackQty = packageQtyNotBulkInSupportingDocuments;
				supportingDoc.CSI_PackType = "BX";
			}
		}
		else
		{
			var previousDoc = invoiceLine.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = docCode;
			previousDoc.CSI_ReferenceNumber = docReference;
			previousDoc.CSI_LineNo = prevDocLineNo;
		}

		var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();
		var packageInfo1 = packingGroups.Packages.AddNew();
		packageInfo1.CW_PackQty = 9;
		packageInfo1.CW_PackType = "BX";
		packageInfo1.CW_MarksAndNos = "marks";
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack1);

		var packageInfo2 = packingGroups.Packages.AddNew();
		packageInfo2.CW_PackQty = 0;
		packageInfo2.CW_PackType = "VG";
		packageInfo2.CW_MarksAndNos = "bulk gas marks";
		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 0;
		invoiceLine.PackagesPivot.Add(pack2);

		if (isExportLAME)
		{
			var invoiceHeader = invoiceLine.InvoiceHeader;
			var entryInstruction = invoiceLine.EntryInstruction;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "2203001010";
			invoiceLine2.JI_Weight = 8m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.PackagesPivot.Remove(pack1);
			invoiceLine2.PackagesPivot.Add(pack1);

			var supportingDoc2 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = docCode;
			supportingDoc2.CSI_ReferenceNumber = docReference;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "2203001010";
			invoiceLine3.JI_Weight = 4m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.PackagesPivot.Remove(pack2);
			invoiceLine3.PackagesPivot.Add(pack2);

			var supportingDoc3 = invoiceLine3.SupportingDocuments.AddNew();
			supportingDoc3.CSI_Code = docCode;
			supportingDoc3.CSI_ReferenceNumber = docReference;

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();
		}

		declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = LocationInEntry;
		declaration.CustomsEntryInstructions[1].GoodsLocation.Address.AuthorisationNumber = LocationInEntry;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_Type = isExportLAME ? "LAM" : "ADT";
		premises1.SRP_CustomsLocation = LocationInEntry;
		premises1.SRP_Code = "X";
		premises1.SRP_Description = "DESC";
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;
		var premises2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises2.SRP_Type = isExportLAME ? "LAM" : "ADT";
		premises2.SRP_CustomsLocation = "9999000005";
		premises2.SRP_Code = "A";
		premises2.SRP_Description = "DESC2";
		premises2.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = setCorrectPremisesInHeader ? premises1.PK : premises2.PK;
		var regLine1 = Factory.New<CusTempStorageRegLine>();
		regLine1.SRL_LineNumber = 1;
		regLine1.SRL_CustomsStatus = "OPN";
		regLine1.SRL_PackageType = isForAmendment ? "CT" : "FR";
		regLine1.SRL_PackageMarks = packageVin;
		regLine1.SRL_SRH = regHeader.PK;
		var regLineTransactionPND1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND1.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransactionPND1.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		var regLineTransactionPND2 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND2.SRT_InternalReferenceNumber = EntryReference2;
		regLineTransactionPND2.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransaction1.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		regLineTransaction1.SRT_PackageQty = 10;
		regLineTransaction1.SRT_GrossWeight = transactionGrossWeightOBLForVINs;

		var regLine2 = Factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_CustomsStatus = "OPN";
		regLine2.SRL_PackageType = bulkPackageTypeForRegLine;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransaction2.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		regLineTransaction2.SRT_PackageQty = 10;
		regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

		var regLine3 = Factory.New<CusTempStorageRegLine>();
		regLine3.SRL_LineNumber = 3;
		regLine3.SRL_CustomsStatus = "OPN";
		regLine3.SRL_PackageType = "BX";
		regLine3.SRL_SRH = regHeader.PK;
		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
		regLineTransaction3.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;
		regLineTransaction3.SRT_GrossWeight = 6m;

		var regLineItem = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = regLineItemNo;

		var regLineItemPivot1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

		var regLineItemPivot2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

		var regLineItemPivot3 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

		return (entryHeader1, entryHeader2, regLineTransactionPND1, regLineTransactionPND2, regLine1, regLine2, regLine3, regLineItem);
	}

	void AssertNewTransactionToReserveGoods(CusTempStorageRegLine regLine, ZInt expectedPackQty, ZDecimal expectedGrossWeight, string expectedInternalRefType, string expectedComment = "")
	{
		var lineNum = regLine.SRL_LineNumber;
		var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_GrossWeight == expectedGrossWeight);
		AssertNotNull("Line " + lineNum + " has new transaction", transaction);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceType correct", expectedInternalRefType, transaction.SRT_InternalReferenceType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_PackageQty correct", expectedPackQty, transaction.SRT_PackageQty);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_GrossWeight correct", expectedGrossWeight, transaction.SRT_GrossWeight);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_Comments correct", expectedComment, transaction.SRT_Comments);
	}

	public GlbStaff Staff
	{
		get
		{
			if (staff == null)
			{
				staff = Factory.GetStaffAccount();
			}

			return staff;
		}
	}
	GlbStaff staff;

	public OrgHeader Declarant
	{
		get
		{
			if (declarant == null)
			{
				declarant = Factory.GetNewDeclarant();
			}
			return declarant;
		}
	}
	OrgHeader declarant;

	public OrgHeader Importer
	{
		get
		{
			if (importer == null)
			{
				importer = Factory.GetNewImporter();
			}
			return importer;
		}
	}
	OrgHeader importer;

	public OrgHeader Supplier
	{
		get
		{
			if (supplier == null)
			{
				supplier = Factory.GetNewSupplier();
			}
			return supplier;
		}
	}
	OrgHeader supplier;
}
