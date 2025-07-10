namespace CargoWise.EntityFramework
{
	public class BusinessObjectCloneStrategy
	{
		public BusinessObjectCloneStrategy(BusinessObject bizObj)
		{
			this.bizObjToClone = bizObj;
		}

		protected readonly BusinessObject bizObjToClone;

		public BusinessObject Clone(BusinessObjectCloneArgs args)
		{
			return CloneInternal(args);
		}

		protected virtual BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			return bizObjToClone.Clone(args);
		}
	}
}
