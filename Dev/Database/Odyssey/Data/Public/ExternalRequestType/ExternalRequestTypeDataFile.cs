using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class ExternalRequestTypeDataFile : EmbeddedDataFile
	{
		public ExternalRequestTypeDataFile() : base(DataFileRelativePath, ExternalRequestTypeSchema.Constants.TableName)
		{
		}

		internal ExternalRequestTypeDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, ExternalRequestTypeSchema.Constants.TableName)
		{
		}

		const string DataFileRelativePath = @"ExternalRequestType\ExternalRequestType.xml";

		protected override string SelectQuery
		{
			get
			{
				return @"
					SELECT *
					FROM dbo.ExternalRequestType
					WHERE RQT_IsSystem = 1
					ORDER BY RQT_PK
					";
			}
		}
	}
}
