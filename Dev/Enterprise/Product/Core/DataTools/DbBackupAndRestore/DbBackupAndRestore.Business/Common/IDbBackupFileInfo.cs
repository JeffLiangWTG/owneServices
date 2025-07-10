using System.Numerics;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Common
{
	public interface IDbBackupFileInfo
	{
		string DatabaseName { get; }
		BigInteger LastLSN { get; }
	}
}
