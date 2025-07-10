using System.IO;
using System.Xml;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GlJournalXmlDataTransferDirector : XmlDataTransferDirector
	{
		public GlJournalXmlDataTransferDirector(GLJournalDataAdapter adapter, bool checkLicence)
			: base(adapter, checkLicence)
		{
		}

		protected void ImportJournalFromXml(Stream xmlFile, ValueObjectImportContext context)
		{
			XmlDocument journalXmlDocument = LoadXmlFileIntoXmlDoc(xmlFile);
			Xsd.GLJournal journalNode = ExtractJournalNodeFromXml(journalXmlDocument);
			ImportFromValueObject(journalNode, context);
		}

		#region implementations

		void ImportFromValueObject(Xsd.GLJournal journalXsd, ValueObjectImportContext context)
		{
			if (journalXsd != null)
			{
				GLJournalDataAdapter dataAdapter = new GLJournalDataAdapter();
				fLastImportedJournal = dataAdapter.CreateOrUpdateFromValueObject(journalXsd, context);
			}
		}

		protected XmlDocument LoadXmlFileIntoXmlDoc(Stream xmlFile)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(xmlFile);
			bool exceptionOccuredViaXmlManipulation = false;
			XmlDocument journalXmlDocument = new XmlDocument();

			try
			{
				journalXmlDocument.Load(xmlTextReader);
			}
			catch (XmlException)
			{
				exceptionOccuredViaXmlManipulation = true;
				Globals.Message.ShowError(GLJournalDataAdapter.InvalidXmlFileErrorMessage, Res.GetString("07374be6-8619-404c-94ba-88a11ddffbf3", "Invalid XML Document"));
			}

			return exceptionOccuredViaXmlManipulation ? null : journalXmlDocument;
		}

		protected Xsd.GLJournal ExtractJournalNodeFromXml(XmlDocument journalXmlDocument)
		{
			Xsd.GLJournal returnXsd = null;

			if (journalXmlDocument != null)
			{
				XmlNodeList gLJournalNodeList = journalXmlDocument.GetElementsByTagName("GLJournals");

				if (gLJournalNodeList.Count == 1)
				{
					XmlNode node = gLJournalNodeList[0];

					if (node.ChildNodes.Count > 1)
					{
						Globals.Message.ShowError(GLJournalDataAdapter.MoreThanOneJournalErrorMsg);
					}
					else
					{
						XmlNode gLJournalNode = node.ChildNodes[0];
						XmlValueObjectSerializer xmlValueObjectSerializer = new XmlValueObjectSerializer(typeof(Xsd.GLJournal));
						returnXsd = (Xsd.GLJournal)xmlValueObjectSerializer.Deserialize(new XmlNodeReader(gLJournalNode));
					}
				}
			}
			return returnXsd;
		}

		protected Xsd.GLJournalCollection ExtractJournalNodesFromXml(XmlDocument journalXmlDocument)
		{
			var returnXsd = new Xsd.GLJournalCollection();

			if (journalXmlDocument != null)
			{
				XmlNodeList gLJournalNodeList = journalXmlDocument.GetElementsByTagName("GLJournals");

				if (gLJournalNodeList.Count == 1)
				{
					XmlNode node = gLJournalNodeList[0];

					foreach (XmlNode gLJournalNode in node.ChildNodes)
					{
						var xmlValueObjectSerializer = new XmlValueObjectSerializer(typeof(Xsd.GLJournal));
						returnXsd.Add((Xsd.GLJournal)xmlValueObjectSerializer.Deserialize(new XmlNodeReader(gLJournalNode)));
					}
				}
			}
			return returnXsd;
		}

		public GLJournal LastImportedJournal
		{
			get { return fLastImportedJournal; }
		}

		GLJournal fLastImportedJournal;

		#endregion
	}
}
