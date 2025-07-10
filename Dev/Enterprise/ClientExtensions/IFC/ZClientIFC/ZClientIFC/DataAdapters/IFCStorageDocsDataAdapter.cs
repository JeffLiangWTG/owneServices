
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentScanning.DataTransfer;

namespace Enterprise.Client.IFC
{
	public class IFCStorageDocsValueObjectDataAdapter : StorageDocsValueObjectDataAdapter
	{
		public IFCStorageDocsValueObjectDataAdapter()
			: base()
		{
		}

		public IFCStorageDocsValueObjectDataAdapter(BusinessObject parentBizObj)
			: base(parentBizObj)
		{
		}

		protected override OutputFormatType ExportFormatType
		{
			get
			{
				return OutputFormatType.PDF;
			}
		}
	}
}
