using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class ExceptionTypesDataFile : EmbeddedDataFile
	{
		public ExceptionTypesDataFile() : base(DataFileRelativePath, ProcessWorkflowExceptionTypeSchema.Constants.TableName)
		{
		}

		internal ExceptionTypesDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, ProcessWorkflowExceptionTypeSchema.Constants.TableName)
		{
		}

		const string DataFileRelativePath = @"ExceptionTypes\ExceptionTypes.xml";

		protected override string SelectQuery
		{
			get
			{
				return @"
					SELECT *
					FROM dbo.ProcessWorkflowExceptionType
					WHERE WET_IsSystem = 1
					ORDER BY WET_PK
					";
			}
		}
	}
}
