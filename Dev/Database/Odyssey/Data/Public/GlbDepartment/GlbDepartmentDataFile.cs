using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	/// <summary>
	/// Summary description for GlbDepartmentDataFile.
	/// </summary>
	public class GlbDepartmentDataFile : EmbeddedDataFile
	{
		public GlbDepartmentDataFile() : base(DataFileRelativePath, GlbDepartmentSchema.Constants.TableName)
		{
		}

		internal GlbDepartmentDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, GlbDepartmentSchema.Constants.TableName)
		{
		}

		protected override string SelectQuery
		{
			get { return "SELECT * FROM dbo.GlbDepartment WHERE GE_SystemCode = 1 order by 1"; }
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\GlbDepartment\GlbDepartment.xml";
		public override string ResourceRelativeName => "GlbDepartment.GlbDepartment.xml";

		#endregion
	}
}
