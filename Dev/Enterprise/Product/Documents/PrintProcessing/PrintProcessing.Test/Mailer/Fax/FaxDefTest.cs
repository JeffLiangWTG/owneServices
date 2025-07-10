using System;
using System.Collections.Specialized;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.FaxRouter.MailSecurity;
using Enterprise.Integration.Licensing;
using Enterprise.PrintProcessing.Mailer.Fax;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Test.Mailer.Fax
{
	sealed class FaxDefTest : TestCaseWithFactory
	{
		ICryptographicProvider cryptographicProvider;

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			cryptographicProvider = new CryptProvider();
		}

		public void TestFaxNumber()
		{
			var fTestFaxNumber = "9555-5555";
			var fFaxDef = new FaxDef();
			fFaxDef.FaxNumber = fTestFaxNumber;
			AssertEquals("Fax number accessible", fTestFaxNumber, fFaxDef.FaxNumber);
		}

		public void TestFaxAttention()
		{
			var fTestFaxAttention = "Mr. Dobalina";
			var fFaxDef = new FaxDef();
			fFaxDef.FaxAttention = fTestFaxAttention;
			AssertEquals("Fax attention accessible", fTestFaxAttention, fFaxDef.FaxAttention);
		}

		public void TestFaxAttentionCompany()
		{
			var fTestFaxAttentionCompany = "Bob Dobalina & Sons";
			var fFaxDef = new FaxDef();
			fFaxDef.FaxAttentionCompany = fTestFaxAttentionCompany;
			AssertEquals("Fax attention company accessible", fTestFaxAttentionCompany, fFaxDef.FaxAttentionCompany);
		}

		public void TestSysFaxJobId()
		{
			var fSysFaxJobId = Guid.NewGuid().ToString();
			var fFaxDef = new FaxDef();
			fFaxDef.SysFaxJobId = fSysFaxJobId;
			AssertEquals("System fax job id accessible", fSysFaxJobId, fFaxDef.SysFaxJobId);
		}

		public void TestSysId()
		{
			var fSysId = "WOWSYD";
			var fFaxDef = new FaxDef();
			fFaxDef.SysId = fSysId;
			AssertEquals("System id accessible", fSysId, fFaxDef.SysId);
		}

		public void TestFaxFileName()
		{
			var fFaxFileName = "fax.tif";
			var fFaxDef = new FaxDef();
			fFaxDef.FaxFileName = fFaxFileName;
			AssertEquals("Fax file name accessible", fFaxFileName, fFaxDef.FaxFileName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestBatchFax()
		{
			var meFax = new FaxDef();
			meFax.FaxAttention = "Hulk";
			meFax.FaxAttentionCompany = "Hulking Co. Ltd.";
			meFax.FaxNumber = "0290251199";
			meFax.FaxFileName = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestFaxTifFileName;
			meFax.Send();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2002, 3, 4, 5, 6, 7)]
		public void TestSend()
		{
			AssertSend("+61 2 9025 1199", "+61 2 9025 1199");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2002, 3, 4, 5, 6, 7)]
		public void TestSendWithOverride()
		{
			var value = Env.Registry.FaxDestinationOverride;
			try
			{
				Env.Registry.FaxDestinationOverride = "+61 2 4444 1111";
				AssertSend("+61 2 9025 1199", "+61 2 4444 1111");
			}
			finally
			{
				Env.Registry.FaxDestinationOverride = value;
			}
		}

		public void AssertSend(string initialFaxNum, string expectedFaxnum)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "PHY";

			Env.Registry.MailboxEmailAddress = "test@fax.com";
			var def = FaxDef.New();
			def.FaxAttention = "My fax attention";
			def.FaxAttentionCompany = "My fax attention company";
			def.FaxFileName = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestFaxTifFileName;
			def.FaxNumber = initialFaxNum;
			def.SysFaxJobId = "jobid";
			def.SysId = "sysid";
			def.SendingCompanyCode = "COD";
			def.Send();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Outbound fax mail with recipients", 1, email.Recipients.Count);
			AssertEquals("Outbound fax mail with attachment", 2, email.Attachments.Count);
			AssertEquals("Recipient should be fax gateway", Core.Constants.EmailAddresses.EDI_FAX_GATEWAY, email.Recipients[0]);
			AssertEquals("Outbound fax mail should be from test@fax.com", "test@fax.com", email.FromAddress);

			var attachmentNames = new StringCollection();
			for (var i = 0; i < email.Attachments.Count; i++)
			{
				_ = attachmentNames.Add(email.Attachments[i].DisplayName);
			}
			var expectedFilename = PrintProcessingConstants.TestGeneratedReport.Substring(0, PrintProcessingConstants.TestGeneratedReport.IndexOf(".")) + ".TIF";
			Assert("Fax image attachment", attachmentNames.Contains(expectedFilename));
			Assert("Fax command attachment", attachmentNames.Contains("FaxCommand.base64"));

			var encodedBytes = email.Attachments[attachmentNames.IndexOf("FaxCommand.base64")].Data;
			var encodedString = Encoding.ASCII.GetString(encodedBytes);
			var commandBytes = Convert.FromBase64String(encodedString);
			var commandString = Encoding.UTF8.GetString(commandBytes);

			Assert("Should include fax number", commandString.IndexOf("FAXNUMBER=" + expectedFaxnum) >= 0);
			Assert("Should include fax attention", commandString.IndexOf("FAXATTENTION=My fax attention") >= 0);
			Assert("Should include fax attention company", commandString.IndexOf("FAXATTENTIONCOMPANY=My fax attention company") >= 0);
			Assert("Should include sys fax job id", commandString.IndexOf("SYSFAXJOBID=jobid") >= 0);
			Assert("Should include sys id", commandString.IndexOf("SYSID=sysid") >= 0);
			Assert("Should include sent date time", commandString.IndexOf("SENTDATETIME=" + new DateTime(2002, 3, 4, 5, 6, 7).ToString(cryptographicProvider.GetDateTimeFormat())) >= 0);
			Assert("Should include fax key", commandString.IndexOf("FAXKEY=" + cryptographicProvider.GenerateKeyFromFile(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestFaxTifFileName, new DateTime(2002, 3, 4, 5, 6, 7))) >= 0);
			Assert("Should include Enterprise code", commandString.IndexOf("ENTERPRISECODE=ENT") >= 0);
			Assert("Should include Company code", commandString.IndexOf("COMPANYCODE=COD") >= 0);
			Assert("Should include Physical Server ID", commandString.IndexOf("PHYSICALSERVERID=PHY") >= 0);
		}

		[TestDate(2002, 3, 4, 5, 6, 7)]
		public void TestSendForLicensingOnlyWithoutAttachment()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "PHY";

			var def = FaxDef.New();
			def.FaxNumber = "+61 2 9025 1199";
			def.SysFaxJobId = "jobid";
			def.SysId = "sysid";
			def.SendingCompanyCode = "COD";
			def.Send();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Outbound fax mail with recipients", 1, email.Recipients.Count);
			AssertEquals("Outbound fax mail with attachment", 1, email.Attachments.Count);
			AssertEquals("Recipient should be fax gateway", Core.Constants.EmailAddresses.EDI_FAX_GATEWAY, email.Recipients[0]);
			AssertEquals("Fax command attachment", "FaxCommand.base64", email.Attachments[0].DisplayName);

			var encodedBytes = email.Attachments[0].Data;
			var encodedString = Encoding.ASCII.GetString(encodedBytes);
			var commandBytes = Convert.FromBase64String(encodedString);
			var commandString = Encoding.UTF8.GetString(commandBytes);

			Assert("Should include fax number", commandString.IndexOf("FAXNUMBER=+61 2 9025 1199") >= 0);
			Assert("Should include sys fax job id", commandString.IndexOf("SYSFAXJOBID=jobid") >= 0);
			Assert("Should include sys id", commandString.IndexOf("SYSID=sysid") >= 0);
			Assert("Should include sent date time", commandString.IndexOf("SENTDATETIME=" + new DateTime(2002, 3, 4, 5, 6, 7).ToString(cryptographicProvider.GetDateTimeFormat())) >= 0);
			Assert("Should include Enterprise code", commandString.IndexOf("ENTERPRISECODE=ENT") >= 0);
			Assert("Should include Company code", commandString.IndexOf("COMPANYCODE=COD") >= 0);
			Assert("Should include Physical Server ID", commandString.IndexOf("PHYSICALSERVERID=PHY") >= 0);
		}

		[TestDateIncremental(1, 0, 0, 0)]
		public void TestSubjectOfEmailIsSomewhatUniqueInOrderToMakeUseOfIndex()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "PHY";

			var def1 = FaxDef.New();
			def1.FaxNumber = "+61 2 9025 1199";
			def1.SysFaxJobId = "jobid";
			def1.SysId = "sysid";
			def1.SendingCompanyCode = "COD";
			def1.Send();

			var def2 = FaxDef.New();
			def2.FaxNumber = "+61 2 9025 1199";
			def2.SysFaxJobId = "leasureid";
			def2.SysId = "nonsysid";
			def2.SendingCompanyCode = "COD";
			def2.Send();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingMailManager.EmailsCreated[0];
			var email2 = Env.OutgoingMailManager.EmailsCreated[1];

			AssertNotEquals("Subjects should be different", email1.Subject, email2.Subject);

			Assert("Subjects1 should contain EDI_FAX_DOC_SUBJECT", email1.Subject.Contains(FaxDef.EDI_FAX_DOC_SUBJECT));
			Assert("Subjects2 should contain EDI_FAX_DOC_SUBJECT", email2.Subject.Contains(FaxDef.EDI_FAX_DOC_SUBJECT));
		}
	}
}
