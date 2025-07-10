using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(DocumentSplitResultInfo))]
	public class DocumentSplitResultInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			var splitManager = new DocumentSplitManager(doc);
			return splitManager.DocumentSplitResultCollection.AddNew();
		}

		DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		#endregion

	}
}
