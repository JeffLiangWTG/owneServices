namespace CargoWise.EntityFramework
{
	public abstract class AbstractFilterPart
	{
		public virtual bool HasParameters
		{
			get { return false; }
		}

		public virtual bool HasComparisonOperatorLike
		{
			get { return false; }
		}
	}
}
