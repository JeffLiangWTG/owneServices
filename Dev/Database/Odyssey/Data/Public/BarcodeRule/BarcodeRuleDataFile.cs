using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class BarcodeRuleDataFile : EmbeddedDataFile
	{
		public BarcodeRuleDataFile() : base(DataFileRelativePath, BarcodeRuleDataFileTables)
		{
		}

		internal BarcodeRuleDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, BarcodeRuleDataFileTables)
		{
		}

		const string DataFileRelativePath = @"Public\BarcodeRule\BarcodeRule.xml";
		public override string ResourceRelativeName => "BarcodeRule.BarcodeRule.xml";

		protected static readonly string[] BarcodeRuleDataFileTables = { BarcodeRuleSetSchema.Constants.TableName, BarcodeRuleSchema.Constants.TableName, BarcodeRuleComponentSchema.Constants.TableName };

		protected override string SelectQuery
		{
			get
			{
				return @"
SELECT *
FROM dbo.BarcodeRuleSet
WHERE BRS_IsSystem = 1
ORDER BY 1

SELECT BarcodeRule.*
FROM dbo.BarcodeRule
	JOIN dbo.BarcodeRuleSet ON BRU_BRS_RuleSet = BRS_PK
WHERE BRS_IsSystem = 1
ORDER BY 1

SELECT BarcodeRuleComponent.*
FROM dbo.BarcodeRuleComponent
	JOIN dbo.BarcodeRule ON BRC_BRU_Rule = BRU_PK
	JOIN dbo.BarcodeRuleSet ON BRU_BRS_RuleSet = BRS_PK
WHERE BRS_IsSystem = 1
ORDER BY 1
";
			}
		}
	}
}
