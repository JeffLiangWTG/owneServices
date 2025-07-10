namespace Enterprise.DbUpgrader.Data.BaseData.Company
{
	using Enterprise.DbUpgrader.Data.BaseData.Common;
	using Enterprise.ZArchitecture.Schema;

	class CompanyDataFile : BaseDataFile
	{
		public CompanyDataFile()
			: base(DataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"BaseData\Company\Company.xml";

		static readonly string[] DataFileTables = new string[]
		{
			GlbCompanySchema.Constants.TableName,
			GlbBranchSchema.Constants.TableName,
			GlbGroupSchema.Constants.TableName,
			GlbStaffSchema.Constants.TableName,
			GlbGroupRoleSchema.Constants.TableName,
			GlbGroupLinkSchema.Constants.TableName,
			GlbSecuritySchema.Constants.TableName,
		};
	}
}
