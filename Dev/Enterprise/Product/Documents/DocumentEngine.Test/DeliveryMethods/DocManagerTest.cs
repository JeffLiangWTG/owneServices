using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class DocManagerTest : TestCaseWithFactory
	{
		public void TestDeliver()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var newStyleTemplateStream = embeddedResourceRetriever.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls");
				var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var deliveryMethod = new DocManager();
				var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				info.Copies = 1;
				info.SetFileContents(newStyleTemplateStream, "xls");
				info.EmailSubjectLine = "Testing Email Subject Line";
				info.DeliveryGroupID = Factory.New(typeof(StmDeliveryGroup)).PK;
				info.ParentGuid = organisation.PK;
				info.ParentTableName = organisation.TableName;
				info.RelatedBusinessContext = ((IDocManagerSupport)organisation).DocManagerInfo.DocManagerCode;

				Factory.Save();

				deliveryMethod.AddFile(info);
				deliveryMethod.Deliver();

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();
				AssertEquals("Job count should be just 1", 1, printJobs.Count);
				AssertEquals("Print job should be a DDS type", nameof(PrintType.DDS), printJobs[0].SP_JobType);
				AssertEquals("ParentGuid", organisation.PK, printJobs[0].SP_ParentGuid);
			}
		}

		public void TestDeliverSetAdditionalPropertiesForDocumentDelivery()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[<OverFlowToFollowPage(""Test Title"", 2, WrapOverflowWithContinued)>This is some long string that will need its cell height to be adjusted. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.]
{A}-[#EndOfReport]"
				);

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate) { IsScheduledReport = false })
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var newStyleTemplateStream = embeddedResourceRetriever.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls");
				var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var contact = new DocDeliveryContact(Factory);
				contact.DeliveryMethod = "EDC";
				contact.AttachmentType = "XLSX";
				var deliveryMethod = new DocManager(contact);

				var instructions = new DeliveryInstructions(documentPack);
				documentPack.Add(report);
				var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				info.Copies = 1;
				info.SetFileContents(newStyleTemplateStream, "xls");
				info.EmailSubjectLine = "Testing Email Subject Line";
				info.DeliveryGroupID = Factory.New(typeof(StmDeliveryGroup)).PK;
				info.ParentGuid = organisation.PK;
				info.ParentTableName = organisation.TableName;
				info.RelatedBusinessContext = ((IDocManagerSupport)organisation).DocManagerInfo.DocManagerCode;
				info.Instructions = instructions;

				Factory.Save();

				deliveryMethod.AddFile(info);
				deliveryMethod.Deliver();

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();
				AssertEquals("Job count should be just 1", 1, printJobs.Count);
				AssertEquals("Print job should be a DDS type", "XLSX", printJobs[0].SP_EmailAttachmentFormat);
			}
		}
	}
}
