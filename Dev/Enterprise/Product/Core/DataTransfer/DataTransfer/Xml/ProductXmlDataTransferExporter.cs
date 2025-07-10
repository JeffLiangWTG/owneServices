using System.Collections;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Business
{
	public class ProductXmlDataTransferExporter : XmlDataTransferExporter
	{
		public ProductXmlDataTransferExporter(ProductValueObjectDataAdapter adapter)
			: base(adapter, false)
		{
		}

		public ProductXmlDataTransferExporter()
			: this(ProductValueObjectDataAdapter.New())
		{
		}

		#region Export

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			var product = (OrgSupplierPart)selectedElements[0];
			DefaultFileName = product.OP_PartNum + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss");

			base.PromptUserAndExportCore(selectedElements);
		}

		#endregion
	}
}
