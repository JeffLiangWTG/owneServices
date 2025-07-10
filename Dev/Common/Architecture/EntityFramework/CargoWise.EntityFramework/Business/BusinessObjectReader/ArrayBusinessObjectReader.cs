using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class ArrayBusinessObjectReader : BusinessObjectReader
	{
		public ArrayBusinessObjectReader(IBusinessObjectCollection collectionToWrap)
			: base(new BusinessObjectFactoryProvider())
		{
			bizOs = collectionToWrap.ToArray();
			businessObjectType = collectionToWrap.TypeOfElements;
		}

		public ArrayBusinessObjectReader(BusinessObject[] bizOs, Type businessObjectType)
			: base(new BusinessObjectFactoryProvider())
		{
			this.bizOs = bizOs;
			this.businessObjectType = businessObjectType;
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
			if (lastBusinessObjectRead == null)
			{
				return bizOs;
			}
			else
			{
				return Array.Empty<BusinessObject>();
			}
		}

		public override int ApproximateCount
		{
			get { return bizOs.Length; }
		}

		public override bool HasRecords
		{
			get { return bizOs.Length > 0; }
		}

		public override Type BusinessObjectType
		{
			get { return businessObjectType; }
		}

		public readonly BusinessObject[] bizOs;
		public readonly Type businessObjectType;
	}
}
