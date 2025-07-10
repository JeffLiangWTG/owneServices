using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Common
{
	public interface IDbHeaderOnlyReader
	{
		IDbBackupFileInfo ReadHeaderOnly(DbConnection connection, string filePath);
	}
}
