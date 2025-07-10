using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class XmlCreateDeliveryInfoStrategyTest : TestCaseWithFactory
	{
		public void TestXmlFromControlCharacters()
		{
			CreateDummyBusinessObject(1, new string(new char[1] { (char)0x1f }));

			Factory.Save();
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""  "", HeadingText=""  ""]    {C}-[DisplayLabel=""  "", HeadingText=""VarCharMax""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate, "DummyReport", ContactType.All, false))
			{
				report.StTemplate = Factory.New<StmTemplateBase>();
				report.Template.ContainsCustomisedSections = false;
				report.StTemplate.SO_IsSystemDefined = true;

				var strategy = new XmlCreateDeliveryInfoStrategy();
				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

				AssertEquals("FileFormat should be XML.", "XML", result.FileFormat);
				var bytes = result.FileContents.CopyToByteArray();
				var ascii = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
				AssertContains("&#x1F;", ascii);
			}
		}

		public void TestCreateDeliveryInfoWithSystemDefinedEmptyColumnHeading()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");

			Factory.Save();
			try
			{
				var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""  "", HeadingText=""  ""]    {C}-[DisplayLabel=""  "", HeadingText=""VarCharMax""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

				using (var report = new Report(new DocumentPack(), excelTemplate, "DummyReport", ContactType.All, false))
				{
					report.StTemplate = Factory.New<StmTemplateBase>();
					report.Template.ContainsCustomisedSections = false;
					report.StTemplate.SO_IsSystemDefined = true;

					var strategy = new XmlCreateDeliveryInfoStrategy();
					var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

					AssertEquals("FileFormat should be XML.", "XML", result.FileFormat);
					Assert("Should not be reported", ErrorReporter.TotalErrorCount == 0);
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestCreateDeliveryInfoWithCustomerDefinedEmptyColumnHeading()
		{
			AddPostMasterGroupEmail();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""  "", HeadingText=""  ""]    {C}-[DisplayLabel=""  "", HeadingText=""VarCharMax""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate, "DummyReport", ContactType.All, false))
			{
				report.StTemplate = Factory.New<StmTemplateBase>();
				report.Template.ContainsCustomisedSections = false;
				report.StTemplate.SO_IsSystemDefined = false;

				var deliveryInstructions = new DeliveryInstructions();
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var strategy = new XmlCreateDeliveryInfoStrategy();
				var result = strategy.CreateDeliveryInfo(report, null, deliveryInstructions);

				AssertEquals("FileFormat should be XML.", "XML", result.FileFormat);
				AssertEquals(null, ErrorReporter.LastExceptionReported);
				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.Recipients should match", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Email.CCRecipients should match", "", email.CCRecipients.RecipientsAsDelimitedString());
				AssertEquals("Error Generating Report [DummyReport]", email.Subject);

				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		public void TestRunScheduledReportAsXmlFileWithEmailNotificationWhenEmptyColumnHeading()
		{
			AddPostMasterGroupEmail();
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Hello World";

			var reportCommand = Factory.New<ReportCommand>();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""  "", HeadingText=""  ""]    {C}-[DisplayLabel=""  "", HeadingText=""VarCharMax""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_EmailAddress = "unit.test@cargowise.com";

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = ContactNotifyModes.Email;
				recipient.AttachmentType = AttachmentTypeList.Codes.Xml;
				recipient.Email = @"unit.test@cargowise.com";

				var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);

				AssertEquals("Pre-condition: There should be one recipient.", 1, scheduledReport.Recipients.Count);

				var scheduledReportRecipient = scheduledReport.Recipients[0];
				scheduledReportRecipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var log = scheduledReport.Logs.AddNew(Events.EditedARecord, ZDateTimeOffset.Now.AddDays(1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				log.SL_GS_NKUser = staff.GS_Code;

				scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				scheduledReport.S5_ScheduleDescription = "My Scheduled Report";
				scheduledReport.Run();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.Recipients should match", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Email.CCRecipients should match", "unit.test@cargowise.com", email.CCRecipients.RecipientsAsDelimitedString());
				AssertEquals("Error Running Scheduled Report [My Scheduled Report]", email.Subject);

				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		public void TestRunScheduledReportAsXmlFileWithEmailNotificationWhenEmpty()
		{
			using (Report.TemporarilyUseMainConnection())
			{
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_VarCharMax = "Hello World";

				var reportCommand = Factory.New<ReportCommand>();
				var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report",
	@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""VarCharMax"", HeadingText=""VarCharMax""]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

				var pivot = reportCommand.Documents.AddNew();
				pivot.SI_SU = reportCommand.PK;
				pivot.SI_SO = template.PK;

				using (var documentPack = new DocumentPack(reportCommand))
				{
					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.Recipients.RemoveAndDeleteAll();

					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = ContactNotifyModes.Email;
					recipient.AttachmentType = AttachmentTypeList.Codes.Xml;
					recipient.Email = @"unit.test@cargowise.com";

					var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
					scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);

					AssertEquals("Pre-condition: There should be one recipient.", 1, scheduledReport.Recipients.Count);

					var scheduledReportRecipient = scheduledReport.Recipients[0];
					scheduledReportRecipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
					scheduledReport.Run();

					var query = new ZDBOnlyQuery(typeof(StmPrintJob));
					var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
					subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
					subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, @"unit.test@cargowise.com");
					query.AddSubQuery(subQuery, JoinCondition.And);

					var printJobs = Factory.Load<StmPrintJob>(query);

					AssertEquals("There should be one print job created.", 1, printJobs.Length);
					AssertXMLEquals("XML should be outputted to the print job.",
	@"<?xml version=""1.0"" encoding=""utf-8""?>
<DummyReport>
  <xs:schema xmlns="""" id=""DummyReport"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""DummyReportItem"">
      <xs:complexType>
        <xs:sequence>
          <xs:element name=""VarCharMax"" type=""xs:string"" minOccurs=""1"" maxOccurs=""1"" />
        </xs:sequence>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <DummyReportItem>
    <VarCharMax>Hello World</VarCharMax>
  </DummyReportItem>
</DummyReport>",
						printJobs[0].SP_CustomProperties.ToUTF8().ToString().TrimWithUnicodeWhitespace());
				}
			}
		}

		public void TestCreateDeliveryInfo()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");

			Factory.Save();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""Number"", HeadingText=""Number""]    {C}-[DisplayLabel=""VarCharMax"", HeadingText=""VarCharMax""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate, "DummyReport", ContactType.All, false))
			{
				var strategy = new XmlCreateDeliveryInfoStrategy();

				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

				AssertEquals("FileFormat should be XML.", "XML", result.FileFormat);

				using (var stream = result.FileContents)
				{
					AssertXMLEquals("Report XML",
@"﻿<?xml version=""1.0"" encoding=""utf-8""?>
<DummyReport>
  <xs:schema xmlns="""" id=""DummyReport"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""DummyReportItem"">
      <xs:complexType>
        <xs:sequence>
          <xs:element name=""Number"" type=""xs:string"" minOccurs=""1"" maxOccurs=""1"" />
          <xs:element name=""VarCharMax"" type=""xs:string"" minOccurs=""1"" maxOccurs=""1"" />
        </xs:sequence>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <DummyReportItem>
    <Number>1</Number>
    <VarCharMax>One</VarCharMax>
  </DummyReportItem>
  <DummyReportItem>
    <Number>2</Number>
    <VarCharMax>Two</VarCharMax>
  </DummyReportItem>
  <DummyReportItem>
    <Number>3</Number>
    <VarCharMax>Three</VarCharMax>
  </DummyReportItem>
</DummyReport>",
						Encoding.UTF8.GetString(stream.CopyToByteArray()));
				}
			}
		}

		void CreateDummyBusinessObject(int number, string text)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = number;
			dummy.Z0_VarCharMax = text;
		}

		void AddPostMasterGroupEmail()
		{
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";
			Factory.Save();
		}
	}
}
