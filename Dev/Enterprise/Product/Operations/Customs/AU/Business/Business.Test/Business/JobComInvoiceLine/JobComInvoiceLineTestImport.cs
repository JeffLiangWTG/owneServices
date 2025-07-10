using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineTestImport : JobComInvoiceLineTest
	{
		public void TestTariffFormatter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var formatProvider = (Common.ITariffFormatProvider)invoiceLine;
			AssertType<AUImportTariffUniversalFormatter>(formatProvider.TariffFormatter);
		}

		public void TestTariffNumberIsSavedDotFormatted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				invoiceLine.JI_Tariff = "2203003115";
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var invoiceLineIOF = otherFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
				AssertEquals("2203.00.31 15", invoiceLineIOF["JI_Tariff"]);
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				invoiceLine.JI_Tariff = "2203003115";
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var invoiceLineIOF = otherFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
				AssertEquals("2203.00.31 15", invoiceLineIOF["JI_Tariff"]);
			}
		}

		public void TestNoExceptionWhenMergeOutputsAreAccessedDuringMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			AssertNoExceptionThrown(() => declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_CLInfo.ValueChanged += new EventHandler(delegate
			{ ZDecimal gstAccessed = invoiceLine2.JI_Calc_GSTVATAmount; });

			AssertNoExceptionThrown(() => declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
		}

		public void TestClassificationDetailsForGenericWrapperCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "4901100001";
			invoiceLine1.AddInfo.ZA_TreatmentCode_Hidden = "504";
			invoiceLine1.AddInfo.ZA_InstrumentType_Hidden = "BL";
			invoiceLine1.AddInfo.ZA_InstrumentCode_Hidden = "012456";

			AssertEquals("4901.10.00 01 / 504 / BL 012456", invoiceLine1.ClassificationDetailsForGenericWrapper);

			invoiceLine1.AddInfo.TCI_InstrumentType = "TCO";
			invoiceLine1.AddInfo.TCI_InstrumentNo = "56789";
			AssertEquals("4901.10.00 01 / 504 / BL 012456 / TCO 56789", invoiceLine1.ClassificationDetailsForGenericWrapper);
		}

		#region AQIS Documents

		public void TestAQISDocuments()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("No values in collection", 0, invoiceLine.AQISDocuments.Count);

			AQISDocument document = invoiceLine.AQISDocuments.AddNew();
			document.Number = "1234";
			document.Type = "Type";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Type/1234", invoiceLine.AddInfo.ZA_AQISDocuments_Hidden);
		}

		public void TestAQISDocumentsWithOneValueInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISDocuments_Hidden = "TT1/Num1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISDocuments.Count);

			AQISDocument document = invoiceLine.AQISDocuments.AddNew();
			document.Type = "TT2";
			document.Number = "Num2";
			AssertEquals("Two values in the collection", 2, invoiceLine.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
		}

		public void TestAQISDocumentsWithMultipleValuesInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISDocuments_Hidden = "TT1/Num1,TT2/Num2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISDocuments.Count);

			AQISDocument document = invoiceLine.AQISDocuments.AddNew();
			document.Type = "TT3";
			document.Number = "Num3";
			AssertEquals("Two values in the collection", 3, invoiceLine.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT3/Num3"));
		}

		public void TestAggreatedAQISDocuments()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Count", 0, invoiceLine.AggreatedAQISDocuments.Count);

			AQISDocument decDocument = declaration.AQISDocuments.AddNew();
			decDocument.Number = "Num1";
			decDocument.Type = "TT1";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISDocuments.Count);
			AssertEquals("Number", "Num1", invoiceLine.AggreatedAQISDocuments[0].Number);
			AssertEquals("Type", "TT1", invoiceLine.AggreatedAQISDocuments[0].Type);

			AQISDocument headerDocument = invoiceHeader.AQISDocuments.AddNew();
			headerDocument.Number = "Num2";
			headerDocument.Type = "TT2";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISDocuments.Count);
			AssertEquals("Number", "Num2", invoiceLine.AggreatedAQISDocuments[0].Number);
			AssertEquals("Type", "TT2", invoiceLine.AggreatedAQISDocuments[0].Type);

			AQISDocument lineDocument = invoiceLine.AQISDocuments.AddNew();
			lineDocument.Number = "Num3";
			lineDocument.Type = "TT3";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISDocuments.Count);
			AssertEquals("Number", "Num3", invoiceLine.AggreatedAQISDocuments[0].Number);
			AssertEquals("Type", "TT3", invoiceLine.AggreatedAQISDocuments[0].Type);
		}

		#endregion

		#region AQIS Premises Id And Processing Types

		public void TestAQISPremisesIdAndProcessingTypes()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("No values in collection", 0, invoiceLine.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem1";
			premisesIdAndProcessingType.ProcessingType = "Process";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Prem1/Process", invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden);
		}

		public void TestAQISPremisesIdAndProcessingTypesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem1/Process1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem2";
			premisesIdAndProcessingType.ProcessingType = "Process2";
			AssertEquals("Two values in the collection", 2, invoiceLine.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
		}

		public void TestAQISPremisesIdAndProcessingTypesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem1/Process1,Prem2/Process2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem3";
			premisesIdAndProcessingType.ProcessingType = "Process3";
			AssertEquals("Two values in the collection", 3, invoiceLine.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem3/Process3"));
		}

		public void TestAggreatedAQISPremisesIdAndProcessingTypes()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Count", 0, invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType decPremPocess = declaration.AQISPremisesIdAndProcessingTypes.AddNew();
			decPremPocess.PremisesId = "Pre1";
			decPremPocess.ProcessingType = "Proc1";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes.Count);
			AssertEquals("Premises", "Pre1", invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes[0].PremisesId);
			AssertEquals("Processing", "Proc1", invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes[0].ProcessingType);

			AQISPremisesIdAndProcessingType headerPremPocess = invoiceHeader.AQISPremisesIdAndProcessingTypes.AddNew();
			headerPremPocess.PremisesId = "Pre2";
			headerPremPocess.ProcessingType = "Proc2";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes.Count);
			AssertEquals("Number", "Pre2", invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes[0].PremisesId);
			AssertEquals("Type", "Proc2", invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes[0].ProcessingType);

			AQISPremisesIdAndProcessingType linePremPocess = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			linePremPocess.PremisesId = "Pre3";
			linePremPocess.ProcessingType = "Proc3";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes.Count);
			AssertEquals("Number", "Pre3", invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes[0].PremisesId);
			AssertEquals("Type", "Proc3", invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes[0].ProcessingType);
		}

		#endregion

		#region AQIS Commodity Codes

		public void TestAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("No values in collection", 0, invoiceLine.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode.Code = "1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "1", invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden = "1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode.Code = "2";
			AssertEquals("Two values in the collection", 2, invoiceLine.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
		}

		public void TestAQISCommodityCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			AssertEquals("Two values in the collection", 3, invoiceLine.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.Contains("3"));
		}

		public void TestReBuildAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			invoiceLine.AddInfo.ReBuildAQISCommodityCodes();
			AssertEquals("Add Info Value", "1,2,3", invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAggreatedAQISCommodityCodes()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Count", 0, invoiceLine.AggreatedAQISCommodityCodes.Count);

			AQISCommodityCode decCommodity = declaration.AQISCommodityCodes.AddNew();
			decCommodity.Code = "C1";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISCommodityCodes.Count);
			AssertEquals("Code", "C1", invoiceLine.AggreatedAQISCommodityCodes[0].Code);

			AQISCommodityCode headerCommodity = invoiceHeader.AQISCommodityCodes.AddNew();
			headerCommodity.Code = "C2";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISCommodityCodes.Count);
			AssertEquals("Code", "C2", invoiceLine.AggreatedAQISCommodityCodes[0].Code);

			AQISCommodityCode lineCommodity = invoiceLine.AQISCommodityCodes.AddNew();
			lineCommodity.Code = "C3";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISCommodityCodes.Count);
			AssertEquals("Code", "C3", invoiceLine.AggreatedAQISCommodityCodes[0].Code);
		}

		#endregion

		#region AQIS Entity Ids

		public void TestAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("No values in collection", 0, invoiceLine.AQISEntityIds.Count);

			AQISEntityId entityId = invoiceLine.AQISEntityIds.AddNew();
			entityId.Code = "Code1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIdsWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISEntityIds.Count);

			AQISEntityId entityId = invoiceLine.AQISEntityIds.AddNew();
			entityId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, invoiceLine.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
		}

		public void TestAQISEntityIdsWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISEntityIds.Count);

			AQISEntityId entityId = invoiceLine.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, invoiceLine.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISEntityIds.Count);

			AQISEntityId entityId = invoiceLine.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			invoiceLine.AddInfo.ReBuildAQISEntityIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAggreatedAQISEntityIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Count", 0, invoiceLine.AggreatedAQISEntityIds.Count);

			AQISEntityId decEntity = declaration.AQISEntityIds.AddNew();
			decEntity.Code = "Code1";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISEntityIds.Count);
			AssertEquals("Code", "Code1", invoiceLine.AggreatedAQISEntityIds[0].Code);

			AQISEntityId headerEntity = invoiceHeader.AQISEntityIds.AddNew();
			headerEntity.Code = "Code2";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISEntityIds.Count);
			AssertEquals("Code", "Code2", invoiceLine.AggreatedAQISEntityIds[0].Code);

			AQISEntityId lineEntity = invoiceLine.AQISEntityIds.AddNew();
			lineEntity.Code = "Code3";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISEntityIds.Count);
			AssertEquals("Code", "Code3", invoiceLine.AggreatedAQISEntityIds[0].Code);
		}

		#endregion

		#region AQIS Permit Ids

		public void TestAQISPermitIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("No values in collection", 0, invoiceLine.AQISPermitIds.Count);

			AQISPermitId permitId = invoiceLine.AQISPermitIds.AddNew();
			permitId.Code = "Code1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAQISPermitIdsWithOneValueInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISPermitIds.Count);

			AQISPermitId permitId = invoiceLine.AQISPermitIds.AddNew();
			permitId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, invoiceLine.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
		}

		public void TestAQISPermitIdsWithMultipleValuesInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISPermitIds.Count);

			AQISPermitId permitId = invoiceLine.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, invoiceLine.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISPermitIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISPermitIds.Count);

			AQISPermitId permitId = invoiceLine.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			invoiceLine.AddInfo.ReBuildAQISPermitIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAggreatedAQISPermitIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Count", 0, invoiceLine.AggreatedAQISPermitIds.Count);

			AQISPermitId decPermit = declaration.AQISPermitIds.AddNew();
			decPermit.Code = "Code1";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISPermitIds.Count);
			AssertEquals("Code", "Code1", invoiceLine.AggreatedAQISPermitIds[0].Code);

			AQISPermitId headerPermit = invoiceHeader.AQISPermitIds.AddNew();
			headerPermit.Code = "Code2";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISPermitIds.Count);
			AssertEquals("Code", "Code2", invoiceLine.AggreatedAQISPermitIds[0].Code);

			AQISPermitId linePermit = invoiceLine.AQISPermitIds.AddNew();
			linePermit.Code = "Code3";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISPermitIds.Count);
			AssertEquals("Code", "Code3", invoiceLine.AggreatedAQISPermitIds[0].Code);
		}

		#endregion

		#region AQIS Producer Codes

		public void TestAQISProducerCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("No values in collection", 0, invoiceLine.AQISProducerCodes.Count);

			AQISProducerCode producerCode = invoiceLine.AQISProducerCodes.AddNew();
			producerCode.Code = "Code1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, invoiceLine.AQISProducerCodes.Count);

			AQISProducerCode producerCode = invoiceLine.AQISProducerCodes.AddNew();
			producerCode.Code = "Code2";
			AssertEquals("Two values in the collection", 2, invoiceLine.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
		}

		public void TestAQISProducerCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISProducerCodes.Count);

			AQISProducerCode producerCode = invoiceLine.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			AssertEquals("Two values in the collection", 3, invoiceLine.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISProcducerCode()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceLine.AQISProducerCodes.Count);

			AQISProducerCode producerCode = invoiceLine.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			invoiceLine.AddInfo.ReBuildAQISProducerCodes();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAggreatedAQISProducerCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Count", 0, invoiceLine.AggreatedAQISProducerCodes.Count);

			AQISProducerCode decProducer = declaration.AQISProducerCodes.AddNew();
			decProducer.Code = "Code1";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISProducerCodes.Count);
			AssertEquals("Code", "Code1", invoiceLine.AggreatedAQISProducerCodes[0].Code);

			AQISProducerCode headerProducer = invoiceHeader.AQISProducerCodes.AddNew();
			headerProducer.Code = "Code2";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISProducerCodes.Count);
			AssertEquals("Code", "Code2", invoiceLine.AggreatedAQISProducerCodes[0].Code);

			AQISProducerCode lineProducer = invoiceLine.AQISProducerCodes.AddNew();
			lineProducer.Code = "Code3";
			AssertEquals("Count", 1, invoiceLine.AggreatedAQISProducerCodes.Count);
			AssertEquals("Code", "Code3", invoiceLine.AggreatedAQISProducerCodes[0].Code);
		}

		#endregion

		public void TestTransportAndInsuranceInLocalCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.5m, helper.USDCurrency);

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "USD");

			AssertEquals("TransportAndInsurance Amount", 100m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("TransportAndInsurance Currency", "USD", invoiceLine.TransportAndInsurance.Currency.Code);
			AssertEquals("TransportAndInsurance in local currency", 200m, invoiceLine.TransportAndInsuranceInLocalCurrency);
		}

		public void TestJI_PriceAdjustmentInLocalCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.5m, helper.USDCurrency);

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.AdjustmentDollarPercentage_Hidden = "$";
			invoiceLine.AddInfo.ZA_ADJ = "100USD";

			AssertEquals("JI_PriceAdjustment Amount", 100m, invoiceLine.JI_PriceAdjustment.Amount);
			AssertEquals("JI_PriceAdjustment Currency", "USD", invoiceLine.JI_PriceAdjustment.Currency.Code);
			AssertEquals("JI_PriceAdjustment in local currency", 200m, invoiceLine.JI_PriceAdjustmentInLocalCurrency);
		}

		[ExpectNoExceptions]
		public void TestProportionOfCusEntryLineForNormalised()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = "USD";

			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1000m;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_CustomsValue = 0m;

			AssertEquals("Should be normalised", true, entryHeader.ShouldEntryBeNormalised);
			ZDecimal proportionOfDutyAccessed = invoiceLine2.JI_Calc_DutyAmount;
		}

		public void TestProportionOfEntryLine()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.AddInfo.ZA_ADJ = "20AUD";

			invoice.Charges.AddNew(Core.Constants.Customs.CustomsCharges.Codes.OverseasInsurance, 300m);

			AssertNull("invoice line is not link to any cusentryline", invoiceLine.CusEntryLine);
			AssertEquals("T&I value from customs", 0m, invoiceLine.JI_Calc_CustomsTransportAndInsuranceInLocalCurrency);

			testDec.DoMerge();

			testDec.CustomsEntryHeaders[0].MergedLines[0].Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 100m;

			AssertNotNull("invoice line is linked to any cusentryline", invoiceLine.CusEntryLine);
			AssertEquals("Duty apportioned Amount for invoice line", 100m, invoiceLine.JI_Calc_DutyAmount);

			AssertEquals("VOTI apportioned Amount for invoice lien", 620m, invoiceLine.JI_Calc_VOTI);

			AssertEquals("T&I apportioned Amount for Invoice line", 300m, invoiceLine.JI_Calc_CustomsTransportAndInsuranceInLocalCurrency);
		}

		public void TestAggregatedZA_PST()
		{
			Header.AddInfo.ZA_PST = "CC";
			Header.AddInfo.ZA_POC = "DD";
			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated Preference Schem", "CC", line.AggregatedZA_PST);

			line.AddInfo.ZA_POC = "BB";
			AssertEquals("Aggregated Preference Schem", "", line.AggregatedZA_PST);

			line.AddInfo.ZA_PST = "AA";
			AssertEquals("Aggregated Preference Schem", "AA", line.AggregatedZA_PST);
		}

		public void TestAggregatedZA_POC()
		{
			Header.AddInfo.ZA_PST = "CC";
			Header.AddInfo.ZA_POC = "DD";
			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated Preference Scheme", "DD", line.AggregatedZA_POC);

			line.AddInfo.ZA_PST = "BB";
			AssertEquals("Aggregated Preference Scheme", "", line.AggregatedZA_POC);

			line.AddInfo.ZA_POC = "AA";
			AssertEquals("Aggregated Preference Scheme", "AA", line.AggregatedZA_POC);
		}

		public void TestAggregatedZA_PRT()
		{
			Header.AddInfo.ZA_PST = "CC";
			Header.AddInfo.ZA_PRT = "DD";
			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated Preference Schem", "DD", line.AggregatedZA_PRT);

			line.AddInfo.ZA_PST = "BB";
			AssertEquals("Aggregated Preference Schem", "", line.AggregatedZA_PRT);

			line.AddInfo.ZA_PRT = "AA";
			AssertEquals("Aggregated Preference Schem", "AA", line.AggregatedZA_PRT);
		}

		public void TestAggregatedZA_GSTE()
		{
			Header.AddInfo.ZA_GSTE = "FOOD";
			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated GSTE", "FOOD", line.AggregatedZA_GSTE);

			line.AddInfo.ZA_GSTE = "ELSE";
			AssertEquals("Aggregated GSTE", "ELSE", line.AggregatedZA_GSTE);
		}

		public void TestJI_DiscountForIncludedInLines()
		{
			Header.JZ_InvoiceAmount = 10000m;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			InvoiceCharge discount = Header.Charges.AddNew();
			discount.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			discount.J7_Amount = 500m;
			discount.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			discount.J7_IsIncludedInITOT = true;

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000;

			AssertEquals("Discount from Line: zero as it is already included in lines", 0m, line.JI_Discount.Amount);
		}

		public void TestJI_DiscountForExcludedFromLines()
		{
			Header.JZ_InvoiceAmount = 10000m;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			InvoiceCharge discount = Header.Charges.AddNew();
			discount.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			discount.J7_Amount = 500m;
			discount.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			discount.J7_IsIncludedInITOT = false;

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10500;
			JobDec.ResumeApportionment();
			AssertEquals("Discount from Line", 500m, line.JI_Discount.Amount);
		}

		public void TestJI_Commission()
		{
			Header.JZ_InvoiceAmount = 10000m;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			InvoiceCharge dutyCommission = Header.Charges.AddNew();
			dutyCommission.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			dutyCommission.J7_Amount = 500m;
			dutyCommission.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			dutyCommission.J7_IsIncludedInITOT = false;

			InvoiceCharge nonDutyCommission = Header.Charges.AddNew();
			nonDutyCommission.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			nonDutyCommission.J7_Amount = 200m;
			nonDutyCommission.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			nonDutyCommission.J7_IsIncludedInITOT = true;
			nonDutyCommission.J7_IsDutiable = false;
			nonDutyCommission.J7_IsGSTApplicable = false;

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 9500;
			JobDec.ResumeApportionment();
			AssertEquals("Commission from Line", 300m, line.JI_Commission.Amount);
		}

		public void TestJI_OverseasFreightWhenTwoAmountsAreInForeignCurrencies()
		{
			Header.JobDeclaration.JE_ExportDate = ZDateTime.Today;
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			RefCurrency gBP = RefCurrency.LoadFromCurrencyCode(Factory, "GBP");

			Header.JZ_InvoiceAmount = 18625;
			Header.JZ_RX_NKInvoice_Currency = gBP.RX_Code;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;

			BaseJobComInvHeaderCharge fIFT = Header.GroupHeader.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 362.50m, gBP.RX_Code);
			fIFT.J7_IsDutiable = false;

			new ZTestHelper(Factory).SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 0.7010m, uSD);
			new ZTestHelper(Factory).SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 0.3910m, gBP);
			Header.GroupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 1525.00m, uSD.RX_Code);
			Header.GroupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 49.60m, uSD.RX_Code);

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 18625m;
			JobDec.ResumeApportionment();
			AssertEquals("FIFT is zero as this is non-dutiable", 0m, line.JI_ForeignInlandFreight.Amount);
			AssertEquals("FIFT should be in GBP", gBP.RX_Code, line.JI_ForeignInlandFreight.Currency.Code);
			AssertEquals("OverseasFreight should be in USD", uSD.RX_Code, line.JI_OverseasFreight.Currency.Code);
		}

		public void TestNonDutiableChargeGoesIntoFreightAndDeductibleOthercharge()
		{
			Header.JZ_InvoiceAmount = 1000;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			BaseJobComInvHeaderCharge nonOTH = Header.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 100);
			nonOTH.J7_IsDutiable = false;
			nonOTH.J7_IsIncludedInITOT = true;

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;
			JobDec.ResumeApportionment();
			AssertEquals("Other charge2 has non-dutiable portion", 100m, line.JI_OtherCharges2.Amount);
			AssertEquals("Overseas Freight does not have non-dutiable portion any more", 0m, line.JI_OverseasFreight.Amount);

			nonOTH.J7_IsIncludedInITOT = false;
			line.JI_LinePrice = 900m;
			JobDec.ResumeApportionment();
			AssertEquals("Deduction doesnt get send as this amount is not included in ITOT", 0m, line.JI_OtherCharges2.Amount);
			AssertEquals("PreCOndition:GST applicable", true, nonOTH.J7_IsGSTApplicable);
			AssertEquals("Overseas Freight does not included non-dutiable any more", 0m, line.JI_OverseasFreight.Amount);
		}

		public void TestDeductionCharge()
		{
			Header.JZ_InvoiceAmount = 1000;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			BaseJobComInvHeaderCharge deduction = Header.Charges.AddNew(AUChargeCodeList.Codes.DeductionCharge, 100);
			JobDec.ResumeApportionment();
			AssertEquals("PreCondition:Deduction is included in ITOT", true, deduction.J7_IsIncludedInITOT);
			AssertEquals("PreCondition:Deduction is Not Dutiable", false, deduction.J7_IsDutiable);
			AssertEquals("PreCondition:Deduction'GST readonly", true, deduction.J7_IsGSTApplicableInfo.ReadOnly);

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;
			JobDec.ResumeApportionment();
			AssertEquals("Other charge2 as deduction", 100m, line.JI_OtherCharges2.Amount);
			AssertEquals("Overseas Freight does not have deduction anymore as deduction is not gst-applicable all the time", 0m, line.JI_OverseasFreight.Amount);
		}

		public void TestAdditionCharge()
		{
			Header.JZ_InvoiceAmount = 1000;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			BaseJobComInvHeaderCharge addition = Header.Charges.AddNew(AUChargeCodeList.Codes.AdditionCharge, 100);
			JobDec.ResumeApportionment();
			AssertEquals("PreCondition:AdditionCharge is not included in ITOT", false, addition.J7_IsIncludedInITOT);
			AssertEquals("PreCondition:AdditionCharge is Dutiable", true, addition.J7_IsDutiable);
			AssertEquals("PreCondition:AdditionCharge is GSTible", true, addition.J7_IsGSTApplicable);

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 900m;
			JobDec.ResumeApportionment();
			AssertEquals("Other charge1 as addition", 100m, line.JI_OtherCharges1.Amount);
		}

		public void TestDeductionGroupCharge()
		{
			JobComInvoiceGroupHeader groupHeader = Header.Master as JobComInvoiceGroupHeader;
			Header.JZ_InvoiceAmount = 1000;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			BaseJobComInvHeaderCharge deduction = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.DeductionCharge, 100, JobDeclaration.LocalCurrencyConstantCode);
			JobDec.ResumeApportionment();
			AssertEquals("PreCondition:Deduction is included in ITOT", false, Header.GroupCharges[0].J7_IsIncludedInITOT);
			AssertEquals("PreCondition:Deduction is Not Dutiable", false, Header.GroupCharges[0].J7_IsDutiable);
			AssertEquals("PreCondition:Deduction's GST is readonly", true, Header.GroupCharges[0].J7_IsGSTApplicableInfo.ReadOnly);
			AssertEquals("PreCondition:Deduction is GST applicable", false, Header.GroupCharges[0].J7_IsGSTApplicable);

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;
			JobDec.ResumeApportionment();
			AssertEquals("Other charge2 as deduction", 0m, line.JI_OtherCharges2.Amount);
			AssertEquals("Deduction is not part of OFT anymore as it is not GST-applicable all the time", 0m, line.JI_OverseasFreight.Amount);
		}

		public void TestAdditionGroupCharge()
		{
			JobComInvoiceGroupHeader groupHeader = Header.Master as JobComInvoiceGroupHeader;
			Header.JZ_InvoiceAmount = 1000;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			BaseJobComInvHeaderCharge addition = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.AdditionCharge, 100, JobDeclaration.LocalCurrencyConstantCode);
			JobDec.ResumeApportionment();
			AssertEquals("PreCondition:AdditionCharge is not included in ITOT", false, Header.GroupCharges[0].J7_IsIncludedInITOT);
			AssertEquals("PreCondition:AdditionCharge is Dutiable", true, Header.GroupCharges[0].J7_IsDutiable);
			AssertEquals("PreCondition:AdditionCharge is GSTible", true, Header.GroupCharges[0].J7_IsGSTApplicable);

			JobComInvoiceLine line = Header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;
			JobDec.ResumeApportionment();
			AssertEquals("Other charge1 as addition", 100m, line.JI_OtherCharges1.Amount);
		}

		JobDeclaration SetupRecordsForParentTrailer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			return declaration;
		}

		public void TestParentLineCode()
		{
			JobDeclaration declaration = SetupRecordsForParentTrailer();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			AssertEquals("Trailer line's ParentLine", line1.JI_LineNo + "/" + line2.JI_Calc_Invoice, line2.JI_ParentLineCode);
		}

		public void TestSettingParentSetsNextLineAsTrailer()
		{
			JobDeclaration declaration = SetupRecordsForParentTrailer();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;

			AssertEquals("Line2's prefix", JobComInvoiceLine.LinePrefixString.Trailer, line2.JI_LinePrefix);
			Assert("Line1 has non-null child line", line1.ChildLine != null);
			Assert("Line2 has non-null parent line", line2.ParentLine != null);
		}

		public void TestSettingTrailerToDifferentInvoices()
		{
			JobDeclaration declaration = SetupRecordsForParentTrailer();
			JobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV 2";

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];
			line2.JI_Calc_Invoice = invoiceHeader2.JZ_InvoiceNumber;

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			line2.RunPreSaveValidation();

			AssertEquals("PreCondition:Line2's prefix", JobComInvoiceLine.LinePrefixString.Trailer, line2.JI_LinePrefix);
			AssertEquals("Parent & Trailer belongs to different invoices", true, line2.JI_Calc_InvoiceInfo.HasMessageErrors());

			line2.JI_Calc_Invoice = line1.JI_Calc_Invoice;
			AssertEquals("Parent & Trailer belongs to same invoices now", false, line2.JI_Calc_InvoiceInfo.HasMessageErrors());
		}

		public void TestSettingTrailerWithLinePrice()
		{
			JobDeclaration declaration = SetupRecordsForParentTrailer();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];

			line1.JI_LinePrice = 1000m;
			line2.JI_LinePrice = 1000m;

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			line2.RunPreSaveValidation();
			AssertEquals("Line2's prefix", JobComInvoiceLine.LinePrefixString.Trailer, line2.JI_LinePrefix);
			AssertEquals("Line2's line price has a message error", true, line2.JI_LinePriceInfo.HasMessageErrors());

			line2.JI_LinePrice = 0m;
			AssertEquals("Line2's line price has a message error", false, line2.JI_LinePriceInfo.HasMessageErrors());
		}

		public void TestValidateParentTrailerPairs()
		{
			JobDeclaration declaration = SetupRecordsForParentTrailer();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines.AddNew();
			line3.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;

			declaration.RunPreSaveValidation();
			AssertEquals("Line3 doesnt have corresponding trailer", true, line3.JI_LinePrefixInfo.HasMessageErrors());
		}

		public void TestLinePriceEditibleWhenTrailerBackToNormal()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;

			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;
			AssertEquals("PreCondition:Line2 should be a trailer line", JobComInvoiceLine.LinePrefixString.Trailer, line2.JI_LinePrefix);

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Normal;
			AssertEquals("Line2's line price", false, line2.JI_LinePriceInfo.ReadOnly);
		}

		public void TestAggregatedZA_Org()
		{
			Header.AddInfo.ZA_ORG = "";
			Header.AddInfo.ZA_PRF = "";
			Line.AddInfo.ZA_ORG = "";
			Line.AddInfo.ZA_PRF = "";

			AssertEquals(ZString.Empty, Line.AggregatedZA_ORG);
			Header.AddInfo.ZA_ORG = "GB";
			AssertEquals("GB", Line.AggregatedZA_ORG);
			Header.AddInfo.ZA_ORG = "";
			Line.AddInfo.ZA_ORG = "US";
			AssertEquals("US", Line.AggregatedZA_ORG);
		}

		public void TestAggregatedZA_PRF()
		{
			Header.AddInfo.ZA_ORG = "";
			Header.AddInfo.ZA_PRF = "";
			Line.AddInfo.ZA_ORG = "";
			Line.AddInfo.ZA_PRF = "";

			AssertEquals(ZString.Empty, Line.AggregatedZA_PRF);
			Header.AddInfo.ZA_PRF = "X";
			AssertEquals("X", Line.AggregatedZA_PRF);
			Header.AddInfo.ZA_PRF = "";
			Line.AddInfo.ZA_PRF = "X";
			AssertEquals("X", Line.AggregatedZA_PRF);
		}

		public void TestInstrumentTypeAndConcessionOrder()
		{
			Line.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Line.InstrumentType = "MD1";
			Line.InstrumentCode = "1234567";
			AssertEquals("Concession Order", "MD1|1234567", Line.JI_ConcessionOrder);
		}

		[ExpectNoExceptions]
		public void TestSettingJI_InvoiceQuantityDoesNotCallItsSelf()
		{
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			AUOrgSupplierPart beerProduct = Factory.New<AUOrgSupplierPart>();
			beerProduct.OP_PartNum = "BEER";
			beerProduct.OP_StockKeepingUnit = "CTN";
			var beerPivot = beerProduct.PivotsForBinding.AddNew();
			beerPivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			beerPivot.CI_CC = BeerClass.PK;
			OrgPartRelation supplier = beerProduct.RelatedOrganisations.AddNew();
			supplier.OU_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			OrgPartUnit unit1 = beerProduct.PartUnits.AddNew();
			unit1.OF_QuantityInParent = 27.3m;
			unit1.OF_PackType = "BOT";
			unit1.OF_ParentPackType = "CTN";

			OrgPartUnit unit2 = beerProduct.PartUnits.AddNew();
			unit2.OF_QuantityInParent = 3.665m;
			unit2.OF_PackType = "L";
			unit2.OF_ParentPackType = "BOT";

			OrgPartUnit unit3 = beerProduct.PartUnits.AddNew();
			unit3.OF_QuantityInParent = 0.013m;
			unit3.OF_PackType = "LA";
			unit3.OF_ParentPackType = "L";
			Factory.Save();

			Header.JZ_OH_Supplier = supplier.OU_OH;
			Line.JI_PartNo = "BEER";
			Line.JI_InvoiceQuantity = 23.9m;
		}

		public void TestJI_LinePrefix()
		{
			Line.JI_LinePrefix = "P";
			Assert("Line prefix should be stored in AddInfo", Line.AddInfo.ToString().IndexOf("LinePrefix_Hidden=P") >= 0);
		}

		public void TestPriceAdjustmentWithPlus()
		{
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			Line.JI_LinePrice = 1000m;
			Line.AddInfo.ZA_ADJ = "+100.00AUD";

			AssertEquals("Price adjustment", 100m, Line.JI_PriceAdjustment.Amount);
			AssertEquals("Price Adjustment Currency", AUDCurrency.RX_Code, Line.JI_PriceAdjustment.Currency.Code);
		}

		public void TestPriceAdjustmentWithSameCurrency()
		{
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			Line.JI_LinePrice = 1000m;
			Line.AddInfo.ZA_ADJ = "-100.00AUD";

			AssertEquals("Price adjustment", -100m, Line.JI_PriceAdjustment.Amount);
			AssertEquals("Price Adjustment Currency", AUDCurrency.RX_Code, Line.JI_PriceAdjustment.Currency.Code);
		}

		public void TestPriceAdjustmentWithPercentage()
		{
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			Line.JI_LinePrice = 1000m;
			Line.AddInfo.ZA_ADJ = "10%";

			AssertEquals("Price after adjustment", 100m, Line.JI_PriceAdjustment.Amount);
			AssertEquals("Price adjustment currency", AUDCurrency.RX_Code, Line.JI_PriceAdjustment.Currency.Code);
		}

		public void TestJI_LCT()
		{
			Line.AddInfo.ZA_LCT = 123456;
			AssertEquals("LCT in line", 123456m, Line.JI_LCT.Amount);
			AssertEquals("LCT currency", JobDeclaration.LocalCurrencyConstantCode, Line.JI_LCT.Currency.Code);
		}

		public void TestJI_ODF()
		{
			Line.AddInfo.ZA_ODF = 123456;
			AssertEquals("ODF in line", 123456m, Line.JI_OtherDutyFactor.Amount);
			AssertEquals("ODF currency", JobDeclaration.LocalCurrencyConstantCode, Line.JI_OtherDutyFactor.Currency.Code);
		}

		public void TestJI_STD()
		{
			Line.AddInfo.ZA_STD = 123456;
			AssertEquals("STD in line", 123456m, Line.JI_StandardDuty.Amount);
			AssertEquals("STD currency", JobDeclaration.LocalCurrencyConstantCode, Line.JI_StandardDuty.Currency.Code);
		}

		public void TestAddInfo()
		{
			string testAddInfoLine = "DTY=23.24*AMB=DVTQPOC*LCTE=404*ICN=C23000H";
			JobComInvoiceLine line = Factory.New<JobComInvoiceLine>();
			line.JI_AddInfo = testAddInfoLine;
			AssertEquals("Declaration AddInfo Object Field - Duty", 23.24m, line.AddInfo.ZA_DTY);
			AssertEquals("Declaration AddInfo Object Field - Amber Processing", "DVTQPOC", line.AddInfo.ZA_AMB);
			AssertEquals("Declaration AddInfo Object Field - Luxury Car Tax Exemption", "404", line.AddInfo.ZA_LCTE);
			AssertEquals("Declaration AddInfo Object Field - Import Credit Number", "C23000H", line.AddInfo.ZA_ICN);
			AssertEquals("Declaration AddInfo Object Field - Origin", "", line.AddInfo.ZA_ORG);
			AssertEquals("Declaration AddInfo Object Field - Invoice Spirit Strength", 0m, line.AddInfo.ZA_ISS);
		}

		public void TestAggregateAddInfo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceGroupHeader group1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceHeader header2 = group1.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header2.JobComInvoiceLines.AddNew();
			declaration.JE_RL_NKOrigin = "JPTKY";
			header1.JZ_AddInfo = "RNO=01";
			group1.JZ_AddInfo = "ORG=USA";
			header2.JZ_AddInfo = "";
			line1.JI_AddInfo = "";
			line2.JI_AddInfo = "LCT=1231.23";
			AssertEquals("Line AddInfo Aggregation", "USA", line2.AddInfo.AggregatedZA_ORG);
			AssertEquals("Line AddInfo Aggregation", 1231.23m, (ZDecimal)line2.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_LCT));

			AssertEquals("Line AddInfo Aggregation", "", line1.AddInfo.AggregatedZA_ORG);
			AssertEquals("Line AddInfo Aggregation", "01", (ZString)line1.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_RNO));
		}

		public void TestInvoiceLineNature()
		{
			Header.JZ_Nature10PackCount = 10;
			Header.JZ_BondPackCount = 20;
			JobComInvoiceLine releaseLine = Header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine bondLine = Header.JobComInvoiceLines.AddNew();

			releaseLine.JI_IsPackToBondForLine = false;
			AssertEquals("This line is Natur10", JobComInvoiceHeader.NatureString.Nature10, releaseLine.Nature);

			bondLine.JI_IsPackToBondForLine = true;
			AssertEquals("This line is Natur20", JobComInvoiceHeader.NatureString.Nature20, bondLine.Nature);

			Header.JZ_BondPackCount = 0;

			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("This line is Natur10 for CMR", JobComInvoiceHeader.NatureString.Nature10, releaseLine.Nature);
			AssertEquals("This line is Natur20 for CMR", JobComInvoiceHeader.NatureString.Nature20, bondLine.Nature);

			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("This line is Natur10 for Legacy", JobComInvoiceHeader.NatureString.Nature10, releaseLine.Nature);
			AssertEquals("This line is Natur10 for Legacy as JZ_Nature10PackCount > 0 and JZ_BondPackCount = 0", JobComInvoiceHeader.NatureString.Nature10, bondLine.Nature);
		}

		public void TestTILVOverridesJI_TransportAndInsurance()
		{
			Header.JZ_InvoiceAmount = 1000m;
			Header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;

			Header.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			Header.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 200);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			Line.JI_LinePrice = 700m;
			Line.AddInfo.ZA_TILV = "350JPY";

			AssertEquals("TILV overrides TAndI from header", 350m, Line.TransportAndInsurance.Amount);
			AssertEquals("TILV overrides currency and Transport and insurance should be expressed in the currency entered", "JPY", Line.TransportAndInsurance.Currency.Code);
		}

		public void TestPartAddInfoGetsDefaultedIntoInvoiceLineAddInfo()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			pivot1.AddInfo.ZA_ORG = "AUST";
			AssertEquals("Precondition - Part.AddInfo.AddInfoLine", "ORG=AUST", pivot1.EffectiveAddInfo.AddInfoLine);
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Default from part to invoice line", "ORG=AUST", Line.AddInfo.AddInfoLine);
		}

		public void TestPartAddInfoDoesOverwriteLineAddInfo()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			pivot1.AddInfo.ZA_ORG = "AUST";
			Line.AddInfo.ZA_ORG = "USA";
			Line.JI_PartNo = Part.OP_PartNum;
			Assert("Line AddInfo should not contain USA", Line.AddInfo.AddInfoLine.IndexOf("USA") < 0);
			Assert("Line AddInfo should contain AUST", Line.AddInfo.AddInfoLine.IndexOf("AUST") >= 0);
		}

		public void TestClassificationAddInfoGetsDefaultedIntoInvoiceAddInfo()
		{
			ImportClass.AddInfo.ZA_GSTE = "444";
			Line.JI_CC = ImportClass.PK;
			Assert("AddInfo defaulted from classification", Line.AddInfo.AddInfoLine.IndexOf("GSTE=444") >= 0);
		}

		public void TestClassificationAddInfoDoesOverwriteLineAddInfo()
		{
			ImportClass.AddInfo.ZA_GSTE = "444";
			ImportClass.AddInfo.ZA_ORG = "AT";

			Line.AddInfo.ZA_ORG = "USA";
			Line.JI_CC = ImportClass.PK;
			Assert("AddInfo aggregated", Line.AddInfo.AddInfoLine.IndexOf("USA") < 0);
			Assert("AddInfo aggregated", Line.AddInfo.AddInfoLine.IndexOf("AT") >= 0);
			Assert("AddInfo aggregated", Line.AddInfo.AddInfoLine.IndexOf("GSTE=444") >= 0);
		}

		public void TestDefaultAddInfoFromPartDoesntRemoveHiddenProperties()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			pivot1.AddInfo.ZA_ORG = "AUST";
			Line.AddInfo.ZA_LinePrefix_Hidden = "P";
			Line.JI_PartNo = Part.OP_PartNum;

			Assert("AddInfo aggregated", Line.JI_AddInfo.IndexOf("ORG=AUST") >= 0);
			Assert("Hidden property stays", Line.JI_AddInfo.IndexOf("LinePrefix_Hidden=P") >= 0);
		}

		public void TestSetInstrumentWhenCMRButProductHasLegacyInstrument()
		{
			Line.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			pivot1.AddInfo.ZA_InstrumentType_Hidden = "TC1";
			pivot1.AddInfo.ZA_InstrumentCode_Hidden = "101";

			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Line's Instrument Type should have been set to CMR Instrument Type", "TC", Line.AddInfo.ZA_InstrumentType_Hidden);
			AssertEquals("Line's Instrument Code should have been set to CMR Instrument Code", "101", Line.AddInfo.ZA_InstrumentCode_Hidden);
		}

		public void TestClassAddInfoGetsUsedWhenPartIsAdded()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			ImportClass.AddInfo.ZA_GSTE = "FOOD";
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Line ZA_GSTE", ImportClass.AddInfo.ZA_GSTE, Line.AddInfo.ZA_GSTE);
		}

		public void TestRemovingCountryUsingPropertyRemovesFromAddInfo()
		{
			Line.AddInfo.AddInfoLine = "ORG=US";
			Assert("Precondition - Non Null CountryOfOrigin", Line.AddInfo.CountryOfOrigin != null);
			AssertEquals("Precondition - CountryOfOrigin Object", "US", Line.AddInfo.CountryOfOrigin.RN_Code);
			AssertEquals("Precondition - Country on Line", Line.AddInfo.CountryOfOrigin.Code, Line.JI_CountryOfOrigin);
			Line.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Line.JI_AddInfo", "", Line.JI_AddInfo);
		}

		public void TestCalculateWUV()
		{
			Line.JI_IsPackToBondForLine = true;
			Line.AddInfo.ZA_WRU = "CM";
			Line.AddInfo.ZA_ADJ = "555AUD";
			Line.JI_InvoiceUQ = "ML";
			Line.JI_InvoiceQuantity = 8m;
			Line.JI_LinePrice = 555m;
			Line.AddInfo.ZA_WRU = "";
			Line.AddInfo.ZA_WRQ = 0m;
			AssertEquals("WUV from invoice qty", 138.75m, Line.WUV);
			Line.JI_CustomsUnitQty = "KG";
			Line.JI_CustomsQuantity = 34m;
			AssertEquals("WUV from customs qty", 32.6471m, Line.WUV);
			Line.AddInfo.ZA_WRQ = 64m;
			AssertEquals("WUV from WRQ qty", 17.3438m, Line.WUV);
			Line.AddInfo.ZA_ADJ = "999AUD";
			AssertEquals("WUV from WRQ qty", 24.2813m, Line.WUV);
		}

		public void TestWUVWithEXW()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			creator.InvoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			creator.InvoiceLine1.AddInfo.ZA_WRU = "KG";
			creator.InvoiceLine1.AddInfo.ZA_WRQ = 10;
			creator.InvoiceLine1.JI_LinePrice = 100;
			creator.InvoiceLine1.Charges.AddNew("ADD", 100);
			AssertEquals(20m, creator.InvoiceLine1.WUV);
			creator.InvoiceLine1.AddInfo.ZA_WUV = 10m;
			AssertEquals(10m, creator.InvoiceLine1.WUV);
		}

		public void TestSetWUVWhenInvPriceUpdated()
		{
			Line.JI_IsPackToBondForLine = true;
			Line.JI_InvoiceQuantity = 10m;
			Line.JI_LinePrice = 1000m;
			AssertEquals("WUV AddInfo", ((ZDecimal)ZArchitecture.Core.Utilities.Round(Line.JI_LinePrice / Line.JI_InvoiceQuantity, 4)), Line.WUV);
		}

		public void TestWUVTakesCurrencyIntoAccount()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2000, 1, 1), new ZDateTime(2050, 1, 1), 0.5m, helper.USDCurrency);

			Header.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			Line.JI_IsPackToBondForLine = true;
			Line.JI_InvoiceQuantity = 10m;
			Line.JI_LinePrice = 1000m;
			AssertEquals("WUV AddInfo", 200m, Line.WUV);
		}

		public void TestSetWUVWhenInvQuantityUpdated()
		{
			Line.JI_IsPackToBondForLine = true;
			Line.JI_LinePrice = 1000m;
			Line.JI_InvoiceQuantity = 32.5m;
			AssertEquals("WUV AddInfo", ((ZDecimal)ZArchitecture.Core.Utilities.Round(Line.JI_LinePrice / Line.JI_InvoiceQuantity, 4)), Line.WUV);
		}

		public void TestSetWUVWhenWRQUpdated()
		{
			Line.JI_IsPackToBondForLine = true;
			Line.JI_LinePrice = 1000m;
			Line.AddInfo.ZA_WRQ = 30m;
			AssertEquals("WUV AddInfo", ((ZDecimal)ZArchitecture.Core.Utilities.Round(Line.JI_Calc_FOB_InLocalCurrency / Line.AddInfo.ZA_WRQ, 4)), Line.WUV);
			Line.JI_InvoiceQuantity = 362m;
			AssertEquals("WUV AddInfo", ((ZDecimal)ZArchitecture.Core.Utilities.Round(Line.JI_Calc_FOB_InLocalCurrency / Line.AddInfo.ZA_WRQ, 4)), Line.WUV);
		}

		public void TestSetWUVWhenNatureIs10()
		{
			Line.JI_IsPackToBondForLine = false;
			Line.JI_LinePrice = 1000m;
			Line.AddInfo.ZA_WRQ = 30m;
			Line.JI_InvoiceQuantity = 362m;
			AssertEquals("WUV AddInfo", 0m, Line.WUV);
			Line.JI_IsPackToBondForLine = true;
			AssertEquals("WUV AddInfo", ((ZDecimal)ZArchitecture.Core.Utilities.Round(Line.JI_Calc_FOB_InLocalCurrency / Line.AddInfo.ZA_WRQ, 4)), Line.WUV);
		}

		public void TestClearWarehouseRelatedProperties()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			line1.JI_IsPackToBondForLine = true;
			line1.JI_LinePrice = 4000m;
			line1.AddInfo.ZA_WRQ = 1000m;
			line1.AddInfo.ZA_WRU = "KG";
			AssertEquals("WUV line1", 4.0000m, line1.WUV);
			AssertEquals("IsPackToBondForLine_Hidden=Y*WRQ=1000*WRU=KG", line1.AddInfo.ToString());

			var line2 = Header.JobComInvoiceLines.AddNew();
			line2.JI_IsPackToBondForLine = false;
			line2.JI_LinePrice = 7000m;
			line2.JI_InvoiceQuantity = 3000m;
			line2.JI_InvoiceUQ = "UNT";
			AssertEquals("WUV line2", 0.0000m, line2.WUV);
			AssertEquals(ZString.Empty, line2.AddInfo.ToString());

			line1.JI_IsPackToBondForLine = false;
			AssertEquals("WUV line1", 0.0000m, line1.WUV);
			AssertEquals("IsPackToBondForLine_Hidden=N", line1.AddInfo.ToString());

			line2.JI_IsPackToBondForLine = true;
			AssertEquals("WUV line2", 2.3333m, line2.WUV);
			AssertEquals("IsPackToBondForLine_Hidden=Y*WRQ=3000*WRU=UNT", line2.AddInfo.ToString());
		}

		public void TestSetWUVWhenNatureIs10AndForiegnCurrency()
		{
			Header.JZ_RX_NKInvoice_Currency = "USD";

			Line.JI_IsPackToBondForLine = false;
			Line.JI_LinePrice = 1000m;
			Line.AddInfo.ZA_WRQ = 30m;
			Line.JI_InvoiceQuantity = 362m;
			AssertEquals("WUV AddInfo", 0m, Line.WUV);
			Line.JI_IsPackToBondForLine = true;
			AssertEquals("WUV AddInfo", ((ZDecimal)ZArchitecture.Core.Utilities.Round(Line.JI_Calc_FOB_InLocalCurrency / Line.AddInfo.ZA_WRQ, 4)), Line.WUV);
		}

		public void TestJI_Calc_AllOtherDuty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 300m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 100m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 200m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty, 400m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimCountervailingDuty, 500m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimDumpingDuty, 600m);

			AssertEquals("All other duty for entry line", 1800m, entryLine.AllOtherDuty);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 30000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;
			line1.JI_CL = entryLine.PK;

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20000m;
			line2.JI_CL = entryLine.PK;

			AssertEquals("All other duties for line1", 600m, line1.JI_Calc_AllOtherDuties);
			AssertEquals("All other duties for line2", 1200m, line2.JI_Calc_AllOtherDuties);
		}

		public void TestSelectingValidPartDoesNotGetUnclassifiedMessageError()
		{
			JobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Assert("Precondition", !Line.JI_PartNoInfo.HasMessageErrors());
			Part.ClassificationsForBinding.Add(ImportClass);
			Line.JI_PartNo = Part.OP_PartNum;
			Assert("Part should not have message error", !Line.JI_PartNoInfo.HasMessageErrors());
		}

		public void TestDontChangeAddInfoIfClassificationChangedOnMerge()
		{
			Classification @class = Factory.New<Classification>();
			@class.CC_AddInfo = "ORG=KR*PRF=S";
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = @class.PK;
			AssertEquals("AddInfoLine", @class.CC_AddInfo, invoiceLine.AddInfo.AddInfoLine);

			@class.CC_AddInfo = "ORG=KR";
			Assert("Contains S", invoiceLine.AddInfo.AddInfoLine.Contains("PRF=S"));
			Assert("Contains KR", invoiceLine.AddInfo.AddInfoLine.Contains("ORG=KR"));

			JobDeclaration clonedDec = (JobDeclaration)dec.TemplateCopy();
			AssertEquals("AddInfoLine", invoiceLine.AddInfo.AddInfoLine, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].AddInfo.AddInfoLine);
		}

		public void TestJI_PriceAdjustmentNotConvertedToInvoiceCurrency()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 100m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceLine.JI_LinePrice = 90m;
			invoiceLine.JI_AddInfo = "ADJ=10HKD";
			AssertEquals("AdjustmentCurrency", "HKD", invoiceLine.JI_PriceAdjustment.Currency.Code);
		}

		public void TestAggregatedAddInfoLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.AddInfo.AddInfoLine = ZString.Empty;
			invLine.AddInfo.ZA_AircraftRegistration_Hidden = "123";
			AssertEquals("", invLine.AggregatedAddInfoLine);
			invLine.AddInfo.ZA_STE = "XXX";
			AssertEquals("STE=XXX", invLine.AggregatedAddInfoLine);
			invHeader.AddInfo.ZA_GSTE = "123";
			Assert("AggregatedAddinfo Correct", invLine.AggregatedAddInfoLine == "GSTE=123*STE=XXX" || invLine.AggregatedAddInfoLine == "STE=XXX*GSTE=123");
			invLine.AddInfo.ZA_GSTE = "321";
			Assert("AggregatedAddinfo Correct", invLine.AggregatedAddInfoLine == "GSTE=321*STE=XXX" || invLine.AggregatedAddInfoLine == "STE=XXX*GSTE=321");
		}

		public void TestPRFAndORGAggregateCorrectly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.AddInfo.AddInfoLine = ZString.Empty;

			invHeader.AddInfo.ZA_ORG = "ZA";
			invHeader.AddInfo.ZA_PRF = "U";

			AssertEquals("Has Preference", true, invLine.AggregatedAddInfoLine.Contains("PRF=U"));
			AssertEquals("Has Origin", true, invLine.AggregatedAddInfoLine.Contains("ORG=ZA"));

			invLine.AddInfo.ZA_ORG = "US";

			AssertEquals("ORG=US", invLine.AggregatedAddInfoLine);
		}

		public void TestSettingClassificationOverwritesClassificationAddInfosWhenDescriptionNotOverridden()
		{
			Classification @class = Factory.New<Classification>();
			@class.CC_LookupCode = "CODE";
			@class.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			@class.CC_ClassificationType = Classification.ClassificationType.IMP;
			@class.AddInfo.ZA_TC2 = "54321";
			@class.CC_Description = "CLASSDESC";
			@class.CC_TariffNum = "2001.10.00 90";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();

			//things that should be removed/overrwritten
			invLine.AddInfo.ZA_TC2 = "12345";
			invLine.AddInfo.ZA_TreatmentCode_Hidden = "123";
			invLine.AddInfo.ZA_InstrumentType_Hidden = "TC1";
			invLine.AddInfo.ZA_InstrumentCode_Hidden = "123456";

			//things that should stay
			invLine.JI_InvoiceUQ = "PCE";
			invLine.JI_InvoiceQuantity = 100m;
			invLine.JI_Weight = 10m;
			invLine.JI_WeightUQ = "KG";
			invLine.AddInfo.ZA_ORG = "US";
			invLine.AddInfo.ZA_PRF = "X";

			invLine.JI_CC = @class.PK;

			AssertEquals("JI_InvoiceUQ", "PCE", invLine.JI_InvoiceUQ);
			AssertEquals("JI_InvoiceQuantity", 100m, invLine.JI_InvoiceQuantity);
			AssertEquals("JI_Weight", 10m, invLine.JI_Weight);
			AssertEquals("JI_WeightUQ", "KG", invLine.JI_WeightUQ);
			AssertEquals("AddInfo.ZA_ORG", "US", invLine.AddInfo.ZA_ORG);
			AssertEquals("AddInfo.ZA_PRF", "X", invLine.AddInfo.ZA_PRF);

			AssertEquals("JI_Description", "CLASSDESC", invLine.JI_Description);
			AssertEquals("AddInfo.ZA_TC2", "54321", invLine.AddInfo.ZA_TC2);
			AssertEquals("AddInfo.ZA_TreatmentCode_Hidden", "", invLine.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("AddInfo.ZA_InstrumentType_Hidden", "", invLine.AddInfo.ZA_InstrumentType_Hidden);
			AssertEquals("AddInfo.ZA_InstrumentCode_Hidden", "", invLine.AddInfo.ZA_InstrumentCode_Hidden);
		}

		public void TestSettingClassificationOverwritesClassificationAddInfosWhenDescriptionIsOverridden()
		{
			Classification @class = Factory.New<Classification>();
			@class.CC_LookupCode = "CODE";
			@class.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			@class.CC_ClassificationType = Classification.ClassificationType.IMP;
			@class.AddInfo.ZA_TC2 = "54321";
			@class.CC_Description = "CLASSDESC";
			@class.CC_TariffNum = "2001.10.00 90";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();

			//things that should be removed/overrwritten
			invLine.AddInfo.ZA_TC2 = "12345";
			invLine.AddInfo.ZA_TreatmentCode_Hidden = "123";
			invLine.AddInfo.ZA_InstrumentType_Hidden = "TC1";
			invLine.AddInfo.ZA_InstrumentCode_Hidden = "123456";

			//things that should stay
			invLine.JI_Description = "DESC";
			invLine.JI_InvoiceUQ = "PCE";
			invLine.JI_InvoiceQuantity = 100m;
			invLine.JI_Weight = 10m;
			invLine.JI_WeightUQ = "KG";
			invLine.AddInfo.ZA_ORG = "US";
			invLine.AddInfo.ZA_PRF = "X";

			invLine.JI_CC = @class.PK;

			AssertEquals("JI_InvoiceUQ", "PCE", invLine.JI_InvoiceUQ);
			AssertEquals("JI_InvoiceQuantity", 100m, invLine.JI_InvoiceQuantity);
			AssertEquals("JI_Weight", 10m, invLine.JI_Weight);
			AssertEquals("JI_WeightUQ", "KG", invLine.JI_WeightUQ);
			AssertEquals("AddInfo.ZA_ORG", "US", invLine.AddInfo.ZA_ORG);
			AssertEquals("AddInfo.ZA_PRF", "X", invLine.AddInfo.ZA_PRF);

			AssertEquals("JI_Description", "DESC", invLine.JI_Description);
			AssertEquals("AddInfo.ZA_TC2", "54321", invLine.AddInfo.ZA_TC2);
			AssertEquals("AddInfo.ZA_TreatmentCode_Hidden", "", invLine.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("AddInfo.ZA_InstrumentType_Hidden", "", invLine.AddInfo.ZA_InstrumentType_Hidden);
			AssertEquals("AddInfo.ZA_InstrumentCode_Hidden", "", invLine.AddInfo.ZA_InstrumentCode_Hidden);
		}

		public void TestRemovingClassificationDoesntBlankClassInfo()
		{
			Classification @class = Factory.New<Classification>();
			@class.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			@class.CC_ClassificationType = Classification.ClassificationType.IMP;
			@class.AddInfo.ZA_TC2 = "54321";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();

			invLine.JI_CC = @class.PK;
			AssertEquals("54321", invLine.AddInfo.ZA_TC2);
			invLine.JI_CC = ZGuid.Empty;
			AssertEquals("54321", invLine.AddInfo.ZA_TC2);
		}

		public void TestReproduce2ndQtyProblem()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_Tariff = "2203.00.31 16";
			invLine.AddInfo.ZA_ISS = 5.6m;
			invLine.AddInfo.ZA_UQ2 = "L";
			invLine.AddInfo.ZA_QT2 = 1005.36m;
			//InvLine.JI_InvoiceUQ = "LA";
			//InvLine.JI_InvoiceQuantity = 0m;
			invLine.JI_CustomsQuantity = 101.1m;
			Assert(true);
		}

		public void TestCustomsUQReadOnly()
		{
			Line.JI_CustomsUnitQty = "L";
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceLine loadedLine = newFactory.Load<JobComInvoiceLine>(Line.PK);
			Assert("Loaded Line should have Customs Unit Qty read only on loaded", loadedLine.JI_CustomsUnitQtyInfo.ReadOnly);

			AUCustomsDataRegistry.Instance.AllowOverrideOfCustomsUnitsOnDeclarations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			JobComInvoiceLine loadedLine2 = newFactory2.Load<JobComInvoiceLine>(Line.PK);
			Assert("Loaded Line should NOT have Customs Unit Qty read only on loaded", !loadedLine.JI_CustomsUnitQtyInfo.ReadOnly);
		}

		public void TestLinesWithDifferentSupplierCIDsDoNotMerge()
		{
			var dec = JobDec;

			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			JobComInvoiceLine line1 = Header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0101.1000/25";
			line1.JI_LinePrice = 1m;
			JobComInvoiceLine line2 = Header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0101.1000/25";
			line2.JI_LinePrice = 2m;

			AssertEquals("PreCondition : Line 1 Supplier invalid", ZGuid.Empty, line1.JI_OH_Supplier);
			AssertEquals("PreCondition : Line 2 Supplier invalid", ZGuid.Empty, line2.JI_OH_Supplier);
			AssertEquals("PreCondition : Invoice Line Count", 2, dec.FilteredInvoiceLines.Count);

			Header.JZ_InvoiceAmount = 30m;
			Header.JZ_RX_NKInvoice_Currency = "AUD";
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			dec.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			dec.DoMerge();
			AssertEquals("No Supplier (uses invoice supplier), merge", 1, dec.CustomsEntryHeaders[0].MergedLines.Count);

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			line1.JI_OH_Supplier = supplier1.PK;
			dec.DoMerge();
			AssertEquals("Line Supplier has no code, merge", 1, dec.CustomsEntryHeaders[0].MergedLines.Count);

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			line2.JI_OH_Supplier = supplier2.PK;
			dec.DoMerge();
			AssertEquals("Line Suppliers have no codes, merge", 1, dec.CustomsEntryHeaders[0].MergedLines.Count);

			supplier1.CustomsClientID = "ABC123";
			dec.DoMerge();
			AssertEquals("A Line Supplier has a code, not merge", 2, dec.CustomsEntryHeaders[0].MergedLines.Count);

			supplier2.CustomsClientID = "XYZ789";
			dec.DoMerge();
			AssertEquals("Line Suppliers have different codes, not merge", 2, dec.CustomsEntryHeaders[0].MergedLines.Count);

			supplier2.CustomsClientID = "ABC123";
			dec.DoMerge();
			AssertEquals("Line Suppliers have same codes, merge", 1, dec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestCustomsUnitDefaultingStrategy()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var testImpTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203009117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testImpTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(testImpTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var line1 = Header.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "2203.00.91 17";
				AssertNotNull("PreCondition : this tariff exists", line1.ImportTariff);
				AssertEquals("UQ1", "LA", line1.JI_CustomsUnitQty);
				AssertEquals("UQ2", "L", line1.JI_CustomsSecondUnitQty);
			}
		}

		public void TestCustomsUnitDefaultingStrategy_AUCClass()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var line1 = Header.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "2203.00.31 15";
				AssertNotNull("PreCondition : this tariff exists", line1.ImportTariff);
				AssertEquals("UQ1", "LA", line1.JI_CustomsUnitQty);
				AssertEquals("UQ2", "L", line1.JI_CustomsSecondUnitQty);
			}
		}

		#region Implementation

		Classification fBeerClass;
		protected Classification BeerClass
		{
			get
			{
				if (fBeerClass == null)
				{
					fBeerClass = Factory.New<Classification>();
					fBeerClass.CC_TariffNum = BeerTariff.UJ_Code;
					fBeerClass.CC_LookupCode = "TESTBEER";
					fBeerClass.CC_ClassificationType = Classification.ClassificationType.IMP;
					fBeerClass.CC_Description = "TESTBEER";
				}
				return fBeerClass;
			}
		}

		AUCClass fBeerTariff;
		protected AUCClass BeerTariff
		{
			get
			{
				if (fBeerTariff == null)
				{
					fBeerTariff = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "2203.00.31 15");
					AssertNotNull("PreCondition : this tariff exists", fBeerTariff);
					AssertEquals("PreCondition: UQ1", "LA", BeerTariff.UJ_UQ1);
					AssertEquals("PreCondition: UQ1", "L", BeerTariff.UJ_UQ2);
				}
				return fBeerTariff;
			}
		}

		AUCClass fBeerTariffWithOnePointFivePercentFree;
		protected AUCClass BeerTariffWithOnePointFivePercentFree
		{
			get
			{
				if (fBeerTariffWithOnePointFivePercentFree == null)
				{
					fBeerTariffWithOnePointFivePercentFree = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "2203.00.69 20");
					AssertNotNull("PreCondition : this tariff exists", BeerTariff);
					AssertEquals("PreCondition: UQ1", "LA", BeerTariff.UJ_UQ1);
					AssertEquals("PreCondition: UQ1", "L", BeerTariff.UJ_UQ2);
				}
				return fBeerTariffWithOnePointFivePercentFree;
			}
		}

		protected override JobDeclaration JobDec
		{
			get
			{
				if (fJobDec == null)
				{
					fJobDec = base.JobDec;
					fJobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					fJobDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				}
				return fJobDec;
			}
		}
		JobDeclaration fJobDec;

		#endregion

	}
}
