using CargoWise.EntityFramework;

namespace Enterprise.Billing.Integration
{
	static class FactoryBillingExtensions
	{
		public static FactorySnapshot RecordSnapshot(this BusinessObjectFactory factory)
		{
			return new FactorySnapshot(factory);
		}

		public static ObjectStatusEnum GetStatus(this BusinessObject obj)
		{
			if (obj.IsDeleted)
			{
				return ObjectStatusEnum.Deleted;
			}

			if (!obj.IsInDatabase)
			{
				return ObjectStatusEnum.New;
			}

			if (obj.IsRowChanged)
			{
				return ObjectStatusEnum.Updated;
			}

			return ObjectStatusEnum.Unchanged;
		}
	}
}