using System.Data;

namespace CargoWise.EntityFramework
{
	public abstract class ZPersistentOperationPerformer
	{
		protected ZPersistentOperationPerformer(DataSet data)
		{
			this.Data = data;
		}

		protected readonly DataSet Data;
	}
}
