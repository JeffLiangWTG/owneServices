using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	sealed class GLJournalXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidXmlImport()
		{
			GLJournalXmlDataTransferDirectorTestClass director = new GLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);

			using (Stream reader = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidGJLJournal.xml"))
			{
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				director.ImportJournalFromXml(reader, context);
			}

			GLJournal journal = director.LastImportedJournal;

			AssertEquals(200501, journal.PostPeriod);
			AssertEquals(0, journal.AgePeriod);
			AssertEquals(2, journal.GLJournalLines.Count);

			AssertEquals("2010.00.00", journal.GLJournalLines[0].GLHeader.AccountNum);
			AssertEquals("BNE", journal.GLJournalLines[0].Branch.GB_Code);
			AssertEquals("BRN", journal.GLJournalLines[0].Department.GE_Code);
			AssertEquals(new ZDecimal(10), journal.GLJournalLines[0].UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), journal.GLJournalLines[0].DebitCreditSign);

			AssertEquals("2020.00.00", journal.GLJournalLines[1].GLHeader.AccountNum);
			AssertEquals("SYD", journal.GLJournalLines[1].Branch.GB_Code);
			AssertEquals("CIA", journal.GLJournalLines[1].Department.GE_Code);
			AssertEquals("Test Line 2", journal.GLJournalLines[1].AL_Desc);
			AssertEquals(new ZDecimal(10), journal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals(new ZString("CR"), journal.GLJournalLines[1].DebitCreditSign);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidXmlImport()
		{
			GLJournalXmlDataTransferDirectorTestClass director = new GLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);

			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			using (Stream reader = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\InvalidGLJournal.xml"))
			{
				XmlDocument xmlDoc = director.LoadXmlDoc(reader);
			}

			AssertEquals(GLJournalDataAdapter.InvalidXmlFileErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMoreThanOneJournals()
		{
			GLJournalXmlDataTransferDirectorTestClass director = new GLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);

			using (Stream reader = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidGJL2Journals.xml"))
			{
				XmlDocument xmlDoc = director.LoadXmlDoc(reader);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				Xsd.GLJournal jxsd = director.ExtractJournalNode(xmlDoc);
			}

			AssertEquals(GLJournalDataAdapter.MoreThanOneJournalErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidDRCR_ThrowsXMLException()
		{
			var director = new GLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);
			using (var reader = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\InvalidGLJournalDRCR.xml"))
			{
				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertExceptionThrown("Expected exception as <DRCR> field contained invalid value", typeof(XmlException), () => director.ImportJournalFromXml(reader, context));
			}
		}

		class GLJournalXmlDataTransferDirectorTestClass : GlJournalXmlDataTransferDirector
		{
			public GLJournalXmlDataTransferDirectorTestClass(GLJournalDataAdapter adapter, bool hasLicence)
				: base(adapter, hasLicence)
			{
			}

			public XmlDocument LoadXmlDoc(Stream xmlDocStream)
			{
				return base.LoadXmlFileIntoXmlDoc(xmlDocStream);
			}

			public Xsd.GLJournal ExtractJournalNode(XmlDocument journalXMLDocument)
			{
				return base.ExtractJournalNodeFromXml(journalXMLDocument);
			}

			public new void PromptUserAndImport(BillingInterfaceName interfaceName)
			{
				base.PromptUserAndImport(interfaceName);
			}

			public new void ImportJournalFromXml(Stream xmlFile, ValueObjectImportContext context)
			{
				base.ImportJournalFromXml(xmlFile, context);
			}
		}
	}
}
