using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Data
{
	public class RefLocalLanguageDataFile : EmbeddedDataFile
	{
		public RefLocalLanguageDataFile() : base(DataFileRelativePath, RefLocalLanguageDataFileTables)
		{
		}

		internal RefLocalLanguageDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, RefLocalLanguageDataFileTables)
		{
		}

		protected override string SelectQuery
		{
			get
			{
				return @"
					SELECT *
					FROM dbo.RefLocalLanguage
					WHERE RA_IsSystem = 1
					ORDER BY RA_PK
					";
			}
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\RefLocalLanguage\RefLocalLanguage.xml";
		public override string ResourceRelativeName => "RefLocalLanguage.RefLocalLanguage.xml";

		[ThreadSafe]
		[SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
		protected static readonly string[] RefLocalLanguageDataFileTables = new string[1] { RefLocalLanguageSchema.Constants.TableName };

		#endregion
	}
}
