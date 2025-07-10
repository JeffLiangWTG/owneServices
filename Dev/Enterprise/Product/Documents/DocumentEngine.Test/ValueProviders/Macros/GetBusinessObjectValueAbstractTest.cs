using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestsSubclassesOf(typeof(GetBusinessObjectValue))]
	abstract class GetBusinessObjectValueAbstractTest<T> : ValueProviderWithLoadControlFactoryTest<T> where T : GetBusinessObjectValue, new()
	{
		public void TestDBHits()
		{
			var factory = new BusinessObjectFactory();
			for (var i = 0; i < 100; i++)
			{
				var declaration = factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_MasterBill = i.ToString();
			}
			factory.Save();

			var declarationMacro1 = "<GetBusinessObjectValue(JOBDECLARATION, <ReportData1.JE_PK>, MasterBill)>";
			var declarationMacro2 = "<GetBusinessObjectValue(JOBDECLARATION, <ReportData2.JE_PK>, MasterBill)>";
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Data:ReportData1=SELECT TOP 50 JE_PK FROM dbo.JobDeclaration ORDER BY JE_MasterBill]
{A}-[Data:ReportData2=SELECT TOP 50 JE_PK FROM dbo.JobDeclaration ORDER BY JE_MasterBill DESC]
{A}-[#SectionBody:Data=ReportData1]
{B}-[" + declarationMacro1 + @"]
{A}-[#SectionBody:Data=ReportData2]
{B}-[" + declarationMacro2 + @"]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), template))
			using (var stream = new MemoryStream())
			using (TestConnection.TrackExecutedCommands())
			{
				var getBusinessObjectValueProvider = report.MacroTranslator.GetValueProvider(Passes.FirstPass, declarationMacro1) as GetBusinessObjectValue;
				getBusinessObjectValueProvider.BOAndPKDic.Add(("JobShipment", "JobShipment", "MasterBill"), new GetBusinessObjectValue.PKListWrapperForFetchHint());
				var valueProviderFactory = getBusinessObjectValueProvider.FactoryForTesting;
				AssertNotNull(valueProviderFactory);
				AssertEquals(1, getBusinessObjectValueProvider.BOAndPKDic.Count);

				report.Save(stream);

				AssertEquals("There should be 2 items as there are 2 SectionBody areas, and the initial one is cleared by Reset.", 2, getBusinessObjectValueProvider.BOAndPKDic.Count);
				AssertNotEquals("The factory should be recycled by Reset.", getBusinessObjectValueProvider.FactoryForTesting, valueProviderFactory);

				var declarationValueProvider1 = report.MacroTranslator.GetValueProvider(Passes.FirstPass, declarationMacro1) as GetBusinessObjectValue;
				var declarationValueProvider2 = report.MacroTranslator.GetValueProvider(Passes.FirstPass, declarationMacro2) as GetBusinessObjectValue;
				AssertEquals(declarationValueProvider1, getBusinessObjectValueProvider);
				AssertEquals(declarationValueProvider1, declarationValueProvider2);

				var count = declarationValueProvider1?.FactoryForTesting.DatabaseLoadCount;
				AssertEquals("The database load should happen only once for each query.", 2, count);

				var declarationQueries = TestConnection.ExecutedCommands.Where(c => c.Contains("WHERE (JE_PK in (SELECT Value FROM")).ToList();
				AssertEquals("There should be 2 TVP queries called as there are 2 different ReportData.", 2, declarationQueries.Count);
			}
		}

		public void TestPreSetupForGettingValueWithNullInPk()
		{
			var factory = new BusinessObjectFactory();
			for (var i = 0; i < 5; i++)
			{
				var declaration = factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_MasterBill = i.ToString();
			}
			factory.Save();

			var declarationMacro1 = "<GetBusinessObjectValue(JOBDECLARATION, <ReportData1.JE_PK>, MasterBill)>";
			var declarationMacro2 = "<GetBusinessObjectValue(JOBDECLARATION, <ReportData2.JE_PK>, MasterBill)>";
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Data:ReportData1=SELECT TOP 2 JE_PK FROM dbo.JobDeclaration]
{A}-[Data:ReportData2=SELECT TOP 2 NULL AS JE_PK FROM dbo.JobDeclaration]
{A}-[#SectionBody:Data=ReportData1]
{B}-[WhatEver]
{A}-[#SectionBody:Data=ReportData2]
{B}-[WhatEver]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), template))
			using (var stream = new MemoryStream())
			using (TestConnection.TrackExecutedCommands())
			{
				var getBusinessObjectValueProvider = report.MacroTranslator.GetValueProvider(Passes.FirstPass, declarationMacro1) as GetBusinessObjectValue;
				report.Save(stream);
				getBusinessObjectValueProvider.BOAndPKDic.Clear();

				report.Renderer.CurrentAreaToProcess = report.Analyser.Areas[1];
				AssertNoExceptionThrown(delegate
				{ getBusinessObjectValueProvider.PreSetupForGettingValue(declarationMacro1, Passes.FirstPass, report); });
				AssertEquals(1, getBusinessObjectValueProvider.BOAndPKDic.Count);

				report.Renderer.CurrentAreaToProcess = report.Analyser.Areas[2];
				AssertNoExceptionThrown(delegate
				{ getBusinessObjectValueProvider.PreSetupForGettingValue(declarationMacro2, Passes.FirstPass, report); });
				AssertEquals(2, getBusinessObjectValueProvider.BOAndPKDic.Count);
			}
		}

		public void TestPreSetupForGettingValueWithDifferentColumns()
		{
			var factory = new BusinessObjectFactory();

			for (var i = 0; i < 5; i++)
			{
				var declaration = factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_MasterBill = i.ToString();
				var entryHeaderPrime = (BusinessObject)factory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
				entryHeaderPrime.FillWithValidTestData();
				entryHeaderPrime["CH_JE"] = declaration.PK;

				var entryHeader1 = (BusinessObject)factory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
				entryHeader1.FillWithValidTestData();
				entryHeader1["CH_JE"] = declaration.PK;

				entryHeader1["CH_CH_PrimeEntry"] = entryHeaderPrime.PK;
			}
			factory.Save();

			var macro1 = "<GetBusinessObjectValue(CusEntryHeader, <ReportData1.CH_CH_PrimeEntry>, EntryNumber)>";
			var macro2 = "<GetBusinessObjectValue(CusEntryHeader, <ReportData1.CH_PK>, EntryNumber)>";

			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
	@"{A}-[#Config]
	{A}-[Data:ReportData1=SELECT CH_PK, CH_CH_PrimeEntry FROM dbo.CusEntryHeader WHERE CH_CH_PrimeEntry IS Not Null]
	{A}-[#SectionBody:Data=ReportData1]
	{B}-[" + macro1 + @"]
	{A}-[#SectionBody:Data=ReportData1]
	{}-[" + macro2 + @"]
	{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), template))
			using (var stream = new MemoryStream())
			using (TestConnection.TrackExecutedCommands())
			{
				var getBusinessObjectValueProvider = report.MacroTranslator.GetValueProvider(Passes.FirstPass, macro1) as GetBusinessObjectValue;
				var valueProviderFactory = getBusinessObjectValueProvider.FactoryForTesting;
				AssertNotNull(valueProviderFactory);

				report.Save(stream);

				AssertEquals("There should be 2 items as there are 2 different column macros in one SectionBody area", 2, getBusinessObjectValueProvider.BOAndPKDic.Count);

				var valueProvider1 = report.MacroTranslator.GetValueProvider(Passes.FirstPass, macro1) as GetBusinessObjectValue;
				var valueProvider2 = report.MacroTranslator.GetValueProvider(Passes.FirstPass, macro2) as GetBusinessObjectValue;
				AssertEquals(valueProvider1, getBusinessObjectValueProvider);
				AssertEquals(valueProvider1, valueProvider2);

				var count = valueProvider1?.FactoryForTesting.DatabaseLoadCount;
				AssertEquals("The database load should happen only once for each query.", 10, count);
			}
		}

		public void TestReplacementWithInvalidExpression()
		{
			var entryNumber = "12345";
			ValueProvider = new GetBusinessObjectValue();

			var factory = ValueProviderFactory;
			var entryHeader = (BusinessObject)factory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
			var cusEntryNum = (BusinessObject)factory.New<Enterprise.Integration.Customs.ICusEntryNumber>();

			cusEntryNum["CE_ParentTable"] = "CusEntryHeader";
			cusEntryNum["CE_ParentID"] = entryHeader.PK;
			cusEntryNum["CE_RN_NKCountryCode"] = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNum["CE_EntryType"] = "IMP";
			cusEntryNum["CE_EntryNum"] = entryNumber;

			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				object value = null;
				AssertNoExceptionThrown(delegate
				{ value = ValueProvider.GetReplacement("<GetBusinessObjectValue(UnknownTypeName, " + entryHeader.PK + ", EntryNumber)>", report); });
				AssertEquals("Value should be null", null, value);
				AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);
				AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning] Message: [Error in GetBusinessObjectValue Macro: BusinessObjectType [UnknownTypeName] not supported by this macro.]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
				report.ErrorManager.ClearErrors();

				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				AssertNoExceptionThrown(delegate
				{ value = ValueProvider.GetReplacement("<GetBusinessObjectValue(CusEntryHeader, " + entryHeader.PK + ", UnknownProperty)>", report); });
				AssertEquals("Value should be null", null, value);
				AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);
				AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning] Message: [Error in GetBusinessObjectValue Macro: Property UnknownProperty is not accessible or not defined in CusEntryHeader]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
				report.ErrorManager.ClearErrors();

				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				var testPK = new ZGuid(new Guid("b962f557-0a15-4a20-b052-5893d68d6521"));
				AssertNoExceptionThrown(delegate
				{ value = ValueProvider.GetReplacement("<GetBusinessObjectValue(CusEntryHeader, " + testPK + ", UnknownProperty)>", report); });
				AssertEquals("Value should be null", null, value);
				AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);
				AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning] Message: [Error in GetBusinessObjectValue Macro: Could not load a [CusEntryHeader] using PK [b962f557-0a15-4a20-b052-5893d68d6521].]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
				report.ErrorManager.ClearErrors();

				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				AssertNoExceptionThrown(delegate
				{ value = ValueProvider.GetReplacement("<GetBusinessObjectValue(CusEntryHeader, 11111-11111-11111, UnknownProperty)>", report); });
				AssertEquals("Value should be null", null, value);
				AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);
				AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning] Message: [Error in GetBusinessObjectValue Macro: Second parameter [11111-11111-11111] is not a valid PK.]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
				report.ErrorManager.ClearErrors();
			}
		}

		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <Two args are missing>", !ValueProviderToTest.IsResponsibleForReplacing("<Duty(AField)>", Passes.FirstPass));
			Assert("should not match <One arg is missing>", !ValueProviderToTest.IsResponsibleForReplacing("<Duty(AField, BField)>", Passes.FirstPass));
			Assert("should not match <Macro name is wrong>", !ValueProviderToTest.IsResponsibleForReplacing("<dut>", Passes.FirstPass));
			Assert("should not match <Macro name is wrong>", !ValueProviderToTest.IsResponsibleForReplacing("<   duty somefield   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   GetBusinessObjectValue  \t  (     fld , dd, cc )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   GetBusinessObjectValue(AField, other, last)   >", Passes.FirstPass));
		}

		public override void TestReplacement() //ForCusEntryHeader
		{
			var entryNumber = "12345";
			ValueProvider = new GetBusinessObjectValue();

			var factory = ValueProviderFactory;
			var entryHeader = (BusinessObject)factory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
			var cusEntryNum = (BusinessObject)factory.New<Enterprise.Integration.Customs.ICusEntryNumber>();

			cusEntryNum["CE_ParentTable"] = "CusEntryHeader";
			cusEntryNum["CE_ParentID"] = entryHeader.PK;
			cusEntryNum["CE_RN_NKCountryCode"] = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNum["CE_EntryType"] = "IMP";
			cusEntryNum["CE_EntryNum"] = entryNumber;

			using (var testReport = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				var value = ValueProvider.GetReplacement("<GetBusinessObjectValue(CusEntryHeader, " + entryHeader.PK + ", EntryNumber)>", testReport);
				AssertEquals(entryNumber, value);
			}
		}

		public void TestJobComInvoice()
		{
			ValueProvider = new GetBusinessObjectValue();
			var factory = ValueProviderFactory;
			var invoiceLine = (BusinessObject)factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();

			var lineNo = new ZShort(4);

			invoiceLine["JI_LineNo"] = lineNo;
			using (var testReport = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				var value = ValueProvider.GetReplacement("<GetBusinessObjectValue(JobComInvoiceLine, " + invoiceLine.PK.ToString() + ", LineNo)>", testReport);
				AssertEquals(lineNo, value);
			}
		}

		public void TestJobDeclaration()
		{
			ValueProvider = new GetBusinessObjectValue();
			ValueProvider.MaxNumberOfObjectsCanBeHeldByFactory = 500;
			var factory = ValueProviderFactory;
			var declaration = (BusinessObject)factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			declaration["JE_DeclarationReference"] = "Big Swinging Dec";
			using (var testReport = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				var value = ValueProvider.GetReplacement("<GetBusinessObjectValue(JobDeclaration, " + declaration.PK.ToString() + ", DeclarationReference)>", testReport);
				AssertEquals("Big Swinging Dec", value);
			}
		}

		public void TestARInvoice()
		{
			ValueProvider = new GetBusinessObjectValue();
			var factory = ValueProviderFactory;
			var aRInvoice = (BusinessObject)factory.New<Accounting.Integration.IARInvoice>();

			aRInvoice["AH_TransactionNum"] = "INVNUM";
			using (var testReport = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				var value = ValueProvider.GetReplacement("<GetBusinessObjectValue(ARInvoice, " + aRInvoice.PK.ToString() + ", JobInvoiceNumber)>", testReport);
				AssertEquals("INVNUM", value);
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			ValueProvider = new GetBusinessObjectValue();
			var factory = ValueProviderFactory;
			var invoiceLine = (BusinessObject)factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
			var lineNo = new ZShort(1);
			invoiceLine["JI_LineNo"] = lineNo;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("EntryHeader.CH_PK", invoiceLine.PK));
		}

		protected override List<FieldInfo> FieldCollection => new List<FieldInfo>() { typeof(GetBusinessObjectValue).GetField("<BOAndPKDic>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) };

		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		ExcelTemplateForUnitTesting emptyAndValidTemplate;
		ExcelTemplateForUnitTesting EmptyAndValidTemplate
		{
			get
			{
				if (emptyAndValidTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return emptyAndValidTemplate;
			}
		}
	}
}
