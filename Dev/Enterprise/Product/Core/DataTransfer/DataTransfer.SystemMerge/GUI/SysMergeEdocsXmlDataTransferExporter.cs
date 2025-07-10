using System.Collections;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeEdocsXmlDataTransferExporter : SysMergeXmlDataTransferExporter
	{
		public SysMergeEdocsXmlDataTransferExporter()
			: base(new SysMergeEdocsValueObjectDataAdapter())
		{
		}

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			if (selectedElements.Count < 1)
			{
				return;
			}

			try
			{
				var docs = GetDocsForAllSelectedOrganisations(selectedElements);
				base.PromptUserAndExportCore(docs);
			}
			catch (ExternalStorageException ex)
			{
				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder.AppendLine(Res.GetString("CC89ABD8-E7C2-4B21-8228-4D22A2E5C5BD", "An error occurred when loading eDocs for all selected organizations."));
				errorMessageBuilder.AppendLine(ex.UnableToAccessStorageFriendlyMessage);

				Globals.Message.ShowError(errorMessageBuilder.ToString());
			}
		}

		StorageDocsForDataTransfer[] GetDocsForAllSelectedOrganisations(IList selectedElements)
		{
			var exportFactory = new BusinessObjectFactory();
			var docFactory = new DocumentFactoryProvider().GetFactory(exportFactory);
			var allDocs = new List<StorageDocsForDataTransfer>();

			foreach (BusinessObject parentOrg in selectedElements)
			{
				allDocs.AddRange(GetOrganisationDocs(docFactory, exportFactory, parentOrg));
			}

			return allDocs.ToArray();
		}

		IEnumerable<StorageDocsForDataTransfer> GetOrganisationDocs(DocumentFactory docFactory, BusinessObjectFactory exportFactory, BusinessObject parentOrg)
		{
			var result = new List<StorageDocsForDataTransfer>();
			var storageMain = docFactory.GetStorageMainForPK(parentOrg.PK);

			if (storageMain != null)
			{
				var query = new ZQuery(StorageDocsSchema.SC_SM, storageMain.PK);
				var loadedDocs = docFactory.GetFactory(storageMain.SM_DB).Load<StorageDocsForDataTransfer>(query);

				foreach (var loadedDoc in loadedDocs)
				{
					if (loadedDoc.SC_IsDeleted || loadedDoc.SC_ImageData.IsEmpty)
					{
						continue;
					}

					var doc = (StorageDocsForDataTransfer)exportFactory.ImportFromAnotherFactory(loadedDoc);
					doc.ParentOrgPk = parentOrg.PK;
					result.Add(doc);
				}
			}

			return result;
		}
	}
}
