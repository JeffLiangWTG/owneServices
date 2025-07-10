using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data.BaseData.System
{
	class SystemDataFile : BaseDataFile
	{
		public SystemDataFile()
			: base(DataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"BaseData\System\SystemBaseData.xml";

		static readonly string[] DataFileTables = new string[]
		{
			FaxDeviceConfigSchema.Constants.TableName,
			FaxRouteSchema.Constants.TableName
		};
	}
}
