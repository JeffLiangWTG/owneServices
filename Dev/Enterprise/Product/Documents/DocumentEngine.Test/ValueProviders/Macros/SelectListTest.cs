using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ExcelTemplates;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SelectList))]
	sealed class SelectListTest : ValueProviderTest
	{
		public void TestReplacementForDataAreasWithIllegalDataSource()
		{
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);

					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[3, 0] = "Data:Lines=select top 140 * from ##LinesTest";
					workSheet[4, 0] = "#DocumentHeader";
					workSheet[5, 0] = "#SectionBody:Data=Lines";
					workSheet[10, 0] = "#EndOfReport";

					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (var report = new Report(null, excelTemplate))
				{
					var mocker = new MockRepository(MockBehavior.Default);
					var renderer = mocker.Create<IReportRenderer>();

					renderer.Setup(m => m.Render()).Callback(() => { new SelectList().GetReplacement("<This should throw excpetion>", report); });

					report.Renderer = renderer.Object;
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;

					using (var outputStream = new MemoryStream())
					{
						AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
						AssertNoExceptionThrown(delegate { report.Save(outputStream); });
						AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
						AssertEquals("Report.Errors", @"Severity: [Fatal] Message: [Data source not correctly defined on select list macro: <This should throw excpetion>]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
						Report.ErrorManager.ClearErrors();
					}

					mocker.VerifyAll();
				}
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <DataSource.SelectList>", GetNewValueProvider().IsResponsibleForReplacing("<DataSource.SelectList>", Passes.FirstPass));
			Assert("Should replace <DataSource1.selectlist>", GetNewValueProvider().IsResponsibleForReplacing("<DataSource1.selectlist>", Passes.FirstPass));
			Assert("Should not replace <SelectList.DataSource2>", !GetNewValueProvider().IsResponsibleForReplacing("<SelectList.DataSource2>", Passes.FirstPass));
			Assert("Should not replace <some random junk>", !GetNewValueProvider().IsResponsibleForReplacing("<some random junk>", Passes.FirstPass));

			var matches = ValueProviderToTest.Regex.Matches("<DataSource.SelectList>");
			AssertEquals("Should match DataSource", "DataSource", matches[0].Groups[1].Value);
			matches = ValueProviderToTest.Regex.Matches("<DataSource1.selectlist>");
			AssertEquals("Should match DataSource1", "DataSource1", matches[0].Groups[1].Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacementForDataAreas()
		{
			var template = new ExcelTemplateForUnitTesting("SelectListTest.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				report.PrepareForRender();
				var groupByArea = new DataAreaForTest("#GroupBy:TestGroup: GroupTitle", report);
				var testSection = new Section();
				testSection.SectionBodyAndGroupByAreas.Add(groupByArea);
				report.Analyser.Sections.Add(testSection);
				var expectedColumns = "[AL_LINEAMOUNT], [AL_LINETYPE], [AL_EXCHANGERATE], [AL_POSTPERIOD], [AL_UNITQTY], [AL_UNITPRICE], [JH_A_JOP], [JH_PK], [JH_OA_LOCALCHARGESADDR], [JH_ISPROFITSHAREPOSTED], [JH_OA_AGENTCOLLECTADDR], [AL_POSTTOGL], [JH_JOBNUM], [AL_JH], [AL_OSAMOUNT]";
				var result = (string)ValueProviderToTest.GetReplacement("<reportdata.SelectList>", report);
				AssertEquals(expectedColumns, result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestNonDataAreasCauseNoException()
		{
			var template = new ExcelTemplateForUnitTesting("SelectListTest.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				report.PrepareForRender();
				var groupByArea = new DataAreaForTest("#GroupBy:TestGroup: GroupTitle", report);
				var testSection = new Section();
				testSection.SectionPageHeader = new SectionPageHeaderArea(0, 1, Report, "a");
				testSection.SectionBodyAndGroupByAreas.Add(groupByArea);
				report.Analyser.Sections.Add(testSection);
				ValueProviderToTest.GetReplacement("<reportdata.SelectList>", report);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestMultipleSectionReport()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SelectListMulitipleSectionTest.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				using (var stream = new MemoryStream())
				{
					report.Save(stream);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSelectListIsCalculatedForEachOptionalTemplate()
		{
			var template = new ExcelTemplateForUnitTesting("SelectListMultipleOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			var docPack = new DocumentPack();
			var factory = new BusinessObjectFactory();
			using (var rpt = new Report(docPack, template, Guid.Empty, Core.Constants.DataContext.None))
			{
				rpt.PrepareForRender();
				rpt.OptionalTemplateSheetCollection["Sheet1"].Selected = true;
				rpt.OptionalTemplateSheetCollection["Sheet2"].Selected = true;

				docPack.Add(rpt);

				var printQueue = factory.New<StmPrintQueue>();
				printQueue.SQ_QueueName = "TestPrinter";
				printQueue.SQ_DisplayName = "TestPrinter";
				factory.Save();

				var instructions = new DeliveryInstructions(docPack);

				instructions.PrinterDelivery.PrintQueuePK = printQueue.PK;
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				instructions.SetAndSaveDeliveryGroupSubjectLine(docPack, new PrintTask.ReportSubjectLineMapping(null, "test"));
				instructions.Recipients[0].DeliveryMethod = "PRN";

				docPack.Run(instructions);
				var printJobs = factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK));
				AssertEquals("Precondition: printJobs.Count", 1, printJobs.Length);
				var printJob = printJobs[0];

				using (var tmpFile = TempFile.NewWithExtension("xls"))
				{
					using (var stream = new FileStream(tmpFile.Filename, FileMode.Create))
					{
						stream.Write(printJob.SP_CustomProperties, 0, printJob.SP_CustomProperties.Length);
					}
					using (var excel = new ExcelInterface())
					{
						excel.LoadExcelFile(tmpFile.Filename);
						excel.Xls.ActiveSheetByName = "Sheet2";
						AssertEquals("Should contain 'DEM'", "DEM", excel.Xls.GetCellValue(1, 3));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}

		protected override List<FieldInfo> FieldCollection => new List<FieldInfo>() { typeof(SelectList).GetField("selectListsCache", BindingFlags.Instance | BindingFlags.NonPublic) };

		protected override ValueProvider GetNewValueProvider() => new SelectList();

		protected override void PrepareDataForExamplesEvaluate()
		{
			var template = new ExcelTemplateForUnitTesting("SelectListTest.xls", TestFilesSubFolder.ReportTestFiles);
			Report = new Report(new DocumentPack(), template);
			Report.PrepareForRender();
		}

		sealed class DataAreaForTest : DataArea
		{
			public DataAreaForTest(string groupName, Report report)
				: base(10, 11, report, groupName)
			{
			}

			public override Area Clone(int position) => throw new NotImplementedException();

			public override bool CanCloseAPage => throw new NotImplementedException();

			public override List<Area> Parents => throw new NotImplementedException();

			public override ValueProviderDocumenter GetDocumentation() => throw new NotImplementedException();
		}
	}
}
