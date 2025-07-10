using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class OrganisationXmlDataImporterTest : XmlDataImporterTest
	{
		public void TestAfterImportXml()
		{
			OrganisationValueObjectDataAdapter adapter = new OrganisationValueObjectDataAdapter();
			SingleBusinessObjectFactoryProvider factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);
			OrganisationXmlDataImporterForTest importer = new OrganisationXmlDataImporterForTest(factoryProvider, adapter);
			NotificationBuffer notify = new NotificationBuffer();

			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithNoError)))
			using (TextReader reader = new StreamReader(stream))
			{
				ITransactionParticipant[] additionalTransactionActions;
				importer.ImportDataToFactory(reader, "", notify, SourceInfo.EmptySourceInfo, out additionalTransactionActions);
				List<ITransactionParticipant> forSave = new List<ITransactionParticipant>();
				forSave.AddRange(additionalTransactionActions);
				Factory.Save();
				AssertEquals(1, importer.OrgStatuses.Count);
				AssertEquals(" created", importer.OrgStatuses[Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Imported Name")).PK]);

				importer.ImportDataToFactory(reader, "", notify, SourceInfo.EmptySourceInfo, out additionalTransactionActions);
				Factory.Save();
				AssertEquals(1, importer.OrgStatuses.Count);
				AssertEquals(" updated", importer.OrgStatuses[Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Imported Name")).PK]);
			}
		}

		class OrganisationXmlDataImporterForTest : OrganisationXmlDataImporter
		{
			public OrganisationXmlDataImporterForTest(IValueObjectDataAdapter adapter)
				: base(adapter)
			{
			}

			public OrganisationXmlDataImporterForTest(BusinessObjectFactoryProvider factoryProvider, IValueObjectDataAdapter adapter)
				: base(factoryProvider, adapter)
			{
			}

			public new Hashtable OrgStatuses
			{
				get
				{
					return base.OrgStatuses;
				}
			}

			public new void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
			{
				base.OnAfterImportData(buffer, sucessfullyImported);
			}
		}
	}
}
