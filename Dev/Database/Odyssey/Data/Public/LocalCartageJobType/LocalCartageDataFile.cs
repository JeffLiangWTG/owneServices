using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	/// <summary>
	/// Summary description for LocalCartageJobTypeDataFile.
	/// </summary>
	public class LocalCartageDataFile : EmbeddedDataFile
	{
		public LocalCartageDataFile() : base(DataFileRelativePath, LocalCartageJobTypeDataFileTables)
		{
		}

		internal LocalCartageDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, LocalCartageJobTypeDataFileTables)
		{
		}

		protected override string SelectQuery
		{
			get
			{
				return @"
SELECT * FROM dbo.LocalCartageJobType WHERE E3_IsSystem = 1 order by 1;

SELECT LocalCartageJobOrg.* FROM dbo.LocalCartageJobOrg 
	INNER JOIN dbo.LocalCartageJobType
	ON E5_E3 = E3_PK
WHERE E3_IsSystem = 1 order by 1;

SELECT LocalCartageJobLegType.* FROM dbo.LocalCartageJobLegType
	INNER JOIN dbo.LocalCartageJobType
	ON E4_E3 = E3_PK
WHERE E3_IsSystem = 1 order by 1";
			}
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\LocalCartageJobType\LocalCartage.xml";
		public override string ResourceRelativeName => "LocalCartageJobType.LocalCartage.xml";

		protected static readonly string[] LocalCartageJobTypeDataFileTables = new string[] { LocalCartageJobTypeSchema.Constants.TableName, LocalCartageJobOrgSchema.Constants.TableName, LocalCartageJobLegTypeSchema.Constants.TableName };

		#endregion
	}
}
