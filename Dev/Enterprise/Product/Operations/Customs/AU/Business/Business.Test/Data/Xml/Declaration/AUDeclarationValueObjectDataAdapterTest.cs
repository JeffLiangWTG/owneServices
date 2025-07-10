using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Country = Enterprise.Core.Constants.CountryCodes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUDeclarationValueObjectDataAdapter))]
	public class AUDeclarationValueObjectDataAdapterTest : DeclarationValueObjectDataAdapterAbstractTest
	{
		public override void TestGetNewInvoicesGenerator()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			TestAUDeclarationValueObjectDataAdapter adapter = new TestAUDeclarationValueObjectDataAdapter();
			AssertEquals("Invoice Generator", InvoicesGeneratorType, adapter.GetNewInvoicesGenerator(jobDec).GetType());
		}

		public void TestSetDeclarationDetails()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			Xsd.Declaration declaration = new Xsd.Declaration();
			TestAUDeclarationValueObjectDataAdapter adapter = new TestAUDeclarationValueObjectDataAdapter();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportDeclarationDetails(jobDec, declaration, "", importContext);
			AssertEquals("", jobDec.AddInfo.ZA_ManifestClientIDOverride_Hidden);
			declaration.ManifestID = "ManifestID";
			adapter = new TestAUDeclarationValueObjectDataAdapter();
			adapter.ImportDeclarationDetails(jobDec, declaration, "", importContext);
			AssertEquals("ManifestID", jobDec.AddInfo.ZA_ManifestClientIDOverride_Hidden);
		}

		public void TestImportDeclarationCorrectLookupSelected()
		{
			Classification importClassification = Factory.New<Classification>();
			importClassification.CC_LookupCode = "IMPCLASS";
			importClassification.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClassification.CC_TariffNum = "0000.00.01";
			importClassification.AddInfo.ZA_ORG = "AU";
			Classification exportClassification = Factory.New<Classification>();
			exportClassification.CC_LookupCode = "EXPCLASS";
			exportClassification.CC_TariffNum = "0000.00.02";
			exportClassification.CC_ClassificationType = Classification.ClassificationType.EXP;
			exportClassification.AddInfo.ZA_ORG = "AU";
			AUOrgSupplierPart product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "APART";
			product.OP_Desc = "PART DESC";
			product.AddNewImportPivotWithClassification(importClassification.PK);
			product.AddNewExportPivotWithClassification(exportClassification.PK);
			product.OP_StockKeepingUnit = "NO";
			OrgHeader owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = owner.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			Xsd.ConsolAndShipment xsdConsol = new Xsd.ConsolAndShipment();
			Xsd.InvoiceHeader xsdInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xsdInvoiceLine = new Xsd.InvoiceLine();
			TestAUDeclarationValueObjectDataAdapter adapter = new TestAUDeclarationValueObjectDataAdapter();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			xsdConsol.Consol.ConsolDetail.PortOfDischarge.Port.Value = "AUSYD";
			xsdConsol.Consol.ConsolDetail.PortOfLoading.Port.Value = "USLAX";
			xsdConsol.Shipment.ShipmentDetails.PortofDestination.Port.Value = "AUSYD";
			xsdConsol.Shipment.ShipmentDetails.PortOfOrigin.Port.Value = "AULAX";
			xsdConsol.Shipment.ShipmentDetails.Consignee.EDICode = "OWNER";
			xsdConsol.Shipment.Invoices.Add(xsdInvoiceHeader);
			xsdConsol.Shipment.Invoices[0].InvoiceLines.Add(xsdInvoiceLine);
			xsdConsol.Shipment.Invoices[0].InvoiceLines[0].ProductNumber = "APART";
			xsdConsol.Shipment.Invoices[0].InvoiceLines[0].InvoiceQty.Value = 1M;
			xsdConsol.Shipment.Invoices[0].InvoiceLines[0].LinePrice.Value = 100M;
			adapter.ImportFromValueObjectCore(jobDec, xsdConsol, importContext);
			AssertEquals(JobMessageTypeList.Codes.Import, jobDec.JE_MessageType);
			AssertEquals("APART", jobDec.InvoiceLines[0].JI_PartNo);
			AssertEquals("PART DESC", jobDec.InvoiceLines[0].JI_Description);
			AssertEquals(importClassification.PK, jobDec.InvoiceLines[0].JI_CC);
		}

		public void TestSetShipmentTypeDetails()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_RL_NKPortOfArrival = "AUMEL";
			TestAUDeclarationValueObjectDataAdapter adapter = new TestAUDeclarationValueObjectDataAdapter();
			adapter.SetShipmentTypeDetailsCore(jobDec);
			AssertEquals(Customs.Business.JobMessageTypeList.Codes.Import, jobDec.JE_MessageType);
			AssertEquals(Core.Constants.ContainerModes.FCL, jobDec.JE_ContainerMode);
			jobDec.JE_RL_NKPortOfArrival = "NZAKL";
			adapter = new TestAUDeclarationValueObjectDataAdapter();
			adapter.SetShipmentTypeDetailsCore(jobDec);
			AssertEquals(Customs.Business.JobMessageTypeList.Codes.Export, jobDec.JE_MessageType);
			AssertEquals(Core.Constants.ContainerModes.Containerised, jobDec.JE_ContainerMode);
		}

		public void TestGetShipmentDetails()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo = new Xsd.InterchangeInfo();
			Xsd.Consols consols = new Xsd.Consols();
			consols.Consol = new Xsd.ConsolCollection();
			consols.Consol.AddNew();
			Xsd.Shipments shipments = new Xsd.Shipments();
			shipments.Shipment = new Xsd.ShipmentCollection();
			shipments.Shipment.AddNew();
			shipments.Shipment[0].Declaration = new Xsd.Declaration();
			shipments.Shipment[0].Declaration.ManifestID = "ManifestID";
			TestAUDeclarationValueObjectDataAdapter adapter = new TestAUDeclarationValueObjectDataAdapter();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			adapter.ImportDeclarationDetails(jobDec, shipments.Shipment[0].Declaration, "", importContext);
			AssertEquals("ManifestID", jobDec.AddInfo.ZA_ManifestClientIDOverride_Hidden);
		}

		public new void TestGetNewInvoiceAdapter()
		{
			TestAUDeclarationValueObjectDataAdapter adapter = new TestAUDeclarationValueObjectDataAdapter();
			AssertEquals(typeof(AUInvoiceValueObjectDataAdapter), adapter.GetNewInvoiceAdapter(Factory.New<BaseJobDeclaration>()).GetType());
		}

		public void TestExportCustomsEntries()
		{
			var uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_EntrySubmittedDate = new ZDateTime(2005, 08, 30);
			jobDec.JE_ExportDate = new ZDateTime(2005, 09, 01);
			SetExchangeRate(jobDec.JE_ExportDate, jobDec.JE_ExportDate, 0.5m, uSD, "CUS");
			var invHead = jobDec.Invoices.AddNew();
			invHead.JZ_RX_NKInvoice_Currency = uSD.RX_Code;
			var invLine1 = invHead.JobComInvoiceLines.AddNew();
			invLine1.AddInfo.ZA_TILV = "USD534.85";
			var invLine2 = invHead.JobComInvoiceLines.AddNew();
			invLine2.AddInfo.ZA_TILV = "USD267.70";
			var cusHead = jobDec.CustomsEntryHeaders.AddNew();
			cusHead.EntryNumber = "111";
			var cusLine1 = cusHead.MergedLines.AddNew();
			cusLine1.CL_CustomsValue = 10M;
			cusLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 100M);
			invLine1.JI_CL = cusLine1.PK;
			var cusLine2 = cusHead.MergedLines.AddNew();
			cusLine2.CL_CustomsValue = 25M;
			invLine2.JI_CL = cusLine2.PK;
			var charge = cusHead.Charges.AddNew();
			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.EntryFee;
			charge.C1_ChargeAmount = 12345.45M;
			var customsEntries = new Xsd.CustomsEntryCollection();
			var adapter = new TestAUDeclarationValueObjectDataAdapter();
			adapter.ExportCustomsEntries(customsEntries, jobDec.CustomsEntryHeaders, new NotificationBuffer());
			AssertEquals("1 CusEntryHeader should be exported", 1, customsEntries.Count);
			var entry = customsEntries[0];
			AssertEquals("CustomsEntryNumber.Country", Country.Australia, entry.CustomsEntryNumber.Country);
			AssertEquals("CustomsEntryNumber.Type", cusHead.CusEntryNumber.CE_EntryType, entry.CustomsEntryNumber.Type);
			AssertEquals("CustomsEntryNumber.Number", "111", entry.CustomsEntryNumber.Number);
			AssertEquals("EntryDate", new ZDate(2005, 08, 30), entry.EntryDate);
			AssertEquals("CustomsFactor", 1M, entry.CustomsFactor);
			AssertEquals("ExchangeRate", 0.5M, entry.ExchangeRate);
			AssertEquals("CustomsValue", 35M, entry.CustomsValue.Value);
			AssertEquals("TransportAndInsurance", 1605.1M, entry.TransportAndInsurance.Value);
			AssertEquals("2 CusEntrylines should be exported", 2, entry.CustomsEntryLines.Count);
			var entryLine = entry.CustomsEntryLines[0];
			AssertEquals("1 CusEntryLineFee should be exported", 1, entryLine.Fees.Count);
			var xsdFee = entryLine.Fees[0];
			AssertEquals("FeeType", CusEntryChargeTypeList.Codes.DutyAmount, xsdFee.FeeType);
			AssertEquals("FeeAmount", 100M, xsdFee.FeeAmount.Value);
			AssertEquals("1 CusEntryHeaderCharge should be exported", 1, entry.Charges.Count);
			var xsdCharge = entry.Charges[0];
			AssertEquals("ChargeType", CusEntryChargeTypeList.Codes.EntryFee, xsdCharge.ChargeType);
			AssertEquals("ChargeAmount", 12345.45M, xsdCharge.ChargeAmount.Value);
		}

		public void TestExportOrders()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			OrderItem item = jobDec.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "111111";
			Xsd.Shipment shipment = new Xsd.Shipment();
			TestAUDeclarationValueObjectDataAdapter adapter = new TestAUDeclarationValueObjectDataAdapter();
			adapter.ExportOrders(shipment, jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("OrderReferences.Length", 1, shipment.ShipmentDetails.OrderReferences.Length);
			AssertEquals("OrderReferences[0]", "111111", shipment.ShipmentDetails.OrderReferences[0]);
			AssertEquals("Orders.Count", 0, shipment.Orders.Count);
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "222222";
			Factory.Save();
			jobDec.AttachedOrders.Add(order);
			shipment = new Xsd.Shipment();
			adapter.ExportOrders(shipment, jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("OrderReferences", 1, shipment.ShipmentDetails.OrderReferences.Length);
			AssertEquals("OrderRef", "111111", shipment.ShipmentDetails.OrderReferences[0]);
			AssertEquals("Orders.Count", 1, shipment.Orders.Count);
			AssertEquals("OrderNumber", "222222", shipment.Orders[0].OrderIdentifier.OrderNumber);
		}

		public void TestApportionmentBalanceProblem()
		{
			string exampleFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUPopulatedLineCharge.xml");
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			SetExchangeRate(new ZDateTime(2010, 11, 2), new ZDateTime(2010, 11, 2), 0.9862m, usd, "CUS");
			new DeclarationXmlDataImporter(new AUDeclarationValueObjectDataAdapter()).ImportData(exampleFileFullPath, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "OOLU2509389880");
			query.OrderBy = JobDeclarationSchema.JE_DeclarationReference.Name + " desc";
			var dec = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull(dec);
			dec.JE_ExportDate = new ZDateTime(2010, 11, 2);
			dec.ResumeApportionment();
			string message;
			if (dec.Invoices.AreChargesBalancedForInvoices(out message))
			{
				Assert(true);
			}
			else
			{
				Fail(message);
			}
		}

		public void TestLineTAndIAndHeaderTAndI()
		{
			string exampleFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUPopulatedLineCharge2.xml");
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			SetExchangeRate(new ZDateTime(2011, 1, 2), new ZDateTime(2011, 1, 2), 1.0161m, usd, "CUS");
			new DeclarationXmlDataImporter(new AUDeclarationValueObjectDataAdapter()).ImportData(exampleFileFullPath, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "KKLUHA52700300");
			query.OrderBy = JobDeclarationSchema.JE_DeclarationReference.Name + " desc";
			var dec = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull(dec);
			dec.JE_ExportDate = new ZDateTime(2011, 1, 2);
			dec.ResumeApportionment();
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			Money totalTAndI = Money.Empty;
			List<CusEntryLine> mergedLines = new List<CusEntryLine>(new TypedEnumerable<CusEntryLine>(dec.CustomsEntryHeaders[0].MergedLines));
			mergedLines.Sort((x, y) => x.CL_LineNumber.CompareTo(y.CL_LineNumber));
			foreach (CusEntryLine entryLine in mergedLines)
			{
				totalTAndI = dec.CustomsEntryHeaders[0].CurrencyConverter.Add(totalTAndI, entryLine.TransportAndInsuranceForMessage);
			}

			AssertEquals(totalTAndI.Amount, dec.CustomsEntryHeaders[0].TransportAndInsuranceForMessage.Amount);
		}

		protected override void AssertDTAF(Xsd.Consol toConsol)
		{
			AssertEquals("MYPKG", toConsol.ConsolDetail.PortFirstArrival.Port.Value);
		}

		protected override void MergeAndSetupCustomsCharges(BaseJobDeclaration declaration)
		{
			base.MergeAndSetupCustomsCharges(declaration);
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CustomsEntryHeaders[0];
			entryHeader.Charges.AddNew().C1_ChargeAmount = 123.45m;
		}

		protected override Type InvoicesGeneratorType => typeof(AUInvoicesGeneratorFromXSD);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			JobDeclaration result = (JobDeclaration)base.GetJobDeclaration();
			result.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			result.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			result.DisableDefaultPackingInformation = true;
			result.ApportionmentDirty = false;
			return result;
		}

		protected override ValueObjectDataAdapter<BaseJobDeclaration, Xsd.ConsolAndShipment> GetNewBizObjXmlDataAdapter()
		{
			return new AUDeclarationValueObjectDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			string outputFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUEmptyDeclaration.xml");
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyJobDeclaration(), outputFileFullPath, ValidationKind.None, "Empty Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			SetupLine1Tariff();
			string outputFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUPopulatedDeclaration.xml");
			TestJobDec.ResumeApportionment();
			return new BusinessObjectAndExpectedOutputFileName(TestJobDec, outputFileFullPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, UseCustomsReferenceDataValue);
			enableCWRefForAHECCRegItem = AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EnableCWRefForAHECCValue);
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
			useCustomsReferenceDataRegItem?.Dispose();
			enableCWRefForAHECCRegItem?.Dispose();
			distributeByForExport?.Dispose();
		}

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(AUDeclarationValueObjectDataAdapterTest)));
		TestFileReader fileReader;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		IDisposable useCustomsReferenceDataRegItem;
		IDisposable enableCWRefForAHECCRegItem;
		IDisposable distributeByForExport;
		protected virtual bool UseCustomsReferenceDataValue => true;
		protected virtual bool EnableCWRefForAHECCValue => true;

		void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency, string exRateType)
		{
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, Env.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exRateType);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);
			var exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty == null)
			{
				exchangeRateDuty = Factory.New<RefExchangeRate>();
				exchangeRateDuty.RE_ExpiryDate = endDate;
				exchangeRateDuty.RE_ExRateType = exRateType;
				exchangeRateDuty.RE_GC = Env.CurrentCompany.PK;
				exchangeRateDuty.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				exchangeRateDuty.RE_StartDate = startDate;
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}
			else
			{
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}

			Factory.Save();
		}

		protected virtual void SetupLine1Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "01051202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST"
				, description: "Turkeys weighing not more than 185g");
			helper.CreateTariffUOM(tariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NO");
		}

		protected override string Line1TariffCode => "0105.12.02";

		const string TestFilesPath = "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.TestFiles";

		sealed class TestAUDeclarationValueObjectDataAdapter : AUDeclarationValueObjectDataAdapter
		{
			public new InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(BaseJobDeclaration jobDec)
			{
				return base.GetNewInvoiceAdapter(jobDec);
			}

			public new void SetShipmentTypeDetailsCore(BaseJobDeclaration jobDec)
			{
				base.SetShipmentTypeDetailsCore(jobDec);
			}

			public new void ImportDeclarationDetails(BaseJobDeclaration jobDec, Xsd.Declaration declarationXsd, ZString consolMasterBill, IValueObjectImportContext context)
			{
				base.ImportDeclarationDetails(jobDec, declarationXsd, consolMasterBill, context);
			}

			public new void ImportFromValueObjectCore(BaseJobDeclaration bizObj, Xsd.ConsolAndShipment value, IValueObjectImportContext context)
			{
				base.ImportFromValueObjectCore(bizObj, value, context);
			}

			public void ExportCustomsEntries(Xsd.CustomsEntryCollection customsEntries, ICusEntryHeaderCollection<CusEntryHeader> customsEntryHeaders, INotifications notify)
			{
				ExportEntryHeaders(customsEntries, customsEntryHeaders, notify);
			}

			public new void ExportOrders(Xsd.Shipment shipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				base.ExportOrders(shipment, jobDec, context);
			}

			public new InvoicesGeneratorFromXSD GetNewInvoicesGenerator(BaseJobDeclaration jobDec)
			{
				return base.GetNewInvoicesGenerator(jobDec);
			}
		}
	}
}
