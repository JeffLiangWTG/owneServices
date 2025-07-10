using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class RefAirlineDataFile : EmbeddedDataFile
	{
		public RefAirlineDataFile() : base(DataFileRelativePath, DataFileTables)
		{
		}

		internal RefAirlineDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, DataFileTables)
		{
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\RefAirline\RefAirline.xml";
		public override string ResourceRelativeName => "RefAirline.RefAirline.xml";
		protected static readonly string[] DataFileTables = new string[1] { RefAirlineSchema.Constants.TableName };

		#endregion
	}
}
