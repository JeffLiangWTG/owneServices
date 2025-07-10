using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class RefLatLongPostcodeDataFile : EmbeddedDataFile
	{
		public RefLatLongPostcodeDataFile() : base(DataFileRelativePath, RefLatLongPostcodeDataFileTables)
		{
		}

		internal RefLatLongPostcodeDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, RefLatLongPostcodeDataFileTables)
		{
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\RefLatLongPostcode\RefLatLongPostcode.xml";
		public override string ResourceRelativeName => "RefLatLongPostcode.RefLatLongPostcode.xml";
		static readonly string[] RefLatLongPostcodeDataFileTables = new string[1] { RefLatLongPostcodeSchema.Constants.TableName };

		#endregion
	}
}
