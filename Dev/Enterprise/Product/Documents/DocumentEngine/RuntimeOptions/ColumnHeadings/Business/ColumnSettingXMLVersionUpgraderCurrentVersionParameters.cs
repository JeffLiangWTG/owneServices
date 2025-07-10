using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public interface IColumnSettingXMLVersionUpgraderV1Parameters
	{
		ZGuid ReportID { get; }
	}

	public class ColumnSettingXMLVersionUpgraderCurrentVersionParameters : IColumnSettingXMLVersionUpgraderV1Parameters
	{
		public ColumnSettingXMLVersionUpgraderCurrentVersionParameters(IColumnSettingXMLVersionUpgraderV1Parameters v1Params)
		{
			fParams = v1Params;
		}
		readonly IColumnSettingXMLVersionUpgraderV1Parameters fParams;

		#region IColumnSettingXMLVersionUpgraderV1Parameters Members

		ZGuid IColumnSettingXMLVersionUpgraderV1Parameters.ReportID
		{
			get { return fParams.ReportID; }
		}

		#endregion
	}
}
