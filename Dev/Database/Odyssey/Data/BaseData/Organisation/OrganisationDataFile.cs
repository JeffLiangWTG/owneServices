namespace Enterprise.DbUpgrader.Data.BaseData.Organisation
{
	using Enterprise.DbUpgrader.Data.BaseData.Common;
	using Enterprise.ZArchitecture.Schema;

	class OrganisationDataFile : BaseDataFile
	{
		public OrganisationDataFile()
			: base(DataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"BaseData\Organisation\Organisation.xml";

		static readonly string[] DataFileTables = new string[]
		{
			OrgHeaderSchema.Constants.TableName,
			OrgAddressSchema.Constants.TableName,
			OrgAddressCapabilitySchema.Constants.TableName,
			OrgWebURLSchema.Constants.TableName,
		};
	}
}
