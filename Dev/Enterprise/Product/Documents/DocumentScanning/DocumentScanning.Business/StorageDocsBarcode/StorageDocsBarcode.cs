using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsBarcode : AutoStorageDocsBarcode, ICanBeSavedByDocumentFactory
	{
		public StorageDocsBarcode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!(factory is NumberedBusinessObjectFactory))
			{
				throw new ArgumentException("StorageDocsBarcode can only be loaded in a NumberedBusinessObjectFactory", nameof(factory));
			}
		}

		[RelatedBusinessObject("StorageDOc")]
		public override CargoWise.Types.ZGuid SCB_SC
		{
			get
			{
				return base.SCB_SC;
			}
			set
			{
				base.SCB_SC = value;
			}
		}

		public virtual StorageDocs StorageDOc
		{
			get
			{
				return Factory.Load<StorageDocs>(SCB_SC);
			}
		}
	}
}
