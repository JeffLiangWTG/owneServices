using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using ClassificationTypes = Enterprise.Customs.CA.Business.ClassificationTypeList;
using Constants = Enterprise.Core.Constants;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;
using EntryChargeTypeList = Enterprise.Customs.CA.Registry.EntryChargeTypeList;
using IManualCancelSupport = Enterprise.Integration.Customs.CA.IManualCancelSupport;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	public class JobDeclarationTest : BaseJobDeclarationTest<JobDeclaration>
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
			helper = new UniversalReferenceTestDataHelper(Factory);
		}

		UniversalReferenceTestDataHelper helper;

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsEnableACROSSValidationForTest
			{
				get => isEnableACROSSValidation;
			}

			public bool IsEnableB3ValidationForTest
			{
				get => isEnableB3Validation;
			}
		}

		public void TestCA_MergeBy()
		{
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CarmR2, Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;
				AssertEquals(B3MergeByList.Codes.NotMerge, declaration.CA_MergeBy);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;
				AssertEquals(B3MergeByList.Codes.ClassificationTariff, declaration.CA_MergeBy);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;
				AssertEquals(B3MergeByList.Codes.ClassificationTariff, declaration.CA_MergeBy);
			}
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var declaration = Factory.New<JobDeclaration>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(declaration,
				"CAJobDeclaration",
				schemaTypeName: nameof(AutoJobDeclaration.Schema));
		}

		public void TestGetNewCopyToPRECARMAdjustmentDeclaration()
		{
			var (previousJob, newDeclaration) = CreatePRECARMAdjustmentDeclaration();
			CombineAssertions(() =>
			{
				AssertNotNull("PRECARM Adjustment Declaration", newDeclaration);
				AssertEquals(previousJob.PK, newDeclaration.ParentRelatedDeclaration.PK);
				AssertEquals("10207500000001", newDeclaration.CA_OriginalTransactionNo);
				AssertEquals(B3MergeByList.Codes.NotMerge, newDeclaration.CA_MergeBy);

				var invoices = newDeclaration.Invoices;
				AssertEquals("Copied Invoices", 1, invoices.Count);
				AssertEquals("InvoiceNumber", "1", invoices[0].JZ_InvoiceNumber);
				AssertEquals("ValuationDateOverride", new ZDateTime(1998, 05, 25), invoices[0].JZ_ValuationDateOverride);
				AssertEquals("Origin", "CA", invoices[0].JZ_RN_NKDefaultOrigin);
				AssertEquals("Currency", "CAD", invoices[0].JZ_RX_NKInvoice_Currency);
				AssertEquals("Supplier", previousJob.Invoices[0].JZ_OH_Supplier, invoices[0].JZ_OH_Supplier);
				AssertEquals("TradeZone", "USC", invoices[0].CA_TradeZone);
				AssertEquals("Export Country", Constants.CountryCodes.UnitedStates, invoices[0].CA_RN_NKExport);
				AssertEquals("Export State", USStatesList.Codes.SouthCarolina, invoices[0].CA_USStateOfExport);
				AssertEquals("USPortOfExit", "2813", invoices[0].CA_USPortOfExit);
				AssertEquals("TreatmentCode", "C3", invoices[0].CA_TreatmentCode);
				AssertEquals("TimeLimitCode", "C2", invoices[0].CA_TimeLimitCode);
				AssertEquals("TimeLimit", 1, invoices[0].CA_TimeLimit);
				AssertEquals("ValueForDutyCode", "T1", invoices[0].CA_ValueForDutyCode);

				var lines = invoices[0].JobComInvoiceLines;
				AssertEquals(1, lines.Count);
				AssertEquals("LineNo", (ZShort)1, lines[0].JI_LineNo);
				AssertEquals("CA", lines[0].JI_CountryOfOrigin);
				AssertEquals("AB", lines[0].JI_StateOrRegionOfOrigin);
				AssertEquals("1234567890", lines[0].JI_Tariff);
				AssertEquals(5m, lines[0].JI_LinePrice);
				AssertEquals("CustomsUnitQty", "NMB", lines[0].JI_CustomsUnitQty);
				AssertEquals("CustomsQuantity", 10m, lines[0].JI_CustomsQuantity);
				AssertEquals("KG", lines[0].JI_InvoiceUQ);
				AssertEquals(3m, lines[0].JI_InvoiceQuantity);
				AssertEquals(0, lines[0].CA_B3SubHeaderNumber);
				AssertEquals("C3", lines[0].CA_TreatmentCode);
				AssertEquals("C1", lines[0].CA_ValueForDutyCode);
				AssertEquals("C2", lines[0].CA_99TariffCode);
				AssertEquals("123", lines[0].CA_AuthorityNumber);
				AssertEquals("TRS", lines[0].CA_TRSNumber);
				AssertEquals(0, lines[0].CA_PageNumber);
				AssertEquals(5m, lines[0].CA_CVforCurrConv);
				AssertEquals("D", lines[0].CA_CalculationMethod);
				AssertEquals(0, lines[0].InvoiceCrossReferencePageLineNumber);
				AssertEquals(0, lines[0].B3SubHeaderNumberForLVX);

				var dutiesAndTaxes = lines[0].DutiesAndTaxes;
				AssertEquals(2, dutiesAndTaxes.Count);
				AssertEquals(true, dutiesAndTaxes[0].C1_Override);
				AssertEquals("GST", dutiesAndTaxes[0].C1_TaxType);
				AssertEquals("001", dutiesAndTaxes[0].C1_Code);
				AssertEquals("AG", dutiesAndTaxes[0].C1_UnitOfMeasure);
				AssertEquals(SIMACodes.Codes.C31, dutiesAndTaxes[0].C1_ExemptCode);
				AssertEquals(0m, dutiesAndTaxes[0].C1_Rate);
				AssertEquals(100m, dutiesAndTaxes[0].C1_Amount);

				AssertEquals(true, dutiesAndTaxes[1].C1_Override);
				AssertEquals("ADD", dutiesAndTaxes[1].C1_TaxType);
				AssertEquals("002", dutiesAndTaxes[1].C1_Code);
				AssertEquals(ZString.Empty, dutiesAndTaxes[1].C1_UnitOfMeasure);
				AssertEquals(ZString.Empty, dutiesAndTaxes[1].C1_ExemptCode);
				AssertEquals(0m, dutiesAndTaxes[1].C1_Rate);
				AssertEquals(1m, dutiesAndTaxes[1].C1_Amount);
			});
		}

		public void TestHasPRECARMAdjustmentDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.HasPRECARMAdjustmentDeclaration());
			var (previousJob, newDeclaration) = CreatePRECARMAdjustmentDeclaration();
			previousJob.RelatedDeclarations.Add(dec);
			AssertEquals(true, previousJob.HasPRECARMAdjustmentDeclaration());
			newDeclaration.IsCancelled = true;
			AssertEquals(false, previousJob.HasPRECARMAdjustmentDeclaration());
		}

		public (JobDeclaration previousJob, JobDeclaration newDeclaration) CreatePRECARMAdjustmentDeclaration()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			var previousJob = Factory.New<JobDeclaration>();
			previousJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			previousJob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			previousJob.TransactionNumber.AccountSecurityCode = "10207";
			previousJob.TransactionNumber.SequentialNumber = "50000000";

			var invoice = previousJob.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(1998, 05, 25);
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.CA_TradeZone = "USC";
			invoice.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			invoice.CA_USPortOfExit = "2813";
			invoice.CA_TimeLimitCode = "C2";
			invoice.CA_TimeLimit = 1;
			invoice.CA_ValueForDutyCode = "T1";

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LineNo = 1;
			line.JI_CountryOfOrigin = "CA";
			line.JI_StateOrRegionOfOrigin = "AB";
			line.JI_Tariff = "1234567890";
			line.JI_LinePrice = 5m;
			line.JI_CustomsUnitQty = "NMB";
			line.JI_CustomsQuantity = 10m;
			line.JI_InvoiceUQ = "KG";
			line.JI_InvoiceQuantity = 3m;
			line.CA_TreatmentCode = "C3";
			line.CA_ValueForDutyCode = "C1";
			line.CA_99TariffCode = "C2";
			line.CA_AuthorityNumber = "123";
			line.CA_TRSNumber = "TRS";
			line.CA_CalculationMethod = "D";

			var tax1 = line.DutiesAndTaxes.AddNew();
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax1.C1_Code = "001";
			tax1.C1_ExemptCode = SIMACodes.Codes.C31;
			tax1.C1_Rate = 5;
			tax1.C1_Amount = 100;
			tax1.C1_UnitOfMeasure = "AG";

			var tax2 = line.DutiesAndTaxes.AddNew();
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			tax2.C1_Code = "002";
			tax2.C1_Rate = 3;
			tax2.C1_Amount = 1;
			previousJob.DoMerge();

			var newDeclaration = previousJob.GetNewCopyToPRECARMAdjustmentDeclaration();
			Factory.Save();
			return (previousJob, newDeclaration);
		}

		public void TestGetSentCADMessageCountInThisVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var message1 = Factory.New<CADMessage>();
			message1.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-1);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message1.EM_MessageText = "<ApplicationReferenceID>1328400800005200001001</ApplicationReferenceID>";
			entry.Messages.Add(message1);
			var message2 = Factory.New<CADMessage>();
			message2.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-2);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message2.EM_MessageText = "<ApplicationReferenceID>1328400800005200001002</ApplicationReferenceID>";
			entry.Messages.Add(message2);
			Factory.Save();

			AssertEquals(2, declaration.GetSentCADMessageCountInThisVersion("00001"));

			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals(0, declaration.GetSentCADMessageCountInThisVersion("00001"));
		}

		public void TestSentCADMessageCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var message1 = Factory.New<EDIMessage>();
			message1.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-1);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			entry.Messages.Add(message1);
			var message2 = Factory.New<EDIMessage>();
			message2.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-2);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			entry.Messages.Add(message2);
			Factory.Save();

			AssertEquals(2, declaration.SentCADMessageCount);

			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals(0, declaration.SentCADMessageCount);
		}

		public void TestIsCADEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				AssertEquals(false, declaration.IsCADEnabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				AssertEquals(true, declaration.IsCADEnabled);
			}

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				AssertEquals(false, declaration.IsCADEnabled);
				entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				AssertEquals(true, declaration.IsCADEnabled);
			}

			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				AssertEquals(false, declaration.IsCADEnabled);
				entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				AssertEquals(true, declaration.IsCADEnabled);
			}
		}

		public void TestCreditRestrictionMessageCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Submit message with credit restriction", declaration.CreditRestrictionMessageCaption);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("Add CLVS to F-Type with Credit Restriction", declaration.CreditRestrictionMessageCaption);
		}

		public void TestBrokerBusinessNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Branch.Company.OrgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "123BRM456");
			AssertEquals("123BRM456", declaration.BrokerBusinessNumber);

			declaration.Branch.OrgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "456BRM456");
			AssertEquals("456BRM456", declaration.BrokerBusinessNumber);

			declaration.Branch.OrgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "789BRM789");
			AssertEquals("789BRM789", declaration.BrokerBusinessNumber);
		}

		public void TestEnableAttachCommercialInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals(false, declaration.EnableAttachCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(false, declaration.EnableAttachCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.EnableAttachCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals(true, declaration.EnableAttachCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.EnableAttachCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals(false, declaration.EnableAttachCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals(true, declaration.EnableAttachCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(true, declaration.EnableAttachCommercialInvoice);
		}

		public override void TestEnableCopyCommercialInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals(false, declaration.EnableCopyCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals(true, declaration.EnableCopyCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.EnableCopyCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.EnableCopyCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals(false, declaration.EnableCopyCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(false, declaration.EnableCopyCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals(true, declaration.EnableCopyCommercialInvoice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(true, declaration.EnableCopyCommercialInvoice);
		}

		public override void TestEnableCommercialInvoiceMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals(false, declaration.EnableCommercialInvoiceMenuItem);

			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals(true, declaration.EnableCommercialInvoiceMenuItem);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.EnableCommercialInvoiceMenuItem);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.EnableCommercialInvoiceMenuItem);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals(false, declaration.EnableCommercialInvoiceMenuItem);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(false, declaration.EnableCommercialInvoiceMenuItem);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals(true, declaration.EnableCommercialInvoiceMenuItem);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(true, declaration.EnableCommercialInvoiceMenuItem);
		}

		public void TestSetupNonPersistentPropertiesWhenMessageTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.IsEnableACROSSValidation);
			Assert(!declaration.IsEnableB3Validation);
			declaration.IsEnableACROSSValidation = true;
			declaration.IsEnableB3Validation = true;
			Assert(declaration.IsEnableACROSSValidation);
			Assert(declaration.IsEnableB3Validation);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(!declaration.IsEnableACROSSValidation);
			Assert(!declaration.IsEnableB3Validation);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(declaration.IsEnableACROSSValidation);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			Assert(declaration.IsEnableB3Validation);
		}

		public void TestRefreshInvoiceLinesDutyAndTaxesWhenMessageTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLineForTesting)invoice.InvoiceLines.AddNew(typeof(JobComInvoiceLineForTesting));
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertNotNull(invoiceLine.dutiesAndTaxesExposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNull(invoiceLine.dutiesAndTaxesExposed);
		}

		public void TestCADeclarationSetupCorrectly()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			var caDeclaration = declaration.CADeclaration;
			AssertEquals(declaration.PK, caDeclaration.CAD_JE);
			AssertEquals(true, caDeclaration.IsInDatabase);
			AssertEquals(1, caDeclaration.CAD_ClusterKey);
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			caDeclaration = declaration.CADeclaration;
			AssertEquals(declaration.PK, caDeclaration.CAD_JE);
			AssertEquals(true, caDeclaration.IsInDatabase);
			AssertEquals(1, caDeclaration.CAD_ClusterKey);
			declaration.Delete();
			AssertEquals(true, caDeclaration.IsDeleted);
			declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			caDeclaration = declaration.CADeclaration;
			AssertEquals(declaration.PK, caDeclaration.CAD_JE);
			AssertEquals(2, caDeclaration.CAD_ClusterKey);
		}

		public void TestIsCFIAAccountNumberRequired()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "CFW1";
			importer.OH_FullName = "CFIAWarning";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CACustomsDataRegistry.Instance.DefaultCFIAFeePaymentMethod.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, CFIAPaymentMethods.Codes.Broker);
			var addinfo = OrgImpAddInfo.Get(importer);
			addinfo.ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Broker;
			AssertEquals(CFIAPaymentMethods.Codes.Broker, addinfo.ZO_EffectiveCFIAFeePaymentMethod);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Required - CFIAFeePaymentMethod is a Broker", true, declaration.IsCFIAAccountNumberRequired);

			var company = GlbCompany.GetCurrentCompany(Factory);
			var orgProxy = company.OrgProxy;
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CFIAAccountNumber, "SALSABACHTA", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Not Required - Org has a CFIA Number", false, declaration.IsCFIAAccountNumberRequired);
		}

		public void TestSetupNonPersistentPropertiesWhenBranchChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.IsEnableACROSSValidation);
			Assert(!declaration.IsEnableB3Validation);
			using (CACustomsDataRegistry.Instance.AlwaysEnableACROSSMessageValidation.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, true))
			{
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert(declaration.IsEnableACROSSValidation);
			}
			using (CACustomsDataRegistry.Instance.AlwaysEnableB3MessageValidation.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, true))
			{
				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert(declaration.IsEnableB3Validation);
			}
		}

		public void TestLazyInitialisation()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			Assert(!declaration.IsEnableACROSSValidationForTest);
			Assert(!declaration.IsEnableB3ValidationForTest);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(declaration.IsEnableACROSSValidation);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			Assert(declaration.IsEnableB3Validation);
		}

		public void TestSupplierAndImporterDefaultAddress()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var org1 = helper.CreateOrganisation("DPT", "DEPOT NAME", "CATOR", "DEPOT ADDRESS", "DEPOT CITY", "123 4567");
			var org1Address2 = org1.Addresses.AddNew();
			org1Address2.OA_CompanyNameOverride = "DEPOT NAME2";
			org1Address2.OA_Address1 = "DEPOT ADDRESS2";
			org1Address2.OA_City = "DEPOT CITY2";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_ImporterAddress_ZAddress.OrgPK = org1.PK;
			AssertEquals(org1.MainAddress.PK, declaration.JE_OA_ImporterAddress);
			AssertEquals(org1.PK, declaration.JE_OH_Importer);
			declaration.JE_OA_ImporterAddress_ZAddress.OrgPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, declaration.JE_OA_ImporterAddress);
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Importer);

			declaration.JE_OA_SupplierAddress_ZAddress.OrgPK = org1.PK;
			AssertEquals(org1.MainAddress.PK, declaration.JE_OA_SupplierAddress);
			AssertEquals(org1.PK, declaration.JE_OH_Supplier);
			declaration.JE_OA_SupplierAddress_ZAddress.OrgPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, declaration.JE_OA_SupplierAddress);
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Supplier);
		}

		public void TestAddAddInfoPropertyToTrigger()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_B2SubmissionDate = ZDateTime.Empty;
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "CA_B2SubmissionDate";

			action.PQ_FieldValue = "123";
			AssertHasWarning(action.PQ_FieldValueInfo,
				"Property Enterprise.Customs.CA.Business.JobDeclaration.CA_B2SubmissionDate of type ZDateTime cannot be set with value '123' in 'Set Field' trigger action.\r\nCannot initialise a CargoWise.Types.ZDateTime with <123> (CargoWise.Types.ZString).");

			action.PQ_FieldValue = "2020-04-15";
			AssertNoErrors(action.PQ_FieldNameInfo);
			AssertNoWarnings(action.PQ_FieldValueInfo);
			Factory.Save();

			AssertNoNotifications(action.PQ_FieldNameInfo);
			AssertEquals(typeof(JobDeclaration), action.Parent.GetCountrySpecificTypeIfApplicable());
			AssertEquals("defaultValue", ZDateTime.Empty, declaration.CA_B2SubmissionDate);

			declaration.Logs.AddNew(Events.CustomisableEvent01); // This should trigger ImmediateFieldChange without running LogWalker
			Factory.Save();

			AssertEquals("Trigger set value to declaration", new ZDateTime(2020, 04, 15), declaration.CA_B2SubmissionDate);
			AssertEquals(false, trigger.HasRowWarnings);
		}

		public void TestIUniversalCopySelectivelySupportableMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals(false, ((IUniversalCopySelectivelySupportable)declaration).SupportsUniversalCopy);
			AssertEquals("Universal Copy not allowed for B2, IM2 and B3X jobs.", ((IUniversalCopySelectivelySupportable)declaration).ReasonForNotSupportingUniversalCopy);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals(false, ((IUniversalCopySelectivelySupportable)declaration).SupportsUniversalCopy);
			AssertEquals("Universal Copy not allowed for B2, IM2 and B3X jobs.", ((IUniversalCopySelectivelySupportable)declaration).ReasonForNotSupportingUniversalCopy);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(false, ((IUniversalCopySelectivelySupportable)declaration).SupportsUniversalCopy);
			AssertEquals("Universal Copy not allowed for B2, IM2 and B3X jobs.", ((IUniversalCopySelectivelySupportable)declaration).ReasonForNotSupportingUniversalCopy);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, ((IUniversalCopySelectivelySupportable)declaration).SupportsUniversalCopy);
		}

		public override void TestSupportScreeningPresentation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Assert(!declaration.SupportScreeningPresentation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			Assert(!declaration.SupportScreeningPresentation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert(!declaration.SupportScreeningPresentation);
		}

		public void TestShouldCalculateDutiesOnMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			declaration.ResetShouldCalculateDutiesOnMergeForTest();
			declaration.ApportionmentDirty = false;

			declaration.DoMerge();
			Assert(declaration.ShouldCalculateDutiesOnMerge);
		}

		public void TestFetchHintsDoMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ClusterKey = 2;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			AssertEquals("CusHouseContPackInvoiceLinePivotSchema", 0, Factory.ActiveFetchHintsForTable(CusHouseContPackInvoiceLinePivotSchema.Constants.TableName));
			AssertEquals("CusHouseContPackInvoiceLinePivotSchema", 0, Factory.ActiveFetchHintsForTable(CusDecHouseContainerPackSchema.Constants.TableName));

			declaration.DoMerge();
			AssertEquals("CusHouseContPackInvoiceLinePivotSchema", 1, Factory.ActiveFetchHintsForTable(CusHouseContPackInvoiceLinePivotSchema.Constants.TableName));
			AssertEquals("CusHouseContPackInvoiceLinePivotSchema", 1, Factory.ActiveFetchHintsForTable(CusDecHouseContainerPackSchema.Constants.TableName));
		}

		public void TestDefaultServiceToIIDForNotPersistent()
		{
			var creator = new FakeDeclarationCreatorForInvoice(Factory.New<JobComInvoiceHeader>());
			var declaration = (JobDeclaration)creator.HeaderData;
			CombineAssertions(() =>
			{
				AssertEquals("Persistent", false, declaration.IsPersistent);
				AssertNotEquals("Not IID", ACROSSServiceOptions.Codes.IID, declaration.CA_ServiceOption);
				declaration.DefaultValueForFakeDeclaration();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("IS IID", ACROSSServiceOptions.Codes.IID, declaration.CA_ServiceOption);
			});
		}

		public void TestDefaultServiceToIIDForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, declaration.CA_ServiceOption);
		}

		public void TestDefaultServiceToIIDForLVS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(ZString.Empty, declaration.CA_ServiceOption);
		}

		public void TestDefaultServiceToIIDForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ACROSSServiceOptions.Codes.IID, declaration.CA_ServiceOption);
		}

		public void TestDefaultServiceOptionForImportServiceOptionNotEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ACROSSServiceOptions.Codes.PARS, declaration.CA_ServiceOption);
		}

		public void TestWillSetServiceOptionBackToEmptyForFakeDec()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var creater = new FakeDeclarationCreatorForInvoice(invoice);
			var dec = creater.HeaderData as JobDeclaration;
			CombineAssertions("For Fake Dec", () =>
			{
				AssertEquals(false, dec.IsPersistent);
				AssertEquals("", dec.CA_ServiceOption);
				dec.JE_MessageType = "IMP";
				AssertEquals("IID", dec.CA_ServiceOption);
				dec.JE_MessageType = "EXP";
				AssertEquals("", dec.CA_ServiceOption);

				dec.JE_MessageType = "LVS";
				AssertEquals("", dec.CA_ServiceOption);

				dec.JE_MessageType = "LVX";
				AssertEquals("", dec.CA_ServiceOption);

				dec.JE_MessageType = "IM2";
				AssertEquals("IID", dec.CA_ServiceOption);
				dec.JE_MessageType = "EXP";
				AssertEquals("", dec.CA_ServiceOption);

				dec.JE_MessageType = "IMP";
				AssertEquals("IID", dec.CA_ServiceOption);
				dec.CA_ServiceOption = "BLA";
				dec.JE_MessageType = "EXW";
				AssertEquals("", dec.CA_ServiceOption);
			});
		}

		public void TestRefreshActualPacksOnInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();

			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();
			var package = packageGroup.Packages.AddNew();
			package.CW_PackQty = 20;

			AssertEquals("Precondition", 0m, invoice1.JZ_NoOfPacks);
			AssertEquals("Precondition", 0m, invoice2.JZ_NoOfPacks);

			var headerPackageCollection = (InvoiceHeaderCusLinkPackageCollection)invoice1.PackagesForInvoicesForBindingOnly;
			var headerLinkPackage = headerPackageCollection[0];
			headerLinkPackage.Package = package;
			headerLinkPackage.IsLinked = true;

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine2.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage = linePackageCollection[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = true;

			headerLinkPackage.PackQty = 5;
			lineLinkPackage.PackQty = 3;

			declaration.DoMerge();
			AssertEquals("Should update the total packs count from the headerLinkPackage.", 5m, invoice1.JZ_NoOfPacks);
			AssertEquals("Should update the total packs count from the lineLinkPackage.", 3m, invoice2.JZ_NoOfPacks);

			invoice1.JZ_NoOfPacks = 10;
			invoice2.JZ_NoOfPacks = 15;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.DoMerge();
			AssertEquals("Should not update the total packs count as the declaration doesn't support package.", 10m, invoice1.JZ_NoOfPacks);
			AssertEquals("Should not update the total packs count as the declaration doesn't support package.", 15m, invoice2.JZ_NoOfPacks);
		}

		public void TestWarningUserWhenChangeSubmittedIM2B2OrB3XJobs()
		{
			var message1 = @"This B2 has been already submitted for Review – hence no changes are allowed to the data that is printed on the B2.
 If the current changes are of the administrative nature, something that would NOT cause the reprint of the B2 to produce different results, please proceed with saving, otherwise please do not save these changes";
			var message2 = @"This IM2 has been already submitted for Review – hence no changes are allowed to the data that is printed on the IM2.
 If the current changes are of the administrative nature, something that would NOT cause the reprint of the IM2 to produce different results, please proceed with saving, otherwise please do not save these changes";
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var messages = new SendsMessagesToCustomsShutterUpperer();
			b2.MessageInitiator = messages;
			Factory.Save();

			AssertNull(messages.Warning);
			b2.CA_B2SubmissionDate = ZDateTime.Now;
			Factory.Save();
			AssertNull(messages.Warning);

			b2.CA_ChequeNo = "123";
			Factory.Save();
			AssertEquals(message1, messages.Warning);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = "TRF";
			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			declaration.CA_IsDocAttached = true;
			declaration.CA_JustificationForRequest = "JUSTIFICATION FOR REQUEST";
			declaration.CA_Under = "UNDER";
			declaration.CA_B2Explanation = "EXPLANATION";
			declaration.CA_ClaimedInterestAmount = 11m;
			declaration.CA_AnySightDepositAmount = 12m;
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "MY COMPANY";
			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MY BRANCH";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_WorkPhone = "MY PHONE";
			staff.GS_FullName = "MY NAME";
			staff.GS_GB_HomeBranch = branch.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_CVforCurrConv = 1000m;
			line1.JI_Tariff = "0000000001";
			var sima = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 500m;
			var duty = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 50m;
			var excise = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 300m;
			var gst = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 60m;

			messages = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = messages;
			Factory.Save();

			AssertNull(messages.Warning);
			im2.CA_ConfirmedDate = ZDateTime.Now;
			Factory.Save();
			AssertNull(messages.Warning);

			im2.CA_ChequeDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals(message2, messages.Warning);
		}

		[TestDate(2024, 01, 15)]
		public void TestReleaseDateForFtypeCADsInJAN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var result = new ZDateTime(2024, 01, 31);
			AssertEquals(result.Month, declaration.JE_PeriodMonth);
			AssertEquals("Defaulting value for the last day of this month is 2024-01-31", result, declaration.JE_EntryAuthorisationDate);
		}

		[TestDate(2024, 02, 15)]
		public void TestReleaseDateForFtypeCADsInFEB()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var result = new ZDateTime(2024, 02, 29);
			AssertEquals(result.Month, declaration.JE_PeriodMonth);
			AssertEquals("Defaulting value for the last day of this month is 2024-02-29", result, declaration.JE_EntryAuthorisationDate);
		}

		[TestDate(2024, 03, 15)]
		public void TestReleaseDateForFtypeCADsInMAR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var result = new ZDateTime(2024, 03, 31);
			AssertEquals(result.Month, declaration.JE_PeriodMonth);
			AssertEquals("Defaulting value for the last day of this month is 2024-03-31", result, declaration.JE_EntryAuthorisationDate);
		}

		public void TestManualReleaseReason()
		{
			try
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var manualReleaseSupport = declaration as Integration.Customs.CA.IManualReleaseSupport;
				var reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				AssertEquals("Entries for this job have not been generated, please run the Brokerage->Generate Entries (Merge) menu item and try again.", reasonForCannotManualRelease);

				var header = declaration.CustomsEntryHeaders.AddNew();
				header.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2018, 01, 18);
				reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				AssertEquals("You don't need to manually release for a released job.", reasonForCannotManualRelease);

				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

				entryHeader.CH_EntryReleaseDate = new ZDateTime(2020, 8, 19);
				AssertEquals("B3AcceptedDate", new ZDateTime(2020, 8, 19), declaration.B3AcceptedDate);

				reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				AssertEquals("Entry message for this shipment has already been accepted, it cannot be manually released again.", reasonForCannotManualRelease);

				entryHeader.CH_EntryReleaseDate = ZDateTime.Empty;
				AssertEquals("B3AcceptedDate", ZDateTime.Empty, declaration.B3AcceptedDate);

				reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				AssertEquals("Cargo Control number and sub-location code or the text identifying cargo location are not specified.", reasonForCannotManualRelease);

				declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
				reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				AssertEquals(ZString.Empty, reasonForCannotManualRelease);

				entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
				reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				AssertEquals(ZString.Empty, reasonForCannotManualRelease);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestHasEntryTransactions()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			AssertEquals(false, dec.HasEntryTransactions);

			var header = dec.CustomsEntryHeaders.AddNew();
			header.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			header.CH_Status = MessageStatusList.Codes.ClearOriginal;
			var outgoingMessage = header.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.CH_Status = MessageStatusList.Codes.ClearOriginal;

			AssertEquals(true, dec.HasEntryTransactions);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		public void TestDeleteInvoiceLinePackagePivot()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MBMB1";
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			AssertEquals(1, declaration.Packages.Count);
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(1, invoiceLine.PackagesPivot.Count);

			Factory.Save();

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			AssertEquals(0, invoiceLine.PackagesPivot.Count);

			invoiceLine.Delete();
			header.InvoiceLines.AddNew();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestPackageValidationNotFiredForNonIID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			AssertEquals("Package count", 1, declaration.Packages.Count);

			Assert(!declaration.IsIID);
			Assert(!declaration.SupportsChcPivotBetweenInvoiceLineAndPacking);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("JobComInvoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking", !invoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking);
				declaration.Packages[0].RunPreSaveValidation();
				AssertNoRowMessageError(declaration.Packages[0], PackageValidation.LinkAtLeastOneInvoiceOrInvoiceLine);
			});

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			CombineAssertions(() =>
			{
				Assert("JobComInvoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking", invoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking);
				declaration.Packages[0].RunPreSaveValidation();
				AssertHasRowMessageError(declaration.Packages[0], PackageValidation.LinkAtLeastOneInvoiceOrInvoiceLine);
			});
		}

		public void TestSetAVSStatusIfNeededWhenCA_ServiceOptionChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert(declaration.IsIID);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA, "0101210000");
			AssertEquals(AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);

			declaration.CA_ServiceOption = ServiceOptions.Codes.ACKResponse;
			Assert(!declaration.IsIID);
			Assert(!declaration.IsOGD);
			AssertEquals(AVSStatusList.Codes.Blank, invoiceLine.CA_OGDStatus);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert(declaration.IsOGD);

			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA, "0201210000");
			AssertEquals(AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
		}

		public void TestJE_EntryAuthorisationDateReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Read only", declaration.JE_EntryAuthorisationDateInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert("Not read only", !declaration.JE_EntryAuthorisationDateInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Assert("Not read only", !declaration.JE_EntryAuthorisationDateInfo.ReadOnly);
		}

		public override void TestResetInvoiceDateForGroupingInvoiceInTemplateCopy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var groupinvoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2007, 08, 14);
			var normalinvoice = declaration.Invoices.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2006, 08, 14);
			Factory.Save();

			var declarationCopy = (JobDeclaration)declaration.TemplateCopy();
			var groupinvoiceCopy = (JobComInvoiceGroupHeader)declarationCopy.AllGroupHeaders.First();
			AssertEquals(ZDateTime.Empty, groupinvoiceCopy.JZ_InvoiceDate);

			var normalinvoiceCopy = (JobComInvoiceHeader)declarationCopy.Invoices.First(x => !x.JZ_GroupInvoice);
			AssertEquals(ZDateTime.Empty, normalinvoiceCopy.JZ_InvoiceDate);
		}

		public void TestDoesRequireImporterContact()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = "IID";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			AssertEquals(false, declaration.DoesRequireImporterHasPGAContact);

			invoiceLine1.CA_HCInd = "Y";
			var hcHeader = invoiceLine1.HCPGAHeader;

			AssertNotNull(hcHeader);
			AssertEquals(true, declaration.DoesRequireImporterHasPGAContact);
		}

		public void TestDoesRequireImporterContactForNonIID()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "3824600001");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			AssertEquals(false, declaration.ClassificationsHasPGARequirements);
			invoiceLine1.JI_Tariff = "3824600001";
			AssertEquals(true, declaration.ClassificationsHasPGARequirements);
		}

		public void TestDoesRequireImporterHasCFIAAccountNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = "IID";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			AssertEquals(false, declaration.DoesRequireImporterHasCFIAAccountNumber);

			invoiceLine1.CA_CFIAInd = "Y";
			var cfiaHeader = invoiceLine1.CFIAPGAHeader;

			AssertNotNull(cfiaHeader);
			AssertEquals(true, declaration.DoesRequireImporterHasCFIAAccountNumber);
		}

		public void TestGetRankerForTemplate()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Factory.Save();

			var tempalateIMP = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateIMP.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateIMP.P0_SubType2 = "IMO";

			var workflowIMP = tempalateIMP.WorkflowItems.AddNew();
			workflowIMP.P9_Description = "IMPWORK";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration1.PK);
			loadedDeclaration.SuspendAddingWorkflow = false;
			loadedDeclaration.JE_TransportMode = loadedDeclaration.TransportModeAirCodeForTesting;
			newFactory.Save();
			AssertEquals(1, loadedDeclaration.WorkflowItems.Tasks.Count);
			AssertEquals("Should match to IMP", "IMPWORK", loadedDeclaration.WorkflowItems.Tasks[0].P9_Description);

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Factory.Save();
			var loadedDeclaration2 = newFactory.Load<BaseJobDeclaration>(declaration2.PK);
			loadedDeclaration2.SuspendAddingWorkflow = false;
			loadedDeclaration2.JE_TransportMode = loadedDeclaration2.TransportModeAirCodeForTesting;
			newFactory.Save();
			AssertEquals(0, loadedDeclaration2.WorkflowItems.Tasks.Count);
		}

		public void TestTimeAtPortOfDischarge()
		{
			var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;

			var unlocoZ = Factory.New<RefUNLOCO>();
			unlocoZ.RL_Code = "!ZZ";
			unlocoZ.RL_R3 = timeZoneSet.PK;
			var effectiveDate = unlocoZ.LocationDateTime;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Constants.TransportModes.Sea;
			dec.JE_RL_NKPortOfArrival = "!ZZ";
			Factory.Save();

			AssertEquals(effectiveDate, dec.TimeAtPortOfDischarge);

			dec.JE_TransportMode = Constants.TransportModes.Road;
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "MY COMPANY";
			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MY BRANCH";

			var unlocoX = Factory.New<RefUNLOCO>();
			unlocoX.RL_Code = "!XX";
			branch.GB_RL_NKHomePort = "!XX";
			unlocoX.RL_R3 = timeZoneSet.PK;
			effectiveDate = unlocoX.LocationDateTime;
			dec.JE_GB = branch.PK;

			Factory.Save();
			AssertEquals(effectiveDate, dec.TimeAtPortOfDischarge);

			dec.JE_TransportMode = Constants.TransportModes.Sea;
			dec.JE_RL_NKPortOfArrival = "XXXXX";

			AssertEquals(effectiveDate, dec.TimeAtPortOfDischarge);
		}

		#region B2 status enhancement test
		public void TestCA_B2TotalOnB2()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var header = b2.B2AsAccountedForInvoices.AddNew();
			header.JZ_InvoiceNumber = "INV1";
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line = header.AsAccountForFilteredInvoiceLines.AddNew();
			line.CA_OriginalLineNo = "1";
			line.JI_Description = "SOME DESCRIPTION";
			line.CA_AuthorityNumber = "AUTHO";
			line.JI_Tariff = "2402.10.00 10";
			line.CA_99TariffCode = "9960";
			line.JI_CustomsQuantity = 5m;
			line.JI_CustomsUnitQty = "MIL";
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsSimilarGoods;
			line.CA_CVforCurrConv = 1000m;

			var sima = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_Override = true;
			sima.C1_ExemptCode = SIMACodes.Codes.C31;
			sima.C1_Amount = 500m;

			var excise = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 50m;
			excise.C1_Rate = 5.0m;
			excise.C1_RateType = RateTypes.Codes.Specific;

			var duty = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 300m;
			duty.C1_Rate = 6.5m;
			duty.C1_UnitOfMeasure = "AG";
			duty.C1_RateType = RateTypes.Codes.AdValorem;

			var gst = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 60m;
			gst.C1_Rate = 5m;
			gst.C1_RateType = RateTypes.Codes.AdValorem;

			var laimLine = line.CorrespondingAsClaimedForInvoiceLine;
			laimLine.DutiesAndTaxes.DeleteAll();
			var sima2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			sima2.C1_Override = true;
			sima2.C1_ExemptCode = SIMACodes.Codes.C31;
			sima2.C1_Amount = 400m;

			var excise2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise2.C1_Override = true;
			excise2.C1_Amount = 40m;
			excise2.C1_Rate = 5.0m;
			excise2.C1_RateType = RateTypes.Codes.Specific;

			var duty2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Override = true;
			duty2.C1_Amount = 200m;
			duty2.C1_Rate = 6.5m;
			duty2.C1_UnitOfMeasure = "AG";
			duty2.C1_RateType = RateTypes.Codes.AdValorem;

			var gst2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst2.C1_Override = true;
			gst2.C1_Amount = 70m;
			gst2.C1_Rate = 5m;
			gst2.C1_RateType = RateTypes.Codes.AdValorem;
			b2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			b2.DoMerge();
			b2.CalculateB2Total();
			var subrefund = 400m + 40 + 200 - 500 - 50 - 300;
			var gstrefund = 70m - 60;
			AssertEquals(b2.CA_B2Total, subrefund + gstrefund);
			AssertEquals(b2.AmountDueCBSA, ZDecimal.Zero);
			AssertEquals(b2.AmountDueImporter, subrefund + gstrefund);

			gst2.C1_Amount = 50m;
			b2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			b2.DoMerge();
			b2.CalculateB2Total();
			AssertEquals(b2.CA_B2Total, subrefund);
			AssertEquals(b2.AmountDueCBSA, ZDecimal.Zero);
			AssertEquals(b2.AmountDueImporter, subrefund);

			sima2.C1_ExemptCode = SIMACodes.Codes.C32;
			sima.C1_ExemptCode = SIMACodes.Codes.C32;
			b2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			b2.DoMerge();
			b2.CalculateB2Total();
			subrefund = 40 + 200 - 50 - 300;
			AssertEquals(b2.CA_B2Total, subrefund);
			AssertEquals(b2.AmountDueCBSA, ZDecimal.Zero);
			AssertEquals(b2.AmountDueImporter, subrefund);
		}

		public void TestCA_B2TotalOnIM2()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = "TRF";
			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			declaration.CA_IsDocAttached = true;
			declaration.CA_JustificationForRequest = "JUSTIFICATION FOR REQUEST";
			declaration.CA_Under = "UNDER";
			declaration.CA_B2Explanation = "EXPLANATION";
			declaration.CA_ClaimedInterestAmount = 11m;
			declaration.CA_AnySightDepositAmount = 12m;
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "MY COMPANY";
			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MY BRANCH";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_WorkPhone = "MY PHONE";
			staff.GS_FullName = "MY NAME";
			staff.GS_GB_HomeBranch = branch.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_CVforCurrConv = 1000m;
			line1.JI_Tariff = "0000000001";
			var sima = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 500m;
			var duty = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 50m;
			var excise = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 300m;
			var gst = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 60m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var im2 = declaration.GetNewCopyToB2Declaration();
			var line2 = im2.InvoiceLines[0];
			line2.JI_Tariff = "0000000002";
			var sima2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			sima2.C1_Amount = 600;
			var duty2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Amount = 60;
			var excise2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			excise2.C1_Amount = 400m;
			var gst2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			gst2.C1_Amount = 70m;
			im2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			im2.ApportionmentDirty = false;
			im2.DoMerge();

			im2.CalculateIM2Total();
			var subrefund = 600m + 60 + 400 - 500 - 50 - 300;
			var gstrefund = 70m - 60;
			AssertEquals(subrefund + gstrefund - declaration.CA_AnySightDepositAmount, im2.CA_B2Total);
			gst2.C1_Amount = 50m;
			im2.DoMerge();
			im2.CalculateIM2Total();
			AssertEquals(subrefund - declaration.CA_AnySightDepositAmount, im2.CA_B2Total);

			sima.C1_ExemptCode = SIMACodes.Codes.C32;
			sima2.C1_ExemptCode = SIMACodes.Codes.C32;
			im2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			im2.CalculateIM2Total();
			subrefund = 60 + 400 - 50 - 300;
			AssertEquals(im2.CA_B2Total, subrefund - declaration.CA_AnySightDepositAmount);
			AssertEquals(im2.AmountDueCBSA, subrefund - declaration.CA_AnySightDepositAmount);
			AssertEquals(im2.AmountDueImporter, ZDecimal.Zero);
		}

		public void TestB2s()
		{
			var originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			originalJob.TransactionNumber.AccountSecurityCode = "12345";
			originalJob.TransactionNumber.SequentialNumber = "00006789";
			originalJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn = originalJob.CargoControlNumbers.AddNew();
			ccn.CY_CargoControlNumber = "8036X557";
			Factory.Save();
			var originalCargoControlNumbers = originalJob.CargoControlNumbers;

			var im2Dec = originalJob.GetNewCopyToB2Declaration();
			AssertEquals("CA_OriginalTransactionNo", "12345000067897", im2Dec.CA_OriginalTransactionNo);

			var b2Dec = Factory.New<JobDeclaration>();
			b2Dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Dec.CA_B2Type = B2TypeList.Codes.Specific;
			b2Dec.CA_OriginalTransactionNo = originalJob.FormattedTransactionNumber;
			Factory.Save();
			AssertEquals(2, originalJob.B2s.Count);
			var newOriginalCargoControlNumbers = b2Dec.OriginalJobCargoControlNumbers;
			AssertArrayEqualsByElements(originalCargoControlNumbers.ToArray(), newOriginalCargoControlNumbers.ToArray());
		}

		public void TestDNHistoryLines()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var collection = new CusStatementLineCollection(statementHeader);

			var originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			originalJob.TransactionNumber.AccountSecurityCode = "12345";
			originalJob.TransactionNumber.SequentialNumber = "00006789";
			originalJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn = originalJob.CargoControlNumbers.AddNew();
			ccn.CY_CargoControlNumber = "8036X557";
			Factory.Save();

			var im2Dec = originalJob.GetNewCopyToB2Declaration();
			im2Dec.JE_DeclarationReference = "B00000002";
			AssertEquals("CA_OriginalTransactionNo", "12345000067897", im2Dec.CA_OriginalTransactionNo);

			var b2Dec = Factory.New<JobDeclaration>();
			b2Dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Dec.CA_B2Type = B2TypeList.Codes.Specific;
			b2Dec.CA_OriginalTransactionNo = originalJob.FormattedTransactionNumber;
			b2Dec.JE_DeclarationReference = "B00000003";
			Factory.Save();

			var line = collection.AddNew();
			line.B3_BrokerReference = "B00000001";
			line.B3_EntryNum = "TESTENTRY000001";
			var lin2 = collection.AddNew();
			lin2.B3_BrokerReference = "B00000002";
			var line3 = collection.AddNew();
			line3.B3_BrokerReference = "B00000003";
			AssertEquals(3, originalJob.DNHistoryLines.Count);

			var statementHeader2 = Factory.New<CusStatementHeader>();
			var collection2 = new CusStatementLineCollection(statementHeader2);
			var line_2 = collection2.AddNew();
			line_2.B3_BrokerReference = "B00000001";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(2, originalJob.DNHistoryLines.Count);

			line_2.B3_EntryNum = "TESTENTRY000001";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(1, originalJob.DNHistoryLines.Count);

			line_2.B3_EntryNum = "TESTENTRY000002";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(2, originalJob.DNHistoryLines.Count);

			line_2.B3_EntryNum = "TESTENTRY000001";
			line_2.B3_EntryType = "B3";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(2, originalJob.DNHistoryLines.Count);

			line.B3_EntryType = "B3";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(1, originalJob.DNHistoryLines.Count);

			statementHeader2.B2_ProcessDate = new ZDateTime(2019, 03, 01);
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(2, originalJob.DNHistoryLines.Count);

			statementHeader.B2_ProcessDate = new ZDateTime(2019, 03, 01);
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(1, originalJob.DNHistoryLines.Count);

			line_2.B3_Status = "X";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(2, originalJob.DNHistoryLines.Count);

			line.B3_Status = "X";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(1, originalJob.DNHistoryLines.Count);

			line_2.B3_ImporterCustomsID = "123456";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(2, originalJob.DNHistoryLines.Count);

			line.B3_ImporterCustomsID = "123456";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			AssertEquals(1, originalJob.DNHistoryLines.Count);

			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B0000000";
			AssertEquals(0, originalJob.DNHistoryLines.Count);

			var statementHeader3 = Factory.New<CusStatementHeader>();
			statementHeader3.B2_AccountNo = "00003";
			var collection3 = new CusStatementLineCollection(statementHeader3);
			var line_3 = collection3.AddNew();
			line_3.B3_BrokerReference = "B00000004";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000004";
			AssertEquals(0, originalJob.DNHistoryLines.Count);

			statementHeader3.B2_AccountNo = "";
			line_3.B3_BrokerReference = "B00000005";
			originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000005";
			AssertEquals(1, originalJob.DNHistoryLines.Count);
		}
		#endregion

		public void TestB3EntrySubmittedDate()
		{
			var dec = Factory.New<JobDeclaration>();

			// Without B3EntryHeader
			AssertEquals("B3EntrySubmittedDate.IsEmpty", true, dec.B3EntrySubmittedDate.IsEmpty);

			try
			{
				dec.B3EntrySubmittedDate = ZDateTime.Now;
			}
			catch (Exception)
			{
			}
			AssertEquals("B3EntrySubmittedDate.IsEmpty", true, dec.B3EntrySubmittedDate.IsEmpty);

			// With B3EntryHeader

			var header = dec.CustomsEntryHeaders.AddNew();
			header.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			header.CH_EntrySubmittedDate = ZDateTime.Now.AddSeconds(-30);

			AssertEquals("B3EntrySubmittedDate.IsEmpty", true, dec.B3EntrySubmittedDate == header.CH_EntrySubmittedDate);
			var entrySubmittedDate = ZDateTime.Now;

			try
			{
				dec.B3EntryHeader.CH_EntrySubmittedDate = entrySubmittedDate;
			}
			catch (Exception e)
			{
				Assert("Exception should not occur: " + e.Message, false);
			}

			AssertEquals("B3EntrySubmittedDate.IsEmpty", true, dec.B3EntrySubmittedDate == entrySubmittedDate);
			AssertEquals("B3EntrySubmittedDate.IsEmpty", true, header.CH_EntrySubmittedDate == entrySubmittedDate);
		}

		public void TestRelEntrySubmittedDate()
		{
			var dec = Factory.New<JobDeclaration>();

			// Without ReleaseEntryHeader
			AssertEquals("RelEntrySubmittedDate.IsEmpty", true, dec.RelEntrySubmittedDate.IsEmpty);

			try
			{
				dec.RelEntrySubmittedDate = ZDateTime.Now;
			}
			catch (Exception)
			{
			}
			AssertEquals("RelEntrySubmittedDate.IsEmpty", true, dec.RelEntrySubmittedDate.IsEmpty);

			// With ReleaseEntryHeader

			var header = dec.CustomsEntryHeaders.AddNew();
			header.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			header.CH_EntrySubmittedDate = ZDateTime.Now.AddSeconds(-30);

			AssertEquals("RelEntrySubmittedDate.IsEmpty", true, dec.RelEntrySubmittedDate == header.CH_EntrySubmittedDate);
			var entrySubmittedDate = ZDateTime.Now;

			try
			{
				dec.ReleaseEntryHeader.CH_EntrySubmittedDate = entrySubmittedDate;
			}
			catch (Exception e)
			{
				Assert("Exception should not occur: " + e.Message, false);
			}

			AssertEquals("RelEntrySubmittedDate.IsEmpty", true, dec.RelEntrySubmittedDate == entrySubmittedDate);
			AssertEquals("RelEntrySubmittedDate.IsEmpty", true, header.CH_EntrySubmittedDate == entrySubmittedDate);
		}

		public void TestErrorIfChangeTransactionNumberOnceMessageSentToCustoms()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.TransactionNumber.AccountSecurityCode = "12345";
			dec.TransactionNumber.SequentialNumber = "00006789";
			Factory.Save();

			dec.TransactionNumber.AccountSecurityCode = "12345";
			dec.TransactionNumber.SequentialNumber = "00007890";

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew();
			AssertExceptionThrown<InvalidOperationException>(Factory.Save);
		}

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CAJobDeclaration);
			}
		}

		public void TestAdditionalJobsToShowChargesFor()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(0, ((IJobInvoicingPlugInAdditionalJobs)lvsJob).AdditionalJobsToShowChargesFor.Length);

			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var lvxHeader = new JobHeader.Loader(lvxJob).TryCreate();

			var accChgCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			accChgCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			accChgCode2.AC_Code = "TS2";
			accChgCode2.AC_GC = GlbCompany.CurrentCompany.PK;

			var lvxcharge = Factory.NewWithValidTestData<JobCharge>();
			lvxcharge.JR_JH = lvxHeader.PK;
			lvxcharge.JR_AC = accChgCode2.PK;
			lvxcharge.JR_LocalSellAmt = 150m;
			lvxcharge.JR_OSSellAmt = 150m;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);

			AssertEquals(1, ((IJobInvoicingPlugInAdditionalJobs)lvsJob).AdditionalJobsToShowChargesFor.Length);
		}

		public void TestDeliveryAddressCore()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			var deliveryAddressOrg = Factory.New<OrgHeader>();
			deliveryAddressOrg.OH_FullName = "TestOrgName2";

			var addinfo = OrgImpAddInfo.Get(importer);
			addinfo.ZO_PrintDeliveryAddressForLVX = false;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDeliveryAddress.OrganisationPK = deliveryAddressOrg.PK;
			AssertEquals("Do not print Delivery Address for import job", ZString.Empty, declaration.DeliveryAddress);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("Do not print Delivery Address for LVX job with unticked Delivery Address", ZString.Empty, declaration.DeliveryAddress);

			addinfo.ZO_PrintDeliveryAddressForLVX = true;
			AssertEquals("Do not print Delivery Address for LVX job with ticked Delivery Address", "TestOrgName2".ToUpper(), declaration.DeliveryAddress);
		}

		#endregion

		#region Copy for B2 Tests
		public void TestIsIM2()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			Assert(dec.IsIM2);
			Assert(dec.IsImport);
		}

		public void TestIsB3X()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert(dec.IsB3X);
			Assert(!dec.IsImport);
			Assert(dec.IsB2OrIM2OrB3X);
			Assert(dec.IsImportIncludingB2);
		}

		public void TestB3XFieldsReadOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert(dec.CA_B2TypeInfo.ReadOnly);
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Assert(!dec.CA_B2TypeInfo.ReadOnly);
		}

		public void TestPreviousB2()
		{
			var previousJob = Factory.New<JobDeclaration>();
			previousJob.JE_DeclarationReference = "B00000001";
			previousJob.TransactionNumber.AccountSecurityCode = "12345";
			previousJob.TransactionNumber.SequentialNumber = "00006789";
			previousJob.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			var newJob = Factory.New<JobDeclaration>();
			newJob.CA_JE_PreviousJob = previousJob.PK;
			Factory.Save();

			AssertEquals("12345000067897", newJob.PreviousTransactionNumber);
			AssertEquals("B00000001", newJob.PreviousJobNumber);
		}

		public void TestOriginalDeclaration()
		{
			var originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			var pccn = CusEntryNumber.New(originalJob, "REL", Core.Constants.CountryCodes.Canada);
			pccn.CE_EntryNum = "12345XX";
			originalJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			var newJob = Factory.New<JobDeclaration>();
			newJob.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			newJob.CA_OriginalTransactionNo = "12345XX";
			AssertEquals("B00000001", newJob.OriginalDeclaration.JE_DeclarationReference);
		}

		public void TestGetNextVersionNumberDeclaration()
		{
			var importJob = Factory.New<JobDeclaration>();
			importJob.JE_DeclarationReference = "B00000001";
			importJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			importJob.TransactionNumber.AccountSecurityCode = "12345";
			importJob.TransactionNumber.SequentialNumber = "00006789";

			var im2Job = Factory.New<JobDeclaration>();
			im2Job.JE_DeclarationReference = "B00000002";
			im2Job.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			im2Job.CA_OriginalTransactionNo = "12345000067897";

			var nextJob = Factory.New<JobDeclaration>();
			nextJob.JE_DeclarationReference = "B00000003";
			nextJob.CA_OriginalTransactionNo = "12345000067897";
			nextJob.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			nextJob.CA_Version = 3;

			Factory.Save();
			var impdec = importJob.GetNextVersionNumberDeclaration();
			AssertNotNull("Next Version Declaration For IMP Job", impdec);
			AssertEquals("B00000003", impdec.JE_DeclarationReference);

			var im2dec = im2Job.GetNextVersionNumberDeclaration();
			AssertNotNull("Next Version Declaration For IM2 Job", im2dec);
			AssertEquals("B00000003", im2dec.JE_DeclarationReference);
		}

		public void TestJE_MessageTypeReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals("Not read only", false, declaration.JE_MessageTypeInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("Not read only", false, declaration.JE_MessageTypeInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Not read only", false, declaration.JE_MessageTypeInfo.ReadOnly);

			var newIM2Job = declaration.GetNewCopyToB2Declaration();
			AssertEquals(JobMessageTypeList.Codes.ImportCopyforB2, newIM2Job.JE_MessageType);
			AssertEquals("Read only", true, newIM2Job.JE_MessageTypeInfo.ReadOnly);
		}

		public void TestCargoControlNumberWhenChangeJE_MessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn1 = declaration.CargoControlNumbers.AddNew();
			ccn1.CY_CargoControlNumber = "CCN1";
			var ccn2 = declaration.CargoControlNumbers.AddNew();
			ccn2.CY_CargoControlNumber = "CCN2";
			AssertEquals("Cargo Control Number Count", 2, declaration.CargoControlNumbers.Count);
			AssertEquals("CCN1", "CCN1", declaration.CargoControlNumbers[0].CY_CargoControlNumber);
			AssertEquals("CCN2", "CCN2", declaration.CargoControlNumbers[1].CY_CargoControlNumber);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Cargo Control Number Count", 0, declaration.CargoControlNumbers.Count);
		}

		public void TestGetNewCopyToB2Declaration()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			var originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			originalJob.JE_OH_Importer = importer.PK;
			originalJob.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
			originalJob.TransactionNumber.AccountSecurityCode = "12345";
			originalJob.TransactionNumber.SequentialNumber = "00006789";
			originalJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			originalJob.CA_WoodPackagingInd = ZBool.True;
			originalJob.CA_PermitApplication = ZBool.True;
			originalJob.CA_InspectionArrangementsComplete = ZBool.True;
			originalJob.CA_CSAEntry = ZBool.True;
			originalJob.CA_ATDExCode = "123";
			originalJob.CA_AmendReasonCode = "11";
			originalJob.CA_OGDCFIA = ZBool.True;
			originalJob.CA_OGDIC = ZBool.True;
			originalJob.CA_OGDNR = ZBool.True;
			originalJob.CA_OGDTC = ZBool.True;

			var ccn = originalJob.CargoControlNumbers.AddNew();
			ccn.CY_CargoControlNumber = "8036X557";
			Factory.Save();

			var newJob = originalJob.GetNewCopyToB2Declaration();
			AssertEquals("Importer", importerOfRecord.PK, newJob.Importer.PK);
			AssertEquals("ImporterOfRecord", importerOfRecord.PK, newJob.ImporterOfRecord.PK);
			AssertEquals("JE_OA_ImporterAddress", importerOfRecord.MainAddress.PK, newJob.JE_OA_ImporterAddress);
			AssertEquals("PreviousJob PK", originalJob.PK, newJob.CA_JE_PreviousJob);
			AssertEquals("TransactionNumber", ZString.Empty, newJob.TransactionNumber.ToString());
			AssertEquals("CA_Version", 2, newJob.CA_Version);
			AssertEquals("CA_OriginalTransactionNo", "12345000067897", newJob.CA_OriginalTransactionNo);
			AssertEquals("CargoControlNumbers", 0, newJob.CargoControlNumbers.Count);
			AssertEquals("Copy from B3", AmendmentToList.Codes.OriginalB3, newJob.CA_AmendmentTo);
			AssertEquals(newJob.CA_WoodPackagingInd, ZBool.False);
			AssertEquals(newJob.CA_PermitApplication, ZBool.False);
			AssertEquals(newJob.CA_InspectionArrangementsComplete, ZBool.False);
			AssertEquals(newJob.CA_CSAEntry, ZBool.False);
			AssertEquals(newJob.CA_ATDExCode, ZString.Empty);
			AssertEquals(newJob.CA_AmendReasonCode, ZString.Empty);
			AssertEquals(newJob.CA_OGDCFIA, ZBool.False);
			AssertEquals(newJob.CA_OGDIC, ZBool.False);
			AssertEquals(newJob.CA_OGDNR, ZBool.False);
			AssertEquals(newJob.CA_OGDTC, ZBool.False);

			newJob.JE_IsCancelled = true;
			newJob.TransactionNumber.AccountSecurityCode = "12345";
			newJob.TransactionNumber.SequentialNumber = "00007890";
			newJob.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			Factory.Save();

			var newJobFromIm2 = newJob.GetNewCopyToB2Declaration();
			AssertEquals("PreviousJob PK", newJob.PK, newJobFromIm2.CA_JE_PreviousJob);
			AssertEquals("TransactionNumber", ZString.Empty, newJobFromIm2.TransactionNumber.ToString());
			AssertEquals("CA_Version", 3, newJobFromIm2.CA_Version);
			AssertEquals("CA_OriginalTransactionNo", "12345000067897", newJobFromIm2.CA_OriginalTransactionNo);
			AssertEquals("CargoControlNumbers", 0, newJobFromIm2.CargoControlNumbers.Count);
			AssertEquals("Copy from B2", AmendmentToList.Codes.B2, newJobFromIm2.CA_AmendmentTo);
		}

		#endregion

		public void TestGetJobDirection()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Base call direction", Directions.Export, declaration.GetJobDirectionExposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX direction", Directions.Import, declaration.GetJobDirectionExposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 direction", Directions.Import, declaration.GetJobDirectionExposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X direction", Directions.Import, declaration.GetJobDirectionExposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("LVS direction", Directions.Import, declaration.GetJobDirectionExposed());
		}

		public void TestIncoTerms()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals("Base call incoterms", "DDP", declaration.IncoTerm);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("Base call incoterms", "FOB", declaration.IncoTerm);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("Base call incoterms", "FOB", declaration.IncoTerm);
		}

		public void TestB2Explanation()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			Assert("Precondition: declaration.HasChanges", !declaration.HasChanges);
			declaration.CA_B2Explanation = "123456";
			Assert("B2Explanation should be registered as an EditableChildObject", declaration.IsRegisteredEditableChildObject(declaration.B2Explanation));
			Assert("declaration.HasChanges should be set", declaration.HasChanges);
		}

		public void TestGetOwnersRefForDocuments()
		{
			var debtor1 = Factory.New<OrgHeader>();
			var debtor2 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";

			var inv1 = declaration.Invoices.AddNew();
			inv1.JZ_InvoiceNumber = "ID1";
			inv1.JZ_OH_Buyer = debtor1.PK;
			var inv2 = declaration.Invoices.AddNew();
			inv2.JZ_InvoiceNumber = "ID2";
			inv2.JZ_OH_Buyer = debtor2.PK;
			var inv3 = declaration.Invoices.AddNew();
			inv3.JZ_InvoiceNumber = "ID3";
			inv3.JZ_OH_Buyer = debtor1.PK;
			var inv4 = declaration.Invoices.AddNew();
			inv4.JZ_InvoiceNumber = "ID4";
			AssertEquals("Base call GetOwnersRefOverrideForDocuments", ZString.Empty, declaration.GetOwnersRefOverrideForDocuments(debtor1));
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			AssertEquals("TotalConsolidation GetOwnersRefOverrideForDocuments", "ID1,ID3,ID4", declaration.GetOwnersRefOverrideForDocuments(debtor1));
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertEquals("Consolidation by Importer GetOwnersRefOverrideForDocuments", "ID1,ID2,ID3,ID4", declaration.GetOwnersRefOverrideForDocuments(debtor1));
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2", "B2 Transaction # 12345000067897", declaration.GetOwnersRefOverrideForDocuments(debtor1));
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X", "B3X Transaction # 12345000067897", declaration.GetOwnersRefOverrideForDocuments(debtor1));
			declaration.Invoices.RemoveAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.Invoices.AddNew().JZ_InvoiceNumber = "ID1";
			AssertEquals("LVX job GetOwnersRefOverrideForDocuments", "LVS ID: ID1", declaration.GetOwnersRefOverrideForDocuments(debtor1));
		}

		public void TestFinalDestinationForDocuments()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "CATOR";
			AssertEquals("Base FinalDestinationForDocuments", "CATOR", declaration.FinalDestinationForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("LVS FinalDestinationForDocuments", ZString.Empty, declaration.FinalDestinationForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 FinalDestinationForDocuments", ZString.Empty, declaration.FinalDestinationForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X FinalDestinationForDocuments", ZString.Empty, declaration.FinalDestinationForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX FinalDestinationForDocuments", ZString.Empty, declaration.FinalDestinationForDocuments);
		}

		public void TestOriginForDocuments()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "CATOR";
			AssertEquals("Base OriginForDocuments", "CATOR", declaration.OriginForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("LVS OriginForDocuments", "CATOR", declaration.OriginForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 OriginForDocuments", "CATOR", declaration.OriginForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX OriginForDocuments", ZString.Empty, declaration.OriginForDocuments);
		}

		public void TestShowInvoiceNumbersOnDocuments()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Base ShowInvoiceNumbersOnDocuments", declaration.ShowInvoiceNumbersOnDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert("LVS ShowInvoiceNumbersOnDocuments", !declaration.ShowInvoiceNumbersOnDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Assert("B2 ShowInvoiceNumbersOnDocuments", !declaration.ShowInvoiceNumbersOnDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert("B3X ShowInvoiceNumbersOnDocuments", !declaration.ShowInvoiceNumbersOnDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			Assert("LVX ShowInvoiceNumbersOnDocuments", !declaration.ShowInvoiceNumbersOnDocuments);
		}

		public void TestGoodsDescriptionForDocuments()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GoodsDescription = "GOODS DESCRIPTION";
			AssertEquals("Base GoodsDescriptionForDocuments", "GOODS DESCRIPTION", declaration.GoodsDescriptionForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("Base GoodsDescriptionForDocuments", "Various low value shipments", declaration.GoodsDescriptionForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.CA_B2Type = B2TypeList.Codes.Blanket;
			AssertEquals("Blanket B2 GoodsDescriptionForDocuments", "Blanket B2", declaration.GoodsDescriptionForDocuments);
			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			declaration.CA_OriginalTransactionNo = "12345123456789";
			AssertEquals("B2 GoodsDescriptionForDocuments", "B2 for Original Transaction # 12345123456789", declaration.GoodsDescriptionForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX GoodsDescriptionForDocuments", "Low Value Shipment", declaration.GoodsDescriptionForDocuments);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			declaration.CA_OriginalTransactionNo = "12345123456789";
			AssertEquals("B3X GoodsDescriptionForDocuments", "B3X for Original Transaction # 12345123456789", declaration.GoodsDescriptionForDocuments);
		}

		public void TestTotalAmounts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 3m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 4m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, 5m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 6m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 7m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 8m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 9m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 10m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, 11m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 12m);

			CombineAssertions(() =>
			{
				AssertEquals("TotalSimaDuty", 34m, declaration.TotalSimaDuty);
				AssertEquals("TotalExciseTax", 14m, declaration.TotalExciseTax);
				AssertEquals("TotalDutyAndTax", 78m, declaration.TotalDutyAndTax);
				AssertEquals("TotalAmountPayable", 62m, declaration.TotalAmountPayable);
				AssertEquals("TotalNormalDuty", 8m, declaration.TotalNormalDuty);
				AssertEquals("TotalGST", 22m, declaration.TotalGST);
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var b2AsClaimedForInvoice = declaration.B2AsClaimedForInvoices.AddNew();
			var claimedLine01 = b2AsClaimedForInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			var duty01 = claimedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty01.C1_Amount = 13m;
			var sima01 = claimedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SIMADuty);
			sima01.C1_Amount = 14m;
			var gst01 = claimedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst01.C1_Amount = 15m;
			var exc01 = claimedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			exc01.C1_Amount = 16m;
			var claimedLine02 = b2AsClaimedForInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			var duty02 = claimedLine02.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty02.C1_Amount = 23m;
			var sima02 = claimedLine02.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SIMADuty);
			sima02.C1_Amount = 24m;
			var gst02 = claimedLine02.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst02.C1_Amount = 25m;
			var exc02 = claimedLine02.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			exc02.C1_Amount = 26m;
			var b2AsAccountedForInvoice = declaration.B2AsAccountedForInvoices.AddNew();
			var accountedLine01 = b2AsAccountedForInvoice.AsAccountForFilteredInvoiceLines.AddNew();
			var duty03 = accountedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty03.C1_Amount = 100m;
			var sima03 = accountedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SIMADuty);
			sima03.C1_Amount = 100m;
			var gst03 = accountedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst03.C1_Amount = 100m;
			var exc03 = accountedLine01.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			exc03.C1_Amount = 100m;

			CombineAssertions(() =>
			{
				AssertEquals("TotalSimaDuty", -62m, declaration.TotalSimaDuty);
				AssertEquals("TotalExciseTax", -58m, declaration.TotalExciseTax);
				AssertEquals("TotalDutyAndTax", 78m, declaration.TotalDutyAndTax);
				AssertEquals("TotalAmountPayable", 62m, declaration.TotalAmountPayable);
				AssertEquals("TotalNormalDuty", -64m, declaration.TotalNormalDuty);
				AssertEquals("TotalGST", -60m, declaration.TotalGST);
			});
		}

		public void TestAttachToForwardedManifestOnSaving()
		{
			var message1 = Factory.New<ACIHouseBillMessage>();
			message1.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X555+4'RFF+AFM:10207:CB'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'";
			message1.EM_ApplicationReference = "8036X555";
			message1.EM_Status = EDIMessage.Status.Received;
			var message2 = Factory.New<ACIHouseBillMessage>();
			message2.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X556+4'RFF+AFM:10207:CB'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'";
			message2.EM_ApplicationReference = "8036X556";
			message2.EM_Status = EDIMessage.Status.Received;
			var message3 = Factory.New<ACIHouseBillMessage>();
			message3.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X557+4'RFF+AFM:10207:CB'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'";
			message3.EM_ApplicationReference = "8036X557";
			message3.EM_Status = EDIMessage.Status.Received;
			var message4 = Factory.New<ACIHouseBillMessage>();
			message4.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X558+4'RFF+AFM:10207:FW'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'";
			message4.EM_ApplicationReference = "8036X558";
			message4.EM_Status = EDIMessage.Status.Received;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "8036X555";
			Factory.Save();
			AssertNull("Not attached because there is no entry header", message1.EM_LinkedObject);
			AssertNull("Not attached to message2", message2.EM_LinkedObject);
			AssertNull("Not attached to message3", message3.EM_LinkedObject);
			AssertNull("Not attached to message4", message4.EM_LinkedObject);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();
			AssertEquals("Now attached when entry header added", entryHeader.PK, message1.EM_LinkedObject.PK);
			AssertNull("Not attached to message2", message2.EM_LinkedObject);
			AssertNull("Not attached to message3", message3.EM_LinkedObject);
			AssertNull("Not attached to message4", message4.EM_LinkedObject);

			number.CE_EntryNum = "8036X556";
			Factory.Save();
			AssertNull("Detached from message1", message1.EM_LinkedObject);
			AssertEquals("Now attached to message2", entryHeader.PK, message2.EM_LinkedObject.PK);
			AssertNull("Not attached to message3", message3.EM_LinkedObject);
			AssertNull("Not attached to message4", message4.EM_LinkedObject);

			var secondCCN = declaration.CargoControlNumbers.AddNew();
			secondCCN.CY_CargoControlNumber = "8036X557";
			Factory.Save();
			AssertNull("Not attached to message1", message1.EM_LinkedObject);
			AssertEquals("message2 still attached", entryHeader.PK, message2.EM_LinkedObject.PK);
			AssertEquals("Now also attached to message3", entryHeader.PK, message3.EM_LinkedObject.PK);
			AssertNull("Not attached to message4", message4.EM_LinkedObject);

			number.CE_EntryNum = "8036X558";
			Factory.Save();
			AssertNull("Not attached to message1", message1.EM_LinkedObject);
			AssertNull("Detached form message2", message2.EM_LinkedObject);
			AssertEquals("message3 still attached", entryHeader.PK, message3.EM_LinkedObject.PK);
			AssertNull("Not attached to message4 since it is not a CB", message4.EM_LinkedObject);
		}

		public void TestReciprocalRates()
		{
			Assert(Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.Canada, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public void TestDefaultingPortOfClearanceAndSubLocation_RegistryOff()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);

			var unlocoZ = Factory.New<RefUNLOCO>();
			unlocoZ.RL_Code = "!ZZ";
			var locoMapZ = Factory.New<RefLocoMap>();
			locoMapZ.RY_LocalPortCode = "PZZ!";
			locoMapZ.RY_RL_NKLocoPort = "!ZZ";
			locoMapZ.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapZ.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;

			var locoMapY = Factory.New<RefLocoMap>();
			locoMapY.RY_LocalPortCode = "PYY!";
			locoMapY.RY_RL_NKLocoPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			locoMapY.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapY.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;

			var unlocoX = Factory.New<RefUNLOCO>();
			unlocoX.RL_Code = "!XX";
			var companyX = Factory.New<GlbCompany>();
			companyX.GC_Code = "CXX";
			companyX.GC_RN_NKCountryCode = "CA";
			var branchX = companyX.Branches.AddNew();
			branchX.GB_Code = "BXX";
			branchX.GB_RL_NKHomePort = "!XX";
			var locoMapX = Factory.New<RefLocoMap>();
			locoMapX.RY_LocalPortCode = "PXX!";
			locoMapX.RY_RL_NKLocoPort = "!XX";
			locoMapX.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapX.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;
			Factory.Save();

			try
			{
				using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					var dec = Factory.NewWithValidTestData<JobDeclaration>();
					dec.JE_MessageType = JobMessageTypeList.Codes.Import;
					dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("No Routing, no default", ZString.Empty, dec.JE_CustomsOffice);
					AssertEquals("No Routing, no default", ZString.Empty, dec.JE_LocationOfGoods);
					dec.JE_RL_NKPortOfArrival = "!ZZ";
					AssertEquals("Default from CTO or CFS", ZString.Empty, dec.JE_CustomsOffice);
					AssertEquals("Default from CTO or CFS", ZString.Empty, dec.JE_LocationOfGoods);

					var depot = helper.CreateOrganisation("DPT", "DEPOT NAME", "CATOR", "DEPOT ADDRESS", "DEPOT CITY", "123 4567");
					var depot1COC = depot.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0001", canada);
					depot1COC.OK_OA_PremisesAddress = depot.MainAddress.PK;
					var depot1CCP = depot.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0011", canada);
					depot1CCP.OK_OA_PremisesAddress = depot.MainAddress.PK;

					var depotOtherAddress2 = depot.Addresses.AddNew();
					depotOtherAddress2.OA_CompanyNameOverride = "DEPOT NAME2";
					depotOtherAddress2.OA_Address1 = "DEPOT ADDRESS2";
					depotOtherAddress2.OA_City = "DEPOT CITY2";
					var depot2COC = depot.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0002", canada);
					depot2COC.OK_OA_PremisesAddress = depotOtherAddress2.PK;
					var depot2CCP = depot.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0022", canada);
					depot2CCP.OK_OA_PremisesAddress = depotOtherAddress2.PK;

					var depotOtherAddress3 = depot.Addresses.AddNew();
					depotOtherAddress3.OA_CompanyNameOverride = "DEPOT NAME3";
					depotOtherAddress3.OA_Address1 = "DEPOT ADDRESS3";
					depotOtherAddress3.OA_City = "DEPOT CITY3";

					var cto = helper.CreateOrganisation("CTO", "CTO NAME", "CATOR", "CTO ADDRESS", "CTP CITY", "123 4567");
					var ctoCOC = cto.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0003", canada);
					ctoCOC.OK_OA_PremisesAddress = cto.MainAddress.PK;
					var ctoCCP = cto.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0033", canada);
					ctoCCP.OK_OA_PremisesAddress = cto.MainAddress.PK;

					dec.DepotDocAddress.E2_OA_Address = depot.MainAddress.PK;
					dec.ContainerTerminalOperatorDocAddress.E2_OA_Address = cto.MainAddress.PK;

					AssertEquals("JE_CustomsOffice defaulted from CTO address", "0003", dec.JE_CustomsOffice);
					AssertEquals("JE_LocationOfGoods defaulted from CTO address", "0033", dec.JE_LocationOfGoods);

					dec.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
					AssertEquals("JE_CustomsOffice defaulted from CFS address", "0001", dec.JE_CustomsOffice);
					AssertEquals("JE_LocationOfGoods defaulted from CFS address", "0011", dec.JE_LocationOfGoods);

					dec.DepotDocAddress.E2_OA_Address = depotOtherAddress2.PK;
					AssertEquals("JE_CustomsOffice should now be picked up from CFS second Address", "0002", dec.JE_CustomsOffice);
					AssertEquals("JE_LocationOfGoods should now be picked up from CFS second Address", "0022", dec.JE_LocationOfGoods);

					var leg1 = dec.TransportsIncludingRelated.AddNew();
					leg1.JW_RL_NKDiscPort = "!ZZ";
					var leg2 = dec.TransportsIncludingRelated.AddNew();

					dec.JE_LocationOfGoods = ZString.Empty;
					dec.JE_CustomsOffice = ZString.Empty;

					leg2.JW_RL_NKDiscPort = "!XX";
					AssertEquals("JE_CustomsOffice should now be picked up from Last Transport", "PXX!", dec.JE_CustomsOffice);
					AssertEquals("JE_LocationOfGoods empty when more than one legs", "", dec.JE_LocationOfGoods);
				}
			}
			finally
			{
				locoMapY.Delete();
			}
		}

		public void TestDefaultingPortOfClearanceAndSubLocation_RegistryOn()
		{
			var unlocoZ = Factory.New<RefUNLOCO>();
			unlocoZ.RL_Code = "!ZZ";
			var locoMapZ = Factory.New<RefLocoMap>();
			locoMapZ.RY_LocalPortCode = "PZZ!";
			locoMapZ.RY_RL_NKLocoPort = "!ZZ";
			locoMapZ.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapZ.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;

			var locoMapSub = Factory.New<RefLocoMap>();
			locoMapSub.RY_LocalPortCode = "SUB!";
			locoMapSub.RY_RL_NKLocoPort = "!ZZ";
			locoMapSub.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSub.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;
			Factory.Save();

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("JE_RL_NKPortOfArrival not set, no default", "", dec.JE_CustomsOffice);
				AssertEquals("JE_RL_NKPortOfArrival not set, no default", "", dec.JE_LocationOfGoods);
				dec.JE_RL_NKPortOfArrival = "!ZZ";
				AssertEquals("Customs port defaults if transport mode is SEA or AIR", "PZZ!", dec.JE_CustomsOffice);
				AssertEquals("Customs port defaults if transport mode is SEA or AIR", "SUB!", dec.JE_LocationOfGoods);

				dec.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("ShouldUsePortOfArrivalForCustomsCodesDefaulting is false for Mode Road", "PZZ!", dec.JE_CustomsOffice);
				AssertEquals("ShouldUsePortOfArrivalForCustomsCodesDefaulting is false for Mode Road", "SUB!", dec.JE_LocationOfGoods);

				dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				dec.JE_CustomsOffice = ZString.Empty;
				dec.JE_LocationOfGoods = ZString.Empty;

				Factory.Save();
				dec.JE_CustomsOffice = "QXX!";
				dec.JE_LocationOfGoods = "QSS!";
				dec.JE_RL_NKPortOfArrival = "!ZZ";
				AssertEquals("Existing Customs port value not changed if declaration is in database", "QXX!", dec.JE_CustomsOffice);
				AssertEquals("Existing Customs port value not changed if declaration is in database", "QSS!", dec.JE_LocationOfGoods);

				dec.JE_RL_NKPortOfArrival = ZString.Empty;
				dec.JE_CustomsOffice = ZString.Empty;
				dec.JE_LocationOfGoods = ZString.Empty;
				dec.JE_RL_NKPortOfArrival = "!ZZ";
				AssertEquals("Default from JE_RL_NKPortOfArrival", "PZZ!", dec.JE_CustomsOffice);
				AssertEquals("Default from JE_RL_NKPortOfArrival", "SUB!", dec.JE_LocationOfGoods);
			}
		}

		public void TestJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0452", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPortOfExit, "3801");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "0452";
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("CA_USPortOfExit", ZString.Empty, invoice.CA_USPortOfExit);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			invoice.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			declaration.JE_CustomsOfficeInfo.RefreshBinding();
			AssertEquals("CA_USPortOfExit", "3801", invoice.CA_USPortOfExit);

			declaration.JE_CustomsOffice = "0705";
			AssertEquals("CA_USPortOfExit", "3801", invoice.CA_USPortOfExit);

			invoice.CA_USPortOfExit = ZString.Empty;
			declaration.JE_CustomsOffice = "0495";
			AssertEquals("CA_USPortOfExit", ZString.Empty, invoice.CA_USPortOfExit);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_CustomsOffice = "0452";
			AssertEquals("CA_USPortOfExit", ZString.Empty, invoice.CA_USPortOfExit);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveP;
			declaration.JE_CustomsOfficeInfo.RefreshBinding();
			AssertEquals("CA_USPortOfExit", ZString.Empty, invoice.CA_USPortOfExit);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			invoice.CA_RN_NKExport = Constants.CountryCodes.UnitedKingdom;
			declaration.JE_CustomsOfficeInfo.RefreshBinding();
			AssertEquals("CA_USPortOfExit", ZString.Empty, invoice.CA_USPortOfExit);
		}

		public void TestPortOfClearanceAndPortOfClearanceRelatedUSPortOfExit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "CA Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPortOfExit, "3801");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "US Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "1111", "CA Customs Port Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "CA Customs Office Code (Expired)", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			AssertNull(declaration.PortOfClearance);
			AssertEquals(ZString.Empty, declaration.PortOfClearanceRelatedUSPortOfExit);

			declaration.JE_CustomsOffice = "1111";

			var portOfClearance = declaration.PortOfClearance;
			AssertNotNull(portOfClearance);
			AssertType<ZZRefCusCodeListCombined>(portOfClearance);
			AssertEquals("CA Customs Office Code", portOfClearance.ZZD_Description);
			AssertEquals("3801", declaration.PortOfClearanceRelatedUSPortOfExit);
		}

		public void TestDefaultingFromCustomsPort()
		{
			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "!ZZ";
			var locoMap1 = Factory.New<RefLocoMap>();
			locoMap1.RY_LocalPortCode = "PZZ!";
			locoMap1.RY_RL_NKLocoPort = "!ZZ";
			locoMap1.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap1.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;

			var locoMap2 = Factory.New<RefLocoMap>();
			locoMap2.RY_LocalPortCode = "PYY!";
			locoMap2.RY_RL_NKLocoPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			locoMap2.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap2.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;

			try
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				dec.JE_GB = GlbBranch.CurrentBranch.PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				dec.JE_CustomsOffice = "PZZ!";
				AssertEquals("Arrival port defaults if transport mode is SEA or AIR", "!ZZ", dec.JE_RL_NKPortOfArrival);
				dec.JE_CustomsOffice = ZString.Empty;
				AssertEquals("Clearing customs port does no change existing arrival port", "!ZZ", dec.JE_RL_NKPortOfArrival);
				dec.JE_RL_NKPortOfArrival = "!QQ";
				dec.JE_CustomsOffice = "PZZ!";
				AssertEquals("Existing arrival port value changed if declaration is not in database", "!ZZ", dec.JE_RL_NKPortOfArrival);
				dec.JE_TransportMode = Core.Constants.TransportModes.Road;
				dec.JE_CustomsOffice = ZString.Empty;
				dec.JE_RL_NKPortOfArrival = "!QQ";
				dec.JE_CustomsOffice = "PZZ!";
				AssertEquals("Arrival port does not default from the home port of the branch if transport mode is not SEA or AIR", "!QQ", dec.JE_RL_NKPortOfArrival);

				Factory.Save();
				dec.JE_CustomsOffice = ZString.Empty;
				dec.JE_RL_NKPortOfArrival = "!QQ";
				dec.JE_CustomsOffice = "PZZ!";
				AssertEquals("Existing arrival port value not changed", "!QQ", dec.JE_RL_NKPortOfArrival);

				dec.JE_RL_NKPortOfArrival = ZString.Empty;
				dec.JE_CustomsOffice = ZString.Empty;
				var shipment = Factory.New<ForwardingShipment>();
				dec.JE_JS = shipment.PK;
				dec.JE_CustomsOffice = "PZZ!";
				Assert("Arrival port does not default when attached to shipment", dec.JE_RL_NKPortOfArrival.IsEmpty);
			}
			finally
			{
				locoMap2.Delete();
			}
		}

		public void TestContainerNumbers()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.G7Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container1 = dec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "APLU12345678";
			var container2 = dec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "APLU87654321";

			AssertEquals("APLU12345678", dec.CusContainers[0].CO_ContainerNumber);
			AssertEquals("APLU87654321", dec.CusContainers[1].CO_ContainerNumber);
		}

		public void TestInvoiceNumbers()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.G7Export;
			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV12345678";
			var invoice2 = dec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV87654321";
			AssertEquals("INV12345678,INV87654321", dec.InvoiceNumbers);
		}

		public void TestCA_PortOfExit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CA_PortOfExit = "1";
			AssertEquals("Port of Exit zero padding", "0001", declaration.CA_PortOfExit);
			declaration.CA_PortOfExit = "11";
			AssertEquals("Port of Exit zero padding", "0011", declaration.CA_PortOfExit);
			declaration.CA_PortOfExit = "111";
			AssertEquals("Port of Exit zero padding", "0111", declaration.CA_PortOfExit);
			declaration.CA_PortOfExit = "1111";
			AssertEquals("Port of Exit zero padding", "1111", declaration.CA_PortOfExit);
		}

		public void TestCA_PlaceOfReport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CA_PlaceOfReport = "1";
			AssertEquals("Place of Report zero padding", "0001", declaration.CA_PlaceOfReport);
			declaration.CA_PlaceOfReport = "11";
			AssertEquals("Place of Report zero padding", "0011", declaration.CA_PlaceOfReport);
			declaration.CA_PlaceOfReport = "111";
			AssertEquals("Place of Report zero padding", "0111", declaration.CA_PlaceOfReport);
			declaration.CA_PlaceOfReport = "1111";
			AssertEquals("Place of Report zero padding", "1111", declaration.CA_PlaceOfReport);
		}

		public void TestGetPortOfficeFromUnLoco()
		{
			var provinceON = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "ON"));
			var provinceAB = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "AB"));

			var office = helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, RefCusCodeListTypes.CustomsOffice, "0001", "St. John's Place", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(office.PK, Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceON.RW_Code);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			var unloco = CreateUnLocoForTest("St. John's Place", provinceON);
			AssertEquals($"Exact match by description ({office.ZZD_Code} = {unloco.RL_PortName})", office.ZZD_Code, declaration.GetPortOfficeFromUnLoco(unloco));

			unloco = CreateUnLocoForTest("St. John's Place", provinceAB);
			AssertEquals($"Don't match if provinces are not the same", ZString.Empty, declaration.GetPortOfficeFromUnLoco(unloco));

			unloco = CreateUnLocoForTest("St John's Place", provinceON);
			AssertEquals($"Approx. match by description ({office.ZZD_Code} = {unloco.RL_PortName})", office.ZZD_Code, declaration.GetPortOfficeFromUnLoco(unloco));

			unloco = CreateUnLocoForTest("Saint-John's Place", provinceON);
			AssertEquals($"Approx. match by description ({office.ZZD_Code} = {unloco.RL_PortName})", office.ZZD_Code, declaration.GetPortOfficeFromUnLoco(unloco));

			unloco = CreateUnLocoForTest("Saint John's Place", provinceON);
			AssertEquals($"Approx. match by description ({office.ZZD_Code} = {unloco.RL_PortName})", office.ZZD_Code, declaration.GetPortOfficeFromUnLoco(unloco));

			unloco = CreateUnLocoForTest("Saint Patricks's Place", provinceON);
			AssertEquals($"Don't match if descriptions differ", ZString.Empty, declaration.GetPortOfficeFromUnLoco(unloco));

			unloco = CreateUnLocoForTest("St. John's Place", provinceON, CountryCodes.Mexico);
			AssertEquals($"Don't match if country is not Canada", ZString.Empty, declaration.GetPortOfficeFromUnLoco(unloco));

			RefUNLOCO CreateUnLocoForTest(ZString portName, RefCountryStates province, string country = CountryCodes.Canada)
			{
				var unloco = Factory.New<RefUNLOCO>();
				unloco.RL_RN_NKCountryCode = country;
				unloco.RL_RW = province.PK;
				unloco.RL_PortName = portName;
				return unloco;
			}
		}

		public void TestGetDescriptionFromPortOfficeCode()
		{
			helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, RefCusCodeListTypes.CustomsOffice, "0001", "St. John's Place", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, RefCusCodeListTypes.CustomsOffice, "0002", "St. Andrew's Place", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, RefCusCodeListTypes.CustomsOffice, "0003", "St. Patrick's Place", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var officeDescription = declaration.GetDescriptionFromPortOfficeCode("0002");
			AssertEquals("St. Andrew's Place", officeDescription);
			officeDescription = declaration.GetDescriptionFromPortOfficeCode("0003");
			AssertEquals("St. Patrick's Place", officeDescription);
			officeDescription = declaration.GetDescriptionFromPortOfficeCode("");
			AssertEquals("", officeDescription);
			officeDescription = declaration.GetDescriptionFromPortOfficeCode(null);
			AssertEquals("", officeDescription);
		}

		public void TestJE_DateOfFirstArrival()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_DateOfFirstArrivalInfo);
				AssertEquals("Caption", "Arrival Date at First Port of Arrival", resourceStrings.Caption);
				AssertEquals("ShortCaption", "ETA", resourceStrings.ShortCaption);
				AssertEquals("FullDescription", "The Date of Importation at the First Port of Arrival.", resourceStrings.FullDescription);
			});
		}

		#region B2 Adjustment

		public void TestCA_B2SubmissionDateOverride()
		{
			var b3X = Factory.New<JobDeclaration>();
			b3X.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3X.CA_B2Type = B2TypeList.Codes.Blanket;

			Assert(b3X.CA_B2SubmissionDateInfo.ReadOnly);
			b3X.CA_B2SubmissionDateOverride = true;
			Assert(!b3X.CA_B2SubmissionDateInfo.ReadOnly);

			var message1 = b3X.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2020, 12, 27);

			var lastSentMessage = b3X.Messages.AddNew();
			lastSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			lastSentMessage.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
			lastSentMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 12, 28);

			b3X.CA_B2SubmissionDateOverride = false;
			b3X.CA_B2SubmissionDate = new ZDateTime(2020, 12, 29);
			Assert(b3X.CA_B2SubmissionDateOverride);
			b3X.CA_B2SubmissionDate = new ZDateTime(2020, 12, 28);
			Assert(!b3X.CA_B2SubmissionDateOverride);
		}

		public void TestCA_B2AcceptedDateOverride()
		{
			var b3X = Factory.New<JobDeclaration>();
			b3X.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3X.CA_B2Type = B2TypeList.Codes.Blanket;

			Assert(b3X.CA_B2AcceptedDateInfo.ReadOnly);
			b3X.CA_B2AcceptedDateOverride = true;
			Assert(!b3X.CA_B2AcceptedDateInfo.ReadOnly);

			var message1 = b3X.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
			message1.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2020, 12, 27);

			var lastRecievedMessage = b3X.Messages.AddNew();
			lastRecievedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			lastRecievedMessage.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
			lastRecievedMessage.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			lastRecievedMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 12, 28);

			b3X.CA_B2AcceptedDateOverride = false;
			b3X.CA_B2AcceptedDate = new ZDateTime(2020, 12, 29);
			Assert(b3X.CA_B2AcceptedDateOverride);
			b3X.CA_B2AcceptedDate = new ZDateTime(2020, 12, 28);
			Assert(!b3X.CA_B2AcceptedDateOverride);
		}

		public void TestB2BlanketFields()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Blanket;
			AssertEquals("VAR", b2.JE_CustomsOffice);
			Assert(b2.JE_CustomsOfficeInfo.ReadOnly);
			AssertEquals("VAR", b2.CA_OriginalTransactionNo);
			Assert(b2.CA_OriginalTransactionNoInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, b2.CA_K84AccountingDate);
			Assert(!b2.CA_K84AccountingDateInfo.ReadOnly);
		}

		public void TestIsB2Adjustments()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!declaration.IsB2Adjustments);
			AssertNull(declaration.B2AsClaimedForInvoiceGroupHeader);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Assert(declaration.IsB2Adjustments);
			AssertNotNull(declaration.B2AsClaimedForInvoiceGroupHeader);
		}

		public void TestOriginalLodgedB3MessageWrapperAndAccountingDate()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0001");
			var b3Dec = Factory.New<JobDeclaration>();
			b3Dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			b3Dec.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveP;
			b3Dec.JE_OH_Importer = importer.PK;
			b3Dec.TransactionNumber.AccountSecurityCode = "40000";
			b3Dec.TransactionNumber.SequentialNumber = "04228";
			b3Dec.CA_K84AccountingDate = new ZDateTime(2012, 05, 29);
			var invoice = b3Dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			b3Dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			b3Dec.DoMerge();
			Factory.Save();
			var b2Dec = Factory.New<JobDeclaration>();
			b2Dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Dec.CA_B2Type = B2TypeList.Codes.Specific;
			b2Dec.CA_OriginalTransactionNo = b3Dec.DeclarationNumber;
			var wrapperAndDate = b2Dec.OriginalLodgedB3MessageWrapperAndAccountingDate;
			AssertEquals("B3ImportMessageWrapper", typeof(B3ImportMessageWrapper), wrapperAndDate.Item1.GetType());
			AssertEquals("CA_K84AccountingDate", b3Dec.CA_K84AccountingDate, wrapperAndDate.Item2);
		}

		[TestDate(2012, 05, 21)]
		public void TestSeedingB2()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Japan, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "2402100010", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "7326909091", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "7326909091");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "7326909091");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			#region interchangeText

			const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+120529:0443+8228
UNG+CUSDEC+U10207V1+KI+120529:0443+1232+UN+S:99B+10207YUSENT
UNH+1088+CUSDEC:S:99B:UN
BGM+:::AB+659+9
LOC+41+495
LOC+11+495
RFF+TN:000002752
RFF+ARA:842957342RM0001
TDT+11++9++9165
DOC+785+8010925243
DTM+204:20120521:102
MOA+43:55140'UNS+D
DMS+1'MOA+64:125
NAD+SE++P.DON INTERNATIONAL TRANSPORT ++++PR
DOC+935
DTM+129:20120315:102
LOC+27+JP+UIL+3901
PAT+1+CONSIGN:::02'MOA+6::USD
CST+1+POS+1+2402100010+23
MOA+40:109044
MOA+43:108117
MOA+125:128026
RFF+LI:1:1
MOA+38:100000
TAX+1+EXC++0.067
MOA+161:2010
TAX+7+VAT++5.0
MOA+1:6401
GIR+1+1
MEA+AAR++MIL:5000
MEA+AAA++KGM:50
TAX+5+++18.50
MOA+155:9250
GIR+1+1
MEA+AAR++NMB:300000
TAX+5+++8.0
MOA+155:8649
CST+2+POS+1+7326909091+13
MOA+40:5452177
MOA+43:5405833
MOA+125:5807212
RFF+LI:1:2
MOA+38:5000000
TAX+1+ADD++51
MOA+46:50000
TAX+7+VAT++5.0
MOA+1:290361
GIR+1+2
TAX+5+++6.5
MOA+155:351379
UNS+S
TAX+5+:::K90
MOA+155:369278
TAX+1+:::K90
MOA+105:50000
TAX+3+:::K90
MOA+4:2010
TAX+7+:::K90
MOA+1:296762
TAX+4+:::K90
MOA+176:718050
UNT+63+1088
UNE+1+1232
UNZ+1+8228
";
			const string refData1 = @"
RA107326909091
RA20732690909190020110815999999992011-08--COLT   NN2011-08--COLT2011081599999999NN
RA3073269090912011081599999999N   N2011-08--COLT
RA40V000650000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
RA40V003500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000003N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000007N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000008N
RA40V000300000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000009N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000010N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000011N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000012N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000013N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000014N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000021N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000022N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000023N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000024N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000025N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000026N
RA520012008010199999999Y1V000500000   NORMAL RATE                                                 N
RA520591991010199999999Y2E000000000   FOOD OR BEVERAGES FOR HUMAN CONSUMPTION                     N
";
			const string refData2 = @"
RA102402100010
RA20240210001090020110815999999992011-08--COLTMILNN2011-08--COLT2011081599999999NN
RA3024021000102011081599999999N   N2011-08--COLT
RA40V000800000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
RA40V003500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000003N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000007N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000008N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000009N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000010N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000011N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000014N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000021N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000022N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000025N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000026N
RA520012008010199999999Y1V000500000   NORMAL RATE                                                 N
RA54E012008010199999999V006700000   CIGARS SEE ALSO E7 & E21 FOR MIN RATE                       N
RA54E072008010199999999S006700000MILCIGARS SEE ALSO E1 & E21                                    N
RA54E212008010199999999S000006700NMBCIGARS SEE ALSO E1 & E7 FOR MIN RATE                        N
RA6024021000102011081599999999NMILS001850000000000000000000000 000000000000000000000000000N
";

			#endregion

			var parser = new CACTestingDataHelper.CadexMessageProcessor();
			parser.ProcessRA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(refData1))));
			parser.ProcessRA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(refData2))));
			var factory = ((IFactoryProvider)parser).Factory;

			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0001");

			var b3Dec = factory.NewWithValidTestData<JobDeclaration>();
			b3Dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			b3Dec.TransactionNumber.AccountSecurityCode = "40000";
			b3Dec.TransactionNumber.SequentialNumber = "04228";
			b3Dec.CA_K84AccountingDate = new ZDateTime(2012, 05, 29);

			var b3Invoice = b3Dec.Invoices.AddNew();
			var b3InvoiceLine = b3Invoice.JobComInvoiceLines.AddNew();
			b3InvoiceLine.JI_Tariff = "123456789";

			b3Dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			b3Dec.DoMerge();
			var entryHeader = b3Dec.B3EntryHeader;

			factory.Save();

			var message = B3AsLodgedDocumentWrapperTest.CreateMessageFromInterchangeString(factory, interchangeText);
			message.EM_SystemCreateTimeUtc = new ZDateTime(2012, 07, 03);
			message.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			entryHeader.Messages.Add(message);

			var acceptedResponse = factory.New<B3Message>();
			acceptedResponse.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			acceptedResponse.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			acceptedResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			acceptedResponse.EM_SystemCreateTimeUtc = message.EM_SystemCreateTimeUtc.AddHours(1);
			entryHeader.Messages.Add(acceptedResponse);

			var b2Dec = factory.New<JobDeclaration>();
			b2Dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Dec.CA_B2Type = B2TypeList.Codes.Specific;
			b2Dec.CA_OriginalTransactionNo = b3Dec.DeclarationNumber;

			AssertEquals("Importer", importer.PK, b2Dec.JE_OH_Importer);
			AssertEquals("Port of Clearance", "0495", b2Dec.JE_CustomsOffice);
			AssertEquals("Accounting Date", new ZDateTime(2012, 05, 29), b2Dec.CA_OriginalAccountingDate);
			AssertEquals("Release Date", new ZDateTime(2012, 05, 21), b2Dec.JE_EntryAuthorisationDate);

			var coll = b2Dec.GetOriginalB3Lines();
			AssertEquals(2, coll.Count);
			coll[0].IsSelected = true;
			coll[1].IsSelected = true;

			b2Dec.SeedingB2(coll);
			AssertEquals(1, b2Dec.B2AsAccountedForInvoices.Count);
			var invoice = b2Dec.B2AsAccountedForInvoices[0];
			AssertEquals("Sub header no.", "1", invoice.JZ_InvoiceNumber);
			AssertEquals("Country/Region of Origin", "JP", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("Country/Region Of Export", "US", invoice.CA_RN_NKExport);
			AssertEquals("Trade Zone", "", invoice.CA_TradeZone);
			AssertEquals("State of export", "IL", invoice.CA_USStateOfExport);
			AssertEquals("Treatment code", "02", invoice.CA_TreatmentCode);
			AssertEquals("Date of Direct shipment", new ZDateTime(2012, 03, 15).Date, invoice.JZ_ValuationDateOverride.Date);
			AssertEquals("Invoice Currency", "USD", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("Time limit", 0, invoice.CA_TimeLimit);
			AssertEquals("Time limit code", ZString.Empty, invoice.CA_TimeLimitCode);
			AssertEquals(JobComInvoiceGroupHeader.AllInvoices, invoice.GroupHeader.JZ_InvoiceNumber);

			AssertEquals(2, invoice.AsAccountForFilteredInvoiceLines.Count);

			var asAccountForLine1 = (JobComInvoiceLine)invoice.AsAccountForFilteredInvoiceLines.FirstOrDefault(l => ((JobComInvoiceLine)l).CA_OriginalLineNo == "1");
			var asClaimForLine1 = asAccountForLine1.CorrespondingAsClaimedForInvoiceLine;

			AssertNotEquals("Create a new InvoiceHeader.", asAccountForLine1.InvoiceHeader.PK, asClaimForLine1.InvoiceHeader.PK);
			AssertEquals(JobComInvoiceGroupHeader.AsClaimed, asClaimForLine1.InvoiceHeader.GroupHeader.JZ_InvoiceNumber);

			foreach (var line in new[] { asAccountForLine1, asClaimForLine1 })
			{
				AssertEquals("Original Line no.", "1", line.CA_OriginalLineNo);
				AssertEquals("Sub Header", "1", line.JI_Calc_Invoice);
				AssertEquals("Classification", "2402100010", line.JI_Tariff);
				AssertEquals("Authority No", ZString.Empty, line.CA_AuthorityNumber);
				AssertEquals("Tariff Code", ZString.Empty, line.CA_99TariffCode);
				AssertEquals("Value For Duty", 1515.71m, line.CA_CustomsValue);
				AssertEquals("Value for duty code", "23", line.CA_ValueForDutyCode);
				AssertEquals("Customs Qty", 5m, line.JI_CustomsQuantity);
				AssertEquals("Customs Qty unit", "MIL", line.JI_CustomsUnitQty);
				AssertEquals("Qty2", 300m, line.JI_CustomsSecondQuantity);
				AssertEquals("Qty2 Unit", "NMB", line.JI_CustomsSecondUnitQty);
				AssertEquals("Qty3", 5m, line.JI_CustomsThirdQuantity);
				AssertEquals("Qty3 Unit", "MIL", line.JI_CustomsThirdUnitQty);
				AssertEquals("Conversion Value", 1090.44m, line.CA_CVforCurrConv);
			}

			var gst = asAccountForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			Assert(gst.C1_Override);
			AssertEquals("Rate of GST", 5m, gst.C1_Rate);
			AssertEquals("GST amount", 64.01m, gst.C1_Amount);

			var excise = asAccountForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertEquals("Excise tax rate", 0.067m, excise.C1_Rate);
			AssertEquals("Excise Tax Amount", 20.10m, excise.C1_Amount);

			var duty = asAccountForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty
				&& l.C1_RateType == RateTypes.Codes.AdValorem && l.C1_Rate == 8m);
			Assert(duty.C1_Override);
			AssertEquals("Ad valorem duty", 86.49m, duty.C1_Amount);

			duty = asAccountForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty
				&& l.C1_RateType == RateTypes.Codes.Specific && l.C1_Rate == 18.5m);
			Assert(duty.C1_Override);
			AssertEquals("Specific duty", 92.5m, duty.C1_Amount);

			var gstForAsClaimForLine = asClaimForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertEquals("Rate of GST", 5m, gstForAsClaimForLine.C1_Rate);
			AssertEquals("GST amount", 64.01m, gstForAsClaimForLine.C1_Amount);

			var exciseForAsClaimForLine = asClaimForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertEquals("Excise tax rate", 0.067m, exciseForAsClaimForLine.C1_Rate);
			AssertEquals("Excise Tax Amount", 0m, exciseForAsClaimForLine.C1_Amount);

			var dutyForAsClaimForLine = asClaimForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty
				&& l.C1_RateType == RateTypes.Codes.AdValorem && l.C1_Rate == 8m);
			AssertEquals("Ad valorem duty", 86.49m, dutyForAsClaimForLine.C1_Amount);

			dutyForAsClaimForLine = asClaimForLine1.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty
				&& l.C1_RateType == RateTypes.Codes.Specific && l.C1_Rate == 18.5m);
			AssertEquals("Specific duty", 92.5m, dutyForAsClaimForLine.C1_Amount);

			var asAccountForLine2 = (JobComInvoiceLine)invoice.AsAccountForFilteredInvoiceLines.FirstOrDefault(l => ((JobComInvoiceLine)l).CA_OriginalLineNo == "2");
			var asClaimForLine2 = asAccountForLine2.CorrespondingAsClaimedForInvoiceLine;

			AssertEquals("Do not create InvoiceHeader.", asClaimForLine1.InvoiceHeader.PK, asClaimForLine2.InvoiceHeader.PK);

			foreach (var line in new[] { asAccountForLine2, asClaimForLine2 })
			{
				AssertEquals("Original Line no.", "2", line.CA_OriginalLineNo);
				AssertEquals("Sub Header", "1", line.JI_Calc_Invoice);
				AssertEquals("Classification", "7326909091", line.JI_Tariff);
				AssertEquals("Authority No", ZString.Empty, line.CA_AuthorityNumber);
				AssertEquals("Tariff Code", ZString.Empty, line.CA_99TariffCode);
				AssertEquals("Value For Duty", 75785.26m, line.CA_CustomsValue);
				AssertEquals("Value for duty code", "13", line.CA_ValueForDutyCode);
				AssertEquals("Customs Qty", 0m, line.JI_CustomsQuantity);
				AssertEquals("Customs Qty unit", ZString.Empty, line.JI_CustomsUnitQty);
				AssertEquals("Qty2", 0m, line.JI_CustomsSecondQuantity);
				AssertEquals("Qty2 Unit", ZString.Empty, line.JI_CustomsSecondUnitQty);
				AssertEquals("Qty3", 0m, line.JI_CustomsThirdQuantity);
				AssertEquals("Qty3 Unit", ZString.Empty, line.JI_CustomsThirdUnitQty);
				AssertEquals("Conversion Value", 54521.77m, line.CA_CVforCurrConv);
			}

			var sima = asAccountForLine2.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.SIMADuty);
			Assert("SIMA not override", !sima.C1_Override);
			AssertEquals("sima exempt code", "51", sima.C1_ExemptCode);
			AssertEquals("sima assessment", 500m, sima.C1_Amount);

			gst = asAccountForLine2.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			Assert(gst.C1_Override);
			AssertEquals("Rate of GST", 5m, gst.C1_Rate);
			AssertEquals("GST amount", 2903.61m, gst.C1_Amount);

			duty = asAccountForLine2.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty
				 && l.C1_RateType == RateTypes.Codes.AdValorem && l.C1_Rate == 6.5m);
			Assert(duty.C1_Override);
			AssertEquals("Ad valorem duty", 3513.79m, duty.C1_Amount);

			gstForAsClaimForLine = asClaimForLine2.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertEquals("Rate of GST", 5m, gst.C1_Rate);
			AssertEquals("GST amount", 2903.61m, gst.C1_Amount);

			dutyForAsClaimForLine = asClaimForLine2.DutiesAndTaxes.First(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty
				 && l.C1_RateType == RateTypes.Codes.AdValorem && l.C1_Rate == 6.5m);
			AssertEquals("Ad valorem duty", 3513.79m, duty.C1_Amount);
		}

		public void TestCreateAsClaimedInvoiceGroupHeaderForB3X()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(JobComInvoiceGroupHeader.AsClaimed, dec.B2AsClaimedForInvoiceGroupHeader.JZ_InvoiceNumber);
		}

		public void TestAsAccountForInvoiceLinesDoNotChange()
		{
			var b2Dec = Factory.New<JobDeclaration>();
			b2Dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Dec.CA_B2Type = B2TypeList.Codes.Specific;

			var originalImporter = Factory.NewWithValidTestData<OrgHeader>();
			var newImporter = Factory.NewWithValidTestData<OrgHeader>();
			b2Dec.JE_OH_Importer = originalImporter.PK;

			var invoice = b2Dec.Invoices.AddNew();
			var asClaimForLine = invoice.JobComInvoiceLines.AddNew();
			asClaimForLine.JI_CustomsQuantity = 100.0;
			asClaimForLine.CA_IsAccountForLine = true;

			var asAccountForLine = invoice.JobComInvoiceLines.AddNew();
			asAccountForLine.JI_CustomsQuantity = 20.0;
			asAccountForLine.CA_IsAccountForLine = false;
			Factory.Save();
			Assert("Original values of As Claim For and As Account For lines", b2Dec.InvoiceLines[0].JI_CustomsQuantity == 100.0 && b2Dec.InvoiceLines[1].JI_CustomsQuantity == 20.0);

			b2Dec.JE_OH_Importer = newImporter.PK;
			Assert("As Claim For and As Account For lines do not change", b2Dec.InvoiceLines[0].JI_CustomsQuantity == 100.0 && b2Dec.InvoiceLines[1].JI_CustomsQuantity == 20.0);
		}

		#endregion

		#region OGD Data

		public void TestCA_OGDIC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDIC = true;
			var line = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.InvoiceLines.RunSITTValidation();
			AssertNoMessageErrors(line.CA_ImportReasonCodeSITTInfo);
			AssertNoMessageErrors(line.CA_ModelSITTInfo);
			AssertNoMessageErrors(line.CA_BrandNameSITTInfo);
			line.CA_Model = "X";
			declaration.InvoiceLines.RunSITTValidation();
			AssertHasMessageErrors(line.CA_ImportReasonCodeSITTInfo);
			AssertHasMessageErrors(line.CA_BrandNameSITTInfo);
			line.CA_Model = ZString.Empty;
			line.JI_BrandName = "X";
			declaration.InvoiceLines.RunSITTValidation();
			AssertHasMessageErrors(line.CA_ModelSITTInfo);
			declaration.CA_OGDIC = false;
			AssertNoMessageErrors(line.CA_ImportReasonCodeSITTInfo);
			AssertNoMessageErrors(line.CA_ModelSITTInfo);
			AssertNoMessageErrors(line.CA_BrandNameSITTInfo);
		}

		public void TestCA_OGDNR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDNR = true;
			var line = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.InvoiceLines.RunNRCANValidation();
			AssertNoMessageErrors(line.CA_BrandNameNRInfo);
			AssertNoMessageErrors(line.CA_ModelNRInfo);
			AssertNoMessageErrors(line.CA_TypeSizeNRInfo);
			line.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan);
			declaration.InvoiceLines.RunNRCANValidation();
			AssertHasMessageErrors(line.CA_BrandNameNRInfo);
			AssertHasMessageErrors(line.CA_ModelNRInfo);
			AssertHasMessageErrors(line.CA_TypeSizeNRInfo);
			declaration.CA_OGDNR = false;
			AssertNoMessageErrors(line.CA_BrandNameNRInfo);
			AssertNoMessageErrors(line.CA_ModelNRInfo);
			AssertNoMessageErrors(line.CA_TypeSizeNRInfo);
		}

		public void TestCA_OGDTC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var line = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDTC = true;
			AssertNoMessageErrors(line.CA_ImportReasonCodeTCInfo);
			AssertNoMessageErrors(line.CA_TypeSizeTCInfo);
			line.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC);
			declaration.InvoiceLines.RunTiresValidation();
			AssertHasMessageErrors(line.CA_ImportReasonCodeTCInfo);
			AssertHasMessageErrors(line.CA_TypeSizeTCInfo);
			declaration.CA_OGDTC = false;
			AssertNoMessageErrors(line.CA_ImportReasonCodeTCInfo);
			AssertNoMessageErrors(line.CA_TypeSizeTCInfo);
		}

		#endregion

		#region Doc Addresses

		public void TestJobDocAddressValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ImportJobDocAddressValidation", typeof(ImportJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("ExportJobDocAddressValidation", typeof(ExportJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
		}

		public void TestVendorDocAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var vendorDocAddress = declaration.VendorDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.VendorDocAddress)", vendorDocAddress);
			AssertEquals(DocAddressType.SellingParty, vendorDocAddress.DocAddressType);
			AssertEquals(ContactType.Administration, vendorDocAddress.DefaultContactType);

			vendorDocAddress.Delete();
			AssertNotNull(declaration.VendorDocAddress);
			Assert(!declaration.VendorDocAddress.IsDeleted);
		}

		#endregion

		#region Booleans

		[TestDate(2013, 5, 1)]
		public void TestIsLowValueNormalReleaseJob()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.IsLowValueNormalReleaseJob);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2000m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 1, 1);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;
			Assert(!declaration.IsLowValueNormalReleaseJob);
			entryHeader.ResetTotalsAndCachedValues();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			Assert(!declaration.IsLowValueNormalReleaseJob);
			entryHeader.ResetTotalsAndCachedValues();
			invoiceLine.JI_LinePrice = 2000m;
			Factory.Save();
			Assert("Ignore the jobs before 2013-01-07", !declaration.IsLowValueNormalReleaseJob);
			entryHeader.ResetTotalsAndCachedValues();
			line.CL_CustomsValue = 2501m;
			Assert(!declaration.IsLowValueNormalReleaseJob);

			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			line.CL_CustomsValue = 1000m;

			entryHeader.ResetTotalsAndCachedValues();
			invoiceLine.JI_LinePrice = 500m;
			declaration.ApportionmentDirty = false;
			Assert(declaration.IsLowValueNormalReleaseJob);

			entryHeader.ResetTotalsAndCachedValues();
			invoiceLine.JI_LinePrice = 2000m;
			declaration.ApportionmentDirty = false;
			Assert(declaration.IsLowValueNormalReleaseJob);

			entryHeader.ResetTotalsAndCachedValues();
			invoiceLine.JI_LinePrice = 5000m;
			declaration.ApportionmentDirty = false;
			Assert(declaration.IsLowValueNormalReleaseJob);
		}

		public void TestIsLVSTotalConsolidation()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			Assert(!dec.IsLVSTotalConsolidation);
			dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			dec.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			Assert(!dec.IsLVSTotalConsolidation);
			dec.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			Assert(dec.IsLVSTotalConsolidation);
		}

		public void TestIsB3NotRequired()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			Assert(dec.IsB3NotRequired);
		}

		public void TestIsB3NotMessageAllowedToSend()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_MessageSubType = ZString.Empty;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
			dec.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
			dec.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
			dec.JE_MessageSubType = B3EntryTypeList.Codes.CashC;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
			dec.JE_MessageSubType = B3EntryTypeList.Codes.CashD;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
			dec.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
			dec.JE_MessageSubType = B3EntryTypeList.Codes.Postal;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
			dec.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			Assert(dec.IsB3CADNotMessageAllowedToSend);
		}

		public void TestIsImporterPaysFlagged()
		{
			var dec = Factory.New<JobDeclaration>();
			var importer = Factory.New<OrgHeader>();
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = true;
			dec.JE_OH_Importer = importer.PK;
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			Assert(dec.IsImporterPaysFlagged);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			Assert(dec.IsImporterPaysFlagged);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			Assert(!dec.IsImporterPaysFlagged);
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = false;
			dec.JE_OH_Importer = importer.PK;
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			Assert(dec.IsImporterPaysFlagged);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			Assert(!dec.IsImporterPaysFlagged);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			Assert(!dec.IsImporterPaysFlagged);
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsLVSImporterDirectPayment = true;
			dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			dec.JE_MessageSubType = "";
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			Assert(dec.IsImporterPaysFlagged);
		}

		public void TestIsImporterAutoRateDutyGSTAmount()
		{
			var dec = Factory.New<JobDeclaration>();
			var importer = Factory.New<OrgHeader>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
			var impoerterAddInfo = (OrgImpAddInfo)importer.CountryData.ImpAddInfo;

			impoerterAddInfo.ZO_IsImporterDirectPayment = true;
			impoerterAddInfo.ZO_IsHighImporterAutoDutyDirectAmts = true;
			AssertEquals("Broker pays Duty&GST on behalf of Importer HVS", false, dec.IsImporterDirectPaymentAutoRated);

			impoerterAddInfo.ZO_IsImporterDirectPayment = true;
			impoerterAddInfo.ZO_IsHighImporterAutoDutyDirectAmts = false;
			AssertEquals("Importer pays Duty&GST HVS", true, dec.IsImporterDirectPaymentAutoRated);

			impoerterAddInfo.ZO_IsImporterDirectPayment = false;
			impoerterAddInfo.ZO_IsHighImporterAutoDutyDirectAmts = false;
			AssertEquals("Broker pays Duty&GST HVS", false, dec.IsImporterDirectPaymentAutoRated);

			dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			dec.JE_MessageSubType = "";
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;

			impoerterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			impoerterAddInfo.ZO_IsLVSImporterAutoDutyDirectAmts = true;
			AssertEquals("Broker pays Duty&GST on behalf of Importer LVS", false, dec.IsImporterDirectPaymentAutoRated);

			impoerterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			impoerterAddInfo.ZO_IsLVSImporterAutoDutyDirectAmts = false;
			AssertEquals("Importer pays Duty&GST LVS", true, dec.IsImporterDirectPaymentAutoRated);

			impoerterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			impoerterAddInfo.ZO_IsLVSImporterAutoDutyDirectAmts = false;
			AssertEquals("Broker pays Duty&GST LVS", false, dec.IsImporterDirectPaymentAutoRated);
		}

		public void TestIsImporterDirectPaymentCore()
		{
			var dec = Factory.New<JobDeclaration>();
			var importer = Factory.New<OrgHeader>();
			var impoerterAddInfo = (OrgImpAddInfo)importer.CountryData.ImpAddInfo;
			impoerterAddInfo.ZO_IsImporterDirectPayment = true;
			dec.JE_OH_Importer = importer.PK;
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			Assert(dec.IsImporterDirectPayment);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			Assert(dec.IsImporterDirectPayment);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			Assert(!dec.IsImporterDirectPayment);
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = false;
			dec.JE_OH_Importer = importer.PK;
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			Assert(dec.IsImporterDirectPayment);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			Assert(!dec.IsImporterDirectPayment);
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			Assert(!dec.IsImporterDirectPayment);
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsLVSImporterDirectPayment = true;
			dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			dec.JE_MessageSubType = "";
			dec.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			Assert(dec.IsImporterDirectPayment);
		}

		public void TestJE_PaymentMethodReadOnly()
		{
			CACustomsDataRegistry.Instance.AllowPaymentPartyOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.True);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert(!declaration.JE_PaymentMethodInfo.ReadOnly);
			CACustomsDataRegistry.Instance.AllowPaymentPartyOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			Assert(!declaration.JE_PaymentMethodInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(declaration.JE_PaymentMethodInfo.ReadOnly);
		}

		public void TestIsImporterOrganizationDirect()
		{
			var dec = Factory.New<JobDeclaration>();
			var importer = Factory.New<OrgHeader>();
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = true;
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsLVSImporterDirectPayment = false;
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			dec.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			Assert(!dec.IsImporterOrganizationDirect);
			dec.JE_MessageSubType = "";
			AssertEquals(((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsLVSImporterDirectPayment, dec.IsImporterOrganizationDirect);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment, dec.IsImporterOrganizationDirect);
		}

		public void TestOtherGenericTransportModeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("Other mode descriptions", "Fixed Transport Installations", declaration.OtherGenericTransportModeDescription);
			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("Other mode descriptions", "Inland Waterways", declaration.OtherGenericTransportModeDescription);
			declaration.JE_TransportMode = TransportTypeList.Codes.NoCarrier;
			AssertEquals("Other mode descriptions", "No Carrier/Hand-Carried", declaration.OtherGenericTransportModeDescription);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("Other mode descriptions", ZString.Empty, declaration.OtherGenericTransportModeDescription);
		}

		public void TestDefaultExportServiceProviderIfRequired()
		{
			CACustomsDataRegistry.Instance.DefaultServiceProviderOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var company = GlbCompany.GetCurrentCompany(Factory);
			var orgProxy = company.OrgProxy;
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICENSE", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			var declaration = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Service provider should be set", orgProxy.PK, declaration.Forwarder.PK);
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull("Service provider should not be set", declaration.Forwarder);
			declaration.JE_OH_Supplier = orgProxy.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNull("Service provider should not be set", declaration.Forwarder);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNull("Service provider should not be set", declaration.Forwarder);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNull("Service provider should not be set", declaration.Forwarder);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Service provider should be set", orgProxy.PK, declaration.Forwarder.PK);

			var serviceProvider = Factory.New<OrgHeader>();
			CACustomsDataRegistry.Instance.DefaultServiceProviderOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceProvider.PK.ToGuid());
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			declaration.JE_OH_Supplier = serviceProvider.PK;
			AssertNull("Service provider should not be set", declaration.Forwarder);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Service provider should be set", serviceProvider.PK, declaration.Forwarder.PK);
		}

		public void TestExportLicenceProxy()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var companyOrg = Factory.NewWithValidTestData<OrgHeader>();
			companyOrg.OH_Code = "~GCORG";
			caCompany.GC_OH_OrgProxy = companyOrg.PK;
			var caBranch = caCompany.Branches.AddNew();
			var branchOrg = Factory.NewWithValidTestData<OrgHeader>();
			branchOrg.OH_Code = "~GBORG";
			caBranch.GB_OH_OrgProxy = branchOrg.PK;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = caBranch.PK;
			AssertNull("No licence set", declaration.ExportLicenceProxy);
			companyOrg.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICENSE", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			AssertEquals("Company proxy applies", companyOrg.PK, declaration.ExportLicenceProxy.PK);
			declaration.ResetCachedValuesForTest();
			branchOrg.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "YLICENSE", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			AssertEquals("Branch proxy applies", branchOrg.PK, declaration.ExportLicenceProxy.PK);
		}

		public void TestIsCompanyOrgProxyTheExporter()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICENSE", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			var declaration = Factory.New<JobDeclaration>();
			Assert("no supplier should not crash", !declaration.IsCompanyOrgProxyTheExporter);
			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			Assert("not exporter", !declaration.IsCompanyOrgProxyTheExporter);
			declaration.JE_OH_Supplier = companyProxy.PK;
			Assert("is exporter", declaration.IsCompanyOrgProxyTheExporter);
		}

		public void TestIsG7ExportDeclaration()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Assert("Not G7 Export", !declaration.IsG7ExportDeclaration);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("G7 Export", declaration.IsG7ExportDeclaration);
			declaration.JE_MessageType = ZString.Empty;
			Assert("Not G7 Export", !declaration.IsG7ExportDeclaration);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.NotSent;
			Assert("Not G7 Export", !declaration.IsG7ExportDeclaration);
			entryHeader.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			Assert("Not G7 Export", !declaration.IsG7ExportDeclaration);
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			Assert("G7 Export", declaration.IsG7ExportDeclaration);
		}

		public void TestIsDataLoadingModule()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("DLM Export", true, declaration.IsDataLoadingModule);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Not DLM Export", false, declaration.IsDataLoadingModule);
		}

		public void TestIsPackingInformationRelevant()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsPackingInformationRelevant for export", false, declaration.IsPackingInformationRelevant);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsPackingInformationRelevant for import", true, declaration.IsPackingInformationRelevant);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("IsPackingInformationRelevant for B3X", false, declaration.IsPackingInformationRelevant);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("IsPackingInformationRelevant for B2", false, declaration.IsPackingInformationRelevant);
		}

		public void TestIsFixedTransportInstallation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("IsFixedTransportInstallation", true, declaration.IsFixedTransportInstallation);
			declaration.JE_TransportMode = "ZZZ";
			AssertEquals("IsFixedTransportInstallation", false, declaration.IsFixedTransportInstallation);
			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsFixedTransportInstallation", false, declaration.IsFixedTransportInstallation);
		}

		public void TestIsInlandWaterwayTransport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("IsOther", true, declaration.IsInlandWaterwayTransport);
			declaration.JE_TransportMode = "ZZZ";
			AssertEquals("IsOther", false, declaration.IsFixedTransportInstallation);
			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsOther", false, declaration.IsInlandWaterwayTransport);
		}

		public void TestIsPARS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
			Assert("IsPARS", declaration.IsPARS);
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert("IsPARS", declaration.IsPARS);
			declaration.CA_ServiceOption = ServiceOptions.Codes.RMDOGD;
			Assert("Not IsPARS", !declaration.IsPARS);
			declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
			Assert("Not IsPARS", !declaration.IsPARS);
		}

		public void TestIsAQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
			Assert("IsAQ", declaration.IsAQ);
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			Assert("Not IsAQ", !declaration.IsAQ);
		}

		public void TestIsOGD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
			Assert("Not IsOGD", !declaration.IsOGD);
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert("IsOGD", declaration.IsOGD);
			declaration.CA_ServiceOption = ServiceOptions.Codes.RMDOGD;
			Assert("IsOGD", declaration.IsOGD);
			declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
			Assert("Not IsOGD", !declaration.IsOGD);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert("Not IsOGD", !declaration.IsOGD);
		}

		public void TestIsIID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			Assert("Not IsIID", !declaration.IsIID);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("IsIID", declaration.IsIID);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert("Not IsIID", !declaration.IsIID);
		}

		public void TestIsCSA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			Assert("Not IsCSA", !declaration.IsCSA);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			Assert("IsCSA", declaration.IsCSA);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert("Not IsCSA", !declaration.IsCSA);
		}

		[TestDate(2017, 1, 2)]
		public void TestIsArrivalDatePast()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("IsArrivalDatePast", !declaration.IsArrivalDatePast);
			declaration.JE_DateOfArrival = new ZDate(2017, 1, 1);
			Assert("IsArrivalDatePast", declaration.IsArrivalDatePast);
			declaration.JE_DateOfArrival = new ZDate(2017, 1, 3);
			Assert("IsArrivalDatePast", !declaration.IsArrivalDatePast);
		}

		public void TestIsWarehouseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("not a Warehouse Entry", !declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(false, declaration.IsOtherWarehouseEntry);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("is duty payable ExWarehouse Entry", declaration.IsExWarehouseAndDutyPayable);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			Assert("not a Warehouse Entry", !declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(false, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(false, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("is duty payable ExWarehouse Entry", declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
			Assert("Is Warehouse Entry", !declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(false, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("Is Warehouse Entry", declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(true, declaration.IsOtherWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Confirming;
			Assert("not a Warehouse Entry", !declaration.IsWarehouseEntry);
			Assert("not duty payable ExWarehouse Entry", !declaration.IsExWarehouseAndDutyPayable);
			AssertEquals(false, declaration.IsOtherWarehouseEntry);
		}

		public void TestIsPaperOnlyEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("not a Paper Entry", !declaration.IsPaperOnlyEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			Assert("Is a Paper Entry", declaration.IsPaperOnlyEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.CashC;
			Assert("Is a Paper Entry", declaration.IsPaperOnlyEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.CashD;
			Assert("Is a Paper Entry", declaration.IsPaperOnlyEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			Assert("Is a Paper Entry", declaration.IsPaperOnlyEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Postal;
			Assert("Is a Paper Entry", declaration.IsPaperOnlyEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			Assert("Is a Paper Entry", declaration.IsPaperOnlyEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			Assert("not a Paper Entry", !declaration.IsPaperOnlyEntry);
		}

		public void TestIsElectronicEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveP;
			Assert("IsElectronicEntry", !declaration.IsElectronicEntry);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Confirming;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("IsElectronicEntry", declaration.IsElectronicEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.AutomotiveP;
			Assert("IsElectronicEntry", !declaration.IsElectronicEntry);
		}

		public void TestIsWarehouseNotInType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("IsWarehouseNotInType", declaration.IsWarehouseNotInType);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
			Assert("IsWarehouseNotInType", !declaration.IsWarehouseNotInType);
		}

		public void TestIsTypeF()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			Assert("IsTypeF", !declaration.IsTypeF);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.LowValueShipments;
			Assert("IsTypeF", declaration.IsTypeF);
		}

		public void TestIsImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(declaration.JE_MessageType, declaration.IsImport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert(declaration.JE_MessageType, declaration.IsImport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			Assert(declaration.JE_MessageType, declaration.IsImport);
		}

		public void TestShouldPromptToSaveBuyerSupplierRelationship()
		{
			Env.Registry.PromptToSaveBuyerSupplier = true;
			var declaration = Factory.New<JobDeclaration>();
			var consumer = (IBuyerSupplierRelationshipConsumer)declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(declaration.JE_MessageType, true, consumer.ShouldPromptToSaveBuyerSupplierRelationship);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(declaration.JE_MessageType, true, consumer.ShouldPromptToSaveBuyerSupplierRelationship);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(declaration.JE_MessageType, false, consumer.ShouldPromptToSaveBuyerSupplierRelationship);
		}

		public void TestPreventBuyerSupplierRelationship()
		{
			Env.Registry.UseBuyerSupplierRelationships = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			var deliveryAddress = supplier.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "Supp addr 1";
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";
			var pickupAddress = importer.Addresses.AddNew();
			pickupAddress.OA_Address1 = "Imp Addr 1";
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			importer.SupplierLinks.AddNew(supplier);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Do not default the supplier for LVS declaration", ZGuid.Empty, declaration.JE_OH_Supplier);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Do not default the supplier for B2 declaration", ZGuid.Empty, declaration.JE_OH_Supplier);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Default the supplier for normal declaration", supplier.PK, declaration.JE_OH_Supplier);
		}

		#endregion

		public void TestCA_ServiceOption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_OGDCFIA = true;
			declaration.CA_OGDIC = true;
			declaration.CA_OGDNR = true;
			declaration.CA_OGDTC = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TotalNoOfPacksPackType = ACROSSPackageTypes.Codes.PACKAGE;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertEquals("Assessment option is 1", AssessmentOptions.Codes.AppraisalQualityData, declaration.CA_AssesmentOption);
			Assert("CA_OGDCFIA false", !declaration.CA_OGDCFIA);
			Assert("CA_OGDIC false", !declaration.CA_OGDIC);
			Assert("CA_OGDNR false", !declaration.CA_OGDNR);
			Assert("CA_OGDTC false", !declaration.CA_OGDTC);
			AssertEquals("JE_TotalNoOfPacksPackType", IIDUnitOfCountCodeList.Codes.Pack, declaration.JE_TotalNoOfPacksPackType);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			AssertEquals("JE_TotalNoOfPacksPackType", ACROSSPackageTypes.Codes.PACKAGE, declaration.JE_TotalNoOfPacksPackType);
			declaration.CA_AssesmentOption = "2";
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			AssertEquals("Assessment option is 1", AssessmentOptions.Codes.AppraisalQualityData, declaration.CA_AssesmentOption);
		}

		public void TestCA_ServiceOption_NoneIID_ClealAllPGAIndicators()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			foreach (PGARequirement requirement in invoiceLine.PGARequirements)
			{
				foreach (PGAProgramRequirement programRequirement in requirement.ProgramCodeRequirements)
				{
					programRequirement.Indicator = "Y";
				}
			}

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;

			Assert(invoiceLine.CA_CFIAInd.IsEmpty);
			Assert(invoiceLine.CA_CNSCInd.IsEmpty);
			Assert(invoiceLine.CA_DFOInd.IsEmpty);
			Assert(invoiceLine.CA_ECCCInd.IsEmpty);
			Assert(invoiceLine.CA_GACInd.IsEmpty);
			Assert(invoiceLine.CA_HCInd.IsEmpty);
			Assert(invoiceLine.CA_NRCanInd.IsEmpty);
			Assert(invoiceLine.CA_PHACInd.IsEmpty);
			Assert(invoiceLine.CA_TCInd.IsEmpty);
		}

		public void TestCA_AssessmentOptions_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Not read only", !declaration.CA_AssesmentOptionInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("Read only", declaration.CA_AssesmentOptionInfo.ReadOnly);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			Assert("Read only", declaration.CA_AssesmentOptionInfo.ReadOnly);
		}

		public void TestProductAuditType()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("ProductAuditType", JobDeclaration.CAProductAuditType.B3High, declaration.ProductAuditType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("ProductAuditType", JobDeclaration.CAProductAuditType.B3High, declaration.ProductAuditType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDate.Empty;
			AssertEquals("ProductAuditType", JobDeclaration.CAProductAuditType.ACROSSHigh, declaration.ProductAuditType);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Factory.Save();
			AssertEquals("ProductAuditType", JobDeclaration.CAProductAuditType.ACROSSLow, declaration.ProductAuditType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("ProductAuditType", JobDeclaration.CAProductAuditType.B3Low, declaration.ProductAuditType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDate.Today;
			AssertEquals("ProductAuditType", JobDeclaration.CAProductAuditType.B3Low, declaration.ProductAuditType);
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			AssertEquals("ProductAuditType", JobDeclaration.CAProductAuditType.B3High, declaration.ProductAuditType);
		}

		#region Defaults

		public void TestCA_PlaceOfReportDefaultFromJE_RL_NKPortOfLoading()
		{
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Constants.CountryCodes.Canada));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0010", "St. John's Place", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, state.RW_Code);
			Factory.Save();

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = Constants.CountryCodes.Canada;
			unloco.RL_Code = "CAXXX";
			unloco.RL_RW = state.PK;
			unloco.RL_PortName = "St John's Place";

			var declaration = Factory.New<JobDeclaration>();
			declaration.Lookups.CBSAOffices.Load();
			declaration.CA_PlaceOfReport = "0999";
			declaration.JE_RL_NKPortOfLoading = unloco.RL_Code;
			AssertEquals("CA_PlaceOfReport not defaulted", "0999", declaration.CA_PlaceOfReport);

			declaration.CA_PlaceOfReport = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = unloco.RL_Code;
			AssertEquals("CA_PlaceOfReport", ZString.Empty, declaration.CA_PlaceOfReport);

			declaration.JE_RL_NKPortOfLoading = "AUZZZ";
			AssertEquals("CA_PlaceOfReport is defaulted", "", declaration.CA_PlaceOfReport);

			declaration.CA_PlaceOfReport = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = unloco.RL_Code;
			AssertEquals("CA_PlaceOfReport is defaulted", officeCode.ZZD_Code, declaration.CA_PlaceOfReport);
		}

		public void TestCA_PortOfExitDefaultFromJE_RL_NKPortOfLoading()
		{
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Constants.CountryCodes.Canada));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0010", "St. John's Place", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, state.RW_Code);
			Factory.Save();

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = Constants.CountryCodes.Canada;
			unloco.RL_Code = "CAXXX";
			unloco.RL_RW = state.PK;
			unloco.RL_PortName = "St John's Place";

			var declaration = Factory.New<JobDeclaration>();
			declaration.Lookups.CBSAOffices.Load();
			declaration.CA_PortOfExit = "0999";
			declaration.JE_RL_NKPortOfLoading = unloco.RL_Code;
			AssertEquals("CA_PortOfExit not defaulted", "0999", declaration.CA_PortOfExit);

			declaration.CA_PortOfExit = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = unloco.RL_Code;
			AssertEquals("CA_PortOfExit", ZString.Empty, declaration.CA_PortOfExit);

			declaration.JE_RL_NKPortOfLoading = "AUZZZ";
			AssertEquals("CA_PortOfExit is defaulted", "", declaration.CA_PortOfExit);

			declaration.CA_PortOfExit = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = unloco.RL_Code;
			AssertEquals("CA_PortOfExit is defaulted", officeCode.ZZD_Code, declaration.CA_PortOfExit);
		}

		public void TestDefaultCA_PlaceOfReport()
		{
			using (CACustomsDataRegistry.Instance.DefaultPlaceOfReport.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0821"))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("Default CA_PlaceOfReport", "0821", declaration.CA_PlaceOfReport);
			}

			using (CACustomsDataRegistry.Instance.DefaultPlaceOfReport.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "12345"))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("Default CA_PlaceOfReport", ZString.Empty, declaration.CA_PlaceOfReport);
			}
		}

		public void TestDefaultJE_MessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
		}

		public void TestJE_LocationOfGoodsAndNameDefaults()
		{
			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var helper = new DeclarationTestHelper(Factory, true);
				var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
				var depot = helper.CreateOrganisation("DPT", "DEPOT NAME", "CATOR", "DEPOT ADDRESS", "DEPOT CITY", "123 4567");
				var depot1CCP = depot.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3021", canada);
				depot1CCP.OK_OA_PremisesAddress = depot.MainAddress.PK;

				var depotOtherAddress2 = depot.Addresses.AddNew();
				depotOtherAddress2.OA_CompanyNameOverride = "DEPOT NAME2";
				depotOtherAddress2.OA_Address1 = "DEPOT ADDRESS2";
				depotOtherAddress2.OA_City = "DEPOT CITY2";
				var depot2CCP = depot.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3022", canada);
				depot2CCP.OK_OA_PremisesAddress = depotOtherAddress2.PK;

				var depotOtherAddress3 = depot.Addresses.AddNew();
				depotOtherAddress3.OA_CompanyNameOverride = "DEPOT NAME3";
				depotOtherAddress3.OA_Address1 = "DEPOT ADDRESS3";
				depotOtherAddress3.OA_City = "DEPOT CITY3";

				var cto = helper.CreateOrganisation("CTO", "CTO NAME", "CATOR", "CTO ADDRESS", "CTP CITY", "123 4567");
				var ctoCCP = cto.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4554", canada);
				ctoCCP.OK_OA_PremisesAddress = cto.MainAddress.PK;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Consignee.PK;
				declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cto.MainAddress.PK;
				declaration.DepotDocAddress.E2_OA_Address = depot.MainAddress.PK;

				AssertEquals("JE_LocationOfGoods defaulted from CTO CCP", "4554", declaration.JE_LocationOfGoods);
				AssertEquals("CA_SubLocationName defaulted as code description", declaration.Lookups.SubLocationCodes.GetDescriptionFromCode("4554"), declaration.CA_SubLocationName);

				AssertEquals("CA_ExamLocationCode defaulted from Sub-Location", "4554", declaration.CA_ExamLocationCode);
				AssertEquals("CA_ExamLocationName is empty since CA_ExamLocationCode has value", ZString.Empty, declaration.CA_ExamLocationName);
				AssertEquals("ExamLocationDescription defaulted as code description", declaration.Lookups.SubLocationCodes.GetDescriptionFromCode("4554"), declaration.ExamLocationDescription);

				declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
				AssertEquals("JE_LocationOfGoods should now be 2nd sub-location", "3021", declaration.JE_LocationOfGoods);
				AssertEquals("CA_SubLocationName defaulted as code description", declaration.Lookups.SubLocationCodes.GetDescriptionFromCode("3021"), declaration.CA_SubLocationName);

				declaration.DepotDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.JE_LocationOfGoods = ZString.Empty;
				declaration.DepotDocAddress.E2_OA_Address = depotOtherAddress3.PK;

				AssertEquals("JE_LocationOfGoods not found", ZString.Empty, declaration.JE_LocationOfGoods);

				declaration.DepotDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cto.MainAddress.PK;
				AssertEquals("JE_LocationOfGoods should now be picked up from CTO", "4554", declaration.JE_LocationOfGoods);
				AssertEquals("CA_SubLocationName defaulted as code description", declaration.Lookups.SubLocationCodes.GetDescriptionFromCode("4554"), declaration.CA_SubLocationName);

				declaration.JE_LocationOfGoods = ZString.Empty;
				declaration.CA_ExamLocationCode = ZString.Empty;
				declaration.CA_ExamLocationName = ZString.Empty;
				declaration.CA_SubLocationName = "Test Sub-Location Name";
				AssertEquals("CA_ExamLocationName defaulted from sub-location description", "Test Sub-Location Name", declaration.CA_ExamLocationName);

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration2.JE_OH_Importer = helper.Consignee.PK;

				using (declaration2.SetterSuspender.SuspendSetting(JobDeclaration.Schema.JE_LocationOfGoods))
				{
					declaration2.DepotDocAddress.E2_OA_Address = depot.MainAddress.PK;
					declaration2.ContainerTerminalOperatorDocAddress.E2_OA_Address = cto.MainAddress.PK;
					AssertEquals("JE_LocationOfGoods", "", declaration2.JE_LocationOfGoods);
					AssertEquals("CA_ExamLocationCode", "", declaration2.CA_ExamLocationCode);
				}
			}
		}

		public void TestJE_CarrierCode()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "4646";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsAir = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("CA_CarrierName", ZString.Empty, declaration.CA_CarrierName);
			AssertEquals("JE_TransportMode", ZString.Empty, declaration.JE_TransportMode);

			declaration.JE_CarrierCode = carrier.ZZ4_Code;
			AssertEquals("CA_CarrierName", carrier.ZZ4_Description, declaration.CA_CarrierName);
			AssertEquals("JE_TransportMode", Constants.TransportModes.Air, declaration.JE_TransportMode);

			declaration.JE_CarrierCode = ZString.Empty;
			AssertEquals("CA_CarrierName", ZString.Empty, declaration.CA_CarrierName);
			AssertEquals("JE_TransportMode", Constants.TransportModes.Air, declaration.JE_TransportMode);
		}

		public void TestCA_CarrierCode_CarrierAttribute()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "4646";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			carrier.Attributes.AddNew(TransportTypeList.Codes.Air, TransportTypeList.Codes.Air);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("CA_CarrierName", ZString.Empty, declaration.CA_CarrierName);
			AssertEquals("JE_TransportMode", ZString.Empty, declaration.JE_TransportMode);

			declaration.JE_CarrierCode = carrier.ZZ4_Code;
			AssertEquals("CA_CarrierName", carrier.ZZ4_Description, declaration.CA_CarrierName);
			AssertEquals("JE_TransportMode", Constants.TransportModes.Air, declaration.JE_TransportMode);

			declaration.JE_CarrierCode = ZString.Empty;
			AssertEquals("CA_CarrierName", ZString.Empty, declaration.CA_CarrierName);
			AssertEquals("JE_TransportMode", Constants.TransportModes.Air, declaration.JE_TransportMode);
		}

		public void TestDefaultCA_RX_DeclaredCurr()
		{
			var declaration = Factory.New<JobDeclaration>();
			var currency = Factory.Load<RefCurrency>(declaration.CA_RX_DeclaredCurr);
			AssertEquals("CA_RX_DeclaredCurr default", JobDeclaration.LocalCurrencyConstantCode, currency != null ? currency.RX_Code : ZString.Empty);
		}

		[TestDate(2016, 3, 3)]
		public void TestJE_DateOfFirstArrivalDefaults()
		{
			var dateTime1 = new ZDateTime(2010, 10, 1, 13, 30, 0);
			var dateTime2 = new ZDateTime(2010, 10, 2, 14, 25, 0);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = dateTime1;
			AssertEquals("JE_DateOfFirstArrival set", dateTime1, declaration.JE_DateOfFirstArrival);
			declaration.JE_DateOfArrival = dateTime2;
			AssertEquals("JE_DateOfFirstArrival changed", dateTime2, declaration.JE_DateOfFirstArrival);
			declaration.JE_DateOfFirstArrival = ZDateTime.Empty;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PaperRMD;
			AssertEquals("JE_DateOfFirstArrival not set", ZDateTime.Empty, declaration.JE_DateOfFirstArrival);
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			AssertEquals("JE_DateOfFirstArrival set", dateTime2, declaration.JE_DateOfFirstArrival);

			var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;

			var unlocoZ = declaration.Branch.HomePort;
			unlocoZ.RL_R3 = timeZoneSet.PK;
			var effectiveDate = unlocoZ.LocationDateTime;

			declaration.JE_RL_NKPortOfArrival = unlocoZ.RL_Code;
			Factory.Save();

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			declaration.JE_DateOfFirstArrival = effectiveDate;
			AssertEquals("ETA First Port of Arrival", effectiveDate.AddHours(3), declaration.JE_DateOfFirstArrival);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_DateOfFirstArrival = effectiveDate;
			AssertEquals("ETA First Port of Arrival", effectiveDate.AddHours(5), declaration.JE_DateOfFirstArrival);
		}

		#endregion

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			ICusCodeDataTypeSupporter supporter = declaration;
			supporter.AssertType(typeof(DeclarationExportPermit), CusCodeDataTypeList.Codes.Permit);
			supporter.AssertType(null, "ZZ!");
			var cargoControlNumber = declaration.CargoControlNumbers.AddNew();
			cargoControlNumber.CY_CargoControlNumber = "1";
			var permit = declaration.Permits.AddNew();
			permit.CY_Data = "1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(permit.PK);
			AssertEquals(typeof(DeclarationExportPermit), codeData.GetType());
		}

		public void TestCanCancel()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = ZString.Empty;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = MessageStatusList.Codes.NotSent;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			foreach (CodeDescriptionPair pair in new MessageStatusList())
			{
				declaration.JE_MessageStatus = pair.Code;
				if (MessageStatusList.IsMessageStatusAllowCancellation(pair.Code))
				{
					Assert(string.IsNullOrEmpty(declaration.CanCancel()));
				}
				else
				{
					AssertEquals(Customs.Business.BaseJobDeclaration.cancellationMessage, declaration.CanCancel());
				}
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = ZString.Empty;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = MessageStatusList.Codes.NotSent;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			entry.CH_Status = MessageStatusList.Codes.NotSent;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			var b3Entry = declaration.CustomsEntryHeaders.AddNew();
			b3Entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			b3Entry.CH_Status = MessageStatusList.Codes.ErrorOriginal;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			entry.CH_Status = MessageStatusList.Codes.ClearOriginal;
			AssertEquals(Customs.Business.BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			entry.CH_Status = MessageStatusList.Codes.ClearDelete;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			b3Entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			AssertEquals(Customs.Business.BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			b3Entry.CH_Status = MessageStatusList.Codes.ErrorOriginal;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));
		}

		public void TestJE_EntryStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = ZString.Empty;

			var releaseEntry = declaration.CustomsEntryHeaders.AddNew();
			releaseEntry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			releaseEntry.CH_EntryStatus = string.Empty;
			releaseEntry.CH_Status = MessageStatusList.Codes.ClearOriginal;

			AssertEquals("JE_EntryStatusDescription", "Release Requested/In Progress", declaration.JE_EntryStatusDescription);

			releaseEntry.CH_EntryStatus = string.Empty;
			releaseEntry.CH_Status = string.Empty;

			AssertEquals("JE_EntryStatusDescription", string.Empty, declaration.JE_EntryStatusDescription);
		}

		public void TestHasUSPlaceOfExportInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			ValidationTestHelper.SetTotalValueForDuty(JobComInvoiceHeaderTest.VFDOverLimit, declaration);
			var invoice = declaration.Invoices.AddNew();
			invoice.CA_TradeZone = "101";
			declaration.Invoices.AddNew();
			declaration.Invoices.AddNew();

			Assert(declaration.HasUSPlaceOfExportInvoice);
			invoice.CA_TradeZone = ZString.Empty;
			Assert(!declaration.HasUSPlaceOfExportInvoice);
		}

		public override void TestTotalCustomsValueInLocalCurreny()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(0m, declaration.TotalCustomsValueInLocalCurrency);

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 100m;

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var line2 = entryHeader2.MergedLines.AddNew();
			line2.CL_CustomsValue = 200m;

			AssertEquals(100m, declaration.TotalCustomsValueInLocalCurrency);
		}

		public override void TestDescription()
		{
			var descriptionImportCustomizationCollection = CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			var descriptionExportCustomizationCollection = CustomsDataRegistry.Instance.DeclarationExportDescriptionCustomization.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("JNO")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("MBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("HBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("ENT")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("IMP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("EXP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("ORF")).Bool = ZBool.True;

			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("JNO")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("MBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("HBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("ENT")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("IMP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("EXP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("ORF")).Bool = ZBool.True;

			CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionImportCustomizationCollection);
			var importdeclaration = Factory.New<JobDeclaration>();
			importdeclaration.JE_DeclarationReference = "ABC";
			importdeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Default Description of import job", "ABC", importdeclaration.Description);
			importdeclaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN001";
			AssertEquals("CCN of import job", "CCN: CCN001", importdeclaration.Description);

			CustomsDataRegistry.Instance.DeclarationExportDescriptionCustomization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionImportCustomizationCollection);
			var exporteclaration = Factory.New<JobDeclaration>();
			exporteclaration.JE_DeclarationReference = "DEF";
			exporteclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Default Description of export job", "DEF", exporteclaration.Description);
			exporteclaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN002";
			AssertEquals("CCN of export job", "CCN: CCN002", exporteclaration.Description);
		}

		public void TestTotalDutyTaxEntryFeeItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.SuspendMarkApportionmentDirtyForDutiesAndTaxes();
			var duty = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Amount = 100m;
			duty.C1_Override = true;
			var tax = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			tax.C1_Amount = 200m;
			tax.C1_Override = true;
			var sima = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_Override = true;
			sima.C1_ExemptCode = "31";
			sima.C1_Amount = 300m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.SuspendMarkApportionmentDirtyForDutiesAndTaxes();
			duty = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Amount = 1000m;
			duty.C1_Override = true;
			tax = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			tax.C1_Amount = 2000m;
			tax.C1_Override = true;
			sima = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_Override = true;
			sima.C1_ExemptCode = "31";
			sima.C1_Amount = 3000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			ILandedCostHeader header = declaration;
			var fee = header.TotalDutyTaxEntryFeeItems;
			AssertEquals("Duty", 1100m, fee["TDT"]);
			AssertEquals("Excise", 2200m, fee["EXC"]);
			AssertEquals("Other duty", 3300m, fee["OTH"]);
		}

		public void TestExportingCarrier()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var helper = new DeclarationTestHelper(Factory, true);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_OH_Forwarder = helper.ExportForwarder.PK;
			AssertEquals("If not G7 then null", null, dec.ExportingCarrier);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("If G7 and no Shipping Line then Forwarder", helper.ExportForwarder.PK, dec.ExportingCarrier.PK);
			dec.JE_OH_ShippingLine = helper.ShippingLine.PK;
			AssertEquals("If G7 and Shipping Line then Shipping Line", helper.ShippingLine.PK, dec.ExportingCarrier.PK);
		}

		public void TestFormattAirwayBillInCA_TransportDocumentNumber()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_MasterBill = "08176289172";
			AssertEquals("Airway bill is formatted", "081-76289172", dec.CA_TransportDocumentNumber);
		}

		public void TestMessageTypeIsLVSForLVSJobs()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "BUYER";
			buyer.OH_RL_NKClosestPort = "CATOR";
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = buyer.PK;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_OH_Importer = buyer.PK;

			AssertEquals("JE_MessageType should always be LVS for LVS jobs", JobMessageTypeList.Codes.LowValueShipments, declaration.JE_MessageType);
		}

		public void TestGetCusAddInfoType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var iCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)declaration;
			iCusAddInfoTypeSupporter.AssertType(typeof(CargoControlNumber), CusAddInfoTypeAttribute.Codes.CACCN);
		}

		public void TestCERSProofOfReportNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNotNull("CERSProofOfReportNumber created when touched", declaration.CERSProofOfReportNumber);
			AssertEquals("CERSProofOfReportNumber", "", declaration.JE_CERSProofOfReportNumber);
			declaration.JE_CERSProofOfReportNumber = "123456";
			AssertEquals("CERSProofOfReportNumber", "123456", declaration.JE_CERSProofOfReportNumber);
		}

		public void TestReleaseOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "0495";
			AssertEquals("Release Office defaults to Port of Clearance", "0495", declaration.ReleaseOffice);
			declaration.CA_ReleaseOffice = "0497";
			AssertEquals("Release Office", "0497", declaration.ReleaseOffice);
		}

		public void TestReleaseStatuses()
		{
			var testHelper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCN1";
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN2";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(testHelper.GetEDIReleaseResponseMessage("CCN1", "1"));
			entry.Messages.Add(testHelper.GetEDIReleaseResponseMessage("CCN2", "2"));

			AssertEquals("ReleaseStatuses.Count", 2, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN1", declaration.ReleaseStatuses[0].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 2", "CCN2", declaration.ReleaseStatuses[1].RL_CargoControlNumber);

			AssertEquals("ReleaseStatusesToPrint.Count", 2, declaration.ReleaseStatusesToPrint.Count);
			AssertEquals("ReleaseStatusesToPrint 1", "CCN1", declaration.ReleaseStatusesToPrint[0].RL_CargoControlNumber);
			AssertEquals("ReleaseStatusesToPrint 2", "CCN2", declaration.ReleaseStatusesToPrint[1].RL_CargoControlNumber);
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN3";
			AssertEquals("CargoControlNumbers.Count", 3, declaration.CargoControlNumbers.Count);
			AssertEquals("ReleaseStatuses.Count", 2, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatusesToPrint.Count", 2, declaration.ReleaseStatusesToPrint.Count);

			entry.Messages.Add(testHelper.GetEDIReleaseResponseMessage("CCN3", "3"));
			declaration.ReleaseStatuses.Load();
			AssertEquals("CargoControlNumbers.Count", 3, declaration.CargoControlNumbers.Count);
			AssertEquals("ReleaseStatuses.Count", 3, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatusesToPrint.Count", 3, declaration.ReleaseStatusesToPrint.Count);

			declaration = Factory.New<JobDeclaration>();
			AssertEquals("ReleaseStatuses.Count", 0, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatusesToPrint.Count", 0, declaration.ReleaseStatusesToPrint.Count);
		}

		public void TestInvoicesToPrint()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("InvoicesToPrint.Count", 1, declaration.LinesToPrint.Count);
			AssertEquals("InvoicesToPrint 1", invoice, ((JobComInvoiceHeaderToPrint)declaration.LinesToPrint[0]).InvoiceHeader);

			invoice = declaration.Invoices.AddNew();
			AssertEquals("InvoicesToPrint.Count", 2, declaration.LinesToPrint.Count);
			AssertEquals("InvoicesToPrint 2", invoice, ((JobComInvoiceHeaderToPrint)declaration.LinesToPrint[1]).InvoiceHeader);

			invoice.Delete();
			AssertEquals("InvoicesToPrint.Count", 1, declaration.LinesToPrint.Count);
			AssertEquals("InvoicesToPrint 3", declaration.Invoices[0], ((JobComInvoiceHeaderToPrint)declaration.LinesToPrint[0]).InvoiceHeader);
		}

		public override void TestMessageTypeForDocumentFilter()
		{
			base.TestMessageTypeForDocumentFilter();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("Export type", JobMessageTypeList.Codes.LowValueShipments, declaration.MessageTypeForDocumentFilter);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("Export type", JobMessageTypeList.Codes.LVSForConsolidation, declaration.MessageTypeForDocumentFilter);
		}

		public override void TestMergeByDefaultsFromClientWhenClientChanges()
		{
			Env.Registry.SetCommercialInvoiceLineMergeMethod(GlbCompany.CurrentCompany.PK.ToGuid(), MergeType2);

			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableMessageTypeChangeOnSupplierChangeForTesting = true;
			declaration.JE_MessageType = DefaultExportMessageType;
			var importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "XXX";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = MergeType1;

			var supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "XXX";
			supplier.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "DEF";

			AssertEquals("Declaration.JE_MergeBy", DefaultMergeType, declaration.JE_MergeBy);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			//export so from supplier
			AssertEquals("Declaration.JE_MergeBy", MergeType2, declaration.JE_MergeBy);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;

			declaration.JE_MessageType = DefaultImportMessageType;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Declaration.JE_MergeBy not changed", "NON", declaration.JE_MergeBy);
			AssertEquals("Declaration.CA_MergeBy set", MergeType1, declaration.CA_MergeBy);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.CA_MergeBy = ZString.Empty;

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Declaration.CA_MergeBy not changed", ZString.Empty, declaration.CA_MergeBy);

			declaration.JE_OH_Importer = ZGuid.Empty;

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.CA_MergeBy = ZString.Empty;

			declaration.LVXInvoiceHeader.JZ_OH_Buyer = importer.PK;
			AssertEquals("Declaration.CA_MergeBy set", MergeType1, declaration.CA_MergeBy);
		}

		public void TestDeclarationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "1";
			AssertEquals("12345000000012", declaration.DeclarationNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ActiveEntryHeaders.AddNew().EntryNumber = "54321X8000002";
			AssertEquals("54321X8000002", declaration.DeclarationNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("12345000000012", declaration.DeclarationNumber);
		}

		public void TestChangingImportToExportTotalyLoosesTransactionNumber()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "1";
			AssertEquals("12345000000012", declaration.DeclarationNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ZString.Empty, declaration.DeclarationNumber);
		}

		public void TestUseImporterSecurityNumberWhenChangingToImport()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "43210");
			CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var importer2 = Factory.New<OrgHeader>();
			importer2.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			declaration.CA_UseImporterAccountSecurityNumber = true;
			declaration.ImporterAddInfo.ZO_AccountSecurityNumber = "1234";
			declaration.ImporterAddInfo.ZO_AccountSecirityPassword = "1234";
			declaration.JE_OH_Supplier = importer2.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			AssertEquals("1234000000000", declaration.FormattedTransactionNumber);
		}

		public void TestInvoicesMentionNewOrInactiveProducts()
		{
			var org = Factory.New<OrgHeader>();
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypes.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = org.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART121";
			invoiceLine.JI_Description = "GOODS AND MORE GOODS";
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CC = classification.PK;
			AssertEquals(true, declaration.InvoicesMentionNewOrInactiveProducts);
			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(false, declaration.InvoicesMentionNewOrInactiveProducts);
			invoiceLine.JI_Tariff = "2030124020";
			AssertEquals(true, declaration.InvoicesMentionNewOrInactiveProducts);
			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals(false, declaration.InvoicesMentionNewOrInactiveProducts);
			invoiceLine.JI_PartNo = "PART121";
			invoiceLine.JI_Description = ZString.Empty;
			AssertEquals(false, declaration.InvoicesMentionNewOrInactiveProducts);
			invoiceLine.JI_Description = "GOODS AND MORE GOODS";
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			AssertEquals(false, declaration.InvoicesMentionNewOrInactiveProducts);
			invoiceLine.JI_InvoiceUQ = "NO";
			AssertEquals(true, declaration.InvoicesMentionNewOrInactiveProducts);
			invoiceLine.JI_PartNo = "PART321";
			AssertEquals(false, declaration.InvoicesMentionNewOrInactiveProducts);
		}

		public void TestSetterOfTransportModeClearsContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ContainerMode = Constants.ContainerModes.Containerised;
			declaration.JE_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Setting JE_TransportMode should clear JE_ContainerMode", ZString.Empty, declaration.JE_ContainerMode);
		}

		public void TestEffectiveDutyDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("EffectiveDutyDate", ZDateTime.Today, declaration.EffectiveDutyDate);
			declaration.CA_EstReleaseDate = new ZDateTime(2010, 1, 1);
			AssertEquals("EffectiveDutyDate", declaration.CA_EstReleaseDate, declaration.EffectiveDutyDate);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 1, 2);
			AssertEquals("EffectiveDutyDate", declaration.JE_EntryAuthorisationDate, declaration.EffectiveDutyDate);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Invalid;
			AssertEquals("EffectiveDutyDate", declaration.CA_EstReleaseDate, declaration.EffectiveDutyDate);
			declaration.CA_EstReleaseDate = ZDateTime.Invalid;
			AssertEquals("EffectiveDutyDate", ZDateTime.Today, declaration.EffectiveDutyDate);
		}

		[TestDate(2014, 9, 1)]
		public void TestEstimatedPaymentDueDate_NormalShipment()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				Assert("Test 0 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(ZDateTime.Empty));
				declaration.JE_DateOfArrival = new ZDateTime(2014, 8, 28);
				AssertEquals("Test 0 CA_AccountingAge", 0, declaration.CA_AccountingAge);
				Assert("Test 1 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 09, 05)));
				AssertEquals("Test 1 CA_AccountingAge", 0, declaration.CA_AccountingAge);
				declaration.JE_DateAtFinalDestination = new ZDateTime(2014, 8, 27);
				Assert("Test 2 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 09, 04)));
				AssertEquals("Test 2 CA_AccountingAge", 0, declaration.CA_AccountingAge);
				declaration.JE_DateOfFirstArrival = new ZDateTime(2014, 8, 26);
				Assert("Test 3 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 09, 03)));
				AssertEquals("Test 3 CA_AccountingAge", 0, declaration.CA_AccountingAge);
				declaration.CA_EstReleaseDate = new ZDateTime(2014, 8, 25);
				Assert("Test 4 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 09, 02)));
				AssertEquals("Test 4 CA_AccountingAge", 0, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 8, 24);
				Assert("Test 5 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 09, 02)));
				AssertEquals("Test 5 CA_AccountingAge", 4, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 8, 23);
				Assert("Test 6 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 09, 02)));
				AssertEquals("Test 6 CA_AccountingAge", 4, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 8, 22);
				Assert("Test 7 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 08, 29)));
				AssertEquals("Test 7 CA_AccountingAge", 5, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 8, 21);
				Assert("Test 8 EstimatedPaymentDueDate", declaration.EstimatedPaymentDueDate.Equals(new DateTime(2014, 08, 28)));
				AssertEquals("Test 8 CA_AccountingAge", 6, declaration.CA_AccountingAge);
			});
		}

		public void TestEstimatedPaymentDueDate_Normal_LowValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0301", "Chicoutimi", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "QC");
			Factory.Save();

			CombineAssertions(() =>
			{
				var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
				Factory.Save();
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 2499m;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 2499m;
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 4, 10);
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var line = entryHeader.MergedLines.AddNew();
				Factory.Save();

				declaration.JE_EntryAuthorisationDate = ZDateTime.Invalid;
				AssertEquals("Test 0 _ Release Invalid_ CutOff Empty", ZDateTime.Empty, declaration.EstimatedPaymentDueDate);
				declaration.JE_DateOfArrival = new ZDateTime(2014, 07, 24);
				AssertEquals("Test 1 _ Release 20140724_ CutOff 20140822(Sun0824)", new ZDateTime(2014, 07, 31), declaration.EstimatedPaymentDueDate);
				declaration.JE_DateAtFinalDestination = new ZDateTime(2014, 07, 25);
				AssertEquals("Test 2 _ Release 20140725_ CutOff 20140822(Sun0824)", new ZDateTime(2014, 08, 01), declaration.EstimatedPaymentDueDate);
				declaration.JE_DateOfFirstArrival = new ZDateTime(2014, 07, 26);
				AssertEquals("Test 3 _ Release 20140726_ CutOff 20140822(Sun0824)", new ZDateTime(2014, 08, 04), declaration.EstimatedPaymentDueDate);
				declaration.CA_EstReleaseDate = new ZDateTime(2014, 08, 01);
				AssertEquals("Test 4 _ Release 20140801_ CutOff 20140924", new ZDateTime(2014, 08, 08), declaration.EstimatedPaymentDueDate);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 07, 25);
				AssertEquals("Test 5 _ Release 20130725_ CutOff 20130823(Sat0824)", new ZDateTime(2013, 08, 01), declaration.EstimatedPaymentDueDate);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 08, 25);
				AssertEquals("Test 6 _ Release 20130825_ CutOff 20130925", new ZDateTime(2013, 09, 02), declaration.EstimatedPaymentDueDate);

				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 05, 01);
				AssertEquals("Test 7 _ Release 20140501_ CutOff 20140624", new ZDateTime(2014, 05, 08), declaration.EstimatedPaymentDueDate);

				declaration.JE_CustomsOffice = "0301";
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 05, 01);
				AssertEquals("Test 8 _ Release 20140501_ CutOff 20140623(Holiday0624InQC)", new ZDateTime(2014, 05, 08), declaration.EstimatedPaymentDueDate);
			});
		}

		public void TestEstimatedPaymentDueDate_ConsolidatedLVS()
		{
			CombineAssertions(() =>
			{
				var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
				Factory.Save();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 4, 10);
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var line = entryHeader.MergedLines.AddNew();
				line.CL_CustomsValue = 2499m;

				declaration.JE_EntryAuthorisationDate = ZDateTime.Invalid;
				AssertEquals("Test 0 _ Release Invalid_ CutOff Empty", ZDateTime.Empty, declaration.EstimatedPaymentDueDate);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 07, 1);
				AssertEquals("Test 1 _ Release 20140724_ CutOff 20140822(Sun0824)", new ZDateTime(2014, 08, 22), declaration.EstimatedPaymentDueDate);

				line.CL_CustomsValue = 2501m;
				entryHeader.ResetTotalsAndCachedValues();
				declaration.JE_EntryAuthorisationDate = ZDateTime.Invalid;
				AssertEquals("Test 0 _ Release Invalid_ CutOff Empty", ZDateTime.Empty, declaration.EstimatedPaymentDueDate);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 07, 1);
				AssertEquals("Test 1 _ Release 20140724_ CutOff 20140822(Sun0824)", new ZDateTime(2014, 08, 22), declaration.EstimatedPaymentDueDate);
			});
		}

		[TestDate(2013, 5, 1)]
		public void TestK84CutOffDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			TestDateAttribute.Date = new DateTime(2013, 5, 10, 12, 0, 0);
			AssertEquals(new ZDate(2013, 5, 24), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 5, 23, 12, 0, 0);
			AssertEquals(new ZDate(2013, 5, 24), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 5, 24, 20, 0, 0);
			AssertEquals(new ZDate(2013, 5, 24), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 5, 24, 20, 0, 1);
			AssertEquals(new ZDate(2013, 6, 24), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 5, 25, 12, 0, 0);
			AssertEquals(new ZDate(2013, 6, 24), declaration.K84CutOffDate);

			TestDateAttribute.Date = new DateTime(2013, 3, 21, 12, 0, 0);
			AssertEquals(new ZDate(2013, 3, 22), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 3, 22, 12, 0, 0);
			AssertEquals(new ZDate(2013, 3, 22), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 3, 23, 12, 0, 0);
			AssertEquals(new ZDate(2013, 4, 24), declaration.K84CutOffDate);

			TestDateAttribute.Date = new DateTime(2013, 1, 23, 12, 0, 0);
			AssertEquals(new ZDate(2013, 1, 24), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 1, 24, 12, 0, 0);
			AssertEquals(new ZDate(2013, 1, 24), declaration.K84CutOffDate);
			TestDateAttribute.Date = new DateTime(2013, 1, 25, 12, 0, 0);
			AssertEquals(new ZDate(2013, 2, 22), declaration.K84CutOffDate);
		}

		[TestDate(2013, 4, 22)]
		public void TestIsPaymentDueDateAfterCutOffDate_RegistryFallBackValue()
		{
			CombineAssertions(() =>
			{
				CACustomsDataRegistry.Instance.EntryStatementDateThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);

				var declaration = Factory.New<JobDeclaration>();
				ZDate k84CutOff;
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 16);
				Assert("6 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 17);
				Assert("5 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 18);
				Assert("4 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 19);
				Assert("3 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 20);
				Assert("3 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 21);
				Assert("3 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 22);
				Assert("2 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 23);
				Assert("1 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 24);
				Assert("release on cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 25);
				Assert("release after cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		[TestDate(2013, 4, 22)]
		public void TestIsPaymentDueDateAfterCutOffDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			ZDate k84CutOff;
			Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 16);
			Assert("6 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 17);
			Assert("5 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 18);
			Assert("4 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 19);
			Assert("3 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 20);
			Assert("3 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 21);
			Assert("3 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 22);
			Assert("2 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 23);
			Assert("1 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 24);
			Assert("release on cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 25);
			Assert("release after cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 16);
			CACustomsDataRegistry.Instance.EntryStatementDateThreshold.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 0);
			Assert("no threshold in registry", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
		}

		[TestDate(2014, 4, 22)]
		public void TestIsPaymentDueDateAfterCutOffDate_WithHolidayConsideration()
		{
			var declaration = Factory.New<JobDeclaration>();
			ZDate k84CutOff;
			CombineAssertions(() =>
			{
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 4, 14);
				Assert("8 non-weekend days(6 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 4, 15);
				Assert("7 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 4, 16);
				Assert("6 non-weekend days(4 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 4, 17);
				Assert("5 non-weekend days(3 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		[TestDate(2017, 2, 21)]
		public void TestIsPaymentDueDateAfterCutOffDate_WithProvincialHolidayConsideration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "T0QC", "Chicoutimi", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "QC");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "T0PE", "Chicoutimi", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PE");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			ZDate k84CutOff;

			CombineAssertions("No Province of Clearance", () =>
			{
				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 16);
				Assert("8 non-weekend days(6 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 17);
				Assert("7 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 18);
				Assert("6 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 19);
				Assert("5 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 20);
				Assert("4 non-weekend days(4 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 21);
				Assert("3 non-weekend days(3 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 22);
				Assert("2 non-weekend days(2 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 23);
				Assert("1 non-weekend days(1 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});

			CombineAssertions("QC as Province of Clearance", () =>
			{
				declaration.JE_CustomsOffice = "T0QC";

				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 16);
				Assert("8 non-weekend days(6 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 17);
				Assert("7 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 18);
				Assert("6 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 19);
				Assert("5 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 20);
				Assert("4 non-weekend days(4 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 21);
				Assert("3 non-weekend days(3 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 22);
				Assert("2 non-weekend days(2 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 23);
				Assert("1 non-weekend days(1 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});

			CombineAssertions("PE as Province of Clearance", () =>
			{
				declaration.JE_CustomsOffice = "T0PE";

				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 15);
				Assert("9 non-weekend days(6 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 16);
				Assert("8 non-weekend days(5 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 17);
				Assert("7 non-weekend days(4 working days) from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 18);
				Assert("6 non-weekend days(3 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 19);
				Assert("5 non-weekend days(3 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 20);
				Assert("4 non-weekend days(3 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 21);
				Assert("3 non-weekend days(3 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 22);
				Assert("2 non-weekend days(2 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 2, 23);
				Assert("1 non-weekend days(1 working days) from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		[TestDate(2013, 4, 24)]
		public void TestIsPaymentDueDateAfterCutOffDateOnCutOffDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			ZDate k84CutOff;
			Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 16);
			Assert("6 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 17);
			Assert("5 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 18);
			Assert("4 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 19);
			Assert("3 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 20);
			Assert("3 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 21);
			Assert("3 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 22);
			Assert("2 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 23);
			Assert("1 working days from release to cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 24);
			Assert("release on cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 25);
			Assert("release after cut off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
		}

		[TestDate(2013, 4, 25)]
		public void TestIsPaymentDueDateAfterCutOffDateAfterCutOffDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			ZDate k84CutOff;
			Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 16);
			Assert("6 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 17);
			Assert("5 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 18);
			Assert("4 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 19);
			Assert("3 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 20);
			Assert("3 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 21);
			Assert("3 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 22);
			Assert("2 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 23);
			Assert("1 working days from release to cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 24);
			Assert("release on cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 25);
			Assert("release after cut off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
		}

		[TestDate(2013, 4, 20)]
		public void TestIsPaymentDueDateAfterCutOffDateForLowValueShipment()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			ZDate k84CutOff;
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2499m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2499m;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 4, 10);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			Factory.Save();
			CombineAssertions(() =>
			{
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 1);
				Assert("early in previous month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 22);
				Assert("Just Before cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 24);
				Assert("on cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 25);
				Assert("after cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));

				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 1);
				Assert("early in month", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 24);
				Assert("on cut-off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 25);
				Assert("after cut-off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		[TestDate(2013, 4, 24)]
		public void TestIsPaymentDueDateAfterCutOffDateForLowValueShipmentOnCutOffDate()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			ZDate k84CutOff;
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2499m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2499m;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 4, 10);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			Factory.Save();
			CombineAssertions(() =>
			{
				Assert("Test 1: No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 1);
				Assert("Test 2: Early in previous month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 22);
				Assert("Test 3: On cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 24);
				Assert("Test 4: On cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 25);
				Assert("Test 5: After cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));

				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 1);
				Assert("Test 6: Early in month", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 24);
				Assert("Test 7: On cut-off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 25);
				Assert("Test 8: After cut-off date", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		[TestDate(2013, 4, 25)]
		public void TestIsPaymentDueDateAfterCutOffDateForLowValueShipmentAfterCutOffDate()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			ZDate k84CutOff;
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 4, 10);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;
			CombineAssertions(() =>
			{
				Assert("Test 1: No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 1);
				Assert("Test 2: Early in previous month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 22);
				Assert("Test 3: On cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 24);
				Assert("Test 4: On cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 25);
				Assert("Test 5: After cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 1);
				Assert("Test 6: Early in month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 24);
				Assert("Test 7: On cut-off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 25);
				Assert("Test 8: After cut-off date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		[TestDate(2013, 8, 19)]
		public void TestIsPaymentDueDateAfterCutOffDateForLowValueShipment_CutOffDateOnWeekEnd()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			ZDate k84CutOff;
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 8, 10);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;
			CombineAssertions(() =>
			{
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 7, 1);
				Assert("early in previous month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 7, 22);
				Assert("on cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 7, 24);
				Assert("on cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 7, 25);
				Assert("after cut-off date Previous Month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		[TestDate(2013, 4, 20)]
		public void TestIsPaymentDueDateAfterCutOffDateForConsolidatedLVS()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			ZDate k84CutOff;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 4, 10);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;
			CombineAssertions(() =>
			{
				declaration.JE_EntryAuthorisationDate = ZDateTime.Invalid;
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 1);
				Assert("last month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));

				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 1);
				Assert("this month", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));

				line.CL_CustomsValue = 2501m;
				entryHeader.ResetTotalsAndCachedValues();

				declaration.JE_EntryAuthorisationDate = ZDateTime.Invalid;
				Assert("No release date", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 3, 1);
				Assert("last month", !declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));

				declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 4, 1);
				Assert("this month", declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOff));
			});
		}

		public void TestPeriodMonthAndYear()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PeriodMonth = 4;
			declaration.JE_PeriodYear = 2011;
			AssertEquals("Period", new ZDateTime(2011, 4, 1), declaration.JE_EntryAuthorisationDate);

			declaration.JE_PeriodMonth = -1;
			declaration.JE_PeriodYear = 100;
			AssertEquals("Period", new ZDateTime(1900, 1, 1), declaration.JE_EntryAuthorisationDate);

			declaration.JE_PeriodMonth = 20;
			declaration.JE_PeriodYear = 3000;
			AssertEquals("Period", new ZDateTime(2078, 12, 1), declaration.JE_EntryAuthorisationDate);
		}

		[TestDate(2012, 08, 17)]
		public void TestCA_AccountingAge()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("AccountingAge_1", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
				AssertEquals("AccountingAge_2", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-1);
				AssertEquals("AccountingAge_3", 1, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-3);
				AssertEquals("AccountingAge_4", 3, declaration.CA_AccountingAge);

				declaration.CA_K84AccountingDate = ZDateTime.Now;
				AssertEquals("AccountingAge_4_2", 0, declaration.CA_AccountingAge);
				declaration.CA_K84AccountingDate = ZDateTime.Empty;

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = ZString.Empty;
				AssertEquals("AccountingAge_4_2", 3, declaration.CA_AccountingAge);

				entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				AssertEquals("AccountingAge_4_3", 3, declaration.CA_AccountingAge);

				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				AssertEquals("AccountingAge_5", 3, declaration.CA_AccountingAge);

				entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				AssertEquals("AccountingAge_5_2", ZInt.Zero, declaration.CA_AccountingAge);

				declaration.CA_K84AccountingDate = ZDateTime.Empty;
				AssertEquals("AccountingAge_5_3", ZInt.Zero, declaration.CA_AccountingAge);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.CA_K84AccountingDate = ZDateTime.Empty;
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-3);
				AssertEquals("AccountingAge_6", ZInt.Zero, declaration.CA_AccountingAge);

				declaration.CA_K84AccountingDate = ZDateTime.Now;
				AssertEquals("AccountingAge_6_2", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.CA_K84AccountingDate = ZDateTime.Empty;

				entryHeader.Delete();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-1);
				AssertEquals("AccountingAge_7", 1, declaration.CA_AccountingAge);
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
				AssertEquals("AccountingAge_8", ZInt.Zero, declaration.CA_AccountingAge);
			});
		}

		[TestDate(2014, 10, 02)]
		public void TestCA_AccountingAge_WithHolidayConsideration()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 10, 1);
				AssertEquals("AccountingAge", 1, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 10, 3);
				AssertEquals("AccountingAge", 0, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 10, 7);
				AssertEquals("AccountingAge", 0, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 09, 01);
				AssertEquals("AccountingAge", 22, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 08, 29);
				AssertEquals("AccountingAge", 23, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 08, 28);
				AssertEquals("AccountingAge", 24, declaration.CA_AccountingAge);

				declaration.CA_CSAEntry = true;
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 10, 1);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 10, 3);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 10, 7);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 09, 01);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 08, 29);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 08, 28);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.CA_CSAEntry = false;

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.CA_K84AccountingDate = ZDateTime.Empty;
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-3);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(3);
				AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
			});
		}

		[TestDate(2015, 08, 18)]
		public void TestCA_AccountingAge_WithProvincialHolidayConsideration()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0701", "0701", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "AB");
			var officeCode2 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0801", "0801", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "BC");
			var officeCode3 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0502", "0502", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "MB");
			var officeCode4 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0201", "0201", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode4.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "NB");
			var officeCode5 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0900", "0900", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode5.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "NL");
			var officeCode6 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0009", "0009", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode6.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "NS");
			var officeCode7 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0512", "0512", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode7.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "NT");
			var officeCode8 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0400", "0400", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ON");
			var officeCode9 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0101", "0101", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PE");
			var officeCode10 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0301", "0301", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode10.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "QC");
			var officeCode11 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0601", "0601", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode11.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "SK");
			var officeCode12 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "T0NU", "T0NU", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode12.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "NU");
			var officeCode13 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "T0YT", "T0YT", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode13.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "YT");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			AssertEquals("AccountingAge", ZInt.Zero, declaration.CA_AccountingAge);

			CombineAssertions("Test of YT Discovery Day", () =>
			{
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 08, 14);
				declaration.JE_CustomsOffice = string.Empty;
				AssertEquals("AccountingAge_emptyPortOfClearance", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0701";
				AssertEquals("AccountingAge_testOfficeAB", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0801";
				AssertEquals("AccountingAge_testOfficeBC", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0502";
				AssertEquals("AccountingAge_testOfficeMB", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0201";
				AssertEquals("AccountingAge_testOfficeNB", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0900";
				AssertEquals("AccountingAge_testOfficeNL", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0009";
				AssertEquals("AccountingAge_testOfficeNS", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0512";
				AssertEquals("AccountingAge_testOfficeNT", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0400";
				AssertEquals("AccountingAge_testOfficeON", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0101";
				AssertEquals("AccountingAge_testOfficePE", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0301";
				AssertEquals("AccountingAge_testOfficeQC", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0601";
				AssertEquals("AccountingAge_testOfficeSK", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "T0NU";
				AssertEquals("AccountingAge_testOfficeNU", 2, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "T0YT";
				AssertEquals("AccountingAge_testOfficeYT", 1, declaration.CA_AccountingAge);
			});

			CombineAssertions("Test of AB,BC,MB,NB,NL,NT,NS,ON,PE,SK civic/provincial Day", () =>
			{
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 07, 31);
				declaration.JE_CustomsOffice = string.Empty;
				AssertEquals("AccountingAge_emptyPortOfClearance", 12, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0701";
				AssertEquals("AccountingAge_testOfficeAB", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0801";
				AssertEquals("AccountingAge_testOfficeBC", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0502";
				AssertEquals("AccountingAge_testOfficeMB", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0201";
				AssertEquals("AccountingAge_testOfficeNB", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0900";
				AssertEquals("AccountingAge_testOfficeNL", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0009";
				AssertEquals("AccountingAge_testOfficeNS", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0512";
				AssertEquals("AccountingAge_testOfficeNT", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0400";
				AssertEquals("AccountingAge_testOfficeON", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0101";
				AssertEquals("AccountingAge_testOfficePE", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0301";
				AssertEquals("AccountingAge_testOfficeQC", 12, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "0601";
				AssertEquals("AccountingAge_testOfficeSK", 11, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "T0NU";
				AssertEquals("AccountingAge_testOfficeNU", 12, declaration.CA_AccountingAge);
				declaration.JE_CustomsOffice = "T0YT";
				AssertEquals("AccountingAge_testOfficeYT", 11, declaration.CA_AccountingAge);
			});
		}

		public void TestIsB3Lodged()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("TestIsB3Lodged_1", false, declaration.IsB3Lodged);
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = ZString.Empty;
				AssertEquals("TestIsB3Lodged_2", false, declaration.IsB3Lodged);

				entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				AssertEquals("TestIsB3Lodged_3", false, declaration.IsB3Lodged);

				entryHeader.CH_MessageType = MessageTypeList.Codes.ACIForwarderClose;
				AssertEquals("TestIsB3Lodged_4", false, declaration.IsB3Lodged);

				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				AssertEquals("TestIsB3Lodged_5", true, declaration.IsB3Lodged);

				entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Error;
				AssertEquals("TestIsB3Lodged_6", false, declaration.IsB3Lodged);

				entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
				AssertEquals("TestIsB3Lodged_7", true, declaration.IsB3Lodged);
			});
		}

		public void TestHasAB3AcceptedOrWaiting()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			AssertEquals("HasAB3AcceptedOrWaiting", false, declaration.HasAB3AcceptedOrWaiting);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals("HasAB3AcceptedOrWaiting", false, declaration.HasAB3AcceptedOrWaiting);
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			AssertEquals("HasAB3AcceptedOrWaiting", true, declaration.HasAB3AcceptedOrWaiting);
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
			AssertEquals("HasAB3AcceptedOrWaiting", true, declaration.HasAB3AcceptedOrWaiting);
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("HasAB3AcceptedOrWaiting", false, declaration.HasAB3AcceptedOrWaiting);
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			AssertEquals("HasAB3AcceptedOrWaiting", true, declaration.HasAB3AcceptedOrWaiting);
		}

		public void TestCANoteTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("B3 comments", declaration.NoteTypes.Cast<PredefinedNoteType>().Any(x => x.Description == PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description));
			Assert("CAD comments", declaration.NoteTypes.Cast<PredefinedNoteType>().Any(x => x.Description == PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD.Description));
			Assert("Lead Sheet comments", declaration.NoteTypes.Cast<PredefinedNoteType>().Any(x => x.Description == PredefinedNoteTypes.Instance.LeadSheetComments.Description));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("B3 comments", !declaration.NoteTypes.Cast<PredefinedNoteType>().Any(x => x.Description == PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description));
			Assert("CAD comments", !declaration.NoteTypes.Cast<PredefinedNoteType>().Any(x => x.Description == PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD.Description));
			Assert("Lead Sheet comments", !declaration.NoteTypes.Cast<PredefinedNoteType>().Any(x => x.Description == PredefinedNoteTypes.Instance.LeadSheetComments.Description));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description, "Note to be printed on B3");
			AssertEquals("B3Comments", "Note to be printed on B3", declaration.B3Comments);
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD.Description, "Note to be printed on CAD");
			AssertEquals("CADComments", "Note to be printed on CAD", declaration.CADComments);
		}

		public void TestJE_CCNsAsAString()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ReleaseStatuses.Load();
			AssertEquals("CCNsAsAString", ZString.Empty, declaration.JE_CCNsAsAString);
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN1";
			AssertEquals("CCNsAsAString", "CCN1", declaration.JE_CCNsAsAString);
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCN2";
			AssertEquals("CCNsAsAString", "CCN1,CCN2", declaration.JE_CCNsAsAString);
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN3";
			AssertEquals("CCNsAsAString", "CCN1,CCN2,CCN3", declaration.JE_CCNsAsAString);
			declaration.AdditionalReferenceNumbers.RemoveAll();
			AssertEquals("CCNsAsAString", "CCN1,CCN3", declaration.JE_CCNsAsAString);
			declaration.CargoControlNumbers.AddNew();
			AssertEquals("CCNsAsAString", "CCN1,CCN3", declaration.JE_CCNsAsAString);
		}

		public override void TestMessageTypeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(JobMessageTypeList.Descriptions.Export, declaration.MessageTypeDescription);
		}

		public void TestJobDeclarationSynchroniser()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(typeof(JobDeclarationSynchroniser), declaration.ShipmentSynchroniser.GetType());
		}

		public void TestJE_MessageStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageStatus = "ZZZ";
			AssertEquals("", declaration.JE_MessageStatusDescription);

			declaration.JE_MessageStatus = MessageStatusList.Codes.Sent;
			AssertEquals("Sent", declaration.JE_MessageStatusDescription);
		}

		#region OnJE_MessageTypeChanged

		[TestDate(2010, 10, 21)]
		public void TestOnMessageTypeChanged()
		{
			CACustomsDataRegistry.Instance.DefaultPortOfClearance.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0497");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("JE_MessageSubType for Misc", ZString.Empty, declaration.JE_MessageSubType);
			AssertEquals("CA_AssesmentOption for Misc", ZString.Empty, declaration.CA_AssesmentOption);
			AssertEquals("JE_TransportMode for Misc", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("JE_EntryAuthorisationDate for Misc", ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
			AssertEquals("JE_CustomsOffice for Misc", ZString.Empty, declaration.JE_CustomsOffice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_MessageSubType for Import", B3EntryTypeList.Codes.Confirming, declaration.JE_MessageSubType);
			AssertEquals("CA_AssesmentOption for Import", AssessmentOptions.Codes.AppraisalQualityData, declaration.CA_AssesmentOption);
			AssertEquals("JE_TransportMode for Import", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("JE_EntryAuthorisationDate for Import", ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
			AssertEquals("JE_CustomsOffice for Import", "0497", declaration.JE_CustomsOffice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_MessageSubType for Export", ZString.Empty, declaration.JE_MessageSubType);
			AssertEquals("CA_AssesmentOption for Export", ZString.Empty, declaration.CA_AssesmentOption);
			AssertEquals("JE_TransportMode for Export", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("JE_EntryAuthorisationDate for Export", ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
			AssertEquals("JE_CustomsOffice for Export", ZString.Empty, declaration.JE_CustomsOffice);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("JE_MessageSubType for LVS", LowValueShipmentsTypes.Codes.TotalConsolidation, declaration.JE_MessageSubType);
			AssertEquals("CA_AssesmentOption for LVS", AssessmentOptions.Codes.AppraisalQualityData, declaration.CA_AssesmentOption);
			AssertEquals("JE_TransportMode for LVS", TransportTypeList.Codes.Road, declaration.JE_TransportMode);
			AssertEquals("JE_EntryAuthorisationDate for LVS", new ZDateTime(2010, 10, 31), declaration.JE_EntryAuthorisationDate);
			AssertEquals("JE_CustomsOffice for LVS", "0497", declaration.JE_CustomsOffice);

			CACustomsDataRegistry.Instance.DefaultPortOfClearance.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, "0498");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_CustomsOffice for Export", ZString.Empty, declaration.JE_CustomsOffice);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("JE_CustomsOffice for LVS", "0498", declaration.JE_CustomsOffice);
		}

		public void TestPGAIndicatorOnMessageTypeChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_CNSCInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_GACInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_HCInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_NRCanInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_PHACInd = YesNoList.Codes.Yes;
			invoiceLine1.CA_TCInd = YesNoList.Codes.Yes;
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_CNSCInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_GACInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_HCInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_NRCanInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_PHACInd = YesNoList.Codes.Yes;
			invoiceLine2.CA_TCInd = YesNoList.Codes.Yes;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("CA_CFIAInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_CFIAInd);
			AssertEquals("CA_CNSCInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_CNSCInd);
			AssertEquals("CA_DFOInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_DFOInd);
			AssertEquals("CA_GACInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_GACInd);
			AssertEquals("CA_ECCCInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_ECCCInd);
			AssertEquals("CA_HCInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_HCInd);
			AssertEquals("CA_NRCanInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_NRCanInd);
			AssertEquals("CA_PHACInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_PHACInd);
			AssertEquals("CA_PHACInd for InvoiceLine1", ZString.Empty, invoiceLine1.CA_PHACInd);

			AssertEquals("CA_CFIAInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_CFIAInd);
			AssertEquals("CA_CNSCInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_CNSCInd);
			AssertEquals("CA_DFOInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_DFOInd);
			AssertEquals("CA_GACInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_GACInd);
			AssertEquals("CA_ECCCInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_ECCCInd);
			AssertEquals("CA_HCInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_HCInd);
			AssertEquals("CA_NRCanInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_NRCanInd);
			AssertEquals("CA_PHACInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_PHACInd);
			AssertEquals("CA_PHACInd for InvoiceLine2", ZString.Empty, invoiceLine2.CA_PHACInd);
		}

		public void TestUpdateTransportDocumentNumberIfApplicable()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("ShouldSynchroniseWithShipment()", false, declaration.ShouldSynchroniseWithShipment());
			declaration.CA_TransportDocumentNumber = "";
			declaration.JE_MasterBill = "0812321111";
			declaration.JE_HouseBill = "HWB23423";

			AssertEquals("CA_TransportDocumentNumber", "", declaration.CA_TransportDocumentNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("CA_TransportDocumentNumber", "081-2321111", declaration.CA_TransportDocumentNumber);

			declaration.JE_HouseBill = "HWB23444";
			AssertEquals("CA_TransportDocumentNumber shouldn't have changed as it wasn't empty", "081-2321111", declaration.CA_TransportDocumentNumber);

			declaration.CA_TransportDocumentNumber = "";
			declaration.JE_HouseBill = "HWB23433";
			AssertEquals("CA_TransportDocumentNumber should have defaulted to MasterBill as it was empty.", "081-2321111", declaration.CA_TransportDocumentNumber);

			declaration.JE_MasterBill = "0812321123";
			AssertEquals("CA_TransportDocumentNumber should defaulted to master bill as it master bill was updated", "081-2321123", declaration.CA_TransportDocumentNumber);

			declaration.CA_TransportDocumentNumber = "";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("CA_TransportDocumentNumber should be empty as it's not use by sea", "", declaration.CA_TransportDocumentNumber);

			declaration.JE_MasterBill = "";
			AssertEquals("CA_TransportDocumentNumber should be empty as it's not use by sea", "", declaration.CA_TransportDocumentNumber);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("CA_TransportDocumentNumber should have defaulted to HouseBill as it was empty.", "HWB-23433", declaration.CA_TransportDocumentNumber);

			shipment.JS_HouseBill = "HWB23444";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			declaration.CA_TransportDocumentNumber = "";
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = false;
			AssertEquals("ShouldSynchroniseWithShipment()", true, declaration.ShouldSynchroniseWithShipment());
			AssertEquals("CA_TransportDocumentNumber should have been synchronize with Shipment.", "HWB-23444", declaration.CA_TransportDocumentNumber);

			declaration.CA_TransportDocumentNumber = "";
			declaration.JE_HouseBill = "HWB2346";
			AssertEquals("CA_TransportDocumentNumber should not have been changed.", "", declaration.CA_TransportDocumentNumber);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("ShouldSynchroniseWithShipment()", false, declaration.ShouldSynchroniseWithShipment());
			declaration.JE_HouseBill = "HWB2345";
			AssertEquals("CA_TransportDocumentNumber should have defaulted to HouseBill.", "HWB-2345", declaration.CA_TransportDocumentNumber);

			declaration.JE_JS = ZGuid.Empty;
			AssertEquals("ShouldSynchroniseWithShipment()", false, declaration.ShouldSynchroniseWithShipment());
			declaration.CA_TransportDocumentNumber = "";
			declaration.JE_HouseBill = "HWB2346";
			AssertEquals("CA_TransportDocumentNumber should have defaulted to HouseBill.", "HWB-2346", declaration.CA_TransportDocumentNumber);
		}

		public void TestUpdateMergeByOnMessageTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Default JE_MergeBy for Misc", OrgConstants.MergeInvoiceLines.Tariff, declaration.JE_MergeBy);
			AssertEquals("Default CA_MergeBy for Misc", ZString.Empty, declaration.CA_MergeBy);
			AssertEquals("JE_MergeBy readonly for Misc", false, declaration.JE_MergeByInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Default JE_MergeBy for Import", OrgConstants.MergeInvoiceLines.NotMerge, declaration.JE_MergeBy);
			AssertEquals("Default CA_MergeBy for Import", OrgConstants.MergeInvoiceLines.Tariff, declaration.CA_MergeBy);
			AssertEquals("JE_MergeBy readonly for Import", true, declaration.JE_MergeByInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Default JE_MergeBy for Export", OrgConstants.MergeInvoiceLines.Tariff, declaration.JE_MergeBy);
			AssertEquals("Default CA_MergeBy for Export", ZString.Empty, declaration.CA_MergeBy);
			AssertEquals("JE_MergeBy readonly for Export", false, declaration.JE_MergeByInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("Default JE_MergeBy for LVS", JobMessageTypeList.Codes.LowValueShipments, declaration.JE_MergeBy);
			AssertEquals("Default CA_MergeBy for LVS", OrgConstants.MergeInvoiceLines.Tariff, declaration.CA_MergeBy);
			AssertEquals("JE_MergeBy readonly for LVS", true, declaration.JE_MergeByInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("Default JE_MergeBy for LVX", OrgConstants.MergeInvoiceLines.NotMerge, declaration.JE_MergeBy);
			AssertEquals("Default CA_MergeBy for LVX", B3MergeByList.Codes.ClassificationTariff, declaration.CA_MergeBy);
			AssertEquals("JE_MergeBy readonly for LVX", true, declaration.JE_MergeByInfo.ReadOnly);
		}

		#endregion

		public void TestResetLineCalculationMethodOnLVSTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;

			AssertEquals("InvoiceLines.Count", 2, invoice.InvoiceLines.Count);
			foreach (JobComInvoiceLine invoiceLine in invoice.InvoiceLines)
			{
				AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RepairsRemission, invoiceLine.CA_CalculationMethod);
			}

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			AssertEquals("InvoiceLines.Count", 1, invoice.InvoiceLines.Count);
			AssertEquals("Calculation method should be reset to No Remission if LVS type changed from OIC", CalculationMethods.Codes.NoRemission, line.CA_CalculationMethod);
		}

		public void TestPermits()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Permits", typeof(DeclarationExportPermitCollection), declaration.Permits.GetType());
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection), declaration.Bills.GetType());
		}

		public void TestDeriveDeclarationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			AssertEquals("no message status yet", ZString.Empty, declaration.JE_MessageStatus);
			AssertEquals("no entry status yet", ZString.Empty, declaration.JE_EntryStatus);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.NotSent;
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("declaration's MessageStatus", MessageStatusList.Codes.NotSent, declaration.JE_MessageStatus);
			AssertEquals("declaration's EntryStatus", ZString.Empty, declaration.JE_EntryStatus);
			entryHeader.CH_Status = MessageStatusList.Codes.Sent;
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			declaration.DeriveDeclarationStatus();
			AssertEquals("declaration's MessageStatus", MessageStatusList.Codes.Sent, declaration.JE_MessageStatus);
			AssertEquals("declaration's EntryStatus", EntryStatusList.Codes.Clear, declaration.JE_EntryStatus);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader2.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsDetained;
			entryHeader2.CH_Status = MessageStatusList.Codes.Sent;
			var message1 = entryHeader.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2010, 5, 21);
			var message2 = entryHeader2.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2010, 5, 20);
			declaration.DeriveDeclarationStatus();
			AssertEquals("declaration's MessageStatus", MessageStatusList.Codes.AwaitingChange, declaration.JE_MessageStatus);
			AssertEquals("declaration's EntryStatus", EDIReleaseImportEntryStatusList.Codes.GoodsDetained, declaration.JE_EntryStatus);
		}

		public void TestReleaseStatusDocWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ZString.Empty, declaration.ReleaseStatusWrapper.ProcessingIndicatorDescription);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var message1 = entryHeader.Messages.AddNew(typeof(EDIReleaseMessage));
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 1);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_Status = EDIMessage.Status.Received;
			message1.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Error;
			message1.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+14'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");
			declaration.ResetCachedValuesForTesting();
			AssertEquals(ZString.Empty, declaration.ReleaseStatusWrapper.ProcessingIndicatorDescription);
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			declaration.ResetCachedValuesForTesting();
			AssertEquals("14 - Error in last message, please fix and re-submit", declaration.ReleaseStatusWrapper.ProcessingIndicatorDescription);

			var message2 = entryHeader.Messages.AddNew(typeof(EDIReleaseMessage));
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 2);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Received;
			message2.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			message2.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+4'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");
			declaration.ResetCachedValuesForTesting();
			AssertEquals("4 - Goods Released", declaration.ReleaseStatusWrapper.ProcessingIndicatorDescription);

			var message3 = entryHeader.Messages.AddNew(typeof(EDIReleaseMessage));
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 4);
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_Status = EDIMessage.Status.Received;
			message3.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Error;
			message3.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+14'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");
			declaration.ResetCachedValuesForTesting();
			AssertEquals("4 - Goods Released", declaration.ReleaseStatusWrapper.ProcessingIndicatorDescription);

			var message4 = entryHeader.Messages.AddNew(typeof(EDIReleaseMessage));
			message4.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 5);
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_Status = EDIMessage.Status.Received;
			message4.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.MessageContentRejected;
			message4.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+14'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");
			declaration.ResetCachedValuesForTesting();
			AssertEquals("4 - Goods Released", declaration.ReleaseStatusWrapper.ProcessingIndicatorDescription);

			var message5 = entryHeader.Messages.AddNew(typeof(EDIReleaseMessage));
			message5.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 3);
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5.EM_Status = EDIMessage.Status.Received;
			message5.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			message5.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+9'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");
			declaration.ResetCachedValuesForTesting();
			AssertEquals("9 - Release Canceled", declaration.ReleaseStatusWrapper.ProcessingIndicatorDescription);
		}

		public void TestPreviousCargoControlNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var pccn = CusEntryNumber.New(declaration, CanadaAdditionalReferenceNumberTypes.Codes.PCN, Core.Constants.CountryCodes.Canada);
			pccn.CE_EntryNum = "12345XX";
			AssertEquals("Previous CCN", "12345XX", declaration.PreviousCCN);
		}

		public void TestOnEntryNumChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(0, declaration.ReleaseStatuses.Count);

			var number = (CusEntryNumber)((IBindingList)declaration.AdditionalReferenceNumbers).AddNew(); // add uncommitted CCN
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCN123456789";
			((ICancelAddNew)declaration.AdditionalReferenceNumbers).EndNew(0); //commit CCN
			AssertEquals(1, declaration.ReleaseStatuses.Count);
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = bizO.Lookups;
			var secondLookup = bizO.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		public void TestTypeDecider()
		{
			Assert("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>().GetType() == GetExpectedBusinessObjectType());
		}

		public void TestIMessageManageableBizObj()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var exportEntry = declaration.CustomsEntryHeaders.AddNew();
			exportEntry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			var message = exportEntry.Messages.AddNew();
			message.EM_MessageType = MessageTypeList.Codes.DataLoadingModule;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;

			IBackDoorSavingSupportableBizObj bizObj = declaration;
			AssertEquals("SupportBackDoorForSavingWhenAmendmentDetected", true, bizObj.SupportBackDoorForSavingWhenAmendmentDetected);
			AssertEquals("IsInAStatusAmendmentSendable", true, bizObj.IsInAStatusAmendmentSendable);

			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;

			bizObj = declaration;
			AssertEquals("SupportBackDoorForSavingWhenAmendmentDetected", true, bizObj.SupportBackDoorForSavingWhenAmendmentDetected);
			AssertEquals("IsInAStatusAmendmentSendable", false, bizObj.IsInAStatusAmendmentSendable);
		}

		public void TestSettingImporterForLVS()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("JE_MessageType not changed", JobMessageTypeList.Codes.LowValueShipments, declaration.JE_MessageType);
			AssertEquals("JE_RL_NKFinalDestination  not changed", string.Empty, declaration.JE_RL_NKFinalDestination);
			AssertEquals("JE_RL_NKPortOfArrival not changed", string.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("ImporterDocumentaryAddress set", importer.PK, declaration.ImporterDocumentaryAddress.OrganisationPK);
			AssertEquals("CA_MergeBy set", "TRF", declaration.CA_MergeBy);

			PartyInCurrentCountry.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";
			declaration.JE_OH_Importer = PartyInCurrentCountry.PK;

			AssertEquals("JE_MessageType not changed", JobMessageTypeList.Codes.LowValueShipments, declaration.JE_MessageType);
			AssertEquals("JE_RL_NKFinalDestination  not changed", string.Empty, declaration.JE_RL_NKFinalDestination);
			AssertEquals("JE_RL_NKPortOfArrival not changed", string.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("ImporterDocumentaryAddress set", PartyInCurrentCountry.PK, declaration.ImporterDocumentaryAddress.OrganisationPK);
			AssertEquals("CA_MergeBy set", "TRF", declaration.CA_MergeBy);
		}

		public virtual void TestSettingDefaultMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = PartyInCurrentCountry.PK;
			AssertEquals("Import as import dec is active", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
		}

		public void TestCustomsClearedEvent()
		{
			try
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var clrLog = declaration.Logs.AddNew(Events.CustomsCleared, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
				var cncLog = declaration.Logs.AddNew(Events.Cancelled, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				Factory.Save();
				((IEDIReleaseMessageAttachee)entry).SettingReleaseDateWithStatusUpdate = true;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 2);
				AssertNull("No declaration event", declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCleared));
				AssertNull("No entry event", entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				((IEDIReleaseMessageAttachee)entry).SettingReleaseDateWithStatusUpdate = false;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 3);
				AssertNotNull("Declaration event exists", declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCleared));
				var clearedEvent = entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
				AssertNotNull("Customs Cleared event should exist on entry header", clearedEvent);
				AssertEquals("Cleared date", new ZDateTime(2011, 1, 3), clearedEvent.SL_EventTime);
				AssertEquals("Cleared status code", "MANUAL", clearedEvent.SL_Reference);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				AssertNull("No declaration event", declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCleared));
				AssertNull("No entry event", entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				var cancelledEvent = declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.Cancelled);
				AssertNotNull("Cancelled Customs Cleared event should exist on declaration", cancelledEvent);
				AssertEquals("Cancelled reference", "Customs Cleared event canceled", cancelledEvent.SL_Reference);
				cancelledEvent = entry.Logs.MostRecentLogByEventTime(Events.Cancelled);
				AssertNotNull("Cancelled Customs Cleared event should exist on entry header", cancelledEvent);
				AssertEquals("Cancelled reference", "Customs Cleared event canceled", cancelledEvent.SL_Reference);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestStuEvents()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Factory.Save();
			declaration.CA_B2SubmissionDate = new ZDateTime(2016, 09, 08);
			Factory.Save();
			var stuEvent = declaration.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull("Status Updated event should exist", stuEvent);
			AssertEquals("Cleared date", new ZDateTime(2016, 09, 08), stuEvent.SL_EventTime);
			AssertEquals("Reference", "|RES=SUBMITTED|TYP=B2", stuEvent.SL_Reference);

			declaration.CA_B2SubmissionDate = ZDateTime.Empty;
			Factory.Save();
			stuEvent = declaration.Logs.MostRecentLogByPostedDate;
			AssertNotNull("Status Updated event should exist", stuEvent);
			AssertEquals("Cleared date", new ZDateTime(2016, 09, 08), stuEvent.SL_EventTime);
			AssertEquals("Reference", "|RES=SUBMITTED CANCELLED|TYP=B2", stuEvent.SL_Reference);

			declaration.CA_B2AcceptedDate = new ZDateTime(2016, 09, 09);
			Factory.Save();

			stuEvent = declaration.Logs.MostRecentLogByPostedDate;
			AssertNotNull("Status Updated event should exist", stuEvent);
			AssertEquals("Update date", new ZDateTime(2016, 09, 09), stuEvent.SL_EventTime);
			AssertEquals("Reference", "|RES=DECIDED|TYP=B2", stuEvent.SL_Reference);

			declaration.CA_B2AcceptedDate = ZDateTime.Empty;
			Factory.Save();

			stuEvent = declaration.Logs.MostRecentLogByPostedDate;
			AssertNotNull("Status Updated event should exist", stuEvent);
			AssertEquals("Update date", new ZDateTime(2016, 09, 09), stuEvent.SL_EventTime);
			AssertEquals("Reference", "|RES=DECIDED CANCELLED|TYP=B2", stuEvent.SL_Reference);

			declaration.CA_ConfirmedDate = new ZDateTime(2016, 09, 10);
			Factory.Save();

			stuEvent = declaration.Logs.MostRecentLogByPostedDate;
			AssertNotNull("Status Updated event should exist", stuEvent);
			AssertEquals("Update date", new ZDateTime(2016, 09, 10), stuEvent.SL_EventTime);
			AssertEquals("Reference", "|RES=CONFIRMED|TYP=B2", stuEvent.SL_Reference);

			declaration.CA_ConfirmedDate = ZDateTime.Empty;
			Factory.Save();

			stuEvent = declaration.Logs.MostRecentLogByPostedDate;
			AssertNotNull("Status Updated event should exist", stuEvent);
			AssertEquals("Update date", new ZDateTime(2016, 09, 10), stuEvent.SL_EventTime);
			AssertEquals("Reference", "|RES=CONFIRMED CANCELLED|TYP=B2", stuEvent.SL_Reference);
		}

		public void TestEffectiveCCN()
		{
			var baseDec = Factory.NewWithValidTestData<JobDeclaration>();
			baseDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			baseDec.JE_TransportMode = Constants.TransportModes.Road;
			baseDec.JE_DeclarationReference = "B00000001";
			var header = baseDec.Invoices.AddNew();
			header.JZ_InvoiceNumber = "INVNO";
			AssertEquals(ZString.Empty, baseDec.EffectiveCCN);
			var release = baseDec.ReleaseStatuses.AddNew();
			release.RL_CargoControlNumber = "00001";
			AssertEquals("00001", baseDec.EffectiveCCN);
			baseDec.EffectiveCCN = "00002";
			AssertEquals("00002", release.RL_CargoControlNumber);
			baseDec.EffectiveCCN = "";
			AssertEquals(0, baseDec.ReleaseStatuses.Count);
			baseDec.EffectiveCCN = "00003";
			var release2 = baseDec.ReleaseStatuses.AddNew();
			release2.RL_CargoControlNumber = "00002";
			AssertEquals(2, baseDec.ReleaseStatuses.Count);
			AssertEquals("Multiple CCNs – See Packing", baseDec.EffectiveCCN);

			baseDec.ReleaseStatuses.RemoveAndDeleteAll();
			baseDec.EffectiveCCN = "1234";
			Factory.Save();

			var newDec = Factory.NewWithValidTestData<JobDeclaration>();
			newDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			newDec.JE_TransportMode = Constants.TransportModes.Road;
			newDec.JE_DeclarationReference = "B00000002";
			newDec.EffectiveCCN = "1234";
			AssertHasMessageErrorContaining("Error expected: Another Declaration already contains the same CCN as this declaration.", newDec.EffectiveCCNPrefixInfo, "Another Declaration already contains the same CCN as this declaration");
			newDec.EffectiveCCN = "1234TEST";
			Factory.Save();
			AssertNoMessageErrorContaining("No error expected.", newDec.EffectiveCCNPrefixInfo, "Another Declaration already contains the same CCN as this declaration");
		}

		public void TestEffectiveCCNPrefix()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.EffectiveCCN = "14352679";
			AssertEquals("1435", dec.EffectiveCCNPrefix.ToString());

			dec.EffectiveCCNPrefix = "869";
			AssertHasMessageError("Error expected: when the length of CCN prefix less than 4", dec.EffectiveCCNPrefixInfo, "The CCN-prefix should be composed of 4 characters");

			dec.EffectiveCCNPrefix = "68168113";
			AssertEquals("8113", dec.EffectiveCCNSuffix.ToString());

			var release2 = dec.ReleaseStatuses.AddNew();
			release2.RL_CargoControlNumber = "00002";
			AssertEquals(2, dec.ReleaseStatuses.Count);
			AssertEquals("Multiple CCNs – See Packing", dec.EffectiveCCN);
			AssertEquals(ZString.Empty, dec.EffectiveCCNPrefix);
		}

		public void TestEffectiveCCNSuffix()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals(dec.EffectiveCCNSuffix, ZString.Empty);

			dec.EffectiveCCNPrefix = "8694";
			dec.EffectiveCCNSuffix = "375392";
			AssertEquals(dec.EffectiveCCN.ToString(), "8694375392");

			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			dec.EffectiveCCNSuffix = "375391";
			AssertEquals(dec.JE_HouseBill.ToString(), "375391");

			var release2 = dec.ReleaseStatuses.AddNew();
			release2.RL_CargoControlNumber = "00002";
			AssertEquals(2, dec.ReleaseStatuses.Count);
			AssertEquals("Multiple CCNs – See Packing", dec.EffectiveCCN);
			AssertEquals("Multiple CCNs – See Packing", dec.EffectiveCCNSuffix);
		}

		public void TestCreationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("Manual B2", declaration.CreationMethod);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals("Copy IM2", declaration.CreationMethod);
		}

		public void TestCustomsReadyToPayEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var estimatedEvent = declaration.LogsOfDeclarationOrShipment.AddNew(Events.CustomsReadyToPay, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();
			declaration.CA_K84AccountingDate = new ZDateTime(2011, 1, 2);
			var readyToPayEvent = declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			AssertNotNull("Customs Ready To Pay event should exist on declaration", readyToPayEvent);
			AssertEquals("Accounting date", declaration.CA_K84AccountingDate, readyToPayEvent.SL_EventTime);
		}

		public void TestCustomsReadyToPayEventWithAttachedLVXJob()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);
			var estimatedEvent = lvsJob.LogsOfDeclarationOrShipment.AddNew(Events.CustomsReadyToPay, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
			var entry = lvsJob.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();
			lvsJob.CA_K84AccountingDate = new ZDateTime(2011, 1, 2);
			var readyToPayEvent = lvsJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			AssertNotNull("Customs Ready To Pay event should exist on declaration", readyToPayEvent);
			AssertEquals("Accounting date", lvsJob.CA_K84AccountingDate, readyToPayEvent.SL_EventTime);
			readyToPayEvent = lvxJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			AssertNotNull("Customs Ready To Pay event should exist on declaration", readyToPayEvent);
			AssertEquals("Accounting date", lvsJob.CA_K84AccountingDate, readyToPayEvent.SL_EventTime);
		}

		public void TestCustomsReadyToPayEventWithAttachedLVXJobWhenCloseDateSet()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);
			var estimatedEvent = lvsJob.LogsOfDeclarationOrShipment.AddNew(Events.CustomsReadyToPay, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
			var entry = lvsJob.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();
			lvsJob.CA_LVSCloseDate = new ZDateTime(2011, 1, 2);
			var readyToPayEventForLVS = lvsJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			AssertNotNull("Customs Ready To Pay event should exist on declaration", readyToPayEventForLVS);
			AssertEquals("Event Date", new ZDateTime(2011, 1, 2), readyToPayEventForLVS.SL_EventTime);
			var readyToPayEventForLVX = lvxJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			AssertNotNull("Customs Ready To Pay event should exist on declaration", readyToPayEventForLVX);
			AssertEquals("Event Date", new ZDateTime(2011, 1, 2), readyToPayEventForLVX.SL_EventTime);
			lvsJob.CA_LVSCloseDate = ZDateTime.Empty;
			Assert("Customs Ready To Pay event should be deleted", readyToPayEventForLVS.IsDeleted);
			Assert("Customs Ready To Pay event should be deleted", readyToPayEventForLVX.IsDeleted);
			lvsJob.CA_LVSCloseDate = new ZDateTime(2011, 1, 2);
			readyToPayEventForLVS = lvsJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			AssertNotNull("Customs Ready To Pay event should exist on declaration", readyToPayEventForLVS);
			AssertEquals("Event Date", new ZDateTime(2011, 1, 2), readyToPayEventForLVS.SL_EventTime);
			readyToPayEventForLVX = lvxJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			AssertNotNull("Customs Ready To Pay event should exist on declaration", readyToPayEventForLVX);
			AssertEquals("Event Date", new ZDateTime(2011, 1, 2), readyToPayEventForLVX.SL_EventTime);
			Factory.Save();
			lvsJob.CA_LVSCloseDate = ZDateTime.Empty;
			Assert("Customs Ready To Pay event should be cancelled", readyToPayEventForLVS.IsCancelled);
			Assert("Customs Ready To Pay event should be cancelled", readyToPayEventForLVX.IsCancelled);
		}

		public void TestTriggerCARActionByCustomsReadyToPayEvent()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);
			var trigger = lvxJob.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger auto-rating";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsReadyToPay.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue;
			Factory.Save();
			Assert("Precondition", trigger.P9_ActualDate.IsEmpty);
			lvsJob.CA_K84AccountingDate = new ZDateTime(2011, 1, 2);
			Assert("Trigger", !trigger.P9_ActualDate.IsEmpty);
		}

		public void TestCustomsCommencedEventWithAttachedLVXJob()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var estimatedEvent = lvsJob.LogsOfDeclarationOrShipment.AddNew(Events.CustomsCommenced, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);
			var commencedEvent = lvxJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCommenced);
			AssertNotNull("Customs Commenced event should exist on declaration", commencedEvent);
			AssertEquals("Reference", lvxJob.JE_DeclarationReference, commencedEvent.SL_Reference);
			LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);
			commencedEvent = lvxJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCommenced);
			AssertNull("Customs Commenced event should be cancelled on declaration", commencedEvent);
		}

		public void TestTriggerCARActionByCustomsCommencedEvent()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var trigger = lvxJob.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger auto-rating";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsCommenced.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue;
			Factory.Save();
			Assert("Precondition", trigger.P9_ActualDate.IsEmpty);
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);
			Assert("Trigger", !trigger.P9_ActualDate.IsEmpty);
		}

		public void TestTriggerPostingByCAREvent()
		{
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var trigger = lvxJob.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "post changes";
			trigger.TriggerConditions.TriggerEventCode = Events.ChargesHaveBeenAutoRatedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.PostAllRevenue;
			Assert("Precondition", trigger.P9_ActualDate.IsEmpty);
			lvxJob.Logs.AddNew(Events.ChargesHaveBeenAutoRated);
			Assert("Trigger", !trigger.P9_ActualDate.IsEmpty);
		}

		public void TestBrokerAndReleaseOfficeEmptyInCopy()
		{
			var aStaff = Factory.New<GlbStaff>();
			aStaff.GS_Code = "KR";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = aStaff.GS_Code;
			declaration.CA_ReleaseOffice = "AAA";

			Factory.Save();

			var declarationCopy = (JobDeclaration)declaration.TemplateCopy();

			AssertEquals("", declarationCopy.CA_ReleaseOffice);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, declarationCopy.JE_GS_NKCusAgent);
		}

		public void TestJobInvoicingSupporterForB2Jobs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(JobInvoicingConsumerTypes.PostClearanceBrokerage, declaration.InvoicingSupporter.ConsumerType);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNotEquals(JobInvoicingConsumerTypes.PostClearanceBrokerage, declaration.InvoicingSupporter.ConsumerType);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals(JobInvoicingConsumerTypes.PostClearanceBrokerage, declaration.InvoicingSupporter.ConsumerType);
		}

		#region TestClearQuantitiesInTemplateCopy

		public void TestClearQuantitiesInTemplateCopy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalVolume = 10m;
			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicMetres;
			declaration.JE_TotalNoOfPacks = 10;
			declaration.CA_AccountingAge = 10;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			fillTestQuantities(groupHeader);
			fillTestQuantities(invoice1);
			fillTestQuantities(invoice2);
			fillTestQuantities((JobComInvoiceLine)invoiceLine1);
			fillTestQuantities((JobComInvoiceLine)invoiceLine2);
			Factory.Save();

			var declarationCopy = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("CA_AccountingAge", 0, declarationCopy.CA_AccountingAge);
			AssertNull(declarationCopy.GetSystemDefinedValues().FirstOrDefault(x => x.PropertyName == JobDeclaration.Schema.CA_AccountingAge));

			foreach (var groupInvoice in declarationCopy.AllGroupHeaders)
			{
				AssertQuantitiesAreEmpty((JobComInvoiceGroupHeader)groupInvoice);
			}
			foreach (var invoice in declarationCopy.Invoices)
			{
				AssertQuantitiesAreEmpty((JobComInvoiceHeader)invoice);
			}
			foreach (var invoiceLine in declarationCopy.InvoiceLines)
			{
				AssertQuantitiesAreEmpty((JobComInvoiceLine)invoiceLine);
			}
			Assert("HasChanges", !declarationCopy.HasChanges);
		}

		void fillTestQuantities(JobComInvoiceGroupHeader groupInvoice)
		{
			groupInvoice.JZ_InvoiceDate = ZDateTime.Now;
			groupInvoice.JZ_InvoiceAmount = 10m;
			groupInvoice.JZ_Weight = 10m;
			groupInvoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			groupInvoice.JZ_Volume = 10m;
			groupInvoice.JZ_VolumeUQ = Core.Constants.Volume.CubicMetres;
			groupInvoice.JZ_NoOfPacks = 10m;
			groupInvoice.Charges.AddNew().J7_Amount = 10m;
		}

		void fillTestQuantities(JobComInvoiceHeader invoice)
		{
			invoice.JZ_InvoiceDate = ZDateTime.Now;
			invoice.JZ_InvoiceAmount = 10m;
			invoice.JZ_Weight = 10m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.JZ_Volume = 10m;
			invoice.JZ_VolumeUQ = Core.Constants.Volume.CubicMetres;
			invoice.JZ_NoOfPacks = 10m;
			invoice.JZ_NetWeight = 10m;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2015, 10, 16);
			invoice.Charges.AddNew().J7_Amount = 10m;
		}

		void fillTestQuantities(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Volume = 10m;
			invoiceLine.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_LinePrice = 10m;
			invoiceLine.JI_NetWeight = 10m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_BondedWhsQuantity = 10m;
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.JI_CustomsThirdQuantity = 10m;
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CVforCurrConv = 10m;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 10m;
			invoiceLine.Charges.AddNew().J7_Amount = 10m;
		}

		void AssertQuantitiesAreEmpty(JobComInvoiceGroupHeader groupInvoice)
		{
			Assert("JZ_InvoiceDate", groupInvoice.JZ_InvoiceDate.IsEmpty);
			Assert("JZ_InvoiceAmount", groupInvoice.JZ_InvoiceAmount.IsEmpty);
			Assert("JZ_Weight", groupInvoice.JZ_Weight.IsEmpty);
			Assert("JZ_WeightUQ", groupInvoice.JZ_WeightUQ.IsEmpty);
			Assert("JZ_Volume", groupInvoice.JZ_Volume.IsEmpty);
			Assert("JZ_VolumeUQ", groupInvoice.JZ_VolumeUQ.IsEmpty);
			Assert("JZ_NoOfPacks", groupInvoice.JZ_NoOfPacks.IsEmpty);
			Assert("HasChanges", !groupInvoice.HasChanges);

			foreach (JobComInvCharge charge in groupInvoice.Charges)
			{
				Assert("J7_Amount", charge.J7_Amount.IsEmpty);
				Assert("HasChanges", !charge.HasChanges);
			}
		}

		void AssertQuantitiesAreEmpty(JobComInvoiceHeader invoice)
		{
			Assert("JZ_InvoiceDate", invoice.JZ_InvoiceDate.IsEmpty);
			Assert("JZ_InvoiceAmount", invoice.JZ_InvoiceAmount.IsEmpty);
			Assert("JZ_Weight", invoice.JZ_Weight.IsEmpty);
			Assert("JZ_WeightUQ", invoice.JZ_WeightUQ.IsEmpty);
			Assert("JZ_Volume", invoice.JZ_Volume.IsEmpty);
			Assert("JZ_VolumeUQ", invoice.JZ_VolumeUQ.IsEmpty);
			Assert("JZ_NoOfPacks", invoice.JZ_NoOfPacks.IsEmpty);
			Assert("JZ_NetWeight", invoice.JZ_NetWeight.IsEmpty);
			Assert("JZ_ValuationDateOverride", invoice.JZ_ValuationDateOverride.IsEmpty);
			Assert("HasChanges", !invoice.HasChanges);

			foreach (JobComInvCharge charge in invoice.Charges)
			{
				Assert("J7_Amount", charge.J7_Amount.IsEmpty);
				Assert("HasChanges", !charge.HasChanges);
			}
		}

		void AssertQuantitiesAreEmpty(JobComInvoiceLine invoiceLine)
		{
			Assert("JI_Weight", invoiceLine.JI_Weight.IsEmpty);
			Assert("JI_WeightUQ", invoiceLine.JI_WeightUQ.IsEmpty);
			Assert("JI_Volume", invoiceLine.JI_Volume.IsEmpty);
			Assert("JI_VolumeUQ", invoiceLine.JI_VolumeUQ.IsEmpty);
			Assert("JI_InvoiceQuantity", invoiceLine.JI_InvoiceQuantity.IsEmpty);
			Assert("JI_InvoiceUQ", invoiceLine.JI_InvoiceUQ.IsEmpty);
			Assert("JI_CustomsQuantity", invoiceLine.JI_CustomsQuantity.IsEmpty);
			Assert("JI_LinePrice", invoiceLine.JI_LinePrice.IsEmpty);
			Assert("JI_NetWeight", invoiceLine.JI_NetWeight.IsEmpty);
			Assert("JI_NetWeightUQ", invoiceLine.JI_NetWeightUQ.IsEmpty);
			Assert("JI_BondedWhsQuantity", invoiceLine.JI_BondedWhsQuantity.IsEmpty);
			Assert("JI_CustomsSecondQuantity", invoiceLine.JI_CustomsSecondQuantity.IsEmpty);
			Assert("JI_CustomsThirdQuantity", invoiceLine.JI_CustomsThirdQuantity.IsEmpty);
			Assert("CA_CVforCurrConvOvr", !invoiceLine.CA_CVforCurrConvOvr);
			Assert("CA_CVforCurrConv", invoiceLine.CA_CVforCurrConv.IsEmpty);
			Assert("CA_CustomsValueOvr", !invoiceLine.CA_CustomsValueOvr);
			Assert("CA_CustomsValue", invoiceLine.CA_CustomsValue.IsEmpty);
			Assert("HasChanges", !invoiceLine.HasChanges);

			foreach (JobComInvCharge charge in invoiceLine.Charges)
			{
				Assert("J7_Amount", charge.J7_Amount.IsEmpty);
				Assert("HasChanges", !charge.HasChanges);
			}
		}

		public void TestResetValuesInTemplateCopyForImportOnly()
		{
			var sourceDecl = Factory.New<JobDeclaration>();
			sourceDecl.JE_MessageType = JobMessageTypeList.Codes.Import;
			sourceDecl.CA_NetWeight = 10m;
			sourceDecl.CA_NetWeightUQ = Core.Constants.Weight.Pounds;
			sourceDecl.CA_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;
			Factory.Save();

			var clonedDecl1 = (JobDeclaration)sourceDecl.TemplateCopy();
			Assert("CA_NetWeight should not be copied", clonedDecl1.CA_NetWeight.IsEmpty);
			Assert("CA_NetWeightUQ should not be copied", clonedDecl1.CA_NetWeightUQ.IsEmpty);
			Assert("CA_NetWeightUQ should not be copied", clonedDecl1.CA_AmendReasonCode.IsEmpty);

			sourceDecl.JE_MessageType = JobMessageTypeList.Codes.Export;
			var clonedDecl2 = (JobDeclaration)sourceDecl.TemplateCopy();
			AssertEquals("CA_NetWeight should be copied", 10m, clonedDecl2.CA_NetWeight);
			AssertEquals("CA_NetWeightUQ should be copied", Core.Constants.Weight.Pounds, clonedDecl2.CA_NetWeightUQ);
		}

		public void TestTemplateCopyForAddInfo()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CA_DeclarationException = "123";
			Factory.Save();
			AssertNotNull(dec.GetSystemDefinedValues().FirstOrDefault(x => x.PropertyName == JobDeclaration.Schema.CA_DeclarationException));

			var clonedDec = (JobDeclaration)dec.TemplateCopy();
			AssertEquals("", clonedDec.CA_DeclarationException);
			AssertNull(clonedDec.GetSystemDefinedValues().FirstOrDefault(x => x.PropertyName == JobDeclaration.Schema.CA_DeclarationException));
			Assert(!clonedDec.HasChanges);
		}

		#endregion

		public void TestDeclarationMessagesHaveBeenSent()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("DeclarationMessagesHaveBeenSent false", !declaration.DeclarationMessagesHaveBeenSent());
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert("DeclarationMessagesHaveBeenSent false", !declaration.DeclarationMessagesHaveBeenSent());
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert("DeclarationMessagesHaveBeenSent false", !declaration.DeclarationMessagesHaveBeenSent());
			var message = entryHeader.Messages.AddNew();
			Assert("DeclarationMessagesHaveBeenSent true", declaration.DeclarationMessagesHaveBeenSent());
			message.EM_Status = EDIMessage.Status.Discarded;
			Assert("DeclarationMessagesHaveBeenSent false", !declaration.DeclarationMessagesHaveBeenSent());
		}

		public void TestIsExistingEffectiveCasualImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			Assert("Is not existing effective casual import", !declaration.IsExistingEffectiveCasualImport);

			invoice1.CA_IsCasualImport = true;
			Assert("Is existing effective casual import", declaration.IsExistingEffectiveCasualImport);
		}

		public void TestDefaultDeclarantOnB3()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, Guid.Empty);
			AssertNull(declaration.DeclarantOnEntryDocsBrokerOnB3);
			var declarant = Factory.New<GlbStaff>();
			declarant.GS_Code = "OOO";
			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, declarant.PK.ToGuid());
			AssertEquals("Default Declaration PK", declarant.PK, declaration.DeclarantOnEntryDocsBrokerOnB3.PK);
		}

		public void TestDefaultDeclarantOnB2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB2.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, Guid.Empty);
			AssertNull(declaration.DeclarantOnEntryDocsBrokerOnB2);
			var declarant = Factory.New<GlbStaff>();
			declarant.GS_Code = "OOO";
			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB2.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, declarant.PK.ToGuid());
			AssertEquals("Default Declaration PK", declarant.PK, declaration.DeclarantOnEntryDocsBrokerOnB2.PK);
		}

		public void TestIsPrintBrokerSignatureImage()
		{
			AssertBooleanRegistryItems(CACustomsDataRegistry.Instance.ShouldPrintBrokerSignatureOnEntryDocs, (declaration) => declaration.IsPrintBrokerSignatureImage, true);
			AssertBooleanRegistryItems(CACustomsDataRegistry.Instance.ShouldPrintBrokerSignatureOnEntryDocs, (declaration) => declaration.IsPrintBrokerSignatureImage, false);
		}

		public void TestDefaultExciseDutyQuantityToFirstCustomsQuantity()
		{
			AssertBooleanRegistryItems(CACustomsDataRegistry.Instance.DefaultExciseDutyQuantityToFirstCustomsQuantity, (declaration) => declaration.IsDefaultExciseDutyQuantityToFirstCustomsQuantity, true);
			AssertBooleanRegistryItems(CACustomsDataRegistry.Instance.DefaultExciseDutyQuantityToFirstCustomsQuantity, (declaration) => declaration.IsDefaultExciseDutyQuantityToFirstCustomsQuantity, false);
		}

		void AssertBooleanRegistryItems(BooleanRegistryItem registryItem, Func<JobDeclaration, bool> propertyGetter, bool expected)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (registryItem.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, expected))
			{
				AssertEquals(registryItem.Name, expected, propertyGetter(declaration));
			}
		}

		#region LVX Jobs

		public void TestLVXInvoiceHeader()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;

			AssertNotNull("LVX should have a default invoice", declaration1.LVXInvoiceHeader);
			AssertEquals("LVX should have a default invoice", 1, declaration1.Invoices.Count);

			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull("LVXInvoiceHeader should be null", declaration1.LVXInvoiceHeader);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull("LVXInvoiceHeader should be null", declaration2.LVXInvoiceHeader);
			AssertEquals("Invoices should be empty", 0, declaration2.Invoices.Count);
		}

		public void TestCA_K84StatementDateForLVX()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1.CA_K84StatementDate = new ZDateTime(2015, 4, 29);
			AssertEquals("CA_K84StatementDate", new ZDateTime(2015, 4, 29), declaration1.CA_K84StatementDate);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.CA_K84StatementDate = new ZDateTime(2015, 4, 29);
			AssertEquals("CA_K84StatementDate", ZDateTime.Empty, declaration2.CA_K84StatementDate);
			declaration2.LVXInvoiceHeader.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("CA_K84StatementDate", new ZDateTime(2015, 4, 29), declaration2.CA_K84StatementDate);
		}

		[TestDate(2025, 5, 28)]
		public void TestCA_K84AccountingDateForNonLVX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);
			declaration.CA_K84AccountingDate = new ZDateTime(2025, 5, 20);
			AssertEquals("CA_AccountingAge", 0, declaration.CA_AccountingAge);
			declaration.CA_K84AccountingDate = ZDateTime.Empty;
			AssertEquals("CA_AccountingAge", 6, declaration.CA_AccountingAge);
		}

		public void TestCA_K84AccountingDateForLVX()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("CA_K84AccountingDate is not ReadOnly when message type is LVS", false, declaration1.CA_K84AccountingDateInfo.ReadOnly);
			declaration1.CA_K84AccountingDate = new ZDateTime(2015, 4, 29);
			AssertEquals("CA_K84AccountingDate", new ZDateTime(2015, 4, 29), declaration1.CA_K84AccountingDate);
			AssertEquals("CA_AccountingAge", 0, declaration1.CA_AccountingAge);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("CA_K84AccountingDate is ReadOnly when message type is LVX", true, declaration2.CA_K84AccountingDateInfo.ReadOnly);
			declaration2.LVXInvoiceHeader.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("CA_K84AccountingDate", ZDateTime.Empty, declaration2.CA_K84AccountingDate);
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("CA_K84AccountingDate is ReadOnly when message type is LVX", true, declaration3.CA_K84AccountingDateInfo.ReadOnly);
			declaration3.LVXInvoiceHeader.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("CA_K84AccountingDate", ZDateTime.Empty, declaration3.CA_K84AccountingDate);
			declaration1.CA_K84AccountingDate = new ZDateTime(2017, 4, 29);
			AssertEquals("CA_K84AccountingDate", new ZDateTime(2017, 4, 29), declaration2.CA_K84AccountingDate);
			AssertEquals("CA_AccountingAge", 0, declaration2.CA_AccountingAge);
			AssertEquals("CA_K84AccountingDate", new ZDateTime(2017, 4, 29), declaration3.CA_K84AccountingDate);
			AssertEquals("CA_AccountingAge", 0, declaration3.CA_AccountingAge);
		}

		public void TestJE_MessageStatusDescriptionForLVX()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1.JE_MessageStatus = MessageStatusList.Codes.Sent;
			AssertEquals("JE_MessageStatusDescription", "Sent", declaration1.JE_MessageStatusDescription);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.JE_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertEquals("JE_MessageStatusDescription", ZString.Empty, declaration2.JE_MessageStatusDescription);
			declaration2.LVXInvoiceHeader.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("JE_MessageStatusDescription", "Sent", declaration2.JE_MessageStatusDescription);
		}

		public void TestTransactionNumberForLVX()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1.TransactionNumber.AccountSecurityCode = "40000";
			declaration1.TransactionNumber.SequentialNumber = "04228";
			AssertEquals("TransactionNumber.AccountSecurityCode", "40000", declaration1.TransactionNumber.AccountSecurityCode);
			AssertEquals("TransactionNumber.SequentialNumber", "00004228", declaration1.TransactionNumber.SequentialNumber);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.TransactionNumber.AccountSecurityCode = "30000";
			declaration2.TransactionNumber.SequentialNumber = "043444";
			AssertEquals("TransactionNumber.AccountSecurityCode", ZString.Empty, declaration2.TransactionNumber.AccountSecurityCode);
			AssertEquals("TransactionNumber.SequentialNumber", ZString.Empty, declaration2.TransactionNumber.SequentialNumber);
			declaration2.LVXInvoiceHeader.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("TransactionNumber.AccountSecurityCode", "40000", declaration2.TransactionNumber.AccountSecurityCode);
			AssertEquals("TransactionNumber.SequentialNumber", "00004228", declaration2.TransactionNumber.SequentialNumber);
		}

		public void TestImporterForAdditionalInvoices()
		{
			var importer1 = Factory.New<OrgHeader>();
			importer1.FillWithValidTestData();
			var importer2 = Factory.New<OrgHeader>();
			importer2.FillWithValidTestData();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration1.JE_OH_Importer = importer1.PK;
			declaration1.Invoices.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.LVXInvoiceHeader.JZ_OH_Buyer = importer1.PK;
			declaration2.LVXInvoiceHeader.AttachToAdditionalDeclaration(declaration1);

			declaration1.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration2.LVXInvoiceHeader.JZ_OH_Buyer = importer2.PK;
			declaration1.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertEquals("Importer specified on the shipment should match the header importer", importer1.PK, declaration2.LVXInvoiceHeader.JZ_OH_Buyer);
			AssertEquals("Importer specified on the shipment should match the header importer", importer1.PK, declaration2.JE_OH_Importer);

			declaration1.JE_OH_Importer = importer2.PK;
			AssertEquals("Importer specified on the shipment should match the header importer", importer2.PK, declaration2.LVXInvoiceHeader.JZ_OH_Buyer);
			AssertEquals("Importer specified on the shipment should match the header importer", importer2.PK, declaration2.JE_OH_Importer);
		}

		public void TestLVSImporter()
		{
			var importer1 = Factory.New<OrgHeader>();
			importer1.FillWithValidTestData();
			var importer2 = Factory.New<OrgHeader>();
			importer2.FillWithValidTestData();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration1.JE_OH_Importer = importer1.PK;

			AssertEquals(importer1, declaration1.LVSImporter);

			var invoice = declaration1.Invoices.AddNew();
			invoice.JZ_OH_Buyer = importer2.PK;
			AssertEquals(importer2, declaration1.LVSImporter);
		}

		public void TestLVSSupplier()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.FillWithValidTestData();
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration1.JE_OH_Supplier = supplier1.PK;

			AssertEquals(supplier1, declaration1.LVSSupplier);

			var invoice = declaration1.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertEquals(supplier2, declaration1.LVSSupplier);
		}

		public void TestForceLVSApportionmentandMerge()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvsJob.ResumeApportionment();

			Assert("Precondition:", !lvsJob.CA_RequiresMerge);
			Assert("Precondition:", !lvsJob.ApportionmentDirty);
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvxJob.LVXInvoiceHeader.InvoiceLines.AddNew();
			lvxJob.LVXInvoiceHeader.AttachToAdditionalDeclaration(lvsJob);
			Factory.Save();
			lvxJob = new BusinessObjectFactory().Load<JobDeclaration>(lvxJob.PK);
			lvsJob = lvxJob.LVXInvoiceHeader.FirstAdditionalDeclaration;
			lvxJob.MarkApportionmentDirty();
			Assert("Set Apportionment Dirty", lvsJob.ApportionmentDirty);
			lvxJob.ResumeApportionment();
			Assert("Set Apportionment Dirty to false", !lvsJob.ApportionmentDirty);
			Assert("Set CA_RequiresMerge", lvsJob.CA_RequiresMerge);

			lvsJob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			lvsJob.RunMergeForLVSIfRequired();
			Assert("CA_RequiresMerge for the LVS job", !lvsJob.CA_RequiresMerge);
			Assert("CA_RequiresMerge for the LVX job", !lvxJob.CA_RequiresMerge);

			lvxJob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			lvxJob.CA_RequiresMerge = true;
			lvxJob.RunMergeForLVSIfRequired();
			Assert("CA_RequiresMerge for the LVS job", !lvsJob.CA_RequiresMerge);
			Assert("CA_RequiresMerge for the LVX job", !lvxJob.CA_RequiresMerge);
		}

		#endregion

		#region Auto-rating

		public void TestIsImporterDirectPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertIsImporterDirectPayment(declaration, "No Importer", false);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			AssertIsImporterDirectPayment(declaration, "Importer with security", true);
			declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			AssertIsImporterDirectPayment(declaration, "Importer without security", false);
		}

		void AssertIsImporterDirectPayment(JobDeclaration declaration, string importerStatusComment, bool defaultExpectedResult)
		{
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is default", defaultExpectedResult, declaration.IsImporterDirectPayment);
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
			Assert(importerStatusComment + ", JE_PaymentMethod is broker", !declaration.IsImporterDirectPayment);
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer;
			Assert(importerStatusComment + ", JE_PaymentMethod is broker", declaration.IsImporterDirectPayment);
		}

		public void TestIsHighImporterAutoDutyDirectAmts()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("No Importer", !declaration.IsGSTDirectPayment);
			Assert("No Importer", !declaration.IsGSTDirectAutoRated);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			Assert("GST Direct not set on Importer", !declaration.IsGSTDirectPayment);
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			Assert("GST Direct is set on Importer", declaration.IsGSTDirectPayment);
		}

		public void TestIsLVSImporterDirectPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("No Importer", !declaration.IsGSTDirectPayment);
			Assert("No Importer", !declaration.IsGSTDirectAutoRated);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			Assert("GST Direct not set on Importer", !declaration.IsGSTDirectPayment);
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			Assert("GST Direct is set on Importer", declaration.IsGSTDirectPayment);
		}

		public void TestIsGSTDirect()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				Assert("No Importer", !declaration.IsGSTDirectPayment);
				Assert("No Importer", !declaration.IsGSTDirectAutoRated);
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				Assert("GST Direct not set on Importer", !declaration.IsGSTDirectPayment);
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				Assert("GST Direct is set on Importer", declaration.IsGSTDirectPayment);
				Assert("GST Direct AutoRate not set on Importer", !declaration.IsGSTDirectAutoRated);
				declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = true;
				Assert("GST Direct AutoRate set on Importer", declaration.IsGSTDirectAutoRated);
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
				Assert("GST Direct AutoRate not set on Importer", !declaration.IsGSTDirectAutoRated);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				declaration.ImporterAddInfo.ZO_CADIsBrokerToPay = true;
				Assert(!declaration.IsGSTDirectPayment);
				Assert(!declaration.IsGSTDirectAutoRated);
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				Assert(declaration.IsGSTDirectPayment);
				declaration.ImporterAddInfo.ZO_CADIsBrokerToPay = true;
				Assert(!declaration.IsGSTDirectPayment);
			}
		}

		public void TestGetService()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(JobDeclarationCustomsCharges), declaration.GetService(typeof(ICustomsCharges)).GetType());
			AssertNull(declaration.GetService(typeof(ICustomsFee)));
		}

		public void TestDefaultTotalNoOfPacksPackType()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(ACROSSServiceOptions.Codes.IID, declaration.CA_ServiceOption);
			AssertEquals(IIDUnitOfCountCodeList.Codes.Pack, declaration.JE_TotalNoOfPacksPackType);
		}

		public void TestSetDefaultValueForCFIARegistrationNumber()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense, "54321");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			AssertEquals("Pre Condition", 0, invoiceLine1.CFIARegistrationNumbers.Count);
			AssertEquals("Pre Condition", 0, invoiceLine2.CFIARegistrationNumbers.Count);
			AssertEquals("Pre Condition", 0, invoiceLine3.CFIARegistrationNumbers.Count);

			declaration.CA_OGDCFIA = true;
			AssertEquals("Pre Condition", 1, invoiceLine1.CFIARegistrationNumbers.Count);
			AssertEquals("Pre Condition", 1, invoiceLine2.CFIARegistrationNumbers.Count);
			AssertEquals("Pre Condition", 1, invoiceLine3.CFIARegistrationNumbers.Count);

			AssertNotNull("Code [SafeFoodForCanadiansLicence] has been added automatically", invoiceLine1.CFIARegistrationNumbers.OfType<CFIARegistrationNumber>().FirstOrDefault(x => x.CY_Code == RegistrationNumberHelper.SafeFoodForCanadiansLicence && x.CY_Data == "54321"));
			AssertNotNull("Code [SafeFoodForCanadiansLicence] has been added automatically", invoiceLine2.CFIARegistrationNumbers.OfType<CFIARegistrationNumber>().FirstOrDefault(x => x.CY_Code == RegistrationNumberHelper.SafeFoodForCanadiansLicence && x.CY_Data == "54321"));
			AssertNotNull("Code [SafeFoodForCanadiansLicence] has been added automatically", invoiceLine3.CFIARegistrationNumbers.OfType<CFIARegistrationNumber>().FirstOrDefault(x => x.CY_Code == RegistrationNumberHelper.SafeFoodForCanadiansLicence && x.CY_Data == "54321"));
		}

		public void TestIsImporterAccountSecurityCodeUsed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsImporterAccountSecurityCodeUsed with no importer", !declaration.IsImporterAccountSecurityCodeUsed);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var importer2 = Factory.New<OrgHeader>();
			importer2.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			declaration.TransactionNumber.AccountSecurityCode = "40000";
			declaration.TransactionNumber.SequentialNumber = "04228";
			Assert("IsImporterAccountSecurityCodeUsed with normal importer", !declaration.IsImporterAccountSecurityCodeUsed);
			declaration.ImporterAddInfo.ZO_AccountSecurityNumber = "50000";
			Assert("IsImporterAccountSecurityCodeUsed with importer code but not used", !declaration.IsImporterAccountSecurityCodeUsed);
			declaration.ImporterAddInfo.ZO_AccountSecurityNumber = "40000";
			Assert("IsImporterAccountSecurityCodeUsed with importer code used", declaration.IsImporterAccountSecurityCodeUsed);
			declaration.ImporterOfRecordAddress.OrganisationPK = importer2.PK;
			declaration.TransactionNumber.AccountSecurityCode = "40000";
			declaration.TransactionNumber.SequentialNumber = "04228";
			declaration.ImporterOfRecordAddInfo.ZO_AccountSecurityNumber = "50000";
			Assert("IsImporterAccountSecurityCodeUsed with importer code but not used", !declaration.IsImporterAccountSecurityCodeUsed);
			declaration.ImporterOfRecordAddInfo.ZO_AccountSecurityNumber = "40000";
			Assert("IsImporterAccountSecurityCodeUsed with importer code used", declaration.IsImporterAccountSecurityCodeUsed);
		}

		public void TestIsAnyInvoiceHasFutureDirectShipmentDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			Assert("IsAnyInvoiceHasFutureDirectShipmentDate", !declaration.IsAnyInvoiceHasFutureDirectShipmentDate);
			invoice.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(2);
			Assert("IsAnyInvoiceHasFutureDirectShipmentDate", declaration.IsAnyInvoiceHasFutureDirectShipmentDate);
		}

		public void TestInvoicingSupporter()
		{
			var testDeclaration = JobDeclaration.New(Factory);
			AssertEquals(typeof(JobDeclaration.JobDeclarationInvoicingSupporter), testDeclaration.InvoicingSupporter.GetType());
		}

		public void TestInvoicingConsigneeDefaultsToOrgProxyForLVSTotalConsolidation()
		{
			var testDeclaration = JobDeclaration.New(Factory);
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			var job = new JobHeader.Loader(testDeclaration).TryCreate();
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, testDeclaration.Job.JH_OA_LocalChargesAddr);

			job.Delete();
			var importer = OrgHeader.New(Factory);
			testDeclaration.JE_OH_Importer = importer.PK;
			job = new JobHeader.Loader(testDeclaration).TryCreate();
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, testDeclaration.Job.JH_OA_LocalChargesAddr);

			job.Delete();
			testDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			job = new JobHeader.Loader(testDeclaration).TryCreate();
			AssertEquals(importer.MainAddress.PK, testDeclaration.Job.JH_OA_LocalChargesAddr);

			job.Delete();
			testDeclaration.JE_OH_Importer = ZGuid.Empty;
			testDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			job = new JobHeader.Loader(testDeclaration).TryCreate();
			AssertEquals(ZGuid.Empty, testDeclaration.Job.JH_OA_LocalChargesAddr);
		}

		public void TestOriginAlwaysNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "CAAAA";

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertNull("Origin always null", declaration.InvoicingSupporter.Origin);
			}
			AssertNull("Origin always null", declaration.InvoicingSupporter.Origin);
		}

		#endregion

		#region AddB3LateSendingWarningEvent

		[TestDate(2022, 7, 1)]
		public void TestCanadianB3LateSendingWarningEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_EntryAuthorisationDate = new DateTime(2022, 5, 13);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 1, 1);
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			Factory.Save();

			var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertNull("New event created because when JE_EntryAuthorisationDate is set ShouldAddB3LateSendingWarningEvent is not fully satisfied", log);

			declaration.JE_EntryAuthorisationDate = new DateTime(2022, 5, 14);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("2 days after Release", new DateTime(2022, 5, 18, 11, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));
		}

		[TestDate(2015, 1, 1)]
		public void TestAddB3LateSendingWarningEventOnSaving_HVS()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 1, 1);
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;
			Assert("Precondition: Is High Value Declaration", !declaration.IsLowValueNormalReleaseJob);

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 5);

			var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("2 days after Release", new DateTime(2015, 1, 7, 11, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));

			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			Assert("Existed event should be cancelled", Factory.Load<StmALog>(log.PK).IsCancelled);
			log = declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CanadianCADLateSendingWarning);
			AssertNull("No Canadian CAD Late Sending Warning event is active", log);

			declaration.ImporterAddInfo.ZO_HVSDelayIntervalTypeFailSafe = DelayIntervalTypeCodes.Codes.DAR;
			declaration.ImporterAddInfo.ZO_HVSDelayIntervalFailSafe = 5;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 7);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("5 days after Release", new DateTime(2015, 1, 14, 11, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));

			declaration.Logs.AddNew(Events.CanadianCADLateWarningSent, ZDateTimeOffset.UtcNow.AddHours(-25));
			Factory.Save();
			log.Cancel();
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 8);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertNull("New event created", log);

			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateWarningSent);
			log.Cancel();
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 9);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("5 days after Release", new DateTime(2015, 1, 16, 11, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));

			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			Assert("Existed event should be cancelled", Factory.Load<StmALog>(log.PK).IsCancelled);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertNull("No Canadian CAD Late Sending Warning event is active", log);
		}

		[TestDate(2015, 1, 1)]
		public void TestAddB3LateSendingWarningEventOnSaving_LVS()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2499m;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2499m;
			Factory.Save();
			Assert("Precondition: Is Low Value Declaration", declaration.IsLowValueNormalReleaseJob);

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 6);
			var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("2 days after Release", new DateTime(2015, 1, 8, 11, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));

			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			Assert("Existed event should be cancelled", Factory.Load<StmALog>(log.PK).IsCancelled);
			log = declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CanadianCADLateSendingWarning);
			AssertNull("No Canadian CAD Late Sending Warning event is active", log);

			declaration.ImporterAddInfo.ZO_HVSDelayIntervalTypeFailSafe = DelayIntervalTypeCodes.Codes.DAR;
			declaration.ImporterAddInfo.ZO_HVSDelayIntervalFailSafe = 5;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 9);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("5 days after Release", new DateTime(2015, 1, 16, 11, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));

			declaration.ImporterAddInfo.ZO_HVSDelayIntervalTypeFailSafe = DelayIntervalTypeCodes.Codes.None;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			Assert("Existed event should be cancelled", Factory.Load<StmALog>(log.PK).IsCancelled);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertNull("No Canadian CAD Late Sending Warning event is active", log);
		}

		[TestDate(2015, 1, 1)]
		public void TestAddB3LateSendingWarningEventOnSaving_CON()
		{
			var declaration = CreateCONJobDeclarationWithB3LateSendingWarningEvent(Factory);

			var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("Next Month's 2nd day", new ZDateTime(2015, 03, 02, 0, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			Assert("Existed event should be cancelled", Factory.Load<StmALog>(log.PK).IsCancelled);
			log = declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CanadianCADLateSendingWarning);
			AssertNull("No Canadian CAD Late Sending Warning event is active", log);

			declaration.ImporterAddInfo.ZO_CONDelayIntervalTypeFailSafe = DelayIntervalTypeCodes.Codes.DAY;
			declaration.ImporterAddInfo.ZO_CONDelayIntervalFailSafe = 5;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 2, 7);
			log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertEquals("Next Month's 5th day", new DateTime(2015, 3, 5, 00, 0, 0), EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", log.SL_EventTime.ToDateTime()));
		}

		[TestDate(2015, 1, 1)]
		public void TestAddB3LateSendingWarningEventWithEventTimeInTimezone()
		{
			var melbourneBranch = Factory.New<GlbBranch>();
			melbourneBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			melbourneBranch.GB_Code = "TML";
			melbourneBranch.GB_RL_NKHomePort = "AUMEL";

			var perthBranch = Factory.New<GlbBranch>();
			perthBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			perthBranch.GB_Code = "TPR";
			perthBranch.GB_RL_NKHomePort = "AUPER";
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 1, 1);
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 6000m;

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 5);

			using (DisposableEnvironment.ForBranch(melbourneBranch.PK.ToGuid()))
			{
				var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
				AssertEquals("2 days after Release", new DateTime(2015, 1, 7, 11, 0, 0), log.SL_EventTime);
				AssertEquals("B3LateSendingWarning event log should not cancel", false, log.IsCancelled);
			}

			using (DisposableEnvironment.ForBranch(perthBranch.PK.ToGuid()))
			{
				declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 5);
				Factory.Save();

				var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
				AssertEquals("2 days after Release", new DateTime(2015, 1, 7, 11, 0, 0), log.SL_EventTime);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmALog));
				query.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.CanadianCADLateSendingWarningCode);

				var result = Factory.Load<StmALog>(query);
				AssertEquals("If a log existed on the same job, it should be updated not cancelled cause Event time is the same.", 1, result.Length);
				AssertEquals("B3LateSendingWarning event log should not be changed", false, result[0].IsCancelled);
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestDoNotCreateB3LateSendingWarningEvent_WhenCA_CSAEntryIsTrue()
		{
			var melbourneBranch = Factory.New<GlbBranch>();
			melbourneBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			melbourneBranch.GB_Code = "TML";
			melbourneBranch.GB_RL_NKHomePort = "AUMEL";

			var perthBranch = Factory.New<GlbBranch>();
			perthBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			perthBranch.GB_Code = "TPR";
			perthBranch.GB_RL_NKHomePort = "AUPER";
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 5);
			declaration.CA_CSAEntry = true;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 1, 1);
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 6000m;

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			Factory.Save();

			using (DisposableEnvironment.ForBranch(melbourneBranch.PK.ToGuid()))
			{
				var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
				AssertNull(log);
			}
		}

		public static JobDeclaration CreateCONJobDeclarationWithB3LateSendingWarningEvent(BusinessObjectFactory factory)
		{
			var importer = factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.CA_LVSCloseDate = new ZDateTime(2015, 1, 1);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			Assert("Precondition:Is Consolidated LVS", declaration.IsLVS);

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			delayFactorRegistryBO.CONDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(declaration.JE_GC.ToGuid(), Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 2, 5);

			return declaration;
		}

		[TestDate(2015, 1, 1)]
		public void TestB3LateSendingWarningEventOnSettingK84Date()
		{
			AssertB3LateSendingWarningEventOnSettingMessageType(x => x.CA_K84AccountingDate = new ZDateTime(2015, 10, 25), x => x.CA_K84AccountingDate = ZDateTime.Empty);
		}

		[TestDate(2015, 1, 1)]
		public void TestB3LateSendingWarningEventOnSettingMessageType()
		{
			void ResetImpdeclaration(JobDeclaration declaration)
			{
				var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
				delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
				delayFactorRegistryBO.HVSDelayInterval = 2;
				CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryAuthorisationDate = new DateTime(2015, 2, 5);
			}
			AssertB3LateSendingWarningEventOnSettingMessageType(x => x.JE_MessageType = JobMessageTypeList.Codes.Export, x => ResetImpdeclaration(x));
		}

		[TestDate(2015, 1, 1)]
		public void TestB3LateSendingWarningEventOnSettingMessageSubType()
		{
			AssertB3LateSendingWarningEventOnSettingMessageType(x => x.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveP, x => x.JE_MessageSubType = B3EntryTypeList.Codes.Confirming);
		}

		void AssertB3LateSendingWarningEventOnSettingMessageType(Action<JobDeclaration> inValidSetter, Action<JobDeclaration> validSetter)
		{
			var declaration = CreateCONJobDeclarationWithB3LateSendingWarningEvent(Factory);
			var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			Assert("CanadianB3LateSendingWarning event log", !log.IsCancelled);
			inValidSetter(declaration);

			Assert("CanadianB3LateSendingWarning event log should be cancelled", log.IsCancelled);
			validSetter(declaration);
			var newLog = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			AssertNotEquals(log.PK, newLog.PK);
			Assert("new event logged", !newLog.IsCancelled);
		}

		#endregion

		#region TestDefaultValueforDutyCodeFromSupplierImporterLink

		public void TestDefaultValueforDutyCodeFromSupplierImporterLink_ImporterChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			RefUNLOCO currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode));
			ZQuery otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);

			OrgHeader consignee1 = OrgHeader.New(Factory);
			consignee1.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			OrgHeader consignor = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link1 = consignee1.SupplierLinks.AddNew(consignor);
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link1.OL_ValuationBasis = "13";

			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee1.PK;
			AssertEquals("Precondition:", link1, declaration.SupplierImporterLink);
			AssertEquals("ValueForDutyCode should default from SupplierImporterLink", "13", invoice1.CA_ValueForDutyCode);

			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals("ValueForDutyCode should default from SupplierImporterLink", "13", invoice2.CA_ValueForDutyCode);

			OrgHeader consignee2 = OrgHeader.New(Factory);
			consignee2.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			OrgSupplierBuyerLink link2 = consignee2.SupplierLinks.AddNew(consignor);
			link2.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link2.OL_ValuationBasis = "23";
			invoice2.CA_ValueForDutyCode = ZString.Empty;
			declaration.JE_OH_Importer = consignee2.PK;
			AssertEquals("Precondition:", link2, declaration.SupplierImporterLink);
			AssertEquals("ValueForDutyCode should not default from SupplierImporterLink", "13", invoice1.CA_ValueForDutyCode);
			AssertEquals("ValueForDutyCode should default from SupplierImporterLink", "23", invoice2.CA_ValueForDutyCode);
		}

		public void TestDefaultValueforDutyCodeFromSupplierImporterLink_SupplierChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			RefUNLOCO currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode));
			ZQuery otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);

			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			OrgHeader consignor1 = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link1 = consignee.SupplierLinks.AddNew(consignor1);
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link1.OL_ValuationBasis = "13";

			declaration.JE_OH_Supplier = consignor1.PK;
			declaration.JE_OH_Importer = consignee.PK;
			AssertEquals("Precondition:", link1, declaration.SupplierImporterLink);
			AssertEquals("ValueForDutyCode should default from SupplierImporterLink", "13", invoice1.CA_ValueForDutyCode);

			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals("ValueForDutyCode should default from SupplierImporterLink", "13", invoice2.CA_ValueForDutyCode);

			OrgHeader consignor2 = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link2 = consignee.SupplierLinks.AddNew(consignor2);
			link2.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link2.OL_ValuationBasis = "23";
			invoice2.CA_ValueForDutyCode = ZString.Empty;
			declaration.JE_OH_Supplier = consignor2.PK;
			AssertEquals("Precondition:", link2, declaration.SupplierImporterLink);
			AssertEquals("ValueForDutyCode should not default from SupplierImporterLink", "13", invoice1.CA_ValueForDutyCode);
			AssertEquals("ValueForDutyCode should default from SupplierImporterLink", "23", invoice2.CA_ValueForDutyCode);
		}

		#endregion

		#region Test K84 Data

		public void TestK84DataSource()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00123456";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.CA_K84AccountingDate = new ZDateTime(2015, 1, 29);
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_EntryFilerCode = "12345";
			statement.B2_AccountNo = "123456789";
			statement.B2_ProcessDate = new ZDateTime(2019, 2, 28);
			statement.B2_StatementNumber = string.Empty;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_BrokerReference = "B00123456,12345000067897";
			statementLine.B3_EntryNum = "12345000067897";
			statementLine.B3_EntryType = "B3";
			statementLine.B3_Status = "X";
			statementLine.B3_ImporterCustomsID = "12345000RM001";

			Factory.Save();

			AssertEquals(0m, declaration.CA_TotalDutyAmount);
			AssertEquals(0, declaration.k84DataSource.Length);

			statement.B2_StatementNumber = "0112345852";
			Factory.Save();

			declaration.k84DataSource = null;

			AssertEquals(0m, declaration.CA_TotalDutyAmount);
			AssertEquals(0, declaration.k84DataSource.Length);

			statementLine.B3_BrokerReference = "B00123456";
			Factory.Save();

			declaration.k84DataSource = null;

			AssertEquals(0m, declaration.CA_TotalDutyAmount);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { statementLine.PK }, declaration.k84DataSource.Select(x => x.PK));
		}

		public void TestK84DataSourceFromDailyNoticeMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00123456";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "10207";
			declaration.TransactionNumber.SequentialNumber = "00350299";
			declaration.CA_K84AccountingDate = new ZDateTime(2015, 1, 29);
			AssertEquals(0m, declaration.CA_TotalDutyAmount);
			AssertEquals(0, declaration.k84DataSource.Length);

			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "DN-178234732-220825");
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.Broker);
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
			var statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
			AssertNull(statementHeader);
			Factory.Save();

			var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeMessageText();

			var message = Factory.New<CARMDailyNoticeMessage>();
			message.EM_MessageText = text;
			message.EM_MessageSubType = CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker;
			var cacMessageProcessor = new CACustomsMessageProcessor(new LoggingInformation());
			AssertNoExceptionThrown(() =>
			{
				cacMessageProcessor.ProcessMessage(message);
			});
			statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
			AssertNotNull(statementHeader);
			AssertEquals("10207", statementHeader.B2_EntryFilerCode);
			var statementLine = (CusStatementLine)statementHeader.StatementLines.FirstOrDefault();
			Factory.Save();

			declaration.k84DataSource = null;

			AssertEquals(100000m, declaration.CA_TotalDutyAmount);
			AssertEquals(10000m, declaration.CA_TotalGSTAmount);
			AssertEquals(1.1m, declaration.CA_TotalExciseTaxAmount);
			AssertEquals(1.3m, declaration.CA_TotalSIMAAmount);
			AssertEquals(110002.4m, declaration.CA_TotalDutyAndTaxAmount);
			AssertEquals(0.1m, declaration.CA_ARLOthersAmount);
			AssertEquals(110002.5m, declaration.CA_TotalIncludingPenalty);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { statementHeader.StatementLines.FirstOrDefault().PK }, declaration.k84DataSource.Select(x => x.PK));
		}

		public void TestK84DataFieldsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Read Only", declaration.CA_K84StatementDateInfo.ReadOnly);
			Assert("Read Only", declaration.CA_ReleaseOfficeInfo.ReadOnly);
		}

		#endregion

		#region Test B3 ScheduleSending Related

		public void TestHasScheduledB3Message()
		{
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("checkpoint_1", !testDeclaration.HasScheduledB3Message);

				var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
				Assert("checkpoint_2", !testDeclaration.HasScheduledB3Message);

				testHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				Assert("checkpoint_3", !testDeclaration.HasScheduledB3Message);

				var testMessage = testHeader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_IsActive = true;
				Assert("checkpoint_4", !testDeclaration.HasScheduledB3Message);

				testMessage.EM_HeldUntilDate = ZDateTime.UtcNow;
				Assert("checkpoint_5", testDeclaration.HasScheduledB3Message);

				testMessage.EM_IsActive = false;
				Assert("checkpoint_6", !testDeclaration.HasScheduledB3Message);

				testMessage.EM_IsActive = true;
				testMessage.EM_Status = EDIMessage.Status.Cancelled;
				Assert("checkpoint_7", !testDeclaration.HasScheduledB3Message);

				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Assert("checkpoint_8", !testDeclaration.HasScheduledB3Message);

				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				Assert("checkpoint_9", testDeclaration.HasScheduledB3Message);

				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				Assert("checkpoint_10", testDeclaration.HasScheduledB3Message);

				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("checkpoint_11", !testDeclaration.HasScheduledB3Message);
			});
		}

		public void TestScheduledB3MessageTime()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			testCompany.GC_OH_OrgProxy = testOrg.PK;
			var testBranch = testCompany.Branches.AddNew();
			testBranch.FillWithValidTestData();
			testBranch.GB_RL_NKHomePort = "CNSHA";
			Factory.Save();
			using (DisposableEnvironment.ForBranch(testBranch.PK.ToGuid()))
			{
				CombineAssertions(() =>
				{
					var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
					testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("checkpoint_1", ZDateTime.Empty, testDeclaration.ScheduledB3MessageTime);

					var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
					AssertEquals("checkpoint_2", ZDateTime.Empty, testDeclaration.ScheduledB3MessageTime);

					testHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
					AssertEquals("checkpoint_3", ZDateTime.Empty, testDeclaration.ScheduledB3MessageTime);

					var testMessage = testHeader.Messages.AddNew();
					testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
					testMessage.EM_Status = EDIMessage.Status.Queued;
					testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					testMessage.EM_IsActive = true;
					AssertEquals("checkpoint_4", ZDateTime.Empty, testDeclaration.ScheduledB3MessageTime);

					var testTime = new ZDateTime(2015, 05, 01, 02, 00, 00);
					testMessage.EM_HeldUntilDate = testTime;
					AssertEquals("checkpoint_5", (new ZDateTime(2015, 05, 01, 10, 00, 00)), testDeclaration.ScheduledB3MessageTime);

					testMessage.EM_IsActive = false;
					AssertEquals("checkpoint_6", ZDateTime.Empty, testDeclaration.ScheduledB3MessageTime);

					testMessage.EM_IsActive = true;
					testMessage.EM_Status = EDIMessage.Status.Cancelled;
					AssertEquals("checkpoint_7", ZDateTime.Empty, testDeclaration.ScheduledB3MessageTime);

					testMessage.EM_Status = EDIMessage.Status.Queued;
					testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					AssertEquals("checkpoint_8", ZDateTime.Empty, testDeclaration.ScheduledB3MessageTime);

					testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					AssertEquals("checkpoint_9", new ZDateTime(2015, 05, 01, 10, 00, 00), testDeclaration.ScheduledB3MessageTime);

					testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
					AssertEquals("checkpoint_10", new ZDateTime(2015, 05, 01, 10, 00, 00), testDeclaration.ScheduledB3MessageTime);

					testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("checkpoint_11", new ZDateTime(2015, 05, 01, 10, 00, 00), testDeclaration.ScheduledB3MessageTime);
				});
			}
		}

		public void TestSettingK83AccountingDateClearScheduledB3Message()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
			testHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var testMessage1 = testHeader.Messages.AddNew();
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage1.EM_IsActive = true;

			var testMessage2 = testHeader.Messages.AddNew();
			testMessage2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_HeldUntilDate = ZDateTime.UtcNow;
			testMessage2.EM_IsActive = true;

			var testMessage3 = testHeader.Messages.AddNew();
			testMessage3.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			testMessage3.EM_Status = EDIMessage.Status.Queued;
			testMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage3.EM_HeldUntilDate = ZDateTime.UtcNow;
			testMessage3.EM_IsActive = true;

			var testHeader2 = testDeclaration.ActiveEntryHeaders.AddNew();
			testHeader2.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var testMessage4 = testHeader2.Messages.AddNew();
			testMessage4.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage4.EM_Status = EDIMessage.Status.Queued;
			testMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage4.EM_HeldUntilDate = ZDateTime.UtcNow;
			testMessage4.EM_IsActive = true;
			Factory.Save();

			testDeclaration.CA_K84AccountingDate = ZDateTime.Now;

			CombineAssertions("Setting Accounting Date will clear Scheduled Message", () =>
			{
				AssertEquals("Check Status_1", EDIMessage.Status.Queued, testMessage1.EM_Status);
				AssertEquals("Check Status_2", EDIMessage.Status.Cancelled, testMessage2.EM_Status);
				AssertEquals("Check Status_3", EDIMessage.Status.Queued, testMessage3.EM_Status);
				AssertEquals("Check Status_4", EDIMessage.Status.Queued, testMessage4.EM_Status);

				AssertEquals("Check Active_1", true, testMessage1.EM_IsActive);
				AssertEquals("Check Active_2", false, testMessage2.EM_IsActive);
				AssertEquals("Check Active_3", true, testMessage3.EM_IsActive);
				AssertEquals("Check Active_4", true, testMessage4.EM_IsActive);
			});

			var testMessage5 = testHeader.Messages.AddNew();
			testMessage5.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage5.EM_Status = EDIMessage.Status.Queued;
			testMessage5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage5.EM_HeldUntilDate = ZDateTime.UtcNow;
			testMessage5.EM_IsActive = true;
			Factory.Save();

			testDeclaration.CA_K84AccountingDate = ZDateTime.Empty;

			CombineAssertions("Setting Accounting Date to Empty will not clear Scheduled Message", () =>
			{
				AssertEquals("Check Status_1", EDIMessage.Status.Queued, testMessage1.EM_Status);
				AssertEquals("Check Status_2", EDIMessage.Status.Cancelled, testMessage2.EM_Status);
				AssertEquals("Check Status_3", EDIMessage.Status.Queued, testMessage3.EM_Status);
				AssertEquals("Check Status_4", EDIMessage.Status.Queued, testMessage4.EM_Status);
				AssertEquals("Check Status_5", EDIMessage.Status.Queued, testMessage5.EM_Status);

				AssertEquals("Check Active_1", true, testMessage1.EM_IsActive);
				AssertEquals("Check Active_2", false, testMessage2.EM_IsActive);
				AssertEquals("Check Active_3", true, testMessage3.EM_IsActive);
				AssertEquals("Check Active_4", true, testMessage4.EM_IsActive);
				AssertEquals("Check Active_5", true, testMessage5.EM_IsActive);
			});
		}

		#endregion

		#region Test Auto B3 Sending

		public void TestScheduledAutoSendingB3Date_HVS()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_B3AutoSend = true;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 1, 1);
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;
			Assert("Precondition: Is High Value Declaration", !declaration.IsLowValueNormalReleaseJob);

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("2 days after Release", new DateTime(2015, 5, 15, 12, 0, 0), declaration.ScheduledB3AutoSendingDate);

			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);

			declaration.ImporterAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			declaration.ImporterAddInfo.ZO_HVSDelayIntervalAutoSend = 5;
			AssertEquals("5 days after Release", new DateTime(2015, 5, 21, 12, 0, 0), declaration.ScheduledB3AutoSendingDate);

			declaration.ImporterAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
		}

		public void TestScheduledAutoSendingB3Date_LVS()
		{
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_B3AutoSend = true;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2499m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2499m;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			var line = entryHeader.MergedLines.AddNew();
			Factory.Save();
			Assert("Precondition: Is Low Value Declaration", declaration.IsLowValueNormalReleaseJob);

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("2 days after Release", new DateTime(2015, 5, 15, 12, 0, 0), declaration.ScheduledB3AutoSendingDate);

			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);

			declaration.ImporterAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			declaration.ImporterAddInfo.ZO_HVSDelayIntervalAutoSend = 5;
			AssertEquals("5 days after Release", new DateTime(2015, 5, 21, 12, 0, 0), declaration.ScheduledB3AutoSendingDate);

			declaration.ImporterAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
		}

		public void TestScheduledAutoSendingB3Date_CONMSI()
		{
			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.CA_B3AutoSend = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
			declaration.CA_LVSCloseDate = new DateTime(2015, 5, 15, 12, 0, 0);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			Assert("Precondition:Is Consolidated LVS", declaration.IsLVS);

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			delayFactorRegistryBO.CONDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("2 days after Close", new ZDateTime(2015, 6, 02, 0, 0, 0), declaration.ScheduledB3AutoSendingDate);

			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);

			declaration.ImporterAddInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAY;
			declaration.ImporterAddInfo.ZO_CONDelayIntervalAutoSend = 5;
			AssertEquals("5 days after Release", new DateTime(2015, 6, 5, 0, 0, 0), declaration.ScheduledB3AutoSendingDate);

			declaration.ImporterAddInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
		}

		public void TestScheduledAutoSendingB3Date_CONVAR()
		{
			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXXXX";
			importer.OH_RL_NKClosestPort = "AUBNE";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.CA_B3AutoSend = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
			declaration.CA_LVSCloseDate = new DateTime(2015, 5, 15, 12, 0, 0);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			Assert("Precondition:Is Consolidated LVS", declaration.IsLVS);

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			delayFactorRegistryBO.CONDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("2 days after Close", new ZDateTime(2015, 6, 2, 0, 0, 0), declaration.ScheduledB3AutoSendingDate);

			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.None;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);

			declaration.ImporterAddInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("2 days after Close", new ZDateTime(2015, 6, 2, 0, 0, 0), declaration.ScheduledB3AutoSendingDate);
		}

		[ExpectNoExceptions]
		public void TestScheduledAutoSendingB3Date_NoDate()
		{
			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.CA_B3AutoSend = true;
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			delayFactorRegistryBO.CONDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
			AssertEquals("no date", new ZDateTime(2015, 6, 2, 0, 0, 0), declaration.ScheduledB3AutoSendingDate);
			declaration.CA_LVSCloseDate = new DateTime(2015, 5, 15, 12, 0, 0);
			AssertNotEquals("2 days after Close", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
			declaration.CA_B3AutoSend = false;
			AssertEquals("no date", ZDateTime.Empty, declaration.ScheduledB3AutoSendingDate);
		}

		public void TestCA_B3AutoSend()
		{
			using (CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				Assert("CA_B3AutoSend should not be set", !declaration.CA_B3AutoSend);
				AssertNotContains("CA_B3AutoSend should not be set", "CA_B3AutoSend", declaration.JE_AddInfo);
			}
			using (CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("CA_B3AutoSend should be set", declaration.CA_B3AutoSend);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("CA_B3AutoSend should not be set", !declaration.CA_B3AutoSend);
				Factory.Save();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				Assert("CA_B3AutoSend should be set", declaration.CA_B3AutoSend);
			}
		}

		#endregion

		public void TestJZ_NetWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_NetWeight = 10m;
			AssertEquals("JZ_NetWeight", 10m, invoice.JZ_NetWeight);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JZ_NetWeight should be zero", 0m, invoice.JZ_NetWeight);
		}

		protected override void CreatePartAndClassification(string partNum, string tariff, OrgHeader importer)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.RelatedOrganisations.AddOwner(importer);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypes.Codes.HTE;
			pivot.CI_OP = part.PK;
			pivot.CI_TariffNum = tariff;
		}

		[ExpectNoExceptions]
		public void TestCommercialInvoiceOriginatorIsDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Test Adress 1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.CommercialInvoiceOriginator.OrganisationPK = org.PK;
			var adress = new AddressFormatter(Factory, declaration.CommercialInvoiceOriginator, GlbCompany.CurrentCompany, false).PostalAddress();
			AssertEquals("TEST ADRESS 1", adress);

			declaration.CommercialInvoiceOriginator.Delete();
			adress = new AddressFormatter(Factory, declaration.CommercialInvoiceOriginator, GlbCompany.CurrentCompany, false).PostalAddress();
			AssertEquals(ZString.Empty, adress);
			Assert(!declaration.CommercialInvoiceOriginator.IsDeleted);
		}

		#region ValidateCA_AuthorityNumberAfterMerging

		public void TestValidateCA_AuthorityNumberAfterMerging()
		{
			var warning = "The total Value for Duty of this job is less than the minimum but you have not entered an OIC regular remission.";
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1m;
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1m;

			declaration.DoMerge();
			AssertEquals("Precondition: total Value for Duty", 2m, declaration.TotalCustomsValueInLocalCurrency);
			AssertHasMessageError(invoiceLine1.CA_AuthorityNumberInfo, warning);
			AssertHasMessageError(invoiceLine2.CA_AuthorityNumberInfo, warning);

			invoiceLine1.JI_LinePrice = 1000m;
			declaration.DoMerge();
			AssertEquals("Precondition: total Value for Duty", 1001m, declaration.TotalCustomsValueInLocalCurrency);
			AssertNoMessageError(invoiceLine1.CA_AuthorityNumberInfo, warning);
			AssertNoMessageError(invoiceLine2.CA_AuthorityNumberInfo, warning);
		}

		#endregion

		public void TestJE_CustomsOfficeNoException()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "0701";
			AssertNoExceptionThrown(() => declaration.JE_CustomsOffice = "AABBB");
		}

		public void TestConvertLVXToNormalDeclaration()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "4646";
			carrier1.ZZ4_Description = "Carrier Name 1";
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier1.ZZ4_IsAir = true;
			carrier1.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);

			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "5656";
			carrier2.ZZ4_Description = "Carrier Name 2";
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier2.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			declaration.CA_AccountingAge = 10;
			var invoice = declaration.LVXInvoiceHeader;
			invoice.CA_CarrierCode = "4646";
			invoice.CA_OtherReference = "11\r\n22\r\n33";

			declaration.ConvertLVXToNormalDeclaration();
			AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("JE_MessageSubType", B3EntryTypeList.Codes.Confirming, declaration.JE_MessageSubType);
			AssertEquals("JE_TransportMode", Constants.TransportModes.Air, declaration.JE_TransportMode);
			AssertEquals("JE_CarrierCode", "4646", declaration.JE_CarrierCode);
			AssertEquals("JE_OwnerRef", "11,22,33", declaration.JE_OwnerRef);
			AssertEquals("CA_AccountingAge", 0, declaration.CA_AccountingAge);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.LVXInvoiceHeader.CA_CarrierCode = "5656";

			declaration.ConvertLVXToNormalDeclaration();
			AssertEquals("JE_CarrierCode", "5656", declaration.JE_CarrierCode);
			AssertEquals("JE_TransportMode", Constants.TransportModes.Road, declaration.JE_TransportMode);
		}

		public void TestSetupNonPersistentProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertIsEnableACROSSB3Validation(declaration, false, false, true, true, false);
			AssertIsEnableACROSSB3Validation(declaration, false, false, false, false, true);
			AssertIsEnableACROSSB3Validation(declaration, true, false, true, true, false);
			AssertIsEnableACROSSB3Validation(declaration, true, false, false, true, true);
			AssertIsEnableACROSSB3Validation(declaration, false, true, true, true, true);
			AssertIsEnableACROSSB3Validation(declaration, false, true, false, false, true);
			AssertIsEnableACROSSB3Validation(declaration, true, true, true, true, true);
			AssertIsEnableACROSSB3Validation(declaration, true, true, false, true, true);
		}

		void AssertIsEnableACROSSB3Validation(JobDeclaration declaration, ZBool isAlwaysEnableACROSSMessageValidation, ZBool isAlwaysEnableB3MessageValidation, ZBool isEmptyEntryAuthorisationDate, ZBool expectedACROSSValue, ZBool expectedB3Value)
		{
			CACustomsDataRegistry.Instance.AlwaysEnableACROSSMessageValidation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, isAlwaysEnableACROSSMessageValidation);
			CACustomsDataRegistry.Instance.AlwaysEnableB3MessageValidation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, isAlwaysEnableB3MessageValidation);
			declaration.JE_EntryAuthorisationDate = isEmptyEntryAuthorisationDate ? ZDate.Empty : ZDate.Today;
			Factory.Save();
			var reloadDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("IsEnableACROSSValidation", expectedACROSSValue, reloadDeclaration.IsEnableACROSSValidation);
			AssertEquals("IsEnableB3Validation", expectedB3Value, reloadDeclaration.IsEnableB3Validation);
		}

		[TestDate(2025, 5, 28)]
		public void TestJE_EntryAuthorisationDate()
		{
			CACustomsDataRegistry.Instance.AlwaysEnableACROSSMessageValidation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
			CACustomsDataRegistry.Instance.AlwaysEnableB3MessageValidation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDate.Today;
			Assert("IsEnableACROSSValidation", !declaration.IsEnableACROSSValidation);
			Assert("IsEnableB3Validation", declaration.IsEnableB3Validation);
			AssertEquals("CA_AccountingAge", 0, declaration.CA_AccountingAge);
			declaration.JE_EntryAuthorisationDate = ZDate.Empty;
			Assert("IsEnableACROSSValidation", declaration.IsEnableACROSSValidation);
			Assert("IsEnableB3Validation", !declaration.IsEnableB3Validation);
			AssertEquals("CA_AccountingAge", 0, declaration.CA_AccountingAge);
			declaration.JE_EntryAuthorisationDate = ZDate.Today.AddDays(-10);
			AssertEquals("CA_AccountingAge", 6, declaration.CA_AccountingAge);
		}

		public void TestJE_EntryAuthorisationDateForIM2()
		{
			CACustomsDataRegistry.Instance.AlwaysEnableACROSSMessageValidation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = ZDate.Today;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			Assert("IsEnableACROSSValidation", !declaration.IsEnableACROSSValidation);
			declaration.JE_EntryAuthorisationDate = ZDate.Empty;
			Assert("IsEnableACROSSValidation", !declaration.IsEnableACROSSValidation);
			declaration.JE_EntryAuthorisationDate = ZDate.Today;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDate.Empty;
			Assert("IsEnableACROSSValidation", declaration.IsEnableACROSSValidation);
		}

		public void TestConsolidatedAVSStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert("Precondition: AVSEventRequired", !declaration.AVSEventRequired);
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("Precondition: CA_OGDStatus", AVSStatusList.Codes.Blank, invoiceLine.CA_OGDStatus);
			var tariffNo = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine.JI_Tariff = tariffNo;
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.NotValidated, declaration.CA_OGDStatus);
			Assert("AVSEventRequired", !declaration.AVSEventRequired);
			var log1 = declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.ServiceRequested);
			AssertNotNull("A QRQ event should be posted", log1);

			invoiceLine.Delete();
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.Blank, declaration.CA_OGDStatus);
			var log2 = declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.ServiceRequested);
			AssertNotNull("A QRQ event should be posted", log2);
			AssertNotEquals("A QRQ event should be posted", log1, log2);

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.Blank, declaration.CA_OGDStatus);

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.NotValidated, declaration.CA_OGDStatus);

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.ReviewRequired;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.ReviewRequired, declaration.CA_OGDStatus);
			AssertNotNull("A QOK event should be posted", declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.ServiceCompleted));

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.InspectionRequired;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.InspectionRequired, declaration.CA_OGDStatus);

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.NotImport;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.NotImport, declaration.CA_OGDStatus);

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Rejected;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.Rejected, declaration.CA_OGDStatus);

			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.CA_OGDStatus = AVSStatusList.Codes.Blank);
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.NoActionRequired;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.WillBeApproved, declaration.CA_OGDStatus);

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.WillBeApproved, declaration.CA_OGDStatus);

			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Unknown;
			Factory.Save();
			AssertEquals("declaration.CA_OGDStatus", AVSStatusList.Codes.Unknown, declaration.CA_OGDStatus);
		}

		public void TestSuppressShipmentRelatedFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Assert(!declaration.SuppressShipmentRelatedFields);

			declaration.JE_JS = ZGuid.Empty;

			CACustomsDataRegistry.Instance.SuppressShipmentRelatedFields.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false);
			Assert(!declaration.SuppressShipmentRelatedFields);

			CACustomsDataRegistry.Instance.SuppressShipmentRelatedFields.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true);
			Assert(declaration.SuppressShipmentRelatedFields);

			declaration.SuppressShipmentRelatedFields = false;
			Assert(!declaration.SuppressShipmentRelatedFields);
			Assert(CACustomsDataRegistry.Instance.SuppressShipmentRelatedFields.Value);
		}

		public void TestJE_RL_NKPortOfFirstArrival()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(string.Empty, declaration.JE_RL_NKPortOfFirstArrival);

			declaration.JE_RL_NKPortOfArrival = "CNSHA";
			AssertEquals("CNSHA", declaration.JE_RL_NKPortOfFirstArrival);
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfFirstArrival);
		}

		public void TestSupplierDocumentaryAddressChanged()
		{
			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor1MainAddress = consignor1.Addresses.MainAddress;
			var consignorAddress1 = consignor1.Addresses.AddNew();
			var consignorAddress2 = consignor1.Addresses.AddNew();
			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor2MainAddress = consignor2.Addresses.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = consignor1.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress1.PK;
			var invoice1 = declaration.Invoices.AddNew();
			AssertEquals("Precondition:", consignor1.PK, invoice1.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Precondition:", consignorAddress1.PK, invoice1.SupplierDocumentaryAddress.E2_OA_Address);
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress2.PK;

			declaration.JE_OH_Supplier = consignor2.PK;
			AssertEquals("Should be changed to the new vendor", consignor2.PK, invoice1.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Should not be changed to the new vendor address", consignor2MainAddress.PK, invoice1.SupplierDocumentaryAddress.E2_OA_Address);
			AssertEquals("Should be changed to the new vendor", consignor1.PK, invoice2.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Should not be changed to the new vendor address", consignorAddress2.PK, invoice2.SupplierDocumentaryAddress.E2_OA_Address);

			declaration.SupplierDocumentaryAddress.OrganisationPK = consignor1.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = consignor1MainAddress.PK;
			AssertEquals("Should be changed to the new vendor", consignor1.PK, declaration.JE_OH_Supplier);
			AssertEquals("Should be changed to the new vendor", consignor1.PK, invoice1.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Should be changed to the new vendor address", consignor1MainAddress.PK, invoice1.SupplierDocumentaryAddress.E2_OA_Address);
			AssertEquals("Should not be changed to the new vendor address", consignorAddress2.PK, invoice2.SupplierDocumentaryAddress.E2_OA_Address);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			AssertNotEquals("Should not be changed to the new vendor", declaration.SupplierDocumentaryAddress.OrganisationPK, invoice1.SupplierDocumentaryAddress.OrganisationPK);
			AssertNotEquals("Should not be changed to the new vendor address", declaration.SupplierDocumentaryAddress.E2_OA_Address, invoice1.SupplierDocumentaryAddress.E2_OA_Address);
		}

		public void TestResetValuesOnInvoiceForTemplateCopy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.CA_OtherReference = "Other Ref";
			invoice.CA_ReadyForConsolidation = true;
			var declarationCopy = declaration.TemplateCopy();
			AssertEquals("JobDeclaration", typeof(JobDeclaration), declarationCopy.GetType());
			AssertEquals("1 Invoice", 1, ((JobDeclaration)declarationCopy).Invoices.Count);
			AssertEquals("Not copy other reference", ZString.Empty, ((JobDeclaration)declarationCopy).Invoices[0].CA_OtherReference);
			AssertEquals("Not copy Ready for Consolidation", ZBool.False, ((JobDeclaration)declarationCopy).Invoices[0].CA_ReadyForConsolidation);
		}

		public void TestResetAVSStatusAfterTemplateCopy()
		{
			var sourceDecl = Factory.New<JobDeclaration>();
			sourceDecl.JE_MessageType = JobMessageTypeList.Codes.Import;
			sourceDecl.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			var invoiceLine1 = (JobComInvoiceLine)sourceDecl.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine1.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			var invoiceLine2 = (JobComInvoiceLine)sourceDecl.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();

			var clonedDecl = (JobDeclaration)sourceDecl.TemplateCopy();
			AssertEquals("declaration.CA_OGDStatus", ZString.Empty, clonedDecl.CA_OGDStatus);
			Assert("declaration.AVSEventRequired", clonedDecl.AVSEventRequired);
			var clonedLine1 = clonedDecl.InvoiceLines[0];
			var clonedLine2 = clonedDecl.InvoiceLines[1];
			AssertEquals("clonedLine1.CA_OGDStatus", AVSStatusList.Codes.NotValidated, clonedLine1.CA_OGDStatus);
			AssertEquals("clonedLine2.CA_OGDStatus", AVSStatusList.Codes.Blank, clonedLine2.CA_OGDStatus);
		}

		public override void TestSetDefaultPackagesType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("default value for JE_TotalNoOfPacksPackType", "PKG", declaration.JE_TotalNoOfPacksPackType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("JE_TotalNoOfPacksPackType should be clear for LVS", "", declaration.JE_TotalNoOfPacksPackType);
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("JE_TotalNoOfPacksPackType should be clear for LVX", "", declaration.JE_TotalNoOfPacksPackType);
		}

		public void TestNoticesMessagesForDisplay()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "S001", "Positive Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "S002", "Negative Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = entry;
			message1.EM_MessageSubType = MessageTypeList.Codes.ACIHouseBill;
			message1.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+70SX020418
DTM+9:201606030000:203
RFF+AGO:123029118RM0003
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1".Replace("\r\n", "'");

			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, new ZDateTime(2016, 6, 4));
			UniversalEventMessageTest.LinkToBusinessObject(message2, entry);
			message2.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message3 = Factory.New<UniversalEventMessage>();
			message3.EM_MessageText = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAeManifestStatusNotice</Type>
					<Key>12345000000012</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2016-06-05T00:00:00</EventTime>
		<EventType>CMS</EventType>
		<ContextCollection>
			<Context>
				<Type>OrganizationReference</Type>
				<Value>SECONDARY BUSINESS ID</Value>
			</Context>
			<Context>
				<Type>Status</Type>
				<Value>S001</Value>
			</Context>
			<Context>
				<Type>Status</Type>
				<Value>S002</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			UniversalEventMessageTest.LinkToBusinessObject(message3, entry);
			message3.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message4 = Factory.New<UniversalEventMessage>();
			message4.EM_MessageText = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAIntegratedImportDeclaration</Type>
					<Key>12345000000012</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2016-06-05T00:00:00</EventTime>
		<EventType>CMS</EventType>
		<ContextCollection>
			<Context>
				<Type>OrganizationReference</Type>
				<Value>SECONDARY BUSINESS ID</Value>
			</Context>
			<Context>
				<Type>Status</Type>
				<Value>S001</Value>
			</Context>
			<Context>
				<Type>Status</Type>
				<Value>S002</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			UniversalEventMessageTest.LinkToBusinessObject(message4, entry);
			var genAddOnColumn = Factory.New<GenAddOnColumn>();
			genAddOnColumn.XA_Name = "XMLCustomsMessageType";
			genAddOnColumn.XA_ParentTableCode = "EM";
			genAddOnColumn.XA_Data = "IID";
			genAddOnColumn.XA_ParentID = message4.PK;
			Factory.Save();
			var messages = declaration.NoticesMessagesForDisplay.Cast<NoticesMessage>();
			AssertEquals("Messages Count", 4, messages.Count());

			Assert(messages.Any(o => o.Message.PK == message1.PK));
			Assert(messages.Any(o => o.Message.PK == message2.PK));
			Assert(messages.Any(o => o.Message.PK == message3.PK && o.StatusDescription == "S001 - Positive Functional Acknowledgement."));
			Assert(messages.Any(o => o.Message.PK == message3.PK && o.StatusDescription == "S002 - Negative Functional Acknowledgement."));
			AssertEquals("LatestNoticeMessage", message3, declaration.LatestNoticeMessage.Message);
		}

		public void TestLatestNoticeMessageWithSameProcessingDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "00001";
			var message1 = Factory.New<ACIHouseBillMessage>();
			message1.EM_EI = interchange1.PK;
			message1.EM_LinkedObject = entry;
			message1.EM_MessageSubType = MessageTypeList.Codes.ACIHouseBill;
			message1.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+70SX020418
DTM+9:201606030000:203
RFF+AGO:123029118RM0003
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1".Replace("\r\n", "'");
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "00002";
			var message2 = Factory.New<ACIHouseBillMessage>();
			message2.EM_EI = interchange2.PK;
			message2.EM_LinkedObject = entry;
			message2.EM_MessageSubType = MessageTypeList.Codes.ACIHouseBill;
			message2.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+70SX020418
DTM+9:201606030000:203
RFF+AGO:123029118RM0003
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1".Replace("\r\n", "'");
			Factory.Save();
			AssertEquals("LatestNoticeMessage", message2.PK, declaration.LatestNoticeMessage.Message.PK);
			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, new ZDateTime(2016, 6, 3));
			UniversalEventMessageTest.LinkToBusinessObject(message3, entry);
			message3.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("LatestNoticeMessage", message3.PK, declaration.LatestNoticeMessage.Message.PK);

			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, new ZDateTime(2016, 6, 3), interchangeNumber: "7598", messageNumber: "3");
			UniversalEventMessageTest.LinkToBusinessObject(message4, entry);
			message4.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			var message5 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, new ZDateTime(2016, 6, 3), interchangeNumber: "7599", messageNumber: "1");
			UniversalEventMessageTest.LinkToBusinessObject(message5, entry);
			message5.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			var message6 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, new ZDateTime(2016, 6, 3), interchangeNumber: "7599", messageNumber: "2");
			UniversalEventMessageTest.LinkToBusinessObject(message6, entry);
			message6.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("LatestNoticeMessage", message6.PK, declaration.LatestNoticeMessage.Message.PK);
		}

		public void TestB3AcceptedDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("B3AcceptedDate", ZDateTime.Empty, declaration.B3AcceptedDate);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals("B3AcceptedDate", ZDateTime.Empty, declaration.B3AcceptedDate);

			entryHeader.CH_EntryReleaseDate = new ZDateTime(2020, 8, 19);
			AssertEquals("B3AcceptedDate", new ZDateTime(2020, 8, 19), declaration.B3AcceptedDate);
		}

		public void TestCalculatedFreightAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 100m;
			AssertEquals(100m, declaration.TotalCustomsValueInLocalCurrency);
			declaration.JE_TransportMode = "ROA";
			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 2.3m;
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("EffectiveFreightPercentage", 2.3m, declaration.EffectiveFreightPercentage);
			AssertEquals("CalculatedFreightAmount", 2m, declaration.CalculatedFreightAmount);
		}

		public void TestEffectiveFreightPercentage()
		{
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultFreightPercentageCollection());
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("EffectiveFreightPercentage", 0m, declaration.EffectiveFreightPercentage);
			declaration.JE_TransportMode = "ROA";
			AssertEquals("EffectiveFreightPercentage", 0m, declaration.EffectiveFreightPercentage);
			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 0.12m;
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("EffectiveFreightPercentage", 0.12m, declaration.EffectiveFreightPercentage);
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = orgHeader.PK;
			var orgImpAddInfo = OrgImpAddInfo.Get(orgHeader);
			var freightPercentage = orgImpAddInfo.FreightPercentages.AddNew();
			freightPercentage.CY_Code = "ROA";
			freightPercentage.DefaultFreightPercentage = 0.34m;
			AssertEquals("EffectiveFreightPercentage", 0.34m, declaration.EffectiveFreightPercentage);
		}

		public void TestIsInwardWarehouseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsInwardWarehouseEntry", !declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("IsInwardWarehouseEntry", declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			Assert("IsInwardWarehouseEntry", declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("IsInwardWarehouseEntry", !declaration.IsInwardWarehouseEntry);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("IsInwardWarehouseEntry", declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("IsInwardWarehouseEntry", declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			Assert("IsInwardWarehouseEntry", declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			Assert("IsInwardWarehouseEntry", declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("IsInwardWarehouseEntry", !declaration.IsInwardWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("IsInwardWarehouseEntry", !declaration.IsInwardWarehouseEntry);
		}

		public void TestRequiresOrderNumbersOnDocs()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = consignee.PK;
			Assert(((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());
			declaration.JE_MessageType = "LVS";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());
			declaration.JE_MessageType = "LVX";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());
			declaration.JE_MessageType = "IM2";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());
			declaration.JE_MessageType = "B2";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());
		}

		public void TestRequiresOrderTrackLink()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = consignee.PK;
			Assert(((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
			declaration.JE_MessageType = "LVS";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
			declaration.JE_MessageType = "LVX";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
			declaration.JE_MessageType = "IM2";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
			declaration.JE_MessageType = "B2";
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
		}

		[TestDate(2025, 5, 28)]
		public void TestJE_MessageSubType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);
			declaration.CA_AccountingAge = 0;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertEquals(6, declaration.CA_AccountingAge);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.CA_AllowOIC = true;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			Assert("CA_AllowOIC", !declaration.CA_AllowOIC);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			var iNeedRow = declaration as INeedRow;
			AssertEquals(CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse201), iNeedRow.Row[JobDeclaration.Schema.JE_MessageSubType]);
			AssertEquals(CADEntryTypeList.Codes.ExWarehouse201, declaration.JE_MessageSubType);
		}

		#region Bonded Warehouse

		public void TestIsExWarehouseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsExWarehouseEntry", !declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("IsExWarehouseEntry", !declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			Assert("IsExWarehouseEntry", !declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("IsExWarehouseEntry", !declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("IsExWarehouseEntry", !declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			Assert("IsExWarehouseEntry", !declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			Assert("IsExWarehouseEntry", !declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("IsExWarehouseEntry", declaration.IsExWarehouseEntry);
		}

		public void TestShouldUpdateOutwardLinesWithInventoryDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", !declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", !declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", !declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", !declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", !declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", !declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", !declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("ShouldUpdateOutwardLinesWithInventoryDetails", declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
		}

		public void TestSupportsBondedWarehousing()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			Assert("SupportsBondedWarehousing", declaration.SupportsBondedWarehousing);

			helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			Assert("SupportsBondedWarehousing", !declaration.SupportsBondedWarehousing);
		}

		public void TestIsWHSUniversalXMLActive()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Assert("IsWHSUniversalXMLActive", declaration.IsWHSUniversalXMLActive);
		}

		public void TestIsInwardBondedWarehousingEnabled()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("IsInwardBondedWarehousingEnabled", declaration.IsInwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("IsInwardBondedWarehousingEnabled", !declaration.IsInwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("IsInwardBondedWarehousingEnabled", !declaration.IsInwardBondedWarehousingEnabled);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("IsInwardBondedWarehousingEnabled", declaration.IsInwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("IsInwardBondedWarehousingEnabled", declaration.IsInwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("IsInwardBondedWarehousingEnabled", !declaration.IsInwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("IsInwardBondedWarehousingEnabled", !declaration.IsInwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("IsInwardBondedWarehousingEnabled", !declaration.IsInwardBondedWarehousingEnabled);
		}

		public void TestIsOutwardBondedWarehousingEnabled()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("IsOutwardBondedWarehousingEnabled", !declaration.IsOutwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("IsOutwardBondedWarehousingEnabled", declaration.IsOutwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("IsOutwardBondedWarehousingEnabled", declaration.IsOutwardBondedWarehousingEnabled);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("IsOutwardBondedWarehousingEnabled", !declaration.IsOutwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("IsOutwardBondedWarehousingEnabled", !declaration.IsOutwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("IsOutwardBondedWarehousingEnabled", declaration.IsOutwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("IsOutwardBondedWarehousingEnabled", declaration.IsOutwardBondedWarehousingEnabled);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("IsOutwardBondedWarehousingEnabled", declaration.IsOutwardBondedWarehousingEnabled);
		}

		public void TestInwardFieldsValidation()
		{
			AssertInwardFieldsValidation(B3EntryTypeList.Codes.Warehouse10, false);
			AssertInwardFieldsValidation(CADEntryTypeList.Codes.Warehouse101, true);
			AssertInwardFieldsValidation(CADEntryTypeList.Codes.Warehouse102, true);
		}
		void AssertInwardFieldsValidation(string messageSubType, bool isCADEnable)
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345");
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_MessageSubType = messageSubType;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = isCADEnable ? MessageTypeList.Codes.CommercialAccountingDeclaration : MessageTypeList.Codes.B3CUSDEC;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceHeader2.JZ_OH_Buyer = Factory.New<OrgHeader>().PK;

			var bondedWarehouseMessage = BaseJobDeclaration.BondedWarehouseIsRequiredForBondedWarehousing("Inventory Management", helper.Importer.OH_Code);
			var errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), errorMessage);

			invoiceHeader2.JZ_OH_Buyer = ZGuid.Empty;
			AssertNotContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
			AssertNotContains(BaseJobDeclaration.BondedWarehouseIsRequiredForBondedWarehousing("Inventory Management", helper.Importer.OH_Code), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine2.JI_PartNo = helper.Part.OP_PartNum;
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine1.JI_InvoiceQuantity = 10m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			declaration.TransactionNumber.SetAccountSecurityNo();
			entryLine.CL_LineNumber = 1;
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
		}

		public void TestOutwardFieldsValidation()
		{
			AssertOutwardFieldsValidation(B3EntryTypeList.Codes.ExWarehouse20, false);
			AssertOutwardFieldsValidation(CADEntryTypeList.Codes.ExWarehouse201, true);
		}

		void AssertOutwardFieldsValidation(string messageSubType, bool isCADEnable)
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345");
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_MessageSubType = messageSubType;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = isCADEnable ? MessageTypeList.Codes.CommercialAccountingDeclaration : MessageTypeList.Codes.B3CUSDEC;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceHeader2.JZ_OH_Buyer = Factory.New<OrgHeader>().PK;

			var bondedWarehouseMessage = BaseJobDeclaration.BondedWarehouseIsRequiredForBondedWarehousing("Inventory Management", helper.Importer.OH_Code);
			var errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), errorMessage);
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			invoiceHeader2.JZ_OH_Buyer = ZGuid.Empty;
			AssertNotContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
			AssertNotContains(BaseJobDeclaration.BondedWarehouseIsRequiredForBondedWarehousing("Inventory Management", helper.Importer.OH_Code), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine2.JI_PartNo = helper.Part.OP_PartNum;
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine1.JI_InvoiceQuantity = 10m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine1.JI_PreviousEntryNumber = "1234500000001";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine2.JI_PreviousEntryNumber = "1234500000001";
			invoiceLine2.JI_PreviousEntryLineNumber = 2;
			AssertNotContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
		}

		#endregion

		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Registration", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air", "Flight/Folio", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Flight/Folio", mediumCaption: "Flight No.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Rail Car No.", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Rail Car No.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("JE_VoyageFlightNoInfo.Description - InlandWaterwayTransport", "Transport Ref.", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Transport Ref.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("JE_VoyageFlightNoInfo.Description - FixedTransportInstallations", "Transport Ref.", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Transport Ref.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportTypeList.Codes.NoCarrier;
			AssertEquals("JE_VoyageFlightNoInfo.Description - NoCarrier", "Transport Ref.", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Transport Ref.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Constants.TransportModes.Mail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Mail", "Transport Ref.", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Transport Ref.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Constants.TransportModes.Unknown;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Unknown", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public void TestSupportedAddressTypes_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;
			var expectedDocAddressTypes = base.ExpectedDocAddressTypes;
			expectedDocAddressTypes[DocAddressTypes.Codes.CommercialInvoiceOriginator] = DocAddressType.CommercialInvoiceOriginator;
			expectedDocAddressTypes[DocAddressTypes.Codes.Manufacturer] = DocAddressType.Manufacturer;
			expectedDocAddressTypes[DocAddressTypes.Codes.AdditionalDeliveryAddress] = DocAddressType.AdditionalDeliveryAddress;
			expectedDocAddressTypes[DocAddressTypes.Codes.AdditionalConsignee] = DocAddressType.AdditionalConsignee;
			expectedDocAddressTypes[DocAddressTypes.Codes.OGDProcessInspectionLPCO] = DocAddressType.OGDProcessInspectionLPCO;
			expectedDocAddressTypes[DocAddressTypes.Codes.CFIAPaymentParty] = DocAddressType.CFIAPaymentParty;
			expectedDocAddressTypes[DocAddressTypes.Codes.ImporterOfRecord] = DocAddressType.ImporterOfRecord;
			AssertEquals("Count of Address Types", expectedDocAddressTypes.Count, supportedAddressTypes.Count);
			foreach (var addressType in expectedDocAddressTypes.Values)
			{
				AssertCollectionContains(addressType, supportedAddressTypes);
			}
		}

		public void TestSupportedAddressTypes_B2B3X()
		{
			AssertSupportedAddressTypes_B2B3X(JobMessageTypeList.Codes.B2Adjustments);
			AssertSupportedAddressTypes_B2B3X(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertSupportedAddressTypes_B2B3X(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;
			var expectedDocAddressTypes = base.ExpectedDocAddressTypes;
			expectedDocAddressTypes.Remove(DocAddressTypes.Codes.SupplierDocumentaryAddress);
			expectedDocAddressTypes.Remove(DocAddressTypes.Codes.SupplierPickupDeliveryAddress);
			AssertEquals("Count of Address Types", expectedDocAddressTypes.Count, supportedAddressTypes.Count);
			foreach (var addressType in expectedDocAddressTypes.Values)
			{
				AssertCollectionContains(addressType, supportedAddressTypes);
			}
		}

		public override void TestGetContainerModeForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Road, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Road, Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Road, Core.Constants.ContainerModes.FTL));
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Road, Core.Constants.ContainerModes.LTL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Rail, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Rail, Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.Bulk, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Bulk));
		}

		#region Test ICusEntryNumFilterProvider

		protected override Customs.Business.CusEntryHeader CreateCancellableEntryHeader(BaseJobDeclaration declaration, string entryNumber)
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader.EntryNumber = entryNumber;
			return entryHeader;
		}

		#endregion

		#region IDISHost

		public void TestIDISHost()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var declaration = documentFactory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_DeclarationReference = "Dec1";
			var docManagerInfo = declaration.DocManagerInfo;
			AssertEquals(0, ((IDISHost)declaration).EDocs.Count());
			docManagerInfo.Save();
			var filter = new ZQuery(StorageMainSchema.SM_ParentFK, declaration.PK);
			AssertNull(documentFactory.LoadTop1<IStorageMain>(filter));

			var org = documentFactory.New<OrgHeader>();
			org.OH_FullName = "org1";
			declaration.JE_OH_Importer = org.PK;
			byte[] contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			var shipment = documentFactory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.DocManagerInfo.AddFileOrDocument(contents, "document.pdf", "ABC");
			declaration.DocManagerInfo.AddFileOrDocument(contents, "document2.pdf", "ABC");

			var disHost = (IDISHost)declaration;
			AssertEquals("DISHost", declaration, ((IDISHostProvider)declaration).DISHost);
			AssertEquals("BranchPK", GlbBranch.CurrentBranch.PK, disHost.BranchPK);
			AssertEquals("CompanyPK", GlbCompany.CurrentCompany.PK, disHost.CompanyPK);
			AssertEquals("ApplicationCodes Count", 1, disHost.ApplicationCodes.Count());
			AssertEquals("ApplicationCodes", Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, disHost.ApplicationCodes.First());
			AssertEquals("RequiredDocumentsProvider", declaration.DocsAndCartage, disHost.RequiredDocumentsProvider);
			AssertEquals(2, ((IDISHost)declaration).EDocs.Count());
			AssertEquals("JobNumber", "Dec1", disHost.JobNumber);
			AssertEquals("ControllerIDProvider", declaration, disHost.ControllerIDProvider);
			AssertEquals("ImporterName", "org1", disHost.ImporterName);
			AssertEquals("ErrorMessages", 0, disHost.ErrorMessages.Count());

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("ShowDISFeatures", disHost.ShowDISFeatures);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			Assert("ShowDISFeatures", !disHost.ShowDISFeatures);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("ShowDISFeatures", !disHost.ShowDISFeatures);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			Assert("ShowDISFeatures", !disHost.ShowDISFeatures);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("ShowDISFeatures", disHost.ShowDISFeatures);

			var caDifHost = (ICADIFHost)declaration;
			AssertNull("ValueProvider", caDifHost.ValueProvider);
		}

		#endregion

		public void TestSupportsParentPackage()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("SupportsParentPackage", !declaration.SupportsParentPackage);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("SupportsParentPackage", declaration.SupportsParentPackage);
		}

		public void TestImporterOfRecord()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG1";
			var declaration = Factory.New<JobDeclaration>();
			AssertNull("ImporterOfRecord", declaration.ImporterOfRecord);
			declaration.ImporterOfRecordAddress.OrganisationPK = org.PK;
			Factory.Save();
			AssertEquals("ImporterOfRecord", org.PK, declaration.ImporterOfRecord.PK);
		}

		public void TestImporterOfRecordAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = org.PK;
			AssertEquals(org.MainAddress.PK, declaration.ImporterOfRecordAddress.E2_OA_Address);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			var addr1 = org2.Addresses.AddNew();
			addr1.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			declaration.ImporterOfRecordAddress.OrganisationPK = org2.PK;
			AssertEquals(org2.MainAddress.PK, declaration.ImporterOfRecordAddress.E2_OA_Address);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertEquals(addr1.PK, declaration.ImporterOfRecordAddress.E2_OA_Address);
		}

		public void TestImporterDocumentaryAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.JE_OH_Importer = org.PK;
			AssertEquals(org.MainAddress.PK, declaration.ImporterDocumentaryAddress.E2_OA_Address);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			var addr1 = org2.Addresses.AddNew();
			addr1.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			declaration.ImporterDocumentaryAddress.OrganisationPK = org2.PK;
			AssertEquals(org2.MainAddress.PK, declaration.ImporterDocumentaryAddress.E2_OA_Address);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertEquals(addr1.PK, declaration.ImporterDocumentaryAddress.E2_OA_Address);
		}

		public void TestEffectiveImporter()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("EffectiveImporter", org1.PK, declaration.EffectiveImporter.PK);
			declaration.ImporterOfRecordAddress.OrganisationPK = org2.PK;
			AssertEquals("EffectiveImporter", org2.PK, declaration.EffectiveImporter.PK);
		}

		public void TestEffectiveImporterAddInfo()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("EffectiveImporterAddInfo", OrgImpAddInfo.Get(org1), declaration.EffectiveImporterAddInfo);
			declaration.ImporterOfRecordAddress.OrganisationPK = org2.PK;
			AssertEquals("EffectiveImporterAddInfo", OrgImpAddInfo.Get(org2), declaration.EffectiveImporterAddInfo);
		}

		public void TestEffectiveCFIAFeePaymentMethod()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			OrgImpAddInfo.Get(org1).ZO_CFIAFeePaymentMethod = "IMP";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			OrgImpAddInfo.Get(org2).ZO_CFIAFeePaymentMethod = "BKR";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("EffectiveImporterAddInfo", "IMP", declaration.EffectiveCFIAFeePaymentMethod);
			declaration.ImporterOfRecordAddress.OrganisationPK = org2.PK;
			AssertEquals("EffectiveImporterAddInfo", "BKR", declaration.EffectiveCFIAFeePaymentMethod);
		}

		public void TestEffectiveBranch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "BR1";
			AssertEquals("EffectiveBranch", GlbBranch.CurrentBranch.PK, declaration.EffectiveBranch.PK);
			declaration.JE_GB = branch.PK;
			AssertEquals("EffectiveBranch", branch.PK, declaration.EffectiveBranch.PK);
		}

		[TestDate(2025, 5, 28)]
		public void TestCA_CSAEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			Assert(!declaration.CA_CSAEntryInfo.ReadOnly);

			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);
			declaration.CA_CSAEntry = true;
			Assert(declaration.CA_CSAEntry);
			AssertEquals("CA_AccountingAge", 0, declaration.CA_AccountingAge);
			declaration.CA_CSAEntry = false;
			AssertEquals("CA_AccountingAge", 6, declaration.CA_AccountingAge);
		}

		public void TestCA_CSAEntry_WhenImporterOrMessageTypeChanges()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IM1";

			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "IM2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.CA_CSAEntry = true;
			Assert(declaration.CA_CSAEntry);

			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals("CA_CSAEntry should be unset when importer changes", ZBool.False, declaration.CA_CSAEntry);

			declaration.CA_CSAEntry = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(importer2.PK, declaration.JE_OH_Importer);
			AssertEquals("CA_CSAEntry should be unset when message type changes", ZBool.False, declaration.CA_CSAEntry);
		}

		public void TestRequiredDocumentAddInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			var docsAndCartage = JobDocsAndCartage.New(declaration);

			var document1 = docsAndCartage.RequiredDocuments.AddNew();
			var addinfo11 = document1.AddInfos.AddNew();
			addinfo11.EX_ReferenceNumber = "100000000011";
			addinfo11.EX_ApplicationCode = "CAD";
			addinfo11.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingOriginal;

			Factory.Save();

			AssertEquals("100000000011", declaration.DIFURNs);
			AssertEquals("Awaiting Original", declaration.DIFMessageStatus);

			var addinfo12 = document1.AddInfos.AddNew();
			addinfo12.EX_ReferenceNumber = "100000000012";
			addinfo12.EX_ApplicationCode = "DIS";
			addinfo12.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingAmendment;

			var addinfo13 = document1.AddInfos.AddNew();
			addinfo13.EX_ReferenceNumber = "100000000013";
			addinfo13.EX_ApplicationCode = "CAD";
			addinfo13.EX_GC_Company = Factory.New<GlbCompany>().PK;
			addinfo13.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingAmendment;

			Factory.Save();

			AssertEquals("100000000011", declaration.DIFURNs);
			AssertEquals("Awaiting Original", declaration.DIFMessageStatus);

			var addinfo14 = document1.AddInfos.AddNew();
			addinfo14.EX_ReferenceNumber = "100000000014";
			addinfo14.EX_ApplicationCode = "CAD";
			addinfo14.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingAmendment;
			var document2 = docsAndCartage.RequiredDocuments.AddNew();
			var addinfo21 = document2.AddInfos.AddNew();
			addinfo21.EX_ReferenceNumber = "100000000021";
			addinfo21.EX_ApplicationCode = "CAD";
			addinfo21.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingOriginal;

			Factory.Save();

			AssertEquals("Multiple", declaration.DIFURNs);
			AssertEquals("Multiple", declaration.DIFMessageStatus);
		}

		public void TestJE_OH_Importer()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			var deliveryAddressOrg = Factory.New<OrgHeader>();
			deliveryAddressOrg.OH_FullName = "TestOrgName2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OH_Importer = importer.PK;
			declaration.Validation.ValidateJE_OH_Importer(); // Importer Documentary will set after JE_OH_Importer set
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_FullName = "TestOrgName2";
			var deliveryAddressOrg2 = Factory.New<OrgHeader>();
			deliveryAddressOrg2.OH_FullName = "TestOrgName22";

			OrgImpAddInfo.Get(importer2).ZO_IsCSAApprovedImporter = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			declaration.JE_OH_Importer = importer2.PK;
			Assert(declaration.IsB3X);
			AssertEquals(B2TypeList.Codes.Specific, declaration.CA_B2Type);
			AssertEquals("", declaration.JE_PaymentMethod);

			declaration.JE_OH_Importer = importer.PK;
			Assert(declaration.IsB2Adjustments);
			AssertEquals(PaymentPartyCodeDescriptionList.Codes.Default, declaration.JE_PaymentMethod);
		}

		public void TestJE_CarrierCode_SetCCN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CarrierCode = "1343";
			AssertEquals("1343", declaration.EffectiveCCN);

			declaration.EffectiveCCN = "15364677";
			declaration.JE_CarrierCode = "1343";
			AssertEquals("15364677", declaration.EffectiveCCN);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.EffectiveCCN = ZString.Empty;
			declaration.JE_CarrierCode = "1343";
			AssertEquals(ZString.Empty, declaration.EffectiveCCN);
		}

		public void TestCalculateEstimatedPaymentDueDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee(Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			var impAddInfo = OrgImpAddInfo.Get(org);
			impAddInfo.ZO_IsCSAApprovedImporter = true;

			var declaration = Factory.New<JobDeclaration>();
			PrepareData();
			TestCase("IMP", string.Empty, false, false, new ZDateTime(2024, 11, 15), new ZDateTime(2024, 11, 22));
			TestCase("LVS", string.Empty, false, false, new ZDateTime(2024, 11, 11), new ZDateTime(2024, 12, 24));

			TestCase("IMP", string.Empty, true, false, new ZDateTime(2024, 11, 01), ZDateTime.Empty);

			TestCase("IMP", "OPTION1", false, false, new ZDateTime(2024, 11, 01), new ZDateTime(2024, 12, 18));
			TestCase("IMP", "OPTION1", false, false, new ZDateTime(2024, 12, 31), new ZDateTime(2025, 01, 18));

			TestCase("IMP", "OPTION2", false, false, new ZDateTime(2024, 11, 17), new ZDateTime(2024, 11, 29));
			TestCase("IMP", "OPTION2", false, false, new ZDateTime(2024, 11, 18), new ZDateTime(2024, 11, 29));

			declaration = Factory.New<JobDeclaration>();
			using (declaration.SuspendMarkApportionmentDirty())
			{
				PrepareData();
				TestCase("IMP", string.Empty, false, true, new ZDateTime(2024, 11, 01, 13, 54, 0), new ZDateTime(2024, 11, 8));
				TestCase("LVS", string.Empty, false, true, new ZDateTime(2024, 12, 01, 20, 05, 0), new ZDateTime(2025, 1, 24));

				TestCase("IMP", string.Empty, true, true, new ZDateTime(2024, 11, 01), ZDateTime.Empty);

				TestCase("IMP", "OPTION1", false, true, new ZDateTime(2024, 11, 19, 13, 54, 0), new ZDateTime(2024, 12, 18));

				TestCase("IMP", "OPTION2", false, true, new ZDateTime(2024, 11, 19, 13, 54, 0), new ZDateTime(2024, 12, 31));
			}

			void PrepareData()
			{
				declaration.SuspendValidation();
				declaration.JE_OH_Importer = org.PK;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 100;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 100m;
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_CustomsValue = 100m;
			}

			void TestCase(string messageType, string accountingTimeOption, bool isCSAEntry, bool isLowValueNormalReleaseJob, ZDateTime dateTimeFilled, ZDateTime expectedDate)
			{
				impAddInfo.ZO_AccountingTimeOption = accountingTimeOption;
				declaration.JE_MessageType = messageType;
				declaration.CA_CSAEntry = isCSAEntry;
				AssertEquals(isLowValueNormalReleaseJob, declaration.IsLowValueNormalReleaseJob);
				declaration.JE_EntryAuthorisationDate = dateTimeFilled;
				declaration.CalculateEstimatedPaymentDueDate();
				AssertEquals($"messageType:{messageType}, accountingTimeOption:{accountingTimeOption}, isCSAEntry:{isCSAEntry}, isLowValueNormalReleaseJob:{isLowValueNormalReleaseJob}", expectedDate, declaration.CA_EstimatedPaymentDueDate);
			}
		}

		[TestDate(2014, 9, 1)]
		public void TestAccountingAgeWhenCA_CSAEntryIsTrue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 8, 21);
			declaration.CA_CSAEntry = true;
			Assert(!declaration.IsLowValueNormalReleaseJob);
			AssertEquals("CA_AccountingAge should be zero when CA_CSAEntry is true", 0, declaration.CA_AccountingAge);

			var declaration1 = Factory.New<JobDeclaration>();
			using (declaration1.SuspendMarkApportionmentDirty())
			{
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration1.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				declaration1.CA_CSAEntry = true;
				var invoice1 = declaration1.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 100;
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 100m;
				var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
				entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var entryLine1 = entryHeader1.MergedLines.AddNew();
				entryLine1.CL_CustomsValue = 100m;
				Assert(declaration1.IsLowValueNormalReleaseJob);
				AssertEquals("CA_AccountingAge should be zero when CA_CSAEntry is true", 0, declaration.CA_AccountingAge);
			}
		}

		[TestDate(2014, 9, 1)]
		public void TestAccountingAgeWhenCA_CSAEntryIsFalse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 8, 21);
			Assert(!declaration.IsLowValueNormalReleaseJob);
			AssertEquals("CA_AccountingAge should be 6 when CA_CSAEntry is false", 6, declaration.CA_AccountingAge);

			var declaration1 = Factory.New<JobDeclaration>();
			using (declaration1.SuspendMarkApportionmentDirty())
			{
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration1.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				var invoice1 = declaration1.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 100;
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 100m;
				var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
				entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var entryLine1 = entryHeader1.MergedLines.AddNew();
				entryLine1.CL_CustomsValue = 100m;
				Assert(declaration1.IsLowValueNormalReleaseJob);
				AssertEquals("CA_AccountingAge should be 6 when CA_CSAEntry is false", 6, declaration.CA_AccountingAge);
			}
		}

		public void TestICurrencyConverterDataProvider()
		{
			var dec = Factory.New<JobDeclaration>();
			var decAsProvider = dec as ICurrencyConverterDataProvider;
			AssertEquals("Fall back days", 365, decAsProvider.MaximumDaysToFallback);
		}

		public void TestReleaseDateRemovedWhenManualCancelled()
		{
			var jobDecl = Factory.New<JobDeclaration>();
			jobDecl.JE_MessageType = JobMessageTypeList.Codes.Import;

			var releaseEntry = jobDecl.ActiveEntryHeaders.AddNew();
			releaseEntry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			releaseEntry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			jobDecl.JE_EntryAuthorisationDate = ZDateTime.Today.AddMonths(-1);
			jobDecl.JE_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;

			var manReleaseCancel = new ManualReleaseCancelBO("Note", Factory);
			manReleaseCancel.ManualReleaseDate = ZDateTime.Today;
			manReleaseCancel.ManualReleaseReason = "Test";

			var manualCancelSupport = jobDecl as IManualCancelSupport;
			var reasonForCannotManualCancel = manualCancelSupport.GetReasonForCannotManualCancel();
			manualCancelSupport.ManualCancel(manReleaseCancel);
			AssertEquals(ZDateTime.Empty, jobDecl.JE_EntryAuthorisationDate);
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.Cancelled, jobDecl.JE_EntryStatus);
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.Cancelled, releaseEntry.CH_EntryStatus);
			AssertEquals(ZString.Empty, reasonForCannotManualCancel);
		}

		public void TestServiceOpionDoesNotDefaultToIIDWhenShipmentTypeIsLVS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ACROSSServiceOptions.Codes.IID, declaration.CA_ServiceOption);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(ZString.Empty, declaration.CA_ServiceOption);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals(ZString.Empty, declaration.CA_ServiceOption.ToString());
		}

		public void TestE2_ContactShouldNotHasInvalidCharacters()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.ImporterDocumentaryAddress);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.VendorDocAddress);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.CommercialInvoiceOriginator);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.Manufacturer);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.AdditionalDeliveryAddress);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.AdditionalConsignee);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.OGDProcessInspectionLPCO);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.CFIAPaymentParty);
			ContactNameValidataionHelperForTest.AssertE2_Contact(declaration.ImporterOfRecordAddress);
		}

		#region Implementation

		protected override System.Collections.Hashtable ExpectedDocAddressTypes
		{
			get
			{
				var result = base.ExpectedDocAddressTypes;
				result[DocAddressTypes.Codes.SellingParty] = DocAddressType.SellingParty;
				return result;
			}
		}

		protected override string DefaultExportMessageType
		{
			get { return JobMessageTypeList.Codes.Export; }
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new JobDeclarationLightValidationTester(bizObjToTest);

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}
		#endregion

		public void TestIEDIFACTMessageAttacheeProperties()
		{
			var helper = new DeclarationTestHelper(Factory, false);
			var declaration = Factory.New<JobDeclaration>();
			helper.UpdateOrAddCustomsRegNo(helper.Consignor, DeclarationTestHelper.ValidBusinessNumber1, OrgCusCode.CACodeTypes.BusinessNumberForImportExport, helper.Canada);
			declaration.JE_OH_Supplier = helper.Consignor.PK;
			declaration.JE_OH_Importer = helper.Consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.JE_DeclarationReference = "B00123457";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 6, 20, 1, 2, 3);
			declaration.JE_MessageStatus = "SNT";
			declaration.JE_EntryStatus = EntryStatusList.Codes.Clear;
			var newMessage = Factory.New<EDIMessage>();

			IEDIFACTMessageAttachee decAttacheee = declaration;
			decAttacheee.AddMessage(newMessage);
			AssertEquals("MessageStatus", "SNT", decAttacheee.MessageStatus);
			Assert("Messages", declaration.Messages.Contains(newMessage));
			AssertEquals("JobStatus", EntryStatusList.Codes.Clear, decAttacheee.JobStatus);
			AssertEquals("JobIdentification", "B00123457", decAttacheee.JobIdentification);
			AssertEquals("TopLevelBusinessObject", declaration, decAttacheee.TopLevelBusinessObject);
		}

		[TestDate(2021, 01, 29)]
		public void TestIB3MessageProcessorLinkedObjectProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var linkedObject = (IB3MessageProcessorLinkedObject)declaration;
			declaration.CA_B2AcceptedDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, linkedObject.EntryReleaseDate);
			linkedObject.EntryReleaseDate = ZDateTime.Today.AddDays(1);
			AssertEquals(ZDateTime.Today.AddDays(1), declaration.CA_B2AcceptedDate);
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Validation", typeof(ExportJobDeclarationValidation), declaration.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportJobDeclarationValidation), declaration.Validation.GetType());
			declaration.JE_MessageType = "XXX";
			AssertEquals("Other Validation", typeof(JobDeclarationValidation), declaration.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Validation", typeof(B2JobDeclarationValidation), declaration.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Validation", typeof(B2JobDeclarationValidation), declaration.Validation.GetType());
		}

		public void TestDisbursementAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var duty1 = invoiceLine1.DutiesAndTaxes.AddNew();
			duty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			duty1.C1_Amount = 10m;
			var tax1 = invoiceLine1.DutiesAndTaxes.AddNew();
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax1.C1_Amount = 11m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var duty2 = invoiceLine2.DutiesAndTaxes.AddNew();
			duty2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			duty2.C1_Amount = 12m;
			var tax2 = invoiceLine2.DutiesAndTaxes.AddNew();
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax2.C1_Amount = 13m;
			AssertEquals(46m, declaration.DisbursementAmount);
		}

		public void TestTotalDutyAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = DefaultImportMessageType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var duty1 = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty1.C1_Amount = 5000.5m;
			var duty2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Amount = 5000.5m;
			AssertEquals(10001m, declaration.TotalDutyAmount);
		}

		public void TestIBondDetailsDefaultMembers()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = org1.PK;
			declaration.ImporterOfRecordAddress.OrganisationPK = org2.PK;
			declaration.CA_EstReleaseDate = ZDateTime.Today.AddDays(-1);

			IBondDetailsDefault bondDefault = declaration;
			AssertEquals(org1.PK, bondDefault.ImportOrg.OrgHeader.PK);
			AssertEquals(org2.PK, bondDefault.ImporterOfRecord.OrgHeader.PK);
			AssertEquals(ZDateTime.Today.AddDays(-1), bondDefault.EffectiveDate);
		}

		public void TestDefaultJE_ApplicationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("'BLT' should be defaulted when registry setting is empty", DeclarationApplicationCodeList.Codes.Builtin, declaration.JE_ApplicationCode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("'BLT' should be defaulted when registry setting is empty", DeclarationApplicationCodeList.Codes.Builtin, declaration.JE_ApplicationCode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("'BLT' should be defaulted when registry setting is empty", DeclarationApplicationCodeList.Codes.Builtin, declaration.JE_ApplicationCode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("Application code should be empty for LVS", ZString.Empty, declaration.JE_ApplicationCode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("Application code should be empty for LVX", ZString.Empty, declaration.JE_ApplicationCode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("Application code should be empty for B2", ZString.Empty, declaration.JE_ApplicationCode);

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals("'ITF' should be defaulted from registry setting", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("'ITF' should be defaulted from registry setting", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("'ITF' should be defaulted from registry setting", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				AssertEquals("Application code should be empty for LVS", ZString.Empty, declaration.JE_ApplicationCode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				AssertEquals("Application code should be empty for LVX", ZString.Empty, declaration.JE_ApplicationCode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				AssertEquals("Application code should be empty for B2", ZString.Empty, declaration.JE_ApplicationCode);
			}
		}

		public void TestJE_ApplicationCode_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Submit Type should be protected when registry value is default (empty).", declaration.JE_ApplicationCodeInfo.ReadOnly);

			var submissionTypeListInRegistry = new DeclarationApplicationCodeListForRegistry().GetAllCodes();
			var submissionTypeListInDeclaration = new DeclarationApplicationCodeList().GetAllCodes();
			foreach (var submissionType in submissionTypeListInRegistry)
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
				customsInterface.RecipientID = "123";
				customsInterface.SubmissionType = submissionType;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					declaration = Factory.New<JobDeclaration>();
					if (submissionTypeListInDeclaration.Contains(submissionType))
					{
						Assert($"Submit Type should be protected when registry value is '{submissionType}'.", declaration.JE_ApplicationCodeInfo.ReadOnly);
					}
					else
					{
						Assert($"Submit Type should NOT be protected when registry value is '{submissionType}'.", !declaration.JE_ApplicationCodeInfo.ReadOnly);
					}
				}
			}
		}

		public void TestShowSubmitMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("ShowSubmitMenuItem should be false because 'BLT' is defaulted into submit type.", !declaration.ShowSubmitMenuItem);

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("ShowSubmitMenuItem should be true because 'ITF' is defaulted for export declaration", declaration.ShowSubmitMenuItem);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("ShowSubmitMenuItem should be true because 'ITF' is defaulted for import declaration", declaration.ShowSubmitMenuItem);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				Assert("ShowSubmitMenuItem should be false for LVX.", !declaration.ShowSubmitMenuItem);

				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				Assert("ShowSubmitMenuItem should be false for B2 adjustments.", !declaration.ShowSubmitMenuItem);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("ShowSubmitMenuItem should be false because 'BLT' is defaulted for export declaration", !declaration.ShowSubmitMenuItem);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("ShowSubmitMenuItem should be false because 'BLT' is defaulted for import declaration", !declaration.ShowSubmitMenuItem);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				Assert("ShowSubmitMenuItem should be false for LVX.", !declaration.ShowSubmitMenuItem);

				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				Assert("ShowSubmitMenuItem should be false for B2 adjustments.", !declaration.ShowSubmitMenuItem);
			}
		}

		public void TestIsDeclarationIntegrated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			Assert(declaration.IsDeclarationIntegrated);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			Assert(!declaration.IsDeclarationIntegrated);
		}

		public void TestExamLocationDescription_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ExamLocationCode = "A";
			Assert(declaration.ExamLocationDescriptionInfo.ReadOnly);

			declaration.CA_ExamLocationCode = ZString.Empty;
			Assert(!declaration.ExamLocationDescriptionInfo.ReadOnly);
		}

		public void TestJE_OH_Supplier_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			Assert(declaration.JE_OH_SupplierInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!declaration.JE_OH_SupplierInfo.ReadOnly);
		}
	}
}
