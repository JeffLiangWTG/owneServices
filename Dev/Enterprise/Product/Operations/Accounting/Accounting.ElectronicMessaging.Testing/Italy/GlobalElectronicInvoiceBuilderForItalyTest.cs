using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	class GlobalElectronicInvoiceBuilderForItalyTest : TestCaseWithFactory
	{
		public void TestGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			var batchAR = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivotAR = ObjectCreator.CreateEInvoicingTransactionPivot(batchAR, arInvoice, Core.Constants.EInvoicingPivotState.Batched);

			var batchAP = ObjectCreator.CreateEInvoicingBatch(101, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var apInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivotAP = ObjectCreator.CreateEInvoicingTransactionPivot(batchAP, apInvoice, Core.Constants.EInvoicingPivotState.Batched);

			Factory.Save();

			var dataAccessAR = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exporterAR = new TransactionBatchExporter(dataAccessAR, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatchAR = exporterAR.CreateTransactionBatch(batchAR);

			SetTransactionInfoForRegistrationNumber(transactionBatchAR, true, false, false);
			var (einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAR.AIB_BatchNumber.ToString(), transactionBatchAR).Create();
			AssertGEIHeaderFields(batchAR, "IT00000000002CWEAR");

			SetTransactionInfoForRegistrationNumber(transactionBatchAR, true, true, false);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAR.AIB_BatchNumber.ToString(), transactionBatchAR).Create();
			AssertGEIHeaderFields(batchAR, "IT00000000002CWEAR");

			SetTransactionInfoForRegistrationNumber(transactionBatchAR, false, true, false);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAR.AIB_BatchNumber.ToString(), transactionBatchAR).Create();
			AssertGEIHeaderFields(batchAR, "IT0000000000000003");

			SetTransactionInfoForRegistrationNumber(transactionBatchAR, false, false, true);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAR.AIB_BatchNumber.ToString(), transactionBatchAR).Create();
			AssertGEIHeaderFields(batchAR, "IT000000000002");

			SetTransactionInfoForRegistrationNumber(transactionBatchAR, false, false, false);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAR.AIB_BatchNumber.ToString(), transactionBatchAR).Create();
			AssertGEIHeaderFields(batchAR, "IT");

			var dataAccessAP = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exporterAP = new TransactionBatchExporter(dataAccessAP, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatchAP = exporterAP.CreateTransactionBatch(batchAP);

			SetTransactionInfoForRegistrationNumber(transactionBatchAP, true, false, false);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAP.AIB_BatchNumber.ToString(), transactionBatchAP).Create();
			AssertGEIHeaderFields(batchAP, "IT00000000002CWEAP");

			SetTransactionInfoForRegistrationNumber(transactionBatchAP, true, true, false);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAP.AIB_BatchNumber.ToString(), transactionBatchAP).Create();
			AssertGEIHeaderFields(batchAP, "IT00000000002CWEAP");

			SetTransactionInfoForRegistrationNumber(transactionBatchAP, false, true, false);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAP.AIB_BatchNumber.ToString(), transactionBatchAP).Create();
			AssertGEIHeaderFields(batchAP, "IT0000000000000003");

			SetTransactionInfoForRegistrationNumber(transactionBatchAP, false, false, true);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAP.AIB_BatchNumber.ToString(), transactionBatchAP).Create();
			AssertGEIHeaderFields(batchAP, "IT000000000002");

			SetTransactionInfoForRegistrationNumber(transactionBatchAP, false, false, false);
			(einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForItaly(batchAP.AIB_BatchNumber.ToString(), transactionBatchAP).Create();
			AssertGEIHeaderFields(batchAP, "IT");

			void SetTransactionInfoForRegistrationNumber(UniversalTransactionBatch transactionBatch, bool useIva, bool useCodiceFiscale, bool useWrongLengthIva)
			{
				var transactionInfo = transactionBatch.TransactionCollection[0];
				transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfo.BranchAddress.Address1 = "Ardenham Court";
				transactionInfo.BranchAddress.Address2 = "Oxford Road";
				transactionInfo.BranchAddress.City = "Aylesbury";
				transactionInfo.BranchAddress.State = "BM";
				transactionInfo.BranchAddress.Postcode = "HP19 3EQ";
				transactionInfo.BranchAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedKingdom };
				transactionInfo.BranchAddress.CompanyName = "My United Kingdom Company";
				transactionInfo.BranchAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
				if (useIva)
				{
					transactionInfo.BranchAddress.RegistrationNumberCollection.Add(CessionarioCommittenteTest.CreateOrgRegistrationNumberIVA());
				}
				if (useCodiceFiscale)
				{
					transactionInfo.BranchAddress.RegistrationNumberCollection.Add(CessionarioCommittenteTest.CreateOrgRegistrationNumberCodiceFiscale());
				}
				if (useWrongLengthIva)
				{
					transactionInfo.BranchAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberWrongLengthIVA());
				}
			}

			void AssertGEIHeaderFields(AccEInvoicingBatch batch, string issuerRegistrationNumberValueExpected)
			{
				AssertEquals("MessagingSystem", "Italy electronic invoicing system", einvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", einvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString(), einvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);

				AssertEquals("AdditionalDataItems", 1, einvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count);
				AssertEquals("IssuerRegistrationNumber_key", "IssuerRegistrationNumber", einvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Key);
				var issuerRegistrationNumberValue = einvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Value;
				AssertEquals("IssuerRegistrationNumber_value", issuerRegistrationNumberValueExpected, issuerRegistrationNumberValue);

				AssertNotEquals("Payload is not Empty", string.Empty, einvoice.Payload);
			}
		}

		public void TestCDATATagWithNewlineCharacterReturnTrueWhenIsMatchTested()
		{
			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = ObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exportor = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exportor.CreateTransactionBatch(batch);

			transactionBatch.TransactionCollection.Clear();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Address1 = "10 MAIN STREET\nSYDNEY NSW"
				},
				Branch = new Branch { Code = GlbCompany.CurrentCompany.FirstActiveBranchCode }
			};

			transactionBatch.TransactionCollection.Add(transactionInfo);

			AssertNoExceptionThrown(() => new GlobalElectronicInvoiceBuilderForItaly(batch.AIB_BatchNumber.ToString(), transactionBatch).Create());
		}

		public void TestGEIMessageIsProductionSystem()
		{
			// Arrange
			var defaultRegistryValue = AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.Value;
			AssertEquals("Precondition", AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, defaultRegistryValue);

			var transactionBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch { Code = GlbCompany.CurrentCompany.FirstActiveBranchCode }
			});

			var globalBuilder = new GlobalElectronicInvoiceBuilderForItaly("12345", transactionBatch);

			// Assert
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, DatabaseTypes.Codes.Training, true, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, DatabaseTypes.Codes.Production, false, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Production, true, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Test, false, globalBuilder);

			void ExecuteWithTemporaryRegistryValue(Guid companyPk, string registryCode, string licenceType, bool expectedValue, IGlobalElectronicInvoiceBuilder globalBuilder)
			{
				LicenceTypeChanger.SetSystemLicence(licenceType);
				using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, registryCode))
				{
					var (eInvoice, _, _) = globalBuilder.Create();
					AssertEquals(expectedValue, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();
		}

		EInvoicingTestHelper Helper
		{
			get { return helper ?? (helper = new EInvoicingTestHelper(ObjectCreator)); }
		}
		EInvoicingTestHelper helper;

		TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumberWrongLengthIVA(string countryCode = Core.Constants.CountryCodes.Italy)
		{
			return CessionarioCommittenteTest.CreateOrgRegistrationNumber(OrgCusCode.CodeTypes.IVA, countryCode, "000000000002");
		}
	}
}
