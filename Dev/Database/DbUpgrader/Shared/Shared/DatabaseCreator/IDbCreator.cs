using CargoWise.Data;

namespace Enterprise.DbUpgrader.Shared
{
	public interface IDbCreator
	{
		void CreateIfNotExists(AdminConnection conn);
		void Drop(AdminConnection conn);
		void CreateDropExisting(AdminConnection conn);
	}
}
