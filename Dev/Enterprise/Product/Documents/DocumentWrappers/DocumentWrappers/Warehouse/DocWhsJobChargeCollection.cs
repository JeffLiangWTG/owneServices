using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsJobChargeCollection : DocJobChargeCollection
	{
		protected DocWhsJobChargeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocWhsJobChargeCollection GetCollection(DocumentWrapper parent, string collectionName, Action<DocWhsJobChargeCollection> populateCollection)
			=> CollectionGetter<DocWhsJobChargeCollection, DocWhsJobCharge>.GetCollection(parent, collectionName, populateCollection);

		public static new DocWhsJobChargeCollection GetCollection(DocumentWrapper parent, string collectionName, params JobCharge[] jobChargesToAdd)
			=> CollectionGetter<DocWhsJobChargeCollection, DocWhsJobCharge>.GetCollection(parent, collectionName, jobChargesToAdd);

		public new DocWhsJobCharge this[int index]
		{
			get { return (DocWhsJobCharge)base[index]; }
		}
	}
}
