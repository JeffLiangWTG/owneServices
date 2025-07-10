using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	class GLJournalFlatFileDataImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestExtractToDataAdapter()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			XmlDocument xmlDoc;
			using (Stream reader = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidGJLJournal.csv"))
			{
				xmlDoc = fDirector.LoadXmlDoc(reader);
			}

			Xsd.GLJournal xsd = fDirector.ExtractJournalNode(xmlDoc);
			fImporter.ExtractToDataAdapter(xsd, new NotificationBuffer());

			AssertEquals("GJL", fImporter.LastImportedJournal.AH_TransactionType);
		}

		public virtual void TestReturnValueFromExtractToDataAdapter()
		{
			AssertEquals("Should return false so it doesn't save", false, fImporter.ExtractToDataAdapter(null, new NotificationBuffer()));
		}

		#region Implementation

		GLJournalFlatFileDataImporterTestClass fImporter;
		GLJournalXmlDataTransferDirectorTestClass fDirector;

		protected override void SetUp()
		{
			fImporter = new GLJournalFlatFileDataImporterTestClass();
			fDirector = new GLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);
			base.SetUp();
		}

		class GLJournalFlatFileDataImporterTestClass : GLJournalFlatFileDataImporter
		{
			public new bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xsd, notifications);
			}
		}

		protected class GLJournalXmlDataTransferDirectorTestClass : GlJournalXmlDataTransferDirector
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

			public Xsd.GLJournalCollection ExtractJournalNodes(XmlDocument journalXMLDocument)
			{
				return base.ExtractJournalNodesFromXml(journalXMLDocument);
			}
		}

		#endregion
	}
}
