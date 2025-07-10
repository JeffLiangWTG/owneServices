using System.Collections;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Business
{
	public class OrganisationXmlDataTransferExporter : XmlDataTransferExporter
	{
		public OrganisationXmlDataTransferExporter(StandardManualAndBatchImportOrganisationValueObjectDataAdapter adapter)
			: base(adapter, false)
		{
		}

		public OrganisationXmlDataTransferExporter()
			: this(new StandardManualAndBatchImportOrganisationValueObjectDataAdapter())
		{
		}

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			if (selectedElements.Count == 1)
			{
				OrgHeader organisation = selectedElements[0] as OrgHeader;
				DefaultFileName = (organisation != null) ? organisation.OH_Code + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss") : "";
			}
			base.PromptUserAndExportCore(selectedElements);
		}
	}
}
