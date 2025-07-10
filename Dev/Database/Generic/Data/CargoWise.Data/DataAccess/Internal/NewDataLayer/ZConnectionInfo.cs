namespace CargoWise.EntityFramework
{
	public abstract class ZConnectionInfo
	{
		protected ZConnectionInfo()
		{
		}

		public abstract string PathToTables { get; }
	}
}
