using CargoWise.Types;

namespace Enterprise.DataPurge.Utility
{
	public class SqlNode : Node
	{
		public SqlNode(string companyFKName, ZGuid companyPk, string pkTableName, string fkTableName, string pkName, string fkName)
			: base(pkTableName, fkTableName, pkName, fkName)
		{
			CompanyFKName = companyFKName;
			CompanyPk = companyPk;
		}

		public ZGuid CompanyPk { get; private set; }
		public string CompanyFKName { get; private set; }
	}
}
