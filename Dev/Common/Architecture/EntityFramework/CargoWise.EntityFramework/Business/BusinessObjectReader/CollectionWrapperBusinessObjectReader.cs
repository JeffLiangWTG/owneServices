using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class CollectionWrapperBusinessObjectReader : BusinessObjectReader
	{
		public CollectionWrapperBusinessObjectReader(IBusinessObjectCollection collectionToWrap) : base(new BusinessObjectFactoryProvider())
		{
			WrappedCollection = collectionToWrap;
		}

		protected override BusinessObject[] LoadNextBatchCore(BusinessObject lastBusinessObjectRead)
		{
			return GetNextBatchWithBusinessObject(lastBusinessObjectRead);
		}

		protected override BusinessObject[] LoadNextBatchCore(ZGuid pk)
		{
			return GetNextBatchWithBusinessObject(Factory.Load(BusinessObjectType, pk));
		}

		BusinessObject[] GetNextBatchWithBusinessObject(BusinessObject lastBusinessObjectRead)
		{
			BusinessObject[] bizObjs;

			if (lastBusinessObjectRead == null && WrappedCollection.Count != 0)
			{
				bizObjs = WrappedCollection.ToArray();
			}
			else
			{
				bizObjs = Array.Empty<BusinessObject>();
			}

			return bizObjs;
		}

		public override int ApproximateCount
		{
			get { return WrappedCollection.Count; }
		}

		public override bool HasRecords
		{
			get { return WrappedCollection.Count > 0; }
		}

		public override Type BusinessObjectType
		{
			get { return WrappedCollection.TypeOfElements; }
		}

		public readonly IBusinessObjectCollection WrappedCollection;
	}
}
