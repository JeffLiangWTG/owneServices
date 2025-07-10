using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	/// <summary>
	/// Summary description for TransportBookingTemplateDataFile.
	/// </summary>
	public class TransportBookingTemplateDataFile : EmbeddedDataFile
	{
		public TransportBookingTemplateDataFile() : base(DataFileRelativePath, transportBookingTemplateDataFileTables)
		{
		}

		internal TransportBookingTemplateDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, transportBookingTemplateDataFileTables)
		{
		}

		protected override string SelectQuery
		{
			get
			{
				return @"
SELECT * FROM dbo.DtbBookingTmpl WHERE KT_IsSystem = 1 order by 1;

SELECT DtbBookingInstructionTmpl.* FROM dbo.DtbBookingInstructionTmpl
	INNER JOIN dbo.DtbBookingTmpl
	ON K2_KT_BookingTmpl = KT_PK
WHERE KT_IsSystem = 1 order by 1";
			}
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\TransportBookingTemplate\TransportBookingTemplate.xml";
		public override string ResourceRelativeName => "TransportBookingTemplate.TransportBookingTemplate.xml";

		protected static readonly string[] transportBookingTemplateDataFileTables = new string[] { DtbBookingTmplSchema.Constants.TableName, DtbBookingInstructionTmplSchema.Constants.TableName };

		#endregion
	}
}
