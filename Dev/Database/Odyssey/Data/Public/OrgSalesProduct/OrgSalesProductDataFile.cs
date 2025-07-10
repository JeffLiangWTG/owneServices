using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(OrgSalesProductSchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class OrgSalesProductDataFile : EmbeddedDataFile
	{
		public OrgSalesProductDataFile() : base(DataFileRelativePath, OrgSalesProductSchema.Constants.TableName)
		{
		}

		internal OrgSalesProductDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, OrgSalesProductSchema.Constants.TableName)
		{
		}

		const string DataFileRelativePath = @"Public\OrgSalesProduct\OrgSalesProduct.xml";
		public override string ResourceRelativeName => "OrgSalesProduct.OrgSalesProduct.xml";

		protected override string SelectQuery
		{
			get
			{
				return
" SELECT * " +
" FROM " + OrgSalesProductSchema.Constants.SqlSchemaName + "." + OrgSalesProductSchema.Constants.TableName +
" WHERE " + OrgSalesProductSchema.Constants.MP_IsSystemDefined + " = 1 " +
" ORDER BY 1 ";
			}
		}
	}
}
