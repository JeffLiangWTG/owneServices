using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.DbUpgrader.Data.BaseData.Accounting
{
	class AccountingDataFile : BaseDataFile
	{
		public AccountingDataFile()
			: base(DataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"BaseData\Accounting\Accounting.xml";

		static readonly string[] DataFileTables = new string[]
		{
			OrgCreditorGroupSchema.Constants.TableName,
			OrgDebtorGroupSchema.Constants.TableName,
			AccGLHeaderSchema.Constants.TableName,
			AccGroupsSchema.Constants.TableName,
			AccTaxRateSchema.Constants.TableName,
			AccChargeCodeSchema.Constants.TableName,
		};
	}
}
